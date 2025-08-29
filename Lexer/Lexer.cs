using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.AST;

namespace ECEngine.Lexer;

// Tokenizer for JavaScript code
public class Lexer
{
    private readonly string _code;
    private int _position;
    private char _currentChar;
    private int _line;
    private int _column;

    public Lexer(string code)
    {
        _code = code;
        _position = 0;
        _line = 1;
        _column = 1;
        _currentChar = _position < _code.Length ? _code[_position] : '\0';
    }

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

    private void SkipWhitespace()
    {
        while (_currentChar != '\0' && char.IsWhiteSpace(_currentChar))
        {
            Advance();
        }
    }

    private void SkipSingleLineComment()
    {
        // Skip //
        Advance(); // Skip first /
        Advance(); // Skip second /
        
        // Skip until end of line or end of file
        while (_currentChar != '\0' && _currentChar != '\n')
        {
            Advance();
        }
        
        // Skip the newline character if we're not at EOF
        if (_currentChar == '\n')
        {
            Advance();
        }
    }

    private void SkipMultiLineComment()
    {
        // Skip /*
        Advance(); // Skip /
        Advance(); // Skip *
        
        // Skip until we find */
        while (_currentChar != '\0')
        {
            if (_currentChar == '*')
            {
                Advance();
                if (_currentChar == '/')
                {
                    Advance(); // Skip the final /
                    break;
                }
            }
            else
            {
                Advance();
            }
        }
    }

    private string ReadNumber()
    {
        var start = _position;
        while (_currentChar != '\0' && (char.IsDigit(_currentChar) || _currentChar == '.'))
        {
            Advance();
        }
        return _code.Substring(start, _position - start);
    }

    private string ReadIdentifier()
    {
        var start = _position;
        while (_currentChar != '\0' && (char.IsLetterOrDigit(_currentChar) || _currentChar == '_'))
        {
            Advance();
        }
        return _code.Substring(start, _position - start);
    }

    private string ReadString()
    {
        Advance(); // Skip opening quote
        var start = _position;
        
        while (_currentChar != '\0' && _currentChar != '"')
        {
            if (_currentChar == '\\') // Handle escape sequences
            {
                Advance(); // Skip backslash
                if (_currentChar != '\0')
                {
                    Advance(); // Skip escaped character
                }
            }
            else
            {
                Advance();
            }
        }
        
        var stringValue = _code.Substring(start, _position - start);
        
        if (_currentChar == '"')
        {
            Advance(); // Skip closing quote
        }
        else
        {
            throw new Exception($"Unterminated string literal at line {_line}, column {_column}");
        }
        
        return stringValue;
    }

    private TokenType GetKeywordTokenType(string identifier)
    {
        return identifier switch
        {
            "var" => TokenType.Var,
            "let" => TokenType.Let,
            "const" => TokenType.Const,
            "function" => TokenType.Function,
            "return" => TokenType.Return,
            "observe" => TokenType.Observe,
            "when" => TokenType.When,
            "if" => TokenType.If,
            "else" => TokenType.Else,
            "true" => TokenType.True,
            "false" => TokenType.False,
            "export" => TokenType.Export,
            "import" => TokenType.Import,
            "from" => TokenType.From,
            "for" => TokenType.For,
            "while" => TokenType.While,
            "do" => TokenType.Do,
            "break" => TokenType.Break,
            "continue" => TokenType.Continue,
            _ => TokenType.Identifier
        };
    }

