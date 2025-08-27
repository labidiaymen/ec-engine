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
                tokens.Add(new Token(TokenType.Identifier, identifier, _position, tokenLine, tokenColumn));
                continue;
            }

            switch (_currentChar)
            {
                case '+':
                    tokens.Add(new Token(TokenType.Plus, "+", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case '-':
                    tokens.Add(new Token(TokenType.Minus, "-", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case '*':
                    tokens.Add(new Token(TokenType.Multiply, "*", _position, tokenLine, tokenColumn));
                    Advance();
                    break;
                case '/':
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
                default:
                    throw new Exception($"Unexpected character: {_currentChar} at line {_line}, column {_column}");
            }
        }

        tokens.Add(new Token(TokenType.EOF, "", _position, _line, _column));
        return tokens;
    }
}
