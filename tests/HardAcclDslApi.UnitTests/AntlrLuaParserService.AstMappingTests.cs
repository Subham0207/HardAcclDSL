using HardAcclDslApi.Models.Ast;
using HardAcclDslApi.Services;

namespace HardAcclDslApi.UnitTests;

public class AntlrLuaParserServiceAstMappingTests
{
    private readonly AntlrLuaParserService _sut = new();

    [Fact]
    public void Parse_LocalAssignment_MapsToLocalDeclarationAst()
    {
        var result = _sut.Parse("local a = 2 + 3");

        var expected = new ProgramNode
        {
            Statements = new List<StatementNode>
            {
                new LocalDeclarationStatementNode
                {
                    Name = "a",
                    Value = new BinaryExpressionNode
                    {
                        Operator = "+",
                        Left = new NumberLiteralExpressionNode { RawText = "2" },
                        Right = new NumberLiteralExpressionNode { RawText = "3" }
                    }
                }
            }
        };

        AssertAstEquivalent(expected, result.AstRoot);
    }

    [Fact]
    public void Parse_Assignment_MapsToAssignmentAst()
    {
        var result = _sut.Parse("a = b");

        var expected = new ProgramNode
        {
            Statements = new List<StatementNode>
            {
                new AssignmentStatementNode
                {
                    Name = "a",
                    Value = new IdentifierExpressionNode { Name = "b" }
                }
            }
        };

        AssertAstEquivalent(expected, result.AstRoot);
    }

    [Fact]
    public void Parse_Return_MapsToReturnAst()
    {
        var result = _sut.Parse("return value");

        var expected = new ProgramNode
        {
            Statements = new List<StatementNode>
            {
                new ReturnStatementNode
                {
                    Value = new IdentifierExpressionNode { Name = "value" }
                }
            }
        };

        AssertAstEquivalent(expected, result.AstRoot);
    }

    [Fact]
    public void Parse_CallStatement_MapsToExpressionStatementAst()
    {
        var result = _sut.Parse("print(1, a)");

        var expected = new ProgramNode
        {
            Statements = new List<StatementNode>
            {
                new ExpressionStatementNode
                {
                    Expression = new CallExpressionNode
                    {
                        FunctionName = "print",
                        Arguments = new List<ExpressionNode>
                        {
                            new NumberLiteralExpressionNode { RawText = "1" },
                            new IdentifierExpressionNode { Name = "a" }
                        }
                    }
                }
            }
        };

        AssertAstEquivalent(expected, result.AstRoot);
    }

    [Fact]
    public void Parse_ParenthesizedExpression_PreservesPrecedenceInAst()
    {
        var result = _sut.Parse("local x = (2 + 3) * 4");

        var expected = new ProgramNode
        {
            Statements = new List<StatementNode>
            {
                new LocalDeclarationStatementNode
                {
                    Name = "x",
                    Value = new BinaryExpressionNode
                    {
                        Operator = "*",
                        Left = new BinaryExpressionNode
                        {
                            Operator = "+",
                            Left = new NumberLiteralExpressionNode { RawText = "2" },
                            Right = new NumberLiteralExpressionNode { RawText = "3" }
                        },
                        Right = new NumberLiteralExpressionNode { RawText = "4" }
                    }
                }
            }
        };

        AssertAstEquivalent(expected, result.AstRoot);
    }

    [Fact]
    public void Parse_StringLiteral_MapsToStringLiteralAst()
    {
        var result = _sut.Parse("local msg = \"hello\"");

        var expected = new ProgramNode
        {
            Statements = new List<StatementNode>
            {
                new LocalDeclarationStatementNode
                {
                    Name = "msg",
                    Value = new StringLiteralExpressionNode { RawText = "\"hello\"" }
                }
            }
        };

        AssertAstEquivalent(expected, result.AstRoot);
    }

    private static void AssertAstEquivalent(ProgramNode expected, ProgramNode? actual)
    {
        Assert.NotNull(actual);
        Assert.Equivalent(ToComparableNode(expected), ToComparableNode(actual!), strict: true);
    }

    private static object ToComparableNode(AstNode node)
    {
        return node switch
        {
            ProgramNode program => new
            {
                Kind = program.Kind,
                Statements = program.Statements.Select(ToComparableNode).ToList()
            },
            LocalDeclarationStatementNode localDecl => new
            {
                Kind = localDecl.Kind,
                localDecl.Name,
                Value = ToComparableNode(localDecl.Value)
            },
            AssignmentStatementNode assign => new
            {
                Kind = assign.Kind,
                assign.Name,
                Value = ToComparableNode(assign.Value)
            },
            ReturnStatementNode ret => new
            {
                Kind = ret.Kind,
                Value = ToComparableNode(ret.Value)
            },
            ExpressionStatementNode exprStmt => new
            {
                Kind = exprStmt.Kind,
                Expression = ToComparableNode(exprStmt.Expression)
            },
            IdentifierExpressionNode identifier => new
            {
                Kind = identifier.Kind,
                identifier.Name
            },
            NumberLiteralExpressionNode number => new
            {
                Kind = number.Kind,
                number.RawText
            },
            StringLiteralExpressionNode str => new
            {
                Kind = str.Kind,
                str.RawText
            },
            BinaryExpressionNode binary => new
            {
                Kind = binary.Kind,
                binary.Operator,
                Left = ToComparableNode(binary.Left),
                Right = ToComparableNode(binary.Right)
            },
            CallExpressionNode call => new
            {
                Kind = call.Kind,
                call.FunctionName,
                Arguments = call.Arguments.Select(ToComparableNode).ToList()
            },
            _ => throw new NotSupportedException($"Unsupported AST node type: {node.GetType().Name}")
        };
    }
}
