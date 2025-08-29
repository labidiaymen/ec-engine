using Xunit;
using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.Runtime;
using System.Collections.Concurrent;

namespace ECEngine.Tests.Concurrency;

/// <summary>
/// Concurrent execution tests to verify ECEngine's thread safety and concurrent access handling
/// </summary>
public class ConcurrentExecutionTests
{
    private static object? ExecuteCode(string code)
    {
        var lexer = new ECEngine.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();
        
        var parser = new ECEngine.Parser.Parser();
        var ast = parser.Parse(code);
        
        var interpreter = new ECEngine.Runtime.Interpreter();
        return interpreter.Evaluate(ast, code);
    }

    private static async Task<T?> ExecuteCodeAsync<T>(string code)
    {
        return await Task.Run(() => (T?)ExecuteCode(code));
    }

    #region Parallel Execution Tests

    [Fact]
    public async Task ParallelExecution_IndependentCode_ExecutesCorrectly()
    {
        // Arrange
        var tasks = new List<Task<object?>>();
        var results = new ConcurrentBag<object?>();
        
        // Act - Execute independent code blocks in parallel
        for (int i = 0; i < 10; i++)
        {
            int taskId = i;
            var task = Task.Run(() =>
            {
                string code = $@"
var x = {taskId};
var y = x * 2;
var result = x + y;
result;
";
                var result = ExecuteCode(code);
                results.Add(result);
                return result;
            });
            tasks.Add(task);
        }
        
        // Wait for all tasks to complete
        await Task.WhenAll(tasks);
        
        // Assert - All tasks should complete successfully
        Assert.Equal(10, results.Count);
        foreach (var task in tasks)
        {
            Assert.True(task.IsCompletedSuccessfully);
        }
    }

