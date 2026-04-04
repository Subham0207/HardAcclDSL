using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using HardAcclDslApi.Models.Ast;
using HardAcclDslApi.Models.Parsing;

namespace HardAcclDslApi.Services;

public sealed class AntlrLuaParserService
{
    public ParseResult Parse(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Source cannot be null or whitespace.", nameof(source));
        }

        var errors = new List<SyntaxError>();
        var input = new AntlrInputStream(source);
        var lexer = new LuaSubsetLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new LuaSubsetParser(tokens);

        parser.RemoveErrorListeners();

        var errorListener = new CollectingErrorListener(errors);
        parser.AddErrorListener(errorListener);

        LuaSubsetParser.ChunkContext chunkContext = parser.chunk();
        tokens.Fill();

        var tokenInfos = tokens.GetTokens()
            .Where(t => t.Type != TokenConstants.EOF)
            .Select(t => new TokenInfo
            {
                Type = lexer.Vocabulary.GetSymbolicName(t.Type) ?? t.Type.ToString(),
                Lexeme = t.Text ?? string.Empty,
                Line = t.Line,
                Column = t.Column + 1
            })
            .ToList();

        return new ParseResult
        {
            Errors = errors,
            Tokens = tokenInfos,
            ParseTree = chunkContext.ToStringTree(parser),
            ParseTreeRoot = BuildParseTreeNode(chunkContext, parser),
            AstRoot = errors.Count == 0 ? BuildAst(chunkContext) : null
        };
    }

    private static ProgramNode BuildAst(LuaSubsetParser.ChunkContext chunkContext)
    {
        var statements = new List<StatementNode>();

        foreach (var statementContext in chunkContext.statement())
        {
            statements.Add(BuildStatement(statementContext));
        }

        return new ProgramNode
        {
            Statements = statements
        };
    }

    private static StatementNode BuildStatement(LuaSubsetParser.StatementContext statementContext)
    {
        if (statementContext.localAssignStmt() is { } localAssign)
        {
            return new LocalDeclarationStatementNode
            {
                Name = localAssign.NAME().GetText(),
                Value = BuildExpression(localAssign.expression())
            };
        }

        if (statementContext.assignStmt() is { } assign)
        {
            return new AssignmentStatementNode
            {
                Name = assign.NAME().GetText(),
                Value = BuildExpression(assign.expression())
            };
        }

        if (statementContext.returnStmt() is { } returnStmt)
        {
            return new ReturnStatementNode
            {
                Value = BuildExpression(returnStmt.expression())
            };
        }

        if (statementContext.callStmt() is { } callStmt)
        {
            return new ExpressionStatementNode
            {
                Expression = BuildCallExpression(callStmt.functionCall())
            };
        }

        throw new InvalidOperationException("Unsupported statement in current Lua subset.");
    }

    private static ExpressionNode BuildExpression(LuaSubsetParser.ExpressionContext expressionContext)
    {
        return BuildAdditiveExpression(expressionContext.additiveExpr());
    }

    private static ExpressionNode BuildAdditiveExpression(LuaSubsetParser.AdditiveExprContext additiveContext)
    {
        var terms = additiveContext.multiplicativeExpr();
        ExpressionNode current = BuildMultiplicativeExpression(terms[0]);

        for (int i = 1; i < terms.Length; i++)
        {
            string op = additiveContext.GetChild(2 * i - 1).GetText();
            ExpressionNode right = BuildMultiplicativeExpression(terms[i]);

            current = new BinaryExpressionNode
            {
                Operator = op,
                Left = current,
                Right = right
            };
        }

        return current;
    }

    private static ExpressionNode BuildMultiplicativeExpression(LuaSubsetParser.MultiplicativeExprContext multiplicativeContext)
    {
        var factors = multiplicativeContext.primaryExpr();
        ExpressionNode current = BuildPrimaryExpression(factors[0]);

        for (int i = 1; i < factors.Length; i++)
        {
            string op = multiplicativeContext.GetChild(2 * i - 1).GetText();
            ExpressionNode right = BuildPrimaryExpression(factors[i]);

            current = new BinaryExpressionNode
            {
                Operator = op,
                Left = current,
                Right = right
            };
        }

        return current;
    }

    private static ExpressionNode BuildPrimaryExpression(LuaSubsetParser.PrimaryExprContext primaryContext)
    {
        if (primaryContext.NUMBER() is { } numberToken)
        {
            return new NumberLiteralExpressionNode
            {
                RawText = numberToken.GetText()
            };
        }

        if (primaryContext.STRING() is { } stringToken)
        {
            return new StringLiteralExpressionNode
            {
                RawText = stringToken.GetText()
            };
        }

        if (primaryContext.NAME() is { } nameToken)
        {
            return new IdentifierExpressionNode
            {
                Name = nameToken.GetText()
            };
        }

        if (primaryContext.functionCall() is { } call)
        {
            return BuildCallExpression(call);
        }

        if (primaryContext.expression() is { } nestedExpression)
        {
            return BuildExpression(nestedExpression);
        }

        throw new InvalidOperationException("Unsupported primary expression in current Lua subset.");
    }

    private static CallExpressionNode BuildCallExpression(LuaSubsetParser.FunctionCallContext functionCallContext)
    {
        var args = new List<ExpressionNode>();
        var argumentList = functionCallContext.argumentList();
        if (argumentList is not null)
        {
            foreach (var expressionContext in argumentList.expression())
            {
                args.Add(BuildExpression(expressionContext));
            }
        }

        return new CallExpressionNode
        {
            FunctionName = functionCallContext.NAME().GetText(),
            Arguments = args
        };
    }

    private static ParseTreeNode BuildParseTreeNode(IParseTree node, Parser parser)
    {
        if (node is ITerminalNode terminalNode)
        {
            var symbol = terminalNode.Symbol;
            var tokenName = parser.Vocabulary.GetSymbolicName(symbol.Type) ?? symbol.Type.ToString();

            return new ParseTreeNode
            {
                NodeType = "Terminal",
                Name = tokenName,
                Text = symbol.Text ?? string.Empty,
                Line = symbol.Line,
                Column = symbol.Column + 1
            };
        }

        if (node is ParserRuleContext ruleContext)
        {
            string ruleName = ruleContext.RuleIndex >= 0 && ruleContext.RuleIndex < parser.RuleNames.Length
                ? parser.RuleNames[ruleContext.RuleIndex]
                : "UnknownRule";

            var children = new List<ParseTreeNode>();
            for (int i = 0; i < ruleContext.ChildCount; i++)
            {
                children.Add(BuildParseTreeNode(ruleContext.GetChild(i), parser));
            }

            return new ParseTreeNode
            {
                NodeType = "Rule",
                Name = ruleName,
                Text = ruleContext.GetText(),
                Line = ruleContext.Start?.Line,
                Column = ruleContext.Start is null ? null : ruleContext.Start.Column + 1,
                Children = children
            };
        }

        return new ParseTreeNode
        {
            NodeType = "Unknown",
            Name = node.GetType().Name,
            Text = node.GetText()
        };
    }

    private sealed class CollectingErrorListener : BaseErrorListener
    {
        private readonly List<SyntaxError> _errors;

        public CollectingErrorListener(List<SyntaxError> errors)
        {
            _errors = errors;
        }

        public override void SyntaxError(
            TextWriter output,
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            _errors.Add(new SyntaxError
            {
                Line = line,
                Column = charPositionInLine + 1,
                Message = msg
            });
        }
    }
}
