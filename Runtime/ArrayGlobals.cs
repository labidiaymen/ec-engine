using ECEngine.AST;

namespace ECEngine.Runtime;

/// <summary>
/// Array object implementation for ECEngine scripts
/// Provides JavaScript-like array functionality
/// </summary>
public class ArrayMethodFunction
{
    private readonly List<object?> _array;
    private readonly string _methodName;

    public ArrayMethodFunction(List<object?> array, string methodName)
    {
        _array = array;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        return _methodName switch
        {
            "push" => Push(arguments),
            "pop" => Pop(),
            "shift" => Shift(),
            "unshift" => Unshift(arguments),
            "slice" => Slice(arguments),
            "splice" => Splice(arguments),
            "join" => Join(arguments),
            "concat" => Concat(arguments),
            "indexOf" => IndexOf(arguments),
            "lastIndexOf" => LastIndexOf(arguments),
            "reverse" => Reverse(),
            "sort" => Sort(arguments),
            _ => throw new ECEngineException($"Array method {_methodName} not implemented",
                1, 1, "", $"The method '{_methodName}' is not available on arrays")
        };
    }

    private object? Push(List<object?> arguments)
    {
        foreach (var arg in arguments)
        {
            _array.Add(arg);
        }
        return (double)_array.Count; // Return new length
    }

    private object? Pop()
    {
        if (_array.Count == 0)
        {
            return null; // undefined in JavaScript
        }
        var lastElement = _array[_array.Count - 1];
        _array.RemoveAt(_array.Count - 1);
        return lastElement;
    }

    private object? Shift()
    {
        if (_array.Count == 0)
        {
            return null; // undefined in JavaScript
        }
        var firstElement = _array[0];
        _array.RemoveAt(0);
        return firstElement;
    }

    private object? Unshift(List<object?> arguments)
    {
        for (int i = arguments.Count - 1; i >= 0; i--)
        {
            _array.Insert(0, arguments[i]);
        }
        return (double)_array.Count; // Return new length
    }

    private object? Slice(List<object?> arguments)
    {
        var start = 0;
        var end = _array.Count;

        if (arguments.Count > 0 && arguments[0] is double startValue)
        {
            start = (int)startValue;
            if (start < 0) start = Math.Max(0, _array.Count + start);
        }

        if (arguments.Count > 1 && arguments[1] is double endValue)
        {
            end = (int)endValue;
            if (end < 0) end = Math.Max(0, _array.Count + end);
        }

        end = Math.Min(end, _array.Count);
        start = Math.Min(start, end);

        var result = new List<object?>();
        for (int i = start; i < end; i++)
        {
            result.Add(_array[i]);
        }
        return result;
    }

    private object? Splice(List<object?> arguments)
    {
        if (arguments.Count == 0) return new List<object?>();

        var start = arguments[0] is double startValue ? (int)startValue : 0;
        if (start < 0) start = Math.Max(0, _array.Count + start);
        start = Math.Min(start, _array.Count);

        var deleteCount = arguments.Count > 1 && arguments[1] is double deleteValue 
            ? (int)deleteValue 
            : _array.Count - start;
        deleteCount = Math.Max(0, Math.Min(deleteCount, _array.Count - start));

        // Remove elements and store them
        var removed = new List<object?>();
        for (int i = 0; i < deleteCount; i++)
        {
            if (start < _array.Count)
            {
                removed.Add(_array[start]);
                _array.RemoveAt(start);
            }
        }

        // Insert new elements
        for (int i = 2; i < arguments.Count; i++)
        {
            _array.Insert(start + i - 2, arguments[i]);
        }

        return removed;
    }

    private object? Join(List<object?> arguments)
    {
        var separator = arguments.Count > 0 ? arguments[0]?.ToString() ?? "," : ",";
        return string.Join(separator, _array.Select(x => x?.ToString() ?? ""));
    }

    private object? Concat(List<object?> arguments)
    {
        var result = new List<object?>(_array);
        
        foreach (var arg in arguments)
        {
            if (arg is List<object?> otherArray)
            {
                result.AddRange(otherArray);
            }
            else
            {
                result.Add(arg);
            }
        }
        
        return result;
    }

    private object? IndexOf(List<object?> arguments)
    {
        if (arguments.Count == 0) return -1.0;

        var searchElement = arguments[0];
        var fromIndex = arguments.Count > 1 && arguments[1] is double from ? (int)from : 0;
        
        if (fromIndex < 0) fromIndex = Math.Max(0, _array.Count + fromIndex);

        for (int i = fromIndex; i < _array.Count; i++)
        {
            if (AreEqual(_array[i], searchElement))
            {
                return (double)i;
            }
        }
        
        return -1.0; // Not found
    }

    private object? LastIndexOf(List<object?> arguments)
    {
        if (arguments.Count == 0) return -1.0;

        var searchElement = arguments[0];
        var fromIndex = arguments.Count > 1 && arguments[1] is double from 
            ? (int)from 
            : _array.Count - 1;
        
        if (fromIndex < 0) fromIndex = _array.Count + fromIndex;
        if (fromIndex >= _array.Count) fromIndex = _array.Count - 1;

        for (int i = fromIndex; i >= 0; i--)
        {
            if (AreEqual(_array[i], searchElement))
            {
                return (double)i;
            }
        }
        
        return -1.0; // Not found
    }

    private object? Reverse()
    {
        _array.Reverse();
        return _array; // Return the array itself
    }

    private object? Sort(List<object?> arguments)
    {
        if (arguments.Count > 0 && arguments[0] is Function compareFn)
        {
            // Custom comparison function - not implemented for now
            throw new ECEngineException("Custom sort comparison functions not yet implemented",
                1, 1, "", "Array.sort() with custom comparison function is not supported yet");
        }
        else
        {
            // Default sort: convert to strings and sort alphabetically
            _array.Sort((a, b) => 
            {
                var aStr = a?.ToString() ?? "";
                var bStr = b?.ToString() ?? "";
                return string.Compare(aStr, bStr, StringComparison.Ordinal);
            });
        }
        
        return _array; // Return the array itself
    }

    private bool AreEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        
        // Handle numeric comparison
        if (a is double ad && b is double bd) return ad == bd;
        
        // Handle string comparison
        if (a is string as1 && b is string bs) return as1 == bs;
        
        // Handle boolean comparison
        if (a is bool ab && b is bool bb) return ab == bb;
        
        // Default object comparison
        return a.Equals(b);
    }
}
