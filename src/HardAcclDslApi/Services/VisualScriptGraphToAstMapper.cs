using System.Text.Json;
using HardAcclDslApi.Models.Ast;
using HardAcclDslApi.Models.Graph;

namespace HardAcclDslApi.Services;

public sealed class VisualScriptGraphToAstMapper
{
    public VisualScriptGraphToAstMapResult Map(VisualScriptGraphSnapshotDto snapshot)
    {
        var diagnostics = new List<VisualScriptGraphDiagnostic>();
        var vsGraphIndex = new VisualScriptGraphIndex(snapshot);

        var orderedStatementNodes = GetStatementNodesInExecutionOrder(snapshot, vsGraphIndex, diagnostics);
        var statements = new List<AstNode>();

        foreach (var node in orderedStatementNodes)
        {
            var mapped = MapStatementNode(node, vsGraphIndex, diagnostics);
            if (mapped is not null)
            {
                statements.Add(mapped);
            }
        }

        var program = new ProgramNode
        {
            NodeId = "program-root",
            Statements = statements,
        };

        return new VisualScriptGraphToAstMapResult
        {
            Ast = program,
            Diagnostics = diagnostics,
        };
    }

    private static List<VisualScriptGraphNodeDto> GetStatementNodesInExecutionOrder(
        VisualScriptGraphSnapshotDto snapshot,
        VisualScriptGraphIndex vsGraphIndex,
        List<VisualScriptGraphDiagnostic> diagnostics)
    {
        var statementNodes = snapshot.Nodes
            .Where(IsStatementNode)
            .ToList();

        var executionNodes = statementNodes
            .Where(node => node.Handles.ExecOut.Contains("exec-out"))
            .ToList();

        var startNodes = executionNodes
            .Where(node => vsGraphIndex.GetIncomingEdgesToPin(new VisualScriptGraphPinRef(node.Id, "exec-in")).Count == 0)
            .OrderBy(node => node.Position.Y)
            .ThenBy(node => node.Position.X)
            .ThenBy(node => node.Id, StringComparer.Ordinal)
            .ToList();

        var ordered = new List<VisualScriptGraphNodeDto>();
        var visited = new HashSet<string>(StringComparer.Ordinal);

        foreach (var start in startNodes)
        {
            var current = start;
            while (current is not null)
            {
                if (!visited.Add(current.Id))
                {
                    diagnostics.Add(new VisualScriptGraphDiagnostic
                    {
                        Severity = "warning",
                        Code = "exec_cycle",
                        Message = $"Execution cycle detected at node '{current.Id}'.",
                        NodeId = current.Id,
                    });
                    break;
                }

                ordered.Add(current);
                current = vsGraphIndex.GetNextExecutionNode(current.Id);
            }
        }

        var remaining = statementNodes
            .Where(node => !visited.Contains(node.Id))
            .OrderBy(node => node.Position.Y)
            .ThenBy(node => node.Position.X)
            .ThenBy(node => node.Id, StringComparer.Ordinal);

        ordered.AddRange(remaining);
        return ordered;
    }

    private static bool IsStatementNode(VisualScriptGraphNodeDto node)
    {
        return node.Type is "localDecl" or "assignment" or "return" or "print";
    }

    private static AstNode? MapStatementNode(
        VisualScriptGraphNodeDto node,
        VisualScriptGraphIndex vsGraphIndex,
        List<VisualScriptGraphDiagnostic> diagnostics)
    {
        return node.Type switch
        {
            "localDecl" => MapLocalDeclaration(node, vsGraphIndex, diagnostics),
            "assignment" => MapAssignment(node, vsGraphIndex, diagnostics),
            "return" => MapReturn(node, vsGraphIndex, diagnostics),
            "print" => MapPrint(node, vsGraphIndex, diagnostics),
            _ => null,
        };
    }

