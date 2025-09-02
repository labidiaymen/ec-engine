using ECEngine.AST;
using ECEngine.Lexer;
using System.Reflection;
using System.Text.Json;

namespace ECEngine.Runtime;

/// <summary>
/// Object and array creation, property access, and manipulation for ECEngine interpreter
/// </summary>
public partial class Interpreter
{
    /// <summary>
    /// Evaluate object literals
    /// </summary>
    private object? EvaluateObjectLiteral(ObjectLiteral objLiteral)
    {
        var obj = new Dictionary<string, object?>();
        
        foreach (var property in objLiteral.Properties)
        {
            string keyName = property.Key;
            var value = Evaluate(property.Value, _sourceCode);
            obj[keyName] = value;
        }
        
        return obj;
    }
    
    /// <summary>
    /// Evaluate array literals
    /// </summary>
    private object? EvaluateArrayLiteral(ArrayLiteral arrayLiteral)
    {
        var array = new List<object?>();
        
        foreach (var element in arrayLiteral.Elements)
        {
            if (element == null)
            {
                // Sparse array element (e.g., [1, , 3])
                array.Add(null);
            }
            else
            {
                array.Add(Evaluate(element, _sourceCode));
            }
        }
        
        return array;
    }
    
    /// <summary>
    /// Get property value from an object
    /// </summary>
    private object? GetObjectProperty(object? obj, string propertyName)
    {
        if (obj == null)
            return null;
            
        // Handle built-in object properties and methods
        switch (obj)
        {
            case Dictionary<string, object?> dict:
                return dict.TryGetValue(propertyName, out var value) ? value : null;
                
            case List<object?> list:
                return GetArrayProperty(list, propertyName);
                
            case string str:
                return str == "Object" ? GetObjectModuleProperty(propertyName) : GetStringProperty(str, propertyName);
                
            case DateObject dateObj:
                return GetDateObjectProperty(dateObj, propertyName);
                
            case DateTime date:
                return GetDateProperty(date, propertyName);
                
            case ConsoleObject when propertyName == "log":
                return new ConsoleLogFunction();
                
            case Generator generator:
                return GetGeneratorProperty(generator, propertyName);
                
            case MathModule mathModule:
                // Handle JavaScript-style lowercase property names for Math object
                return GetMathProperty(mathModule, propertyName);
                
            case DateModule dateModule:
                // Handle Date static methods and properties
                return GetDateModuleProperty(dateModule, propertyName);
                
            case JsonModule jsonModule:
                // Handle JSON methods
                return GetJsonModuleProperty(jsonModule, propertyName);
                
            case StringModule stringModule:
                // Use reflection to get the property from StringModule
                return GetReflectionProperty(obj, propertyName);
                
            default:
                // Try reflection for C# objects
                return GetReflectionProperty(obj, propertyName);
        }
    }
    
    /// <summary>
    /// Set property value on an object
    /// </summary>
    private void SetObjectProperty(object? obj, string propertyName, object? value)
    {
        if (obj == null)
            throw new ECEngineException("Cannot set property on null",
                1, 1, _sourceCode, "Attempted to set property on null value");
        
        switch (obj)
        {
            case Dictionary<string, object?> dict:
                dict[propertyName] = value;
                break;
                
            case List<object?> list:
                SetArrayProperty(list, propertyName, value);
                break;
                
            default:
                // Try reflection for C# objects
                SetReflectionProperty(obj, propertyName, value);
                break;
        }
    }
    
