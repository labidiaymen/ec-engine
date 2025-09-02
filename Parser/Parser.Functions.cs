using ECEngine.AST;
using ECEngine.Lexer;
using ECEngine.Runtime;

namespace ECEngine.Parser;

/// <summary>
/// Function parsing methods for ECEngine parser
/// </summary>
public partial class Parser
{
    /// <summary>
    /// Parse a function expression with parameters and body
    /// </summary>
    private Expression ParseFunctionExpression()
    {
        var token = _currentToken;
        Consume(TokenType.Function, "Expected 'function' keyword");
        
        // Check if this is a generator function (function*)
        bool isGenerator = false;
        if (_currentToken.Type == TokenType.Multiply)
        {
            isGenerator = true;
            Advance(); // consume the *
        }
        
        // Optional function name (for named function expressions)
        string? functionName = null;
        if (_currentToken.Type == TokenType.Identifier)
        {
            functionName = _currentToken.Value;
            Advance(); // consume the function name
        }
        
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
        
        if (isGenerator)
        {
            return new GeneratorFunctionExpression(parameters, body.Body, token);
        }
        else
        {
            return new FunctionExpression(parameters, body.Body, token, functionName);
        }
    }

    /// <summary>
    /// Parse function expression parameters and body (helper for observe statements)
    /// </summary>
    private FunctionExpression ParseFunctionExpressionHandler()
    {
        var token = _currentToken;
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
        
        return new FunctionExpression(parameters, body, token);
    }
}
