using System.Text.Json.Serialization;

namespace HardAcclDslApi.Models.Ast;

public enum AstNodeKind
{
    Program,
    LocalDeclarationStatement,
    AssignmentStatement,
    ReturnStatement,
    ExpressionStatement,
    IdentifierExpression,
    NumberLiteralExpression,
    StringLiteralExpression,
    BinaryExpression,
    CallExpression
}

public abstract class AstNode
{
    // Stable node id that UI clients can use for selection, updates, and patch operations.
    public string NodeId { get; init; } = Guid.NewGuid().ToString("N");
    public abstract AstNodeKind Kind { get; }
}

public sealed class ProgramNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.Program;
    public List<StatementNode> Statements { get; init; } = new();
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(LocalDeclarationStatementNode), "localDecl")]
[JsonDerivedType(typeof(AssignmentStatementNode), "assign")]
[JsonDerivedType(typeof(ReturnStatementNode), "return")]
[JsonDerivedType(typeof(ExpressionStatementNode), "exprStmt")]
public abstract class StatementNode : AstNode
{
}

public sealed class LocalDeclarationStatementNode : StatementNode
{
    public override AstNodeKind Kind => AstNodeKind.LocalDeclarationStatement;
    public string Name { get; init; } = string.Empty;
    public ExpressionNode Value { get; init; } = new IdentifierExpressionNode();
}

public sealed class AssignmentStatementNode : StatementNode
{
    public override AstNodeKind Kind => AstNodeKind.AssignmentStatement;
    public string Name { get; init; } = string.Empty;
    public ExpressionNode Value { get; init; } = new IdentifierExpressionNode();
}

public sealed class ReturnStatementNode : StatementNode
{
    public override AstNodeKind Kind => AstNodeKind.ReturnStatement;
    public ExpressionNode Value { get; init; } = new IdentifierExpressionNode();
}

public sealed class ExpressionStatementNode : StatementNode
{
    public override AstNodeKind Kind => AstNodeKind.ExpressionStatement;
    public ExpressionNode Expression { get; init; } = new IdentifierExpressionNode();
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(IdentifierExpressionNode), "identifier")]
[JsonDerivedType(typeof(NumberLiteralExpressionNode), "number")]
[JsonDerivedType(typeof(StringLiteralExpressionNode), "string")]
[JsonDerivedType(typeof(BinaryExpressionNode), "binary")]
[JsonDerivedType(typeof(CallExpressionNode), "call")]
public abstract class ExpressionNode : AstNode
{
}

public sealed class IdentifierExpressionNode : ExpressionNode
{
    public override AstNodeKind Kind => AstNodeKind.IdentifierExpression;
    public string Name { get; init; } = string.Empty;
}

public sealed class NumberLiteralExpressionNode : ExpressionNode
{
    public override AstNodeKind Kind => AstNodeKind.NumberLiteralExpression;
    public string RawText { get; init; } = string.Empty;
}

public sealed class StringLiteralExpressionNode : ExpressionNode
{
    public override AstNodeKind Kind => AstNodeKind.StringLiteralExpression;
    public string RawText { get; init; } = string.Empty;
}

public sealed class BinaryExpressionNode : ExpressionNode
{
    public override AstNodeKind Kind => AstNodeKind.BinaryExpression;
    public string Operator { get; init; } = string.Empty;
    public ExpressionNode Left { get; init; } = new IdentifierExpressionNode();
    public ExpressionNode Right { get; init; } = new IdentifierExpressionNode();
}

public sealed class CallExpressionNode : ExpressionNode
{
    public override AstNodeKind Kind => AstNodeKind.CallExpression;
    public string FunctionName { get; init; } = string.Empty;
    public List<ExpressionNode> Arguments { get; init; } = new();
}
