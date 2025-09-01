using ECEngine.AST;
using System.Text;

namespace ECEngine.Runtime;

/// <summary>
/// Node.js-compatible Buffer implementation for ECEngine
/// https://nodejs.org/api/buffer.html
/// </summary>
public class BufferModule
{
    public object? Buffer => new BufferConstructor();
    public object? constants => new BufferConstants();
    
    // Static methods
    public object? alloc => new BufferAllocFunction();
    public object? allocUnsafe => new BufferAllocUnsafeFunction();
    public object? allocUnsafeSlow => new BufferAllocUnsafeSlowFunction();
    public object? byteLength => new BufferByteLengthFunction();
    public object? compare => new BufferCompareFunction();
    public object? concat => new BufferConcatFunction();
    public object? from => new BufferFromFunction();
    public object? isBuffer => new BufferIsBufferFunction();
    public object? isEncoding => new BufferIsEncodingFunction();
}

/// <summary>
/// Buffer constructor function
/// </summary>
public class BufferConstructor
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            throw new ECEngineException("Buffer constructor requires arguments", 0, 0, "", "TypeError");

        var arg1 = arguments[0];
        
        // Buffer(size) - deprecated but supported
        if (arg1 is double size)
        {
            return BufferObject.Alloc((int)size);
        }
        
        // Buffer(array)
        if (arg1 is List<object?> list)
        {
            return BufferObject.From(list);
        }
        
        // Buffer(string, encoding)
        if (arg1 is string str)
        {
            var encoding = arguments.Count > 1 ? arguments[1]?.ToString() ?? "utf8" : "utf8";
            return BufferObject.From(str, encoding);
        }
        
        throw new ECEngineException("Invalid Buffer constructor arguments", 0, 0, "", "TypeError");
    }
}

/// <summary>
/// Buffer.alloc() function
/// </summary>
public class BufferAllocFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            throw new ECEngineException("Buffer.alloc() requires size argument", 0, 0, "", "TypeError");
            
        var size = Convert.ToInt32(arguments[0]);
        var fill = arguments.Count > 1 ? arguments[1] : 0;
        var encoding = arguments.Count > 2 ? arguments[2]?.ToString() ?? "utf8" : "utf8";
        
        return BufferObject.Alloc(size, fill, encoding);
    }
}

/// <summary>
/// Buffer.allocUnsafe() function
/// </summary>
public class BufferAllocUnsafeFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            throw new ECEngineException("Buffer.allocUnsafe() requires size argument", 0, 0, "", "TypeError");
            
        var size = Convert.ToInt32(arguments[0]);
        return BufferObject.AllocUnsafe(size);
    }
}

/// <summary>
/// Buffer.allocUnsafeSlow() function
/// </summary>
public class BufferAllocUnsafeSlowFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            throw new ECEngineException("Buffer.allocUnsafeSlow() requires size argument", 0, 0, "", "TypeError");
            
        var size = Convert.ToInt32(arguments[0]);
        return BufferObject.AllocUnsafeSlow(size);
    }
}

/// <summary>
/// Buffer.byteLength() function
/// </summary>
public class BufferByteLengthFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            throw new ECEngineException("Buffer.byteLength() requires string argument", 0, 0, "", "TypeError");
            
        var str = arguments[0]?.ToString() ?? "";
        var encoding = arguments.Count > 1 ? arguments[1]?.ToString() ?? "utf8" : "utf8";
        
        return BufferObject.GetByteLength(str, encoding);
    }
}

/// <summary>
/// Buffer.compare() function
/// </summary>
public class BufferCompareFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
            throw new ECEngineException("Buffer.compare() requires two Buffer arguments", 0, 0, "", "TypeError");
            
        var buf1 = arguments[0] as BufferObject;
        var buf2 = arguments[1] as BufferObject;
        
        if (buf1 == null || buf2 == null)
            throw new ECEngineException("Buffer.compare() requires Buffer arguments", 0, 0, "", "TypeError");
            
        return buf1.Compare(buf2);
    }
}

/// <summary>
/// Buffer.concat() function
/// </summary>
public class BufferConcatFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            throw new ECEngineException("Buffer.concat() requires array argument", 0, 0, "", "TypeError");
            
        var list = arguments[0] as List<object?>;
        if (list == null)
            throw new ECEngineException("Buffer.concat() requires array of Buffers", 0, 0, "", "TypeError");
            
        var totalLength = arguments.Count > 1 ? Convert.ToInt32(arguments[1]) : -1;
        
        return BufferObject.Concat(list, totalLength);
    }
}

