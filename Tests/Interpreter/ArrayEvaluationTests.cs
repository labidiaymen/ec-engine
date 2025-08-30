using System.Collections.Generic;
using Xunit;
using ECEngine.AST;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Interpreter;

/// <summary>
/// Tests for array literal evaluation and array methods
/// </summary>
[Collection("ConsoleTests")]
public class ArrayEvaluationTests
{
    [Fact]
    public void Evaluate_EmptyArrayLiteral_ReturnsEmptyArray()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var arrayLiteral = new ArrayLiteral(new List<Expression>());
        
        // Act
        var result = interpreter.Evaluate(arrayLiteral, "[]");
        
        // Assert
        var array = Assert.IsType<List<object>>(result);
        Assert.Empty(array);
    }

    [Fact]
    public void Evaluate_ArrayLiteralWithElements_ReturnsArrayWithValues()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var elements = new List<Expression>
        {
            new NumberLiteral(1),
            new StringLiteral("hello"),
            new NumberLiteral(3)
        };
        var arrayLiteral = new ArrayLiteral(elements);
        
        // Act
        var result = interpreter.Evaluate(arrayLiteral, "[1, \"hello\", 3]");
        
        // Assert
        var array = Assert.IsType<List<object>>(result);
        Assert.Equal(3, array.Count);
        Assert.Equal(1.0, array[0]);
        Assert.Equal("hello", array[1]);
        Assert.Equal(3.0, array[2]);
    }

    [Fact]
    public void Evaluate_ArrayIndexAccess_ReturnsCorrectElement()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var arrayDeclaration = new VariableDeclaration("var", "arr",
            new ArrayLiteral(new List<Expression>
            {
                new StringLiteral("first"),
                new StringLiteral("second"),
                new StringLiteral("third")
            }));
            
        var memberExpression = new MemberExpression(
            new Identifier("arr"),
            new NumberLiteral(1)); // For bracket notation: arr[1]
        
        // Act
        interpreter.Evaluate(arrayDeclaration, "var arr = [\"first\", \"second\", \"third\"]");
        var result = interpreter.Evaluate(memberExpression, "arr[1]");
        
        // Assert
        Assert.Equal("second", result);
    }

    [Fact]
    public void Evaluate_ArrayIndexOutOfBounds_ReturnsUndefined()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var arrayDeclaration = new VariableDeclaration("var", "arr",
            new ArrayLiteral(new List<Expression>
            {
                new NumberLiteral(1),
                new NumberLiteral(2)
            }));
            
        var memberExpression = new MemberExpression(
            new Identifier("arr"),
            new NumberLiteral(5)); // For bracket notation: arr[5]
        
        // Act
        interpreter.Evaluate(arrayDeclaration, "var arr = [1, 2]");
        var result = interpreter.Evaluate(memberExpression, "arr[5]");
        
        // Assert
        Assert.Null(result); // undefined is represented as null in this implementation
    }

    [Fact]
    public void Evaluate_ArrayLengthProperty_ReturnsCorrectLength()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var arrayDeclaration = new VariableDeclaration("var", "arr",
            new ArrayLiteral(new List<Expression>
            {
                new NumberLiteral(1),
                new NumberLiteral(2),
                new NumberLiteral(3)
            }));
            
        var memberExpression = new MemberExpression(
            new Identifier("arr"),
            "length"); // For dot notation: arr.length
        
        // Act
        interpreter.Evaluate(arrayDeclaration, "var arr = [1, 2, 3]");
        var result = interpreter.Evaluate(memberExpression, "arr.length");
        
        // Assert
        Assert.Equal(3.0, result);
    }

    [Fact]
    public void Evaluate_ArrayPushMethod_AddsElementAndReturnsNewLength()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var arrayDeclaration = new VariableDeclaration("var", "arr",
            new ArrayLiteral(new List<Expression>
            {
                new NumberLiteral(1),
                new NumberLiteral(2)
            }));
            
        var pushCall = new CallExpression(
            new MemberExpression(
                new Identifier("arr"),
                "push"),
            new List<Expression> { new NumberLiteral(3) });
        
        // Act
        interpreter.Evaluate(arrayDeclaration, "var arr = [1, 2]");
        var result = interpreter.Evaluate(pushCall, "arr.push(3)");
        
        // Assert
        Assert.Equal(3.0, result); // push returns new length
        
        // Verify the array was modified
        var arrayAccess = new Identifier("arr");
        var arrayResult = interpreter.Evaluate(arrayAccess, "arr");
        var array = Assert.IsType<List<object>>(arrayResult);
        Assert.Equal(3, array.Count);
        Assert.Equal(3.0, array[2]);
    }

    [Fact]
    public void Evaluate_ArrayPopMethod_RemovesAndReturnsLastElement()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var arrayDeclaration = new VariableDeclaration("var", "arr",
            new ArrayLiteral(new List<Expression>
            {
                new NumberLiteral(1),
                new NumberLiteral(2),
                new NumberLiteral(3)
            }));
            
        var popCall = new CallExpression(
            new MemberExpression(
                new Identifier("arr"),
                "pop"),
            new List<Expression>());
        
        // Act
        interpreter.Evaluate(arrayDeclaration, "var arr = [1, 2, 3]");
        var result = interpreter.Evaluate(popCall, "arr.pop()");
        
        // Assert
        Assert.Equal(3.0, result); // pop returns removed element
        
        // Verify the array was modified
        var arrayAccess = new Identifier("arr");
        var arrayResult = interpreter.Evaluate(arrayAccess, "arr");
        var array = Assert.IsType<List<object>>(arrayResult);
        Assert.Equal(2, array.Count);
    }

    [Fact]
    public void Evaluate_ArraySliceMethod_ReturnsSubArray()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var arrayDeclaration = new VariableDeclaration("var", "arr",
            new ArrayLiteral(new List<Expression>
            {
                new NumberLiteral(1),
                new NumberLiteral(2),
                new NumberLiteral(3),
                new NumberLiteral(4)
            }));
            
        var sliceCall = new CallExpression(
            new MemberExpression(
                new Identifier("arr"),
                "slice"),
            new List<Expression> 
            { 
                new NumberLiteral(1), 
                new NumberLiteral(3) 
            });
        
        // Act
        interpreter.Evaluate(arrayDeclaration, "var arr = [1, 2, 3, 4]");
        var result = interpreter.Evaluate(sliceCall, "arr.slice(1, 3)");
        
        // Assert
        var slicedArray = Assert.IsType<List<object>>(result);
        Assert.Equal(2, slicedArray.Count);
        Assert.Equal(2.0, slicedArray[0]);
        Assert.Equal(3.0, slicedArray[1]);
    }

    [Fact]
    public void Evaluate_ArrayJoinMethod_ReturnsJoinedString()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var arrayDeclaration = new VariableDeclaration("var", "arr",
            new ArrayLiteral(new List<Expression>
            {
                new StringLiteral("hello"),
                new StringLiteral("world"),
                new StringLiteral("test")
            }));
            
        var joinCall = new CallExpression(
            new MemberExpression(
                new Identifier("arr"),
                "join"),
            new List<Expression> { new StringLiteral(", ") });
        
        // Act
        interpreter.Evaluate(arrayDeclaration, "var arr = [\"hello\", \"world\", \"test\"]");
        var result = interpreter.Evaluate(joinCall, "arr.join(\", \")");
        
        // Assert
        Assert.Equal("hello, world, test", result);
    }

    [Fact]
    public void Evaluate_ArrayIndexOfMethod_ReturnsCorrectIndex()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var arrayDeclaration = new VariableDeclaration("var", "arr",
            new ArrayLiteral(new List<Expression>
            {
                new StringLiteral("apple"),
                new StringLiteral("banana"),
                new StringLiteral("cherry")
            }));
            
        var indexOfCall = new CallExpression(
            new MemberExpression(
                new Identifier("arr"),
                "indexOf"),
            new List<Expression> { new StringLiteral("banana") });
        
        // Act
        interpreter.Evaluate(arrayDeclaration, "var arr = [\"apple\", \"banana\", \"cherry\"]");
        var result = interpreter.Evaluate(indexOfCall, "arr.indexOf(\"banana\")");
        
        // Assert
        Assert.Equal(1.0, result);
    }

    [Fact]
    public void Evaluate_ArrayIndexOfMethodNotFound_ReturnsMinusOne()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var arrayDeclaration = new VariableDeclaration("var", "arr",
            new ArrayLiteral(new List<Expression>
            {
                new StringLiteral("apple"),
                new StringLiteral("banana")
            }));
            
        var indexOfCall = new CallExpression(
            new MemberExpression(
                new Identifier("arr"),
                "indexOf"),
            new List<Expression> { new StringLiteral("orange") });
        
        // Act
        interpreter.Evaluate(arrayDeclaration, "var arr = [\"apple\", \"banana\"]");
        var result = interpreter.Evaluate(indexOfCall, "arr.indexOf(\"orange\")");
        
        // Assert
        Assert.Equal(-1.0, result);
    }
}
