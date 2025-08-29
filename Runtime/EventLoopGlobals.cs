namespace ECEngine.Runtime;

/// <summary>
/// Global function wrappers for event loop integration
/// </summary>

public class SetTimeoutFunction
{
    private readonly EventLoop _eventLoop;
    private readonly Interpreter _interpreter;

    public SetTimeoutFunction(EventLoop eventLoop, Interpreter interpreter)
    {
        _eventLoop = eventLoop;
        _interpreter = interpreter;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
            throw new ECEngineException("setTimeout requires 2 arguments: callback and delay", 0, 0, "", "Runtime error");

        var callback = arguments[0] as Function;
        if (callback == null)
            throw new ECEngineException("setTimeout: first argument must be a function", 0, 0, "", "Runtime error");

        var delay = Convert.ToInt32(arguments[1]);
        
        var timerId = Guid.NewGuid().ToString();
        _eventLoop.SetTimeout(() => {
            try
            {
                _interpreter.CallUserFunction(callback, new List<object?>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in setTimeout callback: {ex.Message}");
            }
        }, delay);

        return timerId;
    }
}

public class SetIntervalFunction
{
    private readonly EventLoop _eventLoop;
    private readonly Interpreter _interpreter;

    public SetIntervalFunction(EventLoop eventLoop, Interpreter interpreter)
    {
        _eventLoop = eventLoop;
        _interpreter = interpreter;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
            throw new ECEngineException("setInterval requires 2 arguments: callback and interval", 0, 0, "", "Runtime error");

        var callback = arguments[0] as Function;
        if (callback == null)
            throw new ECEngineException("setInterval: first argument must be a function", 0, 0, "", "Runtime error");

        var interval = Convert.ToInt32(arguments[1]);
        
        var timerId = Guid.NewGuid().ToString();
        _eventLoop.SetInterval(() => {
            try
            {
                _interpreter.CallUserFunction(callback, new List<object?>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in setInterval callback: {ex.Message}");
            }
        }, interval);

        return timerId;
    }
}

public class ClearTimeoutFunction
{
    private readonly EventLoop _eventLoop;

    public ClearTimeoutFunction(EventLoop eventLoop)
    {
        _eventLoop = eventLoop;
    }

    public object? Call(List<object?> arguments)
    {
        // For simplicity, we'll just return null since we don't track timer IDs yet
        // In a full implementation, we'd track and cancel specific timers
        return null;
    }
}

public class ClearIntervalFunction
{
    private readonly EventLoop _eventLoop;

    public ClearIntervalFunction(EventLoop eventLoop)
    {
        _eventLoop = eventLoop;
    }

    public object? Call(List<object?> arguments)
    {
        // For simplicity, we'll just return null since we don't track timer IDs yet
        // In a full implementation, we'd track and cancel specific timers
        return null;
    }
}

public class NextTickFunction
{
    private readonly EventLoop _eventLoop;
    private readonly Interpreter _interpreter;

    public NextTickFunction(EventLoop eventLoop, Interpreter interpreter)
    {
        _eventLoop = eventLoop;
        _interpreter = interpreter;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 1)
            throw new ECEngineException("nextTick requires 1 argument: callback function", 0, 0, "", "Runtime error");

        var callback = arguments[0] as Function;
        if (callback == null)
            throw new ECEngineException("nextTick: argument must be a function", 0, 0, "", "Runtime error");

        _eventLoop.NextTick(() => {
            try
            {
                _interpreter.CallUserFunction(callback, new List<object?>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in nextTick callback: {ex.Message}");
            }
        });

        return null;
    }
}
