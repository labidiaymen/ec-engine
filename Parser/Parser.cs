using ECEngine.AST;
using ECEngine.Lexer;
using ECEngine.Runtime;

namespace ECEngine.Parser;

// Parser for ECEngine language
public partial class Parser
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

    private void RestorePosition(int position, int line, int column)
    {
        _position = position;
        _currentToken = _position < _tokens.Count ? _tokens[_position] : _tokens[^1];
    }

    private bool IsKeywordToken(TokenType tokenType)
    {
        return tokenType == TokenType.Function ||
               tokenType == TokenType.Var ||
               tokenType == TokenType.Let ||
               tokenType == TokenType.Const ||
               tokenType == TokenType.Return ||
               tokenType == TokenType.If ||
               tokenType == TokenType.Else ||
               tokenType == TokenType.For ||
               tokenType == TokenType.While ||
               tokenType == TokenType.Do ||
               tokenType == TokenType.Break ||
               tokenType == TokenType.Continue ||
               tokenType == TokenType.Switch ||
               tokenType == TokenType.Case ||
               tokenType == TokenType.Default ||
               tokenType == TokenType.Try ||
               tokenType == TokenType.Catch ||
               tokenType == TokenType.Finally ||
               tokenType == TokenType.Throw ||
               tokenType == TokenType.New ||
               tokenType == TokenType.Typeof ||
               tokenType == TokenType.Instanceof ||
               tokenType == TokenType.True ||
               tokenType == TokenType.False ||
               tokenType == TokenType.Null ||
               tokenType == TokenType.This ||
               tokenType == TokenType.Export ||
               tokenType == TokenType.Import ||
               tokenType == TokenType.From ||
               tokenType == TokenType.In ||
               tokenType == TokenType.Of ||
               tokenType == TokenType.As ||
               tokenType == TokenType.Yield;
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

    // Parse an expression from a list of tokens (for template literal interpolation)
    public Expression ParseExpressionFromTokens(List<Token> tokens, string sourceCode)
    {
        _sourceCode = sourceCode;
        _tokens = tokens;
        _position = 0;
        _currentToken = _tokens.Count > 0 ? _tokens[0] : new Token(TokenType.EOF, "", 0);
        
        return ParseExpression();
    }

    private Statement ParseStatement()
    {
        // Check for export statements
        if (_currentToken.Type == TokenType.Export)
        {
            return ParseExportStatement();
        }
        
        // Check for import statements
        if (_currentToken.Type == TokenType.Import)
        {
            return ParseImportStatement();
        }
        
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
        
        // Check for yield statements
        if (_currentToken.Type == TokenType.Yield)
        {
            return ParseYieldStatement();
        }
        
        // Check for if statements
        if (_currentToken.Type == TokenType.If)
        {
            return ParseIfStatement();
        }
        
        // Check for for loops
        if (_currentToken.Type == TokenType.For)
        {
            return ParseForStatement();
        }
        
        // Check for while loops
        if (_currentToken.Type == TokenType.While)
        {
            return ParseWhileStatement();
        }
        
        // Check for do-while loops
        if (_currentToken.Type == TokenType.Do)
        {
            return ParseDoWhileStatement();
        }
        
        // Check for break statements
        if (_currentToken.Type == TokenType.Break)
        {
            return ParseBreakStatement();
        }
        
        // Check for continue statements
        if (_currentToken.Type == TokenType.Continue)
        {
            return ParseContinueStatement();
        }
        
        // Check for switch statements
        if (_currentToken.Type == TokenType.Switch)
        {
            return ParseSwitchStatement();
        }
        
        // Check for try statements
        if (_currentToken.Type == TokenType.Try)
        {
            return ParseTryStatement();
        }
        
        // Check for throw statements
        if (_currentToken.Type == TokenType.Throw)
        {
            return ParseThrowStatement();
        }
        
        // Check for observe statements
        if (_currentToken.Type == TokenType.Observe)
        {
            return ParseObserveStatement();
        }
        
        // Check for when statements
        if (_currentToken.Type == TokenType.When)
        {
            return ParseWhenStatement();
        }
        
        // Check for otherwise statements
        if (_currentToken.Type == TokenType.Otherwise)
        {
            return ParseOtherwiseStatement();
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



    private ReturnStatement ParseReturnStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Return, "Expected 'return' keyword");
        
        Expression? argument = null;
        if (_currentToken.Type != TokenType.Semicolon && 
            _currentToken.Type != TokenType.EOF &&
            _currentToken.Type != TokenType.RightBrace)
        {
            argument = ParseExpression();
        }
        
        Match(TokenType.Semicolon); // Optional semicolon
        
        return new ReturnStatement(argument, token);
    }

    private YieldStatement ParseYieldStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Yield, "Expected 'yield' keyword");
        
        Expression? argument = null;
        if (_currentToken.Type != TokenType.Semicolon && _currentToken.Type != TokenType.EOF)
        {
            argument = ParseExpression();
        }
        
        Match(TokenType.Semicolon); // Optional semicolon
        
        return new YieldStatement(argument, token);
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
