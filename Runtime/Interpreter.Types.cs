using ECEngine.AST;
using ECEngine.Lexer;
using System.Reflection;
using System.Text.Json;

namespace ECEngine.Runtime;

/// <summary>
/// Type system support, typeof operator, instanceof checks, and type conversions for ECEngine interpreter
/// </summary>
public partial class Interpreter
{
    /// <summary>
    /// JavaScript typeof operator implementation
    /// </summary>
    private string EvaluateTypeOf(UnaryExpression unaryExpr)
    {
        try
        {
            var operand = Evaluate(unaryExpr.Operand, _sourceCode);
            return GetJavaScriptType(operand);
        }
        catch
        {
            // If evaluation fails (e.g., undefined variable), return "undefined"
            return "undefined";
        }
    }
    
    /// <summary>
    /// Get JavaScript type string for any value (typeof implementation)
    /// </summary>
    private string GetJavaScriptType(object? value)
    {
        return value switch
        {
            null => "object", // In JavaScript, typeof null === "object"
            bool => "boolean",
            byte or sbyte or short or ushort or int or uint or long or ulong => "number",
            float or double or decimal => "number",
            string => "string",
            Func<object[], object?> => "function",
            Function => "function",
            ConsoleLogFunction => "function",
            SetTimeoutFunction or SetIntervalFunction or ClearTimeoutFunction or ClearIntervalFunction => "function",
            RequireFunction or UrlConstructorFunction or URLSearchParamsConstructorFunction => "function",
            QuerystringMethodFunction => "function",
            PathMethodFunction => "function",
            CreateServerFunction or RequestFunction or GetFunction => "function",
            ServerListenFunction or ServerCloseFunction or ServerOnFunction or ServerEmitFunction => "function",
            ClientRequestWriteFunction or ClientRequestEndFunction or ClientRequestOnFunction => "function",
            IncomingMessageOnFunction or IncomingMessageSetEncodingFunction => "function",
            StringModule => "function", // String constructor is a function
            FunctionDeclaration => "function",
            ArrowFunction => "function",
            GeneratorFunction => "function",
            JSGenerator => "object", // Generator instances are objects
            Dictionary<string, object?> => "object",
            System.Collections.IEnumerable => "object", // Arrays and other collections
            _ when value?.GetType().Name == "Object" => "object",
            _ when IsUndefined(value) => "undefined",
            _ => "object" // Default to object for unknown types
        };
    }
    
    /// <summary>
    /// Check if a value represents JavaScript undefined
    /// </summary>
    private bool IsUndefined(object? value)
    {
        return value == null || 
               (value is string str && str == "undefined") ||
               value.Equals("undefined");
    }
    
