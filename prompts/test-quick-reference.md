# ECEngine Test Quick Reference

This document provides quick copy-paste templates for common testing scenarios in ECEngine.

## Basic Test Templates

### Simple Evaluation Test
```csharp
[Fact]
public void Evaluate_Operation_ReturnsExpectedResult()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var expression = new BinaryExpression(
        new NumberLiteral(1), 
        "+", 
        new NumberLiteral(2)
    );

    // Act
    var result = interpreter.Evaluate(expression, "1 + 2");

    // Assert
    Assert.Equal(3, result);
}
```

### Console Output Test
```csharp
[Fact]
[Collection("ConsoleTests")]
public void Evaluate_ConsoleLog_OutputsCorrectValue()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var consoleIdentifier = new Identifier("console");
    var memberExpression = new MemberExpression(consoleIdentifier, "log");
    var argument = new StringLiteral("Hello World");
    var callExpression = new CallExpression(memberExpression, new List<Expression> { argument });

    var originalConsoleOut = Console.Out;
    using var stringWriter = new StringWriter();
    Console.SetOut(stringWriter);

    try
    {
        // Act
        var result = interpreter.Evaluate(callExpression, "console.log(\"Hello World\")");

        // Assert
        Assert.Null(result);
        var output = stringWriter.ToString().Trim();
        Assert.Equal("Hello World", output);
    }
    finally
    {
        Console.SetOut(originalConsoleOut);
    }
}
```

### Member Access Test
```csharp
[Fact]
public void Evaluate_MemberAccess_ReturnsCorrectMember()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var objectIdentifier = new Identifier("console");
    var memberExpression = new MemberExpression(objectIdentifier, "log");

    // Act
    var result = interpreter.Evaluate(memberExpression, "console.log");

    // Assert
    Assert.IsType<ConsoleLogFunction>(result);
}
```

### Function Call Test
```csharp
[Fact]
public void Evaluate_FunctionCall_ReturnsExpectedResult()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var functionIdentifier = new Identifier("Math");
    var memberExpression = new MemberExpression(functionIdentifier, "abs");
    var argument = new NumberLiteral(-5);
    var callExpression = new CallExpression(memberExpression, new List<Expression> { argument });

    // Act
    var result = interpreter.Evaluate(callExpression, "Math.abs(-5)");

    // Assert
    Assert.Equal(5, result);
}
```

### Constructor Call Test
```csharp
[Fact]
public void Evaluate_Constructor_ReturnsNewInstance()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var constructorIdentifier = new Identifier("Date");
    var callExpression = new CallExpression(constructorIdentifier, new List<Expression>());

    // Act
    var result = interpreter.Evaluate(callExpression, "Date()");

    // Assert
    Assert.IsType<DateObject>(result);
}
```

### Error Handling Test
```csharp
[Fact]
public void Evaluate_InvalidOperation_ThrowsException()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var invalidExpression = new BinaryExpression(
        new StringLiteral("hello"), 
        "/", 
        new NumberLiteral(0)
    );

    // Act & Assert
    var exception = Assert.Throws<ECEngineException>(() => 
        interpreter.Evaluate(invalidExpression, "\"hello\" / 0")
    );
    
    Assert.Contains("Cannot divide", exception.Message);
}
```

### Parameterized Test
```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(5, 3, 8)]
[InlineData(-1, 1, 0)]
public void Evaluate_Addition_ReturnsCorrectSum(int left, int right, int expected)
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var expression = new BinaryExpression(
        new NumberLiteral(left), 
        "+", 
        new NumberLiteral(right)
    );

    // Act
    var result = interpreter.Evaluate(expression, $"{left} + {right}");

    // Assert
    Assert.Equal(expected, result);
}
```

## Lexer Test Templates

### Basic Tokenization Test
```csharp
[Fact]
public void Tokenize_Input_ReturnsExpectedTokens()
{
    // Arrange
    var lexer = new ECEngine.Lexer.Lexer("console.log");

    // Act
    var tokens = lexer.Tokenize();

    // Assert
    Assert.Equal(4, tokens.Count); // Identifier, Dot, Identifier, EOF
    Assert.Equal(TokenType.Identifier, tokens[0].Type);
    Assert.Equal("console", tokens[0].Value);
    Assert.Equal(TokenType.Dot, tokens[1].Type);
    Assert.Equal(TokenType.Identifier, tokens[2].Type);
    Assert.Equal("log", tokens[2].Value);
    Assert.Equal(TokenType.EOF, tokens[3].Type);
}
```

