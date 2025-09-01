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
        return ParseConditional();
    }

    /// <summary>
    /// Parse conditional (ternary) expressions (condition ? true : false)
    /// </summary>
    private Expression ParseConditional()
    {
        var expression = ParseAssignment();
        
        if (_currentToken.Type == TokenType.Question)
        {
            var token = _currentToken;
            Advance();
            var consequent = ParseAssignment();
            
            if (_currentToken.Type != TokenType.Colon)
            {
                throw new ECEngineException("Expected ':' in ternary expression", 
                    _currentToken.Line, _currentToken.Column, _sourceCode, "Ternary operator requires both '?' and ':'");
            }
            
            Advance(); // consume ':'
            var alternate = ParseConditional(); // Right-associative
            
            return new ConditionalExpression(expression, consequent, alternate, token);
        }
        
        return expression;
    }

    /// <summary>
    /// Parse assignment expressions (lowest precedence)
    /// </summary>
    private Expression ParseAssignment()
    {
        var expression = ParseArrowFunction();
        
        if (_currentToken.Type == TokenType.Assign)
        {
            var token = _currentToken;
            Advance();
            var right = ParseAssignment(); // Right-associative
            
            if (expression is Identifier identifier)
            {
                return new AssignmentExpression(identifier, right, token);
            }
            else if (expression is MemberExpression memberExpr)
            {
                return new MemberAssignmentExpression(memberExpr, right, token);
            }
            
            throw new ECEngineException("Invalid assignment target", 
                token.Line, token.Column, _sourceCode, "Only identifiers and member expressions can be assigned to");
        }
        else if (_currentToken.Type == TokenType.PlusAssign ||
                 _currentToken.Type == TokenType.MinusAssign ||
                 _currentToken.Type == TokenType.MultiplyAssign ||
                 _currentToken.Type == TokenType.DivideAssign)
        {
            var op = _currentToken.Value;
            var token = _currentToken;
            Advance();
            var right = ParseConditional(); // Parse ternary expressions properly
            
            if (expression is Identifier identifier)
            {
                return new CompoundAssignmentExpression(identifier, op, right, token);
            }
            
            throw new ECEngineException("Invalid compound assignment target", 
                token.Line, token.Column, _sourceCode, "Only identifiers can be used in compound assignment");
        }
        
        return expression;
    }

    /// <summary>
    /// Parse arrow function expressions (x => x * 2, (a, b) => a + b)
    /// </summary>
    private Expression ParseArrowFunction()
    {
        // Try to parse as arrow function
        var start = _position;
        var startLine = _currentToken.Line;
        var startColumn = _currentToken.Column;
        
        try
        {
            var parameters = new List<string>();
            
            // Check for parenthesized parameter list or single parameter
            if (_currentToken.Type == TokenType.LeftParen)
            {
                // Parenthesized parameters: (a, b) => ...
                Advance(); // consume '('
                
                if (_currentToken.Type != TokenType.RightParen)
                {
                    parameters.Add(Consume(TokenType.Identifier, "Expected parameter name").Value);
                    
                    while (Match(TokenType.Comma))
                    {
                        parameters.Add(Consume(TokenType.Identifier, "Expected parameter name after ','").Value);
                    }
                }
                
                Consume(TokenType.RightParen, "Expected ')' after parameters");
            }
            else if (_currentToken.Type == TokenType.Identifier)
            {
                // Single parameter without parentheses: x => ...
                parameters.Add(_currentToken.Value);
                Advance();
            }
            else
            {
                // Not an arrow function, backtrack
                RestorePosition(start, startLine, startColumn);
                return ParseLogicalOr();
            }
            
            // Must have arrow operator
            if (_currentToken.Type != TokenType.Arrow)
            {
                // Not an arrow function, backtrack
                RestorePosition(start, startLine, startColumn);
                return ParseLogicalOr();
            }
            
            var arrowToken = _currentToken;
            Advance(); // consume '=>'
            
            // Parse function body - either expression or block
            if (_currentToken.Type == TokenType.LeftBrace)
            {
                // Block body: x => { return x * 2; }
                var blockBody = ParseBlockStatement();
                return new ArrowFunctionExpression(parameters, blockBody.Body, arrowToken);
            }
            else
            {
                // Expression body: x => x * 2
                var body = ParseAssignment();
                return new ArrowFunctionExpression(parameters, body, arrowToken);
            }
        }
        catch
        {
            // If parsing fails, backtrack and parse as regular expression
            RestorePosition(start, startLine, startColumn);
            return ParseLogicalOr();
        }
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
        var left = ParseBitwiseOr();

        while (_currentToken.Type == TokenType.LogicalAnd)
        {
            var op = _currentToken.Value;
            var token = _currentToken;
            Advance();
            var right = ParseBitwiseOr();
            left = new LogicalExpression(left, op, right, token);
        }

        return left;
    }

    /// <summary>
    /// Parse bitwise OR expressions (|)
    /// </summary>
    private Expression ParseBitwiseOr()
    {
        var left = ParseBitwiseXor();

        while (_currentToken.Type == TokenType.BitwiseOr)
        {
            var op = _currentToken.Value;
            Advance();
            var right = ParseBitwiseXor();
            left = new BinaryExpression(left, op, right);
        }

        return left;
    }

    /// <summary>
    /// Parse bitwise XOR expressions (^)
    /// </summary>
    private Expression ParseBitwiseXor()
    {
        var left = ParseBitwiseAnd();

        while (_currentToken.Type == TokenType.BitwiseXor)
        {
            var op = _currentToken.Value;
            Advance();
            var right = ParseBitwiseAnd();
            left = new BinaryExpression(left, op, right);
        }

        return left;
    }

    /// <summary>
    /// Parse bitwise AND expressions (&)
    /// </summary>
    private Expression ParseBitwiseAnd()
    {
        var left = ParseEquality();

        while (_currentToken.Type == TokenType.BitwiseAnd)
        {
            var op = _currentToken.Value;
            Advance();
            var right = ParseEquality();
            left = new BinaryExpression(left, op, right);
        }

        return left;
    }

    /// <summary>
    /// Parse equality expressions (==, !=, ===, !==)
    /// </summary>
    private Expression ParseEquality()
    {
        var left = ParseRelational();

        while (_currentToken.Type == TokenType.Equal ||
               _currentToken.Type == TokenType.NotEqual ||
               _currentToken.Type == TokenType.StrictEqual ||
               _currentToken.Type == TokenType.StrictNotEqual)
        {
            var op = _currentToken.Value;
            Advance();
            var right = ParseRelational();
            left = new BinaryExpression(left, op, right);
        }

        return left;
    }

    /// <summary>
    /// Parse relational expressions (<, <=, >, >=)
    /// </summary>
    private Expression ParseRelational()
    {
        var left = ParseBitwiseShift();

        while (_currentToken.Type == TokenType.LessThan ||
               _currentToken.Type == TokenType.LessThanOrEqual ||
               _currentToken.Type == TokenType.GreaterThan ||
               _currentToken.Type == TokenType.GreaterThanOrEqual)
        {
            var op = _currentToken.Value;
            Advance();
            var right = ParseBitwiseShift();
            left = new BinaryExpression(left, op, right);
        }

        return left;
    }

    /// <summary>
    /// Parse bitwise shift expressions (<<, >>, >>>)
    /// </summary>
    private Expression ParseBitwiseShift()
    {
        var left = ParseAdditive();

        while (_currentToken.Type == TokenType.LeftShift ||
               _currentToken.Type == TokenType.RightShift ||
               _currentToken.Type == TokenType.UnsignedRightShift)
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
    /// Parse unary expressions (!, +, -, ++, --, ~) - HIGHEST PRECEDENCE
    /// </summary>
    private Expression ParseUnary()
    {
        // Handle prefix unary operators
        if (_currentToken.Type == TokenType.LogicalNot ||
            _currentToken.Type == TokenType.Increment ||
            _currentToken.Type == TokenType.Decrement ||
            _currentToken.Type == TokenType.Plus ||
            _currentToken.Type == TokenType.Minus ||
            _currentToken.Type == TokenType.BitwiseNot)
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

        while (_currentToken.Type == TokenType.LeftParen || _currentToken.Type == TokenType.Dot || _currentToken.Type == TokenType.LeftBracket)
        {
            if (_currentToken.Type == TokenType.LeftParen)
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
            else if (_currentToken.Type == TokenType.Dot)
            {
                var token = _currentToken;
                Advance(); // consume '.'
                var property = Consume(TokenType.Identifier, "Expected property name after '.'");
                expression = new MemberExpression(expression, property.Value, token);
            }
            else if (_currentToken.Type == TokenType.LeftBracket)
            {
                var token = _currentToken;
                Advance(); // consume '['
                var propertyExpr = ParseExpression();
                Consume(TokenType.RightBracket, "Expected ']' after computed property");
                expression = new MemberExpression(expression, propertyExpr, token);
            }
        }

        return expression;
    }

    /// <summary>
    /// Parse member access expressions (dot notation and bracket notation)
    /// </summary>
    private Expression ParseMember()
    {
        var expression = ParsePrimary();

        while (_currentToken.Type == TokenType.Dot || _currentToken.Type == TokenType.LeftBracket)
        {
            if (_currentToken.Type == TokenType.Dot)
            {
                var token = _currentToken;
                Advance(); // consume '.'
                var property = Consume(TokenType.Identifier, "Expected property name after '.'");
                expression = new MemberExpression(expression, property.Value, token);
            }
            else if (_currentToken.Type == TokenType.LeftBracket)
            {
                var token = _currentToken;
                Advance(); // consume '['
                var propertyExpr = ParseExpression();
                Consume(TokenType.RightBracket, "Expected ']' after computed property");
                expression = new MemberExpression(expression, propertyExpr, token);
            }
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

        if (_currentToken.Type == TokenType.TemplateLiteral)
        {
            return ParseTemplateLiteral();
        }

        if (_currentToken.Type == TokenType.TemplateStart)
        {
            return ParseTemplateLiteral();
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

        if (_currentToken.Type == TokenType.Null)
        {
            var token = _currentToken;
            Advance();
            return new NullLiteral(token);
        }

        if (_currentToken.Type == TokenType.This)
        {
            var token = _currentToken;
            Advance();
            return new ThisExpression(token);
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

        if (_currentToken.Type == TokenType.LeftBracket)
        {
            return ParseArrayLiteral();
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

    /// <summary>
    /// Parse array literal expressions [ element1, element2, ... ]
    /// </summary>
    private Expression ParseArrayLiteral()
    {
        var startToken = _currentToken;
        Consume(TokenType.LeftBracket, "Expected '['");
        
        var elements = new List<Expression>();
        
        // Handle empty array []
        if (_currentToken.Type == TokenType.RightBracket)
        {
            Advance();
            return new ArrayLiteral(elements, startToken);
        }
        
        // Parse array elements
        do
        {
            // Parse array element
            var element = ParseExpression();
            elements.Add(element);
            
            // Check for comma or end
            if (_currentToken.Type == TokenType.Comma)
            {
                Advance();
                // Allow trailing comma
                if (_currentToken.Type == TokenType.RightBracket)
                {
                    break;
                }
            }
            else if (_currentToken.Type == TokenType.RightBracket)
            {
                break;
            }
            else
            {
                throw new ECEngineException("Expected ',' or ']' after array element",
                    _currentToken.Line, _currentToken.Column, _sourceCode,
                    "Invalid array literal syntax");
            }
        } while (_currentToken.Type != TokenType.RightBracket && _currentToken.Type != TokenType.EOF);
        
        Consume(TokenType.RightBracket, "Expected ']' to close array literal");
        
        return new ArrayLiteral(elements, startToken);
    }
    
    /// <summary>
    /// Parse template literal expressions `text${expr}text`
    /// </summary>
    private Expression ParseTemplateLiteral()
    {
        var startToken = _currentToken;
        var elements = new List<TemplateElement>();
        
        // Handle simple template literal without interpolation
        if (_currentToken.Type == TokenType.TemplateLiteral)
        {
            var textValue = _currentToken.Value;
            elements.Add(new TemplateText(textValue, _currentToken));
            Advance();
            return new TemplateLiteral(elements, startToken);
        }
        
        // Handle template literal with interpolation
        while (_currentToken.Type != TokenType.EOF)
        {
            if (_currentToken.Type == TokenType.TemplateStart || _currentToken.Type == TokenType.TemplateMiddle)
            {
                // Add text portion
                var textValue = _currentToken.Value;
                elements.Add(new TemplateText(textValue, _currentToken));
                Advance();
                
                // Parse expression
                if (_currentToken.Type == TokenType.TemplateExpression)
                {
                    var expressionCode = _currentToken.Value;
                    
                    // Create a mini lexer and parser for the expression
                    var expressionLexer = new Lexer.Lexer(expressionCode);
                    var expressionTokens = expressionLexer.Tokenize();
                    var expressionParser = new Parser();
                    var expression = expressionParser.ParseExpressionFromTokens(expressionTokens, expressionCode);
                    
                    elements.Add(new TemplateExpression(expression, _currentToken));
                    Advance();
                }
                else
                {
                    throw new ECEngineException("Expected template expression after '${' in template literal",
                        _currentToken.Line, _currentToken.Column, _sourceCode,
                        "Template literal interpolation requires an expression");
                }
            }
            else if (_currentToken.Type == TokenType.TemplateEnd)
            {
                // Final text portion
                var textValue = _currentToken.Value;
                elements.Add(new TemplateText(textValue, _currentToken));
                Advance();
                break;
            }
            else
            {
                throw new ECEngineException("Unexpected token in template literal",
                    _currentToken.Line, _currentToken.Column, _sourceCode,
                    "Template literal parsing error");
            }
        }
        
        return new TemplateLiteral(elements, startToken);
    }
}