using System.Text.Json;
using HardAcclDslApi.Controllers;
using HardAcclDslApi.Models.Ast;
using HardAcclDslApi.Models.Graph;
using HardAcclDslApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HardAcclDslApi.UnitTests;

public class LuaControllerGraphToAstRouteTests
{
    [Fact]
    public async Task GraphToAst_WithSharedLocalDeclInExpressionTree_DoesNotReportFalseCycleAndExecutes()
    {
        var snapshotJson = ReadTestDataFile("graph-to-ast-request.json");
        var expectedJson = ReadTestDataFile("graph-to-ast-response.json");
        using var expectedDoc = JsonDocument.Parse(expectedJson);
        var expected = expectedDoc.RootElement;

        var snapshot = JsonSerializer.Deserialize<VisualScriptGraphSnapshotDto>(
            snapshotJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(snapshot);

        var parserService = new AntlrLuaParserService();
        var controller = new LuaController(
            new LuaToIR(parserService),
            parserService,
            new VisualScriptGraphToAstMapper(),
            new AstToVisualScriptGraphMapper(),
            new AstToLuaScribanRenderer(),
            new LuaExecutionService(),
            TestLuaScriptStorageFactory.Create());

        var request = new GraphToAstRequest
        {
            User = "demo-user",
            ScriptName = "shared-result-graph",
            GraphSnapshot = snapshot!,
        };

        var actionResult = await controller.GraphToAst(request, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<VisualScriptGraphToAstResponse>(ok.Value);

        Assert.DoesNotContain(response.Diagnostics, d => d.Code == "expression_cycle");
        Assert.Equal(expected.GetProperty("luaCode").GetString(), response.LuaCode);

        var secondPrint = Assert.IsType<FunctionCallNode>(response.Ast.Statements[2]);
        var secondArg = Assert.IsType<BinaryExpressionNode>(Assert.Single(secondPrint.Arguments));
        var add = Assert.IsType<BinaryExpressionNode>(secondArg.Right);
        var leftOfAdd = Assert.IsType<IdentifierExpressionNode>(add.Left);
        var expectedLeftIdentifier = expected
            .GetProperty("ast")
            .GetProperty("statements")[2]
            .GetProperty("arguments")[0]
            .GetProperty("right")
            .GetProperty("left")
            .GetProperty("name")
            .GetString();
        Assert.Equal(expectedLeftIdentifier, leftOfAdd.Name);

        Assert.Equal(expected.GetProperty("execution").GetProperty("success").GetBoolean(), response.Execution.Success);
        var expectedPrintedLines = expected
            .GetProperty("execution")
            .GetProperty("printedLines")
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();
        Assert.Equal(expectedPrintedLines, response.Execution.PrintedLines);
    }

    private static string ReadTestDataFile(string fileName)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
        return File.ReadAllText(fullPath);
    }
}
