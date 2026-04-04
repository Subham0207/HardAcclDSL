namespace HardAcclDslApi.Models.Parsing;

public sealed class ParseResult
{
    public bool IsValid => Errors.Count == 0;
    public List<SyntaxError> Errors { get; init; } = new();
    public List<TokenInfo> Tokens { get; init; } = new();
    public string ParseTree { get; init; } = string.Empty;
    public ParseTreeNode ParseTreeRoot { get; init; } = new();
}
