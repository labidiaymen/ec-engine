using ECEngine.AST;
using ECEngine.Lexer;
using ECEngine.Runtime.Streams;
using System.Reflection;

namespace ECEngine.Runtime;

/// <summary>
/// Variable management, scope handling, and variable declaration/assignment for ECEngine interpreter
/// </summary>
public partial class Interpreter
{
    /// <summary>
    /// Declare a new variable in the current scope
    /// </summary>
    public void DeclareVariable(string kind, string name, object? value)
    {
        var currentScope = _scopes.Peek();
        
        // Check if variable already declared in current scope
        if (currentScope.ContainsKey(name))
        {
            // ECEngine is strict: no redeclaration allowed for any variable type
            throw new ECEngineException($"Variable '{name}' already declared", 
                1, 1, _sourceCode, "Variable redeclaration error");
        }
        
        // Declare variable in current scope
        currentScope[name] = new VariableInfo(kind, value);
        SyncVariables(); // Update backwards compatibility variables
    }
    
    /// <summary>
    /// Set value of an existing variable (assignment)
    /// </summary>
    public void SetVariable(string name, object? value)
    {
        var variableInfo = FindVariable(name);
        if (variableInfo == null)
        {
            // Strict mode behavior: assignment to undeclared variable throws error
            throw new ECEngineException($"Variable '{name}' not declared", 
                1, 1, _sourceCode, "Undeclared variable assignment");
        }
        
        if (variableInfo.Type == "const")
        {
            throw new ECEngineException($"Cannot assign to const variable '{name}'", 
                1, 1, _sourceCode, "Const assignment error");
        }
        
        var oldValue = variableInfo.Value;
        variableInfo.Value = value;
        
        // Trigger observers if value changed
        if (!Equals(oldValue, value))
        {
            TriggerObservers(name, variableInfo, oldValue, value);
        }
        
        SyncVariables(); // Update backwards compatibility variables
    }

