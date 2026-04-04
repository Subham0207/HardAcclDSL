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
            ParseTree = parseTree.ToStringTree(parser)
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
