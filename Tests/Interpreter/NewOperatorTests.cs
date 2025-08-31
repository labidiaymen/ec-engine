using Xunit;
using ECEngine.AST;
using ECEngine.Runtime;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;
using System.Collections.Generic;

namespace ECEngine.Tests.Interpreter;

[Collection("ConsoleTests")]
public class NewOperatorTests
{
    #region Basic Constructor Tests

    [Fact]
    public void Evaluate_NewObject_CreatesEmptyObject()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("Object");
        var newExpression = new NewExpression(constructor, new List<Expression>());

        // Act
        var result = interpreter.Evaluate(newExpression, "new Object()");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Dictionary<string, object?>>(result);
        var obj = (Dictionary<string, object?>)result;
        Assert.Empty(obj);
    }

    [Fact]
    public void Evaluate_NewArray_CreatesEmptyArray()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("Array");
        var newExpression = new NewExpression(constructor, new List<Expression>());

        // Act
        var result = interpreter.Evaluate(newExpression, "new Array()");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<System.Collections.IList>(result);
    }

    [Fact]
    public void Evaluate_NewArrayWithSize_CreatesArrayWithLength()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("Array");
        var sizeArg = new NumberLiteral(5);
        var newExpression = new NewExpression(constructor, new List<Expression> { sizeArg });

        // Act
        var result = interpreter.Evaluate(newExpression, "new Array(5)");

        // Assert
        Assert.NotNull(result);
        var array = result as List<object?>;
        Assert.NotNull(array);
        Assert.Equal(5, array.Count);
        // All elements should be undefined/null
        Assert.All(array, item => Assert.Null(item));
    }

    [Fact]
    public void Evaluate_NewString_CreatesStringObject()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("String");
        var valueArg = new StringLiteral("hello");
        var newExpression = new NewExpression(constructor, new List<Expression> { valueArg });

        // Act
        var result = interpreter.Evaluate(newExpression, "new String('hello')");

        // Assert
        Assert.NotNull(result);
        // In JavaScript, new String() returns a String object, not a primitive
        // The exact implementation depends on how the engine handles wrapper objects
        Assert.IsType<string>(result);
        Assert.Equal("hello", result);
    }

    [Fact]
    public void Evaluate_NewNumber_CreatesNumberObject()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("Number");
        var valueArg = new NumberLiteral(42);
        var newExpression = new NewExpression(constructor, new List<Expression> { valueArg });

        // Act
        var result = interpreter.Evaluate(newExpression, "new Number(42)");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<double>(result);
        Assert.Equal(42.0, (double)result);
    }

    [Fact]
    public void Evaluate_NewBoolean_CreatesBooleanObject()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("Boolean");
        var valueArg = new BooleanLiteral(true);
        var newExpression = new NewExpression(constructor, new List<Expression> { valueArg });

        // Act
        var result = interpreter.Evaluate(newExpression, "new Boolean(true)");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<bool>(result);
        Assert.True((bool)result);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Evaluate_NewWithUndefinedConstructor_ThrowsException()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("UndefinedConstructor");
        var newExpression = new NewExpression(constructor, new List<Expression>());

        // Act & Assert
        var exception = Assert.Throws<ECEngineException>(() =>
            interpreter.Evaluate(newExpression, "new UndefinedConstructor()"));
        
        Assert.Contains("Unknown identifier", exception.Message);
    }

    [Fact]
    public void Evaluate_NewWithNonConstructor_ThrowsException()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        // First declare a regular variable
        interpreter.Evaluate(new VariableDeclaration("var", "notAConstructor", new StringLiteral("test")), "var notAConstructor = 'test';");
        
        var constructor = new Identifier("notAConstructor");
        var newExpression = new NewExpression(constructor, new List<Expression>());

        // Act & Assert
        var exception = Assert.Throws<ECEngineException>(() =>
            interpreter.Evaluate(newExpression, "new notAConstructor()"));
        
        Assert.Contains("Constructor not found", exception.Message);
    }

    #endregion

    #region Array Constructor Special Cases

    [Fact]
    public void Evaluate_NewArrayWithMultipleArgs_CreatesArrayWithValues()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("Array");
        var args = new List<Expression>
        {
            new NumberLiteral(1),
            new NumberLiteral(2),
            new NumberLiteral(3)
        };
        var newExpression = new NewExpression(constructor, args);

        // Act
        var result = interpreter.Evaluate(newExpression, "new Array(1, 2, 3)");

        // Assert
        Assert.NotNull(result);
        var array = result as List<object?>;
        Assert.NotNull(array);
        Assert.Equal(3, array.Count);
        Assert.Equal(1.0, array[0]);
        Assert.Equal(2.0, array[1]);
        Assert.Equal(3.0, array[2]);
    }

    [Fact]
    public void Evaluate_NewArrayWithZeroLength_CreatesEmptyArray()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("Array");
        var sizeArg = new NumberLiteral(0);
        var newExpression = new NewExpression(constructor, new List<Expression> { sizeArg });

        // Act
        var result = interpreter.Evaluate(newExpression, "new Array(0)");

        // Assert
        Assert.NotNull(result);
        var array = result as List<object?>;
        Assert.NotNull(array);
        Assert.Empty(array);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Evaluate_NewOperatorInAssignment_WorksCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("Object");
        var newExpression = new NewExpression(constructor, new List<Expression>());
        var assignment = new VariableDeclaration("var", "myObject", newExpression);

        // Act
        interpreter.Evaluate(assignment, "var myObject = new Object();");
        var result = interpreter.Evaluate(new Identifier("myObject"), "myObject");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Dictionary<string, object?>>(result);
    }

    [Fact]
    public void Evaluate_NewOperatorInFunctionCall_WorksCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // Create a function that takes an object parameter
        var funcBody = new List<Statement>
        {
            new ReturnStatement(new Identifier("param"))
        };
        var func = new FunctionDeclaration("testFunc", new List<string> { "param" }, funcBody);
        interpreter.Evaluate(func, "function testFunc(param) { return param; }");

        // Create new expression as function argument
        var constructor = new Identifier("Array");
        var newExpression = new NewExpression(constructor, new List<Expression>());
        var functionCall = new CallExpression(new Identifier("testFunc"), new List<Expression> { newExpression });

        // Act
        var result = interpreter.Evaluate(functionCall, "testFunc(new Array())");

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<System.Collections.IList>(result);
    }

    #endregion
}