/// <summary>
/// Buffer.from() function
/// </summary>
public class BufferFromFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            throw new ECEngineException("Buffer.from() requires arguments", 0, 0, "", "TypeError");
            
        var arg1 = arguments[0];
        
        // Buffer.from(string, encoding)
        if (arg1 is string str)
        {
            var encoding = arguments.Count > 1 ? arguments[1]?.ToString() ?? "utf8" : "utf8";
            return BufferObject.From(str, encoding);
        }
        
        // Buffer.from(array)
        if (arg1 is List<object?> list)
        {
            return BufferObject.From(list);
        }
        
        // Buffer.from(buffer)
        if (arg1 is BufferObject buffer)
        {
            return BufferObject.From(buffer);
        }
        
        throw new ECEngineException("Invalid Buffer.from() arguments", 0, 0, "", "TypeError");
    }
}

/// <summary>
/// Buffer.isBuffer() function
/// </summary>
public class BufferIsBufferFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        return arguments[0] is BufferObject;
    }
}

/// <summary>
/// Buffer.isEncoding() function
/// </summary>
public class BufferIsEncodingFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return false;
        
        var encoding = arguments[0]?.ToString()?.ToLowerInvariant();
        return encoding switch
        {
            "ascii" or "utf8" or "utf-8" or "utf16le" or "ucs2" or "ucs-2" or 
            "base64" or "base64url" or "latin1" or "binary" or "hex" => true,
            _ => false
        };
    }
}

/// <summary>
/// Buffer constants
/// </summary>
public class BufferConstants
{
    public double MAX_LENGTH => 2147483647; // 2^31 - 1
    public double MAX_STRING_LENGTH => 536870888; // 2^29 - 8
}

/// <summary>
/// Main Buffer object implementation
/// </summary>
public class BufferObject
{
    private byte[] _data;
    
    public BufferObject(byte[] data)
    {
        _data = data;
    }
    
    public int length => _data.Length;
    public byte[] Data => _data;
    
    // Static factory methods
    public static BufferObject Alloc(int size, object? fill = null, string encoding = "utf8")
    {
        if (size < 0) throw new ECEngineException("Invalid size", 0, 0, "", "RangeError");
        
        var buffer = new byte[size];
        
        if (fill != null)
        {
            var fillValue = GetFillValue(fill, encoding);
            for (int i = 0; i < size; i++)
            {
                buffer[i] = fillValue[i % fillValue.Length];
            }
        }
        
        return new BufferObject(buffer);
    }
    
    public static BufferObject AllocUnsafe(int size)
    {
        if (size < 0) throw new ECEngineException("Invalid size", 0, 0, "", "RangeError");
        return new BufferObject(new byte[size]);
    }
    
    public static BufferObject AllocUnsafeSlow(int size)
    {
        if (size < 0) throw new ECEngineException("Invalid size", 0, 0, "", "RangeError");
        return new BufferObject(new byte[size]);
    }
    
    public static BufferObject From(string str, string encoding = "utf8")
    {
        var bytes = GetBytesFromString(str, encoding);
        return new BufferObject(bytes);
    }
    
    public static BufferObject From(List<object?> array)
    {
        var bytes = new byte[array.Count];
        for (int i = 0; i < array.Count; i++)
        {
            bytes[i] = Convert.ToByte(array[i]);
        }
        return new BufferObject(bytes);
    }
    
    public static BufferObject From(BufferObject buffer)
    {
        var newData = new byte[buffer._data.Length];
        Array.Copy(buffer._data, newData, buffer._data.Length);
        return new BufferObject(newData);
    }
    
    public static BufferObject Concat(List<object?> list, int totalLength = -1)
    {
        var buffers = list.Cast<BufferObject>().ToList();
        
        if (totalLength < 0)
        {
            totalLength = buffers.Sum(b => b.length);
        }
        
        var result = new byte[totalLength];
        int offset = 0;
        
        foreach (var buffer in buffers)
        {
            var copyLength = Math.Min(buffer.length, totalLength - offset);
            Array.Copy(buffer._data, 0, result, offset, copyLength);
            offset += copyLength;
            
            if (offset >= totalLength) break;
        }
        
        return new BufferObject(result);
    }
    
