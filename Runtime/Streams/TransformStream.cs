namespace ECEngine.Runtime.Streams;

/// <summary>
/// Transform stream - extends Duplex with transform logic
/// </summary>
public class TransformStream : DuplexStream
{
    private IStreamCallback? _transformFunction;
    private IStreamCallback? _flushFunction;
    
    public TransformStream(Interpreter interpreter, Dictionary<string, object?>? options = null) : base(interpreter, options)
    {
        if (options != null)
        {
            if (options.ContainsKey("transform"))
                _transformFunction = ConvertToCallback(options["transform"]);
            if (options.ContainsKey("flush"))
                _flushFunction = ConvertToCallback(options["flush"]);
        }
    }
    
    protected override void ProcessWriteData(object? chunk, string? encoding, IStreamCallback? callback)
    {
        if (_transformFunction != null)
        {
            try
            {
                var transformCallback = new StreamTransformCallbackFunction(this, callback);
                _transformFunction.Call(new List<object?> { chunk, encoding, transformCallback });
            }
            catch (Exception ex)
            {
                callback?.Call(new List<object?> { ex });
                Emit("error", ex);
            }
        }
        else
        {
            // Default transform - pass through
            Push(chunk, encoding);
            callback?.Call(new List<object?>());
        }
    }
    
    public override object? End(List<object?> args = null)
    {
        // Before ending, call flush if available
        if (_flushFunction != null)
        {
            try
            {
                var callback = ConvertToCallback(args?.LastOrDefault());
                var flushCallback = new StreamFlushCallbackFunction(this, callback);
                _flushFunction.Call(new List<object?> { flushCallback });
            }
            catch (Exception ex)
            {
                Emit("error", ex);
            }
        }
        
        return base.End(args);
    }
}

/// <summary>
/// PassThrough stream - simple transform that passes data through unchanged
/// </summary>
public class PassThroughStream : TransformStream
{
    public PassThroughStream(Interpreter interpreter, Dictionary<string, object?>? options = null) : base(interpreter, options)
    {
        // PassThrough just passes data through unchanged
    }
}

/// <summary>
/// Transform stream constructor function
/// </summary>
public class TransformConstructorFunction
{
    public object? Call(List<object?> arguments)
    {
        var interpreter = GetCurrentInterpreter();
        var options = arguments.Count > 0 && arguments[0] is Dictionary<string, object?> opts ? opts : null;
        return new TransformStream(interpreter, options);
    }
    
    private Interpreter GetCurrentInterpreter()
    {
        return new Interpreter(); // Placeholder
    }
}

/// <summary>
/// PassThrough stream constructor function
/// </summary>
public class PassThroughConstructorFunction
{
    public object? Call(List<object?> arguments)
    {
        var interpreter = GetCurrentInterpreter();
        var options = arguments.Count > 0 && arguments[0] is Dictionary<string, object?> opts ? opts : null;
        return new PassThroughStream(interpreter, options);
    }
    
    private Interpreter GetCurrentInterpreter()
    {
        return new Interpreter(); // Placeholder
    }
}
