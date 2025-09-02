using System.Text;

namespace ECEngine.Runtime.Streams;

/// <summary>
/// Readable stream implementation
/// </summary>
public class ReadableStream : BaseStream
{
    private readonly Queue<object?> _readBuffer = new();
    private readonly object _readLock = new();
    private bool _reading = false;
    private int _highWaterMark = 64 * 1024; // 64KB default
    private int _currentBufferSize = 0;
    private string? _encoding = null;
    private bool _objectMode = false;
    private bool _flowing = false;
    private IStreamCallback? _readFunction;
    
    // Public properties
    public bool readable => !_destroyed && !_ended;
    public bool readableEnded => _ended;
    public bool readableFlowing => _flowing;
    public int readableHighWaterMark => _highWaterMark;
    public int readableLength => _readBuffer.Count;
    public bool readableObjectMode => _objectMode;
    
    public ReadableStream(Interpreter interpreter, Dictionary<string, object?>? options = null) : base(interpreter)
    {
        if (options != null)
        {
            if (options.ContainsKey("highWaterMark") && options["highWaterMark"] is double hwm)
                _highWaterMark = (int)hwm;
            if (options.ContainsKey("encoding"))
                _encoding = options["encoding"]?.ToString();
            if (options.ContainsKey("objectMode") && options["objectMode"] is bool objMode)
                _objectMode = objMode;
            if (options.ContainsKey("read"))
                _readFunction = ConvertToCallback(options["read"]);
        }
    }
    
    /// <summary>
    /// Read data from the stream
    /// </summary>
    public virtual object? Read(List<object?> args)
    {
        lock (_readLock)
        {
            if (_readBuffer.Count == 0)
            {
                if (!_reading && !_ended)
                {
                    _reading = true;
                    // Schedule read operation
                    Task.Run(() => InternalRead());
                }
                return null;
            }
            
            var size = args?.FirstOrDefault();
            if (_objectMode)
            {
                return _readBuffer.Count > 0 ? _readBuffer.Dequeue() : null;
            }
            
            // For buffer/string mode
            if (size is double sizeNum && sizeNum > 0)
            {
                return ReadBytes((int)sizeNum);
            }
            
            return ReadAllAvailable();
        }
    }
    
    private object? ReadBytes(int size)
    {
        var result = new List<byte>();
        var remaining = size;
        
        while (remaining > 0 && _readBuffer.Count > 0)
        {
            var chunk = _readBuffer.Peek();
            var bytes = ChunkToBytes(chunk);
            
            if (bytes.Length <= remaining)
            {
                _readBuffer.Dequeue();
                result.AddRange(bytes);
                remaining -= bytes.Length;
                _currentBufferSize -= bytes.Length;
            }
            else
            {
                // Split the chunk
                result.AddRange(bytes.Take(remaining));
                var leftover = bytes.Skip(remaining).ToArray();
                _readBuffer.Dequeue();
                // Re-queue the leftover data
                _readBuffer.Enqueue(leftover);
                _currentBufferSize -= remaining;
                remaining = 0;
            }
        }
        
        return _encoding != null ? Encoding.UTF8.GetString(result.ToArray()) : result.ToArray();
    }
    
    private object? ReadAllAvailable()
    {
        if (_readBuffer.Count == 0) return null;
        
        if (_objectMode)
        {
            var results = new List<object?>();
            while (_readBuffer.Count > 0)
            {
                results.Add(_readBuffer.Dequeue());
            }
            return results.ToArray();
        }
        
        var allBytes = new List<byte>();
        while (_readBuffer.Count > 0)
        {
            var chunk = _readBuffer.Dequeue();
            allBytes.AddRange(ChunkToBytes(chunk));
        }
        _currentBufferSize = 0;
        
        return _encoding != null ? Encoding.UTF8.GetString(allBytes.ToArray()) : allBytes.ToArray();
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
    
    private void InternalRead()
    {
        try
        {
            if (_readFunction != null)
            {
                _readFunction.Call(new List<object?> { _highWaterMark });
            }
            else
            {
                // Default read behavior - emit 'readable' event
                Emit("readable");
            }
        }
        catch (Exception ex)
        {
            Emit("error", ex);
        }
        finally
        {
            _reading = false;
        }
    }
    
    /// <summary>
    /// Push data into the readable stream
    /// </summary>
    public virtual bool Push(object? chunk, string? encoding = null)
    {
        if (_destroyed) return false;
        
        if (chunk == null)
        {
            // End of stream
            _ended = true;
            Emit("end");
            return false;
        }
        
        lock (_readLock)
        {
            _readBuffer.Enqueue(chunk);
            if (!_objectMode)
            {
                _currentBufferSize += ChunkToBytes(chunk).Length;
            }
            
            // Emit data event if in flowing mode
            if (_flowing)
            {
                Emit("data", chunk);
            }
            else
            {
                Emit("readable");
            }
            
            return _currentBufferSize < _highWaterMark;
        }
    }
    
    /// <summary>
    /// Set the stream to flowing mode
    /// </summary>
    public virtual object? Resume(List<object?> args = null)
    {
        _flowing = true;
        Emit("resume");
        
        // Emit all buffered data
        lock (_readLock)
        {
            while (_readBuffer.Count > 0)
            {
                var chunk = _readBuffer.Dequeue();
                Emit("data", chunk);
            }
            _currentBufferSize = 0;
        }
        
        return this;
    }
    
    /// <summary>
    /// Pause the stream
    /// </summary>
    public virtual object? Pause(List<object?> args = null)
    {
        _flowing = false;
        Emit("pause");
        return this;
    }
    
    /// <summary>
    /// Set encoding for string output
    /// </summary>
    public virtual object? SetEncoding(List<object?> args)
    {
        if (args.Count > 0)
        {
            _encoding = args[0]?.ToString();
        }
        return this;
    }
    
    /// <summary>
    /// Pipe to a writable stream
    /// </summary>
    public virtual object? Pipe(List<object?> args)
    {
        if (args.Count < 1 || args[0] is not WritableStream destination)
            return this;
        
        var options = args.Count > 1 && args[1] is Dictionary<string, object?> opts ? opts : new Dictionary<string, object?>();
        var endOnFinish = !options.ContainsKey("end") || (bool)(options["end"] ?? true);
        
        // Set up data flow
        On(new List<object?> { "data", new StreamPipeDataCallback(destination) });
        
        if (endOnFinish)
        {
            On(new List<object?> { "end", new StreamPipeEndCallback(destination) });
        }
        
        On(new List<object?> { "error", new StreamPipeErrorCallback(destination) });
        
        // Emit pipe event on destination
        destination.Emit("pipe", this);
        
        // Start flowing
        Resume();
        
        return destination;
    }
}

/// <summary>
/// Readable stream constructor function
/// </summary>
public class ReadableConstructorFunction
{
    public object? Call(List<object?> arguments)
    {
        var interpreter = GetCurrentInterpreter();
        var options = arguments.Count > 0 && arguments[0] is Dictionary<string, object?> opts ? opts : null;
        return new ReadableStream(interpreter, options);
    }
    
    private Interpreter GetCurrentInterpreter()
    {
        // Get current interpreter from context
        // This would need to be implemented based on how ECEngine handles current context
        return new Interpreter(); // Placeholder
    }
}
