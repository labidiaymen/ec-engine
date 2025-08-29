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
    /// Parse an export statement: export var x = 5; or export function foo() {}
    /// </summary>
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

    /// <summary>
    /// Parse an import statement: import { name1, name2 } from "module";
    /// </summary>
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
}
