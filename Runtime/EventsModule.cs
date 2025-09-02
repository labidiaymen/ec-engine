using ECEngine.AST;

namespace ECEngine.Runtime;

/// <summary>
/// Node.js-compatible events module implementation
/// Provides EventEmitter class and utility functions following Node.js events API
/// https://nodejs.org/api/events.html
/// </summary>
public class EventsModule
{
    private static int _defaultMaxListeners = 10;
    private static readonly string _errorMonitor = "Symbol(events.errorMonitor)";

    /// <summary>
    /// Gets the complete events module exports
    /// </summary>
    public static Dictionary<string, object?> GetEventsModule()
    {
        return new Dictionary<string, object?>
        {
            // Main EventEmitter class constructor
            ["EventEmitter"] = new EventEmitterConstructor(),
            
            // Static utility functions
            ["once"] = new EventsOnceFunction(),
            ["listenerCount"] = new EventsListenerCountFunction(),
            ["getEventListeners"] = new EventsGetEventListenersFunction(),
            ["getMaxListeners"] = new EventsGetMaxListenersFunction(),
            ["setMaxListeners"] = new EventsSetMaxListenersFunction(),
            ["addAbortListener"] = new EventsAddAbortListenerFunction(),
            
            // Properties
            ["defaultMaxListeners"] = (double)_defaultMaxListeners,
            ["errorMonitor"] = _errorMonitor,
            ["captureRejections"] = false,
            ["captureRejectionSymbol"] = "Symbol(nodejs.rejection)",
            
            // Default export (for CommonJS compatibility)
            ["default"] = new EventEmitterConstructor()
        };
    }

    /// <summary>
    /// Updates the default max listeners value
    /// </summary>
    public static void SetDefaultMaxListeners(int value)
    {
        _defaultMaxListeners = value;
    }

    /// <summary>
    /// Gets the default max listeners value
    /// </summary>
    public static int GetDefaultMaxListeners()
    {
        return _defaultMaxListeners;
    }
}

/// <summary>
/// EventEmitter constructor function - behaves like Node.js EventEmitter class
/// Can be called with 'new EventEmitter()' or as a function
/// </summary>
public class EventEmitterConstructor
{
    public object? Call(List<object?> arguments)
    {
        // Extract options if provided
        var options = arguments.Count > 0 && arguments[0] is Dictionary<string, object?> opts ? opts : null;
        
        var captureRejections = false;
        if (options?.ContainsKey("captureRejections") == true && options["captureRejections"] is bool cr)
        {
            captureRejections = cr;
        }

        return CreateEventEmitterInstance(captureRejections);
    }

    public static Dictionary<string, object?> CreateEventEmitterInstance(bool captureRejections = false)
    {
        // Create the events storage
        var events = new Dictionary<string, List<EventListenerInfo>>();
        var maxListeners = EventsModule.GetDefaultMaxListeners();
        
        // Create the EventEmitter instance with all Node.js methods
        var eventEmitter = new Dictionary<string, object?>
        {
            // Internal properties
            ["_events"] = events,
            ["_maxListeners"] = maxListeners,
            ["_captureRejections"] = captureRejections
        };

        // Add methods that don't need self-reference
        eventEmitter["listenerCount"] = new NodeEventEmitterListenerCountFunction(events);
        eventEmitter["listeners"] = new NodeEventEmitterListenersFunction(events);
        eventEmitter["rawListeners"] = new NodeEventEmitterRawListenersFunction(events);
        eventEmitter["eventNames"] = new NodeEventEmitterEventNamesFunction(events);
        eventEmitter["getMaxListeners"] = new NodeEventEmitterGetMaxListenersFunction(events);

        // Add methods that need self-reference
        eventEmitter["on"] = new NodeEventEmitterOnFunction(events, eventEmitter);
        eventEmitter["addListener"] = new NodeEventEmitterOnFunction(events, eventEmitter); // Alias for on()
        eventEmitter["once"] = new NodeEventEmitterOnceFunction(events, eventEmitter);
        eventEmitter["emit"] = new NodeEventEmitterEmitFunction(events, eventEmitter, captureRejections);
        eventEmitter["off"] = new NodeEventEmitterOffFunction(events, eventEmitter);
        eventEmitter["removeListener"] = new NodeEventEmitterOffFunction(events, eventEmitter); // Alias for off()
        eventEmitter["removeAllListeners"] = new NodeEventEmitterRemoveAllListenersFunction(events, eventEmitter);
        eventEmitter["prependListener"] = new NodeEventEmitterPrependListenerFunction(events, eventEmitter);
        eventEmitter["prependOnceListener"] = new NodeEventEmitterPrependOnceListenerFunction(events, eventEmitter);
        eventEmitter["setMaxListeners"] = new NodeEventEmitterSetMaxListenersFunction(events, eventEmitter);

        return eventEmitter;
    }
}

