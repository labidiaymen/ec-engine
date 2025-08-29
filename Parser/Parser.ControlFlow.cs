using ECEngine.AST;
using ECEngine.Lexer;
using ECEngine.Runtime;

namespace ECEngine.Parser;

/// <summary>
/// Control flow statement parsing methods for ECEngine parser
/// (if/else, try/catch/finally, throw statements)
/// </summary>
public partial class Parser
{
    /// <summary>
    /// Parse an if statement: if (condition) statement [else statement]
    /// </summary>
    private IfStatement ParseIfStatement()
    {
        var token = _currentToken;
        Consume(TokenType.If, "Expected 'if' keyword");
        
        Consume(TokenType.LeftParen, "Expected '(' after 'if'");
        var condition = ParseExpression();
        Consume(TokenType.RightParen, "Expected ')' after if condition");
        
        var thenStatement = ParseStatement();
        
        Statement? elseStatement = null;
        if (_currentToken.Type == TokenType.Else)
        {
            Consume(TokenType.Else, "Expected 'else' keyword");
            elseStatement = ParseStatement();
        }
        
        return new IfStatement(condition, thenStatement, elseStatement, token);
    }

    /// <summary>
    /// Parse a try statement: try { ... } catch (e) { ... } finally { ... }
    /// </summary>
    private Statement ParseTryStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Try, "Expected 'try'");
        
        var tryBlock = ParseBlockStatement();
        
        CatchClause? catchClause = null;
        BlockStatement? finallyBlock = null;
        
        if (_currentToken.Type == TokenType.Catch)
        {
            Advance(); // consume 'catch'
            
            Identifier? param = null;
            if (_currentToken.Type == TokenType.LeftParen)
            {
                Advance(); // consume '('
                param = new Identifier(Consume(TokenType.Identifier, "Expected identifier in catch clause").Value);
                Consume(TokenType.RightParen, "Expected ')' after catch parameter");
            }
            
            var catchBody = ParseBlockStatement();
            catchClause = new CatchClause(catchBody, param);
        }
        
        if (_currentToken.Type == TokenType.Finally)
        {
            Advance(); // consume 'finally'
            finallyBlock = ParseBlockStatement();
        }
        
        if (catchClause == null && finallyBlock == null)
        {
            throw new ECEngineException("Try statement must have either a catch or finally block",
                token.Line, token.Column, _sourceCode, "Try statement requires catch or finally");
        }
        
        return new TryStatement(tryBlock, catchClause, finallyBlock, token);
    }

    /// <summary>
    /// Parse a throw statement: throw expression;
    /// </summary>
    private Statement ParseThrowStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Throw, "Expected 'throw'");
        
        var argument = ParseExpression();
        
        Match(TokenType.Semicolon); // Optional semicolon
        
        return new ThrowStatement(argument, token);
    }
}
