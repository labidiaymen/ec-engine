using System.Text;
using System.Web;

namespace ECEngine.Runtime;

/// <summary>
/// Node.js querystring module implementation
/// Provides utilities for parsing and formatting URL query strings
/// </summary>
public class QuerystringModule
{
    /// <summary>
    /// Parse a query string into an object
    /// </summary>
    /// <param name="str">The query string to parse</param>
    /// <param name="sep">The substring used to delimit key and value pairs (default: '&')</param>
    /// <param name="eq">The substring used to delimit keys and values (default: '=')</param>
    /// <param name="options">Parsing options</param>
    /// <returns>Parsed object</returns>
    public object parse(string str, string? sep = null, string? eq = null, object? options = null)
    {
        if (string.IsNullOrEmpty(str))
        {
            return new Dictionary<string, object?>();
        }

        sep ??= "&";
        eq ??= "=";
        
        var maxKeys = int.MaxValue;
        var decodeURIComponent = true;
        
        // Handle options if provided
        if (options is Dictionary<string, object?> opts)
        {
            if (opts.TryGetValue("maxKeys", out var maxKeysValue) && maxKeysValue != null)
            {
                if (double.TryParse(maxKeysValue.ToString(), out var maxKeysNum))
                {
                    maxKeys = (int)maxKeysNum;
                }
            }
            
            if (opts.TryGetValue("decodeURIComponent", out var decodeValue))
            {
                decodeURIComponent = decodeValue?.ToString()?.ToLower() != "false";
            }
        }

        var result = new Dictionary<string, object?>();
        var pairs = str.Split(new[] { sep }, StringSplitOptions.None);
        var keyCount = 0;

        foreach (var pair in pairs)
        {
            if (keyCount >= maxKeys) break;
            
            var eqIndex = pair.IndexOf(eq);
            string key, value;
            
            if (eqIndex >= 0)
            {
                key = pair.Substring(0, eqIndex);
                value = pair.Substring(eqIndex + eq.Length);
            }
            else
            {
                key = pair;
                value = "";
            }

            // Decode URI components if enabled
            if (decodeURIComponent)
            {
                try
                {
                    key = HttpUtility.UrlDecode(key);
                    value = HttpUtility.UrlDecode(value);
                }
                catch
                {
                    // If decoding fails, use original values
                }
            }

            // Handle arrays (multiple values for same key)
            if (result.ContainsKey(key))
            {
                var existing = result[key];
                if (existing is List<object?> list)
                {
                    list.Add(value);
                }
                else
                {
                    result[key] = new List<object?> { existing, value };
                }
            }
            else
            {
                result[key] = value;
            }
            
            keyCount++;
        }

        return result;
    }

    /// <summary>
    /// Serialize an object into a query string
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="sep">The substring used to delimit key and value pairs (default: '&')</param>
    /// <param name="eq">The substring used to delimit keys and values (default: '=')</param>
    /// <param name="options">Serialization options</param>
    /// <returns>Query string</returns>
    public string stringify(object? obj, string? sep = null, string? eq = null, object? options = null)
    {
        if (obj == null)
        {
            return "";
        }

        sep ??= "&";
        eq ??= "=";
        
        var encodeURIComponent = true;
        
        // Handle options if provided
        if (options is Dictionary<string, object?> opts)
        {
            if (opts.TryGetValue("encodeURIComponent", out var encodeValue))
            {
                encodeURIComponent = encodeValue?.ToString()?.ToLower() != "false";
            }
        }

        var pairs = new List<string>();

        if (obj is Dictionary<string, object?> dict)
        {
            foreach (var kvp in dict)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                if (encodeURIComponent)
                {
                    key = HttpUtility.UrlEncode(key);
                }

                if (value is List<object?> list)
                {
                    // Handle arrays
                    foreach (var item in list)
                    {
                        var itemStr = item?.ToString() ?? "";
                        if (encodeURIComponent)
                        {
                            itemStr = HttpUtility.UrlEncode(itemStr);
                        }
                        pairs.Add($"{key}{eq}{itemStr}");
                    }
                }
                else
                {
                    var valueStr = value?.ToString() ?? "";
                    if (encodeURIComponent)
                    {
                        valueStr = HttpUtility.UrlEncode(valueStr);
                    }
                    pairs.Add($"{key}{eq}{valueStr}");
                }
            }
        }

        return string.Join(sep, pairs);
    }

    /// <summary>
    /// Escape a string for use in a query string
    /// </summary>
    /// <param name="str">The string to escape</param>
    /// <returns>Escaped string</returns>
    public string escape(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return "";
        }
        
        return HttpUtility.UrlEncode(str);
    }

    /// <summary>
    /// Unescape a query string component
    /// </summary>
    /// <param name="str">The string to unescape</param>
    /// <returns>Unescaped string</returns>
    public string unescape(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return "";
        }
        
        return HttpUtility.UrlDecode(str);
    }
}

/// <summary>
/// Function wrapper for querystring module methods
/// </summary>
public class QuerystringMethodFunction
{
    private readonly QuerystringModule _module;
    private readonly string _methodName;

    public QuerystringMethodFunction(QuerystringModule module, string methodName)
    {
        _module = module;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        return _methodName switch
        {
            "parse" => _module.parse(
                arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : "",
                arguments.Count > 1 ? arguments[1]?.ToString() : null,
                arguments.Count > 2 ? arguments[2]?.ToString() : null,
                arguments.Count > 3 ? arguments[3] : null
            ),
            "stringify" => _module.stringify(
                arguments.Count > 0 ? arguments[0] : null,
                arguments.Count > 1 ? arguments[1]?.ToString() : null,
                arguments.Count > 2 ? arguments[2]?.ToString() : null,
                arguments.Count > 3 ? arguments[3] : null
            ),
            "escape" => _module.escape(
                arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : ""
            ),
            "unescape" => _module.unescape(
                arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : ""
            ),
            _ => throw new ECEngineException($"Unknown querystring method: {_methodName}", 1, 1, "", "Runtime error")
        };
    }
}