/// <summary>
/// Represents a listener with metadata
/// </summary>
public class EventListenerInfo
{
    public Function Listener { get; set; }
    public bool Once { get; set; }
    public Function? OriginalListener { get; set; } // For once() wrapping

    public EventListenerInfo(Function listener, bool once = false, Function? originalListener = null)
    {
        Listener = listener;
        Once = once;
        OriginalListener = originalListener;
    }
}

/// <summary>
/// Node.js EventEmitter.on() method with chaining and newListener event
/// </summary>
public class NodeEventEmitterOnFunction
{
    private readonly Dictionary<string, List<EventListenerInfo>> _events;
    private readonly Dictionary<string, object?> _emitterInstance;

    public NodeEventEmitterOnFunction(Dictionary<string, List<EventListenerInfo>> events, Dictionary<string, object?> emitterInstance)
    {
        _events = events;
        _emitterInstance = emitterInstance;
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

        // Emit newListener event before adding
        EmitNewListenerEvent(eventName, listenerFunc);

        // Add listener to the event
        if (!_events.ContainsKey(eventName))
        {
            _events[eventName] = new List<EventListenerInfo>();
        }
        
        _events[eventName].Add(new EventListenerInfo(listenerFunc));
        
        // Check max listeners warning
        CheckMaxListeners(eventName);
        
        // Return the EventEmitter for chaining
        return _emitterInstance;
    }

    private void EmitNewListenerEvent(string eventName, Function listener)
    {
        if (_events.ContainsKey("newListener") && _events["newListener"].Count > 0)
        {
            // Create arguments for newListener event
            var newListenerArgs = new List<object?> { "newListener", eventName, listener };
            
            // Get emit function and call it
            if (_emitterInstance.TryGetValue("emit", out var emitFunc) && emitFunc is NodeEventEmitterEmitFunction emitter)
            {
                emitter.Call(newListenerArgs);
            }
        }
    }

    private void CheckMaxListeners(string eventName)
    {
        if (_events.ContainsKey(eventName))
        {
            var count = _events[eventName].Count;
            var maxListeners = GetMaxListeners();
            
            if (maxListeners > 0 && count > maxListeners)
            {
                Console.WriteLine($"(node) warning: possible EventEmitter memory leak detected. {count} {eventName} listeners added. Use emitter.setMaxListeners() to increase limit.");
            }
        }
    }

    private int GetMaxListeners()
    {
        if (_emitterInstance.TryGetValue("_maxListeners", out var max) && max is int maxInt)
        {
            return maxInt;
        }
        return EventsModule.GetDefaultMaxListeners();
    }
}

/// <summary>
/// EventEmitter.once() method implementation
/// </summary>
public class NodeEventEmitterOnceFunction
{
    private readonly Dictionary<string, List<EventListenerInfo>> _events;
    private readonly Dictionary<string, object?> _emitterInstance;

    public NodeEventEmitterOnceFunction(Dictionary<string, List<EventListenerInfo>> events, Dictionary<string, object?> emitterInstance)
    {
        _events = events;
        _emitterInstance = emitterInstance;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
        {
            throw new ECEngineException("EventEmitter.once() requires 2 arguments: event name and listener function",
                1, 1, "", "Usage: emitter.once('event', function() { ... })");
        }

        var eventName = arguments[0]?.ToString() ?? "";
        var listener = arguments[1];

        if (listener is not Function listenerFunc)
        {
            throw new ECEngineException("EventEmitter listener must be a function",
                1, 1, "", "The second argument to .once() must be a function");
        }

        // Create a wrapped function that removes itself after execution
        // In practice, the interpreter would handle this during emit
        // For now, we mark it as a once listener
        
        if (!_events.ContainsKey(eventName))
        {
            _events[eventName] = new List<EventListenerInfo>();
        }
        
        _events[eventName].Add(new EventListenerInfo(listenerFunc, once: true, originalListener: listenerFunc));
        
        // Return the EventEmitter for chaining
        return _emitterInstance;
    }
}

