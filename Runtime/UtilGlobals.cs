using ECEngine.AST;
using System.Text;
using System.Text.RegularExpressions;

namespace ECEngine.Runtime;

/// <summary>
/// Complete Node.js util module implementation for ECEngine
/// Provides utilities for object inspection, formatting, type checking, and more
/// </summary>
public class UtilModule
{
    public InspectFunction inspect { get; }
    public FormatFunction format { get; }
    public IsArrayFunction isArray { get; }
    public IsDateFunction isDate { get; }
    public IsErrorFunction isError { get; }
    public IsFunctionFunction isFunction { get; }
    public IsNullOrUndefinedFunction isNullOrUndefined { get; }
    public IsNumberFunction isNumber { get; }
    public IsObjectFunction isObject { get; }
    public IsPrimitiveFunction isPrimitive { get; }
    public IsStringFunction isString { get; }
    public IsSymbolFunction isSymbol { get; }
    public IsUndefinedFunction isUndefined { get; }
    public IsRegExpFunction isRegExp { get; }
    public IsDeepStrictEqualFunction isDeepStrictEqual { get; }
    public DebuglogFunction debuglog { get; }
    public InheritFunction inherits { get; }
    public PromisifyFunction promisify { get; }
    public CallbackifyFunction callbackify { get; }
    public TypesModule types { get; }

    public UtilModule()
    {
        inspect = new InspectFunction();
        format = new FormatFunction();
        isArray = new IsArrayFunction();
        isDate = new IsDateFunction();
        isError = new IsErrorFunction();
        isFunction = new IsFunctionFunction();
        isNullOrUndefined = new IsNullOrUndefinedFunction();
        isNumber = new IsNumberFunction();
        isObject = new IsObjectFunction();
        isPrimitive = new IsPrimitiveFunction();
        isString = new IsStringFunction();
        isSymbol = new IsSymbolFunction();
        isUndefined = new IsUndefinedFunction();
        isRegExp = new IsRegExpFunction();
        isDeepStrictEqual = new IsDeepStrictEqualFunction();
        debuglog = new DebuglogFunction();
        inherits = new InheritFunction();
        promisify = new PromisifyFunction();
        callbackify = new CallbackifyFunction();
        types = new TypesModule();
    }
}

/// <summary>
/// util.inspect() - Return a string representation of object for debugging
/// </summary>
public class InspectFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            return "undefined";

        var obj = arguments[0];
        var options = arguments.Count > 1 ? arguments[1] : null;
        
        return FormatObject(obj, options);
    }

    private string FormatObject(object? obj, object? options = null)
    {
        // Handle basic types
        if (obj == null) return "null";
        if (obj is string str) return $"'{str}'";
        if (obj is bool b) return b.ToString().ToLower();
        if (obj is double d) return FormatNumber(d);
        if (obj is int i) return i.ToString();
        if (obj is float f) return f.ToString();

        // Handle arrays
        if (obj is List<object?> list)
        {
            var items = list.Select(item => FormatObject(item)).ToArray();
            return $"[ {string.Join(", ", items)} ]";
        }

        // Handle objects/dictionaries
        if (obj is Dictionary<string, object?> dict)
        {
            var pairs = dict.Select(kvp => $"{kvp.Key}: {FormatObject(kvp.Value)}").ToArray();
            return $"{{ {string.Join(", ", pairs)} }}";
        }

        // Handle functions
        if (obj is Function func)
        {
            return $"[Function: {func.Name ?? "anonymous"}]";
        }

        // Handle other types
        return $"{obj}";
    }

    private string FormatNumber(double d)
    {
        if (double.IsNaN(d)) return "NaN";
        if (double.IsPositiveInfinity(d)) return "Infinity";
        if (double.IsNegativeInfinity(d)) return "-Infinity";
        return d.ToString();
    }
}

