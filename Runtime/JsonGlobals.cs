namespace ECEngine.Runtime;

using System.Text.Json;

/// <summary>
/// JSON global object for ECEngine scripts
/// Provides JavaScript-compatible JSON parsing and stringification
/// </summary>
public class JsonModule
{
    // Static function instances
    public JsonParseFunction Parse { get; } = new JsonParseFunction();
    public JsonStringifyFunction Stringify { get; } = new JsonStringifyFunction();
}

/// <summary>
/// JSON.parse() function
/// Parses a JSON string and returns the corresponding ECEngine value
/// </summary>
public class JsonParseFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
        {
            throw new ECEngineException("JSON.parse requires at least 1 argument", 0, 0, "", "Runtime error");
        }

        var jsonString = arguments[0]?.ToString();
        if (string.IsNullOrEmpty(jsonString))
        {
            throw new ECEngineException("JSON.parse argument must be a string", 0, 0, "", "Runtime error");
        }

        try
        {
            // Parse JSON using System.Text.Json
            var jsonDocument = JsonDocument.Parse(jsonString);
            return ConvertJsonElementToECEngineValue(jsonDocument.RootElement);
        }
        catch (JsonException ex)
        {
            throw new ECEngineException($"Invalid JSON: {ex.Message}", 0, 0, "", "SyntaxError");
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"JSON.parse error: {ex.Message}", 0, 0, "", "Runtime error");
        }
    }

    /// <summary>
    /// Converts a JsonElement to an appropriate ECEngine value
    /// </summary>
    private object? ConvertJsonElementToECEngineValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => element.TryGetDouble(out var doubleValue) ? doubleValue : element.GetDecimal(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Array => ConvertJsonArrayToList(element),
            JsonValueKind.Object => ConvertJsonObjectToDictionary(element),
            _ => throw new ECEngineException($"Unsupported JSON value kind: {element.ValueKind}", 0, 0, "", "Runtime error")
        };
    }

    /// <summary>
    /// Converts a JSON array to a List for ECEngine
    /// </summary>
    private List<object?> ConvertJsonArrayToList(JsonElement arrayElement)
    {
        var list = new List<object?>();
        foreach (var item in arrayElement.EnumerateArray())
        {
            list.Add(ConvertJsonElementToECEngineValue(item));
        }
        return list;
    }

    /// <summary>
    /// Converts a JSON object to a Dictionary for ECEngine
    /// </summary>
    private Dictionary<string, object?> ConvertJsonObjectToDictionary(JsonElement objectElement)
    {
        var dictionary = new Dictionary<string, object?>();
        foreach (var property in objectElement.EnumerateObject())
        {
            dictionary[property.Name] = ConvertJsonElementToECEngineValue(property.Value);
        }
        return dictionary;
    }
}

/// <summary>
/// JSON.stringify() function
/// Converts an ECEngine value to a JSON string
/// </summary>
public class JsonStringifyFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
        {
            return "undefined";
        }

        var value = arguments[0];
        var replacer = arguments.Count > 1 ? arguments[1] : null;
        var space = arguments.Count > 2 ? arguments[2] : null;

        try
        {
            // Use direct serialization without reflection
            return SerializeValue(value, ShouldIndent(space));
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"JSON.stringify error: {ex.Message}", 0, 0, "", "Runtime error");
        }
    }

    /// <summary>
    /// Directly serializes a value to JSON string without using System.Text.Json reflection
    /// </summary>
    private string SerializeValue(object? value, bool indent = false)
    {
        return value switch
        {
            null => "null",
            bool b => b ? "true" : "false",
            double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
            int i => i.ToString(),
            long l => l.ToString(),
            string s => $"\"{EscapeJsonString(s)}\"",
            List<object?> list => SerializeArray(list, indent),
            Dictionary<string, object?> dict => SerializeObject(dict, indent),
            DateObject dateObj => SerializeValue(ConvertDateToISOString(dateObj), indent),
            _ => SerializeObjectProperties(value, indent)
        };
    }

    /// <summary>
    /// Serializes an array to JSON string
    /// </summary>
    private string SerializeArray(List<object?> list, bool indent = false)
    {
        var items = list.Select(item => SerializeValue(item, indent));
        return $"[{string.Join(",", items)}]";
    }

    /// <summary>
    /// Serializes an object to JSON string
    /// </summary>
    private string SerializeObject(Dictionary<string, object?> dict, bool indent = false)
    {
        var properties = dict.Select(kvp => 
            $"\"{EscapeJsonString(kvp.Key)}\":{SerializeValue(kvp.Value, indent)}"
        );
        return $"{{{string.Join(",", properties)}}}";
    }

    /// <summary>
    /// Serializes object properties to JSON string (fallback for unknown objects)
    /// </summary>
    private string SerializeObjectProperties(object obj, bool indent = false)
    {
        // For unknown objects, create a simple object representation
        var typeName = obj.GetType().Name;
        return $"{{\"__type\":\"{EscapeJsonString(typeName)}\"}}";
    }

    /// <summary>
    /// Escapes a string for JSON
    /// </summary>
    private string EscapeJsonString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";

        var result = new System.Text.StringBuilder();
        
        foreach (char c in input)
        {
            switch (c)
            {
                case '\\':
                    result.Append("\\\\");
                    break;
                case '"':
                    result.Append("\\\"");
                    break;
                case '\n':
                    result.Append("\\n");
                    break;
                case '\r':
                    result.Append("\\r");
                    break;
                case '\t':
                    result.Append("\\t");
                    break;
                case '\b':
                    result.Append("\\b");
                    break;
                case '\f':
                    result.Append("\\f");
                    break;
                case '\0':
                    result.Append("\\u0000");
                    break;
                case '\v':
                    result.Append("\\u000B");
                    break;
                default:
                    // Handle other control characters that aren't valid in JSON
                    if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
                    {
                        result.Append($"\\u{(int)c:X4}");
                    }
                    else
                    {
                        result.Append(c);
                    }
                    break;
            }
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Determines if the output should be indented based on the space parameter
    /// </summary>
    private bool ShouldIndent(object? space)
    {
        if (space == null) return false;
        
        if (space is double d && d > 0) return true;
        if (space is int i && i > 0) return true;
        if (space is string s && !string.IsNullOrEmpty(s)) return true;
        
        return false;
    }

    /// <summary>
    /// Converts a DateObject to ISO string for JSON serialization
    /// </summary>
    private string ConvertDateToISOString(DateObject dateObj)
    {
        if (!dateObj.IsValid)
        {
            return "null";
        }
        
        return dateObj.DateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }
}

/// <summary>
/// Helper class for JSON method function calls
/// </summary>
public class JsonMethodFunction
{
    private readonly object _jsonFunction;
    private readonly string _methodName;

    public JsonMethodFunction(object jsonFunction, string methodName)
    {
        _jsonFunction = jsonFunction;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        return _jsonFunction switch
        {
            JsonParseFunction parse => parse.Call(arguments),
            JsonStringifyFunction stringify => stringify.Call(arguments),
            _ => throw new ECEngineException($"Unknown JSON method: {_methodName}", 0, 0, "", "Runtime error")
        };
    }
}
