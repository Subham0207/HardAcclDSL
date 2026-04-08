using System.Globalization;
using System.Text.RegularExpressions;
using HardAcclDslApi.Models.Ast;

namespace HardAcclDslApi.Services;

public static class LuaGraphPositionCommentCodec
{
    private static readonly Regex PositionCommentRegex = new(
        "^\\s*--\\s*@vs-node\\s+(?<id>\\S+)\\s+(?<type>\\S+)\\s+(?<x>-?\\d+(?:\\.\\d+)?)\\s+(?<y>-?\\d+(?:\\.\\d+)?)\\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IReadOnlyList<string> BuildCommentLines(ProgramNode program)
    {
        var lines = new List<string>();
        foreach (var node in EnumerateNodes(program))
        {
            if (!node.GraphX.HasValue || !node.GraphY.HasValue)
            {
                continue;
            }

            var nodeType = string.IsNullOrWhiteSpace(node.GraphNodeType)
                ? node.Kind.ToString()
                : node.GraphNodeType;

            lines.Add($"-- @vs-node {node.NodeId} {nodeType} {node.GraphX.Value.ToString(CultureInfo.InvariantCulture)} {node.GraphY.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        return lines;
    }

    public static void ApplyCommentsToAst(string luaCode, ProgramNode program)
    {
        var comments = ParseComments(luaCode);
        if (comments.Count == 0)
        {
            return;
        }

        var nodes = EnumerateNodes(program).ToList();
        var localDeclarationInlineNumbers = program.Statements
            .OfType<LocalDeclarationStatementNode>()
            .Select(local => local.Value)
            .OfType<NumberLiteralExpressionNode>()
            .Cast<AstNode>()
            .ToHashSet();
        var nextCommentIndex = 0;

        foreach (var node in nodes)
        {
            if (localDeclarationInlineNumbers.Contains(node))
            {
                continue;
            }

            var expectedTypes = GetExpectedGraphNodeTypes(node);
            if (expectedTypes.Count == 0)
            {
                continue;
            }

            var matchedIndex = -1;
            for (var i = nextCommentIndex; i < comments.Count; i++)
            {
                if (expectedTypes.Contains(comments[i].Type, StringComparer.Ordinal))
                {
                    matchedIndex = i;
                    break;
                }
            }

            if (matchedIndex < 0)
            {
                continue;
            }

            var comment = comments[matchedIndex];
            nextCommentIndex = matchedIndex + 1;

            node.NodeId = comment.Id;
            node.GraphNodeType = comment.Type;
            node.GraphX = comment.X;
            node.GraphY = comment.Y;
        }
    }

    private static IReadOnlyList<string> GetExpectedGraphNodeTypes(AstNode node)
    {
        return node switch
        {
            LocalDeclarationStatementNode => new[] { "localDecl" },
            AssignmentStatementNode => new[] { "assignment" },
            ReturnStatementNode => new[] { "return" },
            FunctionCallNode call when string.Equals(call.FunctionName, "print", StringComparison.OrdinalIgnoreCase) => new[] { "print" },
            IdentifierExpressionNode => new[] { "identifier", "global" },
            GlobalReferenceExpressionNode => new[] { "global" },
            NumberLiteralExpressionNode => new[] { "numberLiteral" },
            BinaryExpressionNode binary => binary.Operator switch
            {
                "+" => new[] { "add" },
                "-" => new[] { "subtract" },
                "*" => new[] { "multiply" },
                "/" => new[] { "divide" },
                "%" => new[] { "modulo" },
                _ => Array.Empty<string>(),
            },
            _ => Array.Empty<string>(),
        };
    }

    private static List<PositionComment> ParseComments(string luaCode)
    {
        var matches = PositionCommentRegex.Matches(luaCode);
        var comments = new List<PositionComment>(matches.Count);

        foreach (Match match in matches)
        {
            comments.Add(new PositionComment
            {
                Id = match.Groups["id"].Value,
                Type = match.Groups["type"].Value,
                X = double.Parse(match.Groups["x"].Value, CultureInfo.InvariantCulture),
                Y = double.Parse(match.Groups["y"].Value, CultureInfo.InvariantCulture),
            });
        }

        return comments;
    }

    private static IEnumerable<AstNode> EnumerateNodes(ProgramNode program)
    {
        foreach (var statement in program.Statements)
        {
            foreach (var node in EnumerateNode(statement))
            {
                yield return node;
            }
        }
    }

    private static IEnumerable<AstNode> EnumerateNode(AstNode node)
    {
        yield return node;

        switch (node)
        {
            case LocalDeclarationStatementNode localDecl:
                foreach (var child in EnumerateNode(localDecl.Value))
                {
                    yield return child;
                }
                break;

            case AssignmentStatementNode assignment:
                foreach (var child in EnumerateNode(assignment.Value))
                {
                    yield return child;
                }
                break;

            case ReturnStatementNode ret:
                foreach (var child in EnumerateNode(ret.Value))
                {
                    yield return child;
                }
                break;

            case BinaryExpressionNode binary:
                foreach (var child in EnumerateNode(binary.Left))
                {
                    yield return child;
                }
                foreach (var child in EnumerateNode(binary.Right))
                {
                    yield return child;
                }
                break;

            case FunctionCallNode call:
                foreach (var argument in call.Arguments)
                {
                    foreach (var child in EnumerateNode(argument))
                    {
                        yield return child;
                    }
                }
                break;
        }
    }

    private sealed class PositionComment
    {
        public string Id { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public double X { get; init; }
        public double Y { get; init; }
    }
}