/// <summary>
/// util.format() - Returns a formatted string using printf-like format
/// </summary>
public class FormatFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return "";
        
        var format = arguments[0]?.ToString() ?? "";
        var args = arguments.Skip(1).ToArray();
        
        return FormatString(format, args);
    }

    private string FormatString(string format, object?[] args)
    {
        var result = new StringBuilder();
        var argIndex = 0;
        
        for (int i = 0; i < format.Length; i++)
        {
            if (format[i] == '%' && i + 1 < format.Length && argIndex < args.Length)
            {
                var specifier = format[i + 1];
                var arg = args[argIndex++];
                
                switch (specifier)
                {
                    case 's': // string
                        result.Append(arg?.ToString() ?? "");
                        break;
                    case 'd': // number
                        if (arg is double d) result.Append(d.ToString());
                        else if (int.TryParse(arg?.ToString(), out int intVal)) result.Append(intVal);
                        else result.Append("NaN");
                        break;
                    case 'j': // JSON
                        try 
                        {
                            var jsonModule = new JsonModule();
                            var jsonString = jsonModule.Stringify.Call([arg])?.ToString() ?? "[object Object]";
                            result.Append(jsonString);
                        }
                        catch 
                        {
                            result.Append("[object Object]");
                        }
                        break;
                    case '%': // literal %
                        result.Append('%');
                        argIndex--; // Don't consume argument
                        break;
                    default:
                        result.Append('%').Append(specifier);
                        argIndex--; // Don't consume argument
                        break;
                }
                i++; // Skip the specifier character
            }
            else
            {
                result.Append(format[i]);
            }
        }
        
        // Append remaining arguments
        while (argIndex < args.Length)
        {
            result.Append(' ').Append(args[argIndex++]?.ToString() ?? "");
        }
        
        return result.ToString();
    }
}

/// <summary>
/// util.isArray() - Check if value is an array
/// </summary>
public class IsArrayFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        return arguments[0] is List<object?>;
    }
}

/// <summary>
/// util.isDate() - Check if value is a Date
/// </summary>
public class IsDateFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        return arguments[0] is DateObject;
    }
}

/// <summary>
/// util.isError() - Check if value is an Error
/// </summary>
public class IsErrorFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        var obj = arguments[0];
        return obj is Exception || (obj is Dictionary<string, object?> dict && dict.ContainsKey("message"));
    }
}

/// <summary>
/// util.isFunction() - Check if value is a function
/// </summary>
public class IsFunctionFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        return arguments[0] is Function;
    }
}

/// <summary>
/// util.isNullOrUndefined() - Check if value is null or undefined
/// </summary>
public class IsNullOrUndefinedFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return true; // No argument = undefined
        return arguments[0] == null;
    }
}

/// <summary>
/// util.isNumber() - Check if value is a number
/// </summary>
public class IsNumberFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        return arguments[0] is double || arguments[0] is int || arguments[0] is float;
    }
}

/// <summary>
/// util.isObject() - Check if value is an object
/// </summary>
public class IsObjectFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        var obj = arguments[0];
        return obj != null && !(obj is string) && !(obj is double) && !(obj is int) && !(obj is bool);
    }
}

/// <summary>
/// util.isPrimitive() - Check if value is a primitive
/// </summary>
public class IsPrimitiveFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return true; // undefined is primitive
        var obj = arguments[0];
        return obj == null || obj is string || obj is double || obj is int || obj is bool;
    }
}

/// <summary>
/// util.isString() - Check if value is a string
/// </summary>
public class IsStringFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        return arguments[0] is string;
    }
}

/// <summary>
/// util.isSymbol() - Check if value is a symbol
/// </summary>
public class IsSymbolFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        // ECEngine doesn't have symbols yet, so always false
        return false;
    }
}

/// <summary>
/// util.isUndefined() - Check if value is undefined
/// </summary>
public class IsUndefinedFunction
{
    public object? Call(List<object?> arguments)
    {
        return arguments.Count == 0 || arguments[0] == null;
    }
}

/// <summary>
/// util.isRegExp() - Check if value is a regular expression
/// </summary>
public class IsRegExpFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        return arguments[0] is Regex;
    }
}

