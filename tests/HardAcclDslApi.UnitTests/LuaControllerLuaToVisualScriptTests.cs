using HardAcclDslApi.Controllers;
using HardAcclDslApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HardAcclDslApi.UnitTests;

public class LuaControllerLuaToVisualScriptTests
{
    [Fact]
    public void LuaToVisualScript_WithValidLua_ReturnsGraphSnapshot()
    {
        var parserService = new AntlrLuaParserService();
        var controller = new LuaController(
            new LuaToIR(parserService),
            parserService,
            new VisualScriptGraphToAstMapper(),
            new AstToVisualScriptGraphMapper(),
            new AstToLuaScribanRenderer(),
            new LuaExecutionService());

        var result = controller.LuaToVisualScript(new LuaConvertRequest
        {
            LuaCode = "local result = 14\nprint(result)"
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LuaToVisualScriptResponse>(ok.Value);

        Assert.NotNull(response.Ast);
        Assert.NotEmpty(response.GraphSnapshot.Nodes);
        Assert.NotEmpty(response.GraphSnapshot.Edges);
        Assert.Contains(response.GraphSnapshot.Nodes, n => n.Type == "localDecl");
        Assert.Contains(response.GraphSnapshot.Nodes, n => n.Type == "print");
        Assert.Contains(response.GraphSnapshot.Edges, e => e.Flow == "exec");
    }
}
