using ECEngine.AST;
using ECEngine.Lexer;
using ECEngine.Runtime;

namespace ECEngine.Parser;

/// <summary>
/// Loop and control flow parsing methods for ECEngine parser (for, while, do-while, switch, break, continue)
/// </summary>
public partial class Parser
{
    /// <summary>
    /// Parse a for statement: for (init; condition; update) body
    /// </summary>
    private ForStatement ParseForStatement()
    {
        var token = _currentToken;
        Consume(TokenType.For, "Expected 'for' keyword");
        Consume(TokenType.LeftParen, "Expected '(' after 'for'");
        
        // Parse initialization (can be variable declaration or expression)
        Statement? init = null;
        if (_currentToken.Type != TokenType.Semicolon)
        {
            if (_currentToken.Type == TokenType.Var || _currentToken.Type == TokenType.Let || _currentToken.Type == TokenType.Const)
            {
                init = ParseVariableDeclaration();
            }
            else
            {
                var expr = ParseExpression();
                Consume(TokenType.Semicolon, "Expected ';' after for loop initialization");
                init = new ExpressionStatement(expr);
            }
        }
        else
        {
            Consume(TokenType.Semicolon, "Expected ';' after for loop initialization");
        }
        
        // Parse condition
        Expression? condition = null;
        if (_currentToken.Type != TokenType.Semicolon)
        {
            condition = ParseExpression();
        }
        Consume(TokenType.Semicolon, "Expected ';' after for loop condition");
        
        // Parse update expression
        Expression? update = null;
        if (_currentToken.Type != TokenType.RightParen)
        {
            update = ParseExpression();
        }
        Consume(TokenType.RightParen, "Expected ')' after for loop header");
        
        // Parse body
        var body = ParseStatement();
        
        return new ForStatement(init, condition, update, body, token);
    }
    
    /// <summary>
    /// Parse a while statement: while (condition) body
    /// </summary>
    private WhileStatement ParseWhileStatement()
    {
        var token = _currentToken;
        Consume(TokenType.While, "Expected 'while' keyword");
        Consume(TokenType.LeftParen, "Expected '(' after 'while'");
        var condition = ParseExpression();
        Consume(TokenType.RightParen, "Expected ')' after while condition");
        var body = ParseStatement();
        
        return new WhileStatement(condition, body, token);
    }
    
    /// <summary>
    /// Parse a do-while statement: do body while (condition);
    /// </summary>
    private DoWhileStatement ParseDoWhileStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Do, "Expected 'do' keyword");
        var body = ParseStatement();
        Consume(TokenType.While, "Expected 'while' after do body");
        Consume(TokenType.LeftParen, "Expected '(' after 'while'");
        var condition = ParseExpression();
        Consume(TokenType.RightParen, "Expected ')' after while condition");
        Consume(TokenType.Semicolon, "Expected ';' after do-while statement");
        
        return new DoWhileStatement(body, condition, token);
    }
    
    /// <summary>
    /// Parse a break statement: break;
    /// </summary>
    private BreakStatement ParseBreakStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Break, "Expected 'break' keyword");
        Consume(TokenType.Semicolon, "Expected ';' after break statement");
        return new BreakStatement(token);
    }
    
    /// <summary>
    /// Parse a continue statement: continue;
    /// </summary>
    private ContinueStatement ParseContinueStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Continue, "Expected 'continue' keyword");
        Consume(TokenType.Semicolon, "Expected ';' after continue statement");
        return new ContinueStatement(token);
    }

    /// <summary>
    /// Parse a switch statement: switch (expr) { case value: ... default: ... }
    /// </summary>
    private SwitchStatement ParseSwitchStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Switch, "Expected 'switch'");
        Consume(TokenType.LeftParen, "Expected '(' after 'switch'");
        var discriminant = ParseExpression();
        Consume(TokenType.RightParen, "Expected ')' after switch discriminant");
        Consume(TokenType.LeftBrace, "Expected '{' after switch condition");
        
        var cases = new List<SwitchCase>();
        
        while (_currentToken.Type != TokenType.RightBrace && _currentToken.Type != TokenType.EOF)
        {
            if (_currentToken.Type == TokenType.Case)
            {
                Advance(); // consume 'case'
                var test = ParseExpression();
                Consume(TokenType.Colon, "Expected ':' after case value");
                
                var consequent = new List<Statement>();
                while (_currentToken.Type != TokenType.Case && 
                       _currentToken.Type != TokenType.Default && 
                       _currentToken.Type != TokenType.RightBrace && 
                       _currentToken.Type != TokenType.EOF)
                {
                    consequent.Add(ParseStatement());
                }
                
                cases.Add(new SwitchCase(test, consequent));
            }
            else if (_currentToken.Type == TokenType.Default)
            {
                Advance(); // consume 'default'
                Consume(TokenType.Colon, "Expected ':' after 'default'");
                
                var consequent = new List<Statement>();
                while (_currentToken.Type != TokenType.Case && 
                       _currentToken.Type != TokenType.Default && 
                       _currentToken.Type != TokenType.RightBrace && 
                       _currentToken.Type != TokenType.EOF)
                {
                    consequent.Add(ParseStatement());
                }
                
                cases.Add(new SwitchCase(null, consequent)); // null test indicates default case
            }
            else
            {
                throw new ECEngineException($"Expected 'case' or 'default' in switch statement, got {_currentToken.Type}",
                    _currentToken.Line, _currentToken.Column, _sourceCode, "Switch statement syntax error");
            }
        }
        
        Consume(TokenType.RightBrace, "Expected '}' to close switch statement");
        
        return new SwitchStatement(discriminant, cases, token);
    }
}