    /// <summary>
    /// Get array-specific properties and methods
    /// </summary>
    private object? GetArrayProperty(List<object?> array, string propertyName)
    {
        return propertyName switch
        {
            "length" => (double)array.Count,
            "push" => new Func<object[], object?>(args => {
                foreach (var arg in args)
                {
                    array.Add(arg);
                }
                return (double)array.Count;
            }),
            "pop" => new Func<object[], object?>(args => {
                if (array.Count == 0) return null;
                var last = array[array.Count - 1];
                array.RemoveAt(array.Count - 1);
                return last;
            }),
            "shift" => new Func<object[], object?>(args => {
                if (array.Count == 0) return null;
                var first = array[0];
                array.RemoveAt(0);
                return first;
            }),
            "unshift" => new Func<object[], object?>(args => {
                for (int i = args.Length - 1; i >= 0; i--)
                {
                    array.Insert(0, args[i]);
                }
                return (double)array.Count;
            }),
            "slice" => new Func<object[], object?>(args => {
                var start = args.Length > 0 ? (int)ToNumber(args[0]) : 0;
                var end = args.Length > 1 ? (int)ToNumber(args[1]) : array.Count;
                
                if (start < 0) start = Math.Max(0, array.Count + start);
                if (end < 0) end = Math.Max(0, array.Count + end);
                
                start = Math.Max(0, Math.Min(start, array.Count));
                end = Math.Max(start, Math.Min(end, array.Count));
                
                return array.Skip(start).Take(end - start).ToList();
            }),
            "join" => new Func<object[], object?>(args => {
                var separator = args.Length > 0 ? ToString(args[0]) : ",";
                return string.Join(separator, array.Select(ToString));
            }),
            "indexOf" => new Func<object[], object?>(args => {
                if (args.Length == 0) return -1.0;
                var searchValue = args[0];
                for (int i = 0; i < array.Count; i++)
                {
                    if (StrictEquals(array[i], searchValue))
                        return (double)i;
                }
                return -1.0;
            }),
            "includes" => new Func<object[], object?>(args => {
                if (args.Length == 0) return false;
                var searchValue = args[0];
                return array.Any(item => StrictEquals(item, searchValue));
            }),
            _ when int.TryParse(propertyName, out int index) => 
                index >= 0 && index < array.Count ? array[index] : null,
            _ => null
        };
    }
    
    /// <summary>
    /// Set array-specific properties
    /// </summary>
    private void SetArrayProperty(List<object?> array, string propertyName, object? value)
    {
        if (propertyName == "length")
        {
            var newLength = (int)ToNumber(value);
            if (newLength < 0)
                throw new ECEngineException("Invalid array length", 1, 1, _sourceCode, "Array length cannot be negative");
                
            if (newLength < array.Count)
            {
                // Truncate array
                array.RemoveRange(newLength, array.Count - newLength);
            }
            else if (newLength > array.Count)
            {
                // Extend array with undefined values
                while (array.Count < newLength)
                {
                    array.Add(null);
                }
            }
        }
        else if (int.TryParse(propertyName, out int index))
        {
            if (index < 0)
                return; // Invalid index
                
            // Extend array if necessary
            while (array.Count <= index)
            {
                array.Add(null);
            }
            
            array[index] = value;
        }
        // Other properties are ignored for arrays
    }
    
