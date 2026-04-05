using HardAcclDslApi.Services;
using HardAcclDslApi.Models.Parsing;
using HardAcclDslApi.Models.Ast;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HardAcclDslApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LuaController : ControllerBase
{
    private readonly LuaToIR _luaToIr;
    private readonly AntlrLuaParserService _parserService;

    public LuaController(LuaToIR luaToIr, AntlrLuaParserService parserService)
    {
        _luaToIr = luaToIr;
        _parserService = parserService;
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

    [HttpPost("graph-snapshot")]
    public ActionResult<GraphSnapshotAckResponse> GraphSnapshot([FromBody] JsonElement snapshot)
    {
        if (snapshot.ValueKind != JsonValueKind.Object)
        {
            return BadRequest("Graph snapshot object is required.");
        }

        var nodeCount = 0;
        var edgeCount = 0;

        if (snapshot.TryGetProperty("nodes", out var nodes) && nodes.ValueKind == JsonValueKind.Array)
        {
            nodeCount = nodes.GetArrayLength();
        }

        if (snapshot.TryGetProperty("edges", out var edges) && edges.ValueKind == JsonValueKind.Array)
        {
            edgeCount = edges.GetArrayLength();
        }

        return Ok(new GraphSnapshotAckResponse
        {
            Accepted = true,
            NodeCount = nodeCount,
            EdgeCount = edgeCount,
            Message = "Graph snapshot received."
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

public sealed class GraphSnapshotAckResponse
{
    public bool Accepted { get; init; }
    public int NodeCount { get; init; }
    public int EdgeCount { get; init; }
    public string Message { get; init; } = string.Empty;
}