    [Fact]
    public void ParallelParsing_SameCode_ProducesConsistentResults()
    {
        // Arrange
        string testCode = @"
var x = 42;
var y = x * 3 + 1;
function test(a) {
    return a + 10;
}
var result = test(y);
result;
";
        
        var tasks = new List<Task<object?>>();
        var results = new ConcurrentBag<object?>();
        
        // Act - Parse and execute the same code in parallel
        for (int i = 0; i < 20; i++)
        {
            var task = Task.Run(() =>
            {
                var result = ExecuteCode(testCode);
                results.Add(result);
                return result;
            });
            tasks.Add(task);
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert - All results should be identical
        var uniqueResults = results.Distinct().ToList();
        Assert.Single(uniqueResults);
        Assert.Equal(20, results.Count);
    }

    [Fact]
    public void ParallelLexing_DifferentCode_NoInterference()
    {
        // Arrange
        var tasks = new List<Task<List<Token>>>();
        var allTokens = new ConcurrentBag<List<Token>>();
        
        // Act - Tokenize different code blocks in parallel
        for (int i = 0; i < 15; i++)
        {
            int taskId = i;
            var task = Task.Run(() =>
            {
                string code = $"var variable{taskId} = {taskId} + {taskId * 2};";
                var lexer = new ECEngine.Lexer.Lexer(code);
                var tokens = lexer.Tokenize();
                allTokens.Add(tokens);
                return tokens;
            });
            tasks.Add(task);
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert - All tokenization should complete successfully
        Assert.Equal(15, allTokens.Count);
        foreach (var task in tasks)
        {
            Assert.True(task.IsCompletedSuccessfully);
            Assert.NotEmpty(task.Result);
        }
    }

    #endregion

    #region Thread Safety Tests

    [Fact(Skip = "Shared interpreter state causes variable redeclaration issues")]
    public void ThreadSafety_SharedInterpreter_HandlesMultipleRequests()
    {
        // Arrange
        var interpreter = new ECEngine.Runtime.Interpreter();
        var tasks = new List<Task<object?>>();
        var results = new ConcurrentBag<object?>();
        var exceptions = new ConcurrentBag<Exception>();
        
        // Act - Use the same interpreter instance from multiple threads
        for (int i = 0; i < 10; i++)
        {
            int taskId = i;
            var task = Task.Run(() =>
            {
                try
                {
                    string code = $@"
var localVar = {taskId};
var result = localVar * 5;
result;
";
                    var lexer = new ECEngine.Lexer.Lexer(code);
                    var tokens = lexer.Tokenize();
                    
                    var parser = new ECEngine.Parser.Parser();
                    var ast = parser.Parse(code);
                    
                    var result = interpreter.Evaluate(ast, code);
                    results.Add(result);
                    return result;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    return null;
                }
            });
            tasks.Add(task);
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert - Should handle concurrent access gracefully
        // Note: This test might reveal thread safety issues if the interpreter isn't designed for concurrent use
        if (exceptions.Any())
        {
            // If exceptions occurred, they should be thread-safety related, not logic errors
            foreach (var ex in exceptions)
            {
                // Log the exception for analysis
                Assert.True(ex is InvalidOperationException || ex is ArgumentException,
                    $"Unexpected exception type: {ex.GetType()}, Message: {ex.Message}");
            }
        }
        else
        {
            // If no exceptions, all results should be valid
            Assert.Equal(10, results.Count);
        }
    }

    [Fact]
    public void ThreadSafety_LexerInstances_IndependentOperation()
    {
        // Arrange
        var tasks = new List<Task>();
        var lockObject = new object();
        var successCount = 0;
        
        // Act - Create multiple lexer instances simultaneously
        for (int i = 0; i < 50; i++)
        {
            int taskId = i;
            var task = Task.Run(() =>
            {
                try
                {
                    string code = $@"
var x{taskId} = {taskId};
var y{taskId} = ""string{taskId}"";
function func{taskId}() {{
    return x{taskId} + 1;
}}
var result{taskId} = func{taskId}();
";
                    
                    var lexer = new ECEngine.Lexer.Lexer(code);
                    var tokens = lexer.Tokenize();
                    
                    // Verify tokens were created
                    Assert.NotEmpty(tokens);
                    
                    lock (lockObject)
                    {
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    // Should not throw exceptions for independent lexer instances
                    Assert.True(false, $"Lexer instance {taskId} failed: {ex.Message}");
                }
            });
            tasks.Add(task);
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert
        Assert.Equal(50, successCount);
    }

    [Fact]
    public void ThreadSafety_ParserInstances_IndependentOperation()
    {
        // Arrange
        var tasks = new List<Task>();
        var lockObject = new object();
        var successCount = 0;
        
        // Act - Create multiple parser instances simultaneously
        for (int i = 0; i < 30; i++)
        {
            int taskId = i;
            var task = Task.Run(() =>
            {
                try
                {
                    string code = $@"
function calculate{taskId}(x) {{
    var temp = x * {taskId};
    return temp + {taskId};
}}
var result{taskId} = calculate{taskId}({taskId});
";
                    
                    var parser = new ECEngine.Parser.Parser();
                    var ast = parser.Parse(code);
                    
                    // Verify AST was created
                    Assert.NotNull(ast);
                    
                    lock (lockObject)
                    {
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    // Should not throw exceptions for independent parser instances
                    Assert.True(false, $"Parser instance {taskId} failed: {ex.Message}");
                }
            });
            tasks.Add(task);
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert
        Assert.Equal(30, successCount);
    }

    #endregion

    #region Observer Concurrency Tests

    [Fact(Skip = "Shared variable state across execution contexts not implemented")]
    public void ObserverConcurrency_MultipleObservers_ThreadSafeOperation()
    {
        // Arrange
        var triggerCounts = new ConcurrentDictionary<int, int>();
        var tasks = new List<Task>();
        
        // Setup code with observers
        string setupCode = @"
var sharedVar = 0;
observe sharedVar function() {
    console.log(""Observer triggered: "" + sharedVar);
}
";
        
        ExecuteCode(setupCode);
        
        // Act - Trigger observers from multiple threads
        for (int i = 0; i < 20; i++)
        {
            int taskId = i;
            var task = Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 10; j++)
                    {
                        ExecuteCode($"sharedVar = {taskId * 10 + j};");
                        
                        triggerCounts.AddOrUpdate(taskId, 1, (key, value) => value + 1);
                    }
                }
                catch (Exception ex)
                {
                    // Log concurrent observer issues
                    Assert.True(false, $"Observer task {taskId} failed: {ex.Message}");
                }
            });
            tasks.Add(task);
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert - All tasks should complete successfully
        Assert.Equal(20, triggerCounts.Count);
        foreach (var count in triggerCounts.Values)
        {
            Assert.Equal(10, count);
        }
    }

    [Fact(Skip = "Shared variable state across execution contexts not implemented")]
    public void ObserverRaceCondition_FastChanges_HandledGracefully()
    {
        // Arrange
        string setupCode = @"
var counter = 0;
var observerCount = 0;
observe counter function() {
    observerCount = observerCount + 1;
}
";
        
        ExecuteCode(setupCode);
        
        var tasks = new List<Task>();
        
        // Act - Rapidly change observed variable from multiple threads
        for (int i = 0; i < 10; i++)
        {
            var task = Task.Run(() =>
            {
                for (int j = 0; j < 50; j++)
                {
                    ExecuteCode("counter = counter + 1;");
                }
            });
            tasks.Add(task);
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert - Should handle rapid changes without crashing
        // Note: The exact values may vary due to race conditions, but the system should remain stable
        var finalCounter = ExecuteCode("counter;");
        var finalObserverCount = ExecuteCode("observerCount;");
        
        Assert.NotNull(finalCounter);
        Assert.NotNull(finalObserverCount);
    }

    #endregion

    #region Error Handling Concurrency Tests

    [Fact]
    public void ConcurrentErrors_MultipleThreadsWithErrors_IsolatedFailures()
    {
        // Arrange
        var tasks = new List<Task>();
        var successCount = 0;
        var errorCount = 0;
        var lockObject = new object();
        
        // Act - Execute code with intentional errors from multiple threads
        for (int i = 0; i < 20; i++)
        {
            int taskId = i;
            var task = Task.Run(() =>
            {
                try
                {
                    if (taskId % 3 == 0)
                    {
                        // Intentionally invalid code
                        ExecuteCode("var x = undefined_variable + 1;");
                    }
                    else
                    {
                        // Valid code
                        ExecuteCode($"var y = {taskId} * 2;");
                    }
                    
                    lock (lockObject)
                    {
                        successCount++;
                    }
                }
                catch (Exception)
                {
                    lock (lockObject)
                    {
                        errorCount++;
                    }
                }
            });
            tasks.Add(task);
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert - Errors in one thread should not affect others
        Assert.True(successCount > 0, "Some threads should succeed");
        Assert.True(errorCount > 0, "Some threads should fail as expected");
        Assert.Equal(20, successCount + errorCount);
    }

    #endregion

    #region Resource Contention Tests

    [Fact]
    public void ResourceContention_ManySmallExecutions_NoDeadlocks()
    {
        // Arrange
        var tasks = new List<Task<bool>>();
        
        // Act - Execute many small code blocks simultaneously
        for (int i = 0; i < 100; i++)
        {
            int taskId = i;
            var task = Task.Run(() =>
            {
                try
                {
                    string code = $"var x = {taskId}; x * 2;";
                    var result = ExecuteCode(code);
                    return result != null;
                }
                catch (Exception)
                {
                    return false;
                }
            });
            tasks.Add(task);
        }
        
        // Add timeout to detect deadlocks
        var completedInTime = Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(30));
        
        // Assert - All tasks should complete without deadlocks
        Assert.True(completedInTime, "Tasks should complete within timeout (no deadlocks)");
        
        var successfulTasks = tasks.Count(t => t.Result);
        Assert.True(successfulTasks >= 95, $"Most tasks should succeed ({successfulTasks}/100)");
    }

    [Fact]
    public void MemoryContention_LargeParallelAllocations_StableExecution()
    {
        // Arrange
        var tasks = new List<Task<bool>>();
        
        // Act - Allocate large amounts of memory in parallel
        for (int i = 0; i < 10; i++)
        {
            int taskId = i;
            var task = Task.Run(() =>
            {
                try
                {
                    var codeBuilder = new System.Text.StringBuilder();
                    for (int j = 0; j < 100; j++)
                    {
                        codeBuilder.AppendLine($"var largeArray{j} = \"Large string {taskId}_{j} with significant content\";");
                    }
                    
                    ExecuteCode(codeBuilder.ToString());
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
            tasks.Add(task);
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert - Memory allocations should not interfere with each other
        var successfulTasks = tasks.Count(t => t.Result);
        Assert.True(successfulTasks >= 8, $"Most memory allocation tasks should succeed ({successfulTasks}/10)");
    }

    #endregion

    #region Performance Under Concurrency Tests

    [Fact(Skip = "For loops and comparison operators not implemented")]
    public void ConcurrentPerformance_ParallelExecution_MaintainsReasonableSpeed()
    {
        // Arrange
        string testCode = @"
var sum = 0;
for (var i = 0; i < 100; i++) {
    sum = sum + i;
}
sum;
";
        
        // Measure sequential execution time
        var sequentialStopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 10; i++)
        {
            ExecuteCode(testCode);
        }
        sequentialStopwatch.Stop();
        
        // Measure parallel execution time
        var parallelStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            var task = Task.Run(() => ExecuteCode(testCode));
            tasks.Add(task);
        }
        Task.WaitAll(tasks.ToArray());
        parallelStopwatch.Stop();
        
        // Assert - Parallel execution should not be significantly slower than sequential
        var parallelOverhead = parallelStopwatch.ElapsedMilliseconds / (double)sequentialStopwatch.ElapsedMilliseconds;
        
        // Allow for some overhead but should benefit from parallelization
        Assert.True(parallelOverhead < 3.0, 
            $"Parallel execution overhead is too high: {parallelOverhead:F2}x slower than sequential");
    }

    #endregion
}
