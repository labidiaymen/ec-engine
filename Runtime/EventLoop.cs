using System.Collections.Concurrent;

namespace ECEngine.Runtime;

/// <summary>
/// Simple event loop implementation inspired by V8's design
/// Handles asynchronous operations, timers, and reactive events
/// </summary>
public class EventLoop
{
    private readonly ConcurrentQueue<Action> _taskQueue = new();
    private readonly ConcurrentQueue<TimerTask> _timerQueue = new();
    private readonly object _lock = new object();
    private bool _isRunning = false;
    private bool _shouldExit = false;

    /// <summary>
    /// Schedule a task to run on the next tick of the event loop
    /// </summary>
    public void NextTick(Action task)
    {
        _taskQueue.Enqueue(task);
    }

    /// <summary>
    /// Schedule a task to run after a delay (setTimeout equivalent)
    /// </summary>
    public void SetTimeout(Action task, int delayMs)
    {
        var executeAt = DateTime.UtcNow.AddMilliseconds(delayMs);
        _timerQueue.Enqueue(new TimerTask(task, executeAt, false));
    }

    /// <summary>
    /// Schedule a task to run repeatedly (setInterval equivalent)
    /// </summary>
    public void SetInterval(Action task, int intervalMs)
    {
        var executeAt = DateTime.UtcNow.AddMilliseconds(intervalMs);
        _timerQueue.Enqueue(new TimerTask(task, executeAt, true, intervalMs));
    }

    /// <summary>
    /// Run the event loop until no more tasks are pending
    /// </summary>
    public void Run()
    {
        lock (_lock)
        {
            if (_isRunning)
                return; // Already running
            
            _isRunning = true;
            _shouldExit = false;
        }

        try
        {
            while (!_shouldExit)
            {
                var processedTasks = ProcessTasks();
                var processedTimers = ProcessTimers();
                
                // If no tasks were processed, check if we should exit
                if (processedTasks == 0 && processedTimers == 0)
                {
                    if (!HasPendingTasks() && !HasPendingTimers())
                    {
                        break; // No more work to do
                    }
                    
                    // Small delay to prevent busy waiting
                    Thread.Sleep(1);
                }
            }
        }
        finally
        {
            lock (_lock)
            {
                _isRunning = false;
            }
        }
    }

    /// <summary>
    /// Stop the event loop
    /// </summary>
    public void Stop()
    {
        _shouldExit = true;
    }

    /// <summary>
    /// Check if there are pending tasks or timers
    /// </summary>
    public bool HasPendingWork()
    {
        return HasPendingTasks() || HasPendingTimers();
    }

    private bool HasPendingTasks()
    {
        return !_taskQueue.IsEmpty;
    }

    private bool HasPendingTimers()
    {
        return !_timerQueue.IsEmpty;
    }

    private int ProcessTasks()
    {
        // Process all current tasks in the queue
        var processedCount = 0;
        var maxTasksPerTick = 100; // Prevent infinite loops

        while (_taskQueue.TryDequeue(out var task) && processedCount < maxTasksPerTick)
        {
            try
            {
                task.Invoke();
            }
            catch (Exception ex)
            {
                // Log error but don't crash the event loop
                Console.WriteLine($"Error in event loop task: {ex.Message}");
            }
            processedCount++;
        }
        
        return processedCount;
    }

    private int ProcessTimers()
    {
        var now = DateTime.UtcNow;
        var timersToRequeue = new List<TimerTask>();
        var processedCount = 0;

        // Process all timers that are ready to execute
        while (_timerQueue.TryDequeue(out var timer))
        {
            if (timer.ExecuteAt <= now)
            {
                try
                {
                    timer.Task.Invoke();
                    processedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in timer task: {ex.Message}");
                }

                // If it's an interval timer, reschedule it
                if (timer.IsInterval)
                {
                    var nextExecuteAt = now.AddMilliseconds(timer.IntervalMs);
                    timersToRequeue.Add(new TimerTask(timer.Task, nextExecuteAt, true, timer.IntervalMs));
                }
            }
            else
            {
                // Timer not ready yet, put it back
                timersToRequeue.Add(timer);
            }
        }

        // Requeue timers that aren't ready or are intervals
        foreach (var timer in timersToRequeue)
        {
            _timerQueue.Enqueue(timer);
        }
        
        return processedCount;
    }

    /// <summary>
    /// Represents a timer task in the event loop
    /// </summary>
    private class TimerTask
    {
        public Action Task { get; }
        public DateTime ExecuteAt { get; }
        public bool IsInterval { get; }
        public int IntervalMs { get; }

        public TimerTask(Action task, DateTime executeAt, bool isInterval, int intervalMs = 0)
        {
            Task = task;
            ExecuteAt = executeAt;
            IsInterval = isInterval;
            IntervalMs = intervalMs;
        }
    }
}
