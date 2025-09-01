using ECEngine.AST;

namespace ECEngine.Runtime;

/// <summary>
/// EventEmitter implementation for ECEngine using object factory pattern
/// Provides Node.js-style event emission and listening capabilities
/// </summary>
public class EventEmitterModule
{
    /// <summary>
    /// Creates a new EventEmitter instance using object factory pattern
    /// </summary>
    public object? createEventEmitter()
    {
        // Create the events storage as a dictionary
        var events = new Dictionary<string, List<Function>>();
        
        // Create the EventEmitter object with methods
        var eventEmitter = new Dictionary<string, object?>
        {
            ["on"] = new EventEmitterOnFunction(events),
            ["addListener"] = new EventEmitterOnFunction(events), // Alias for on()
            ["emit"] = new EventEmitterEmitFunction(events),
            ["off"] = new EventEmitterOffFunction(events),
            ["removeListener"] = new EventEmitterOffFunction(events), // Alias for off()
            ["removeAllListeners"] = new EventEmitterRemoveAllFunction(events),
            ["listenerCount"] = new EventEmitterListenerCountFunction(events),
            ["listeners"] = new EventEmitterListenersFunction(events),
            ["eventNames"] = new EventEmitterEventNamesFunction(events)
        };
        
        return eventEmitter;
    }
}

/// <summary>
/// Helper function for EventEmitter.createEventEmitter() method calls
/// </summary>
public class EventEmitterCreateFunction
{
    private readonly EventEmitterModule _eventEmitterModule;

    public EventEmitterCreateFunction(EventEmitterModule eventEmitterModule)
    {
        _eventEmitterModule = eventEmitterModule;
    }

    public object? Call(List<object?> arguments)
    {
        return _eventEmitterModule.createEventEmitter();
    }
}

/// <summary>
/// EventEmitter.on() method implementation
/// </summary>
public class EventEmitterOnFunction
{
    private readonly Dictionary<string, List<Function>> _events;

    public EventEmitterOnFunction(Dictionary<string, List<Function>> events)
    {
        _events = events;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
        {
            throw new ECEngineException("EventEmitter.on() requires 2 arguments: event name and listener function",
                1, 1, "", "Usage: emitter.on('event', function() { ... })");
        }

        var eventName = arguments[0]?.ToString() ?? "";
        var listener = arguments[1];

        if (listener is not Function listenerFunc)
        {
            throw new ECEngineException("EventEmitter listener must be a function",
                1, 1, "", "The second argument to .on() must be a function");
        }

        // Add listener to the event
        if (!_events.ContainsKey(eventName))
        {
            _events[eventName] = new List<Function>();
        }
        
        _events[eventName].Add(listenerFunc);
        
        // Return the EventEmitter for chaining (return this)
        return null; // For now, we don't support chaining
    }
}

/// <summary>
/// EventEmitter.emit() method implementation
/// </summary>
public class EventEmitterEmitFunction
{
    private readonly Dictionary<string, List<Function>> _events;

    public EventEmitterEmitFunction(Dictionary<string, List<Function>> events)
    {
        _events = events;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 1)
        {
            throw new ECEngineException("EventEmitter.emit() requires at least 1 argument: event name",
                1, 1, "", "Usage: emitter.emit('event', arg1, arg2, ...)");
        }

        var eventName = arguments[0]?.ToString() ?? "";
        var eventArgs = arguments.Skip(1).ToList();

        // Check if there are listeners for this event
        if (!_events.ContainsKey(eventName))
        {
            return false; // No listeners
        }

        var listeners = _events[eventName];
        if (listeners.Count == 0)
        {
            return false; // No listeners
        }

