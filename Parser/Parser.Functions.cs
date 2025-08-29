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