/// <summary>
/// Enhanced EventEmitter.emit() method with proper listener handling
/// </summary>
public class NodeEventEmitterEmitFunction
{
    private readonly Dictionary<string, List<EventListenerInfo>> _events;
    private readonly Dictionary<string, object?> _emitterInstance;
    private readonly bool _captureRejections;

    public NodeEventEmitterEmitFunction(Dictionary<string, List<EventListenerInfo>> events, Dictionary<string, object?> emitterInstance, bool captureRejections = false)
    {
        _events = events;
        _emitterInstance = emitterInstance;
        _captureRejections = captureRejections;
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

        // Special handling for 'error' events
        if (eventName == "error" && (!_events.ContainsKey("error") || _events["error"].Count == 0))
        {
            // If no error listeners and it's an error event, throw the error
            var error = eventArgs.Count > 0 ? eventArgs[0] : new ECEngineException("Unhandled 'error' event", 1, 1, "", "");
            throw new ECEngineException($"Unhandled 'error' event: {error}", 1, 1, "", "Add an error event listener to handle this event");
        }

        // Check if there are listeners for this event
        if (!_events.ContainsKey(eventName) || _events[eventName].Count == 0)
        {
            return false; // No listeners
        }

        var listeners = _events[eventName].ToList(); // Copy to avoid modification during iteration
        var hadListeners = listeners.Count > 0;

        // Execute all listeners
        var listenersToRemove = new List<EventListenerInfo>();
        
        foreach (var listenerInfo in listeners)
        {
            try
            {
                // The interpreter will handle actual function execution
                // Mark once listeners for removal
                if (listenerInfo.Once)
                {
                    listenersToRemove.Add(listenerInfo);
                }
            }
            catch (Exception ex)
            {
                if (_captureRejections)
                {
                    // Handle promise rejections
                    var rejectionArgs = new List<object?> { "error", ex };
                    Call(rejectionArgs);
                }
                else
                {
                    // Continue with other listeners
                    Console.WriteLine($"Warning: EventEmitter listener error: {ex.Message}");
                }
            }
        }

        // Remove once listeners
        foreach (var listenerToRemove in listenersToRemove)
        {
            _events[eventName].Remove(listenerToRemove);
        }

        // Clean up empty event arrays
        if (_events[eventName].Count == 0)
        {
            _events.Remove(eventName);
        }

        return hadListeners;
    }

    // Helper method for the interpreter to get event data
    public (string eventName, List<object?> eventArgs, List<EventListenerInfo> listeners) GetEventData(List<object?> arguments)
    {
        var eventName = arguments[0]?.ToString() ?? "";
        var eventArgs = arguments.Skip(1).ToList();
        var listeners = _events.ContainsKey(eventName) ? _events[eventName] : new List<EventListenerInfo>();
        return (eventName, eventArgs, listeners);
    }

    // Helper method for the interpreter to access internal data structures
    public (Dictionary<string, List<EventListenerInfo>> events, Dictionary<string, object?> emitterInstance, bool captureRejections) GetEventData()
    {
        return (_events, _emitterInstance, _captureRejections);
    }
}

/// <summary>
/// EventEmitter.prependListener() method implementation
/// </summary>
public class NodeEventEmitterPrependListenerFunction
{
    private readonly Dictionary<string, List<EventListenerInfo>> _events;
    private readonly Dictionary<string, object?> _emitterInstance;

    public NodeEventEmitterPrependListenerFunction(Dictionary<string, List<EventListenerInfo>> events, Dictionary<string, object?> emitterInstance)
    {
        _events = events;
        _emitterInstance = emitterInstance;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
        {
            throw new ECEngineException("EventEmitter.prependListener() requires 2 arguments: event name and listener function",
                1, 1, "", "Usage: emitter.prependListener('event', function() { ... })");
        }

        var eventName = arguments[0]?.ToString() ?? "";
        var listener = arguments[1];

        if (listener is not Function listenerFunc)
        {
            throw new ECEngineException("EventEmitter listener must be a function",
                1, 1, "", "The second argument to .prependListener() must be a function");
        }

        // Add listener to the beginning of the event array
        if (!_events.ContainsKey(eventName))
        {
            _events[eventName] = new List<EventListenerInfo>();
        }
        
        _events[eventName].Insert(0, new EventListenerInfo(listenerFunc));
        
        // Return the EventEmitter for chaining
        return _emitterInstance;
    }
}

