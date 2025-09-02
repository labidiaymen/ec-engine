namespace ECEngine.Runtime.Streams;

/// <summary>
/// Pipe data callback for readable streams
/// </summary>
public class StreamPipeDataCallback : IStreamCallback
{
    private readonly WritableStream _destination;
    
    public StreamPipeDataCallback(WritableStream destination)
    {
        _destination = destination;
    }
    
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count > 0)
        {
            _destination.Write(new List<object?> { arguments[0] });
        }
        return null;
    }
}

/// <summary>
/// Pipe end callback for readable streams
/// </summary>
public class StreamPipeEndCallback : IStreamCallback
{
    private readonly WritableStream _destination;
    
    public StreamPipeEndCallback(WritableStream destination)
    {
        _destination = destination;
    }
    
    public object? Call(List<object?> arguments)
    {
        _destination.End();
        return null;
    }
}

/// <summary>
/// Pipe error callback for readable streams
/// </summary>
public class StreamPipeErrorCallback : IStreamCallback
{
    private readonly WritableStream _destination;
    
    public StreamPipeErrorCallback(WritableStream destination)
    {
        _destination = destination;
    }
    
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count > 0)
        {
            _destination.Emit("error", arguments[0]);
        }
        return null;
    }
}

/// <summary>
/// Write callback wrapper for writable streams
/// </summary>
public class StreamWriteCallbackFunction : IStreamCallback
{
    private readonly WritableStream _stream;
    private readonly IStreamCallback? _originalCallback;
    
    public StreamWriteCallbackFunction(WritableStream stream, IStreamCallback? originalCallback)
    {
        _stream = stream;
        _originalCallback = originalCallback;
    }
    
    public object? Call(List<object?> arguments)
    {
        _originalCallback?.Call(arguments);
        return null;
    }
}

/// <summary>
/// Final callback wrapper for writable streams
/// </summary>
public class StreamFinalCallbackFunction : IStreamCallback
{
    private readonly WritableStream _stream;
    private readonly IStreamCallback? _originalCallback;
    
    public StreamFinalCallbackFunction(WritableStream stream, IStreamCallback? originalCallback)
    {
        _stream = stream;
        _originalCallback = originalCallback;
    }
    
    public object? Call(List<object?> arguments)
    {
        // Use reflection to access private field
        var finishedField = typeof(WritableStream).BaseType?.GetField("_finished", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        finishedField?.SetValue(_stream, true);
        
        _stream.Emit("finish");
        _originalCallback?.Call(arguments);
        return null;
    }
}

/// <summary>
/// Transform callback wrapper for transform streams
/// </summary>
public class StreamTransformCallbackFunction : IStreamCallback
{
    private readonly TransformStream _stream;
    private readonly IStreamCallback? _originalCallback;
    
    public StreamTransformCallbackFunction(TransformStream stream, IStreamCallback? originalCallback)
    {
        _stream = stream;
        _originalCallback = originalCallback;
    }
    
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count > 1)
        {
            // Transform callback called with (error, data)
            var error = arguments[0];
            var data = arguments[1];
            
            if (error == null && data != null)
            {
                _stream.Push(data);
            }
        }
        
        _originalCallback?.Call(arguments);
        return null;
    }
}

/// <summary>
/// Flush callback wrapper for transform streams
/// </summary>
public class StreamFlushCallbackFunction : IStreamCallback
{
    private readonly TransformStream _stream;
    private readonly IStreamCallback? _originalCallback;
    
    public StreamFlushCallbackFunction(TransformStream stream, IStreamCallback? originalCallback)
    {
        _stream = stream;
        _originalCallback = originalCallback;
    }
    
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count > 1)
        {
            var error = arguments[0];
            var data = arguments[1];
            
            if (error == null && data != null)
            {
                _stream.Push(data);
            }
        }
        
        // End the readable side
        _stream.Push(null);
        _originalCallback?.Call(arguments);
        return null;
    }
}

/// <summary>
/// Event forwarding callback for duplex streams
/// </summary>
public class StreamForwardEventCallback : IStreamCallback
{
    private readonly BaseStream _targetStream;
    private readonly string _eventName;
    
    public StreamForwardEventCallback(BaseStream targetStream, string eventName)
    {
        _targetStream = targetStream;
        _eventName = eventName;
    }
    
    public object? Call(List<object?> arguments)
    {
        _targetStream.Emit(_eventName, arguments.ToArray());
        return null;
    }
}
