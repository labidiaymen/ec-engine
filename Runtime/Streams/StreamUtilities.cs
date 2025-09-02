namespace ECEngine.Runtime.Streams;

/// <summary>
/// Stream pipeline utility function
/// </summary>
public class StreamPipelineFunction
{
    public object? Call(List<object?> args)
    {
        if (args.Count < 2) return null;
        
        var streams = args.Take(args.Count - 1).ToList();
        var callback = args.Last();
        var callbackWrapper = callback is Function func ? 
            new ECFunctionWrapper(func, GetCurrentInterpreter()) : 
            callback as IStreamCallback;
        
        try
        {
            // Simple pipeline implementation
            for (int i = 0; i < streams.Count - 1; i++)
            {
                if (streams[i] is ReadableStream readable && streams[i + 1] is WritableStream writable)
                {
                    readable.Pipe(new List<object?> { writable });
                }
            }
            
            // Call callback on completion
            if (streams.Last() is BaseStream lastStream)
            {
                lastStream.On(new List<object?> { "finish", new StreamPipelineCallbackFunction(callbackWrapper) });
                lastStream.On(new List<object?> { "error", new StreamPipelineErrorFunction(callbackWrapper) });
            }
        }
        catch (Exception ex)
        {
            callbackWrapper?.Call(new List<object?> { ex });
        }
        
        return null;
    }
    
    private Interpreter GetCurrentInterpreter()
    {
        return new Interpreter(); // Placeholder
    }
}

/// <summary>
/// Stream finished utility function
/// </summary>
public class StreamFinishedFunction
{
    public object? Call(List<object?> args)
    {
        if (args.Count < 2) return null;
        
        var stream = args[0] as BaseStream;
        var callback = args[1];
        var callbackWrapper = callback is Function func ? 
            new ECFunctionWrapper(func, GetCurrentInterpreter()) : 
            callback as IStreamCallback;
        
        if (stream != null && callbackWrapper != null)
        {
            stream.On(new List<object?> { "end", new StreamFinishedCallbackFunction(callbackWrapper) });
            stream.On(new List<object?> { "finish", new StreamFinishedCallbackFunction(callbackWrapper) });
            stream.On(new List<object?> { "error", callbackWrapper });
        }
        
        return null;
    }
    
    private Interpreter GetCurrentInterpreter()
    {
        return new Interpreter(); // Placeholder
    }
}

/// <summary>
/// Stream compose utility function
/// </summary>
public class StreamComposeFunction
{
    public object? Call(List<object?> args)
    {
        // Simplified compose implementation
        var interpreter = GetCurrentInterpreter();
        return new PassThroughStream(interpreter);
    }
    
    private Interpreter GetCurrentInterpreter()
    {
        return new Interpreter(); // Placeholder
    }
}

/// <summary>
/// Check if stream is readable
/// </summary>
public class StreamIsReadableFunction
{
    public object? Call(List<object?> args)
    {
        return args.Count > 0 && args[0] is ReadableStream readable && readable.readable;
    }
}

/// <summary>
/// Check if stream is writable
/// </summary>
public class StreamIsWritableFunction
{
    public object? Call(List<object?> args)
    {
        return args.Count > 0 && args[0] is WritableStream writable && writable.writable;
    }
}

// Pipeline callback functions
public class StreamPipelineCallbackFunction : IStreamCallback
{
    private readonly IStreamCallback? _callback;
    
    public StreamPipelineCallbackFunction(IStreamCallback? callback)
    {
        _callback = callback;
    }
    
    public object? Call(List<object?> arguments)
    {
        _callback?.Call(new List<object?>());
        return null;
    }
}

public class StreamPipelineErrorFunction : IStreamCallback
{
    private readonly IStreamCallback? _callback;
    
    public StreamPipelineErrorFunction(IStreamCallback? callback)
    {
        _callback = callback;
    }
    
    public object? Call(List<object?> arguments)
    {
        _callback?.Call(arguments);
        return null;
    }
}

public class StreamFinishedCallbackFunction : IStreamCallback
{
    private readonly IStreamCallback? _callback;
    
    public StreamFinishedCallbackFunction(IStreamCallback? callback)
    {
        _callback = callback;
    }
    
    public object? Call(List<object?> arguments)
    {
        _callback?.Call(new List<object?>());
        return null;
    }
}
