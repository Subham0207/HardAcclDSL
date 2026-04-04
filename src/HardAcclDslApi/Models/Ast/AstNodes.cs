namespace HardAcclDslApi.Models.Ast;

public enum AstNodeKind
{
    Program,
    LocalDeclarationStatement,
    AssignmentStatement,
    ReturnStatement,
    IdentifierExpression,
    NumberLiteralExpression,
    StringLiteralExpression,
    BinaryExpression,
    FunctionCall,
    FunctionDeclaration
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
    public List<AstNode> Statements { get; init; } = new();
}

public sealed class LocalDeclarationStatementNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.LocalDeclarationStatement;
    public string Name { get; init; } = string.Empty;
    public AstNode Value { get; init; } = new IdentifierExpressionNode();
}

public sealed class AssignmentStatementNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.AssignmentStatement;
    public string Name { get; init; } = string.Empty;
    public AstNode Value { get; init; } = new IdentifierExpressionNode();
}

public sealed class ReturnStatementNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.ReturnStatement;
    public AstNode Value { get; init; } = new IdentifierExpressionNode();
}

public sealed class IdentifierExpressionNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.IdentifierExpression;
    public string Name { get; init; } = string.Empty;
}

public sealed class NumberLiteralExpressionNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.NumberLiteralExpression;
    public string RawText { get; init; } = string.Empty;
}

public sealed class StringLiteralExpressionNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.StringLiteralExpression;
    public string RawText { get; init; } = string.Empty;
}

public sealed class BinaryExpressionNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.BinaryExpression;
    public string Operator { get; init; } = string.Empty;
    public AstNode Left { get; init; } = new IdentifierExpressionNode();
    public AstNode Right { get; init; } = new IdentifierExpressionNode();
}

public sealed class FunctionCallNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.FunctionCall;
    public string FunctionName { get; init; } = string.Empty;
    public List<AstNode> Arguments { get; init; } = new();
}

public sealed class FunctionDeclarationNode : AstNode
{
    public override AstNodeKind Kind => AstNodeKind.FunctionDeclaration;
    public string Name { get; init; } = string.Empty;
    public List<string> Parameters { get; init; } = new();
    public List<AstNode> Body { get; init; } = new();
}
