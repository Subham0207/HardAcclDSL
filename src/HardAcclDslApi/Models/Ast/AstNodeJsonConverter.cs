using System.Text.Json;
using System.Text.Json.Serialization;

namespace HardAcclDslApi.Models.Ast;

public sealed class AstNodeJsonConverter : JsonConverter<AstNode>
{
    public override AstNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("AST deserialization is not supported yet.");
    }

    public override void Write(Utf8JsonWriter writer, AstNode value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("nodeId", value.NodeId);
        writer.WriteString("kind", value.Kind.ToString());
        if (!string.IsNullOrWhiteSpace(value.GraphNodeType))
        {
            writer.WriteString("graphNodeType", value.GraphNodeType);
        }
        if (value.GraphX.HasValue)
        {
            writer.WriteNumber("graphX", value.GraphX.Value);
        }
        if (value.GraphY.HasValue)
        {
            writer.WriteNumber("graphY", value.GraphY.Value);
        }

        switch (value)
        {
            case ProgramNode program:
                writer.WritePropertyName("statements");
                JsonSerializer.Serialize(writer, program.Statements, options);
                break;

            case LocalDeclarationStatementNode localDecl:
                writer.WriteString("name", localDecl.Name);
                writer.WritePropertyName("value");
                JsonSerializer.Serialize(writer, localDecl.Value, options);
                break;

            case AssignmentStatementNode assign:
                writer.WriteString("name", assign.Name);
                writer.WritePropertyName("value");
                JsonSerializer.Serialize(writer, assign.Value, options);
                break;

            case ReturnStatementNode ret:
                writer.WritePropertyName("value");
                JsonSerializer.Serialize(writer, ret.Value, options);
                break;

            case IdentifierExpressionNode identifier:
                writer.WriteString("name", identifier.Name);
                break;

            case NumberLiteralExpressionNode number:
                writer.WriteString("rawText", number.RawText);
                break;

            case StringLiteralExpressionNode str:
                writer.WriteString("rawText", str.RawText);
                break;

            case BinaryExpressionNode binary:
                writer.WriteString("operator", binary.Operator);
                writer.WritePropertyName("left");
                JsonSerializer.Serialize(writer, binary.Left, options);
                writer.WritePropertyName("right");
                JsonSerializer.Serialize(writer, binary.Right, options);
                break;

            case FunctionCallNode call:
                writer.WriteString("functionName", call.FunctionName);
                writer.WritePropertyName("arguments");
                JsonSerializer.Serialize(writer, call.Arguments, options);
                break;

            case FunctionDeclarationNode functionDecl:
                writer.WriteString("name", functionDecl.Name);
                writer.WritePropertyName("parameters");
                JsonSerializer.Serialize(writer, functionDecl.Parameters, options);
                writer.WritePropertyName("body");
                JsonSerializer.Serialize(writer, functionDecl.Body, options);
                break;

            default:
                throw new NotSupportedException($"Unsupported AST node type: {value.GetType().Name}");
        }

        writer.WriteEndObject();
    }
}