### String Tokenization Test
```csharp
[Fact]
public void Tokenize_StringLiteral_ReturnsStringToken()
{
    // Arrange
    var lexer = new ECEngine.Lexer.Lexer("\"Hello World\"");

    // Act
    var tokens = lexer.Tokenize();

    // Assert
    Assert.Equal(2, tokens.Count); // String + EOF
    Assert.Equal(TokenType.String, tokens[0].Type);
    Assert.Equal("Hello World", tokens[0].Value);
}
```

## Parser Test Templates

### Expression Parsing Test
```csharp
[Fact]
public void Parse_Expression_ReturnsCorrectAST()
{
    // Arrange
    var parser = new ECEngine.Parser.Parser();

    // Act
    var ast = parser.Parse("1 + 2 * 3");

    // Assert
    var program = Assert.IsType<ProgramNode>(ast);
    var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
    var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

    Assert.Equal("+", binaryExpression.Operator);
    Assert.IsType<NumberLiteral>(binaryExpression.Left);
    Assert.IsType<BinaryExpression>(binaryExpression.Right); // 2 * 3
}
```

### Function Declaration Parsing Test
```csharp
[Fact]
public void Parse_FunctionDeclaration_ReturnsCorrectAST()
{
    // Arrange
    var parser = new ECEngine.Parser.Parser();

    // Act
    var ast = parser.Parse("function add(a, b) { return a + b; }");

    // Assert
    var program = Assert.IsType<ProgramNode>(ast);
    var functionDeclaration = Assert.IsType<FunctionDeclaration>(program.Body[0]);

    Assert.Equal("add", functionDeclaration.Name.Name);
    Assert.Equal(2, functionDeclaration.Parameters.Count);
    Assert.Equal("a", functionDeclaration.Parameters[0].Name);
    Assert.Equal("b", functionDeclaration.Parameters[1].Name);
}
```

## Runtime Test Templates

### EventLoop Test
```csharp
[Fact]
public void EventLoop_Operation_BehavesCorrectly()
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
```

### Timer Test
```csharp
[Fact]
public void EventLoop_SetTimeout_ExecutesAfterDelay()
{
    // Arrange
    var eventLoop = new EventLoop();
    var executed = false;
    var startTime = DateTime.UtcNow;

    // Act
    eventLoop.SetTimeout(() => executed = true, 10);
    eventLoop.Run();
    var endTime = DateTime.UtcNow;

    // Assert
    Assert.True(executed);
    Assert.True((endTime - startTime).TotalMilliseconds >= 5);
}
```

## Global Object Test Templates

### Date Object Tests
```csharp
[Fact]
public void Evaluate_DateNow_ReturnsTimestamp()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var dateIdentifier = new Identifier("Date");
    var memberExpression = new MemberExpression(dateIdentifier, "now");
    var callExpression = new CallExpression(memberExpression, new List<Expression>());

    // Act
    var result = interpreter.Evaluate(callExpression, "Date.now()");

    // Assert
    Assert.IsType<double>(result);
    var timestamp = (double)result;
    Assert.True(timestamp > 0);
}

[Fact]
public void Evaluate_DateConstructor_ReturnsDateObject()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var dateIdentifier = new Identifier("Date");
    var args = new List<Expression>
    {
        new NumberLiteral(2023),
        new NumberLiteral(11),
        new NumberLiteral(25)
    };
    var callExpression = new CallExpression(dateIdentifier, args);

    // Act
    var result = interpreter.Evaluate(callExpression, "Date(2023, 11, 25)");

    // Assert
    Assert.IsType<DateObject>(result);
    var dateObj = (DateObject)result;
    Assert.Equal(2023, dateObj.Value.Year);
    Assert.Equal(12, dateObj.Value.Month); // Month is 0-indexed in JS, 1-indexed in C#
}

[Fact]
public void Evaluate_DateMethod_ReturnsCorrectValue()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var dateIdentifier = new Identifier("Date");
    var dateCall = new CallExpression(dateIdentifier, new List<Expression>
    {
        new NumberLiteral(2023), new NumberLiteral(11), new NumberLiteral(25)
    });
    
    // First create the date object (this would typically be done in setup)
    var dateObj = interpreter.Evaluate(dateCall, "Date(2023, 11, 25)");
    
    // Create a member access for the method
    var memberExpression = new MemberExpression(/* dateObj reference */, "getFullYear");
    var methodCall = new CallExpression(memberExpression, new List<Expression>());

    // Act
    var result = interpreter.Evaluate(methodCall, "date.getFullYear()");

    // Assert
    Assert.Equal(2023, result);
}
```

