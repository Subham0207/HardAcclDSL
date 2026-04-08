using HardAcclDslApi.Services;
using HardAcclDslApi.Models.Parsing;
using HardAcclDslApi.Models.Ast;
using HardAcclDslApi.Models.Graph;
using Microsoft.AspNetCore.Mvc;

namespace HardAcclDslApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LuaController : ControllerBase
{
    private const string DefaultVisualScriptLua = "local result = 10";

    private readonly LuaToIR _luaToIr;
    private readonly AntlrLuaParserService _parserService;
    private readonly VisualScriptGraphToAstMapper _graphToAstMapper;
    private readonly AstToVisualScriptGraphMapper _astToGraphMapper;
    private readonly AstToLuaScribanRenderer _astToLuaRenderer;
    private readonly LuaExecutionService _luaExecutionService;
    private readonly ILuaScriptStorageService _scriptStorageService;

    public LuaController(
        LuaToIR luaToIr,
        AntlrLuaParserService parserService,
        VisualScriptGraphToAstMapper graphToAstMapper,
        AstToVisualScriptGraphMapper astToGraphMapper,
        AstToLuaScribanRenderer astToLuaRenderer,
        LuaExecutionService luaExecutionService,
        ILuaScriptStorageService scriptStorageService)
    {
        _luaToIr = luaToIr;
        _parserService = parserService;
        _graphToAstMapper = graphToAstMapper;
        _astToGraphMapper = astToGraphMapper;
        _astToLuaRenderer = astToLuaRenderer;
        _luaExecutionService = luaExecutionService;
        _scriptStorageService = scriptStorageService;
    }

    [HttpPost("convert")]
    public ActionResult<LuaConvertResponse> Convert([FromBody] LuaConvertRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.LuaCode))
        {
            return BadRequest("luaCode is required.");
        }

        try
        {
            var result = _luaToIr.ConvertWithDetails(request.LuaCode);
            return Ok(new LuaConvertResponse
            {
                Ir = result.Ir,
                ParseTree = result.ParseTree,
                Tokens = result.Tokens,
                Ast = result.Ast
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("parse-tree")]
    public ActionResult<ParseTreeOnlyResponse> ParseTree([FromBody] LuaConvertRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.LuaCode))
        {
            return BadRequest("luaCode is required.");
        }

        var parseResult = _parserService.Parse(request.LuaCode);
        if (!parseResult.IsValid)
        {
            var firstError = parseResult.Errors[0];
            return BadRequest(new
            {
                error = $"Lua syntax error at line {firstError.Line}, column {firstError.Column}: {firstError.Message}"
            });
        }

        return Ok(new ParseTreeOnlyResponse
        {
            ParseTree = parseResult.ParseTreeRoot
        });
    }

    [HttpPost("ast")]
    public ActionResult<AstOnlyResponse> Ast([FromBody] LuaConvertRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.LuaCode))
        {
            return BadRequest("luaCode is required.");
        }

        var parseResult = _parserService.Parse(request.LuaCode);
        if (!parseResult.IsValid)
        {
            var firstError = parseResult.Errors[0];
            return BadRequest(new
            {
                error = $"Lua syntax error at line {firstError.Line}, column {firstError.Column}: {firstError.Message}"
            });
        }

        return Ok(new AstOnlyResponse
        {
            Ast = parseResult.AstRoot ?? new ProgramNode()
        });
    }

    [HttpPost("lua-to-visualscript")]
    public async Task<ActionResult<LuaToVisualScriptResponse>> LuaToVisualScript([FromBody] LuaToVisualScriptRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        var hasUser = !string.IsNullOrWhiteSpace(request.User);
        var hasScriptName = !string.IsNullOrWhiteSpace(request.ScriptName);
        if (hasUser ^ hasScriptName)
        {
            return BadRequest("user and scriptName must be provided together.");
        }

        var luaCode = request.LuaCode;
        if (hasUser && hasScriptName)
        {
            var stored = await _scriptStorageService.GetScriptAsync(request.User.Trim(), request.ScriptName.Trim(), cancellationToken);
            if (stored is null)
            {
                return NotFound(new
                {
                    error = $"Script '{request.ScriptName}' not found for user '{request.User}'."
                });
            }

            luaCode = stored.LuaCode;
        }

        if (string.IsNullOrWhiteSpace(luaCode))
        {
            return BadRequest("luaCode is required when user and scriptName are not provided.");
        }

        var parseResult = _parserService.Parse(luaCode);
        if (!parseResult.IsValid)
        {
            var firstError = parseResult.Errors[0];
            return BadRequest(new
            {
                error = $"Lua syntax error at line {firstError.Line}, column {firstError.Column}: {firstError.Message}"
            });
        }

    var ast = parseResult.AstRoot ?? new ProgramNode();
    LuaGraphPositionCommentCodec.ApplyCommentsToAst(luaCode, ast);
        var mapped = _astToGraphMapper.Map(ast);

        return Ok(new LuaToVisualScriptResponse
        {
            Ast = ast,
            GraphSnapshot = mapped.Snapshot,
            Diagnostics = mapped.Diagnostics,
        });
    }

    [HttpGet("lua-to-visualscript/default")]
    public ActionResult<LuaToVisualScriptResponse> DefaultLuaToVisualScript()
    {
        var parseResult = _parserService.Parse(DefaultVisualScriptLua);
        if (!parseResult.IsValid)
        {
            var firstError = parseResult.Errors[0];
            return BadRequest(new
            {
                error = $"Lua syntax error at line {firstError.Line}, column {firstError.Column}: {firstError.Message}"
            });
        }

        var ast = parseResult.AstRoot ?? new ProgramNode();
        var mapped = _astToGraphMapper.Map(ast);

        return Ok(new LuaToVisualScriptResponse
        {
            Ast = ast,
            GraphSnapshot = mapped.Snapshot,
            Diagnostics = mapped.Diagnostics,
        });
    }

    [HttpPost("graph-snapshot")]
    public ActionResult<VisualScriptGraphSnapshotAckResponse> GraphSnapshot([FromBody] VisualScriptGraphSnapshotDto snapshot)
    {
        if (snapshot is null)
        {
            return BadRequest("Graph snapshot is required.");
        }

        var index = new VisualScriptGraphIndex(snapshot);
        var nodeCount = snapshot.Nodes.Count;
        var edgeCount = snapshot.Edges.Count;

        return Ok(new VisualScriptGraphSnapshotAckResponse
        {
            Accepted = true,
            NodeCount = nodeCount,
            EdgeCount = edgeCount,
            Message = "Graph snapshot received and indexed."
        });
    }

    [HttpPost("graph-to-ast")]
    public async Task<ActionResult<VisualScriptGraphToAstResponse>> GraphToAst([FromBody] GraphToAstRequest request, CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.User) ||
            string.IsNullOrWhiteSpace(request.ScriptName) ||
            request.GraphSnapshot is null)
        {
            return BadRequest("user, scriptName, and graphSnapshot are required.");
        }

        var result = _graphToAstMapper.Map(request.GraphSnapshot);
        var luaCode = _astToLuaRenderer.RenderProgram(result.Ast);
        var luaCodeWithGraphComments = _astToLuaRenderer.RenderProgram(result.Ast, includeGraphPositionComments: true);
        var execution = _luaExecutionService.Execute(luaCode, request.Globals);

        await _scriptStorageService.SaveScriptAsync(new SaveLuaScriptRequest
        {
            User = request.User,
            ScriptName = request.ScriptName,
            LuaCode = luaCodeWithGraphComments,
        }, cancellationToken);

        return Ok(new VisualScriptGraphToAstResponse
        {
            Ast = result.Ast,
            Diagnostics = result.Diagnostics,
            LuaCode = luaCode,
            Execution = execution,
        });
    }
}