    /// <summary>
    /// Get string-specific properties and methods
    /// </summary>
    private object? GetStringProperty(string str, string propertyName)
    {
        return propertyName switch
        {
            "length" => (double)str.Length,
            "charAt" => new Func<object[], object?>(args => {
                var index = args.Length > 0 ? (int)ToNumber(args[0]) : 0;
                return index >= 0 && index < str.Length ? str[index].ToString() : "";
            }),
            "charCodeAt" => new Func<object[], object?>(args => {
                var index = args.Length > 0 ? (int)ToNumber(args[0]) : 0;
                return index >= 0 && index < str.Length ? (double)str[index] : double.NaN;
            }),
            "indexOf" => new Func<object[], object?>(args => {
                if (args.Length == 0) return -1.0;
                var searchStr = ToString(args[0]);
                var startIndex = args.Length > 1 ? (int)ToNumber(args[1]) : 0;
                return (double)str.IndexOf(searchStr, Math.Max(0, startIndex));
            }),
            "lastIndexOf" => new Func<object[], object?>(args => {
                if (args.Length == 0) return -1.0;
                var searchStr = ToString(args[0]);
                var startIndex = args.Length > 1 ? (int)ToNumber(args[1]) : str.Length;
                return (double)str.LastIndexOf(searchStr, Math.Min(str.Length - 1, startIndex));
            }),
            "substring" => new Func<object[], object?>(args => {
                var start = args.Length > 0 ? (int)ToNumber(args[0]) : 0;
                var end = args.Length > 1 ? (int)ToNumber(args[1]) : str.Length;
                
                start = Math.Max(0, Math.Min(start, str.Length));
                end = Math.Max(0, Math.Min(end, str.Length));
                
                if (start > end)
                    (start, end) = (end, start);
                
                return str.Substring(start, end - start);
            }),
            "slice" => new Func<object[], object?>(args => {
                var start = args.Length > 0 ? (int)ToNumber(args[0]) : 0;
                var end = args.Length > 1 ? (int)ToNumber(args[1]) : str.Length;
                
                if (start < 0) start = Math.Max(0, str.Length + start);
                if (end < 0) end = Math.Max(0, str.Length + end);
                
                start = Math.Max(0, Math.Min(start, str.Length));
                end = Math.Max(start, Math.Min(end, str.Length));
                
                return str.Substring(start, end - start);
            }),
            "toLowerCase" => new Func<object[], object?>(args => str.ToLower()),
            "toUpperCase" => new Func<object[], object?>(args => str.ToUpper()),
            "trim" => new Func<object[], object?>(args => str.Trim()),
            "trimStart" => new Func<object[], object?>(args => str.TrimStart()),
            "trimEnd" => new Func<object[], object?>(args => str.TrimEnd()),
            "concat" => new Func<object[], object?>(args => {
                var result = str;
                foreach (var arg in args)
                {
                    result += ToString(arg);
                }
                return result;
            }),
            "normalize" => new Func<object[], object?>(args => {
                // Simplified normalization - JavaScript supports various forms
                return str.Normalize();
            }),
            "includes" => new Func<object[], object?>(args => {
                if (args.Length == 0) return false;
                var searchValue = ToString(args[0]);
                var fromIndex = args.Length > 1 ? Math.Max(0, (int)ToNumber(args[1])) : 0;
                if (fromIndex >= str.Length) return false;
                return str.IndexOf(searchValue, fromIndex, StringComparison.Ordinal) >= 0;
            }),
            "startsWith" => new Func<object[], object?>(args => {
                if (args.Length == 0) return false;
                var searchString = ToString(args[0]);
                var position = args.Length > 1 ? Math.Max(0, (int)ToNumber(args[1])) : 0;
                if (position >= str.Length) return false;
                return str.Substring(position).StartsWith(searchString, StringComparison.Ordinal);
            }),
            "endsWith" => new Func<object[], object?>(args => {
                if (args.Length == 0) return false;
                var searchString = ToString(args[0]);
                var length = args.Length > 1 ? (int)ToNumber(args[1]) : str.Length;
                if (length < 0) length = 0;
                if (length > str.Length) length = str.Length;
                var substring = str.Substring(0, length);
                return substring.EndsWith(searchString, StringComparison.Ordinal);
            }),
            "repeat" => new Func<object[], object?>(args => {
                var count = args.Length > 0 ? (int)ToNumber(args[0]) : 0;
                if (count < 0) throw new ArgumentOutOfRangeException();
                if (count == 0) return "";
                return string.Concat(Enumerable.Repeat(str, count));
            }),
            "padStart" => new Func<object[], object?>(args => {
                var targetLength = args.Length > 0 ? (int)ToNumber(args[0]) : 0;
                var padString = args.Length > 1 ? ToString(args[1]) : " ";
                if (targetLength <= str.Length) return str;
                if (string.IsNullOrEmpty(padString)) return str;
                var padLength = targetLength - str.Length;
                var pad = string.Concat(Enumerable.Repeat(padString, (padLength / padString.Length) + 1));
                return pad.Substring(0, padLength) + str;
            }),
            "padEnd" => new Func<object[], object?>(args => {
                var targetLength = args.Length > 0 ? (int)ToNumber(args[0]) : 0;
                var padString = args.Length > 1 ? ToString(args[1]) : " ";
                if (targetLength <= str.Length) return str;
                if (string.IsNullOrEmpty(padString)) return str;
                var padLength = targetLength - str.Length;
                var pad = string.Concat(Enumerable.Repeat(padString, (padLength / padString.Length) + 1));
                return str + pad.Substring(0, padLength);
            }),
            "at" => new Func<object[], object?>(args => {
                var index = args.Length > 0 ? (int)ToNumber(args[0]) : 0;
                // Handle negative indices
                if (index < 0) index = str.Length + index;
                if (index < 0 || index >= str.Length) return null;
                return str[index].ToString();
            }),
            "split" => new Func<object[], object?>(args => {
                if (args.Length == 0) return new List<object?> { str };
                
                var separator = ToString(args[0]);
                var limit = args.Length > 1 ? (int)ToNumber(args[1]) : int.MaxValue;
                
                if (string.IsNullOrEmpty(separator))
                {
                    return str.Take(limit).Select(c => c.ToString()).Cast<object?>().ToList();
                }
                
                return str.Split(new[] { separator }, limit, StringSplitOptions.None)
                         .Cast<object?>().ToList();
            }),
            "replace" => new Func<object[], object?>(args => {
                if (args.Length < 2) return str;
                var searchValue = ToString(args[0]);
                var replaceValue = ToString(args[1]);
                
                var index = str.IndexOf(searchValue);
                if (index == -1) return str;
                
                return str.Substring(0, index) + replaceValue + str.Substring(index + searchValue.Length);
            }),
            _ when int.TryParse(propertyName, out int index) => 
                index >= 0 && index < str.Length ? str[index].ToString() : null,
            _ => GetStringMethodFunction(str, propertyName)
        };
    }
    
