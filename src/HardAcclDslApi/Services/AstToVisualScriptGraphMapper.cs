using System.Text.Json;
using HardAcclDslApi.Models.Ast;
using HardAcclDslApi.Models.Graph;

namespace HardAcclDslApi.Services;

public sealed class AstToVisualScriptGraphMapper
{
    private readonly List<VisualScriptGraphNodeDto> _nodes = new();
    private readonly List<VisualScriptGraphEdgeDto> _edges = new();
    private readonly List<VisualScriptGraphDiagnostic> _diagnostics = new();

    private double _statementY = 80;
    private double _expressionY = 120;

    public AstToVisualScriptGraphMapResult Map(ProgramNode program)
    {
        _nodes.Clear();
        _edges.Clear();
        _diagnostics.Clear();
        _statementY = 80;
        _expressionY = 120;

        string? previousExecNodeId = null;

        foreach (var statement in program.Statements)
        {
            var statementNodeId = MapStatement(statement);
            if (statementNodeId is null)
            {
                continue;
            }

            if (previousExecNodeId is not null)
            {
                var previous = _nodes.FirstOrDefault(n => n.Id == previousExecNodeId);
                var current = _nodes.FirstOrDefault(n => n.Id == statementNodeId);

                if (previous is not null && current is not null &&
                    previous.Handles.ExecOut.Contains("exec-out") && current.Handles.ExecIn.Contains("exec-in"))
                {
                    AddEdge(previousExecNodeId, "exec-out", statementNodeId, "exec-in", "exec");
                }
            }

            var currentNode = _nodes.FirstOrDefault(n => n.Id == statementNodeId);
            if (currentNode is not null &&
                currentNode.Handles.ExecIn.Contains("exec-in") &&
                currentNode.Handles.ExecOut.Contains("exec-out"))
            {
                previousExecNodeId = statementNodeId;
            }
        }

        StampNodeEdgeIds();

        return new AstToVisualScriptGraphMapResult
        {
            Snapshot = new VisualScriptGraphSnapshotDto
            {
                Nodes = _nodes,
                Edges = _edges,
            },
            Diagnostics = _diagnostics,
        };
    }

    private string? MapStatement(AstNode statement)
    {
        var nodeId = string.IsNullOrWhiteSpace(statement.NodeId) ? NewNodeId("stmt") : statement.NodeId;

        switch (statement)
        {
            case LocalDeclarationStatementNode localDecl:
            {
                var initialValue = localDecl.Value is NumberLiteralExpressionNode numberLiteral ? numberLiteral.RawText : "0";

                AddNode(
                    nodeId,
                    "localDecl",
                    PositionForNode(localDecl, NextStatementPosition),
                    new { label = "LocalDeclaration", role = "Statement", detail = "local name = value", variableName = localDecl.Name, initialValue },
                    Handles(dataIn: new[] { "value" }, dataOut: new[] { "out" }, execIn: new[] { "exec-in" }, execOut: new[] { "exec-out" }));

                if (localDecl.Value is not NumberLiteralExpressionNode)
                {
                    var valueNodeId = MapExpression(localDecl.Value);
                    if (valueNodeId is not null)
                    {
                        AddEdge(valueNodeId, "out", nodeId, "value", "data");
                    }
                }

                return nodeId;
            }

            case AssignmentStatementNode assignment:
            {
                AddNode(
                    nodeId,
                    "assignment",
                    PositionForNode(assignment, NextStatementPosition),
                    new { label = "Assignment", role = "Statement", detail = "result = value" },
                    Handles(dataIn: new[] { "target", "value" }, dataOut: Array.Empty<string>(), execIn: Array.Empty<string>(), execOut: Array.Empty<string>()));

                var targetId = AddNode(
                    NewNodeId("identifier"),
                    "identifier",
                    NextExpressionPosition(),
                    new { label = "Identifier", role = "Expression", detail = "read variable", variableName = assignment.Name },
                    Handles(dataIn: Array.Empty<string>(), dataOut: new[] { "out" }, execIn: Array.Empty<string>(), execOut: Array.Empty<string>())).Id;

                AddEdge(targetId, "out", nodeId, "target", "data");

                var valueId = MapExpression(assignment.Value);
                if (valueId is not null)
                {
                    AddEdge(valueId, "out", nodeId, "value", "data");
                }

                return nodeId;
            }

            case ReturnStatementNode ret:
            {
                AddNode(
                    nodeId,
                    "return",
                    PositionForNode(ret, NextStatementPosition),
                    new { label = "Return", role = "Statement", detail = "return result" },
                    Handles(dataIn: new[] { "value" }, dataOut: Array.Empty<string>(), execIn: Array.Empty<string>(), execOut: Array.Empty<string>()));

                var valueId = MapExpression(ret.Value);
                if (valueId is not null)
                {
                    AddEdge(valueId, "out", nodeId, "value", "data");
                }

                return nodeId;
            }

            case FunctionCallNode call when string.Equals(call.FunctionName, "print", StringComparison.OrdinalIgnoreCase):
            {
                AddNode(
                    nodeId,
                    "print",
                    PositionForNode(call, NextStatementPosition),
                    new { label = "Print", role = "Statement", detail = "print(value)" },
                    Handles(dataIn: new[] { "value" }, dataOut: Array.Empty<string>(), execIn: new[] { "exec-in" }, execOut: new[] { "exec-out" }));

                var firstArg = call.Arguments.FirstOrDefault();
                if (firstArg is not null)
                {
                    var argId = MapExpression(firstArg);
                    if (argId is not null)
                    {
                        AddEdge(argId, "out", nodeId, "value", "data");
                    }
                }

                return nodeId;
            }

            default:
                _diagnostics.Add(new VisualScriptGraphDiagnostic
                {
                    Severity = "warning",
                    Code = "unsupported_statement",
                    Message = $"AST statement '{statement.Kind}' is not mapped to visual graph yet.",
                    NodeId = statement.NodeId,
                });
                return null;
        }
    }