    /// <summary>
    /// Convert value to JavaScript truthy/falsy boolean
    /// </summary>
    private bool IsTruthy(object? value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            int i => i != 0,
            long l => l != 0,
            double d => d != 0.0 && !double.IsNaN(d),
            float f => f != 0.0f && !float.IsNaN(f),
            string s => s.Length > 0,
            System.Collections.ICollection collection => collection.Count > 0,
            _ when IsUndefined(value) => false,
            _ => true // Objects and other types are truthy
        };
    }
    
    /// <summary>
    /// Convert value to JavaScript number
    /// </summary>
    private double ToNumber(object? value)
    {
        return value switch
        {
            null => 0.0,
            bool b => b ? 1.0 : 0.0,
            byte b => (double)b,
            sbyte sb => (double)sb,
            short s => (double)s,
            ushort us => (double)us,
            int i => (double)i,
            uint ui => (double)ui,
            long l => (double)l,
            ulong ul => (double)ul,
            float f => (double)f,
            double d => d,
            decimal dec => (double)dec,
            string str => ParseStringToNumber(str),
            _ when IsUndefined(value) => double.NaN,
            _ => double.NaN // Objects convert to NaN unless they have valueOf/toString
        };
    }
    
    /// <summary>
    /// Parse string to number following JavaScript rules
    /// </summary>
    private double ParseStringToNumber(string str)
    {
        if (string.IsNullOrEmpty(str))
            return 0.0;
            
        str = str.Trim();
        
        if (string.IsNullOrEmpty(str))
            return 0.0;
            
        if (str == "Infinity")
            return double.PositiveInfinity;
        if (str == "-Infinity")
            return double.NegativeInfinity;
        if (str == "NaN")
            return double.NaN;
            
        if (double.TryParse(str, out double result))
            return result;
            
        return double.NaN;
    }
    
    /// <summary>
    /// Convert value to 32-bit signed integer following JavaScript ToInt32 operation
    /// </summary>
    private int ToInt32(object? value)
    {
        var number = ToNumber(value);
        
        if (double.IsNaN(number) || double.IsInfinity(number))
            return 0;
            
        // Convert to 32-bit signed integer with proper overflow handling
        return (int)(long)number;
    }
    
    /// <summary>
    /// Convert value to JavaScript string
    /// </summary>
    private string ToString(object? value)
    {
        return value switch
        {
            null => "null",
            bool b => b ? "true" : "false",
            string s => s,
            double d when double.IsNaN(d) => "NaN",
            double d when double.IsPositiveInfinity(d) => "Infinity",
            double d when double.IsNegativeInfinity(d) => "-Infinity",
            float f when float.IsNaN(f) => "NaN",
            float f when float.IsPositiveInfinity(f) => "Infinity",
            float f when float.IsNegativeInfinity(f) => "-Infinity",
            _ when IsUndefined(value) => "undefined",
            Dictionary<string, object?> dict => JsonSerializer.Serialize(dict),
            System.Collections.IEnumerable enumerable => $"[{string.Join(",", enumerable.Cast<object?>().Select(ToString))}]",
            _ => value?.ToString() ?? "undefined"
        };
    }
    
    /// <summary>
    /// Perform JavaScript type coercion for binary operations
    /// </summary>
    private (object?, object?) CoerceOperands(object? left, object? right, string operatorType)
    {
        return operatorType switch
        {
            "+" => CoerceForAddition(left, right),
            "-" or "*" or "/" or "%" => (ToNumber(left), ToNumber(right)),
            "<" or ">" or "<=" or ">=" => CoerceForComparison(left, right),
            "==" or "!=" => CoerceForEquality(left, right),
            "===" or "!==" => (left, right), // No coercion for strict equality
            _ => (left, right)
        };
    }
    
    /// <summary>
    /// Coerce operands for addition (handles string concatenation vs numeric addition)
    /// </summary>
    private (object?, object?) CoerceForAddition(object? left, object? right)
    {
        // If either operand is a string, convert both to strings
        if (left is string || right is string)
        {
            return (ToString(left), ToString(right));
        }
        
        // Otherwise, convert both to numbers
        return (ToNumber(left), ToNumber(right));
    }
    
    /// <summary>
    /// Coerce operands for comparison operators
    /// </summary>
    private (object?, object?) CoerceForComparison(object? left, object? right)
    {
        // If both are strings, keep as strings for lexicographic comparison
        if (left is string && right is string)
        {
            return (left, right);
        }
        
        // Otherwise, convert both to numbers
        return (ToNumber(left), ToNumber(right));
    }
    
    /// <summary>
    /// Coerce operands for equality comparison (== and !=)
    /// </summary>
    private (object?, object?) CoerceForEquality(object? left, object? right)
    {
        // null and undefined are equal to each other but nothing else
        if ((left == null || IsUndefined(left)) && (right == null || IsUndefined(right)))
        {
            return (null, null);
        }
        
        // If types are the same, no coercion needed
        if (left?.GetType() == right?.GetType())
        {
            return (left, right);
        }
        
        // Number and string comparison
        if ((left is string || IsNumericType(left)) && (right is string || IsNumericType(right)))
        {
            return (ToNumber(left), ToNumber(right));
        }
        
        // Boolean gets converted to number
        if (left is bool)
        {
            return CoerceForEquality(ToNumber(left), right);
        }
        if (right is bool)
        {
            return CoerceForEquality(left, ToNumber(right));
        }
        
        return (left, right);
    }
    
    /// <summary>
    /// Check if a value is a numeric type
    /// </summary>
    private bool IsNumericType(object? value)
    {
        return value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }
    
    /// <summary>
    /// Perform strict equality comparison (=== and !==)
    /// </summary>
    private bool StrictEquals(object? left, object? right)
    {
        // Both null or undefined
        if ((left == null || IsUndefined(left)) && (right == null || IsUndefined(right)))
        {
            return true;
        }
        
        // One null/undefined, other not
        if ((left == null || IsUndefined(left)) || (right == null || IsUndefined(right)))
        {
            return false;
        }
        
        // Different types
        if (left?.GetType() != right?.GetType())
        {
            return false;
        }
        
        // Same type, use standard equality
        return Equals(left, right);
    }
    
    /// <summary>
    /// Check if value is an instance of a constructor/type
    /// </summary>
    private bool IsInstanceOf(object? value, object? constructor)
    {
        if (value == null || constructor == null)
            return false;
            
        string constructorName = constructor.ToString() ?? "";
        
        return constructorName switch
        {
            "Object" => value is Dictionary<string, object?>,
            "Array" => value is System.Collections.IList,
            "String" => value is string,
            "Number" => IsNumericType(value),
            "Boolean" => value is bool,
            "Function" => value is Func<object[], object?> || value is FunctionDeclaration || value is ArrowFunction,
            "Date" => value is DateTime,
            "RegExp" => value is System.Text.RegularExpressions.Regex,
            _ => false
        };
    }
}
