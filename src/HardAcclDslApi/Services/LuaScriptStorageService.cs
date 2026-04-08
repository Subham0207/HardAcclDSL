using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;

namespace HardAcclDslApi.Services;

public interface ILuaScriptStorageService
{
    Task<SaveLuaScriptResult> SaveScriptAsync(SaveLuaScriptRequest request, CancellationToken cancellationToken);
    Task<StoredLuaScript?> GetScriptAsync(string user, string scriptName, CancellationToken cancellationToken);
    Task<IReadOnlyList<LuaScriptMetadata>> ListScriptsAsync(string user, CancellationToken cancellationToken);
    Task<bool> DeleteScriptAsync(string user, string scriptName, CancellationToken cancellationToken);
}

public sealed class LuaScriptStorageService : ILuaScriptStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly IAmazonDynamoDB _dynamo;
    private readonly string _bucketName;
    private readonly string _tableName;

    public LuaScriptStorageService(IAmazonS3 s3, IAmazonDynamoDB dynamo, IConfiguration configuration)
    {
        _s3 = s3;
        _dynamo = dynamo;
        _bucketName = configuration["SCRIPT_BUCKET_NAME"] ?? "hardaccldsl";
        _tableName = configuration["SCRIPT_TABLE_NAME"] ?? "HardAcclDSL";
    }

    public async Task<SaveLuaScriptResult> SaveScriptAsync(SaveLuaScriptRequest request, CancellationToken cancellationToken)
    {
        var key = $"{Guid.NewGuid():N}.lua";
        var s3Link = $"s3://{_bucketName}/{key}";

        try
        {
            await _dynamo.PutItemAsync(new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    ["user"] = new AttributeValue { S = request.User },
                    ["scriptname"] = new AttributeValue { S = request.ScriptName },
                    ["s3Link"] = new AttributeValue { S = s3Link },
                },
                ConditionExpression = "attribute_not_exists(#u) AND attribute_not_exists(#s)",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    ["#u"] = "user",
                    ["#s"] = "scriptname",
                }
            }, cancellationToken);
        }
        catch (ConditionalCheckFailedException)
        {
            return SaveLuaScriptResult.FromConflict(request.User, request.ScriptName);
        }

        try
        {
            await _s3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                ContentBody = request.LuaCode,
                ContentType = "text/plain; charset=utf-8",
            }, cancellationToken);
        }
        catch
        {
            await _dynamo.DeleteItemAsync(new DeleteItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["user"] = new AttributeValue { S = request.User },
                    ["scriptname"] = new AttributeValue { S = request.ScriptName },
                }
            }, cancellationToken);
            throw;
        }

        return SaveLuaScriptResult.Success(request.User, request.ScriptName, s3Link);
    }

    public async Task<StoredLuaScript?> GetScriptAsync(string user, string scriptName, CancellationToken cancellationToken)
    {
        var itemResponse = await _dynamo.GetItemAsync(new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["user"] = new AttributeValue { S = user },
                ["scriptname"] = new AttributeValue { S = scriptName },
            },
            ConsistentRead = true,
        }, cancellationToken);

        if (itemResponse.Item.Count == 0)
        {
            return null;
        }

        var s3Link = itemResponse.Item["s3Link"].S;
        var (_, key) = ParseS3Link(s3Link);

        using var objectResponse = await _s3.GetObjectAsync(_bucketName, key, cancellationToken);
        using var reader = new StreamReader(objectResponse.ResponseStream);
        var luaCode = await reader.ReadToEndAsync(cancellationToken);

        return new StoredLuaScript
        {
            User = user,
            ScriptName = scriptName,
            S3Link = s3Link,
            LuaCode = luaCode,
        };
    }

    public async Task<IReadOnlyList<LuaScriptMetadata>> ListScriptsAsync(string user, CancellationToken cancellationToken)
    {
        var response = await _dynamo.QueryAsync(new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "#u = :u",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#u"] = "user",
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":u"] = new AttributeValue { S = user },
            },
            ProjectionExpression = "scriptname",
        }, cancellationToken);

        return response.Items.Select(item => new LuaScriptMetadata
        {
            User = user,
            ScriptName = item["scriptname"].S,
        }).OrderBy(x => x.ScriptName, StringComparer.Ordinal).ToList();
    }

    public async Task<bool> DeleteScriptAsync(string user, string scriptName, CancellationToken cancellationToken)
    {
        var existing = await _dynamo.GetItemAsync(new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["user"] = new AttributeValue { S = user },
                ["scriptname"] = new AttributeValue { S = scriptName },
            },
            ConsistentRead = true,
        }, cancellationToken);

        if (existing.Item.Count == 0)
        {
            return false;
        }

        var s3Link = existing.Item["s3Link"].S;
        var (_, key) = ParseS3Link(s3Link);

        try
        {
            await _s3.DeleteObjectAsync(_bucketName, key, cancellationToken);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // If object does not exist, still delete metadata row to keep table clean.
        }

        await _dynamo.DeleteItemAsync(new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["user"] = new AttributeValue { S = user },
                ["scriptname"] = new AttributeValue { S = scriptName },
            },
        }, cancellationToken);

        return true;
    }

    private static (string bucket, string key) ParseS3Link(string s3Link)
    {
        const string prefix = "s3://";
        if (!s3Link.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Invalid s3Link format: {s3Link}");
        }

        var path = s3Link[prefix.Length..];
        var slashIndex = path.IndexOf('/');
        if (slashIndex < 1 || slashIndex == path.Length - 1)
        {
            throw new InvalidOperationException($"Invalid s3Link format: {s3Link}");
        }

        var bucket = path[..slashIndex];
        var key = path[(slashIndex + 1)..];
        return (bucket, key);
    }
}

public sealed class SaveLuaScriptRequest
{
    public string User { get; init; } = string.Empty;
    public string ScriptName { get; init; } = string.Empty;
    public string LuaCode { get; init; } = string.Empty;
}

public sealed class SaveLuaScriptResult
{
    public bool Created { get; init; }
    public bool IsConflict { get; init; }
    public string User { get; init; } = string.Empty;
    public string ScriptName { get; init; } = string.Empty;
    public string S3Link { get; init; } = string.Empty;

    public static SaveLuaScriptResult Success(string user, string scriptName, string s3Link)
    {
        return new SaveLuaScriptResult
        {
            Created = true,
            IsConflict = false,
            User = user,
            ScriptName = scriptName,
            S3Link = s3Link,
        };
    }

    public static SaveLuaScriptResult FromConflict(string user, string scriptName)
    {
        return new SaveLuaScriptResult
        {
            Created = false,
            IsConflict = true,
            User = user,
            ScriptName = scriptName,
        };
    }
}

public sealed class LuaScriptMetadata
{
    public string User { get; init; } = string.Empty;
    public string ScriptName { get; init; } = string.Empty;
}

public sealed class StoredLuaScript
{
    public string User { get; init; } = string.Empty;
    public string ScriptName { get; init; } = string.Empty;
    public string S3Link { get; init; } = string.Empty;
    public string LuaCode { get; init; } = string.Empty;
}
