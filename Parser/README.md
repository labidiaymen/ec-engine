# Parser Component

The Parser converts a stream of tokens from the lexer into an **Abstract Syntax Tree (AST)** that represents the structure and meaning of the JavaScript code.

## 🎯 Purpose

The parser performs syntactic analysis by:
- Consuming tokens in sequence from the lexer
- Building an Abstract Syntax Tree (AST) that represents program structure
- Handling operator precedence and associativity
- Providing meaningful parse error messages with location information
- Supporting recursive expression parsing

## 🌳 AST Node Types

The parser produces various AST node types:

### Program Structure
```csharp
ProgramNode              // Root of the AST, contains statements
└── Statement[]          // List of statements in the program
    ├── ExpressionStatement  // Statement containing an expression
    └── ... (future: VariableDeclaration, FunctionDeclaration)
```

### Expressions
```csharp
Expression               // Base class for all expressions
├── NumberLiteral        // Numeric values: 42, 3.14
├── Identifier           // Variable/function names: console, myVar
├── BinaryExpression     // Binary operations: 1 + 2, a * b
├── MemberExpression     // Property access: console.log, obj.prop
└── CallExpression       // Function calls: console.log(42)
```

## 📝 Parsing Algorithm

The parser uses **Recursive Descent Parsing** with the following precedence levels:

1. **Primary**: Numbers, identifiers, parenthesized expressions
2. **Member/Call**: Property access (`.`), function calls `()`
3. **Multiplicative**: `*`, `/` (left-associative)
4. **Additive**: `+`, `-` (left-associative)

### Precedence Hierarchy
```
Expression
├── Additive      (lowest precedence)
│   ├── Multiplicative
│   │   ├── Call
│   │   │   ├── Member
│   │   │   │   └── Primary  (highest precedence)
```

## 💻 Usage Examples

### Simple Expression
```csharp
var parser = new Parser();
var ast = parser.Parse("42");

// Results in:
// ProgramNode
// └── ExpressionStatement
//     └── NumberLiteral(42)
```

### Binary Expression
```csharp
var ast = parser.Parse("1 + 2 * 3");

// Results in (with correct precedence):
// ProgramNode
// └── ExpressionStatement
//     └── BinaryExpression("+")
//         ├── NumberLiteral(1)
//         └── BinaryExpression("*")
//             ├── NumberLiteral(2)
//             └── NumberLiteral(3)
```

### Function Call
```csharp
var ast = parser.Parse("console.log(1 + 2)");

// Results in:
// ProgramNode
// └── ExpressionStatement
//     └── CallExpression
//         ├── MemberExpression
//         │   ├── Identifier("console")
//         │   └── Property("log")
//         └── Arguments[]
//             └── BinaryExpression("+")
//                 ├── NumberLiteral(1)
//                 └── NumberLiteral(2)
```

## 🔧 Implementation Details

### Parser State

```csharp
public class Parser
{
    private List<Token> _tokens;       // Token stream from lexer
    private int _position;             // Current position in token stream
    private Token _currentToken;       // Current token being processed
    private string _sourceCode;        // Original source for error reporting
}
```

### Core Parsing Methods

#### Expression Parsing
```csharp
private Expression ParseExpression()
{
    return ParseAdditive();  // Start with lowest precedence
}

private Expression ParseAdditive()
{
    var left = ParseMultiplicative();
    
    while (_currentToken.Type == TokenType.Plus || _currentToken.Type == TokenType.Minus)
    {
        var op = _currentToken.Value;
        Advance();
        var right = ParseMultiplicative();
        left = new BinaryExpression(left, op, right);
    }
    
    return left;
}
```

#### Primary Expression Parsing
```csharp
private Expression ParsePrimary()
{
    if (_currentToken.Type == TokenType.Number)
    {
        var value = double.Parse(_currentToken.Value);
        var token = _currentToken;
        Advance();
        return new NumberLiteral(value, token);
    }
    
    if (_currentToken.Type == TokenType.Identifier)
    {
        var name = _currentToken.Value;
        var token = _currentToken;
        Advance();
        return new Identifier(name, token);
    }
    
    // Handle parenthesized expressions
    if (_currentToken.Type == TokenType.LeftParen)
    {
        Advance(); // consume '('
        var expression = ParseExpression();
        Consume(TokenType.RightParen, "Expected ')' after expression");
        return expression;
    }
    
    throw new ECEngineException($"Unexpected token: {_currentToken.Type}", 
        _currentToken.Line, _currentToken.Column, _sourceCode);
}
```

