using HardAcclDslApi.Services;

namespace HardAcclDslApi.UnitTests;

public class LuaToIRIntegrationTests
{
    [Fact]
    public void Convert_WithValidLuaSubset_ReturnsPlaceholderIr()
    {
        var parserService = new AntlrLuaParserService();
        var sut = new LuaToIR(parserService);

        var result = sut.Convert("local a = 2 + 3");

        Assert.Equal("IR_PLACEHOLDER | tokens=6", result);
    }

    [Fact]
    public void Convert_WithInvalidLua_ThrowsInvalidOperationException()
    {
        var parserService = new AntlrLuaParserService();
        var sut = new LuaToIR(parserService);

        var ex = Assert.Throws<InvalidOperationException>(() => sut.Convert("local = 2"));

        Assert.Contains("Lua syntax error", ex.Message);
    }
}