    public static int GetByteLength(string str, string encoding = "utf8")
    {
        return GetBytesFromString(str, encoding).Length;
    }
    
    // Instance methods
    public double Get(int index)
    {
        if (index < 0 || index >= _data.Length) return double.NaN;
        return _data[index];
    }
    
    public void Set(int index, object? value)
    {
        if (index < 0 || index >= _data.Length) return;
        _data[index] = Convert.ToByte(value);
    }
    
    public string toString(string encoding = "utf8")
    {
        return GetStringFromBytes(_data, encoding);
    }
    
    public object? toJSON()
    {
        var result = new Dictionary<string, object?>
        {
            ["type"] = "Buffer",
            ["data"] = _data.Select(b => (double)b).ToList()
        };
        return result;
    }
    
    public double Compare(BufferObject other)
    {
        var minLength = Math.Min(_data.Length, other._data.Length);
        
        for (int i = 0; i < minLength; i++)
        {
            if (_data[i] < other._data[i]) return -1;
            if (_data[i] > other._data[i]) return 1;
        }
        
        return _data.Length.CompareTo(other._data.Length);
    }
    
    public BufferObject copy(BufferObject target, int targetStart = 0, int sourceStart = 0, int sourceEnd = -1)
    {
        if (sourceEnd < 0) sourceEnd = _data.Length;
        
        var copyLength = Math.Min(sourceEnd - sourceStart, target._data.Length - targetStart);
        Array.Copy(_data, sourceStart, target._data, targetStart, copyLength);
        
        return target;
    }
    
    public bool equals(BufferObject other)
    {
        if (_data.Length != other._data.Length) return false;
        return _data.SequenceEqual(other._data);
    }
    
    public BufferObject fill(object? value, int offset = 0, int end = -1, string encoding = "utf8")
    {
        if (end < 0) end = _data.Length;
        
        var fillBytes = GetFillValue(value, encoding);
        
        for (int i = offset; i < end; i++)
        {
            _data[i] = fillBytes[i % fillBytes.Length];
        }
        
        return this;
    }
    
    public bool includes(object? value, int byteOffset = 0, string encoding = "utf8")
    {
        return indexOf(value, byteOffset, encoding) >= 0;
    }
    
    public double indexOf(object? value, int byteOffset = 0, string encoding = "utf8")
    {
        byte[] searchBytes;
        
        if (value is string str)
        {
            searchBytes = GetBytesFromString(str, encoding);
        }
        else if (value is BufferObject buf)
        {
            searchBytes = buf._data;
        }
        else
        {
            searchBytes = new[] { Convert.ToByte(value) };
        }
        
        for (int i = byteOffset; i <= _data.Length - searchBytes.Length; i++)
        {
            bool found = true;
            for (int j = 0; j < searchBytes.Length; j++)
            {
                if (_data[i + j] != searchBytes[j])
                {
                    found = false;
                    break;
                }
            }
            if (found) return i;
        }
        
        return -1;
    }
    
    public double lastIndexOf(object? value, int byteOffset = -1, string encoding = "utf8")
    {
        if (byteOffset < 0) byteOffset = _data.Length - 1;
        
        byte[] searchBytes;
        
        if (value is string str)
        {
            searchBytes = GetBytesFromString(str, encoding);
        }
        else if (value is BufferObject buf)
        {
            searchBytes = buf._data;
        }
        else
        {
            searchBytes = new[] { Convert.ToByte(value) };
        }
        
        for (int i = Math.Min(byteOffset, _data.Length - searchBytes.Length); i >= 0; i--)
        {
            bool found = true;
            for (int j = 0; j < searchBytes.Length; j++)
            {
                if (_data[i + j] != searchBytes[j])
                {
                    found = false;
                    break;
                }
            }
            if (found) return i;
        }
        
        return -1;
    }
    
    public BufferObject slice(int start = 0, int end = -1)
    {
        if (end < 0) end = _data.Length;
        if (start < 0) start = Math.Max(0, _data.Length + start);
        if (end < 0) end = Math.Max(0, _data.Length + end);
        
        var length = Math.Max(0, end - start);
        var result = new byte[length];
        Array.Copy(_data, start, result, 0, length);
        
        return new BufferObject(result);
    }
    
    public BufferObject subarray(int start = 0, int end = -1)
    {
        // subarray creates a view, but for simplicity we'll copy like slice
        return slice(start, end);
    }
    
