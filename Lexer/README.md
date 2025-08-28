# Lexer Component

The Lexer is responsible for **tokenization** - converting raw JavaScript source code into a stream of tokens that can be processed by the parser.

## 🎯 Purpose

The lexer performs lexical analysis by:
- Reading source code character by character
- Identifying different token types (numbers, identifiers, operators, etc.)
- Tracking precise location information (line/column numbers)
- Handling whitespace and producing meaningful error messages

## 📋 Token Types

The lexer recognizes the following token types:

```csharp
public enum TokenType
{
    Identifier,     // Variable names, function names (e.g., "console", "myVar")
    Number,         // Numeric literals (e.g., "42", "3.14")
    Plus,           // Addition operator "+"
    Minus,          // Subtraction operator "-"
    Multiply,       // Multiplication operator "*"
    Divide,         // Division operator "/"
    LeftParen,      // Opening parenthesis "("
    RightParen,     // Closing parenthesis ")"
    LeftBrace,      // Opening brace "{"
    RightBrace,     // Closing brace "}"
    Semicolon,      // Statement terminator ";"
    Dot,            // Property access "."
    EOF             // End of file marker
}
```

## 🏗️ Token Structure

Each token contains:

```csharp
public class Token
{
    public TokenType Type { get; }     // The type of token
    public string Value { get; }       // The actual text value
    public int Position { get; }       // Character position in source
    public int Line { get; }           // Line number (1-based)
    public int Column { get; }         // Column number (1-based)
}
```

## 💻 Usage Examples

### Basic Tokenization

```csharp
var lexer = new Lexer("1 + 2");
var tokens = lexer.Tokenize();

// Results in:
// Token(Number, "1", pos=0, line=1, col=1)
// Token(Plus, "+", pos=2, line=1, col=3)
// Token(Number, "2", pos=4, line=1, col=5)
// Token(EOF, "", pos=5, line=1, col=6)
```

### Complex Expression

```csharp
var lexer = new Lexer("console.log(10 * 5)");
var tokens = lexer.Tokenize();

// Results in tokens for:
// console, ., log, (, 10, *, 5, ), EOF
```

### Error Handling

```csharp
var lexer = new Lexer("1 @ 2");  // Invalid character '@'
try {
    var tokens = lexer.Tokenize();
} catch (Exception ex) {
    // Error: "Unexpected character: @ at line 1, column 3"
}
```

## 🔧 Implementation Details

### Character Processing

The lexer uses a **single-character lookahead** approach:

1. **Current Character**: `_currentChar` - the character being processed
2. **Position Tracking**: Maintains `_position`, `_line`, and `_column`
3. **Advance Method**: Moves to the next character and updates position

### Number Recognition

```csharp
private string ReadNumber()
{
    var start = _position;
    while (_currentChar != '\0' && (char.IsDigit(_currentChar) || _currentChar == '.'))
    {
        Advance();
    }
    return _code.Substring(start, _position - start);
}
```

Supports:
- Integer numbers: `42`, `0`, `123`
- Decimal numbers: `3.14`, `0.5`, `123.456`

### Identifier Recognition

```csharp
private string ReadIdentifier()
{
    var start = _position;
    while (_currentChar != '\0' && (char.IsLetterOrDigit(_currentChar) || _currentChar == '_'))
    {
        Advance();
    }
    return _code.Substring(start, _position - start);
}
```

Supports:
- Variable names: `myVar`, `console`, `log`
- Underscore support: `my_variable`, `_private`
- Alphanumeric: `var1`, `item2`

### Location Tracking

The lexer tracks precise locations for error reporting:

```csharp
private void Advance()
{
    if (_currentChar == '\n')
    {
        _line++;
        _column = 1;
    }
    else
    {
        _column++;
    }
    
    _position++;
    _currentChar = _position < _code.Length ? _code[_position] : '\0';
}
```

## ✅ Features

- ✅ **Position Tracking**: Precise line and column numbers
- ✅ **Number Support**: Integers and decimals
- ✅ **Identifier Support**: Variables and function names
- ✅ **Operator Support**: Arithmetic and punctuation
- ✅ **Error Handling**: Meaningful error messages with location
- ✅ **Whitespace Handling**: Automatically skips whitespace
- ✅ **EOF Handling**: Proper end-of-file token generation

## 🧪 Testing

The lexer is thoroughly tested with **16 test cases** covering:

### Basic Tokenization (3 tests)
- Simple numbers
- Identifiers
- Decimal numbers

### Operator Tokenization (4 tests)
- Plus operator (+)
- Minus operator (-)
- Multiply operator (*)
- Divide operator (/)

### Expression Tokenization (3 tests)
- Simple arithmetic expressions
- Complex multi-operator expressions
- Parenthesized expressions

### Location Tracking (3 tests)
- Single token location
- Multiple token locations
- Error location reporting

### Test Files
- `Tests/Lexer/BasicTokenizationTests.cs`
- `Tests/Lexer/OperatorTokenizationTests.cs`
- `Tests/Lexer/ExpressionTokenizationTests.cs`
- `Tests/Lexer/LocationTrackingTests.cs`

## 🚀 Future Enhancements

Potential improvements for the lexer:

- **String Literals**: Support for `"hello"` and `'world'`
- **Comments**: Support for `//` and `/* */` comments
- **Keywords**: Reserved words like `var`, `function`, `if`
- **More Operators**: `==`, `!=`, `<=`, `>=`, `&&`, `||`
- **Regular Expressions**: Support for `/pattern/flags`

## 🔍 Debugging

To debug lexer behavior, you can inspect tokens:

```csharp
var lexer = new Lexer("console.log(42)");
var tokens = lexer.Tokenize();

foreach (var token in tokens)
{
    Console.WriteLine($"{token.Type}: '{token.Value}' at Line {token.Line}, Col {token.Column}");
}
```

Output:
```
Identifier: 'console' at Line 1, Col 1
Dot: '.' at Line 1, Col 8
Identifier: 'log' at Line 1, Col 9
LeftParen: '(' at Line 1, Col 12
Number: '42' at Line 1, Col 13
RightParen: ')' at Line 1, Col 15
EOF: '' at Line 1, Col 16
```
