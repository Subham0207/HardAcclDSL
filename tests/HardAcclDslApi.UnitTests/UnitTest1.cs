using HardAcclDslApi.Services;
using HardAcclDslApi.Models.Ast;

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

public class AntlrLuaParserAstMappingTests
{
    [Fact]
    public void Parse_WithLocalAssignment_MapsToProgramAst()
    {
        var parserService = new AntlrLuaParserService();

        var result = parserService.Parse("local a = 2 + 3");

        Assert.True(result.IsValid);
        Assert.NotNull(result.AstRoot);
        Assert.Single(result.AstRoot!.Statements);

        var statement = Assert.IsType<LocalDeclarationStatementNode>(result.AstRoot.Statements[0]);
        Assert.Equal("a", statement.Name);

        var binary = Assert.IsType<BinaryExpressionNode>(statement.Value);
        Assert.Equal("+", binary.Operator);
        Assert.IsType<NumberLiteralExpressionNode>(binary.Left);
        Assert.IsType<NumberLiteralExpressionNode>(binary.Right);
    }

    [Fact]
    public void Parse_WithCallStatement_MapsToExpressionStatementAst()
    {
        var parserService = new AntlrLuaParserService();

        var result = parserService.Parse("print(1, a)");

        Assert.True(result.IsValid);
        Assert.NotNull(result.AstRoot);
        Assert.Single(result.AstRoot!.Statements);

        var statement = Assert.IsType<ExpressionStatementNode>(result.AstRoot.Statements[0]);
        var call = Assert.IsType<CallExpressionNode>(statement.Expression);

        Assert.Equal("print", call.FunctionName);
        Assert.Equal(2, call.Arguments.Count);
    }
}