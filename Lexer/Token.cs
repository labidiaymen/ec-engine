namespace ECEngine.Lexer;

// Enum for all token types
public enum TokenType
{
    Identifier,
    Number,
    Plus,
    Minus,
    Multiply,
    Divide,
    LeftParen,
    RightParen,
    LeftBrace,
    RightBrace,
    Semicolon,
    Dot,
    Assign,        // =
    Var,           // var keyword
    Let,           // let keyword
    Const,         // const keyword
    EOF
}

// Token class to hold type and value
public class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public int Position { get; }
    public int Line { get; }
    public int Column { get; }

    public Token(TokenType type, string value, int position, int line = 1, int column = 1)
    {
        Type = type;
        Value = value;
        Position = position;
        Line = line;
        Column = column;
    }

    public override string ToString()
    {
        return $"{Type}: {Value} (Line {Line}, Col {Column})";
    }
}
