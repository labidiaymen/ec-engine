using Xunit;
using ECEngine.Runtime;
using ECEngine.AST;
using System.Collections.Generic;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Runtime;

public class BufferTests
{
    private object? EvaluateCode(string code)
    {
        var lexer = new ECEngine.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();
        var parser = new ECEngine.Parser.Parser();
        var ast = parser.Parse(code);
        var interpreter = new RuntimeInterpreter();
        
        // Initialize module system and event loop for full functionality
        var moduleSystem = new ModuleSystem();
        interpreter.SetModuleSystem(moduleSystem);
        
        return interpreter.Evaluate(ast, code);
    }

    [Fact]
    public void Buffer_RequireModule_ShouldLoadCorrectly()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const bufferModule = require('buffer');
            bufferModule;
        ");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Dictionary<string, object?>>(result);
        var module = (Dictionary<string, object?>)result;
        Assert.True(module.ContainsKey("Buffer"));
        Assert.True(module.ContainsKey("alloc"));
        Assert.True(module.ContainsKey("from"));
        Assert.True(module.ContainsKey("concat"));
    }

    [Fact]
    public void Buffer_RequireWithNodePrefix_ShouldLoadCorrectly()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const bufferModule = require('node:buffer');
            bufferModule;
        ");

        // Assert
        var module = (Dictionary<string, object?>)result;
        Assert.True(module.ContainsKey("Buffer"));
        Assert.True(module.ContainsKey("alloc"));
        Assert.True(module.ContainsKey("from"));
    }

    [Fact]
    public void Buffer_Alloc_ShouldCreateBufferWithCorrectSize()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf = Buffer.alloc(10);
            buf.length;
        ");

        // Assert
        Assert.Equal(10.0, result);
    }

    [Fact]
    public void Buffer_FromString_ShouldCreateBufferWithCorrectContent()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf = Buffer['from']('hello world', 'utf8');
            buf.toString();
        ");

        // Assert
        Assert.Equal("hello world", result);
    }

    [Fact]
    public void Buffer_FromArray_ShouldCreateBufferFromByteArray()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf = Buffer['from']([72, 101, 108, 108, 111]);
            buf.toString();
        ");

        // Assert
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void Buffer_Slice_ShouldReturnSubBuffer()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf = Buffer['from']('hello world');
            const sliced = buf.slice(0, 5);
            sliced.toString();
        ");

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public void Buffer_Concat_ShouldCombineBuffers()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf1 = Buffer['from']('hello ');
            const buf2 = Buffer['from']('world');
            const result = Buffer.concat([buf1, buf2]);
            result.toString();
        ");

        // Assert
        Assert.Equal("hello world", result);
    }

    [Fact]
    public void Buffer_IsBuffer_ShouldReturnTrueForBuffer()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf = Buffer.alloc(10);
            Buffer.isBuffer(buf);
        ");

        // Assert
        Assert.True((bool)result);
    }

    [Fact]
    public void Buffer_IsBuffer_ShouldReturnFalseForNonBuffer()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            Buffer.isBuffer('not a buffer');
        ");

        // Assert
        Assert.False((bool)result);
    }

    [Fact]
    public void Buffer_ByteLength_ShouldReturnCorrectByteLength()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            Buffer.byteLength('hello');
        ");

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void Buffer_Length_ShouldReturnCorrectLength()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf = Buffer['from']('test string');
            buf.length;
        ");

        // Assert
        Assert.Equal(11.0, result);
    }

    [Fact]
    public void Buffer_AllocUnsafe_ShouldCreateBuffer()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf = Buffer.allocUnsafe(20);
            buf.length;
        ");

        // Assert
        Assert.Equal(20.0, result);
    }

    [Fact]
    public void Buffer_Fill_ShouldFillBufferWithValue()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf = Buffer.alloc(5);
            buf.fill(65);
            buf.toString();
        ");

        // Assert
        Assert.Equal("AAAAA", result);
    }

    [Fact]
    public void Buffer_Write_ShouldWriteStringToBuffer()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf = Buffer.alloc(10);
            buf.write('hello', 0);
            buf.toString('utf8', 0, 5);
        ");

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public void Buffer_Copy_ShouldCopyBytesToAnotherBuffer()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const source = Buffer['from']('hello');
            const target = Buffer.alloc(10);
            source.copy(target, 0);
            target.toString('utf8', 0, 5);
        ");

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public void Buffer_Equals_ShouldReturnTrueForEqualBuffers()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf1 = Buffer['from']('test');
            const buf2 = Buffer['from']('test');
            buf1.equals(buf2);
        ");

        // Assert
        Assert.True((bool)result);
    }

    [Fact]
    public void Buffer_IndexOf_ShouldFindSubstring()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf = Buffer['from']('hello world');
            buf.indexOf('world');
        ");

        // Assert
        Assert.Equal(6.0, result);
    }

    [Fact]
    public void Buffer_Includes_ShouldCheckForSubstring()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf = Buffer['from']('hello world');
            buf.includes('world');
        ");

        // Assert
        Assert.True((bool)result);
    }

    [Fact]
    public void Buffer_ToStringWithEncoding_ShouldRespectEncoding()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf = Buffer['from']('hello', 'utf8');
            buf.toString('utf8');
        ");

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public void Buffer_Compare_ShouldCompareBuffers()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf1 = Buffer['from']('abc');
            const buf2 = Buffer['from']('abc');
            Buffer.compare(buf1, buf2);
        ");

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Buffer_Subarray_ShouldReturnSubarray()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            const buf = Buffer['from']('hello world');
            const sub = buf.subarray(6);
            sub.toString();
        ");

        // Assert
        Assert.Equal("world", result);
    }

    [Fact]
    public void Buffer_IsEncoding_ShouldValidateEncodings()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            Buffer.isEncoding('utf8');
        ");

        // Assert
        Assert.True((bool)result);
    }

    [Fact]
    public void Buffer_IsEncoding_ShouldReturnFalseForInvalidEncoding()
    {
        // Arrange & Act
        var result = EvaluateCode(@"
            const Buffer = require('buffer').Buffer;
            Buffer.isEncoding('invalid-encoding');
        ");

        // Assert
        Assert.False((bool)result);
    }
}
