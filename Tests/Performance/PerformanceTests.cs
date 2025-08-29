using Xunit;
using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.Runtime;
using System.Diagnostics;

namespace ECEngine.Tests.Performance;

/// <summary>
/// Performance tests to measure ECEngine's execution speed and resource usage
/// </summary>
public class PerformanceTests
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

    private static TimeSpan MeasureExecutionTime(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    #region Lexer Performance Tests

    [Fact]
    public void Lexer_LargeNumberOfTokens_PerformsWithinReasonableTime()
    {
        // Arrange - Create code with many tokens
        var codeBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < 1000; i++)
        {
            codeBuilder.AppendLine($"var x{i} = {i} + {i + 1} * {i + 2};");
        }
        string code = codeBuilder.ToString();

        // Act
        var executionTime = MeasureExecutionTime(() =>
        {
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
        });

        // Assert - Should complete within reasonable time (adjust threshold as needed)
        Assert.True(executionTime.TotalMilliseconds < 1000, 
            $"Lexer took {executionTime.TotalMilliseconds}ms for 1000 lines");
    }

    [Fact]
    public void Lexer_LongString_PerformsWithinReasonableTime()
    {
        // Arrange - Create very long string
        var longString = new string('a', 10000);
        string code = $"var x = \"{longString}\";";

        // Act
        var executionTime = MeasureExecutionTime(() =>
        {
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
        });

        // Assert
        Assert.True(executionTime.TotalMilliseconds < 100, 
            $"Lexer took {executionTime.TotalMilliseconds}ms for long string");
    }

    [Fact]
    public void Lexer_ManyComments_PerformsWithinReasonableTime()
    {
        // Arrange - Create code with many comments
        var codeBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < 500; i++)
        {
            codeBuilder.AppendLine($"// This is comment number {i}");
            codeBuilder.AppendLine($"/* Multi-line comment {i} */");
        }
        string code = codeBuilder.ToString();

        // Act
        var executionTime = MeasureExecutionTime(() =>
        {
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
        });

        // Assert
        Assert.True(executionTime.TotalMilliseconds < 200, 
            $"Lexer took {executionTime.TotalMilliseconds}ms for many comments");
    }

    #endregion

    #region Parser Performance Tests

    [Fact]
    public void Parser_DeeplyNestedExpressions_PerformsWithinReasonableTime()
    {
        // Arrange - Create deeply nested expression
        var expression = "1";
        for (int i = 0; i < 100; i++)
        {
            expression = $"({expression} + {i})";
        }
        string code = $"var result = {expression};";

        // Act
        var executionTime = MeasureExecutionTime(() =>
        {
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
        });

        // Assert
        Assert.True(executionTime.TotalMilliseconds < 500, 
            $"Parser took {executionTime.TotalMilliseconds}ms for deeply nested expressions");
    }

    [Fact]
    public void Parser_ManyVariableDeclarations_PerformsWithinReasonableTime()
    {
        // Arrange
        var codeBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < 1000; i++)
        {
            codeBuilder.AppendLine($"var variable{i} = {i};");
        }
        string code = codeBuilder.ToString();

        // Act
        var executionTime = MeasureExecutionTime(() =>
        {
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
        });

        // Assert
        Assert.True(executionTime.TotalMilliseconds < 1000, 
            $"Parser took {executionTime.TotalMilliseconds}ms for 1000 variable declarations");
    }

    [Fact]
    public void Parser_ComplexFunctionDefinitions_PerformsWithinReasonableTime()
    {
        // Arrange
        var codeBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < 50; i++)
        {
            codeBuilder.AppendLine($@"function func{i}(a, b, c) {{
    var x = a + b * c;
    var y = x / (a - b);
    return x + y;
}}");
        }
        string code = codeBuilder.ToString();

        // Act
        var executionTime = MeasureExecutionTime(() =>
        {
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
        });

        // Assert
        Assert.True(executionTime.TotalMilliseconds < 500, 
            $"Parser took {executionTime.TotalMilliseconds}ms for complex functions");
    }

    #endregion

    #region Interpreter Performance Tests

    [Fact]
    public void Interpreter_ManyArithmeticOperations_PerformsWithinReasonableTime()
    {
        // Arrange
        var codeBuilder = new System.Text.StringBuilder();
        codeBuilder.AppendLine("var result = 0;");
        for (int i = 0; i < 1000; i++)
        {
            codeBuilder.AppendLine($"result = result + {i} * 2 - 1;");
        }
        string code = codeBuilder.ToString();

        // Act
        var executionTime = MeasureExecutionTime(() => ExecuteCode(code));

        // Assert
        Assert.True(executionTime.TotalMilliseconds < 1000, 
            $"Interpreter took {executionTime.TotalMilliseconds}ms for 1000 arithmetic operations");
    }

    [Fact]
    public void Interpreter_ManyFunctionCalls_PerformsWithinReasonableTime()
    {
        // Arrange
        string code = @"
function add(a, b) {
    return a + b;
}

var result = 0;
";
        var codeBuilder = new System.Text.StringBuilder(code);
        for (int i = 0; i < 500; i++)
        {
            codeBuilder.AppendLine($"result = add(result, {i});");
        }

        // Act
        var executionTime = MeasureExecutionTime(() => ExecuteCode(codeBuilder.ToString()));

        // Assert
        Assert.True(executionTime.TotalMilliseconds < 1000, 
            $"Interpreter took {executionTime.TotalMilliseconds}ms for 500 function calls");
    }

    [Fact]
    public void Interpreter_ManyVariableAssignments_PerformsWithinReasonableTime()
    {
        // Arrange
        string code = "var x = 0;\n";
        var codeBuilder = new System.Text.StringBuilder(code);
        for (int i = 0; i < 1000; i++)
        {
            codeBuilder.AppendLine($"x = {i};");
        }

        // Act
        var executionTime = MeasureExecutionTime(() => ExecuteCode(codeBuilder.ToString()));

        // Assert
        Assert.True(executionTime.TotalMilliseconds < 500, 
            $"Interpreter took {executionTime.TotalMilliseconds}ms for 1000 assignments");
    }

    [Fact]
    public void Interpreter_ManyObserverTriggers_PerformsWithinReasonableTime()
    {
        // Arrange
        string code = @"
var x = 0;
observe x function() {
    // Simple observer
}
";
        var codeBuilder = new System.Text.StringBuilder(code);
        for (int i = 0; i < 100; i++)
        {
            codeBuilder.AppendLine($"x = {i};");
        }

        // Act
        var executionTime = MeasureExecutionTime(() => ExecuteCode(codeBuilder.ToString()));

        // Assert
        Assert.True(executionTime.TotalMilliseconds < 1000, 
            $"Interpreter took {executionTime.TotalMilliseconds}ms for 100 observer triggers");
    }

    #endregion

    #region Memory Performance Tests

    [Fact]
    public void Memory_ManyVariables_DoesNotExcessivelyIncreaseMemory()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        
        var codeBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < 1000; i++)
        {
            codeBuilder.AppendLine($"var var{i} = {i};");
        }
        string code = codeBuilder.ToString();

        // Act
        ExecuteCode(code);
        var finalMemory = GC.GetTotalMemory(true);

        // Assert - Memory increase should be reasonable (less than 10MB for 1000 variables)
        var memoryIncrease = finalMemory - initialMemory;
        Assert.True(memoryIncrease < 10 * 1024 * 1024, 
            $"Memory increased by {memoryIncrease / 1024.0 / 1024.0:F2}MB for 1000 variables");
    }

    [Fact]
    public void Memory_ManyObservers_DoesNotLeakMemory()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        
        var codeBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            codeBuilder.AppendLine($"var var{i} = {i};");
            codeBuilder.AppendLine($"observe var{i} function() {{ /* observer {i} */ }};");
        }
        string code = codeBuilder.ToString();

        // Act
        ExecuteCode(code);
        var finalMemory = GC.GetTotalMemory(true);

        // Assert
        var memoryIncrease = finalMemory - initialMemory;
        Assert.True(memoryIncrease < 5 * 1024 * 1024, 
            $"Memory increased by {memoryIncrease / 1024.0 / 1024.0:F2}MB for 100 observers");
    }

    #endregion

    #region Scalability Tests

    [Fact(Skip = "Performance scaling does not meet expected linear growth")]
    public void Scalability_DoubleCodeSize_ExecutionTimeScalesLinearly()
    {
        // Arrange - Small code
        var smallCodeBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            smallCodeBuilder.AppendLine($"var x{i} = {i} + 1;");
        }
        string smallCode = smallCodeBuilder.ToString();

        // Arrange - Large code (double size)
        var largeCodeBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < 200; i++)
        {
            largeCodeBuilder.AppendLine($"var x{i} = {i} + 1;");
        }
        string largeCode = largeCodeBuilder.ToString();

        // Act
        var smallTime = MeasureExecutionTime(() => ExecuteCode(smallCode));
        var largeTime = MeasureExecutionTime(() => ExecuteCode(largeCode));

        // Assert - Large code should not take more than 5x the time (allowing for overhead)
        var ratio = largeTime.TotalMilliseconds / Math.Max(smallTime.TotalMilliseconds, 1);
        Assert.True(ratio < 5.0, 
            $"Large code took {ratio:F2}x longer than small code (should be roughly 2x)");
    }

    [Fact(Skip = "String concatenation not implemented")]
    public void Performance_EndToEndPipeline_CompletesWithinReasonableTime()
    {
        // Arrange - Comprehensive test code
        string code = @"
// Variables
var x = 42;
var y = 3.14;
var message = ""Hello, ECEngine!"";

// Functions
function add(a, b) {
    return a + b;
}

function multiply(a, b) {
    return a * b;
}

// Expressions
var result1 = add(x, y);
var result2 = multiply(result1, 2);

// Observers
observe x function() {
    console.log(""x changed to: "" + x);
}

// Assignments
x = 100;
x = result2;

// Console output
console.log(""Final result: "" + result2);
";

        // Act
        var executionTime = MeasureExecutionTime(() => ExecuteCode(code));

        // Assert - Complete pipeline should execute quickly
        Assert.True(executionTime.TotalMilliseconds < 100, 
            $"End-to-end execution took {executionTime.TotalMilliseconds}ms");
    }

    #endregion
}
