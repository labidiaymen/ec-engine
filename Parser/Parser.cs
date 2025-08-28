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
        // Check for function declarations
        if (_currentToken.Type == TokenType.Function)
        {
            return ParseFunctionDeclaration();
        }
        
        // Check for return statements
        if (_currentToken.Type == TokenType.Return)
        {
            return ParseReturnStatement();
        }
        
        // Check for observe statements
        if (_currentToken.Type == TokenType.Observe)
        {
            return ParseObserveStatement();
        }
        
        // Check for variable declarations
        if (_currentToken.Type == TokenType.Var || _currentToken.Type == TokenType.Let || _currentToken.Type == TokenType.Const)
        {
            return ParseVariableDeclaration();
        }
        
        // Check for block statements
        if (_currentToken.Type == TokenType.LeftBrace)
        {
            return ParseBlockStatement();
        }
        
        // Otherwise, it's an expression statement
        var expression = ParseExpression();
        Match(TokenType.Semicolon); // Optional semicolon
        return new ExpressionStatement(expression);
    }

    private VariableDeclaration ParseVariableDeclaration()
    {
        var kind = _currentToken.Value; // "var", "let", or "const"
        var token = _currentToken;
        Advance();
        
        var name = Consume(TokenType.Identifier, "Expected identifier after variable declaration").Value;
        
        Expression? initializer = null;
        if (Match(TokenType.Assign))
        {
            initializer = ParseExpression();
        }
        
        Match(TokenType.Semicolon); // Optional semicolon
        return new VariableDeclaration(kind, name, initializer, token);
    }

    private Expression ParseExpression()
    {
        return ParseAssignment();
    }

    private Expression ParseAssignment()
    {
        var expression = ParseAdditive();
        
        if (_currentToken.Type == TokenType.Assign)
        {
            var token = _currentToken;
            Advance();
            var right = ParseAssignment(); // Right-associative
            
            if (expression is Identifier identifier)
            {
                return new AssignmentExpression(identifier, right, token);
            }
            
            throw new ECEngineException("Invalid assignment target", 
                token.Line, token.Column, _sourceCode, "Only identifiers can be assigned to");
        }
        
        return expression;
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
                while (Match(TokenType.Comma))
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

        if (_currentToken.Type == TokenType.String)
        {
            var value = _currentToken.Value;
            var token = _currentToken;
            Advance();
            return new StringLiteral(value, token);
        }

        if (_currentToken.Type == TokenType.Identifier)
        {
            var name = _currentToken.Value;
            var token = _currentToken;
            Advance();
            return new Identifier(name, token);
        }

        if (_currentToken.Type == TokenType.Function)
        {
            return ParseFunctionExpression();
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

    private FunctionDeclaration ParseFunctionDeclaration()
    {
        var token = _currentToken;
        Consume(TokenType.Function, "Expected 'function' keyword");
        
        var name = Consume(TokenType.Identifier, "Expected function name").Value;
        
        Consume(TokenType.LeftParen, "Expected '(' after function name");
        
        var parameters = new List<string>();
        if (_currentToken.Type != TokenType.RightParen)
        {
            parameters.Add(Consume(TokenType.Identifier, "Expected parameter name").Value);
            
            while (Match(TokenType.Comma))
            {
                parameters.Add(Consume(TokenType.Identifier, "Expected parameter name after ','").Value);
            }
        }
        
        Consume(TokenType.RightParen, "Expected ')' after parameters");
        
        var body = ParseBlockStatement();
        
        return new FunctionDeclaration(name, parameters, body.Body, token);
    }

    private FunctionExpression ParseFunctionExpression()
    {
        var token = _currentToken;
        Consume(TokenType.Function, "Expected 'function' keyword");
        
        Consume(TokenType.LeftParen, "Expected '(' after 'function'");
        
        var parameters = new List<string>();
        if (_currentToken.Type != TokenType.RightParen)
        {
            parameters.Add(Consume(TokenType.Identifier, "Expected parameter name").Value);
            
            while (Match(TokenType.Comma))
            {
                parameters.Add(Consume(TokenType.Identifier, "Expected parameter name after ','").Value);
            }
        }
        
        Consume(TokenType.RightParen, "Expected ')' after parameters");
        
        var body = ParseBlockStatement();
        
        return new FunctionExpression(parameters, body.Body, token);
    }

    private ReturnStatement ParseReturnStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Return, "Expected 'return' keyword");
        
        Expression? argument = null;
        if (_currentToken.Type != TokenType.Semicolon && _currentToken.Type != TokenType.EOF)
        {
            argument = ParseExpression();
        }
        
        Match(TokenType.Semicolon); // Optional semicolon
        
        return new ReturnStatement(argument, token);
    }

    private ObserveStatement ParseObserveStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Observe, "Expected 'observe' keyword");
        
        // Parse variable name to observe
        var variableName = Consume(TokenType.Identifier, "Expected variable name").Value;
        
        // Parse function expression handler
        Consume(TokenType.Function, "Expected 'function' keyword");
        
        // Parse parameters (optional)
        Consume(TokenType.LeftParen, "Expected '(' after 'function'");
        var parameters = new List<string>();
        
        while (_currentToken.Type == TokenType.Identifier)
        {
            parameters.Add(_currentToken.Value);
            Advance();
            
            if (_currentToken.Type == TokenType.Comma)
            {
                Advance();
            }
            else
            {
                break;
            }
        }
        
        Consume(TokenType.RightParen, "Expected ')' after function parameters");
        
        // Parse function body
        var body = ParseBlockStatement().Body;
        
        var handler = new FunctionExpression(parameters, body, token);
        
        Match(TokenType.Semicolon); // Optional semicolon
        
        return new ObserveStatement(variableName, handler, token);
    }

    private BlockStatement ParseBlockStatement()
    {
        var token = _currentToken;
        Consume(TokenType.LeftBrace, "Expected '{'");
        
        var statements = new List<Statement>();
        
        while (_currentToken.Type != TokenType.RightBrace && _currentToken.Type != TokenType.EOF)
        {
            statements.Add(ParseStatement());
        }
        
        Consume(TokenType.RightBrace, "Expected '}'");
        
        return new BlockStatement(statements, token);
    }
}
