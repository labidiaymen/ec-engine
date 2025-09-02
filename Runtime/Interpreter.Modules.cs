using ECEngine.AST;
using ECEngine.Runtime;

namespace ECEngine.Runtime;

public partial class Interpreter
{
    /// <summary>
    /// Evaluate import statements
    /// </summary>
    private object? EvaluateImportStatement(ImportStatement importStmt)
    {
        if (_moduleSystem == null)
        {
            var token = importStmt.Token;
            throw new ECEngineException("Module system not available",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                "Import statements require a module system to be configured");
        }
        
        try
        {
            // Load the module
            var module = _moduleSystem.LoadModule(importStmt.ModulePath, this);
            
            // Check if module loaded successfully
            if (module == null)
            {
                var token = importStmt.Token;
                throw new ECEngineException($"Failed to load module '{importStmt.ModulePath}'",
                    token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                    $"Module '{importStmt.ModulePath}' could not be loaded or resolved");
            }
            
            // Handle namespace import (import * as name from "module")
            if (importStmt.IsNamespaceImport && importStmt.NamespaceImportName != null)
            {
                // Create a namespace object containing all exports
                var namespaceObject = new Dictionary<string, object?>(module.Exports);
                DeclareVariable("const", importStmt.NamespaceImportName, namespaceObject);
                return null;
            }
            
            // Handle default import (import name from "module")
            if (importStmt.DefaultImportName != null)
            {
                // For default imports, look for "default" export or use the entire module if CommonJS
                object? defaultValue = null;
                
                if (module.Exports.ContainsKey("default"))
                {
                    defaultValue = module.Exports["default"];
                }
                else if (module.Exports.Count == 1)
                {
                    // If there's only one export, use it as the default (common in CommonJS)
                    defaultValue = module.Exports.Values.First();
                }
                else
                {
                    // If no default and multiple exports, create an object with all exports
                    defaultValue = module.Exports;
                }
                
                if (defaultValue != null)
                {
                    DeclareVariable("const", importStmt.DefaultImportName, defaultValue);
                }
            }
            
            // Handle named imports (import { name1, name2 as alias } from "module")
            if (importStmt.ImportedNames.Count > 0)
            {
                foreach (var name in importStmt.ImportedNames)
                {
                    // Skip default if already handled above
                    if (name == "default" && importStmt.DefaultImportName != null)
                        continue;
                        
                    if (module.Exports.ContainsKey(name))
                    {
                        var value = module.Exports[name];
                        
                        // Check if this import has an alias
                        var importName = importStmt.ImportAliases.ContainsKey(name) 
                            ? importStmt.ImportAliases[name] 
                            : name;
                        
                        // Add to current scope using DeclareVariable
                        DeclareVariable("const", importName, value);
                    }
                    else
                    {
                        var token = importStmt.Token;
                        throw new ECEngineException($"'{name}' is not exported by module '{importStmt.ModulePath}'",
                            token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                            $"Module '{importStmt.ModulePath}' does not export '{name}'");
                    }
                }
            }
            
            return null;
        }
        catch (ECEngineException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var token = importStmt.Token;
            throw new ECEngineException($"Failed to import module '{importStmt.ModulePath}': {ex.Message}",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                ex.Message);
        }
    }

    /// <summary>
    /// Evaluate export statements
    /// </summary>
    private object? EvaluateExportStatement(ExportStatement exportStmt)
    {
        // Evaluate the declaration first
        var result = Evaluate(exportStmt.Declaration, _sourceCode);
        
        // Extract the exported name and value
        if (exportStmt.Declaration is VariableDeclaration varDecl)
        {
            var variable = FindVariable(varDecl.Name);
            if (variable != null)
            {
                _exports[varDecl.Name] = variable.Value;
            }
        }
        else if (exportStmt.Declaration is FunctionDeclaration funcDecl)
        {
            if (funcDecl.Name != null)
            {
                var variable = FindVariable(funcDecl.Name);
                if (variable != null)
                {
                    _exports[funcDecl.Name] = variable.Value;
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// Evaluate named export statements
    /// </summary>
    private object? EvaluateNamedExportStatement(NamedExportStatement namedExportStmt)
    {
        foreach (var export in namedExportStmt.ExportedNames)
        {
            var localName = export.LocalName;
            var exportName = export.ExportName ?? localName;
            
            // Check if the variable exists
            var variable = FindVariable(localName);
            if (variable == null)
            {
                var token = namedExportStmt.Token;
                throw new ECEngineException($"Cannot export '{localName}': variable not found",
                    token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                    $"Variable '{localName}' must be declared before it can be exported");
            }
            
            // Export the variable with the specified name
            _exports[exportName] = variable.Value;
        }
        
        return null;
    }

    /// <summary>
    /// Evaluate default export statements
    /// </summary>
    private object? EvaluateDefaultExportStatement(DefaultExportStatement defaultExportStmt)
    {
        // Evaluate the value to be exported
        var result = Evaluate(defaultExportStmt.Value, _sourceCode);
        
        // Store as default export
        _exports["default"] = result;
        
        return result;
    }
    
    /// <summary>
    /// Evaluate dynamic import expressions (import("module"))
    /// </summary>
    private object? EvaluateDynamicImportExpression(DynamicImportExpression dynamicImport)
    {
        if (_moduleSystem == null)
        {
            var token = dynamicImport.Token;
            throw new ECEngineException("Module system not available",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                "Dynamic import expressions require a module system to be configured");
        }
        
        try
        {
            // Evaluate the module path expression to get the actual path
            var modulePathValue = Evaluate(dynamicImport.ModulePath, _sourceCode);
            var modulePath = ToString(modulePathValue);
            
            // Load the module
            var module = _moduleSystem.LoadModule(modulePath, this);
            
            // Check if module loaded successfully
            if (module == null)
            {
                var token = dynamicImport.Token;
                throw new ECEngineException($"Failed to load module '{modulePath}'",
                    token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                    $"Module '{modulePath}' could not be loaded or resolved");
            }
            
            // For dynamic imports, return the module's exports as an object
            // This matches JavaScript's behavior where import() returns a promise that resolves to the module namespace
            return new Dictionary<string, object?>(module.Exports);
        }
        catch (Exception ex)
        {
            var token = dynamicImport.Token;
            throw new ECEngineException($"Dynamic import failed: {ex.Message}",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                $"Error during dynamic import: {ex.Message}");
        }
    }
}