### Math Object Tests
```csharp
[Fact]
public void Evaluate_MathAbs_ReturnsAbsoluteValue()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var mathIdentifier = new Identifier("Math");
    var memberExpression = new MemberExpression(mathIdentifier, "abs");
    var argument = new NumberLiteral(-5);
    var callExpression = new CallExpression(memberExpression, new List<Expression> { argument });

    // Act
    var result = interpreter.Evaluate(callExpression, "Math.abs(-5)");

    // Assert
    Assert.Equal(5, result);
}

[Theory]
[InlineData("PI", 3.141592653589793)]
[InlineData("E", 2.718281828459045)]
public void Evaluate_MathConstant_ReturnsCorrectValue(string constant, double expected)
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var mathIdentifier = new Identifier("Math");
    var memberExpression = new MemberExpression(mathIdentifier, constant);

    // Act
    var result = interpreter.Evaluate(memberExpression, $"Math.{constant}");

    // Assert
    Assert.Equal(expected, (double)result, 10); // 10 decimal places precision
}
```

## Performance Test Templates

### Basic Performance Test
```csharp
[Fact]
public void Performance_Operation_CompletesInTime()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    const int iterations = 1000;

    // Act
    for (int i = 0; i < iterations; i++)
    {
        var expression = new NumberLiteral(i);
        interpreter.Evaluate(expression, i.ToString());
    }
    
    stopwatch.Stop();

    // Assert
    Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
        $"Operation took {stopwatch.ElapsedMilliseconds}ms, expected less than 1000ms");
}
```

### Memory Test Template
```csharp
[Fact]
public void Memory_LargeOperations_DoesNotLeak()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var initialMemory = GC.GetTotalMemory(true);

    // Act
    for (int i = 0; i < 10000; i++)
    {
        var expression = new NumberLiteral(i);
        interpreter.Evaluate(expression, i.ToString());
    }

    // Force garbage collection
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    var finalMemory = GC.GetTotalMemory(false);

    // Assert
    var memoryIncrease = finalMemory - initialMemory;
    Assert.True(memoryIncrease < 1024 * 1024, // Less than 1MB increase
        $"Memory increased by {memoryIncrease} bytes, expected less than 1MB");
}
```

## Integration Test Templates

### End-to-End Test
```csharp
[Fact]
public void Integration_FullPipeline_WorksCorrectly()
{
    // Arrange
    var code = "console.log(Date.now());";
    var lexer = new ECEngine.Lexer.Lexer(code);
    var parser = new ECEngine.Parser.Parser();
    var interpreter = new RuntimeInterpreter();

    var originalConsoleOut = Console.Out;
    using var stringWriter = new StringWriter();
    Console.SetOut(stringWriter);

    try
    {
        // Act
        var tokens = lexer.Tokenize();
        var ast = parser.Parse(tokens);
        var result = interpreter.Execute(ast);

        // Assert
        var output = stringWriter.ToString().Trim();
        Assert.True(double.TryParse(output, out var timestamp));
        Assert.True(timestamp > 0);
    }
    finally
    {
        Console.SetOut(originalConsoleOut);
    }
}
```

## Common Assertions

```csharp
// Type assertions
Assert.IsType<DateObject>(result);
Assert.IsAssignableFrom<IFunction>(result);

// Null/Not null
Assert.Null(result);
Assert.NotNull(result);

// Equality
Assert.Equal(expected, actual);
Assert.NotEqual(unexpected, actual);

// Numeric comparisons
Assert.True(value > 0);
Assert.InRange(value, min, max);

// String assertions
Assert.Contains("substring", fullString);
Assert.StartsWith("prefix", text);
Assert.EndsWith("suffix", text);

// Collections
Assert.Empty(collection);
Assert.NotEmpty(collection);
Assert.Single(collection);
Assert.Equal(expectedCount, collection.Count);

// Exceptions
Assert.Throws<ECEngineException>(() => operation());
var ex = Assert.Throws<ECEngineException>(() => operation());
Assert.Contains("expected message", ex.Message);
```

## Quick Setup Snippets

### Test Class Header
```csharp
using Xunit;
using ECEngine.AST;
using ECEngine.Runtime;
using System.IO;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.YourCategory;

[Collection("ConsoleTests")] // Only if needed
public class YourFeatureTests
{
    // Tests go here
}
```

### Console Capture Setup
```csharp
var originalConsoleOut = Console.Out;
using var stringWriter = new StringWriter();
Console.SetOut(stringWriter);

try
{
    // Your test code
    var output = stringWriter.ToString().Trim();
    Assert.Equal("expected", output);
}
finally
{
    Console.SetOut(originalConsoleOut);
}
```

Use these templates as starting points and modify them based on your specific testing needs!