### Member Expression Parsing
```csharp
private Expression ParseMember()
{
    var expression = ParsePrimary();
    
    while (_currentToken.Type == TokenType.Dot)
    {
        Advance(); // consume '.'
        var property = Consume(TokenType.Identifier, "Expected property name after '.'");
        expression = new MemberExpression(expression, property.Value);
    }
    
    return expression;
}
```

### Function Call Parsing
```csharp
private Expression ParseCall()
{
    var expression = ParseMember();
    
    while (_currentToken.Type == TokenType.LeftParen)
    {
        Advance(); // consume '('
        var arguments = new List<Expression>();
        
        if (_currentToken.Type != TokenType.RightParen)
        {
            arguments.Add(ParseExpression());
            // Note: Real implementation would handle comma-separated arguments
        }
        
        Consume(TokenType.RightParen, "Expected ')' after arguments");
        expression = new CallExpression(expression, arguments);
    }
    
    return expression;
}
```

## 🛡️ Error Handling

The parser provides detailed error messages:

```csharp
private Token Consume(TokenType type, string message)
{
    if (_currentToken.Type == type)
    {
        var token = _currentToken;
        Advance();
        return token;
    }
    
    throw new ECEngineException($"{message}. Got {_currentToken.Type} instead.", 
        _currentToken.Line, _currentToken.Column, _sourceCode, 
        $"Expected {type} but found {_currentToken.Type}");
}
```

Example error output:
```
Parse Error at Line 1, Column 5: Expected ')' after expression. Got EOF instead.
Context: Expected RightParen but found EOF

Source:
>>>   1: (1 + 2
           ^
```

## ✅ Features

- ✅ **Operator Precedence**: Correct mathematical precedence (`*` before `+`)
- ✅ **Left Associativity**: `1 + 2 + 3` parsed as `((1 + 2) + 3)`
- ✅ **Parentheses**: Support for `(1 + 2) * 3`
- ✅ **Member Access**: Support for `console.log`
- ✅ **Function Calls**: Support for `func(args)`
- ✅ **Error Recovery**: Meaningful error messages with locations
- ✅ **Token Preservation**: AST nodes retain original tokens for error reporting

## 🧪 Testing

The parser is tested with **12 test cases** covering:

### Literal Parsing (3 tests)
- Simple numbers
- Decimal numbers  
- Identifiers

### Binary Expression Parsing (4 tests)
- Addition expressions
- Subtraction expressions
- Multiplication expressions
- Division expressions

### Function Call Parsing (3 tests)
- Simple console.log calls
- Console.log with expressions
- Member expression parsing

### Test Files
- `Tests/Parser/LiteralParsingTests.cs`
- `Tests/Parser/BinaryExpressionParsingTests.cs`
- `Tests/Parser/FunctionCallParsingTests.cs`

## 🚀 Future Enhancements

Potential improvements for the parser:

### Statements
- **Variable Declarations**: `var x = 5;`
- **Function Declarations**: `function add(a, b) { return a + b; }`
- **Control Flow**: `if`, `for`, `while` statements

### Expressions
- **Assignment**: `x = 5`, `x += 3`
- **Comparison**: `==`, `!=`, `<`, `>`
- **Logical**: `&&`, `||`, `!`
- **Conditional**: `condition ? true : false`

### Advanced Features
- **Object Literals**: `{key: value}`
- **Array Literals**: `[1, 2, 3]`
- **String Literals**: `"hello world"`
- **Arrow Functions**: `(x) => x * 2`

## 🔍 Debugging

To debug parser behavior, inspect the AST:

```csharp
var parser = new Parser();
var ast = parser.Parse("1 + 2 * 3");

// Walk the AST to understand structure
void PrintAST(ASTNode node, int indent = 0)
{
    var spaces = new string(' ', indent * 2);
    Console.WriteLine($"{spaces}{node.GetType().Name}");
    
    if (node is BinaryExpression binary)
    {
        Console.WriteLine($"{spaces}  Operator: {binary.Operator}");
        PrintAST(binary.Left, indent + 1);
        PrintAST(binary.Right, indent + 1);
    }
    else if (node is NumberLiteral number)
    {
        Console.WriteLine($"{spaces}  Value: {number.Value}");
    }
    // ... handle other node types
}
```

## 📐 Grammar

The parser implements this simplified JavaScript grammar:

```
Program         ::= Statement*
Statement       ::= ExpressionStatement
ExpressionStatement ::= Expression ';'?

Expression      ::= Additive
Additive        ::= Multiplicative (('+' | '-') Multiplicative)*
Multiplicative  ::= Call (('*' | '/') Call)*
Call            ::= Member ('(' ArgumentList? ')')*
Member          ::= Primary ('.' Identifier)*
Primary         ::= Number | Identifier | '(' Expression ')'

ArgumentList    ::= Expression (',' Expression)*
```
