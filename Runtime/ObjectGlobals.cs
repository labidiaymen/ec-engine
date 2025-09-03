using ECEngine.AST;

namespace ECEngine.Runtime;

/// <summary>
/// Global Object methods and functions for ECEngine scripts
/// </summary>
public class ObjectModule
{
    public object? keys(object? obj)
    {
        if (obj is Dictionary<string, object?> dict)
        {
            return dict.Keys.ToList<object?>();
        }
        
        if (obj == null)
        {
            throw new ECEngineException("Cannot convert undefined or null to object",
                1, 1, "", "Object.keys() requires a non-null object");
        }

        // For other objects, try to get properties using reflection
        var type = obj.GetType();
        var properties = type.GetProperties()
            .Where(p => p.CanRead)
            .Select(p => p.Name)
            .ToList<object?>();
        
        return properties;
    }

    public object? values(object? obj)
    {
        if (obj is Dictionary<string, object?> dict)
        {
            return dict.Values.ToArray();
        }
        
        if (obj == null)
        {
            throw new ECEngineException("Cannot convert undefined or null to object",
                1, 1, "", "Object.values() requires a non-null object");
        }

        // For other objects, try to get property values using reflection
        var type = obj.GetType();
        var values = type.GetProperties()
            .Where(p => p.CanRead)
            .Select(p => p.GetValue(obj))
            .ToArray();
        
        return values;
    }

    public object? entries(object? obj)
    {
        if (obj is Dictionary<string, object?> dict)
        {
            return dict.Select(kvp => new object[] { kvp.Key, kvp.Value }).ToArray();
        }
        
        if (obj == null)
        {
            throw new ECEngineException("Cannot convert undefined or null to object",
                1, 1, "", "Object.entries() requires a non-null object");
        }

        // For other objects, try to get property entries using reflection
        var type = obj.GetType();
        var entries = type.GetProperties()
            .Where(p => p.CanRead)
            .Select(p => new object[] { p.Name, p.GetValue(obj) })
            .ToArray();
        
        return entries;
    }

    public object? hasOwnProperty(object? obj, object? prop)
    {
        if (obj is Dictionary<string, object?> dict && prop is string key)
        {
            return dict.ContainsKey(key);
        }
        
        if (obj == null)
        {
            return false;
        }

        if (prop is string propName)
        {
            var type = obj.GetType();
            return type.GetProperty(propName) != null;
        }
        
        return false;
    }

    public object? assign(params object?[] args)
    {
        if (args.Length < 1)
        {
            throw new ECEngineException("Object.assign requires at least 1 argument",
                1, 1, "", "Object.assign(target, ...sources)");
        }

        var target = args[0];
        if (target == null)
        {
            throw new ECEngineException("Cannot convert undefined or null to object",
                1, 1, "", "Object.assign() target cannot be null");
        }

        if (target is not Dictionary<string, object?> targetDict)
        {
            // Convert target to dictionary if it's not already
            targetDict = new Dictionary<string, object?>();
        }

        // Copy properties from source objects
        for (int i = 1; i < args.Length; i++)
        {
            var source = args[i];
            if (source == null) continue;

            if (source is Dictionary<string, object?> sourceDict)
            {
                foreach (var kvp in sourceDict)
                {
                    targetDict[kvp.Key] = kvp.Value;
                }
            }
        }

        return targetDict;
    }

    public object? create(object? prototype, object? properties = null)
    {
        // Simple implementation - just create a new empty object
        // In a full implementation, this would set up prototype chain
        var obj = new Dictionary<string, object?>();
        
        if (properties is Dictionary<string, object?> props)
        {
            foreach (var kvp in props)
            {
                obj[kvp.Key] = kvp.Value;
            }
        }
        
        return obj;
    }

    public object? freeze(object? obj)
    {
        // In a real implementation, this would make the object immutable
        // For now, just return the object as-is
        return obj;
    }

    public object? seal(object? obj)
    {
        // In a real implementation, this would prevent adding/removing properties
        // For now, just return the object as-is
        return obj;
    }

    public object? defineProperty(object? obj, object? prop, object? descriptor)
    {
        if (obj == null)
        {
            throw new ECEngineException("Cannot call defineProperty on null or undefined",
                1, 1, "", "Object.defineProperty() requires a non-null object");
        }