    public BufferObject swap16()
    {
        for (int i = 0; i < _data.Length - 1; i += 2)
        {
            (_data[i], _data[i + 1]) = (_data[i + 1], _data[i]);
        }
        return this;
    }
    
    public BufferObject swap32()
    {
        for (int i = 0; i < _data.Length - 3; i += 4)
        {
            (_data[i], _data[i + 3]) = (_data[i + 3], _data[i]);
            (_data[i + 1], _data[i + 2]) = (_data[i + 2], _data[i + 1]);
        }
        return this;
    }
    
    public BufferObject swap64()
    {
        for (int i = 0; i < _data.Length - 7; i += 8)
        {
            for (int j = 0; j < 4; j++)
            {
                (_data[i + j], _data[i + 7 - j]) = (_data[i + 7 - j], _data[i + j]);
            }
        }
        return this;
    }
    
    // Write methods
    public double write(string str, int offset = 0, int length = -1, string encoding = "utf8")
    {
        if (length < 0) length = _data.Length - offset;
        
        var bytes = GetBytesFromString(str, encoding);
        var writeLength = Math.Min(length, bytes.Length);
        
        Array.Copy(bytes, 0, _data, offset, writeLength);
        return writeLength;
    }
    
    // Helper methods
    private static byte[] GetBytesFromString(string str, string encoding)
    {
        return encoding.ToLowerInvariant() switch
        {
            "ascii" => Encoding.ASCII.GetBytes(str),
            "utf8" or "utf-8" => Encoding.UTF8.GetBytes(str),
            "utf16le" or "ucs2" or "ucs-2" => Encoding.Unicode.GetBytes(str),
            "latin1" or "binary" => Encoding.Latin1.GetBytes(str),
            "base64" => Convert.FromBase64String(str),
            "hex" => Convert.FromHexString(str),
            _ => Encoding.UTF8.GetBytes(str)
        };
    }
    
    private static string GetStringFromBytes(byte[] bytes, string encoding)
    {
        return encoding.ToLowerInvariant() switch
        {
            "ascii" => Encoding.ASCII.GetString(bytes),
            "utf8" or "utf-8" => Encoding.UTF8.GetString(bytes),
            "utf16le" or "ucs2" or "ucs-2" => Encoding.Unicode.GetString(bytes),
            "latin1" or "binary" => Encoding.Latin1.GetString(bytes),
            "base64" => Convert.ToBase64String(bytes),
            "hex" => Convert.ToHexString(bytes).ToLowerInvariant(),
            _ => Encoding.UTF8.GetString(bytes)
        };
    }
    
    private static byte[] GetFillValue(object? fill, string encoding)
    {
        if (fill is string str)
        {
            return GetBytesFromString(str, encoding);
        }
        else if (fill is BufferObject buf)
        {
            return buf._data;
        }
        else
        {
            return new[] { Convert.ToByte(fill) };
        }
    }
}

/// <summary>
/// Buffer method function wrapper
/// </summary>
public class BufferMethodFunction
{
    private readonly BufferObject _buffer;
    private readonly string _methodName;

    public BufferMethodFunction(BufferObject buffer, string methodName)
    {
        _buffer = buffer;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        return _methodName switch
        {
            "toString" => _buffer.toString(arguments.Count > 0 ? arguments[0]?.ToString() ?? "utf8" : "utf8"),
            "toJSON" => _buffer.toJSON(),
            "compare" => _buffer.Compare(arguments[0] as BufferObject ?? throw new ECEngineException("Buffer required", 0, 0, "", "TypeError")),
            "copy" => CallCopy(arguments),
            "equals" => _buffer.equals(arguments[0] as BufferObject ?? throw new ECEngineException("Buffer required", 0, 0, "", "TypeError")),
            "fill" => CallFill(arguments),
            "includes" => CallIncludes(arguments),
            "indexOf" => CallIndexOf(arguments),
            "lastIndexOf" => CallLastIndexOf(arguments),
            "slice" => CallSlice(arguments),
            "subarray" => CallSubarray(arguments),
            "swap16" => _buffer.swap16(),
            "swap32" => _buffer.swap32(),
            "swap64" => _buffer.swap64(),
            "write" => CallWrite(arguments),
            _ => throw new ECEngineException($"Unknown Buffer method: {_methodName}", 0, 0, "", "TypeError")
        };
    }

