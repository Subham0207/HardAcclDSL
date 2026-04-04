namespace HardAcclDslApi.Services;

/// <summary>
/// Converts Lua code to an Intermediate Representation (IR).
/// This currently validates and parses Lua subset syntax via ANTLR.
/// </summary>
public class LuaToIR
{
    private readonly AntlrLuaParserService _parserService;

    public LuaToIR(AntlrLuaParserService parserService)
    {
        _parserService = parserService;
    }

    /// <summary>
    /// Converts Lua source code to IR format.
    /// </summary>
    /// <param name="luaCode">The Lua source code to convert.</param>
    /// <returns>A placeholder IR payload with parse information.</returns>
    public string Convert(string luaCode)
    {
        if (string.IsNullOrWhiteSpace(luaCode))
        {
            throw new ArgumentException("Lua code cannot be null or empty.", nameof(luaCode));
        }

        var parseResult = _parserService.Parse(luaCode);
        if (!parseResult.IsValid)
        {
            var firstError = parseResult.Errors[0];
            throw new InvalidOperationException(
                $"Lua syntax error at line {firstError.Line}, column {firstError.Column}: {firstError.Message}");
        }

        return $"IR_PLACEHOLDER | tokens={parseResult.Tokens.Count}";
    }
}