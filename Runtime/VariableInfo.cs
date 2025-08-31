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

/// <summary>
/// Exception thrown when a yield statement is executed
/// Used for control flow in generator function execution
/// </summary>
public class YieldException : Exception
{
    public object? Value { get; }

    public YieldException(object? value)
    {
        Value = value;
    }
}

/// <summary>
/// Generator function instance that maintains state between yields
/// </summary>
public class Generator
{
    public string? Name { get; }
    public List<string> Parameters { get; }
    public List<Statement> Body { get; }
    public Dictionary<string, VariableInfo> Closure { get; }
    
    // Generator state
    private Interpreter? _interpreter;
    private List<object?> _arguments;
    private int _currentStatementIndex;
    private bool _isDone;
    private object? _thisContext;
    private Dictionary<string, object?> _localScope;
    
    public bool IsDone => _isDone;

    public Generator(string? name, List<string> parameters, List<Statement> body, Dictionary<string, VariableInfo> closure)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
        Closure = closure;
        _currentStatementIndex = 0;
        _isDone = false;
        _arguments = new List<object?>();
        _localScope = new Dictionary<string, object?>();
    }

    public void Initialize(Interpreter interpreter, List<object?> arguments, object? thisContext = null)
    {
        _interpreter = interpreter;
        _arguments = arguments;
        _thisContext = thisContext;
    }

    public object? Next()
    {
        if (_isDone || _interpreter == null)
        {
            return new Dictionary<string, object?> { ["value"] = null, ["done"] = true };
        }

        try
        {
            // Push new scope for generator execution
            _interpreter.PushScope();
            
            // Push 'this' context
            _interpreter.PushThisContext(_thisContext);
            
            // Add closure variables to current scope
            foreach (var variable in Closure)
            {
                _interpreter.GetScopes().Peek()[variable.Key] = variable.Value;
            }
            
            // Restore local scope variables from previous execution
            foreach (var variable in _localScope)
            {
                _interpreter.GetScopes().Peek()[variable.Key] = new VariableInfo("var", variable.Value);
            }
            
            // Bind arguments to parameters (only on first call)
            if (_currentStatementIndex == 0)
            {
                for (int i = 0; i < Parameters.Count; i++)
                {
                    var paramName = Parameters[i];
                    var paramValue = i < _arguments.Count ? _arguments[i] : null;
                    _interpreter.SetVariable(paramName, paramValue);
                }
            }
            
            // Execute statements from current position
            while (_currentStatementIndex < Body.Count)
            {
                var statement = Body[_currentStatementIndex];
                _currentStatementIndex++;
                
                try
                {
                    _interpreter.Evaluate(statement);
                }
                catch (YieldException yieldEx)
                {
                    // Save current local scope before yielding
                    SaveLocalScope();
                    // Yield encountered - return the yielded value
                    return new Dictionary<string, object?> { ["value"] = yieldEx.Value, ["done"] = false };
                }
                catch (ReturnException returnEx)
                {
                    // Return encountered - generator is done
                    _isDone = true;
                    return new Dictionary<string, object?> { ["value"] = returnEx.Value, ["done"] = true };
                }
            }
            
            // Reached end of function without return - generator is done
            _isDone = true;
            return new Dictionary<string, object?> { ["value"] = null, ["done"] = true };
        }
        finally
        {
            // Pop generator scope and this context
            _interpreter.PopScope();
            _interpreter.PopThisContext();
        }
    }

    private void SaveLocalScope()
    {
        if (_interpreter != null)
        {
            // Save the current scope variables (excluding closure variables and parameters)
            var currentScope = _interpreter.GetScopes().Peek();
            _localScope.Clear();
            
            foreach (var variable in currentScope)
            {
                // Don't save closure variables or 'this' context
                if (!Closure.ContainsKey(variable.Key) && variable.Key != "this")
                {
                    _localScope[variable.Key] = variable.Value.Value; // Get the actual value from VariableInfo
                }
            }
        }
    }

    public override string ToString()
    {
        var functionName = Name ?? "anonymous";
        return $"[Generator {functionName}]";
    }
}

/// <summary>
/// Generator function factory that creates generator instances
/// </summary>
public class GeneratorFunction
{
    public string? Name { get; }
    public List<string> Parameters { get; }
    public List<Statement> Body { get; }
    public Dictionary<string, VariableInfo> Closure { get; }

    public GeneratorFunction(string? name, List<string> parameters, List<Statement> body, Dictionary<string, VariableInfo> closure)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
        Closure = closure;
    }

    public Generator CreateGenerator()
    {
        return new Generator(Name, Parameters, Body, Closure);
    }

    public override string ToString()
    {
        var functionName = Name ?? "anonymous";
        return $"[GeneratorFunction {functionName}]";
    }
}

/// <summary>
/// Wrapper for generator methods like next()
/// </summary>
public class GeneratorMethodFunction
{
    private readonly Generator _generator;
    private readonly string _methodName;

    public GeneratorMethodFunction(Generator generator, string methodName)
    {
        _generator = generator;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        return _methodName switch
        {
            "next" => _generator.Next(),
            _ => throw new InvalidOperationException($"Unknown generator method: {_methodName}")
        };
    }

    public override string ToString()
    {
        return $"[GeneratorMethod {_methodName}]";
    }
}
