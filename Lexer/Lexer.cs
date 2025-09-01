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
        char quote = _currentChar; // Store the opening quote (either ' or ")
        Advance(); // Skip opening quote
        var result = new System.Text.StringBuilder();
        
        while (_currentChar != '\0' && _currentChar != quote)
        {
            if (_currentChar == '\\') // Handle escape sequences
            {
                Advance(); // Skip backslash
                if (_currentChar != '\0')
                {
                    switch (_currentChar)
                    {
                        case 'n':
                            result.Append('\n');
                            break;
                        case 't':
                            result.Append('\t');
                            break;
                        case 'r':
                            result.Append('\r');
                            break;
                        case '\\':
                            result.Append('\\');
                            break;
                        case '"':
                            result.Append('"');
                            break;
                        case '\'':
                            result.Append('\'');
                            break;
                        case 'b':
                            result.Append('\b');
                            break;
                        case 'f':
                            result.Append('\f');
                            break;
                        case 'v':
                            result.Append('\v');
                            break;
                        case '0':
                            result.Append('\0');
                            break;
                        default:
                            // For unrecognized escape sequences, just include the character as-is
                            result.Append(_currentChar);
                            break;
                    }
                    Advance(); // Skip escaped character
                }
            }
            else
            {
                result.Append(_currentChar);
                Advance();
            }
        }
        
        if (_currentChar == quote)
        {
            Advance(); // Skip closing quote
        }
        else
        {
            throw new Exception($"Unterminated string literal at line {_line}, column {_column}");
        }
        
        return result.ToString();
    }

    private List<Token> ReadTemplateLiteral()
    {
        var tokens = new List<Token>();
        var startLine = _line;
        var startColumn = _column;
        
        Advance(); // Skip opening backtick
        var textBuilder = new System.Text.StringBuilder();
        
        while (_currentChar != '\0' && _currentChar != '`')
        {
            if (_currentChar == '$' && _position + 1 < _code.Length && _code[_position + 1] == '{')
            {
                // Found interpolation start
                var textValue = textBuilder.ToString();
                if (textValue.Length > 0 || tokens.Count == 0)
                {
                    // Add text part (either TemplateStart or TemplateMiddle)
                    var tokenType = tokens.Count == 0 ? TokenType.TemplateStart : TokenType.TemplateMiddle;
                    tokens.Add(new Token(tokenType, textValue, _position, startLine, startColumn));
                }
                
                // Skip ${ 
                Advance(); // Skip $
                Advance(); // Skip {
                
                // Find matching }
                var braceLevel = 1;
                var exprStart = _position;
                var exprBuilder = new System.Text.StringBuilder();
                
                while (_currentChar != '\0' && braceLevel > 0)
                {
                    if (_currentChar == '{')
                        braceLevel++;
                    else if (_currentChar == '}')
                        braceLevel--;
                        
                    if (braceLevel > 0)
                    {
                        exprBuilder.Append(_currentChar);
                        Advance();
                    }
                }
                
                if (braceLevel == 0)
                {
                    // Add expression token
                    var exprValue = exprBuilder.ToString();
                    tokens.Add(new Token(TokenType.TemplateExpression, exprValue, exprStart, _line, _column));
                    Advance(); // Skip closing }
                }
                else
                {
                    throw new Exception($"Unterminated template expression at line {_line}, column {_column}");
                }
                
                textBuilder.Clear();
            }
            else
            {
                // Handle escape sequences in template literals
                if (_currentChar == '\\' && _position + 1 < _code.Length)
                {
                    Advance(); // Skip backslash
                    switch (_currentChar)
                    {
                        case 'n':
                            textBuilder.Append('\n');
                            break;
                        case 't':
                            textBuilder.Append('\t');
                            break;
                        case 'r':
                            textBuilder.Append('\r');
                            break;
                        case '\\':
                            textBuilder.Append('\\');
                            break;
                        case '`':
                            textBuilder.Append('`');
                            break;
                        case '$':
                            textBuilder.Append('$');
                            break;
                        default:
                            textBuilder.Append(_currentChar);
                            break;
                    }
                }
                else
                {
                    textBuilder.Append(_currentChar);
                }
                Advance();
            }
        }
        
        if (_currentChar == '`')
        {
            // Add final text part only if there's text or if it's a simple template
            var finalText = textBuilder.ToString();
            
            // Always add TemplateEnd for complex templates, or TemplateLiteral for simple ones
            var tokenType = tokens.Count == 0 ? TokenType.TemplateLiteral : TokenType.TemplateEnd;
            tokens.Add(new Token(tokenType, finalText, _position, startLine, startColumn));
            
            Advance(); // Skip closing backtick
        }
        else
        {
            throw new Exception($"Unterminated template literal at line {_line}, column {_column}");
        }
        
        return tokens;
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
            "otherwise" => TokenType.Otherwise,
            "if" => TokenType.If,
            "else" => TokenType.Else,
            "true" => TokenType.True,
            "false" => TokenType.False,
            "null" => TokenType.Null,
            "this" => TokenType.This,
            "export" => TokenType.Export,
            "import" => TokenType.Import,
            "from" => TokenType.From,
            "for" => TokenType.For,
            "while" => TokenType.While,
            "do" => TokenType.Do,
            "break" => TokenType.Break,
            "continue" => TokenType.Continue,
            "switch" => TokenType.Switch,
            "case" => TokenType.Case,
            "default" => TokenType.Default,
            "try" => TokenType.Try,
            "catch" => TokenType.Catch,
            "finally" => TokenType.Finally,
            "throw" => TokenType.Throw,
            "in" => TokenType.In,
            "of" => TokenType.Of,
            "as" => TokenType.As,
            "yield" => TokenType.Yield,
            "new" => TokenType.New,
            "typeof" => TokenType.Typeof,
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
                case '\'':
                    var stringValue = ReadString();
                    tokens.Add(new Token(TokenType.String, stringValue, _position, tokenLine, tokenColumn));
                    break;
                case '`':
                    var templateTokens = ReadTemplateLiteral();
                    tokens.AddRange(templateTokens);
                    break;
                case '+':
                    // Check for ++ or +=
                    if (_position + 1 < _code.Length && _code[_position + 1] == '+')
                    {
                        tokens.Add(new Token(TokenType.Increment, "++", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else if (_position + 1 < _code.Length && _code[_position + 1] == '=')
                    {
                        tokens.Add(new Token(TokenType.PlusAssign, "+=", _position, tokenLine, tokenColumn));
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
                    // Check for -- or -=
                    if (_position + 1 < _code.Length && _code[_position + 1] == '-')
                    {
                        tokens.Add(new Token(TokenType.Decrement, "--", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else if (_position + 1 < _code.Length && _code[_position + 1] == '=')
                    {
                        tokens.Add(new Token(TokenType.MinusAssign, "-=", _position, tokenLine, tokenColumn));
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
                    // Check for *=
                    if (_position + 1 < _code.Length && _code[_position + 1] == '=')
                    {
                        tokens.Add(new Token(TokenType.MultiplyAssign, "*=", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Multiply, "*", _position, tokenLine, tokenColumn));
                        Advance();
                    }
                    break;
                case '/':
                    // Check for comments first
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
                        else if (nextChar == '=')
                        {
                            // /= operator
                            tokens.Add(new Token(TokenType.DivideAssign, "/=", _position, tokenLine, tokenColumn));
                            Advance();
                            Advance();
                            break;
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
                case '[':
                    tokens.Add(new Token(TokenType.LeftBracket, "[", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case ']':
                    tokens.Add(new Token(TokenType.RightBracket, "]", _position, tokenLine, tokenColumn));
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
                    if (_position + 2 < _code.Length && _code[_position + 1] == '=' && _code[_position + 2] == '=')
                    {
                        tokens.Add(new Token(TokenType.StrictEqual, "===", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                        Advance();
                    }
                    else if (_position + 1 < _code.Length && _code[_position + 1] == '=')
                    {
                        tokens.Add(new Token(TokenType.Equal, "==", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else if (_position + 1 < _code.Length && _code[_position + 1] == '>')
                    {
                        tokens.Add(new Token(TokenType.Arrow, "=>", _position, tokenLine, tokenColumn));
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
                    if (_position + 2 < _code.Length && _code[_position + 1] == '=' && _code[_position + 2] == '=')
                    {
                        tokens.Add(new Token(TokenType.StrictNotEqual, "!==", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                        Advance();
                    }
                    else if (_position + 1 < _code.Length && _code[_position + 1] == '=')
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
                    if (_position + 1 < _code.Length && _code[_position + 1] == '<')
                    {
                        tokens.Add(new Token(TokenType.LeftShift, "<<", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else if (_position + 1 < _code.Length && _code[_position + 1] == '=')
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
                    if (_position + 2 < _code.Length && _code[_position + 1] == '>' && _code[_position + 2] == '>')
                    {
                        tokens.Add(new Token(TokenType.UnsignedRightShift, ">>>", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                        Advance();
                    }
                    else if (_position + 1 < _code.Length && _code[_position + 1] == '>')
                    {
                        tokens.Add(new Token(TokenType.RightShift, ">>", _position, tokenLine, tokenColumn));
                        Advance();
                        Advance();
                    }
                    else if (_position + 1 < _code.Length && _code[_position + 1] == '=')
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
                        tokens.Add(new Token(TokenType.BitwiseAnd, "&", _position, tokenLine, tokenColumn));
                        Advance();
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
                        tokens.Add(new Token(TokenType.BitwiseOr, "|", _position, tokenLine, tokenColumn));
                        Advance();
                    }
                    break;
                case '^':
                    tokens.Add(new Token(TokenType.BitwiseXor, "^", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case '~':
                    tokens.Add(new Token(TokenType.BitwiseNot, "~", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case '?':
                    tokens.Add(new Token(TokenType.Question, "?", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case ':':
                    tokens.Add(new Token(TokenType.Colon, ":", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                default:
                    throw new Exception($"Unexpected character: {_currentChar} at line {_line}, column {_column}");
            }
        }

        tokens.Add(new Token(TokenType.EOF, "", _position, _line, _column));
        return tokens;
    }
}