/// <summary>
/// EventEmitter.prependOnceListener() method implementation
/// </summary>
public class NodeEventEmitterPrependOnceListenerFunction
{
    private readonly Dictionary<string, List<EventListenerInfo>> _events;
    private readonly Dictionary<string, object?> _emitterInstance;

    public NodeEventEmitterPrependOnceListenerFunction(Dictionary<string, List<EventListenerInfo>> events, Dictionary<string, object?> emitterInstance)
    {
        _events = events;
        _emitterInstance = emitterInstance;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
        {
            throw new ECEngineException("EventEmitter.prependOnceListener() requires 2 arguments: event name and listener function",
                1, 1, "", "Usage: emitter.prependOnceListener('event', function() { ... })");
        }

        var eventName = arguments[0]?.ToString() ?? "";
        var listener = arguments[1];

        if (listener is not Function listenerFunc)
        {
            throw new ECEngineException("EventEmitter listener must be a function",
                1, 1, "", "The second argument to .prependOnceListener() must be a function");
        }

        // Add once listener to the beginning of the event array
        if (!_events.ContainsKey(eventName))
        {
            _events[eventName] = new List<EventListenerInfo>();
        }
        
        _events[eventName].Insert(0, new EventListenerInfo(listenerFunc, once: true, originalListener: listenerFunc));
        
        // Return the EventEmitter for chaining
        return _emitterInstance;
    }
}

/// <summary>
/// Enhanced EventEmitter.off() / removeListener() method
/// </summary>
public class NodeEventEmitterOffFunction
{
    private readonly Dictionary<string, List<EventListenerInfo>> _events;
    private readonly Dictionary<string, object?> _emitterInstance;

    public NodeEventEmitterOffFunction(Dictionary<string, List<EventListenerInfo>> events, Dictionary<string, object?> emitterInstance)
    {
        _events = events;
        _emitterInstance = emitterInstance;
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

        // Remove the specific listener (most recent instance)
        if (_events.ContainsKey(eventName))
        {
            var listeners = _events[eventName];
            
            // Find the listener to remove (from end to beginning to remove most recent)
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                var listenerInfo = listeners[i];
                
                // Check if this is the listener we want to remove
                // For once listeners, check both the wrapper and original
                if (listenerInfo.Listener == listenerFunc || 
                    (listenerInfo.OriginalListener != null && listenerInfo.OriginalListener == listenerFunc))
                {
                    listeners.RemoveAt(i);
                    
                    // Emit removeListener event
                    EmitRemoveListenerEvent(eventName, listenerFunc);
                    break;
                }
            }
            
            // Clean up empty event arrays
            if (listeners.Count == 0)
            {
                _events.Remove(eventName);
            }
        }

        // Return the EventEmitter for chaining
        return _emitterInstance;
    }

    private void EmitRemoveListenerEvent(string eventName, Function listener)
    {
        if (_events.ContainsKey("removeListener") && _events["removeListener"].Count > 0)
        {
            // Create arguments for removeListener event
            var removeListenerArgs = new List<object?> { "removeListener", eventName, listener };
            
            // Get emit function and call it
            if (_emitterInstance.TryGetValue("emit", out var emitFunc) && emitFunc is NodeEventEmitterEmitFunction emitter)
            {
                emitter.Call(removeListenerArgs);
            }
        }
    }
}

/// <summary>
/// Enhanced EventEmitter.removeAllListeners() method
/// </summary>
public class NodeEventEmitterRemoveAllListenersFunction
{
    private readonly Dictionary<string, List<EventListenerInfo>> _events;
    private readonly Dictionary<string, object?> _emitterInstance;

    public NodeEventEmitterRemoveAllListenersFunction(Dictionary<string, List<EventListenerInfo>> events, Dictionary<string, object?> emitterInstance)
    {
        _events = events;
        _emitterInstance = emitterInstance;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
        {
            // Remove all listeners for all events
            var allEventNames = _events.Keys.ToList();
            foreach (var eventName in allEventNames)
            {
                var listeners = _events[eventName].ToList();
                foreach (var listenerInfo in listeners)
                {
                    EmitRemoveListenerEvent(eventName, listenerInfo.Listener);
                }
            }
            _events.Clear();
        }
        else
        {
            // Remove all listeners for specific event
            var eventName = arguments[0]?.ToString() ?? "";
            if (_events.ContainsKey(eventName))
            {
                var listeners = _events[eventName].ToList();
                foreach (var listenerInfo in listeners)
                {
                    EmitRemoveListenerEvent(eventName, listenerInfo.Listener);
                }
                _events.Remove(eventName);
            }
        }

        // Return the EventEmitter for chaining
        return _emitterInstance;
    }

