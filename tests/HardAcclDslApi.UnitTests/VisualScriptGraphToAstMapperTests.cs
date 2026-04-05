using System.Text.Json;
using HardAcclDslApi.Models.Ast;
using HardAcclDslApi.Models.Graph;
using HardAcclDslApi.Services;

namespace HardAcclDslApi.UnitTests;

public class VisualScriptGraphToAstMapperTests
{
    private readonly VisualScriptGraphToAstMapper _sut = new();

    [Fact]
    public void Map_LocalDeclOutputUsedInAdd_MapsToIdentifierExpressionWithoutUnsupportedDiagnostic()
    {
        var snapshot = new VisualScriptGraphSnapshotDto
        {
            Nodes = new List<VisualScriptGraphNodeDto>
            {
                new()
                {
                    Id = "localDecl-1",
                    Type = "localDecl",
                    Position = new VisualScriptGraphPositionDto { X = 10, Y = 10 },
                    Data = JsonSerializer.SerializeToElement(new { variableName = "result", initialValue = "12" }),
                    Handles = Handles(dataIn: new[] { "value" }, dataOut: new[] { "out" }, execIn: new[] { "exec-in" }, execOut: new[] { "exec-out" }),
                },
                new()
                {
                    Id = "add-1",
                    Type = "add",
                    Position = new VisualScriptGraphPositionDto { X = 120, Y = 20 },
                    Data = JsonSerializer.SerializeToElement(new { }),
                    Handles = Handles(dataIn: new[] { "left", "right" }, dataOut: new[] { "out" }, execIn: Array.Empty<string>(), execOut: Array.Empty<string>()),
                },
                new()
                {
                    Id = "number-1",
                    Type = "numberLiteral",
                    Position = new VisualScriptGraphPositionDto { X = 120, Y = 80 },
                    Data = JsonSerializer.SerializeToElement(new { value = "10" }),
                    Handles = Handles(dataIn: Array.Empty<string>(), dataOut: new[] { "out" }, execIn: Array.Empty<string>(), execOut: Array.Empty<string>()),
                },
                new()
                {
                    Id = "print-1",
                    Type = "print",
                    Position = new VisualScriptGraphPositionDto { X = 220, Y = 20 },
                    Data = JsonSerializer.SerializeToElement(new { }),
                    Handles = Handles(dataIn: new[] { "value" }, dataOut: Array.Empty<string>(), execIn: new[] { "exec-in" }, execOut: new[] { "exec-out" }),
                },
            },
            Edges = new List<VisualScriptGraphEdgeDto>
            {
                new()
                {
                    Id = "e-local-to-add-left",
                    Source = "localDecl-1",
                    SourceHandle = "out",
                    Target = "add-1",
                    TargetHandle = "left",
                    Flow = "data",
                },
                new()
                {
                    Id = "e-number-to-add-right",
                    Source = "number-1",
                    SourceHandle = "out",
                    Target = "add-1",
                    TargetHandle = "right",
                    Flow = "data",
                },
                new()
                {
                    Id = "e-add-to-print-value",
                    Source = "add-1",
                    SourceHandle = "out",
                    Target = "print-1",
                    TargetHandle = "value",
                    Flow = "data",
                },
                new()
                {
                    Id = "e-exec-local-to-print",
                    Source = "localDecl-1",
                    SourceHandle = "exec-out",
                    Target = "print-1",
                    TargetHandle = "exec-in",
                    Flow = "exec",
                },
            }
        };

        var result = _sut.Map(snapshot);

        Assert.Empty(result.Diagnostics.Where(d => d.Code == "unsupported_expression"));

        var program = result.Ast;
        Assert.Equal(2, program.Statements.Count);

        var printStmt = Assert.IsType<FunctionCallNode>(program.Statements[1]);
        var addExpr = Assert.IsType<BinaryExpressionNode>(Assert.Single(printStmt.Arguments));

        var left = Assert.IsType<IdentifierExpressionNode>(addExpr.Left);
        Assert.Equal("result", left.Name);

        var right = Assert.IsType<NumberLiteralExpressionNode>(addExpr.Right);
        Assert.Equal("10", right.RawText);
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
}