    // Tokenize input code into a list of tokens
    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (_currentChar != '\0')
        {
            if (char.IsWhiteSpace(_currentChar))
            {
                SkipWhitespace();
                continue;
            }

            var tokenLine = _line;
            var tokenColumn = _column;

            if (char.IsDigit(_currentChar))
            {
                var number = ReadNumber();
                tokens.Add(new Token(TokenType.Number, number, _position, tokenLine, tokenColumn));
                continue;
            }

            if (char.IsLetter(_currentChar))
            {
                var identifier = ReadIdentifier();
                var tokenType = GetKeywordTokenType(identifier);
                tokens.Add(new Token(tokenType, identifier, _position, tokenLine, tokenColumn));
                continue;
            }

            switch (_currentChar)
            {
                case '"':
                    var stringValue = ReadString();
                    tokens.Add(new Token(TokenType.String, stringValue, _position, tokenLine, tokenColumn));
                    break;
                case '+':
                    // Check for ++
                    if (_position + 1 < _code.Length && _code[_position + 1] == '+')
                    {
                        tokens.Add(new Token(TokenType.Increment, "++", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Plus, "+", _position, tokenLine, tokenColumn));
                        Advance();
                    }
                    break;
                case '-':
                    // Check for --
                    if (_position + 1 < _code.Length && _code[_position + 1] == '-')
                    {
                        tokens.Add(new Token(TokenType.Decrement, "--", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Minus, "-", _position, tokenLine, tokenColumn));
                        Advance();
                    }
                    break;
                case '*':
                    tokens.Add(new Token(TokenType.Multiply, "*", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case '/':
                    // Check for comments
                    if (_position + 1 < _code.Length)
                    {
                        var nextChar = _code[_position + 1];
                        if (nextChar == '/')
                        {
                            // Single-line comment
                            SkipSingleLineComment();
                            continue;
                        }
                        else if (nextChar == '*')
                        {
                            // Multi-line comment
                            SkipMultiLineComment();
                            continue;
                        }
                    }
                    
                    // Regular division operator
                    tokens.Add(new Token(TokenType.Divide, "/", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case '(':
                    tokens.Add(new Token(TokenType.LeftParen, "(", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case ')':
                    tokens.Add(new Token(TokenType.RightParen, ")", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case '{':
                    tokens.Add(new Token(TokenType.LeftBrace, "{", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case '}':
                    tokens.Add(new Token(TokenType.RightBrace, "}", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case ';':
                    tokens.Add(new Token(TokenType.Semicolon, ";", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case '.':
                    tokens.Add(new Token(TokenType.Dot, ".", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case ',':
                    tokens.Add(new Token(TokenType.Comma, ",", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case '=':
                    if (_position + 1 < _code.Length && _code[_position + 1] == '=')
                    {
                        tokens.Add(new Token(TokenType.Equal, "==", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Assign, "=", _position, tokenLine, tokenColumn));
                        Advance();
                    }
                    break;
                case '!':
                    if (_position + 1 < _code.Length && _code[_position + 1] == '=')
                    {
                        tokens.Add(new Token(TokenType.NotEqual, "!=", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.LogicalNot, "!", _position, tokenLine, tokenColumn));
                        Advance();
                    }
                    break;
                case '<':
                    if (_position + 1 < _code.Length && _code[_position + 1] == '=')
                    {
                        tokens.Add(new Token(TokenType.LessThanOrEqual, "<=", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.LessThan, "<", _position, tokenLine, tokenColumn));
                        Advance();
                    }
                    break;
                case '>':
                    if (_position + 1 < _code.Length && _code[_position + 1] == '=')
                    {
                        tokens.Add(new Token(TokenType.GreaterThanOrEqual, ">=", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.GreaterThan, ">", _position, tokenLine, tokenColumn));
                        Advance();
                    }
                    break;
                case '&':
                    if (_position + 1 < _code.Length && _code[_position + 1] == '&')
                    {
                        tokens.Add(new Token(TokenType.LogicalAnd, "&&", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else
                    {
                        throw new Exception($"Unexpected character: {_currentChar} at line {_line}, column {_column}");
                    }
                    break;
                case '|':
                    if (_position + 1 < _code.Length && _code[_position + 1] == '|')
                    {
                        tokens.Add(new Token(TokenType.LogicalOr, "||", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else
                    {
                        throw new Exception($"Unexpected character: {_currentChar} at line {_line}, column {_column}");
                    }
                    break;
                default:
                    throw new Exception($"Unexpected character: {_currentChar} at line {_line}, column {_column}");
            }
        }

        tokens.Add(new Token(TokenType.EOF, "", _position, _line, _column));
        return tokens;
    }
}