        // Call all listeners with the provided arguments
        // Note: The interpreter will need to handle this properly
        // For now, we'll store the event for later processing
        foreach (var listener in listeners.ToList()) // ToList() to avoid modification during iteration
        {
            try
            {
                // The interpreter will need to call the user function
                // This will be handled by the interpreter's function calling mechanism
                // For now, we just acknowledge the listener exists
            }
            catch (Exception ex)
            {
                // In Node.js, EventEmitter errors are handled specially
                // For now, we'll just continue with other listeners
                Console.WriteLine($"Warning: EventEmitter listener error: {ex.Message}");
            }
        }

        return true; // Had listeners
    }

    // Helper method for the interpreter to get event data
    public (string eventName, List<object?> eventArgs, List<Function> listeners) GetEventData(List<object?> arguments)
    {
        var eventName = arguments[0]?.ToString() ?? "";
        var eventArgs = arguments.Skip(1).ToList();
        var listeners = _events.ContainsKey(eventName) ? _events[eventName] : new List<Function>();
        return (eventName, eventArgs, listeners);
    }
}

/// <summary>
/// EventEmitter.off() / removeListener() method implementation
/// </summary>
public class EventEmitterOffFunction
{
    private readonly Dictionary<string, List<Function>> _events;

    public EventEmitterOffFunction(Dictionary<string, List<Function>> events)
    {
        _events = events;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
        {
            throw new ECEngineException("EventEmitter.off() requires 2 arguments: event name and listener function",
                1, 1, "", "Usage: emitter.off('event', listenerFunction)");
        }

        var eventName = arguments[0]?.ToString() ?? "";
        var listener = arguments[1];

        if (listener is not Function listenerFunc)
        {
            throw new ECEngineException("EventEmitter listener must be a function",
                1, 1, "", "The second argument to .off() must be a function");
        }

        // Remove the specific listener
        if (_events.ContainsKey(eventName))
        {
            _events[eventName].Remove(listenerFunc);
            
            // Clean up empty event arrays
            if (_events[eventName].Count == 0)
            {
                _events.Remove(eventName);
            }
        }

        return null;
    }
}

/// <summary>
/// EventEmitter.removeAllListeners() method implementation
/// </summary>
public class EventEmitterRemoveAllFunction
{
    private readonly Dictionary<string, List<Function>> _events;

    public EventEmitterRemoveAllFunction(Dictionary<string, List<Function>> events)
    {
        _events = events;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
        {
            // Remove all listeners for all events
            _events.Clear();
        }
        else
        {
            // Remove all listeners for specific event
            var eventName = arguments[0]?.ToString() ?? "";
            _events.Remove(eventName);
        }

        return null;
    }
}

/// <summary>
/// EventEmitter.listenerCount() method implementation
/// </summary>
public class EventEmitterListenerCountFunction
{
    private readonly Dictionary<string, List<Function>> _events;

    public EventEmitterListenerCountFunction(Dictionary<string, List<Function>> events)
    {
        _events = events;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 1)
        {
            return 0.0;
        }

        var eventName = arguments[0]?.ToString() ?? "";
        
        if (_events.ContainsKey(eventName))
        {
            return (double)_events[eventName].Count;
        }

        return 0.0;
    }
}

/// <summary>
/// EventEmitter.listeners() method implementation
/// </summary>
public class EventEmitterListenersFunction
{
    private readonly Dictionary<string, List<Function>> _events;

    public EventEmitterListenersFunction(Dictionary<string, List<Function>> events)
    {
        _events = events;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 1)
        {
            return new List<object?>();
        }

        var eventName = arguments[0]?.ToString() ?? "";
        
        if (_events.ContainsKey(eventName))
        {
            return _events[eventName].Cast<object?>().ToList();
        }

        return new List<object?>();
    }
}

/// <summary>
/// EventEmitter.eventNames() method implementation
/// </summary>
public class EventEmitterEventNamesFunction
{
    private readonly Dictionary<string, List<Function>> _events;

    public EventEmitterEventNamesFunction(Dictionary<string, List<Function>> events)
    {
        _events = events;
    }

    public object? Call(List<object?> arguments)
    {
        return _events.Keys.Cast<object?>().ToList();
    }
}
