using HardAcclDslApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HardAcclDslApi.Controllers;

[ApiController]
[Route("api/lua-scripts")]
public sealed class LuaScriptStorageController : ControllerBase
{
    private readonly ILuaScriptStorageService _storage;
    private readonly LuaExecutionService _luaExecutionService;

    public LuaScriptStorageController(ILuaScriptStorageService storage, LuaExecutionService luaExecutionService)
    {
        _storage = storage;
        _luaExecutionService = luaExecutionService;
    }

    [HttpPost("save")]
    public async Task<ActionResult<SaveLuaScriptResponse>> Save([FromBody] SaveLuaScriptApiRequest request, CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.User) ||
            string.IsNullOrWhiteSpace(request.ScriptName) ||
            string.IsNullOrWhiteSpace(request.LuaCode))
        {
            return BadRequest("user, scriptName, and luaCode are required.");
        }

        var result = await _storage.SaveScriptAsync(new SaveLuaScriptRequest
        {
            User = request.User,
            ScriptName = request.ScriptName,
            LuaCode = request.LuaCode,
        }, cancellationToken);

        return Ok(new SaveLuaScriptResponse
        {
            User = result.User,
            ScriptName = result.ScriptName,
        });
    }

    [HttpGet("{user}")]
    public async Task<ActionResult<ListLuaScriptsResponse>> ListByUser(string user, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user))
        {
            return BadRequest("user is required.");
        }

        var scripts = await _storage.ListScriptsAsync(user, cancellationToken);
        return Ok(new ListLuaScriptsResponse
        {
            User = user,
            Scripts = scripts,
        });
    }

    [HttpGet("{user}/{scriptName}")]
    public async Task<ActionResult<GetLuaScriptResponse>> GetByUserAndScriptName(string user, string scriptName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(scriptName))
        {
            return BadRequest("user and scriptName are required.");
        }

        var script = await _storage.GetScriptAsync(user, scriptName, cancellationToken);
        if (script is null)
        {
            return NotFound(new
            {
                error = $"Script '{scriptName}' not found for user '{user}'."
            });
        }

        return Ok(new GetLuaScriptResponse
        {
            User = script.User,
            ScriptName = script.ScriptName,
            LuaCode = script.LuaCode,
        });
    }

    [HttpDelete("{user}/{scriptName}")]
    public async Task<IActionResult> DeleteByUserAndScriptName(string user, string scriptName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(scriptName))
        {
            return BadRequest("user and scriptName are required.");
        }

        var deleted = await _storage.DeleteScriptAsync(user, scriptName, cancellationToken);
        if (!deleted)
        {
            return NotFound(new
            {
                error = $"Script '{scriptName}' not found for user '{user}'."
            });
        }

        return NoContent();
    }

    [HttpPost("execute")]
    public async Task<ActionResult<LuaExecutionResult>> Execute([FromBody] ExecuteLuaScriptRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.User) || string.IsNullOrWhiteSpace(request.ScriptName))
        {
            return BadRequest("user and scriptName are required.");
        }

        var script = await _storage.GetScriptAsync(request.User, request.ScriptName, cancellationToken);
        if (script is null)
        {
            return NotFound(new
            {
                error = $"Script '{request.ScriptName}' not found for user '{request.User}'."
            });
        }

        var execution = _luaExecutionService.Execute(script.LuaCode, request.Globals);
        return Ok(execution);
    }
}

public sealed class SaveLuaScriptApiRequest
{
    public string User { get; init; } = string.Empty;
    public string ScriptName { get; init; } = string.Empty;
    public string LuaCode { get; init; } = string.Empty;
}

public sealed class SaveLuaScriptResponse
{
    public string User { get; init; } = string.Empty;
    public string ScriptName { get; init; } = string.Empty;
}

public sealed class ExecuteLuaScriptRequest
{
    public string User { get; init; } = string.Empty;
    public string ScriptName { get; init; } = string.Empty;
    public Dictionary<string, double> Globals { get; init; } = new(StringComparer.Ordinal);
}

public sealed class GetLuaScriptResponse
{
    public string User { get; init; } = string.Empty;
    public string ScriptName { get; init; } = string.Empty;
    public string LuaCode { get; init; } = string.Empty;
}

public sealed class ListLuaScriptsResponse
{
    public string User { get; init; } = string.Empty;
    public IReadOnlyList<LuaScriptMetadata> Scripts { get; init; } = Array.Empty<LuaScriptMetadata>();
}
