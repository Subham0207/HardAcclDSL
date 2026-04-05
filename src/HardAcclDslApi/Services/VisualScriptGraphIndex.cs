using HardAcclDslApi.Models.Graph;

namespace HardAcclDslApi.Services;

public sealed class VisualScriptGraphIndex
{
    private readonly Dictionary<string, VisualScriptGraphNodeDto> _nodesById;
    // Incoming edge lookup by pin key (target side): "nodeId::handleId" -> edges arriving at that pin.
    private readonly Dictionary<string, List<VisualScriptGraphEdgeDto>> _edgesByTargetPin;
    // Outgoing edge lookup by pin key (source side): "nodeId::handleId" -> edges leaving that pin.
    private readonly Dictionary<string, List<VisualScriptGraphEdgeDto>> _edgesBySourcePin;

    public VisualScriptGraphIndex(VisualScriptGraphSnapshotDto snapshot)
    {
        _nodesById = BuildNodeIndex(snapshot);
        _edgesByTargetPin = CreatePinEdgeIndex();
        _edgesBySourcePin = CreatePinEdgeIndex();

        IndexEdgesByPins(snapshot.Edges);
        SortAllIndexedEdgeLists();
    }

    public bool TryGetNode(string nodeId, out VisualScriptGraphNodeDto? node)
    {
        var found = _nodesById.TryGetValue(nodeId, out var value);
        node = value;
        return found;
    }

    // Question: what node owns this pin?
    public VisualScriptGraphNodeDto? GetNodeOwningPin(VisualScriptGraphPinRef pin)
    {
        if (!_nodesById.TryGetValue(pin.NodeId, out var node))
        {
            return null;
        }

        var ownsPin = IsKnownHandleForNode(node, pin.HandleId);

        return ownsPin ? node : null;
    }

    // Question: what is connected to this pin?
    public IReadOnlyList<VisualScriptGraphEdgeDto> GetIncomingEdgesToPin(VisualScriptGraphPinRef pin)
    {
        return _edgesByTargetPin.TryGetValue(PinKey(pin.NodeId, pin.HandleId), out var edges)
            ? edges
            : Array.Empty<VisualScriptGraphEdgeDto>();
    }

    public IReadOnlyList<VisualScriptGraphEdgeDto> GetOutgoingEdgesFromPin(VisualScriptGraphPinRef pin)
    {
        return _edgesBySourcePin.TryGetValue(PinKey(pin.NodeId, pin.HandleId), out var edges)
            ? edges
            : Array.Empty<VisualScriptGraphEdgeDto>();
    }

    public VisualScriptGraphNodeDto? GetNodeConnectedToInputPin(VisualScriptGraphPinRef inputPin)
    {
        var edge = FindFirstIncomingEdge(inputPin);
        if (edge is null)
        {
            return null;
        }

        return _nodesById.GetValueOrDefault(edge.Source);
    }

    // Question: what node executes next?
    public VisualScriptGraphNodeDto? GetNextExecutionNode(string nodeId)
    {
        var execOutPin = new VisualScriptGraphPinRef(nodeId, "exec-out");
        var nextExecEdge = GetOutgoingEdgesFromPin(execOutPin)
            .FirstOrDefault(edge => string.Equals(edge.Flow, "exec", StringComparison.OrdinalIgnoreCase)
                && string.Equals(edge.TargetHandle, "exec-in", StringComparison.OrdinalIgnoreCase));

        if (nextExecEdge is null)
        {
            return null;
        }

        return _nodesById.GetValueOrDefault(nextExecEdge.Target);
    }

    private static string PinKey(string nodeId, string handleId) => $"{nodeId}::{handleId}";

    private static Dictionary<string, VisualScriptGraphNodeDto> BuildNodeIndex(VisualScriptGraphSnapshotDto snapshot)
    {
        return snapshot.Nodes
            .Where(node => !string.IsNullOrWhiteSpace(node.Id))
            .GroupBy(node => node.Id)
            .ToDictionary(group => group.Key, group => group.First());
    }

    private static Dictionary<string, List<VisualScriptGraphEdgeDto>> CreatePinEdgeIndex()
    {
        return new(StringComparer.OrdinalIgnoreCase);
    }

    private void IndexEdgesByPins(IEnumerable<VisualScriptGraphEdgeDto> edges)
    {
        foreach (var edge in edges)
        {
            IndexIncomingEdge(edge);
            IndexOutgoingEdge(edge);
        }
    }

    private void IndexIncomingEdge(VisualScriptGraphEdgeDto edge)
    {
        if (string.IsNullOrWhiteSpace(edge.Target) || string.IsNullOrWhiteSpace(edge.TargetHandle))
        {
            return;
        }

        var targetPinKey = PinKey(edge.Target, edge.TargetHandle);
        AddEdgeToPinIndex(_edgesByTargetPin, targetPinKey, edge);
    }

    private void IndexOutgoingEdge(VisualScriptGraphEdgeDto edge)
    {
        if (string.IsNullOrWhiteSpace(edge.Source) || string.IsNullOrWhiteSpace(edge.SourceHandle))
        {
            return;
        }

        var sourcePinKey = PinKey(edge.Source, edge.SourceHandle);
        AddEdgeToPinIndex(_edgesBySourcePin, sourcePinKey, edge);
    }

    private static void AddEdgeToPinIndex(
        Dictionary<string, List<VisualScriptGraphEdgeDto>> pinIndex,
        string pinKey,
        VisualScriptGraphEdgeDto edge)
    {
        if (!pinIndex.TryGetValue(pinKey, out var edgesForPin))
        {
            edgesForPin = new List<VisualScriptGraphEdgeDto>();
            pinIndex[pinKey] = edgesForPin;
        }

        edgesForPin.Add(edge);
    }

    private void SortAllIndexedEdgeLists()
    {
        SortIndexedEdgeLists(_edgesByTargetPin);
        SortIndexedEdgeLists(_edgesBySourcePin);
    }

    private static void SortIndexedEdgeLists(Dictionary<string, List<VisualScriptGraphEdgeDto>> pinIndex)
    {
        foreach (var list in pinIndex.Values)
        {
            list.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
        }
    }

    private static bool IsKnownHandleForNode(VisualScriptGraphNodeDto node, string handleId)
    {
        return node.Handles.DataIn.Contains(handleId)
            || node.Handles.DataOut.Contains(handleId)
            || node.Handles.ExecIn.Contains(handleId)
            || node.Handles.ExecOut.Contains(handleId);
    }

    private VisualScriptGraphEdgeDto? FindFirstIncomingEdge(VisualScriptGraphPinRef inputPin)
    {
        return GetIncomingEdgesToPin(inputPin).FirstOrDefault();
    }
}