    private void EmitRemoveListenerEvent(string eventName, Function listener)
    {
        if (_events.ContainsKey("removeListener") && _events["removeListener"].Count > 0)
        {
            // Create arguments for removeListener event
            var removeListenerArgs = new List<object?> { "removeListener", eventName, listener };
            
            // Get emit function and call it
            if (_emitterInstance.TryGetValue("emit", out var emitFunc) && emitFunc is NodeEventEmitterEmitFunction emitter)
            {
                emitter.Call(removeListenerArgs);
            }
        }
    }
}

/// <summary>
/// Enhanced EventEmitter.listenerCount() method
/// </summary>
public class NodeEventEmitterListenerCountFunction
{
    private readonly Dictionary<string, List<EventListenerInfo>> _events;

    public NodeEventEmitterListenerCountFunction(Dictionary<string, List<EventListenerInfo>> events)
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
        
        if (arguments.Count >= 2 && arguments[1] is Function specificListener)
        {
            // Count specific listener instances
            if (_events.ContainsKey(eventName))
            {
                var count = _events[eventName].Count(li => 
                    li.Listener == specificListener || 
                    (li.OriginalListener != null && li.OriginalListener == specificListener));
                return (double)count;
            }
        }
        else
        {
            // Count all listeners for event
            if (_events.ContainsKey(eventName))
            {
                return (double)_events[eventName].Count;
            }
        }

        return 0.0;
    }
}

/// <summary>
/// Enhanced EventEmitter.listeners() method
/// </summary>
public class NodeEventEmitterListenersFunction
{
    private readonly Dictionary<string, List<EventListenerInfo>> _events;

    public NodeEventEmitterListenersFunction(Dictionary<string, List<EventListenerInfo>> events)
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
            // Return copy of listener functions (unwrapping once listeners)
            return _events[eventName]
                .Select(li => li.OriginalListener ?? li.Listener)
                .Cast<object?>()
                .ToList();
        }

        return new List<object?>();
    }
}

/// <summary>
/// EventEmitter.rawListeners() method
/// </summary>
public class NodeEventEmitterRawListenersFunction
{
    private readonly Dictionary<string, List<EventListenerInfo>> _events;

    public NodeEventEmitterRawListenersFunction(Dictionary<string, List<EventListenerInfo>> events)
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
            // Return raw listener functions (including wrappers)
            return _events[eventName]
                .Select(li => (object?)li.Listener)
                .ToList();
        }

        return new List<object?>();
    }
}

/// <summary>
/// Enhanced EventEmitter.eventNames() method
/// </summary>
public class NodeEventEmitterEventNamesFunction
{
    private readonly Dictionary<string, List<EventListenerInfo>> _events;

    public NodeEventEmitterEventNamesFunction(Dictionary<string, List<EventListenerInfo>> events)
    {
        _events = events;
    }

    public object? Call(List<object?> arguments)
    {
        return _events.Keys.Cast<object?>().ToList();
    }
}

/// <summary>
/// EventEmitter.setMaxListeners() method
/// </summary>
public class NodeEventEmitterSetMaxListenersFunction
{
    private readonly Dictionary<string, List<EventListenerInfo>> _events;
    private readonly Dictionary<string, object?> _emitterInstance;

    public NodeEventEmitterSetMaxListenersFunction(Dictionary<string, List<EventListenerInfo>> events, Dictionary<string, object?> emitterInstance)
    {
        _events = events;
        _emitterInstance = emitterInstance;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 1 || arguments[0] is not double n)
        {
            throw new ECEngineException("EventEmitter.setMaxListeners() requires a number argument",
                1, 1, "", "Usage: emitter.setMaxListeners(10)");
        }

        var maxListeners = (int)n;
        _emitterInstance["_maxListeners"] = maxListeners;
        
        // Return the EventEmitter for chaining
        return _emitterInstance;
    }
}

