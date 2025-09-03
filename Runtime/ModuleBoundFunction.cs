using System;
using System.Collections.Generic;

namespace ECEngine.Runtime;

/// <summary>
/// A wrapper for functions exported from modules that preserves their original execution context
/// </summary>
public class ModuleBoundFunction
{
    private readonly Function _originalFunction;
    private readonly Interpreter _moduleInterpreter;
    
    public ModuleBoundFunction(Function originalFunction, Interpreter moduleInterpreter)
    {
        _originalFunction = originalFunction;
        _moduleInterpreter = moduleInterpreter;
    }
    
    /// <summary>
    /// Execute the function in its original module context
    /// </summary>
    public object? Call(List<object?> arguments, object? thisContext = null)
    {
        // The issue is that CallUserFunctionPublic isolates the function scope
        // We need to temporarily add module variables to the function's closure
        var originalClosure = new Dictionary<string, VariableInfo>(_originalFunction.Closure);
        
        // Add all module variables to the function's closure temporarily
        foreach (var moduleVar in _moduleInterpreter.Variables)
        {
            _originalFunction.Closure[moduleVar.Key] = moduleVar.Value;
        }
        
        try
        {
            // Now call the function with the enhanced closure
            return _moduleInterpreter.CallUserFunctionPublic(_originalFunction, arguments, thisContext);
        }
        finally
        {
            // Restore original closure
            _originalFunction.Closure.Clear();
            foreach (var item in originalClosure)
            {
                _originalFunction.Closure[item.Key] = item.Value;
            }
        }
    }
    
    /// <summary>
    /// Get the original function for inspection
    /// </summary>
    public Function GetOriginalFunction()
    {
        return _originalFunction;
    }
    
    /// <summary>
    /// Get the module interpreter for debugging
    /// </summary>
    public Interpreter GetModuleInterpreter()
    {
        return _moduleInterpreter;
    }
    
    /// <summary>
    /// Get a property from the underlying function (for JavaScript function.property access)
    /// </summary>
    public object? GetProperty(string propertyName)
    {
        return _originalFunction.Properties.TryGetValue(propertyName, out var value) ? value : null;
    }
    
    /// <summary>
    /// Set a property on the underlying function (for JavaScript function.property = value)
    /// </summary>
    public void SetProperty(string propertyName, object? value)
    {
        _originalFunction.Properties[propertyName] = value;
    }
}