    private object? CallCopy(List<object?> arguments)
    {
        var target = arguments[0] as BufferObject ?? throw new ECEngineException("Buffer required", 0, 0, "", "TypeError");
        var targetStart = arguments.Count > 1 ? Convert.ToInt32(arguments[1]) : 0;
        var sourceStart = arguments.Count > 2 ? Convert.ToInt32(arguments[2]) : 0;
        var sourceEnd = arguments.Count > 3 ? Convert.ToInt32(arguments[3]) : -1;
        
        return _buffer.copy(target, targetStart, sourceStart, sourceEnd);
    }

    private object? CallFill(List<object?> arguments)
    {
        var value = arguments.Count > 0 ? arguments[0] : 0;
        var offset = arguments.Count > 1 ? Convert.ToInt32(arguments[1]) : 0;
        var end = arguments.Count > 2 ? Convert.ToInt32(arguments[2]) : -1;
        var encoding = arguments.Count > 3 ? arguments[3]?.ToString() ?? "utf8" : "utf8";
        
        return _buffer.fill(value, offset, end, encoding);
    }

    private object? CallIncludes(List<object?> arguments)
    {
        var value = arguments.Count > 0 ? arguments[0] : null;
        var byteOffset = arguments.Count > 1 ? Convert.ToInt32(arguments[1]) : 0;
        var encoding = arguments.Count > 2 ? arguments[2]?.ToString() ?? "utf8" : "utf8";
        
        return _buffer.includes(value, byteOffset, encoding);
    }

    private object? CallIndexOf(List<object?> arguments)
    {
        var value = arguments.Count > 0 ? arguments[0] : null;
        var byteOffset = arguments.Count > 1 ? Convert.ToInt32(arguments[1]) : 0;
        var encoding = arguments.Count > 2 ? arguments[2]?.ToString() ?? "utf8" : "utf8";
        
        return _buffer.indexOf(value, byteOffset, encoding);
    }

    private object? CallLastIndexOf(List<object?> arguments)
    {
        var value = arguments.Count > 0 ? arguments[0] : null;
        var byteOffset = arguments.Count > 1 ? Convert.ToInt32(arguments[1]) : -1;
        var encoding = arguments.Count > 2 ? arguments[2]?.ToString() ?? "utf8" : "utf8";
        
        return _buffer.lastIndexOf(value, byteOffset, encoding);
    }

    private object? CallSlice(List<object?> arguments)
    {
        var start = arguments.Count > 0 ? Convert.ToInt32(arguments[0]) : 0;
        var end = arguments.Count > 1 ? Convert.ToInt32(arguments[1]) : -1;
        
        return _buffer.slice(start, end);
    }

    private object? CallSubarray(List<object?> arguments)
    {
        var start = arguments.Count > 0 ? Convert.ToInt32(arguments[0]) : 0;
        var end = arguments.Count > 1 ? Convert.ToInt32(arguments[1]) : -1;
        
        return _buffer.subarray(start, end);
    }

    private object? CallWrite(List<object?> arguments)
    {
        var str = arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : "";
        var offset = arguments.Count > 1 ? Convert.ToInt32(arguments[1]) : 0;
        var length = arguments.Count > 2 ? Convert.ToInt32(arguments[2]) : -1;
        var encoding = arguments.Count > 3 ? arguments[3]?.ToString() ?? "utf8" : "utf8";
        
        return _buffer.write(str, offset, length, encoding);
    }
}

/// <summary>
/// Static methods for BufferGlobals to integrate with the module system
/// </summary>
public static class BufferGlobals
{
    public static Dictionary<string, object?> GetBufferModule()
    {
        var bufferModule = new BufferModule();
        var result = new Dictionary<string, object?>();
        
        // Add Buffer constructor and static methods
        result["Buffer"] = bufferModule.Buffer;
        result["constants"] = bufferModule.constants;
        result["alloc"] = bufferModule.alloc;
        result["allocUnsafe"] = bufferModule.allocUnsafe;
        result["allocUnsafeSlow"] = bufferModule.allocUnsafeSlow;
        result["byteLength"] = bufferModule.byteLength;
        result["compare"] = bufferModule.compare;
        result["concat"] = bufferModule.concat;
        result["from"] = bufferModule.from;
        result["isBuffer"] = bufferModule.isBuffer;
        result["isEncoding"] = bufferModule.isEncoding;
        
        return result;
    }
}
