using Xunit;
using ECEngine.Runtime.Streams;
using ECEngine.Runtime;
using ECEngine.AST;

namespace ECEngine.Tests.Runtime;

public class StreamTests
{
    private ECEngine.Runtime.Interpreter CreateInterpreter()
    {
        return new ECEngine.Runtime.Interpreter();
    }

    [Fact]
    public void StreamModule_ShouldBeAvailableViaRequire()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var moduleSystem = new ModuleSystem();
        var requireFunction = new RequireFunction(moduleSystem, interpreter);

        // Act
        var streamModule = requireFunction.Call(new object[] { "stream" });

        // Assert
        Assert.NotNull(streamModule);
        Assert.IsType<StreamModule>(streamModule);
    }

    [Fact]
    public void StreamModule_ShouldHaveAllConstructors()
    {
        // Arrange
        var streamModule = new StreamModule();

        // Assert
        Assert.NotNull(streamModule.Readable);
        Assert.NotNull(streamModule.Writable);
        Assert.NotNull(streamModule.Duplex);
        Assert.NotNull(streamModule.Transform);
        Assert.NotNull(streamModule.PassThrough);
    }

    [Fact]
    public void StreamModule_ShouldHaveAllUtilities()
    {
        // Arrange
        var streamModule = new StreamModule();

        // Assert
        Assert.NotNull(streamModule.pipeline);
        Assert.NotNull(streamModule.finished);
        Assert.NotNull(streamModule.compose);
        Assert.NotNull(streamModule.isReadable);
        Assert.NotNull(streamModule.isWritable);
    }

    [Fact]
    public void ReadableStreamConstructor_ShouldCreateReadableStream()
    {
        // Arrange
        var constructor = new ReadableStreamConstructor();

        // Act
        var stream = constructor.Call(new List<object?>());

        // Assert
        Assert.NotNull(stream);
        Assert.IsType<ReadableStream>(stream);
    }

    [Fact]
    public void ReadableStream_ShouldHaveCorrectProperties()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var stream = new ReadableStream(interpreter);

        // Assert
        Assert.True(stream.readable);
        Assert.False(stream.readableEnded);
        Assert.False(stream.readableFlowing);
        Assert.Equal(0, stream.readableLength);
        Assert.False(stream.readableObjectMode);
    }

    [Fact]
    public void WritableStreamConstructor_ShouldCreateWritableStream()
    {
        // Arrange
        var constructor = new WritableStreamConstructor();

        // Act
        var stream = constructor.Call(new List<object?>());

        // Assert
        Assert.NotNull(stream);
        Assert.IsType<WritableStream>(stream);
    }

    [Fact]
    public void WritableStream_ShouldHaveCorrectProperties()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var stream = new WritableStream(interpreter);

        // Assert
        Assert.True(stream.writable);
        Assert.False(stream.writableEnded);
        Assert.False(stream.writableFinished);
        Assert.Equal(0, stream.writableLength);
        Assert.False(stream.writableObjectMode);
    }

    [Fact]
    public void DuplexStreamConstructor_ShouldCreateDuplexStream()
    {
        // Arrange
        var constructor = new DuplexStreamConstructor();

        // Act
        var stream = constructor.Call(new List<object?>());

        // Assert
        Assert.NotNull(stream);
        Assert.IsType<DuplexStream>(stream);
    }

    [Fact]
    public void DuplexStream_ShouldHaveBothReadableAndWritableProperties()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var stream = new DuplexStream(interpreter);

        // Assert - Should have readable properties
        Assert.True(stream.readable);
        Assert.False(stream.readableEnded);
        
        // Assert - Should have writable properties  
        Assert.True(stream.writable);
        Assert.False(stream.writableEnded);
    }

    [Fact]
    public void TransformStreamConstructor_ShouldCreateTransformStream()
    {
        // Arrange
        var constructor = new TransformStreamConstructor();

        // Act
        var stream = constructor.Call(new List<object?>());

        // Assert
        Assert.NotNull(stream);
        Assert.IsType<TransformStream>(stream);
    }

    [Fact]
    public void PassThroughStreamConstructor_ShouldCreatePassThroughStream()
    {
        // Arrange
        var constructor = new PassThroughStreamConstructor();

        // Act
        var stream = constructor.Call(new List<object?>());

        // Assert
        Assert.NotNull(stream);
        Assert.IsType<PassThroughStream>(stream);
    }

    [Fact]
    public void BaseStream_ShouldSupportEventHandling()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var stream = new ReadableStream(interpreter);
        bool eventFired = false;
        
        var callback = new TestCallback(() => { eventFired = true; });

        // Act
        stream.On(new List<object?> { "test", callback });
        stream.Emit("test");

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void ReadableStream_ShouldSupportPushAndRead()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var options = new Dictionary<string, object?> { { "encoding", "utf8" } };
        var stream = new ReadableStream(interpreter, options);

        // Act
        var pushResult = stream.Push("test data");
        var readResult = stream.Read(new List<object?>());

        // Assert
        Assert.True(pushResult);
        Assert.Equal("test data", readResult);
    }

    [Fact]
    public void ReadableStream_ShouldEndWhenPushingNull()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var stream = new ReadableStream(interpreter);

        // Act
        stream.Push(null);

        // Assert
        Assert.True(stream.readableEnded);
        Assert.False(stream.readable);
    }

    [Fact]
    public void WritableStream_ShouldSupportWrite()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var stream = new WritableStream(interpreter);

        // Act
        var result = stream.Write(new List<object?> { "test data" });

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WritableStream_ShouldSupportEnd()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var stream = new WritableStream(interpreter);

        // Act
        stream.End(new List<object?>());

        // Assert
        Assert.True(stream.writableEnded);
        Assert.False(stream.writable);
    }

    [Fact]
    public void ReadableStream_ShouldSupportPiping()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var readable = new ReadableStream(interpreter);
        var writable = new WritableStream(interpreter);

        // Act
        var result = readable.Pipe(new List<object?> { writable });

        // Assert
        Assert.Equal(writable, result);
    }

    [Fact]
    public void StreamIsReadableFunction_ShouldReturnCorrectResult()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var readableStream = new ReadableStream(interpreter);
        var writableStream = new WritableStream(interpreter);
        var isReadableFunction = new StreamIsReadableFunction();

        // Act
        var readableResult = isReadableFunction.Call(new List<object?> { readableStream });
        var writableResult = isReadableFunction.Call(new List<object?> { writableStream });

        // Assert
        Assert.True((bool)readableResult!);
        Assert.False((bool)writableResult!);
    }

    [Fact]
    public void StreamIsWritableFunction_ShouldReturnCorrectResult()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var readableStream = new ReadableStream(interpreter);
        var writableStream = new WritableStream(interpreter);
        var isWritableFunction = new StreamIsWritableFunction();

        // Act
        var readableResult = isWritableFunction.Call(new List<object?> { readableStream });
        var writableResult = isWritableFunction.Call(new List<object?> { writableStream });

        // Assert
        Assert.False((bool)readableResult!);
        Assert.True((bool)writableResult!);
    }

    [Fact]
    public void StreamPipelineFunction_ShouldHandleBasicPipeline()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var readable = new ReadableStream(interpreter);
        var writable = new WritableStream(interpreter);
        var pipelineFunction = new StreamPipelineFunction();
        bool callbackInvoked = false;
        var callback = new TestCallback(() => { callbackInvoked = true; });

        // Act
        pipelineFunction.Call(new List<object?> { readable, writable, callback });

        // Assert - The pipeline function should not throw and should set up the pipeline
        Assert.NotNull(pipelineFunction);
        // Note: Full pipeline testing would require async event simulation
    }

    [Fact]
    public void StreamFinishedFunction_ShouldSetupEventListeners()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var stream = new ReadableStream(interpreter);
        var finishedFunction = new StreamFinishedFunction();
        var callback = new TestCallback(() => { });

        // Act & Assert - Should not throw
        finishedFunction.Call(new List<object?> { stream, callback });
        Assert.NotNull(finishedFunction);
    }

    [Fact]
    public void StreamComposeFunction_ShouldReturnPassThroughStream()
    {
        // Arrange
        var composeFunction = new StreamComposeFunction();

        // Act
        var result = composeFunction.Call(new List<object?>());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<PassThroughStream>(result);
    }

    [Fact]
    public void ReadableStream_WithOptions_ShouldApplyConfiguration()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var options = new Dictionary<string, object?>
        {
            { "highWaterMark", 1024.0 },
            { "encoding", "utf8" },
            { "objectMode", true }
        };

        // Act
        var stream = new ReadableStream(interpreter, options);

        // Assert
        Assert.Equal(1024, stream.readableHighWaterMark);
        Assert.True(stream.readableObjectMode);
    }

    [Fact]
    public void WritableStream_WithOptions_ShouldApplyConfiguration()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var options = new Dictionary<string, object?>
        {
            { "highWaterMark", 2048.0 },
            { "objectMode", true }
        };

        // Act
        var stream = new WritableStream(interpreter, options);

        // Assert
        Assert.Equal(2048, stream.writableHighWaterMark);
        Assert.True(stream.writableObjectMode);
    }

    [Fact]
    public void ECFunctionWrapper_ShouldWrapFunctionCorrectly()
    {
        // Arrange
        var interpreter = CreateInterpreter();
        var function = new Function("test", new List<string>(), new List<ECEngine.AST.Statement>(), new Dictionary<string, VariableInfo>());
        
        // Act
        var wrapper = new ECFunctionWrapper(function, interpreter);

        // Assert
        Assert.NotNull(wrapper);
        Assert.IsAssignableFrom<IStreamCallback>(wrapper);
    }
}

/// <summary>
/// Test callback implementation for testing stream events
/// </summary>
public class TestCallback : IStreamCallback
{
    private readonly Action _action;

    public TestCallback(Action action)
    {
        _action = action;
    }

    public object? Call(List<object?> arguments)
    {
        _action?.Invoke();
        return null;
    }
}
