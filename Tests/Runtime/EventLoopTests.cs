using Xunit;
using ECEngine.Runtime;

namespace ECEngine.Tests.Runtime;

public class EventLoopTests
{
    [Fact]
    public void EventLoop_NextTick_ShouldExecuteTaskOnNextTick()
    {
        // Arrange
        var eventLoop = new EventLoop();
        var executed = false;

        // Act
        eventLoop.NextTick(() => executed = true);
        eventLoop.Run();

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void EventLoop_SetTimeout_ShouldExecuteAfterDelay()
    {
        // Arrange
        var eventLoop = new EventLoop();
        var executed = false;
        var startTime = DateTime.UtcNow;

        // Act
        eventLoop.SetTimeout(() => executed = true, 10); // 10ms delay
        eventLoop.Run();
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.True(executed);
        Assert.True((endTime - startTime).TotalMilliseconds >= 5); // Allow some tolerance
    }

    [Fact]
    public void EventLoop_SetInterval_ShouldExecuteMultipleTimes()
    {
        // Arrange
        var eventLoop = new EventLoop();
        var executionCount = 0;

        // Act
        eventLoop.SetInterval(() =>
        {
            executionCount++;
            if (executionCount >= 3)
            {
                eventLoop.Stop(); // Stop after 3 executions
            }
        }, 10); // 10ms interval

        eventLoop.Run();

        // Assert
        Assert.True(executionCount >= 3);
    }

    [Fact]
    public void EventLoop_MultipleTasks_ShouldExecuteInOrder()
    {
        // Arrange
        var eventLoop = new EventLoop();
        var results = new List<int>();

        // Act
        eventLoop.NextTick(() => results.Add(1));
        eventLoop.NextTick(() => results.Add(2));
        eventLoop.NextTick(() => results.Add(3));
        eventLoop.Run();

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, results);
    }

    [Fact]
    public void EventLoop_ExceptionInTask_ShouldNotCrashEventLoop()
    {
        // Arrange
        var eventLoop = new EventLoop();
        var executed = false;

        // Act
        eventLoop.NextTick(() => throw new Exception("Test exception"));
        eventLoop.NextTick(() => executed = true);
        eventLoop.Run();

        // Assert
        Assert.True(executed); // Second task should still execute
    }

    [Fact]
    public void AsyncRuntime_Execute_ShouldWork()
    {
        // Arrange
        var runtime = new AsyncRuntime();
        var code = "var x = 5 + 3; console.log(x);";

        // Act & Assert (should not throw)
        var result = runtime.Execute(code);
    }

    [Fact]
    public void AsyncRuntime_HasPendingWork_ShouldReturnCorrectStatus()
    {
        // Arrange
        var runtime = new AsyncRuntime();

        // Act
        runtime.NextTick(() => { });
        var hasPendingWork = runtime.HasPendingWork();

        // Assert
        Assert.True(hasPendingWork);
    }
}