    private static AstNode MapLocalDeclaration(
        VisualScriptGraphNodeDto node,
        VisualScriptGraphIndex vsGraphIndex,
        List<VisualScriptGraphDiagnostic> diagnostics)
    {
        var name = GetDataString(node.Data, "variableName") ?? "value";
        var valueNode = vsGraphIndex.GetNodeConnectedToInputPin(new VisualScriptGraphPinRef(node.Id, "value"));

        var value = valueNode is not null
            ? MapExpressionNode(valueNode, vsGraphIndex, diagnostics, new HashSet<string>(StringComparer.Ordinal))
            : new NumberLiteralExpressionNode
            {
                NodeId = $"{node.Id}-initial",
                RawText = GetDataString(node.Data, "initialValue") ?? "0",
            };

        return new LocalDeclarationStatementNode
        {
            NodeId = node.Id,
            Name = name,
            Value = value,
        };
    }

    private static AstNode MapAssignment(
        VisualScriptGraphNodeDto node,
        VisualScriptGraphIndex vsGraphIndex,
        List<VisualScriptGraphDiagnostic> diagnostics)
    {
        var targetNode = vsGraphIndex.GetNodeConnectedToInputPin(new VisualScriptGraphPinRef(node.Id, "target"));
        var valueNode = vsGraphIndex.GetNodeConnectedToInputPin(new VisualScriptGraphPinRef(node.Id, "value"));

        var targetName = targetNode?.Type == "identifier"
            ? (GetDataString(targetNode.Data, "variableName") ?? "value")
            : "value";

        if (targetNode is null || targetNode.Type != "identifier")
        {
            diagnostics.Add(new VisualScriptGraphDiagnostic
            {
                Severity = "warning",
                Code = "assignment_target_missing",
                Message = $"Assignment node '{node.Id}' expects identifier target input.",
                NodeId = node.Id,
            });
        }

        var value = valueNode is not null
            ? MapExpressionNode(valueNode, vsGraphIndex, diagnostics, new HashSet<string>(StringComparer.Ordinal))
            : new IdentifierExpressionNode
            {
                NodeId = $"{node.Id}-value-missing",
                Name = "value",
            };

        return new AssignmentStatementNode
        {
            NodeId = node.Id,
            Name = targetName,
            Value = value,
        };
    }

    private static AstNode MapReturn(
        VisualScriptGraphNodeDto node,
        VisualScriptGraphIndex vsGraphIndex,
        List<VisualScriptGraphDiagnostic> diagnostics)
    {
        var valueNode = vsGraphIndex.GetNodeConnectedToInputPin(new VisualScriptGraphPinRef(node.Id, "value"));
        var value = valueNode is not null
            ? MapExpressionNode(valueNode, vsGraphIndex, diagnostics, new HashSet<string>(StringComparer.Ordinal))
            : new IdentifierExpressionNode
            {
                NodeId = $"{node.Id}-value-missing",
                Name = "value",
            };

        return new ReturnStatementNode
        {
            NodeId = node.Id,
            Value = value,
        };
    }

    private static AstNode MapPrint(
        VisualScriptGraphNodeDto node,
        VisualScriptGraphIndex vsGraphIndex,
        List<VisualScriptGraphDiagnostic> diagnostics)
    {
        var valueNode = vsGraphIndex.GetNodeConnectedToInputPin(new VisualScriptGraphPinRef(node.Id, "value"));
        var arg = valueNode is not null
            ? MapExpressionNode(valueNode, vsGraphIndex, diagnostics, new HashSet<string>(StringComparer.Ordinal))
            : new IdentifierExpressionNode
            {
                NodeId = $"{node.Id}-value-missing",
                Name = "value",
            };

        return new FunctionCallNode
        {
            NodeId = node.Id,
            FunctionName = "print",
            Arguments = new List<AstNode> { arg },
        };
    }

