using ECEngine.AST;
using ECEngine.Lexer;
using ECEngine.Runtime;

namespace ECEngine.Parser;

// Parser for JavaScript code
public class Parser
{
    private List<Token> _tokens = new List<Token>();
    private int _position;
    private Token _currentToken = new Token(TokenType.EOF, "", 0);
    private string _sourceCode = "";

    public Parser()
    {
        _position = 0;
    }

    private void Advance()
    {
        _position++;
        _currentToken = _position < _tokens.Count ? _tokens[_position] : _tokens[^1];
    }

    private Token Peek(int offset = 1)
    {
        var pos = _position + offset;
        return pos < _tokens.Count ? _tokens[pos] : _tokens[^1];
    }

    private bool Match(TokenType type)
    {
        if (_currentToken.Type == type)
        {
            Advance();
            return true;
        }
        return false;
    }

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

    // Parse code and return AST
    public ASTNode Parse(string code)
    {
        _sourceCode = code;
        var lexer = new Lexer.Lexer(code);
        _tokens = lexer.Tokenize();
        _position = 0;
        _currentToken = _tokens[0];

        var statements = new List<Statement>();
        
        while (_currentToken.Type != TokenType.EOF)
        {
            statements.Add(ParseStatement());
        }

        return new ProgramNode(statements);
    }

    private Statement ParseStatement()
    {
        var expression = ParseExpression();
        Match(TokenType.Semicolon); // Optional semicolon
        return new ExpressionStatement(expression);
    }

    private Expression ParseExpression()
    {
        return ParseAdditive();
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

    private Expression ParseMultiplicative()
    {
        var left = ParseCall();

        while (_currentToken.Type == TokenType.Multiply || _currentToken.Type == TokenType.Divide)
        {
            var op = _currentToken.Value;
            Advance();
            var right = ParseCall();
            left = new BinaryExpression(left, op, right);
        }

        return left;
    }

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
                while (Match(TokenType.Semicolon)) // In a real parser, this would be comma
                {
                    arguments.Add(ParseExpression());
                }
            }

            Consume(TokenType.RightParen, "Expected ')' after arguments");
            expression = new CallExpression(expression, arguments);
        }

        return expression;
    }

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

        if (_currentToken.Type == TokenType.LeftParen)
        {
            Advance(); // consume '('
            var expression = ParseExpression();
            Consume(TokenType.RightParen, "Expected ')' after expression");
            return expression;
        }

        throw new ECEngineException($"Unexpected token: {_currentToken.Type}", 
            _currentToken.Line, _currentToken.Column, _sourceCode,
            $"Cannot parse expression starting with {_currentToken.Type}");
    }
}