        if (prop is not string propName)
        {
            throw new ECEngineException("Property name must be a string",
                1, 1, "", "Object.defineProperty() property name must be a string");
        }

        // Convert object to dictionary if it's not already
        Dictionary<string, object?> targetDict;
        if (obj is Dictionary<string, object?> dict)
        {
            targetDict = dict;
        }
        else
        {
            // For now, we'll just throw an error for non-dictionary objects
            // In a full implementation, we'd handle native objects differently
            throw new ECEngineException("Cannot define property on non-object",
                1, 1, "", "Object.defineProperty() only supports dictionary objects");
        }

        // Handle descriptor properties
        if (descriptor is Dictionary<string, object?> desc)
        {
            // Simple implementation - just set the value if provided
            if (desc.ContainsKey("value"))
            {
                targetDict[propName] = desc["value"];
            }
            
            // Note: In a full implementation, we'd handle:
            // - get/set (accessor properties)
            // - writable, enumerable, configurable flags
            // - Property descriptors and attribute enforcement
        }

        return obj;
    }
}

/// <summary>
/// Object method function wrapper for dynamic method calls
/// </summary>
public class ObjectMethodFunction
{
    private readonly ObjectModule _objectModule;
    private readonly string _methodName;

    public ObjectMethodFunction(ObjectModule objectModule, string methodName)
    {
        _objectModule = objectModule;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        return _methodName switch
        {
            "keys" => _objectModule.keys(arguments.Count > 0 ? arguments[0] : null),
            "values" => _objectModule.values(arguments.Count > 0 ? arguments[0] : null),
            "entries" => _objectModule.entries(arguments.Count > 0 ? arguments[0] : null),
            "hasOwnProperty" => _objectModule.hasOwnProperty(
                arguments.Count > 0 ? arguments[0] : null,
                arguments.Count > 1 ? arguments[1] : null),
            "assign" => _objectModule.assign(arguments.ToArray()),
            "create" => _objectModule.create(
                arguments.Count > 0 ? arguments[0] : null,
                arguments.Count > 1 ? arguments[1] : null),
            "freeze" => _objectModule.freeze(arguments.Count > 0 ? arguments[0] : null),
            "seal" => _objectModule.seal(arguments.Count > 0 ? arguments[0] : null),
            "defineProperty" => _objectModule.defineProperty(
                arguments.Count > 0 ? arguments[0] : null,
                arguments.Count > 1 ? arguments[1] : null,
                arguments.Count > 2 ? arguments[2] : null),
            _ => throw new ECEngineException($"Unknown Object method: {_methodName}",
                1, 1, "", $"Object.{_methodName} is not implemented")
        };
    }
}

/// <summary>
/// Global Object module for ECEngine scripts, providing static Object methods
/// </summary>
public class ObjectModuleClass
{
    public ObjectMethodFunction keys { get; }
    public ObjectMethodFunction values { get; }
    public ObjectMethodFunction entries { get; }
    public ObjectMethodFunction hasOwnProperty { get; }
    public ObjectMethodFunction assign { get; }
    public ObjectMethodFunction create { get; }
    public ObjectMethodFunction freeze { get; }
    public ObjectMethodFunction seal { get; }
    public ObjectMethodFunction defineProperty { get; }

    public ObjectModuleClass()
    {
        var objectModule = new ObjectModule();
        keys = new ObjectMethodFunction(objectModule, "keys");
        values = new ObjectMethodFunction(objectModule, "values");
        entries = new ObjectMethodFunction(objectModule, "entries");
        hasOwnProperty = new ObjectMethodFunction(objectModule, "hasOwnProperty");
        assign = new ObjectMethodFunction(objectModule, "assign");
        create = new ObjectMethodFunction(objectModule, "create");
        freeze = new ObjectMethodFunction(objectModule, "freeze");
        seal = new ObjectMethodFunction(objectModule, "seal");
        defineProperty = new ObjectMethodFunction(objectModule, "defineProperty");
    }

    /// <summary>
    /// Object constructor function
    /// </summary>
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            return new Dictionary<string, object?>();
        
        var value = arguments[0];
        if (value == null)
            return new Dictionary<string, object?>();
        
        // Convert primitive values to object form
        if (value is string || value is double || value is bool)
            return new Dictionary<string, object?> { ["value"] = value };
        
        return value;
    }
}
