using System.Text.Json;

namespace HardAcclDslApi.Models.Graph;

public sealed class VisualScriptGraphSnapshotDto
{
    public List<VisualScriptGraphNodeDto> Nodes { get; init; } = new();
    public List<VisualScriptGraphEdgeDto> Edges { get; init; } = new();
}

public sealed class VisualScriptGraphNodeDto
{
    public string Id { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public VisualScriptGraphPositionDto Position { get; init; } = new();
    public JsonElement Data { get; init; }
    public VisualScriptGraphNodeHandlesDto Handles { get; init; } = new();
    public List<string> DataFlowEdgeIds { get; init; } = new();
    public List<string> ExecFlowEdgeIds { get; init; } = new();
}

public sealed class VisualScriptGraphPositionDto
{
    public double X { get; init; }
    public double Y { get; init; }
}

public sealed class VisualScriptGraphNodeHandlesDto
{
    public List<string> DataIn { get; init; } = new();
    public List<string> DataOut { get; init; } = new();
    public List<string> ExecIn { get; init; } = new();
    public List<string> ExecOut { get; init; } = new();
}

public sealed class VisualScriptGraphEdgeDto
{
    public string Id { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string? SourceHandle { get; init; }
    public string Target { get; init; } = string.Empty;
    public string? TargetHandle { get; init; }
    public string Flow { get; init; } = string.Empty;
}

public readonly record struct VisualScriptGraphPinRef(string NodeId, string HandleId);
