using HardAcclDslApi.Models.Ast;
using Scriban;

namespace HardAcclDslApi.Services;

public sealed class AstToLuaScribanRenderer
{
    private static readonly Template LocalDeclarationTemplate = ParseTemplate("local {{ name }} = {{ value }}");
    private static readonly Template AssignmentTemplate = ParseTemplate("{{ name }} = {{ value }}");
    private static readonly Template ReturnTemplate = ParseTemplate("return {{ value }}");
    private static readonly Template IdentifierTemplate = ParseTemplate("{{ name }}");
    private static readonly Template NumberTemplate = ParseTemplate("{{ raw_text }}");
    private static readonly Template StringTemplate = ParseTemplate("{{ raw_text }}");
    private static readonly Template BinaryTemplate = ParseTemplate("{{ left }} {{ operator }} {{ right }}");
    private static readonly Template FunctionCallTemplate = ParseTemplate("{{ function_name }}({{ arguments }})");

    public string RenderProgram(ProgramNode program)
    {
        var lines = program.Statements.Select(RenderNode).ToList();
        return string.Join("\n", lines);
    }

    private string RenderNode(AstNode node)
    {
        return node switch
        {
            LocalDeclarationStatementNode localDecl => LocalDeclarationTemplate.Render(new
            {
                name = localDecl.Name,
                value = RenderNode(localDecl.Value),
            }),
            AssignmentStatementNode assignment => AssignmentTemplate.Render(new
            {
                name = assignment.Name,
                value = RenderNode(assignment.Value),
            }),
            ReturnStatementNode ret => ReturnTemplate.Render(new
            {
                value = RenderNode(ret.Value),
            }),
            IdentifierExpressionNode identifier => IdentifierTemplate.Render(new
            {
                name = identifier.Name,
            }),
            NumberLiteralExpressionNode number => NumberTemplate.Render(new
            {
                raw_text = number.RawText,
            }),
            StringLiteralExpressionNode str => StringTemplate.Render(new
            {
                raw_text = str.RawText,
            }),
            BinaryExpressionNode binary => BinaryTemplate.Render(new
            {
                left = RenderOperand(binary.Left),
                @operator = binary.Operator,
                right = RenderOperand(binary.Right),
            }),
            FunctionCallNode call => FunctionCallTemplate.Render(new
            {
                function_name = call.FunctionName,
                arguments = string.Join(", ", call.Arguments.Select(RenderNode)),
            }),
            ProgramNode program => RenderProgram(program),
            _ => throw new NotSupportedException($"Unsupported AST node type for Lua rendering: {node.GetType().Name}"),
        };
    }

    private string RenderOperand(AstNode operand)
    {
        var rendered = RenderNode(operand);
        return operand is BinaryExpressionNode ? $"({rendered})" : rendered;
    }

    private static Template ParseTemplate(string templateText)
    {
        var template = Template.Parse(templateText);
        if (template.HasErrors)
        {
            var errors = string.Join("; ", template.Messages.Select(message => message.ToString()));
            throw new InvalidOperationException($"Invalid Lua template: {errors}");
        }

        return template;
    }
}
