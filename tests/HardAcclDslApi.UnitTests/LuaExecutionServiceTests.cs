using HardAcclDslApi.Services;

namespace HardAcclDslApi.UnitTests;

public class LuaExecutionServiceTests
{
    private readonly LuaExecutionService _sut = new();

    [Fact]
    public void Execute_WithReturnValues_ReturnsSuccessAndValues()
    {
        var result = _sut.Execute("return 1 + 2, 'ok', true");

        Assert.True(result.Success);
        Assert.Empty(result.Error);
        Assert.Equal(new[] { "3", "ok", "true" }, result.ReturnValues);
        Assert.Empty(result.PrintedLines);
    }

    [Fact]
    public void Execute_WithPrintStatement_CapturesPrintedOutput()
    {
        var result = _sut.Execute("local result = 14\nprint(result)");

        Assert.True(result.Success);
        Assert.Equal(new[] { "14" }, result.PrintedLines);
    }

    [Fact]
    public void Execute_WithRuntimeError_ReturnsFailure()
    {
        var result = _sut.Execute("error('boom')");

        Assert.False(result.Success);
        Assert.Contains("Lua runtime error", result.Error);
        Assert.Empty(result.ReturnValues);
    }

    [Fact]
    public void Execute_WithNumberGlobals_InjectsGlobalsBeforeExecution()
    {
        var globals = new Dictionary<string, double>
        {
            ["multiplier"] = 2.5,
        };

        var result = _sut.Execute("return multiplier * 4", globals);

        Assert.True(result.Success);
        Assert.Equal(new[] { "10" }, result.ReturnValues);
    }
}