    /// <summary>
    /// Trigger observers when a variable value changes
    /// </summary>
    private void TriggerObservers(string variableName, VariableInfo variableInfo, object? oldValue, object? newValue)
    {
        // Call each observer function with old and new values
        foreach (var observer in variableInfo.Observers)
        {
            try
            {
                // Call observer function with (oldValue, newValue) parameters
                CallUserFunction(observer, new object?[] { oldValue, newValue });
            }
            catch (Exception ex)
            {
                // Don't let observer errors break variable assignment
                // In a production system, you might want to log this
                Console.WriteLine($"Observer error for variable '{variableName}': {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Get value of a variable
    /// </summary>
    public object? GetVariable(string name)
    {
        var variableInfo = FindVariable(name);
        return variableInfo?.Value;
    }
    
    /// <summary>
    /// Find variable in scope chain (from current scope up to global)
    /// </summary>
    private VariableInfo? FindVariable(string name)
    {
        // Search from current scope up to global scope
        foreach (var scope in _scopes)
        {
            if (scope.ContainsKey(name))
            {
                return scope[name];
            }
        }
        return null;
    }
    
    /// <summary>
    /// Check if a variable exists in any scope
    /// </summary>
    public bool HasVariable(string name)
    {
        return FindVariable(name) != null;
    }
    
    /// <summary>
    /// Synchronize the flattened variables dictionary with current scopes (for backwards compatibility)
    /// </summary>
    private void SyncVariables()
    {
        _variables.Clear();
        
        // Build flattened view from bottom to top (global to current)
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!_variables.ContainsKey(kvp.Key))
                {
                    _variables[kvp.Key] = kvp.Value;
                }
            }
        }
    }
    
    /// <summary>
    /// Push a new scope onto the stack (public for Generator support)
    /// </summary>
    public void PushScope()
    {
        _scopes.Push(new Dictionary<string, VariableInfo>());
    }
    
    /// <summary>
    /// Pop the current scope from the stack (public for Generator support)
    /// </summary>
    public void PopScope()
    {
        if (_scopes.Count > 1) // Always keep at least global scope
        {
            _scopes.Pop();
            SyncVariables(); // Update backwards compatibility variables
        }
    }
    
    /// <summary>
    /// Get the current scope depth
    /// </summary>
    public int ScopeDepth => _scopes.Count;
    
    /// <summary>
    /// Reset interpreter to global scope only
    /// </summary>
    public void ResetToGlobalScope()
    {
        // Clear all scopes except global
        while (_scopes.Count > 1)
        {
            _scopes.Pop();
        }
        
        // Clear global scope
        _scopes.Peek().Clear();
        
        // Reset to fresh global scope
        _scopes.Push(new Dictionary<string, VariableInfo>()); // Reset to global scope
        SyncVariables();
    }
    
    /// <summary>
    /// Evaluate a variable declaration statement
    /// </summary>
    private object? EvaluateVariableDeclaration(VariableDeclaration varDecl)
    {
        var value = varDecl.Initializer != null ? Evaluate(varDecl.Initializer, _sourceCode) : null;
        
        // Handle default values for different variable types
        if (value == null)
        {
            value = varDecl.Kind switch
            {
                "const" => throw new ECEngineException($"Missing initializer in const declaration '{varDecl.Name}'",
                    varDecl.Token?.Line ?? 1, varDecl.Token?.Column ?? 1, _sourceCode,
                    "Const variables must be initialized"),
                _ => null // let and var can be undefined initially
            };
        }
        
        DeclareVariable(varDecl.Kind, varDecl.Name, value);
        return value;
    }
    
    /// <summary>
    /// Evaluate an identifier (variable access)
    /// </summary>
    private object? EvaluateIdentifier(Identifier identifier)
    {
        var name = identifier.Name;
        
        // Check for built-in global objects first
        switch (name)
        {
            case "undefined":
                return null; // JavaScript undefined is represented as C# null
            case "console":
                return new ConsoleObject();
            case "process":
                return ProcessGlobals.CreateProcessObject(this);
            case "__dirname":
                if (_currentFilePath != null)
                {
                    return Path.GetDirectoryName(_currentFilePath) ?? "/";
                }
                return Environment.CurrentDirectory;
            case "__filename":
                return _currentFilePath ?? "";
            case "setTimeout":
                return new SetTimeoutFunction(_eventLoop, this);
            case "setInterval":
                return new SetIntervalFunction(_eventLoop, this);
            case "clearTimeout":
                return new ClearTimeoutFunction(_eventLoop);
            case "clearInterval":
                return new ClearIntervalFunction(_eventLoop);
            case "require":
                return new RequireFunction(_moduleSystem, this);
            case "exports":
                return _exports;
            case "module":
                var moduleObj = new Dictionary<string, object?>
                {
                    ["exports"] = _exports
                };
                return moduleObj;
            case "URL":
                return new UrlConstructorFunction();
            case "URLSearchParams":
                return new URLSearchParamsConstructorFunction();
            case "Object":
                return new ObjectModuleClass(); // Return Object module with methods
            case "Array":
                return new ArrayModuleClass(); // Return Array module with static methods
            case "String":
                return new StringModule();
            case "Number":
                return new NumberModule();
            case "Boolean":
                return "Boolean"; // String identifier for constructor
            case "Error":
                return new ErrorModule();
            case "TypeError":
                return new ErrorModule(); // For now, use the same ErrorModule
            case "ReferenceError":
                return new ErrorModule(); // For now, use the same ErrorModule
            case "SyntaxError":
                return new ErrorModule(); // For now, use the same ErrorModule
            case "RangeError":
                return new ErrorModule(); // For now, use the same ErrorModule
            case "Date":
                return new DateModule();
            case "querystring":
                return new QuerystringModule();
            case "Math":
                return new MathModule();
            case "JSON":
                return new JsonModule();
            case "stream":
                var streamModule = new Dictionary<string, object?>();
                
                // Add stream constructors to the module
                streamModule["Readable"] = new ReadableStreamConstructor();
                streamModule["Writable"] = new WritableStreamConstructor();
                streamModule["Duplex"] = new DuplexStreamConstructor();
                streamModule["Transform"] = new TransformStreamConstructor();
                streamModule["PassThrough"] = new PassThroughStreamConstructor();
                
                // Add stream utilities
                streamModule["pipeline"] = new StreamPipelineFunction();
                streamModule["finished"] = new StreamFinishedFunction();
                streamModule["compose"] = new StreamComposeFunction();
                streamModule["isReadable"] = new StreamIsReadableFunction();
                streamModule["isWritable"] = new StreamIsWritableFunction();
                
                return streamModule;
            case "Readable":
                return new ReadableStreamConstructor();
            case "Writable":
                return new WritableStreamConstructor();
            case "Duplex":
                return new DuplexStreamConstructor();
            case "Transform":
                return new TransformStreamConstructor();
            case "PassThrough":
                return new PassThroughStreamConstructor();
        }
        
        // Look up in variable scopes
        var variableInfo = FindVariable(name);
        if (variableInfo != null)
        {
            return variableInfo.Value;
        }
        
        // Variable not found
        throw new ECEngineException($"Unknown identifier: {name}", 
            identifier.Token?.Line ?? 1, identifier.Token?.Column ?? 1, _sourceCode,
            $"Variable '{name}' is not defined in the current scope");
    }
}
