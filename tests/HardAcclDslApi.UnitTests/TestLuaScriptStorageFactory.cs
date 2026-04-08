using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using HardAcclDslApi.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace HardAcclDslApi.UnitTests;

internal static class TestLuaScriptStorageFactory
{
    public static ILuaScriptStorageService Create()
    {
        var s3 = new Mock<IAmazonS3>();
        s3.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        var dynamo = new Mock<IAmazonDynamoDB>();
        dynamo.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse
            {
                Item = new Dictionary<string, AttributeValue>()
            });
        dynamo.Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutItemResponse());
        dynamo.Setup(x => x.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteItemResponse());

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SCRIPT_BUCKET_NAME"] = "hardaccldsl",
                ["SCRIPT_TABLE_NAME"] = "HardAcclDSL",
            })
            .Build();

        return new LuaScriptStorageService(s3.Object, dynamo.Object, configuration);
    }
}