    /// <summary>
    /// Get a string method function if it exists
    /// </summary>
    private object? GetStringMethodFunction(string str, string methodName)
    {
        try
        {
            var stringMethod = new StringMethodFunction(str, methodName);
            return new Func<object[], object?>(args => stringMethod.Call(args.ToList()));
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Get Date-specific properties and methods
    /// </summary>
    private object? GetDateProperty(DateTime date, string propertyName)
    {
        return propertyName switch
        {
            "getTime" => new Func<object[], object?>(args => 
                (date - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds),
            "getFullYear" => new Func<object[], object?>(args => (double)date.Year),
            "getMonth" => new Func<object[], object?>(args => (double)(date.Month - 1)), // 0-based
            "getDate" => new Func<object[], object?>(args => (double)date.Day),
            "getDay" => new Func<object[], object?>(args => (double)date.DayOfWeek),
            "getHours" => new Func<object[], object?>(args => (double)date.Hour),
            "getMinutes" => new Func<object[], object?>(args => (double)date.Minute),
            "getSeconds" => new Func<object[], object?>(args => (double)date.Second),
            "getMilliseconds" => new Func<object[], object?>(args => (double)date.Millisecond),
            "toString" => new Func<object[], object?>(args => date.ToString()),
            "toISOString" => new Func<object[], object?>(args => date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")),
            "valueOf" => new Func<object[], object?>(args => 
                (date - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds),
            _ => null
        };
    }
    
    /// <summary>
    /// Get DateObject-specific properties and methods
    /// </summary>
    private object? GetDateObjectProperty(DateObject dateObj, string propertyName)
    {
        return propertyName switch
        {
            "getTime" => new DateMethodFunction(dateObj, "getTime"),
            "getFullYear" => new DateMethodFunction(dateObj, "getFullYear"),
            "getMonth" => new DateMethodFunction(dateObj, "getMonth"),
            "getDate" => new DateMethodFunction(dateObj, "getDate"),
            "getDay" => new DateMethodFunction(dateObj, "getDay"),
            "getHours" => new DateMethodFunction(dateObj, "getHours"),
            "getMinutes" => new DateMethodFunction(dateObj, "getMinutes"),
            "getSeconds" => new DateMethodFunction(dateObj, "getSeconds"),
            "getMilliseconds" => new DateMethodFunction(dateObj, "getMilliseconds"),
            "getUTCFullYear" => new DateMethodFunction(dateObj, "getUTCFullYear"),
            "getUTCMonth" => new DateMethodFunction(dateObj, "getUTCMonth"),
            "getUTCDate" => new DateMethodFunction(dateObj, "getUTCDate"),
            "getUTCDay" => new DateMethodFunction(dateObj, "getUTCDay"),
            "getUTCHours" => new DateMethodFunction(dateObj, "getUTCHours"),
            "getUTCMinutes" => new DateMethodFunction(dateObj, "getUTCMinutes"),
            "getUTCSeconds" => new DateMethodFunction(dateObj, "getUTCSeconds"),
            "getUTCMilliseconds" => new DateMethodFunction(dateObj, "getUTCMilliseconds"),
            "toString" => new DateMethodFunction(dateObj, "toString"),
            "toDateString" => new DateMethodFunction(dateObj, "toDateString"),
            "toTimeString" => new DateMethodFunction(dateObj, "toTimeString"),
            "toISOString" => new DateMethodFunction(dateObj, "toISOString"),
            "toUTCString" => new DateMethodFunction(dateObj, "toUTCString"),
            "toLocaleDateString" => new DateMethodFunction(dateObj, "toLocaleDateString"),
            "toLocaleTimeString" => new DateMethodFunction(dateObj, "toLocaleTimeString"),
            "toLocaleString" => new DateMethodFunction(dateObj, "toLocaleString"),
            "valueOf" => new DateMethodFunction(dateObj, "valueOf"),
            _ => null
        };
    }
    
    /// <summary>
    /// Get Math object property with case-insensitive JavaScript-style property names
    /// </summary>
    private object? GetMathProperty(MathModule mathModule, string propertyName)
    {
        return propertyName.ToLowerInvariant() switch
        {
            // Constants
            "e" => mathModule.E,
            "pi" => mathModule.PI,
            "ln2" => mathModule.LN2,
            "ln10" => mathModule.LN10,
            "log2e" => mathModule.LOG2E,
            "log10e" => mathModule.LOG10E,
            "sqrt1_2" => mathModule.SQRT1_2,
            "sqrt2" => mathModule.SQRT2,
            
            // Functions
            "abs" => mathModule.Abs,
            "acos" => mathModule.Acos,
            "asin" => mathModule.Asin,
            "atan" => mathModule.Atan,
            "atan2" => mathModule.Atan2,
            "ceil" => mathModule.Ceil,
            "cos" => mathModule.Cos,
            "exp" => mathModule.Exp,
            "floor" => mathModule.Floor,
            "log" => mathModule.Log,
            "max" => mathModule.Max,
            "min" => mathModule.Min,
            "pow" => mathModule.Pow,
            "random" => mathModule.Random,
            "round" => mathModule.Round,
            "sin" => mathModule.Sin,
            "sqrt" => mathModule.Sqrt,
            "tan" => mathModule.Tan,
            "trunc" => mathModule.Trunc,
            
            _ => null
        };
    }
    
    /// <summary>
    /// Get Date module static properties and methods
    /// </summary>
    private object? GetDateModuleProperty(DateModule dateModule, string propertyName)
    {
        return propertyName switch
        {
            "now" => dateModule.Now,
            "parse" => dateModule.Parse,
            "UTC" => dateModule.UTC,
            _ => null
        };
    }
    
    /// <summary>
    /// Get JSON module-specific properties and methods
    /// </summary>
    private object? GetJsonModuleProperty(JsonModule jsonModule, string propertyName)
    {
        return propertyName switch
        {
            "parse" => new JsonMethodFunction(jsonModule.Parse, "parse"),
            "stringify" => new JsonMethodFunction(jsonModule.Stringify, "stringify"),
            _ => null
        };
    }
    
    /// <summary>
    /// Get Object static methods and properties
    /// </summary>
    private object? GetObjectModuleProperty(string propertyName)
    {
        var objectModule = new ObjectModule();
        return propertyName switch
        {
            "keys" => new ObjectMethodFunction(objectModule, "keys"),
            "values" => new ObjectMethodFunction(objectModule, "values"),
            "entries" => new ObjectMethodFunction(objectModule, "entries"),
            "assign" => new ObjectMethodFunction(objectModule, "assign"),
            "create" => new ObjectMethodFunction(objectModule, "create"),
            "freeze" => new ObjectMethodFunction(objectModule, "freeze"),
            "seal" => new ObjectMethodFunction(objectModule, "seal"),
            "hasOwnProperty" => new ObjectMethodFunction(objectModule, "hasOwnProperty"),
            _ => null
        };
    }
    
    /// <summary>
    /// Get property using reflection for C# objects
    /// </summary>
    private object? GetReflectionProperty(object obj, string propertyName)
    {
        var type = obj.GetType();
        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        
        if (property != null && property.CanRead)
        {
            return property.GetValue(obj);
        }
        
        var field = type.GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (field != null)
        {
            return field.GetValue(obj);
        }
        
        return null;
    }
    
    /// <summary>
    /// Set property using reflection for C# objects
    /// </summary>
    private void SetReflectionProperty(object obj, string propertyName, object? value)
    {
        var type = obj.GetType();
        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        
        if (property != null && property.CanWrite)
        {
            try
            {
                property.SetValue(obj, value);
                return;
            }
            catch
            {
                // Ignore type conversion errors
            }
        }
        
        var field = type.GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (field != null)
        {
            try
            {
                field.SetValue(obj, value);
            }
            catch
            {
                // Ignore type conversion errors
            }
        }
    }
    
    /// <summary>
    /// Create new instance using constructor
    /// </summary>
    private object? CreateNewInstance(object? constructor, object[] args)
    {
        if (constructor == null)
        {
            throw new ECEngineException("Constructor is null",
                1, 1, _sourceCode, "Cannot create instance from null constructor");
        }
        
        string constructorName = constructor.ToString() ?? "";
        
        return constructorName switch
        {
            "Object" => new Dictionary<string, object?>(),
            "Array" => args.Length == 1 && ToNumber(args[0]) % 1 == 0 && ToNumber(args[0]) >= 0
                ? new List<object?>(new object?[(int)ToNumber(args[0])]) // Single numeric arg = length
                : new List<object?>(args), // Multiple args = array elements
            "String" => args.Length > 0 ? ToString(args[0]) : "",
            "Number" => args.Length > 0 ? ToNumber(args[0]) : 0.0,
            "Boolean" => args.Length > 0 ? IsTruthy(args[0]) : false,
            "Date" => args.Length > 0 
                ? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(ToNumber(args[0]))
                : DateTime.Now,
            _ when constructor is StringModule => args.Length > 0 ? ToString(args[0]) : "",
            _ when constructor is DateModule => args.Length > 0 
                ? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(ToNumber(args[0]))
                : DateTime.Now,
            _ when constructor is UrlConstructorFunction => 
                new UrlConstructorFunction().Call(args.ToList()),
            _ when constructor is URLSearchParamsConstructorFunction => 
                new URLSearchParamsConstructorFunction().Call(args.ToList()),
            _ => throw new ECEngineException($"Constructor not found: {constructorName}",
                1, 1, _sourceCode, $"Constructor '{constructorName}' is not defined")
        };
    }
    
    /// <summary>
    /// Get generator-specific properties and methods
    /// </summary>
    private object? GetGeneratorProperty(Generator generator, string propertyName)
    {
        return propertyName switch
        {
            "next" => new Func<object[], object?>(args => generator.Next()),
            _ => null
        };
    }
}
