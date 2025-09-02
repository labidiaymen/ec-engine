namespace ECEngine.Runtime.Streams;

/// <summary>
/// Duplex stream - both readable and writable
/// </summary>
public class DuplexStream : ReadableStream
{
    private readonly WritableStream _writableImpl;
    
    // Writable properties
    public bool writable => _writableImpl.writable;
    public bool writableEnded => _writableImpl.writableEnded;
    public bool writableFinished => _writableImpl.writableFinished;
    public int writableHighWaterMark => _writableImpl.writableHighWaterMark;
    public int writableLength => _writableImpl.writableLength;
    public bool writableNeedDrain => _writableImpl.writableNeedDrain;
    public bool writableObjectMode => _writableImpl.writableObjectMode;
    
    public DuplexStream(Interpreter interpreter, Dictionary<string, object?>? options = null) : base(interpreter, options)
    {
        _writableImpl = new WritableStream(interpreter, options);
        
        // Forward writable events
        _writableImpl.On(new List<object?> { "drain", new StreamForwardEventCallback(this, "drain") });
        _writableImpl.On(new List<object?> { "finish", new StreamForwardEventCallback(this, "finish") });
        _writableImpl.On(new List<object?> { "pipe", new StreamForwardEventCallback(this, "pipe") });
        _writableImpl.On(new List<object?> { "unpipe", new StreamForwardEventCallback(this, "unpipe") });
        _writableImpl.On(new List<object?> { "error", new StreamForwardEventCallback(this, "error") });
    }
    
    // Writable methods
    public virtual bool Write(List<object?> args)
    {
        return _writableImpl.Write(args);
    }
    
    public virtual object? End(List<object?> args = null)
    {
        _writableImpl.End(args);
        return this;
    }
    
    public virtual object? Cork(List<object?> args = null)
    {
        return _writableImpl.Cork(args);
    }
    
    public virtual object? Uncork(List<object?> args = null)
    {
        return _writableImpl.Uncork(args);
    }
    
    public override object? Destroy(List<object?> args = null)
    {
        _writableImpl.Destroy(args);
        return base.Destroy(args);
    }
    
    protected virtual void ProcessWriteData(object? chunk, string? encoding, IStreamCallback? callback)
    {
        // Default behavior - just push to readable side
        Push(chunk, encoding);
        callback?.Call(new List<object?>());
    }
}

/// <summary>
/// Duplex stream constructor function
/// </summary>
public class DuplexConstructorFunction
{
    public object? Call(List<object?> arguments)
    {
        var interpreter = GetCurrentInterpreter();
        var options = arguments.Count > 0 && arguments[0] is Dictionary<string, object?> opts ? opts : null;
        return new DuplexStream(interpreter, options);
    }
    
    private Interpreter GetCurrentInterpreter()
    {
        // Get current interpreter from context
        return new Interpreter(); // Placeholder
    }
}
