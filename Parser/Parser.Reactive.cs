using ECEngine.AST;
using ECEngine.Lexer;
using ECEngine.Runtime;

namespace ECEngine.Parser;

/// <summary>
/// Reactive programming features parsing methods for ECEngine parser
/// These are ECEngine-specific extensions beyond standard ECMAScript
/// </summary>
public partial class Parser
{
    /// <summary>
    /// Parse an observe statement: observe x function(oldVal, newVal) { ... }
    /// </summary>
    private Statement ParseObserveStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Observe, "Expected 'observe' keyword");
        
        // Check if it's multi-variable syntax: observe (x, y)
        if (_currentToken.Type == TokenType.LeftParen)
        {
            return ParseMultiObserveStatement(token);
        }
        
        // Single variable syntax: observe x
        var variableName = Consume(TokenType.Identifier, "Expected variable name").Value;
        
        // Parse function expression handler
        var handler = ParseFunctionExpressionHandler();
        
        Match(TokenType.Semicolon); // Optional semicolon
        
        return new ObserveStatement(variableName, handler, token);
    }

    /// <summary>
    /// Parse a multi-variable observe statement: observe (x, y, z) function(changes) { ... }
    /// </summary>
    private MultiObserveStatement ParseMultiObserveStatement(Token token)
    {
        Consume(TokenType.LeftParen, "Expected '('");
        
        var variableNames = new List<string>();
        
        // Parse variable list: x, y, z
        while (_currentToken.Type == TokenType.Identifier)
        {
            variableNames.Add(_currentToken.Value);
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
        
        Consume(TokenType.RightParen, "Expected ')' after variable list");
        
        // Parse function expression handler
        var handler = ParseFunctionExpressionHandler();
        
        Match(TokenType.Semicolon); // Optional semicolon
        
        return new MultiObserveStatement(variableNames, handler, token);
    }

    /// <summary>
    /// Parse a when statement: when (condition) { ... } - used within observe handlers
    /// </summary>
    private WhenStatement ParseWhenStatement()
    {
        var token = _currentToken;
        Consume(TokenType.When, "Expected 'when' keyword");
        
        // Parse condition expression
        Expression condition;
        
        // Handle parenthesized expressions: when (x && y)
        if (_currentToken.Type == TokenType.LeftParen)
        {
            Advance();
            condition = ParseExpression();
            Consume(TokenType.RightParen, "Expected ')' after when condition");
        }
        else
        {
            // Simple identifier: when x
            condition = ParseExpression();
        }
        
        // Parse body block
        var body = ParseBlockStatement();
        
        return new WhenStatement(condition, body, token);
    }
}