    private static AstNode MapExpressionNode(
        VisualScriptGraphNodeDto node,
        VisualScriptGraphIndex vsGraphIndex,
        List<VisualScriptGraphDiagnostic> diagnostics,
        HashSet<string> visiting)
    {
        if (!visiting.Add(node.Id))
        {
            diagnostics.Add(new VisualScriptGraphDiagnostic
            {
                Severity = "warning",
                Code = "expression_cycle",
                Message = $"Expression cycle detected at node '{node.Id}'.",
                NodeId = node.Id,
            });

            return new IdentifierExpressionNode
            {
                NodeId = $"{node.Id}-cycle",
                Name = "cycle",
            };
        }

        return node.Type switch
        {
            "localDecl" => new IdentifierExpressionNode
            {
                NodeId = node.Id,
                Name = GetDataString(node.Data, "variableName") ?? "value",
            },
            "identifier" => new IdentifierExpressionNode
            {
                NodeId = node.Id,
                Name = GetDataString(node.Data, "variableName") ?? "value",
            },
            "numberLiteral" => new NumberLiteralExpressionNode
            {
                NodeId = node.Id,
                RawText = GetDataString(node.Data, "value") ?? "0",
            },
            "add" or "subtract" or "multiply" or "divide" or "modulo" =>
                MapBinary(node, vsGraphIndex, diagnostics, visiting),
            _ => UnsupportedExpression(node, diagnostics),
        };
    }

    private static AstNode MapBinary(
        VisualScriptGraphNodeDto node,
        VisualScriptGraphIndex vsGraphIndex,
        List<VisualScriptGraphDiagnostic> diagnostics,
        HashSet<string> visiting)
    {
        var leftNode = vsGraphIndex.GetNodeConnectedToInputPin(new VisualScriptGraphPinRef(node.Id, "left"));
        var rightNode = vsGraphIndex.GetNodeConnectedToInputPin(new VisualScriptGraphPinRef(node.Id, "right"));

        var left = leftNode is not null
            ? MapExpressionNode(leftNode, vsGraphIndex, diagnostics, visiting)
            : new IdentifierExpressionNode
            {
                NodeId = $"{node.Id}-left-missing",
                Name = "left",
            };

        var right = rightNode is not null
            ? MapExpressionNode(rightNode, vsGraphIndex, diagnostics, visiting)
            : new IdentifierExpressionNode
            {
                NodeId = $"{node.Id}-right-missing",
                Name = "right",
            };

        return new BinaryExpressionNode
        {
            NodeId = node.Id,
            Operator = BinaryOperator(node.Type),
            Left = left,
            Right = right,
        };
    }

    private static AstNode UnsupportedExpression(VisualScriptGraphNodeDto node, List<VisualScriptGraphDiagnostic> diagnostics)
    {
        diagnostics.Add(new VisualScriptGraphDiagnostic
        {
            Severity = "warning",
            Code = "unsupported_expression",
            Message = $"Node type '{node.Type}' is not mapped as expression yet.",
            NodeId = node.Id,
        });

        return new IdentifierExpressionNode
        {
            NodeId = $"{node.Id}-unsupported",
            Name = "value",
        };
    }

    private static string BinaryOperator(string nodeType)
    {
        return nodeType switch
        {
            "add" => "+",
            "subtract" => "-",
            "multiply" => "*",
            "divide" => "/",
            "modulo" => "%",
            _ => "+",
        };
    }

    private static string? GetDataString(JsonElement data, string propertyName)
    {
        if (data.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!data.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String ? property.GetString() : property.ToString();
    }
}

public sealed class VisualScriptGraphToAstMapResult
{
    public ProgramNode Ast { get; init; } = new();
    public List<VisualScriptGraphDiagnostic> Diagnostics { get; init; } = new();
}

public sealed class VisualScriptGraphDiagnostic
{
    public string Severity { get; init; } = "warning";
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? NodeId { get; init; }
    public string? EdgeId { get; init; }
}
