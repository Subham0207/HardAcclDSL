namespace HardAcclDslApi.Models.Parsing;

public sealed class ParseTreeNode
{
    public string NodeType { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public int? Line { get; init; }
    public int? Column { get; init; }
    public List<ParseTreeNode> Children { get; init; } = new();
}
