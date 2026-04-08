using HardAcclDslApi.Models.Ast;
using HardAcclDslApi.Services;

namespace HardAcclDslApi.UnitTests;

public class LuaGraphPositionCommentCodecTests
{
    [Fact]
    public void ApplyCommentsToAst_WithMetadataComments_AppliesNodeIdsAndPositions()
    {
        var ast = new ProgramNode
        {
            Statements = new List<AstNode>
            {
                new LocalDeclarationStatementNode
                {
                    Name = "result",
                    Value = new NumberLiteralExpressionNode { RawText = "10" },
                },
                new FunctionCallNode
                {
                    FunctionName = "print",
                    Arguments = new List<AstNode>
                    {
                        new IdentifierExpressionNode { Name = "result" }
                    }
                }
            }
        };

        var luaWithComments = string.Join("\n", new[]
        {
            "-- @vs-node localDecl-1 localDecl 100 200",
            "-- @vs-node number-1 numberLiteral 150 260",
            "-- @vs-node print-1 print 300 200",
            "-- @vs-node id-1 identifier 340 260",
            "local result = 10",
            "print(result)",
        });

        LuaGraphPositionCommentCodec.ApplyCommentsToAst(luaWithComments, ast);

        var first = Assert.IsType<LocalDeclarationStatementNode>(ast.Statements[0]);
        Assert.Equal("localDecl-1", first.NodeId);
        Assert.Equal(100, first.GraphX);
        Assert.Equal(200, first.GraphY);

        var number = Assert.IsType<NumberLiteralExpressionNode>(first.Value);
        Assert.Null(number.GraphX);
        Assert.Null(number.GraphY);

        var print = Assert.IsType<FunctionCallNode>(ast.Statements[1]);
        Assert.Equal("print-1", print.NodeId);
        Assert.Equal(300, print.GraphX);
        Assert.Equal(200, print.GraphY);

        var identifier = Assert.IsType<IdentifierExpressionNode>(Assert.Single(print.Arguments));
        Assert.Equal("id-1", identifier.NodeId);
        Assert.Equal(340, identifier.GraphX);
        Assert.Equal(260, identifier.GraphY);
    }

    [Fact]
    public void ApplyCommentsToAst_WithMissingLiteralComment_DoesNotShiftFollowingNodePositions()
    {
        var ast = new ProgramNode
        {
            Statements = new List<AstNode>
            {
                new LocalDeclarationStatementNode
                {
                    Name = "result",
                    Value = new NumberLiteralExpressionNode { RawText = "12" },
                },
                new FunctionCallNode
                {
                    FunctionName = "print",
                    Arguments = new List<AstNode>
                    {
                        new BinaryExpressionNode
                        {
                            Operator = "*",
                            Left = new IdentifierExpressionNode { Name = "result" },
                            Right = new NumberLiteralExpressionNode { RawText = "30" },
                        }
                    }
                }
            }
        };

        var luaWithComments = string.Join("\n", new[]
        {
            "-- @vs-node local-1 localDecl 415.98 -167.23",
            "-- @vs-node print-1 print 1065.22 -21.12",
            "-- @vs-node mul-1 multiply 759.62 150.76",
            "-- @vs-node id-1 identifier 194.28 112.07",
            "-- @vs-node n30-1 numberLiteral 292.81 362.33",
            "local result = 12",
            "print(result * 30)",
        });

        LuaGraphPositionCommentCodec.ApplyCommentsToAst(luaWithComments, ast);

        var localDecl = Assert.IsType<LocalDeclarationStatementNode>(ast.Statements[0]);
        Assert.Equal("local-1", localDecl.NodeId);

        // Local declaration inline literal has no corresponding graph node comment.
        var localInitial = Assert.IsType<NumberLiteralExpressionNode>(localDecl.Value);
        Assert.Null(localInitial.GraphX);
        Assert.Null(localInitial.GraphY);

        var print = Assert.IsType<FunctionCallNode>(ast.Statements[1]);
        Assert.Equal("print-1", print.NodeId);
        Assert.Equal("print", print.GraphNodeType);

        var binary = Assert.IsType<BinaryExpressionNode>(Assert.Single(print.Arguments));
        Assert.Equal("mul-1", binary.NodeId);
        Assert.Equal("multiply", binary.GraphNodeType);

        var left = Assert.IsType<IdentifierExpressionNode>(binary.Left);
        Assert.Equal("id-1", left.NodeId);
        Assert.Equal("identifier", left.GraphNodeType);

        var right = Assert.IsType<NumberLiteralExpressionNode>(binary.Right);
        Assert.Equal("n30-1", right.NodeId);
        Assert.Equal("numberLiteral", right.GraphNodeType);
    }
}
