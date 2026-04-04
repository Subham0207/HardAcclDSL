namespace HardAcclDslApi.Services;

using HardAcclDslApi.Models.Ast;

/// <summary>
/// Converts Lua code to an Intermediate Representation (IR).
/// This currently validates and parses Lua subset syntax via ANTLR.
/// </summary>
public class LuaToIR
{
    private readonly AntlrLuaParserService _parserService;
    private readonly ILogger<LuaToIR>? _logger;

    public LuaToIR(AntlrLuaParserService parserService, ILogger<LuaToIR>? logger = null)
    {
        _parserService = parserService;
        _logger = logger;
    }

    /// <summary>
    /// Converts Lua source code to IR format.
    /// </summary>
    /// <param name="luaCode">The Lua source code to convert.</param>
    /// <returns>A placeholder IR payload with parse information.</returns>
    public string Convert(string luaCode)
    {
        return ConvertWithDetails(luaCode).Ir;
    }

    /// <summary>
    /// Converts Lua source code and returns parse details useful for debugging.
    /// </summary>
    /// <param name="luaCode">The Lua source code to convert.</param>
    /// <returns>IR placeholder plus parse tree and tokens.</returns>
    public ConvertDetailsResult ConvertWithDetails(string luaCode)
    {
        if (string.IsNullOrWhiteSpace(luaCode))
        {
            throw new ArgumentException("Lua code cannot be null or empty.", nameof(luaCode));
        }

        var parseResult = _parserService.Parse(luaCode);
        _logger?.LogInformation("ANTLR parse tree: {ParseTree}", parseResult.ParseTree);

        if (!parseResult.IsValid)
        {
            var firstError = parseResult.Errors[0];
            throw new InvalidOperationException(
                $"Lua syntax error at line {firstError.Line}, column {firstError.Column}: {firstError.Message}");
        }

        return new ConvertDetailsResult
        {
            Ir = $"IR_PLACEHOLDER | tokens={parseResult.Tokens.Count}",
            ParseTree = parseResult.ParseTree,
            Tokens = parseResult.Tokens,
            Ast = parseResult.AstRoot ?? new ProgramNode()
        };
    }
}

public sealed class ConvertDetailsResult
{
    public string Ir { get; init; } = string.Empty;
    public string ParseTree { get; init; } = string.Empty;
    public IReadOnlyList<HardAcclDslApi.Models.Parsing.TokenInfo> Tokens { get; init; } =
        Array.Empty<HardAcclDslApi.Models.Parsing.TokenInfo>();
    public ProgramNode Ast { get; init; } = new();
}