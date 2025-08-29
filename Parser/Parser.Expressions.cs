using ECEngine.AST;
using ECEngine.Lexer;
using ECEngine.Runtime;

namespace ECEngine.Parser;

/// <summary>
/// Expression parsing methods for ECEngine parser
/// </summary>
public partial class Parser
{
    /// <summary>
    /// Parse an expression (entry point for all expressions)
    /// </summary>
    private Expression ParseExpression()
    {
        return ParseAssignment();
    }

    /// <summary>
    /// Parse assignment expressions (lowest precedence)
    /// </summary>
    private Expression ParseAssignment()
    {
        var expression = ParseLogicalOr();
        
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

    /// <summary>
    /// Parse logical OR expressions (||)
    /// </summary>
    private Expression ParseLogicalOr()
    {
        var left = ParseLogicalAnd();

        while (_currentToken.Type == TokenType.LogicalOr)
        {
            var op = _currentToken.Value;
            var token = _currentToken;
            Advance();
            var right = ParseLogicalAnd();
            left = new LogicalExpression(left, op, right, token);
        }

        return left;
    }

    /// <summary>
    /// Parse logical AND expressions (&&)
    /// </summary>
    private Expression ParseLogicalAnd()
    {
        var left = ParseComparison();

        while (_currentToken.Type == TokenType.LogicalAnd)
        {
            var op = _currentToken.Value;
            var token = _currentToken;
            Advance();
            var right = ParseComparison();
            left = new LogicalExpression(left, op, right, token);
        }

        return left;
    }

    /// <summary>
    /// Parse comparison expressions (==, !=, <, <=, >, >=)
    /// </summary>
    private Expression ParseComparison()
    {
        var left = ParseAdditive();

        while (_currentToken.Type == TokenType.Equal ||
               _currentToken.Type == TokenType.NotEqual ||
               _currentToken.Type == TokenType.LessThan ||
               _currentToken.Type == TokenType.LessThanOrEqual ||
               _currentToken.Type == TokenType.GreaterThan ||
               _currentToken.Type == TokenType.GreaterThanOrEqual)
        {
            var op = _currentToken.Value;
            Advance();
            var right = ParseAdditive();
            left = new BinaryExpression(left, op, right);
        }

        return left;
    }

    /// <summary>
    /// Parse additive expressions (+, -) - PRECEDENCE LEVEL 5
    /// </summary>
    private Expression ParseAdditive()
    {
        var left = ParseMultiplicative(); // ← CALLS HIGHER PRECEDENCE FIRST!

        while (_currentToken.Type == TokenType.Plus || _currentToken.Type == TokenType.Minus)
        {
            var op = _currentToken.Value;
            Advance();
            var right = ParseMultiplicative(); // ← ENSURES * and / are grouped first
            left = new BinaryExpression(left, op, right);
        }

        return left;
    }

    /// <summary>
    /// Parse multiplicative expressions (*, /) - PRECEDENCE LEVEL 6 (HIGHER than +/-)
    /// </summary>
    private Expression ParseMultiplicative()
    {
        var left = ParseUnary();

        while (_currentToken.Type == TokenType.Multiply || _currentToken.Type == TokenType.Divide)
        {
            var op = _currentToken.Value;
            Advance();
            var right = ParseUnary();
            left = new BinaryExpression(left, op, right);
        }

        return left;
    }

    /// <summary>
    /// Parse unary expressions (!, +, -, ++, --) - HIGHEST PRECEDENCE
    /// </summary>
    private Expression ParseUnary()
    {
        // Handle prefix unary operators
        if (_currentToken.Type == TokenType.LogicalNot ||
            _currentToken.Type == TokenType.Increment ||
            _currentToken.Type == TokenType.Decrement ||
            _currentToken.Type == TokenType.Plus ||
            _currentToken.Type == TokenType.Minus)
        {
            var op = _currentToken.Value;
            var token = _currentToken;
            Advance();
            var operand = ParseUnary(); // Right-associative
            
            // Convert Plus/Minus to UnaryPlus/UnaryMinus for clarity
            if (op == "+") op = "unary+";
            if (op == "-") op = "unary-";
            
            return new UnaryExpression(op, operand, true, token);
        }

        // Handle postfix unary operators (++ and --)
        var expression = ParseCall();
        
        if (_currentToken.Type == TokenType.Increment || _currentToken.Type == TokenType.Decrement)
        {
            var op = _currentToken.Value;
            var token = _currentToken;
            Advance();
            return new UnaryExpression(op, expression, false, token);
        }

        return expression;
    }

    /// <summary>
    /// Parse function call expressions
    /// </summary>
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

    /// <summary>
    /// Parse member access expressions (obj.property)
    /// </summary>
    private Expression ParseMember()
    {
        var expression = ParsePrimary();

        while (_currentToken.Type == TokenType.Dot)
        {
            var token = _currentToken;
            Advance(); // consume '.'
            var property = Consume(TokenType.Identifier, "Expected property name after '.'");
            expression = new MemberExpression(expression, property.Value, token);
        }

        return expression;
    }

    /// <summary>
    /// Parse primary expressions (literals, identifiers, parentheses)
    /// </summary>
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

        if (_currentToken.Type == TokenType.True)
        {
            var token = _currentToken;
            Advance();
            return new BooleanLiteral(true, token);
        }

        if (_currentToken.Type == TokenType.False)
        {
            var token = _currentToken;
            Advance();
            return new BooleanLiteral(false, token);
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

        if (_currentToken.Type == TokenType.LeftBrace)
        {
            return ParseObjectLiteral();
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

    /// <summary>
    /// Parse object literal expressions { key: value, ... }
    /// </summary>
    private Expression ParseObjectLiteral()
    {
        var startToken = _currentToken;
        Consume(TokenType.LeftBrace, "Expected '{'");
        
        var properties = new List<ObjectProperty>();
        
        // Handle empty object {}
        if (_currentToken.Type == TokenType.RightBrace)
        {
            Advance();
            return new ObjectLiteral(properties, startToken);
        }
        
        // Parse properties
        do
        {
            // Parse property key (identifier or string)
            string key;
            if (_currentToken.Type == TokenType.Identifier)
            {
                key = _currentToken.Value;
                Advance();
            }
            else if (_currentToken.Type == TokenType.String)
            {
                key = _currentToken.Value;
                Advance();
            }
            else
            {
                throw new ECEngineException("Expected property name (identifier or string)",
                    _currentToken.Line, _currentToken.Column, _sourceCode,
                    "Object properties must have a key");
            }
            
            Consume(TokenType.Colon, "Expected ':' after property key");
            
            // Parse property value
            var value = ParseExpression();
            
            properties.Add(new ObjectProperty(key, value, _currentToken));
            
            // Check for comma or end
            if (_currentToken.Type == TokenType.Comma)
            {
                Advance();
                // Allow trailing comma
                if (_currentToken.Type == TokenType.RightBrace)
                {
                    break;
                }
            }
            else if (_currentToken.Type == TokenType.RightBrace)
            {
                break;
            }
            else
            {
                throw new ECEngineException("Expected ',' or '}' after property value",
                    _currentToken.Line, _currentToken.Column, _sourceCode,
                    "Invalid object literal syntax");
            }
        } while (_currentToken.Type != TokenType.RightBrace && _currentToken.Type != TokenType.EOF);
        
        Consume(TokenType.RightBrace, "Expected '}' to close object literal");
        
        return new ObjectLiteral(properties, startToken);
    }
}