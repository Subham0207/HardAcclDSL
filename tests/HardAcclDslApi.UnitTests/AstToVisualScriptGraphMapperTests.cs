using HardAcclDslApi.Models.Ast;
using HardAcclDslApi.Services;

namespace HardAcclDslApi.UnitTests;

public class AstToVisualScriptGraphMapperTests
{
    private readonly AstToVisualScriptGraphMapper _sut = new();

    [Fact]
    public void Map_WithLocalDeclAndPrintBinary_ProducesGraphWithExecAndDataEdges()
    {
        var ast = new ProgramNode
        {
            Statements = new List<AstNode>
            {
                new LocalDeclarationStatementNode
                {
                    NodeId = "local-1",
                    Name = "result",
                    Value = new NumberLiteralExpressionNode { NodeId = "num-1", RawText = "14" }
                },
                new FunctionCallNode
                {
                    NodeId = "print-1",
                    FunctionName = "print",
                    Arguments = new List<AstNode>
                    {
                        new BinaryExpressionNode
                        {
                            NodeId = "add-1",
                            Operator = "+",
                            Left = new IdentifierExpressionNode { NodeId = "id-1", Name = "result" },
                            Right = new NumberLiteralExpressionNode { NodeId = "num-2", RawText = "6" }
                        }
                    }
                }
            }
        };

        var result = _sut.Map(ast);

        Assert.Empty(result.Diagnostics);

        var snapshot = result.Snapshot;
        Assert.Contains(snapshot.Nodes, n => n.Id == "local-1" && n.Type == "localDecl");
        Assert.Contains(snapshot.Nodes, n => n.Id == "print-1" && n.Type == "print");
        Assert.Contains(snapshot.Nodes, n => n.Id == "add-1" && n.Type == "add");

        Assert.Contains(snapshot.Edges, e => e.Flow == "exec" && e.Source == "local-1" && e.Target == "print-1");
        Assert.Contains(snapshot.Edges, e => e.Flow == "data" && e.Source == "add-1" && e.Target == "print-1" && e.TargetHandle == "value");
        Assert.Contains(snapshot.Edges, e => e.Flow == "data" && e.Source == "id-1" && e.Target == "add-1" && e.TargetHandle == "left");
        Assert.Contains(snapshot.Edges, e => e.Flow == "data" && e.Source == "num-2" && e.Target == "add-1" && e.TargetHandle == "right");
    }
}