public sealed class LuaConvertRequest
{
    public string LuaCode { get; init; } = string.Empty;
}

public sealed class LuaConvertResponse
{
    public string Ir { get; init; } = string.Empty;
    public string ParseTree { get; init; } = string.Empty;
    public IReadOnlyList<HardAcclDslApi.Models.Parsing.TokenInfo> Tokens { get; init; } =
        Array.Empty<HardAcclDslApi.Models.Parsing.TokenInfo>();
    public ProgramNode Ast { get; init; } = new();
}

public sealed class ParseTreeOnlyResponse
{
    public ParseTreeNode ParseTree { get; init; } = new();
}

public sealed class AstOnlyResponse
{
    public ProgramNode Ast { get; init; } = new();
}

public sealed class VisualScriptGraphSnapshotAckResponse
{
    public bool Accepted { get; init; }
    public int NodeCount { get; init; }
    public int EdgeCount { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class VisualScriptGraphToAstResponse
{
    public ProgramNode Ast { get; init; } = new();
    public IReadOnlyList<VisualScriptGraphDiagnostic> Diagnostics { get; init; } =
        Array.Empty<VisualScriptGraphDiagnostic>();
    public string LuaCode { get; init; } = string.Empty;
    public LuaExecutionResult Execution { get; init; } = new();
}

public sealed class GraphToAstRequest
{
    public string User { get; init; } = string.Empty;
    public string ScriptName { get; init; } = string.Empty;
    public VisualScriptGraphSnapshotDto GraphSnapshot { get; init; } = new();
    public Dictionary<string, double> Globals { get; init; } = new(StringComparer.Ordinal);
}

public sealed class LuaToVisualScriptRequest
{
    public string LuaCode { get; init; } = string.Empty;
    public string User { get; init; } = string.Empty;
    public string ScriptName { get; init; } = string.Empty;
}

public sealed class LuaToVisualScriptResponse
{
    public ProgramNode Ast { get; init; } = new();
    public VisualScriptGraphSnapshotDto GraphSnapshot { get; init; } = new();
    public IReadOnlyList<VisualScriptGraphDiagnostic> Diagnostics { get; init; } =
        Array.Empty<VisualScriptGraphDiagnostic>();
}
