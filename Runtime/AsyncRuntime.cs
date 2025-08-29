using ECEngine.AST;
using ECEngine.Parser;
using ECEngine.Lexer;

namespace ECEngine.Runtime;

/// <summary>
/// Asynchronous runtime that integrates event loop for handling async operations
/// </summary>
public class AsyncRuntime
{
    private readonly EventLoop _eventLoop;
    private readonly Interpreter _interpreter;
    private readonly ECEngine.Parser.Parser _parser;

    public AsyncRuntime()
    {
        _eventLoop = new EventLoop();
        _interpreter = new Interpreter();
        _parser = new ECEngine.Parser.Parser();
    }

    /// <summary>
    /// Execute code with event loop support
    /// </summary>
    public object? Execute(string code)
    {
        try
        {
            // Parse and execute the main code
            var ast = _parser.Parse(code);
            var result = _interpreter.Evaluate(ast, code);
            
            // Run the event loop to handle any async operations
            _eventLoop.Run();
            
            return result;
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Execution error: {ex.Message}", 0, 0, code, ex.Message);
        }
    }

    /// <summary>
    /// Schedule a task for the next tick
    /// </summary>
    public void NextTick(Action task)
    {
        _eventLoop.NextTick(task);
    }

    /// <summary>
    /// Schedule a timeout (like setTimeout in JavaScript)
    /// </summary>
    public void SetTimeout(Action task, int delayMs)
    {
        _eventLoop.SetTimeout(task, delayMs);
    }

    /// <summary>
    /// Schedule an interval (like setInterval in JavaScript)
    /// </summary>
    public void SetInterval(Action task, int intervalMs)
    {
        _eventLoop.SetInterval(task, intervalMs);
    }

    /// <summary>
    /// Stop the event loop
    /// </summary>
    public void Stop()
    {
        _eventLoop.Stop();
    }

    /// <summary>
    /// Check if there's pending work
    /// </summary>
    public bool HasPendingWork()
    {
        return _eventLoop.HasPendingWork();
    }

    /// <summary>
    /// Get the interpreter for accessing variables
    /// </summary>
    public Interpreter GetInterpreter() => _interpreter;

    /// <summary>
    /// Get the event loop for advanced usage
    /// </summary>
    public EventLoop GetEventLoop() => _eventLoop;
}
