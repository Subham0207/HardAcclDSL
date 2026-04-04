using HardAcclDslApi.Models.Ast;
using HardAcclDslApi.Services;

namespace HardAcclDslApi.UnitTests;

public class AntlrLuaParserServiceAstMappingTests
{
    private readonly AntlrLuaParserService _sut = new();

    [Fact]
    public void Parse_LocalAssignment_MapsToLocalDeclarationAst()
    {
        var result = _sut.Parse("local a = 2 + 3");

        var program = Assert.IsType<ProgramNode>(result.AstRoot);
        var statement = Assert.IsType<LocalDeclarationStatementNode>(Assert.Single(program.Statements));

        Assert.Equal("a", statement.Name);
        var binary = Assert.IsType<BinaryExpressionNode>(statement.Value);
        Assert.Equal("+", binary.Operator);
    }

    [Fact]
    public void Parse_Assignment_MapsToAssignmentAst()
    {
        var result = _sut.Parse("a = b");

        var program = Assert.IsType<ProgramNode>(result.AstRoot);
        var statement = Assert.IsType<AssignmentStatementNode>(Assert.Single(program.Statements));

        Assert.Equal("a", statement.Name);
        var identifier = Assert.IsType<IdentifierExpressionNode>(statement.Value);
        Assert.Equal("b", identifier.Name);
    }

    [Fact]
    public void Parse_Return_MapsToReturnAst()
    {
        var result = _sut.Parse("return value");

        var program = Assert.IsType<ProgramNode>(result.AstRoot);
        var statement = Assert.IsType<ReturnStatementNode>(Assert.Single(program.Statements));

        var identifier = Assert.IsType<IdentifierExpressionNode>(statement.Value);
        Assert.Equal("value", identifier.Name);
    }

    [Fact]
    public void Parse_CallStatement_MapsToExpressionStatementAst()
    {
        var result = _sut.Parse("print(1, a)");

        var program = Assert.IsType<ProgramNode>(result.AstRoot);
        var statement = Assert.IsType<ExpressionStatementNode>(Assert.Single(program.Statements));
        var call = Assert.IsType<CallExpressionNode>(statement.Expression);

        Assert.Equal("print", call.FunctionName);
        Assert.Equal(2, call.Arguments.Count);
        Assert.IsType<NumberLiteralExpressionNode>(call.Arguments[0]);
        Assert.IsType<IdentifierExpressionNode>(call.Arguments[1]);
    }

    [Fact]
    public void Parse_ParenthesizedExpression_PreservesPrecedenceInAst()
    {
        var result = _sut.Parse("local x = (2 + 3) * 4");

        var program = Assert.IsType<ProgramNode>(result.AstRoot);
        var statement = Assert.IsType<LocalDeclarationStatementNode>(Assert.Single(program.Statements));
        var topBinary = Assert.IsType<BinaryExpressionNode>(statement.Value);

        Assert.Equal("*", topBinary.Operator);
        var leftBinary = Assert.IsType<BinaryExpressionNode>(topBinary.Left);
        Assert.Equal("+", leftBinary.Operator);
    }

    [Fact]
    public void Parse_StringLiteral_MapsToStringLiteralAst()
    {
        var result = _sut.Parse("local msg = \"hello\"");

        var program = Assert.IsType<ProgramNode>(result.AstRoot);
        var statement = Assert.IsType<LocalDeclarationStatementNode>(Assert.Single(program.Statements));
        var literal = Assert.IsType<StringLiteralExpressionNode>(statement.Value);

        Assert.Equal("\"hello\"", literal.RawText);
    }
}
