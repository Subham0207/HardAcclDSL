namespace HardAcclDslApi.Models.Parsing;

public sealed class SyntaxError
{
    public int Line { get; init; }
    public int Column { get; init; }
    public string Message { get; init; } = string.Empty;
}
