using HardAcclDslApi.Controllers;
using HardAcclDslApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HardAcclDslApi.UnitTests;

public class LuaScriptStorageControllerTests
{
    [Fact]
    public async Task Execute_WithStoredScriptAndGlobals_ReturnsLuaExecutionResult()
    {
        var storage = new Mock<ILuaScriptStorageService>();
        storage.Setup(s => s.GetScriptAsync("alice", "calc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredLuaScript
            {
                User = "alice",
                ScriptName = "calc",
                LuaCode = "return multiplier * 4"
            });

        var controller = new LuaScriptStorageController(storage.Object, new LuaExecutionService());

        var result = await controller.Execute(new ExecuteLuaScriptRequest
        {
            User = "alice",
            ScriptName = "calc",
            Globals = new Dictionary<string, double> { ["multiplier"] = 2.5 }
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<LuaExecutionResult>(ok.Value);
        Assert.True(payload.Success);
        Assert.Equal(new[] { "10" }, payload.ReturnValues);
    }

    [Fact]
    public async Task Execute_WhenScriptNotFound_ReturnsNotFound()
    {
        var storage = new Mock<ILuaScriptStorageService>();
        storage.Setup(s => s.GetScriptAsync("alice", "missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredLuaScript?)null);

        var controller = new LuaScriptStorageController(storage.Object, new LuaExecutionService());

        var result = await controller.Execute(new ExecuteLuaScriptRequest
        {
            User = "alice",
            ScriptName = "missing",
        }, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Execute_WhenUserOrScriptMissing_ReturnsBadRequest()
    {
        var storage = new Mock<ILuaScriptStorageService>();
        var controller = new LuaScriptStorageController(storage.Object, new LuaExecutionService());

        var result = await controller.Execute(new ExecuteLuaScriptRequest
        {
            User = "",
            ScriptName = "calc",
        }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