/// <summary>
/// EventEmitter.getMaxListeners() method
/// </summary>
public class NodeEventEmitterGetMaxListenersFunction
{
    private readonly Dictionary<string, List<EventListenerInfo>> _events;

    public NodeEventEmitterGetMaxListenersFunction(Dictionary<string, List<EventListenerInfo>> events)
    {
        _events = events;
    }

    public object? Call(List<object?> arguments)
    {
        // This will be handled by accessing _maxListeners property
        return (double)EventsModule.GetDefaultMaxListeners();
    }
}

// Static utility functions for the events module

/// <summary>
/// events.once() static method
/// </summary>
public class EventsOnceFunction
{
    public object? Call(List<object?> arguments)
    {
        // This would need Promise support for full implementation
        // For now, we can provide a basic placeholder
        throw new ECEngineException("events.once() is not yet implemented - requires Promise support",
            1, 1, "", "This feature requires async/await and Promise implementation");
    }
}

/// <summary>
/// events.listenerCount() static method (deprecated but included for compatibility)
/// </summary>
public class EventsListenerCountFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
        {
            throw new ECEngineException("events.listenerCount() requires 2 arguments: emitter and event name",
                1, 1, "", "Usage: events.listenerCount(emitter, 'event')");
        }

        var emitter = arguments[0];
        var eventName = arguments[1]?.ToString() ?? "";

        if (emitter is Dictionary<string, object?> emitterDict && 
            emitterDict.TryGetValue("listenerCount", out var listenerCountFunc) &&
            listenerCountFunc is NodeEventEmitterListenerCountFunction func)
        {
            return func.Call(new List<object?> { eventName });
        }

        return 0.0;
    }
}

/// <summary>
/// events.getEventListeners() static method
/// </summary>
public class EventsGetEventListenersFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
        {
            throw new ECEngineException("events.getEventListeners() requires 2 arguments: emitter and event name",
                1, 1, "", "Usage: events.getEventListeners(emitter, 'event')");
        }

        var emitter = arguments[0];
        var eventName = arguments[1]?.ToString() ?? "";

        if (emitter is Dictionary<string, object?> emitterDict && 
            emitterDict.TryGetValue("listeners", out var listenersFunc) &&
            listenersFunc is NodeEventEmitterListenersFunction func)
        {
            return func.Call(new List<object?> { eventName });
        }

        return new List<object?>();
    }
}

/// <summary>
/// events.getMaxListeners() static method
/// </summary>
public class EventsGetMaxListenersFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 1)
        {
            return (double)EventsModule.GetDefaultMaxListeners();
        }

        var emitter = arguments[0];
        if (emitter is Dictionary<string, object?> emitterDict && 
            emitterDict.TryGetValue("getMaxListeners", out var getMaxFunc) &&
            getMaxFunc is NodeEventEmitterGetMaxListenersFunction func)
        {
            return func.Call(new List<object?>());
        }

        return (double)EventsModule.GetDefaultMaxListeners();
    }
}

/// <summary>
/// events.setMaxListeners() static method
/// </summary>
public class EventsSetMaxListenersFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 1 || arguments[0] is not double n)
        {
            throw new ECEngineException("events.setMaxListeners() requires a number argument",
                1, 1, "", "Usage: events.setMaxListeners(10, emitter1, emitter2, ...)");
        }

        var maxListeners = (int)n;
        var emitters = arguments.Skip(1).ToList();

        if (emitters.Count == 0)
        {
            // Set default for all new EventEmitters
            EventsModule.SetDefaultMaxListeners(maxListeners);
        }
        else
        {
            // Set for specific emitters
            foreach (var emitter in emitters)
            {
                if (emitter is Dictionary<string, object?> emitterDict && 
                    emitterDict.TryGetValue("setMaxListeners", out var setMaxFunc) &&
                    setMaxFunc is NodeEventEmitterSetMaxListenersFunction func)
                {
                    func.Call(new List<object?> { (double)maxListeners });
                }
            }
        }

        return null;
    }
}

/// <summary>
/// events.addAbortListener() static method
/// </summary>
public class EventsAddAbortListenerFunction
{
    public object? Call(List<object?> arguments)
    {
        // This would need AbortSignal support for full implementation
        throw new ECEngineException("events.addAbortListener() is not yet implemented - requires AbortSignal support",
            1, 1, "", "This feature requires AbortSignal and DOM-like event handling");
    }
}
