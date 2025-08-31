using ECEngine.AST;

namespace ECEngine.Runtime;

/// <summary>
/// Represents information about a variable including its type and value
/// </summary>
public class VariableInfo
{
    public string Type { get; }
    public object? Value { get; set; }
    public bool IsConstant => Type == "const";
    public List<Function> Observers { get; } = new List<Function>();
    public List<Interpreter.MultiVariableObserver> MultiObservers { get; } = new List<Interpreter.MultiVariableObserver>();

    public VariableInfo(string type, object? value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString()
    {
        return $"{Type} = {Value ?? "undefined"}";
    }
}

/// <summary>
/// Represents a function value
/// </summary>
public class Function
{
    public string? Name { get; }
    public List<string> Parameters { get; }
    public List<Statement> Body { get; }
    public Dictionary<string, VariableInfo> Closure { get; }
    
    // Properties for observable server proxy support
    public bool IsObservableProxy { get; set; } = false;
    public object? ObservableServer { get; set; }

    public Function(string? name, List<string> parameters, List<Statement> body, Dictionary<string, VariableInfo> closure)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
        // Store reference to the closure, not a copy, to maintain variable sharing
        Closure = closure;
    }

    public override string ToString()
    {
        var functionName = Name ?? "anonymous";
        return $"function {functionName}({string.Join(", ", Parameters)}) {{ ... }}";
    }
}

/// <summary>
/// Exception thrown when a return statement is executed
/// Used for control flow in function execution
/// </summary>
public class ReturnException : Exception
{
    public object? Value { get; }

    public ReturnException(object? value)
    {
        Value = value;
    }
}
