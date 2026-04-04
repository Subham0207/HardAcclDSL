using HardAcclDslApi.Services;

namespace HardAcclDslApi.UnitTests;

public class AntlrLuaParserServiceParseTests
{
    private readonly AntlrLuaParserService _sut = new();

    [Fact]
    public void Parse_WithLocalAssignment_IsValidAndHasExpectedTokens()
    {
        var result = _sut.Parse("local a = 2 + 3");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(new[] { "LOCAL", "NAME", "ASSIGN", "NUMBER", "PLUS", "NUMBER" }, result.Tokens.Select(t => t.Type));
    }

    [Fact]
    public void Parse_WithAssignment_IsValid()
    {
        var result = _sut.Parse("a = 10");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(new[] { "NAME", "ASSIGN", "NUMBER" }, result.Tokens.Select(t => t.Type));
    }

    [Fact]
    public void Parse_WithReturnStatement_IsValid()
    {
        var result = _sut.Parse("return a");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(new[] { "RETURN", "NAME" }, result.Tokens.Select(t => t.Type));
    }

    [Fact]
    public void Parse_WithCallStatement_IsValid()
    {
        var result = _sut.Parse("print(1, a)");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(new[] { "NAME", "LPAREN", "NUMBER", "COMMA", "NAME", "RPAREN" }, result.Tokens.Select(t => t.Type));
    }


    [Fact]
    public void Parse_WithStringExpression_IsValid()
    {
        var result = _sut.Parse("local s = \"hello\"");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Contains(result.Tokens, t => t.Type == "STRING" && t.Lexeme == "\"hello\"");
        Assert.Equal(new[] { "LOCAL", "NAME", "ASSIGN", "STRING" }, result.Tokens.Select(t => t.Type));
    }

    [Fact]
    public void Parse_WithParenthesizedExpression_IsValid()
    {
        var result = _sut.Parse("local x = (2 + 3) * 4");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Contains("LPAREN", result.Tokens.Select(t => t.Type));
        Assert.Contains("RPAREN", result.Tokens.Select(t => t.Type));
        Assert.Equal(new[] { "LOCAL", "NAME", "ASSIGN", "LPAREN", "NUMBER", "PLUS", "NUMBER", "RPAREN", "STAR", "NUMBER" }, result.Tokens.Select(t => t.Type));
    }

    [Fact]
    public void Parse_WithCommentAndWhitespace_IsValidAndSkipsCommentToken()
    {
        var lua = "-- test comment\nlocal a = 1 + 2";
        var result = _sut.Parse(lua);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.DoesNotContain(result.Tokens, t => t.Type == "LINE_COMMENT");
        Assert.Equal(new[] { "LOCAL", "NAME", "ASSIGN", "NUMBER", "PLUS", "NUMBER" }, result.Tokens.Select(t => t.Type));
    }

    [Fact]
    public void Parse_WithMultipleStatements_IsValid()
    {
        var lua = "local a = 2\na = a + 3\nprint(a)\nreturn a";
        var result = _sut.Parse(lua);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.NotEmpty(result.ParseTree);
        Assert.Equal("chunk", result.ParseTreeRoot.Name);
        Assert.Equal(new[] { 
            "LOCAL", "NAME", "ASSIGN", "NUMBER",
            "NAME", "ASSIGN", "NAME", "PLUS", "NUMBER",
            "NAME", "LPAREN", "NAME", "RPAREN",
            "RETURN", "NAME" }, result.Tokens.Select(t => t.Type));
    }

    [Fact]
    public void Parse_WithInvalidSyntax_ReturnsErrors()
    {
        var result = _sut.Parse("local = 2");

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Parse_WithWhitespaceOnly_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.Parse("   \n\t  "));
    }
}
