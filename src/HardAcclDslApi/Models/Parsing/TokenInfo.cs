namespace HardAcclDslApi.Models.Parsing;

public sealed class TokenInfo
{
    public string Type { get; init; } = string.Empty;
    public string Lexeme { get; init; } = string.Empty;
    public int Line { get; init; }
    public int Column { get; init; }
}
