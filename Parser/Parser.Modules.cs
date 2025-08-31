using ECEngine.AST;
using ECEngine.Lexer;
using ECEngine.Runtime;

namespace ECEngine.Parser;

/// <summary>
/// Module system parsing methods for ECEngine parser (import/export statements)
/// </summary>
public partial class Parser
{
    /// <summary>
    /// Parse an export statement: export var x = 5; or export function foo() {} or export default ... or export { ... }
    /// </summary>
    private Statement ParseExportStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Export, "Expected 'export'");
        
        // Check for default export
        if (_currentToken.Type == TokenType.Default)
        {
            return ParseDefaultExportStatement(token);
        }
        
        // Check for named export
        if (_currentToken.Type == TokenType.LeftBrace)
        {
            return ParseNamedExportStatement(token);
        }
        
        // Regular export with declaration
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
            throw new ECEngineException("Expected variable declaration, function declaration, 'default', or '{' after 'export'",
                _currentToken.Line, _currentToken.Column, _sourceCode,
                "Export statements must be followed by a declaration, 'default', or named exports");
        }
        
        return new ExportStatement(declaration, token);
    }

    /// <summary>
    /// Parse a default export statement: export default function() {} or export default expression;
    /// </summary>
    private DefaultExportStatement ParseDefaultExportStatement(Token exportToken)
    {
        Consume(TokenType.Default, "Expected 'default'");
        
        ASTNode value;
        
        // Check if it's a function declaration
        if (_currentToken.Type == TokenType.Function)
        {
            // For default exports, we can have anonymous functions
            value = ParseDefaultExportFunction();
        }
        else
        {
            // Parse expression and consume semicolon
            value = ParseExpression();
            Match(TokenType.Semicolon); // Optional semicolon
        }
        
        return new DefaultExportStatement(value, exportToken);
    }

    /// <summary>
    /// Parse named export statement: export { name1, name2 as alias } [from "module"];
    /// </summary>
    private Statement ParseNamedExportStatement(Token exportToken)
    {
        Consume(TokenType.LeftBrace, "Expected '{'");
        
        var exportedNames = new List<NamedExport>();
        
        if (_currentToken.Type != TokenType.RightBrace)
        {
            do
            {
                var localName = Consume(TokenType.Identifier, "Expected identifier in export list").Value;
                string? exportName = null;
                
                // Check for 'as' renaming
                if (_currentToken.Type == TokenType.As)
                {
                    Advance(); // consume 'as'
                    exportName = Consume(TokenType.Identifier, "Expected identifier after 'as'").Value;
                }
                
                exportedNames.Add(new NamedExport(localName, exportName));
                
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
        
        Consume(TokenType.RightBrace, "Expected '}' after export list");
        
        // Check for 'from' clause (re-export)
        if (_currentToken.Type == TokenType.From)
        {
            Advance(); // consume 'from'
            var modulePath = Consume(TokenType.String, "Expected string literal for module path").Value;
            Match(TokenType.Semicolon); // Optional semicolon
            
            return new ReExportStatement(exportedNames, modulePath, exportToken);
        }
        else
        {
            // Regular named export
            Match(TokenType.Semicolon); // Optional semicolon
            return new NamedExportStatement(exportedNames, exportToken);
        }
    }

    /// <summary>
    /// Parse an import statement: 
    /// import { name1, name2 } from "module"; (named imports)
    /// import name from "module"; (default import)
    /// </summary>
    private ImportStatement ParseImportStatement()
    {
        var token = _currentToken;
        Consume(TokenType.Import, "Expected 'import'");
        
        var importedNames = new List<string>();
        
        // Check if this is a default import (import name from "module")
        if (_currentToken.Type == TokenType.Identifier)
        {
            // Default import: import name from "module"
            var defaultName = Consume(TokenType.Identifier, "Expected identifier for default import").Value;
            importedNames.Add("default"); // Store as "default" internally
            
            Consume(TokenType.From, "Expected 'from' after default import");
            var modulePath = Consume(TokenType.String, "Expected string literal for module path").Value;
            Match(TokenType.Semicolon); // Optional semicolon
            
            return new ImportStatement(importedNames, modulePath, token)
            {
                DefaultImportName = defaultName // Store the actual variable name
            };
        }
        else
        {
            // Named imports: import { name1, name2 } from "module"
            Consume(TokenType.LeftBrace, "Expected '{' after 'import'");
            
            if (_currentToken.Type != TokenType.RightBrace)
            {
                do
                {
                    string name;
                    if (_currentToken.Type == TokenType.Identifier)
                    {
                        name = Consume(TokenType.Identifier, "Expected identifier in import list").Value;
                    }
                    else if (_currentToken.Type == TokenType.Default)
                    {
                        name = "default";
                        Advance(); // consume the 'default' token
                    }
                    else
                    {
                        throw new ECEngineException("Expected identifier or 'default' in import list",
                            _currentToken.Line, _currentToken.Column, _sourceCode,
                            "Import lists can only contain identifiers or 'default'");
                    }
                    
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
    }

    /// <summary>
    /// Parse function for default export - can be anonymous
    /// </summary>
    private FunctionDeclaration ParseDefaultExportFunction()
    {
        var token = _currentToken;
        Consume(TokenType.Function, "Expected 'function' keyword");
        
        string name;
        // Check if there's a name after 'function' - if not, it's anonymous
        if (_currentToken.Type == TokenType.Identifier)
        {
            name = Consume(TokenType.Identifier, "Expected function name").Value;
        }
        else
        {
            name = null; // Anonymous function
        }
        
        Consume(TokenType.LeftParen, "Expected '(' after function");
        
        var parameters = new List<string>();
        if (_currentToken.Type != TokenType.RightParen)
        {
            parameters.Add(Consume(TokenType.Identifier, "Expected parameter name").Value);
            
            while (Match(TokenType.Comma))
            {
                parameters.Add(Consume(TokenType.Identifier, "Expected parameter name").Value);
            }
        }
        
        Consume(TokenType.RightParen, "Expected ')' after parameters");
        
        var body = ParseBlockStatement().Body;
        
        return new FunctionDeclaration(name, parameters, body, token);
    }
}