    private string? MapExpression(AstNode expression)
    {
        var nodeId = string.IsNullOrWhiteSpace(expression.NodeId) ? NewNodeId("expr") : expression.NodeId;

        switch (expression)
        {
            case IdentifierExpressionNode identifier:
                AddNode(
                    nodeId,
                    "identifier",
                    PositionForNode(identifier, NextExpressionPosition),
                    new { label = "Identifier", role = "Expression", detail = "read variable", variableName = identifier.Name },
                    Handles(dataIn: Array.Empty<string>(), dataOut: new[] { "out" }, execIn: Array.Empty<string>(), execOut: Array.Empty<string>()));
                return nodeId;

            case NumberLiteralExpressionNode number:
                AddNode(
                    nodeId,
                    "numberLiteral",
                    PositionForNode(number, NextExpressionPosition),
                    new { label = "NumberLiteral", role = "Expression", detail = "numeric constant", value = number.RawText },
                    Handles(dataIn: Array.Empty<string>(), dataOut: new[] { "out" }, execIn: Array.Empty<string>(), execOut: Array.Empty<string>()));
                return nodeId;

            case BinaryExpressionNode binary:
            {
                var type = BinaryType(binary.Operator);
                AddNode(
                    nodeId,
                    type,
                    PositionForNode(binary, NextExpressionPosition),
                    new { label = BinaryLabel(type), role = "Expression", detail = BinaryDetail(binary.Operator), operatorSymbol = binary.Operator },
                    Handles(dataIn: new[] { "left", "right" }, dataOut: new[] { "out" }, execIn: Array.Empty<string>(), execOut: Array.Empty<string>()));

                var leftId = MapExpression(binary.Left);
                if (leftId is not null)
                {
                    AddEdge(leftId, "out", nodeId, "left", "data");
                }

                var rightId = MapExpression(binary.Right);
                if (rightId is not null)
                {
                    AddEdge(rightId, "out", nodeId, "right", "data");
                }

                return nodeId;
            }

            default:
                _diagnostics.Add(new VisualScriptGraphDiagnostic
                {
                    Severity = "warning",
                    Code = "unsupported_expression",
                    Message = $"AST expression '{expression.Kind}' is not mapped to visual graph yet.",
                    NodeId = expression.NodeId,
                });
                return null;
        }
    }

