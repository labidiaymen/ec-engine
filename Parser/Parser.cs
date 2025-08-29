using ECEngine.AST;
using ECEngine.Lexer;
using ECEngine.Runtime;

namespace ECEngine.Parser;

// Parser for JavaScript code
public class Parser
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

    private Expression ParseExpression()
    {
        return ParseAssignment();
    }

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

    private Expression ParseAdditive()
    {
        var left = ParseMultiplicative();

        while (_currentToken.Type == TokenType.Plus || _currentToken.Type == TokenType.Minus)
        {
            var op = _currentToken.Value;
            Advance();
            var right = ParseMultiplicative();
            left = new BinaryExpression(left, op, right);
        }

        return left;
    }

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

    private ReturnStatement ParseReturnStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Return, "Expected 'return' keyword");
        
        Expression? argument = null;
        if (_currentToken.Type != TokenType.Semicolon && _currentToken.Type != TokenType.EOF)
        {
            argument = ParseExpression();
        }
        
        Match(TokenType.Semicolon); // Optional semicolon
        
        return new ReturnStatement(argument, token);
    }

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
    
    private BreakStatement ParseBreakStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Break, "Expected 'break' keyword");
        Consume(TokenType.Semicolon, "Expected ';' after break statement");
        return new BreakStatement(token);
    }
    
    private ContinueStatement ParseContinueStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Continue, "Expected 'continue' keyword");
        Consume(TokenType.Semicolon, "Expected ';' after continue statement");
        return new ContinueStatement(token);
    }

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
        
        var handler = new FunctionExpression(parameters, body, token);
        
        Match(TokenType.Semicolon); // Optional semicolon
        
        return new ObserveStatement(variableName, handler, token);
    }

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
        
        var handler = new FunctionExpression(parameters, body, token);
        
        Match(TokenType.Semicolon); // Optional semicolon
        
        return new MultiObserveStatement(variableNames, handler, token);
    }

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

    private ExportStatement ParseExportStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Export, "Expected 'export'");
        
        // Parse the declaration that follows export
        Statement declaration;
        
        if (_currentToken.Type == TokenType.Var || _currentToken.Type == TokenType.Let || _currentToken.Type == TokenType.Const)
        {
            declaration = ParseVariableDeclaration();
        }
        else if (_currentToken.Type == TokenType.Function)
        {
            declaration = ParseFunctionDeclaration();
        }
        else
        {
            throw new ECEngineException("Expected variable declaration or function declaration after 'export'",
                _currentToken.Line, _currentToken.Column, _sourceCode,
                "Export statements must be followed by a declaration");
        }
        
        return new ExportStatement(declaration, token);
    }

    private ImportStatement ParseImportStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Import, "Expected 'import'");
        
        // Parse import list: { name1, name2, ... }
        var importedNames = new List<string>();
        
        Consume(TokenType.LeftBrace, "Expected '{' after 'import'");
        
        if (_currentToken.Type != TokenType.RightBrace)
        {
            do
            {
                var name = Consume(TokenType.Identifier, "Expected identifier in import list").Value;
                importedNames.Add(name);
                
                if (_currentToken.Type == TokenType.Comma)
                {
                    Advance();
                }
                else
                {
                    break;
                }
            } while (_currentToken.Type != TokenType.RightBrace && _currentToken.Type != TokenType.EOF);
        }
        
        Consume(TokenType.RightBrace, "Expected '}' after import list");
        Consume(TokenType.From, "Expected 'from' after import list");
        
        var modulePath = Consume(TokenType.String, "Expected string literal for module path").Value;
        
        Match(TokenType.Semicolon); // Optional semicolon
        
        return new ImportStatement(importedNames, modulePath, token);
    }

    // Parse switch statement: switch (expr) { case value: ... default: ... }
    private Statement ParseSwitchStatement()
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

    // Parse try statement: try { ... } catch (e) { ... } finally { ... }
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

    // Parse throw statement: throw expression;
    private Statement ParseThrowStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Throw, "Expected 'throw'");
        
        var argument = ParseExpression();
        
        Match(TokenType.Semicolon); // Optional semicolon
        
        return new ThrowStatement(argument, token);
    }
}
