using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
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

        IParseTree parseTree = parser.chunk();
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
            ParseTree = parseTree.ToStringTree(parser),
            ParseTreeRoot = BuildParseTreeNode(parseTree, parser)
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
