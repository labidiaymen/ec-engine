using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.AST;

namespace ECEngine.Lexer;

// Tokenizer for JavaScript code
public class Lexer
{
    private readonly string _code;
    public Lexer(string code) => _code = code;

    // Tokenize input code into a list of tokens
    public List<Token> Tokenize()
    {
        // Stub: returns empty list for now
        return new List<Token>();
    }
}
