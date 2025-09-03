using System;
using System.Collections.Generic;

namespace ECEngine.Runtime;

/// <summary>
/// Error constructor and related functions for ECEngine scripts
/// </summary>
public class ErrorConstructor
{
    /// <summary>
    /// Error constructor function - creates Error objects
    /// </summary>
    public object? Call(List<object?> arguments)
    {
        var message = arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : "";
        
        var errorObj = new Dictionary<string, object?>
        {
            ["name"] = "Error",
            ["message"] = message,
            ["stack"] = GenerateStackTrace()
        };
        
        return errorObj;
    }
    
    /// <summary>
    /// Error.captureStackTrace static method - Node.js specific
    /// In a real implementation, this would capture stack traces
    /// For now, we'll just add a simple stack property
    /// </summary>
    public static object? CaptureStackTrace(List<object?> arguments)
    {
        if (arguments.Count < 1) return null;
        
        var targetObject = arguments[0];
        if (targetObject is Dictionary<string, object?> errorDict)
        {
            // Add or update stack trace
            errorDict["stack"] = GenerateStackTrace();
        }
        
        return null; // captureStackTrace returns undefined
    }
    
    /// <summary>
    /// Generate a simple stack trace string
    /// In a real implementation, this would capture actual call stack
    /// </summary>
    private static string GenerateStackTrace()
    {
        return "Error\n    at <anonymous>\n    at ECEngine runtime";
    }
}

/// <summary>
/// Error module class that provides Error constructor and static methods
/// </summary>
public class ErrorModule
{
    public ErrorConstructor Error { get; }
    public ErrorMethodFunction captureStackTrace { get; }
    public double stackTraceLimit { get; set; } = 10; // Node.js default
    public object? prepareStackTrace { get; set; } = null; // Node.js stack trace formatter
    
    public ErrorModule()
    {
        Error = new ErrorConstructor();
        captureStackTrace = new ErrorMethodFunction("captureStackTrace");
    }
    
    /// <summary>
    /// Error constructor function when called directly
    /// </summary>
    public object? Call(List<object?> arguments)
    {
        return Error.Call(arguments);
    }
}

/// <summary>
/// Error method function wrapper for static Error methods
/// </summary>
public class ErrorMethodFunction
{
    private readonly string _methodName;
    
    public ErrorMethodFunction(string methodName)
    {
        _methodName = methodName;
    }
    
    public object? Call(List<object?> arguments)
    {
        return _methodName switch
        {
            "captureStackTrace" => ErrorConstructor.CaptureStackTrace(arguments),
            _ => throw new ECEngineException($"Unknown Error method: {_methodName}",
                1, 1, "", $"Error.{_methodName} is not implemented")
        };
    }
}
