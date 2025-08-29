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
        
        // Parse the target expression (could be simple identifier or property chain)
        var targetExpression = ParseObserveTarget();
        
        // Parse function expression handler
        var handler = ParseFunctionExpressionHandler();
        
        Match(TokenType.Semicolon); // Optional semicolon
        
        return new ObserveStatement(targetExpression, handler, token);
    }

    /// <summary>
    /// Parse the target of an observe statement (identifier or property chain)
    /// </summary>
    private Expression ParseObserveTarget()
    {
        var identifier = new Identifier(Consume(TokenType.Identifier, "Expected identifier").Value, _currentToken);
        Expression target = identifier;
        
        // Handle property chains: server.requests, server.connection.data, etc.
        while (_currentToken.Type == TokenType.Dot)
        {
            Advance(); // consume dot
            var property = Consume(TokenType.Identifier, "Expected property name").Value;
            target = new MemberExpression(target, property, _currentToken);
        }
        
        return target;
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

    /// <summary>
    /// Parse an otherwise statement: otherwise { ... } - used as a fallback within observe handlers
    /// </summary>
    private OtherwiseStatement ParseOtherwiseStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Otherwise, "Expected 'otherwise' keyword");
        
        // Parse body block
        var body = ParseBlockStatement();
        
        return new OtherwiseStatement(body, token);
    }
}