    private VisualScriptGraphNodeDto AddNode(
        string id,
        string type,
        VisualScriptGraphPositionDto position,
        object data,
        VisualScriptGraphNodeHandlesDto handles)
    {
        var existing = _nodes.FirstOrDefault(n => n.Id == id);
        if (existing is not null)
        {
            return existing;
        }

        var node = new VisualScriptGraphNodeDto
        {
            Id = id,
            Type = type,
            Position = position,
            Data = JsonSerializer.SerializeToElement(data),
            Handles = handles,
            DataFlowEdgeIds = new List<string>(),
            ExecFlowEdgeIds = new List<string>(),
        };

        _nodes.Add(node);
        return node;
    }

    private void AddEdge(string source, string sourceHandle, string target, string targetHandle, string flow)
    {
        _edges.Add(new VisualScriptGraphEdgeDto
        {
            Id = $"e-{Guid.NewGuid():N}",
            Source = source,
            SourceHandle = sourceHandle,
            Target = target,
            TargetHandle = targetHandle,
            Flow = flow,
        });
    }

    private void StampNodeEdgeIds()
    {
        var byNode = _nodes.ToDictionary(n => n.Id, StringComparer.Ordinal);

        foreach (var edge in _edges)
        {
            if (byNode.TryGetValue(edge.Source, out var sourceNode))
            {
                if (edge.Flow == "exec")
                {
                    sourceNode.ExecFlowEdgeIds.Add(edge.Id);
                }
                else
                {
                    sourceNode.DataFlowEdgeIds.Add(edge.Id);
                }
            }

            if (byNode.TryGetValue(edge.Target, out var targetNode))
            {
                if (edge.Flow == "exec")
                {
                    targetNode.ExecFlowEdgeIds.Add(edge.Id);
                }
                else
                {
                    targetNode.DataFlowEdgeIds.Add(edge.Id);
                }
            }
        }
    }

    private static VisualScriptGraphNodeHandlesDto Handles(
        IReadOnlyList<string> dataIn,
        IReadOnlyList<string> dataOut,
        IReadOnlyList<string> execIn,
        IReadOnlyList<string> execOut)
    {
        return new VisualScriptGraphNodeHandlesDto
        {
            DataIn = dataIn.ToList(),
            DataOut = dataOut.ToList(),
            ExecIn = execIn.ToList(),
            ExecOut = execOut.ToList(),
        };
    }

    private VisualScriptGraphPositionDto NextStatementPosition()
    {
        var position = new VisualScriptGraphPositionDto { X = 120, Y = _statementY };
        _statementY += 180;
        return position;
    }

    private VisualScriptGraphPositionDto NextExpressionPosition()
    {
        var position = new VisualScriptGraphPositionDto { X = 520, Y = _expressionY };
        _expressionY += 120;
        return position;
    }

    private static VisualScriptGraphPositionDto PositionForNode(AstNode node, Func<VisualScriptGraphPositionDto> fallbackFactory)
    {
        if (node.GraphX.HasValue && node.GraphY.HasValue)
        {
            return new VisualScriptGraphPositionDto
            {
                X = node.GraphX.Value,
                Y = node.GraphY.Value,
            };
        }

        return fallbackFactory();
    }

    private static string BinaryType(string @operator)
    {
        return @operator switch
        {
            "+" => "add",
            "-" => "subtract",
            "*" => "multiply",
            "/" => "divide",
            "%" => "modulo",
            _ => "add",
        };
    }

    private static string BinaryLabel(string type)
    {
        return type switch
        {
            "add" => "Add",
            "subtract" => "Subtract",
            "multiply" => "Multiply",
            "divide" => "Divide",
            "modulo" => "Modulo",
            _ => "Add",
        };
    }

    private static string BinaryDetail(string @operator)
    {
        return @operator switch
        {
            "+" => "left + right",
            "-" => "left - right",
            "*" => "left * right",
            "/" => "left / right",
            "%" => "left % right",
            _ => "left + right",
        };
    }

    private static string NewNodeId(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}";
    }
}

public sealed class AstToVisualScriptGraphMapResult
{
    public VisualScriptGraphSnapshotDto Snapshot { get; init; } = new();
    public List<VisualScriptGraphDiagnostic> Diagnostics { get; init; } = new();
}
