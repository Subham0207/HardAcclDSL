using HardAcclDslApi.Models.Ast;
using HardAcclDslApi.Services;

namespace HardAcclDslApi.UnitTests;

public class AstToLuaScribanRendererTests
{
    private readonly AstToLuaScribanRenderer _sut = new();

    [Fact]
    public void RenderProgram_WithLocalDeclarationAndPrintBinary_RendersExpectedLua()
    {
        var ast = new ProgramNode
        {
            Statements = new List<AstNode>
            {
                new LocalDeclarationStatementNode
                {
                    Name = "result",
                    Value = new NumberLiteralExpressionNode { RawText = "12" }
                },
                new FunctionCallNode
                {
                    FunctionName = "print",
                    Arguments = new List<AstNode>
                    {
                        new BinaryExpressionNode
                        {
                            Operator = "+",
                            Left = new IdentifierExpressionNode { Name = "result" },
                            Right = new NumberLiteralExpressionNode { RawText = "10" }
                        }
                    }
                }
            }
        };

        var lua = _sut.RenderProgram(ast);

        Assert.Equal("local result = 12\nprint(result + 10)", lua);
    }

    [Fact]
    public void RenderProgram_WithNestedBinaryOperations_AddsParenthesesToPreserveOrder()
    {
        var ast = new ProgramNode
        {
            Statements = new List<AstNode>
            {
                new LocalDeclarationStatementNode
                {
                    Name = "result",
                    Value = new NumberLiteralExpressionNode { RawText = "0" }
                },
                new FunctionCallNode
                {
                    FunctionName = "print",
                    Arguments = new List<AstNode>
                    {
                        new BinaryExpressionNode
                        {
                            Operator = "*",
                            Left = new BinaryExpressionNode
                            {
                                Operator = "+",
                                Left = new IdentifierExpressionNode { Name = "result" },
                                Right = new NumberLiteralExpressionNode { RawText = "12" }
                            },
                            Right = new NumberLiteralExpressionNode { RawText = "11" }
                        }
                    }
                }
            }
        };

        var lua = _sut.RenderProgram(ast);

        Assert.Equal("local result = 0\nprint((result + 12) * 11)", lua);
    }
}
