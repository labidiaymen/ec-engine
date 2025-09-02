using System.Collections.Concurrent;

namespace ECEngine.Runtime.Streams;

/// <summary>
/// Callback interface for stream event handlers
/// </summary>
public interface IStreamCallback
{
    object? Call(List<object?> arguments);
}

/// <summary>
/// Wrapper to make ECEngine Function work as IStreamCallback
/// </summary>
public class ECFunctionWrapper : IStreamCallback
{
    private readonly Function _function;
    private readonly Interpreter _interpreter;
    
    public ECFunctionWrapper(Function function, Interpreter interpreter)
    {
        _function = function;
        _interpreter = interpreter;
    }
    
    public object? Call(List<object?> arguments)
    {
        if (_function != null)
        {
            return _interpreter?.CallUserFunctionPublic(_function, arguments);
        }
        return null;
    }
}

/// <summary>
/// Base stream class that all stream types inherit from
/// </summary>
public abstract class BaseStream
{
    protected readonly Dictionary<string, List<IStreamCallback>> _eventListeners = new();
    protected readonly Interpreter _interpreter;
    protected bool _destroyed = false;
    protected bool _ended = false;
    protected bool _finished = false;
    protected Exception? _error = null;
    
    // Stream state
    public bool destroyed => _destroyed;
    public bool errored => _error != null;
    public Exception? error => _error;
    
    protected BaseStream(Interpreter interpreter)
    {
        _interpreter = interpreter;
    }
    
    /// <summary>
    /// Convert callback object to IStreamCallback
    /// </summary>
    protected IStreamCallback? ConvertToCallback(object? callback)
    {
        return callback switch
        {
            Function func => new ECFunctionWrapper(func, _interpreter),
            IStreamCallback cb => cb,
            _ => null
        };
    }
    
    /// <summary>
    /// Add event listener
    /// </summary>
    public virtual object? On(List<object?> args)
    {
        if (args.Count < 2) return this;
        
        var eventName = args[0]?.ToString() ?? "";
        var listener = ConvertToCallback(args[1]);
        
        if (listener != null)
        {
            if (!_eventListeners.ContainsKey(eventName))
                _eventListeners[eventName] = new List<IStreamCallback>();
            _eventListeners[eventName].Add(listener);
        }
        return this;
    }
    
    /// <summary>
    /// Add one-time event listener
    /// </summary>
    public virtual object? Once(List<object?> args)
    {
        if (args.Count < 2) return this;
        
        var eventName = args[0]?.ToString() ?? "";
        var originalListener = ConvertToCallback(args[1]);
        
        if (originalListener != null)
        {
            // Create a wrapper that removes itself after calling
            var onceWrapper = new StreamOnceWrapperCallback(eventName, originalListener, this);
            return On(new List<object?> { eventName, onceWrapper });
        }
        return this;
    }
    
    /// <summary>
    /// Remove event listener
    /// </summary>
    public virtual object? Off(List<object?> args)
    {
        if (args.Count < 1) return this;
        
        var eventName = args[0]?.ToString() ?? "";
        
        if (args.Count == 1)
        {
            // Remove all listeners for this event
            _eventListeners.Remove(eventName);
        }
        else if (args.Count > 1 && _eventListeners.ContainsKey(eventName))
        {
            // For specific listener removal, we'll remove all for now since
            // ECEngine function equality is complex
            _eventListeners.Remove(eventName);
        }
        
        return this;
    }
    
    /// <summary>
    /// Emit an event to all registered listeners
    /// </summary>
    public void Emit(string eventName, params object?[] args)
    {
        if (_eventListeners.ContainsKey(eventName))
        {
            var listeners = _eventListeners[eventName].ToList(); // Copy to avoid modification during iteration
            foreach (var listener in listeners)
            {
                try
                {
                    listener.Call(args.ToList());
                }
                catch (Exception ex)
                {
                    // If error event fails, log to console but don't re-throw to avoid infinite loops
                    if (eventName != "error")
                    {
                        Console.WriteLine($"Error in {eventName} event listener: {ex.Message}");
                    }
                }
            }
        }
        
        // If this is an error event and no listeners, throw the error
        if (eventName == "error" && (!_eventListeners.ContainsKey("error") || _eventListeners["error"].Count == 0))
        {
            if (args.Length > 0 && args[0] is Exception exception)
                throw exception;
            throw new Exception("Unhandled stream error");
        }
    }
    
    /// <summary>
    /// Destroy the stream
    /// </summary>
    public virtual object? Destroy(List<object?> args = null)
    {
        if (_destroyed) return this;
        
        _destroyed = true;
        
        var error = args?.FirstOrDefault() as Exception;
        if (error != null)
        {
            _error = error;
            Emit("error", error);
        }
        
        Emit("close");
        return this;
    }
}

/// <summary>
/// One-time event listener wrapper
/// </summary>
public class StreamOnceWrapperCallback : IStreamCallback
{
    private readonly string _eventName;
    private readonly IStreamCallback _originalListener;
    private readonly BaseStream _stream;
    
    public StreamOnceWrapperCallback(string eventName, IStreamCallback originalListener, BaseStream stream)
    {
        _eventName = eventName;
        _originalListener = originalListener;
        _stream = stream;
    }
    
    public object? Call(List<object?> arguments)
    {
        // Remove this listener
        _stream.Off(new List<object?> { _eventName, this });
        // Call the original listener
        return _originalListener.Call(arguments);
    }
}