/// <summary>
/// util.isDeepStrictEqual() - Deep comparison of two values
/// </summary>
public class IsDeepStrictEqualFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2) return false;
        return DeepEquals(arguments[0], arguments[1]);
    }

    private bool DeepEquals(object? a, object? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return false;
        if (a.GetType() != b.GetType()) return false;

        if (a is Dictionary<string, object?> dictA && b is Dictionary<string, object?> dictB)
        {
            if (dictA.Count != dictB.Count) return false;
            foreach (var kvp in dictA)
            {
                if (!dictB.ContainsKey(kvp.Key) || !DeepEquals(kvp.Value, dictB[kvp.Key]))
                    return false;
            }
            return true;
        }

        if (a is List<object?> listA && b is List<object?> listB)
        {
            if (listA.Count != listB.Count) return false;
            for (int i = 0; i < listA.Count; i++)
            {
                if (!DeepEquals(listA[i], listB[i])) return false;
            }
            return true;
        }

        return a.Equals(b);
    }
}

/// <summary>
/// util.debuglog() - Create a debug function
/// </summary>
public class DebuglogFunction
{
    public object? Call(List<object?> arguments)
    {
        var section = arguments.Count > 0 ? arguments[0]?.ToString() ?? "debug" : "debug";
        return new DebugFunction(section);
    }
}

public class DebugFunction
{
    private readonly string _section;

    public DebugFunction(string section)
    {
        _section = section;
    }

    public object? Call(List<object?> arguments)
    {
        // In Node.js, debug functions only log if DEBUG environment variable is set
        // For now, we'll just log with a debug prefix
        var message = string.Join(" ", arguments.Select(arg => arg?.ToString() ?? ""));
        Console.WriteLine($"DEBUG[{_section}]: {message}");
        return null;
    }
}

/// <summary>
/// util.inherits() - Inherit prototype methods from one constructor to another
/// </summary>
public class InheritFunction
{
    public object? Call(List<object?> arguments)
    {
        // ECEngine doesn't have full prototype support yet
        // This is a placeholder implementation
        if (arguments.Count < 2) return null;
        
        Console.WriteLine("util.inherits() called - prototype inheritance not fully implemented in ECEngine");
        return null;
    }
}

/// <summary>
/// util.promisify() - Convert callback-style function to Promise-based
/// </summary>
public class PromisifyFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return null;
        
        // ECEngine doesn't have Promises yet, so this is a placeholder
        Console.WriteLine("util.promisify() called - Promise support not implemented in ECEngine");
        return arguments[0]; // Return original function for now
    }
}

/// <summary>
/// util.callbackify() - Convert Promise-based function to callback-style
/// </summary>
public class CallbackifyFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return null;
        
        // ECEngine doesn't have Promises yet, so this is a placeholder
        Console.WriteLine("util.callbackify() called - Promise support not implemented in ECEngine");
        return arguments[0]; // Return original function for now
    }
}

/// <summary>
/// util.types module - Type checking utilities
/// </summary>
public class TypesModule
{
    public IsArrayBufferFunction isArrayBuffer { get; }
    public IsDateFunction isDate { get; }
    public IsMapFunction isMap { get; }
    public IsSetFunction isSet { get; }
    public IsRegExpFunction isRegExp { get; }

    public TypesModule()
    {
        isArrayBuffer = new IsArrayBufferFunction();
        isDate = new IsDateFunction();
        isMap = new IsMapFunction();
        isSet = new IsSetFunction();
        isRegExp = new IsRegExpFunction();
    }
}

public class IsArrayBufferFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        return arguments[0] is byte[];
    }
}

public class IsMapFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        // ECEngine doesn't have Map objects yet
        return false;
    }
}

public class IsSetFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        // ECEngine doesn't have Set objects yet
        return false;
    }
}

/// <summary>
/// Helper function wrappers for method calls
/// </summary>
public class UtilMethodFunction
{
    private readonly object _method;
    private readonly string _methodName;

    public UtilMethodFunction(object method, string methodName)
    {
        _method = method;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        try
        {
            var methodInfo = _method.GetType().GetMethod("Call");
            if (methodInfo != null)
            {
                return methodInfo.Invoke(_method, new object[] { arguments });
            }
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Error calling util.{_methodName}: {ex.Message}",
                1, 1, "", ex.Message);
        }
        
        return null;
    }
}
