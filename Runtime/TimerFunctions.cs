namespace ECEngine.Runtime;

/// <summary>
/// Built-in timer functions for ECEngine (setTimeout, setInterval, etc.)
/// </summary>
public class TimerFunctions
{
    private readonly EventLoop _eventLoop;

    public TimerFunctions(EventLoop eventLoop)
    {
        _eventLoop = eventLoop;
    }

    /// <summary>
    /// setTimeout implementation
    /// </summary>
    public void SetTimeout(Function callback, double delay)
    {
        var delayMs = (int)Math.Max(0, delay);
        _eventLoop.SetTimeout(() =>
        {
            try
            {
                // Call the user function with no arguments
                var interpreter = new Interpreter();
                CallUserFunction(interpreter, callback, new List<object?>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in setTimeout callback: {ex.Message}");
            }
        }, delayMs);
    }

    /// <summary>
    /// setInterval implementation
    /// </summary>
    public void SetInterval(Function callback, double interval)
    {
        var intervalMs = (int)Math.Max(1, interval); // Minimum 1ms interval
        _eventLoop.SetInterval(() =>
        {
            try
            {
                // Call the user function with no arguments
                var interpreter = new Interpreter();
                CallUserFunction(interpreter, callback, new List<object?>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in setInterval callback: {ex.Message}");
            }
        }, intervalMs);
    }

    /// <summary>
    /// nextTick implementation (like process.nextTick in Node.js)
    /// </summary>
    public void NextTick(Function callback)
    {
        _eventLoop.NextTick(() =>
        {
            try
            {
                var interpreter = new Interpreter();
                CallUserFunction(interpreter, callback, new List<object?>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in nextTick callback: {ex.Message}");
            }
        });
    }

    private object? CallUserFunction(Interpreter interpreter, Function function, List<object?> arguments)
    {
        // Create a simplified function call - this would need to be integrated
        // with the actual interpreter's function calling mechanism
        try
        {
            // For now, just execute the function body
            // In a real implementation, this would properly handle scope and parameters
            foreach (var statement in function.Body)
            {
                interpreter.Evaluate(statement);
            }
            return null;
        }
        catch (ReturnException returnEx)
        {
            return returnEx.Value;
        }
    }
}

/// <summary>
/// Global timer object for ECEngine scripts
/// </summary>
public class GlobalTimers
{
    private readonly TimerFunctions _timerFunctions;

    public GlobalTimers(EventLoop eventLoop)
    {
        _timerFunctions = new TimerFunctions(eventLoop);
    }

    public TimerFunctions Functions => _timerFunctions;
}
