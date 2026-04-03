namespace HardAcclDslApi.Models
{
    public class Token
    {
        public Kind Kind { get; set; }
        public string Lexeme { get; set; }
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
}