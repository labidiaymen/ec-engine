using System.Text;

namespace ECEngine.Runtime.Streams;

/// <summary>
/// Writable stream implementation
/// </summary>
public class WritableStream : BaseStream
{
    private readonly Queue<WriteRequest> _writeBuffer = new();
    private readonly object _writeLock = new();
    private bool _writing = false;
    private int _highWaterMark = 64 * 1024; // 64KB default
    private int _currentBufferSize = 0;
    private bool _objectMode = false;
    private string _defaultEncoding = "utf8";
    private bool _needDrain = false;
    private IStreamCallback? _writeFunction;
    private IStreamCallback? _finalFunction;
    
    // Public properties  
    public bool writable => !_destroyed && !_ended;
    public bool writableEnded => _ended;
    public bool writableFinished => _finished;
    public int writableHighWaterMark => _highWaterMark;
    public int writableLength => _writeBuffer.Count;
    public bool writableNeedDrain => _needDrain;
    public bool writableObjectMode => _objectMode;
    
    private class WriteRequest
    {
        public object? Chunk { get; set; }
        public string? Encoding { get; set; }
        public IStreamCallback? Callback { get; set; }
    }
    
    public WritableStream(Interpreter interpreter, Dictionary<string, object?>? options = null) : base(interpreter)
    {
        if (options != null)
        {
            if (options.ContainsKey("highWaterMark") && options["highWaterMark"] is double hwm)
                _highWaterMark = (int)hwm;
            if (options.ContainsKey("objectMode") && options["objectMode"] is bool objMode)
                _objectMode = objMode;
            if (options.ContainsKey("defaultEncoding"))
                _defaultEncoding = options["defaultEncoding"]?.ToString() ?? "utf8";
            if (options.ContainsKey("write"))
                _writeFunction = ConvertToCallback(options["write"]);
            if (options.ContainsKey("final"))
                _finalFunction = ConvertToCallback(options["final"]);
        }
    }
    
    /// <summary>
    /// Write data to the stream
    /// </summary>
    public virtual bool Write(List<object?> args)
    {
        if (_destroyed || _ended)
        {
            var errorCallback = ConvertToCallback(args.Count > 2 ? args[2] : null);
            errorCallback?.Call(new List<object?> { new Exception("Cannot write after end") });
            return false;
        }
        
        var chunk = args.Count > 0 ? args[0] : null;
        var encoding = args.Count > 1 ? args[1]?.ToString() ?? _defaultEncoding : _defaultEncoding;
        var callback = ConvertToCallback(args.Count > 2 ? args[2] : null);
        
        var request = new WriteRequest
        {
            Chunk = chunk,
            Encoding = encoding,
            Callback = callback
        };
        
        lock (_writeLock)
        {
            _writeBuffer.Enqueue(request);
            if (!_objectMode && chunk != null)
            {
                _currentBufferSize += ChunkToBytes(chunk).Length;
            }
            
            if (!_writing)
            {
                _writing = true;
                Task.Run(ProcessWrites);
            }
            
            var shouldDrain = _currentBufferSize >= _highWaterMark;
            if (shouldDrain)
            {
                _needDrain = true;
            }
            
            return !shouldDrain;
        }
    }
    
    private async Task ProcessWrites()
    {
        while (true)
        {
            WriteRequest? request;
            lock (_writeLock)
            {
                if (_writeBuffer.Count == 0)
                {
                    _writing = false;
                    if (_needDrain)
                    {
                        _needDrain = false;
                        Emit("drain");
                    }
                    return;
                }
                request = _writeBuffer.Dequeue();
            }
            
            try
            {
                if (_writeFunction != null)
                {
                    // Custom write function
                    var writeCallback = new StreamWriteCallbackFunction(this, request.Callback);
                    _writeFunction.Call(new List<object?> { request.Chunk, request.Encoding, writeCallback });
                }
                else
                {
                    // Default write behavior
                    await Task.Delay(1); // Simulate async write
                    request.Callback?.Call(new List<object?>());
                }
                
                if (!_objectMode && request.Chunk != null)
                {
                    _currentBufferSize -= ChunkToBytes(request.Chunk).Length;
                }
            }
            catch (Exception ex)
            {
                request.Callback?.Call(new List<object?> { ex });
                Emit("error", ex);
                return;
            }
        }
    }
    
    private byte[] ChunkToBytes(object? chunk)
    {
        return chunk switch
        {
            byte[] bytes => bytes,
            string str => Encoding.UTF8.GetBytes(str),
            _ => Encoding.UTF8.GetBytes(chunk?.ToString() ?? "")
        };
    }
    
    /// <summary>
    /// End the writable stream
    /// </summary>
    public virtual object? End(List<object?> args = null)
    {
        if (_ended) return this;
        
        var chunk = args?.Count > 0 ? args[0] : null;
        var encoding = args?.Count > 1 ? args[1]?.ToString() : null;
        var callback = ConvertToCallback(args?.Count > 2 ? args[2] : null);
        
        if (chunk != null)
        {
            Write(new List<object?> { chunk, encoding });
        }
        
        _ended = true;
        
        // Wait for all writes to complete, then call final and finish
        Task.Run(async () =>
        {
            while (_writing)
            {
                await Task.Delay(10);
            }
            
            try
            {
                if (_finalFunction != null)
                {
                    var finalCallback = new StreamFinalCallbackFunction(this, callback);
                    _finalFunction.Call(new List<object?> { finalCallback });
                }
                else
                {
                    _finished = true;
                    Emit("finish");
                    callback?.Call(new List<object?>());
                }
            }
            catch (Exception ex)
            {
                Emit("error", ex);
            }
        });
        
        return this;
    }
    
    /// <summary>
    /// Cork the stream to buffer writes
    /// </summary>
    public virtual object? Cork(List<object?> args = null)
    {
        // Simple implementation - just mark that we're corked
        // In a full implementation, this would batch multiple writes
        return this;
    }
    
    /// <summary>
    /// Uncork the stream to flush buffered writes
    /// </summary>
    public virtual object? Uncork(List<object?> args = null)
    {
        // Simple implementation - trigger drain if needed
        if (_needDrain)
        {
            _needDrain = false;
            Emit("drain");
        }
        return this;
    }
}

/// <summary>
/// Writable stream constructor function
/// </summary>
public class WritableConstructorFunction
{
    public object? Call(List<object?> arguments)
    {
        var interpreter = GetCurrentInterpreter();
        var options = arguments.Count > 0 && arguments[0] is Dictionary<string, object?> opts ? opts : null;
        return new WritableStream(interpreter, options);
    }
    
    private Interpreter GetCurrentInterpreter()
    {
        // Get current interpreter from context
        return new Interpreter(); // Placeholder
    }
}
