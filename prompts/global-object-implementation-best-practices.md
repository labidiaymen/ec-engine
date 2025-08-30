# ECEngine Global Object Implementation Best Practices

This document outlines the best practices for implementing JavaScript-like global objects in ECEngine, using the Date and Math objects as reference examples.

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [File Structure](#file-structure)
3. [Implementation Patterns](#implementation-patterns)
4. [Integration Steps](#integration-steps)
5. [Testing Strategy](#testing-strategy)
6. [Complete Examples](#complete-examples)
7. [Common Pitfalls](#common-pitfalls)

## Architecture Overview

Global objects in ECEngine follow a consistent pattern:

1. **Module Class**: Main object that represents the global (e.g., `DateModule`, `MathModule`)
2. **Function Classes**: Individual functions/methods (e.g., `DateNowFunction`, `MathMethodFunction`)
3. **Object Classes**: Instance objects when applicable (e.g., `DateObject`)
4. **Integration**: Registration in `Interpreter.cs` for recognition and evaluation

## File Structure

### Recommended File Organization
```
Runtime/
├── DateGlobals.cs      # Date object implementation
├── MathGlobals.cs      # Math object implementation
├── ConsoleGlobals.cs   # Console object (existing)
├── HttpGlobals.cs      # HTTP objects (existing)
└── Interpreter.cs     # Integration point
```

### File Naming Convention
- `{ObjectName}Globals.cs` for global object implementations
- Place all related classes in the same file for cohesion
- Use the `ECEngine.Runtime` namespace

## Implementation Patterns

### 1. Module Class Pattern

The main module class represents the global object itself:

```csharp
/// <summary>
/// Math global object for ECEngine scripts
/// </summary>
public class MathModule
{
    // Constants as properties
    public double PI => Math.PI;
    public double E => Math.E;
    public double LN2 => Math.Log(2);
    
    // Function instances
    public MathAbsFunction Abs { get; } = new MathAbsFunction();
    public MathSinFunction Sin { get; } = new MathSinFunction();
    // ... other functions
}
```

**Key Points:**
- Use descriptive XML documentation
- Constants as properties with getters
- Functions as property instances
- Follow JavaScript naming conventions (e.g., `PI`, not `pi`)

### 2. Static Function Pattern

For static methods (like `Math.abs()`, `Date.now()`):

```csharp
/// <summary>
/// Math.abs() function
/// </summary>
public class MathAbsFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            return double.NaN;
            
        var value = arguments[0];
        
        // Handle different number types
        if (value is double d) return Math.Abs(d);
        if (value is int i) return Math.Abs(i);
        if (value is long l) return Math.Abs(l);
        
        // Try to convert to double
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Abs(parsed);
            
        return double.NaN;
    }
}
```

**Key Points:**
- Always implement `Call(List<object?> arguments)` method
- Handle argument count validation
- Support multiple numeric types
- Return appropriate JavaScript-like values (e.g., `NaN` for invalid inputs)
- Use null-safe operations

### 3. Constructor Function Pattern

For objects that can be instantiated (like `Date`):

```csharp
/// <summary>
/// Date constructor function
/// </summary>
public class DateConstructorFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
        {
            // new Date() - current date/time
            return new DateObject(DateTime.Now);
        }
        else if (arguments.Count == 1)
        {
            // Handle single argument (milliseconds or string)
            var arg = arguments[0];
            
            if (arg is double || arg is int || arg is long)
            {
                var milliseconds = Convert.ToInt64(arg);
                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds(milliseconds);
                return new DateObject(dateTime);
            }
            
            if (arg is string dateString)
            {
                if (DateTime.TryParse(dateString, out var parsedDate))
                    return new DateObject(parsedDate);
                else
                    return new DateObject(DateTime.MinValue); // Invalid Date
            }
        }
        // Handle multiple arguments...
        
        return new DateObject(DateTime.Now);
    }
}
```

### 4. Instance Object Pattern

For objects with instance methods:

```csharp
/// <summary>
/// Date object that wraps a DateTime and provides JavaScript Date methods
/// </summary>
public class DateObject
{
    private readonly DateTime _dateTime;
    private readonly bool _isValid;

    public DateObject(DateTime dateTime)
    {
        _dateTime = dateTime;
        _isValid = dateTime != DateTime.MinValue;
    }

    public DateTime DateTime => _dateTime;
    public bool IsValid => _isValid;

    // Instance methods
    public object? GetTime(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var milliseconds = (long)(_dateTime.ToUniversalTime() - epoch).TotalMilliseconds;
        return (double)milliseconds;
    }

    public object? GetFullYear(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)_dateTime.Year;
    }
    
    // ... other instance methods
}
```

### 5. Method Function Wrapper Pattern

For cleaner method access:

```csharp
/// <summary>
/// Helper class for Date instance method function calls
/// </summary>
public class DateMethodFunction
{
    private readonly DateObject _dateObject;
    private readonly string _methodName;

    public DateMethodFunction(DateObject dateObject, string methodName)
    {
        _dateObject = dateObject;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        return _methodName switch
        {
            "getTime" => _dateObject.GetTime(arguments),
            "getFullYear" => _dateObject.GetFullYear(arguments),
            "getMonth" => _dateObject.GetMonth(arguments),
            // ... all other methods
            _ => throw new ECEngineException($"Unknown Date method: {_methodName}", 0, 0, "", "Runtime error")
        };
    }
}
```

## Integration Steps

### Step 1: Register in EvaluateIdentifier

Add your global object to the identifier switch statement:

```csharp
private object? EvaluateIdentifier(Identifier identifier)
{
    // ... existing code ...
    
    var result = identifier.Name switch
    {
        "console" => (object?)new ConsoleObject(),
        "Date" => (object?)new DateModule(),
        "Math" => (object?)new MathModule(),
        "YourNewObject" => (object?)new YourNewModule(),
        _ => null
    };
    
    // ... rest of method
}
```

### Step 2: Handle Member Access in EvaluateMemberExpression

Add property/method access handling:

```csharp
private object? EvaluateMemberExpression(MemberExpression member)
{
    var obj = Evaluate(member.Object, _sourceCode);
    
    // ... existing objects ...
    
    // Handle YourNewObject static methods/constants
    if (obj is YourNewModule yourModule)
    {
        return member.Property switch
        {
            // Constants
            "SOME_CONSTANT" => yourModule.SomeConstant,
            
            // Functions  
            "someMethod" => new YourMethodFunction(yourModule.SomeMethod, "someMethod"),
            
            _ => throw new ECEngineException($"Property {member.Property} not found on YourNewObject",
                member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                $"The property '{member.Property}' does not exist on the YourNewObject object")
        };
    }
    
    // Handle instance methods if applicable
    if (obj is YourObjectInstance yourInstance)
    {
        return member.Property switch
        {
            "instanceMethod" => new YourInstanceMethodFunction(yourInstance, "instanceMethod"),
            _ => throw new ECEngineException($"Property {member.Property} not found on YourObject instance",
                member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                $"The property '{member.Property}' does not exist on the YourObject instance")
        };
    }
    
    // ... rest of method
}
```

### Step 3: Handle Function Calls in EvaluateCallExpression

Add function call handling:

```csharp
private object? EvaluateCallExpression(CallExpression call)
{
    var function = Evaluate(call.Callee, _sourceCode);
    var arguments = call.Arguments.Select(arg => Evaluate(arg, _sourceCode)).ToList();
    
    // ... existing function handlers ...
    
    // Handle YourNewObject constructor if applicable
    if (function is YourNewModule yourModuleAsFunc)
    {
        return yourModuleAsFunc.Constructor.Call(arguments);
    }
    
    if (function is YourConstructorFunction yourConstructorFunc)
    {
        return yourConstructorFunc.Call(arguments);
    }
    
    if (function is YourMethodFunction yourMethodFunc)
    {
        return yourMethodFunc.Call(arguments);
    }
    
    if (function is YourInstanceMethodFunction yourInstanceMethodFunc)
    {
        return yourInstanceMethodFunc.Call(arguments);
    }
    
    // ... rest of method
}
```

## Testing Strategy

### Test Organization
Create tests in `Tests/Runtime/YourObjectTests.cs`:

```csharp
using Xunit;
using ECEngine.AST;
using ECEngine.Runtime;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Runtime;

public class YourObjectTests
{
    #region Static Methods Tests
    
    [Fact]
    public void Evaluate_YourObjectMethod_ReturnsExpectedValue()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var objectIdentifier = new Identifier("YourObject");
        var memberExpression = new MemberExpression(objectIdentifier, "method");
        var callExpression = new CallExpression(memberExpression, new List<Expression>
        {
            new NumberLiteral(42)
        });

        // Act
        var result = interpreter.Evaluate(callExpression, "YourObject.method(42)");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(42.0, (double)result);
    }
    
    #endregion
    
    #region Constructor Tests
    
    [Fact]
    public void Evaluate_YourObjectConstructor_ReturnsInstance()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var objectIdentifier = new Identifier("YourObject");
        var callExpression = new CallExpression(objectIdentifier, new List<Expression>());

        // Act
        var result = interpreter.Evaluate(callExpression, "YourObject()");

        // Assert
        Assert.IsType<YourObjectInstance>(result);
    }
    
    #endregion
    
    #region Instance Method Tests
    
    [Fact]
    public void Evaluate_YourObjectInstanceMethod_ReturnsExpectedValue()
    {
        // Arrange - Create instance first, then test method
        var interpreter = new RuntimeInterpreter();
        var objectIdentifier = new Identifier("YourObject");
        var constructorCall = new CallExpression(objectIdentifier, new List<Expression>());
        
        var instance = interpreter.Evaluate(constructorCall, "YourObject()");
        Assert.IsType<YourObjectInstance>(instance);
        
        var yourInstance = (YourObjectInstance)instance;
        var result = yourInstance.SomeMethod(new List<object?>());

        // Assert
        Assert.NotNull(result);
    }
    
    #endregion
    
    #region Error Handling Tests
    
    [Fact]
    public void Evaluate_YourObjectInvalidMethod_ThrowsException()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var objectIdentifier = new Identifier("YourObject");
        var memberExpression = new MemberExpression(objectIdentifier, "nonExistentMethod");
        var callExpression = new CallExpression(memberExpression, new List<Expression>());

        // Act & Assert
        var exception = Assert.Throws<ECEngineException>(() => 
            interpreter.Evaluate(callExpression, "YourObject.nonExistentMethod()")
        );
        
        Assert.Contains("not found", exception.Message);
    }
    
    #endregion
    
    #region Performance Tests
    
    [Fact]
    public void Performance_CreateManyInstances_CompletesInReasonableTime()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        const int iterations = 1000;

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var objectIdentifier = new Identifier("YourObject");
            var callExpression = new CallExpression(objectIdentifier, new List<Expression>());
            interpreter.Evaluate(callExpression, "YourObject()");
        }
        
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
            $"Creating {iterations} instances took {stopwatch.ElapsedMilliseconds}ms, expected less than 2000ms");
    }
    
    #endregion
}
```

### Test Categories to Include
1. **Static Methods Tests**: Test all static functions
2. **Constructor Tests**: Test object instantiation with various arguments
3. **Instance Method Tests**: Test methods on created instances
4. **Constants Tests**: Verify constant values
5. **Error Handling Tests**: Invalid inputs, missing methods
6. **Performance Tests**: Ensure reasonable performance
7. **Integration Tests**: End-to-end workflows

## Complete Examples

### Example 1: Math Object Implementation

```csharp
namespace ECEngine.Runtime;

/// <summary>
/// Math global object for ECEngine scripts
/// </summary>
public class MathModule
{
    // Mathematical constants
    public double E => Math.E;
    public double PI => Math.PI;
    public double LN2 => Math.Log(2);
    public double LN10 => Math.Log(10);
    public double LOG2E => Math.Log2(Math.E);
    public double LOG10E => Math.Log10(Math.E);
    public double SQRT1_2 => Math.Sqrt(0.5);
    public double SQRT2 => Math.Sqrt(2);

    // Function instances
    public MathAbsFunction Abs { get; } = new MathAbsFunction();
    public MathFloorFunction Floor { get; } = new MathFloorFunction();
    public MathCeilFunction Ceil { get; } = new MathCeilFunction();
    public MathRoundFunction Round { get; } = new MathRoundFunction();
    public MathMaxFunction Max { get; } = new MathMaxFunction();
    public MathMinFunction Min { get; } = new MathMinFunction();
    public MathPowFunction Pow { get; } = new MathPowFunction();
    public MathSqrtFunction Sqrt { get; } = new MathSqrtFunction();
    public MathRandomFunction Random { get; } = new MathRandomFunction();
    
    // Trigonometric functions
    public MathSinFunction Sin { get; } = new MathSinFunction();
    public MathCosFunction Cos { get; } = new MathCosFunction();
    public MathTanFunction Tan { get; } = new MathTanFunction();
    public MathAsinFunction Asin { get; } = new MathAsinFunction();
    public MathAcosFunction Acos { get; } = new MathAcosFunction();
    public MathAtanFunction Atan { get; } = new MathAtanFunction();
    public MathAtan2Function Atan2 { get; } = new MathAtan2Function();
    
    // Logarithmic functions
    public MathLogFunction Log { get; } = new MathLogFunction();
    public MathExpFunction Exp { get; } = new MathExpFunction();
    
    // Other functions
    public MathTruncFunction Trunc { get; } = new MathTruncFunction();
}

/// <summary>
/// Math.abs() function
/// </summary>
public class MathAbsFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            return double.NaN;
            
        var value = arguments[0];
        
        if (value is double d) return Math.Abs(d);
        if (value is int i) return Math.Abs(i);
        if (value is long l) return Math.Abs(l);
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Abs(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Helper class for Math method function calls
/// </summary>
public class MathMethodFunction
{
    private readonly object _mathFunction;
    private readonly string _methodName;

    public MathMethodFunction(object mathFunction, string methodName)
    {
        _mathFunction = mathFunction;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        return _mathFunction switch
        {
            MathAbsFunction abs => abs.Call(arguments),
            MathFloorFunction floor => floor.Call(arguments),
            MathCeilFunction ceil => ceil.Call(arguments),
            // ... all other functions
            _ => throw new ECEngineException($"Unknown Math method: {_methodName}", 0, 0, "", "Runtime error")
        };
    }
}
```

### Example 2: Date Object Implementation

```csharp
namespace ECEngine.Runtime;

/// <summary>
/// Date global object for ECEngine scripts
/// </summary>
public class DateModule
{
    public DateConstructorFunction Constructor { get; } = new DateConstructorFunction();
    public DateNowFunction Now { get; } = new DateNowFunction();
    public DateParseFunction Parse { get; } = new DateParseFunction();
    public DateUTCFunction UTC { get; } = new DateUTCFunction();
}

/// <summary>
/// Date constructor function
/// </summary>
public class DateConstructorFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
        {
            return new DateObject(DateTime.Now);
        }
        // Handle other argument patterns...
        
        return new DateObject(DateTime.Now);
    }
}

/// <summary>
/// Date object instance
/// </summary>
public class DateObject
{
    private readonly DateTime _dateTime;
    private readonly bool _isValid;

    public DateObject(DateTime dateTime)
    {
        _dateTime = dateTime;
        _isValid = dateTime != DateTime.MinValue;
    }

    public DateTime DateTime => _dateTime;
    public bool IsValid => _isValid;

    public object? GetTime(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var milliseconds = (long)(_dateTime.ToUniversalTime() - epoch).TotalMilliseconds;
        return (double)milliseconds;
    }
    
    // ... other instance methods
}
```

## Common Pitfalls

### 1. Forgetting Integration Steps
❌ **Wrong**: Implementing the object but not registering it in Interpreter.cs
✅ **Right**: Always update all three methods in Interpreter.cs

### 2. Inconsistent Return Types
❌ **Wrong**: Returning `int` sometimes, `double` other times
✅ **Right**: Use consistent types (`double` for numbers, `string` for text)

### 3. Poor Error Handling
❌ **Wrong**: Throwing generic exceptions or returning `null`
✅ **Right**: Return JavaScript-like values (`NaN`, appropriate error messages)

### 4. Missing Argument Validation
❌ **Wrong**: Assuming arguments exist and are correct type
✅ **Right**: Always check argument count and types

### 5. Not Following JavaScript Semantics
❌ **Wrong**: Using C# conventions (0-based months, different string formats)
✅ **Right**: Follow JavaScript behavior (0-based months for Date, specific formatting)

### 6. Memory Leaks in Static Objects
❌ **Wrong**: Creating new instances every time
✅ **Right**: Use static instances or proper cleanup

### 7. Inadequate Testing
❌ **Wrong**: Only testing happy path
✅ **Right**: Test edge cases, errors, performance, and integration

### 8. Poor Documentation
❌ **Wrong**: No XML documentation or unclear naming
✅ **Right**: Clear documentation and JavaScript-consistent naming

## Checklist for New Global Objects

- [ ] Created `{ObjectName}Globals.cs` file
- [ ] Implemented main module class
- [ ] Implemented all function classes with `Call()` method
- [ ] Implemented instance object class if needed
- [ ] Added to `EvaluateIdentifier` in `Interpreter.cs`
- [ ] Added to `EvaluateMemberExpression` in `Interpreter.cs` 
- [ ] Added to `EvaluateCallExpression` in `Interpreter.cs`
- [ ] Created comprehensive test file
- [ ] Tested all methods and error cases
- [ ] Added performance tests
- [ ] Added integration tests
- [ ] Verified JavaScript compatibility
- [ ] Added XML documentation
- [ ] Built and tested successfully

Following these patterns ensures consistency, maintainability, and JavaScript compatibility across all global objects in ECEngine.
