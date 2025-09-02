namespace ECEngine.Lexer;

// Enum for all token types
public enum TokenType
{
    Identifier,
    Number,
    String,         // string literals like "hello"
    Plus,
    Minus,
    Multiply,
    Divide,
    LeftParen,
    RightParen,
    LeftBrace,
    RightBrace,
    LeftBracket,
    RightBracket,
    Semicolon,
    Dot,
    Comma,          // ,
    Assign,         // =
    Equal,          // ==
    StrictEqual,    // ===
    NotEqual,       // !=
    StrictNotEqual, // !==
    LessThan,       // <
    LessThanOrEqual, // <=
    GreaterThan,    // >
    GreaterThanOrEqual, // >=
    LogicalAnd,     // &&
    LogicalOr,      // ||
    
    // Compound Assignment operators
    PlusAssign,     // +=
    MinusAssign,    // -=
    MultiplyAssign, // *=
    DivideAssign,   // /=
    
    // Bitwise operators
    BitwiseAnd,     // &
    BitwiseOr,      // |
    BitwiseXor,     // ^
    BitwiseNot,     // ~
    LeftShift,      // <<
    RightShift,     // >>
    UnsignedRightShift, // >>>
    
    // Ternary operator
    Question,       // ?
    Var,            // var keyword
    Let,            // let keyword
    Const,          // const keyword
    Function,       // function keyword
    Return,         // return keyword
    Observe,        // observe keyword
    When,           // when keyword
    Otherwise,      // otherwise keyword
    If,             // if keyword
    Else,           // else keyword
    True,           // true keyword
    False,          // false keyword
    Null,           // null keyword
    This,           // this keyword
    Export,         // export keyword
    Import,         // import keyword
    From,           // from keyword
    For,            // for keyword
    While,          // while keyword
    Do,             // do keyword
    Break,          // break keyword
    Continue,       // continue keyword
    Switch,         // switch keyword
    Case,           // case keyword
    Default,        // default keyword
    Try,            // try keyword
    Catch,          // catch keyword
    Finally,        // finally keyword
    Throw,          // throw keyword
    In,             // in keyword (for...in loops)
    Of,             // of keyword (for...of loops)
    As,             // as keyword (for export renaming)
    Yield,          // yield keyword (for generator functions)
    New,            // new keyword
    Typeof,         // typeof keyword
    Colon,          // :
    
    // Template literal tokens
    TemplateLiteral,    // `simple template`
    TemplateStart,      // `text${
    TemplateMiddle,     // }text${  
    TemplateEnd,        // }text`
    TemplateExpression, // expression inside ${}
    
    // Unary operators
    Increment,      // ++
    Decrement,      // --
    LogicalNot,     // !
    UnaryPlus,      // +x (when used as unary)
    UnaryMinus,     // -x (when used as unary)
    
    // Arrow function operator
    Arrow,          // =>
    
    // Regular expression literal
    Regex,          // /pattern/flags
    
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
