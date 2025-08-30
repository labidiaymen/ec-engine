# ECEngine Test Writing Best Practices

This document outlines the best practices for writing tests in ECEngine based on the existing test patterns and architecture.

## Table of Contents
1. [Test Organization](#test-organization)
2. [Naming Conventions](#naming-conventions)
3. [Test Structure](#test-structure)
4. [Testing Framework](#testing-framework)
5. [Component-Specific Patterns](#component-specific-patterns)
6. [Common Patterns](#common-patterns)
7. [Test Categories](#test-categories)
8. [Examples](#examples)

## Test Organization

### Directory Structure
```
Tests/
├── ConsoleTestCollection.cs        # Test collection configurations
├── Lexer/                         # Lexer component tests
│   ├── BasicTokenizationTests.cs
│   ├── CommentTokenizationTests.cs
│   └── ...
├── Parser/                        # Parser component tests
│   ├── BinaryExpressionParsingTests.cs
│   ├── FunctionCallParsingTests.cs
│   └── ...
├── Interpreter/                   # Interpreter component tests
│   ├── FunctionCallEvaluationTests.cs
│   └── ...
├── Runtime/                       # Runtime component tests
│   ├── EventLoopTests.cs
│   └── ...
├── Integration/                   # End-to-end tests
├── Performance/                   # Performance tests
└── ErrorHandling/                 # Error handling tests
```

### File Naming
- Use descriptive names ending with `Tests.cs`
- Group related functionality: `BasicTokenizationTests.cs`, `CommentTokenizationTests.cs`
- Use component prefixes when testing specific features: `FunctionCallEvaluationTests.cs`

## Naming Conventions

### Test Method Names
Follow the pattern: `Verb_Scenario_ExpectedResult`

**Good Examples:**
```csharp
[Fact]
public void Tokenize_SimpleNumber_ReturnsNumberToken()

[Fact]
public void Parse_SimpleAddition_ReturnsBinaryExpression()

[Fact]
public void Evaluate_ConsoleLogWithNumber_ReturnsNull()

[Fact]
public void EventLoop_NextTick_ShouldExecuteTaskOnNextTick()
```

**Bad Examples:**
```csharp
[Fact]
public void Test1() // Not descriptive

[Fact]
public void NumberTest() // Vague

[Fact]
public void TestConsoleLog() // Doesn't describe scenario or expected result
```

## Test Structure

### Arrange-Act-Assert Pattern
Always use the AAA pattern with clear comments:

```csharp
[Fact]
public void Evaluate_ConsoleLogWithNumber_ReturnsNull()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var consoleIdentifier = new Identifier("console");
    var memberExpression = new MemberExpression(consoleIdentifier, "log");
    var argument = new NumberLiteral(42);
    var callExpression = new CallExpression(memberExpression, new List<Expression> { argument });

    // Act
    var result = interpreter.Evaluate(callExpression, "console.log(42)");

    // Assert
    Assert.Null(result);
}
```

### Resource Management
Always properly dispose of resources and reset global state:

```csharp
[Fact]
public void Evaluate_ConsoleLogWithNumber_OutputsCorrectValue()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var originalConsoleOut = Console.Out;
    using var stringWriter = new StringWriter();
    Console.SetOut(stringWriter);

    try
    {
        // Act
        interpreter.Evaluate(callExpression, "console.log(42)");

        // Assert
        var output = stringWriter.ToString().Trim();
        Assert.Equal("42", output);
    }
    finally
    {
        Console.SetOut(originalConsoleOut);
    }
}
```

## Testing Framework

### XUnit Attributes
- Use `[Fact]` for simple tests
- Use `[Theory]` with `[InlineData]` for parameterized tests
- Use `[Collection("ConsoleTests")]` for tests that modify console output

### Collections
For tests that cannot run in parallel (e.g., console manipulation):

```csharp
[Collection("ConsoleTests")]
public class FunctionCallEvaluationTests
{
    // Tests that modify global state like Console
}
```

## Component-Specific Patterns

### Lexer Tests
Focus on tokenization correctness:

```csharp
[Fact]
public void Tokenize_SimpleNumber_ReturnsNumberToken()
{
    // Arrange
    var lexer = new ECEngine.Lexer.Lexer("42");

    // Act
    var tokens = lexer.Tokenize();

    // Assert
    Assert.Equal(2, tokens.Count); // Number + EOF
    Assert.Equal(TokenType.Number, tokens[0].Type);
    Assert.Equal("42", tokens[0].Value);
    Assert.Equal(TokenType.EOF, tokens[1].Type);
}
```

### Parser Tests
Focus on AST structure correctness:

```csharp
[Fact]
public void Parse_SimpleAddition_ReturnsBinaryExpression()
{
    // Arrange
    var parser = new ECEngine.Parser.Parser();

    // Act
    var ast = parser.Parse("1 + 2");

    // Assert
    var program = Assert.IsType<ProgramNode>(ast);
    var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
    var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

    Assert.Equal("+", binaryExpression.Operator);

    var left = Assert.IsType<NumberLiteral>(binaryExpression.Left);
    Assert.Equal(1, left.Value);

    var right = Assert.IsType<NumberLiteral>(binaryExpression.Right);
    Assert.Equal(2, right.Value);
}
```

### Interpreter Tests
Use AST nodes directly rather than parsing strings:

```csharp
[Fact]
public void Evaluate_BinaryAddition_ReturnsCorrectSum()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var binaryExpression = new BinaryExpression(
        new NumberLiteral(1), 
        "+", 
        new NumberLiteral(2)
    );

    // Act
    var result = interpreter.Evaluate(binaryExpression, "1 + 2");

    // Assert
    Assert.Equal(3, result);
}
```

### Runtime Tests
Focus on behavior and state management:

```csharp
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
```

## Common Patterns

### Testing Functions and Method Calls
Create AST nodes manually for predictable testing:

```csharp
// For member expressions (obj.method)
var memberExpression = new MemberExpression(
    new Identifier("console"), 
    "log"
);

// For function calls
var callExpression = new CallExpression(
    memberExpression, 
    new List<Expression> { new NumberLiteral(42) }
);
```

### Testing Global Objects
Test both member access and function calls:

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
```

### Testing Constructors
Handle both function calls and constructor patterns:

```csharp
[Fact]
public void Evaluate_DateConstructor_ReturnsDateObject()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var dateIdentifier = new Identifier("Date");
    var callExpression = new CallExpression(dateIdentifier, new List<Expression>());

    // Act
    var result = interpreter.Evaluate(callExpression, "Date()");

    // Assert
    Assert.IsType<DateObject>(result);
}
```

### Error Testing
Test both expected errors and edge cases:

```csharp
[Fact]
public void Evaluate_InvalidOperation_ThrowsException()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var invalidExpression = new BinaryExpression(
        new StringLiteral("hello"), 
        "+", 
        new NullLiteral()
    );

    // Act & Assert
    var exception = Assert.Throws<ECEngineException>(() => 
        interpreter.Evaluate(invalidExpression, "\"hello\" + null")
    );
    
    Assert.Contains("Cannot add", exception.Message);
}
```

## Test Categories

### 1. Unit Tests
Test individual components in isolation:
- Single method behavior
- Edge cases
- Error conditions
- Boundary values

### 2. Integration Tests
Test component interactions:
- Lexer → Parser → Interpreter flow
- Runtime object interactions
- Module system functionality

### 3. Performance Tests
Test execution performance:
- Large input handling
- Memory usage
- Execution time benchmarks

### 4. Error Handling Tests
Test error scenarios:
- Syntax errors
- Runtime errors
- Type errors
- Null reference handling

## Examples

### Complete Test Class Template

```csharp
using Xunit;
using ECEngine.AST;
using ECEngine.Runtime;
using System.IO;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Runtime;

[Collection("ConsoleTests")] // Only if needed for console tests
public class DateObjectTests
{
    [Fact]
    public void Evaluate_DateNow_ReturnsCurrentTimestamp()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var memberExpression = new MemberExpression(dateIdentifier, "now");
        var callExpression = new CallExpression(memberExpression, new List<Expression>());
        var timestampBefore = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Act
        var result = interpreter.Evaluate(callExpression, "Date.now()");

        // Assert
        Assert.IsType<double>(result);
        var timestamp = (double)result;
        var timestampAfter = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        Assert.True(timestamp >= timestampBefore);
        Assert.True(timestamp <= timestampAfter + 1000); // Allow 1 second tolerance
    }

    [Fact]
    public void Evaluate_DateConstructorWithNoArgs_ReturnsDateObject()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var callExpression = new CallExpression(dateIdentifier, new List<Expression>());

        // Act
        var result = interpreter.Evaluate(callExpression, "Date()");

        // Assert
        Assert.IsType<DateObject>(result);
        var dateObj = (DateObject)result;
        Assert.True(dateObj.Value <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData(2023, 11, 25)] // December 25, 2023 (month is 0-indexed)
    [InlineData(2024, 0, 1)]   // January 1, 2024
    public void Evaluate_DateConstructorWithYearMonthDay_ReturnsCorrectDate(int year, int month, int day)
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var args = new List<Expression>
        {
            new NumberLiteral(year),
            new NumberLiteral(month),
            new NumberLiteral(day)
        };
        var callExpression = new CallExpression(dateIdentifier, args);

        // Act
        var result = interpreter.Evaluate(callExpression, $"Date({year}, {month}, {day})");

        // Assert
        Assert.IsType<DateObject>(result);
        var dateObj = (DateObject)result;
        Assert.Equal(year, dateObj.Value.Year);
        Assert.Equal(month + 1, dateObj.Value.Month); // Convert from 0-indexed to 1-indexed
        Assert.Equal(day, dateObj.Value.Day);
    }
}
```

### Performance Test Template

```csharp
[Fact]
public void Performance_LargeNumberOfDateObjects_CompletesInReasonableTime()
{
    // Arrange
    var interpreter = new RuntimeInterpreter();
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    const int iterations = 1000;

    // Act
    for (int i = 0; i < iterations; i++)
    {
        var dateIdentifier = new Identifier("Date");
        var callExpression = new CallExpression(dateIdentifier, new List<Expression>());
        interpreter.Evaluate(callExpression, "Date()");
    }
    
    stopwatch.Stop();

    // Assert
    Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
        $"Creating {iterations} Date objects took {stopwatch.ElapsedMilliseconds}ms, expected less than 1000ms");
}
```

## Key Principles

1. **Isolation**: Each test should be independent and not rely on other tests
2. **Clarity**: Test names and structure should clearly communicate intent
3. **Completeness**: Test happy paths, edge cases, and error conditions
4. **Performance**: Tests should run quickly and efficiently
5. **Maintainability**: Tests should be easy to update when code changes
6. **Reliability**: Tests should be deterministic and not flaky

## When to Use Each Pattern

- **Lexer Tests**: Use when testing tokenization logic
- **Parser Tests**: Use when testing AST generation
- **Interpreter Tests**: Use when testing evaluation logic
- **Runtime Tests**: Use when testing runtime behavior and state
- **Integration Tests**: Use when testing full pipeline functionality
- **Performance Tests**: Use for benchmarking and regression testing

Remember to always follow the existing patterns in the codebase and maintain consistency with the established testing architecture.
