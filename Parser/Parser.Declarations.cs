using ECEngine.AST;
using ECEngine.Lexer;
using ECEngine.Runtime;

namespace ECEngine.Parser;

/// <summary>
/// Variable declaration parsing methods for ECEngine parser
/// </summary>
public partial class Parser
{
    /// <summary>
    /// Parse a variable declaration (var, let, const)
    /// </summary>
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

    /// <summary>
    /// Parse a function declaration
    /// </summary>
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
}