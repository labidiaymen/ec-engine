using Xunit;
using ECEngine.AST;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Interpreter;

public class ArithmeticEvaluationTests
{
    [Fact]
    public void Evaluate_SimpleAddition_ReturnsSum()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(1);
        var right = new NumberLiteral(2);
        var binaryExpression = new BinaryExpression(left, "+", right);

        // Act
        var result = interpreter.Evaluate(binaryExpression, "1 + 2");

        // Assert
        Assert.Equal(3.0, result);
    }

    [Fact]
    public void Evaluate_Subtraction_ReturnsDifference()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(5);
        var right = new NumberLiteral(3);
        var binaryExpression = new BinaryExpression(left, "-", right);

        // Act
        var result = interpreter.Evaluate(binaryExpression, "5 - 3");

        // Assert
        Assert.Equal(2.0, result);
    }

    [Fact]
    public void Evaluate_Multiplication_ReturnsProduct()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(4);
        var right = new NumberLiteral(3);
        var binaryExpression = new BinaryExpression(left, "*", right);

        // Act
        var result = interpreter.Evaluate(binaryExpression, "4 * 3");

        // Assert
        Assert.Equal(12.0, result);
    }

    [Fact]
    public void Evaluate_Division_ReturnsQuotient()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(8);
        var right = new NumberLiteral(2);
        var binaryExpression = new BinaryExpression(left, "/", right);

        // Act
        var result = interpreter.Evaluate(binaryExpression, "8 / 2");

        // Assert
        Assert.Equal(4.0, result);
    }

    [Fact]
    public void Evaluate_DecimalArithmetic_ReturnsCorrectResult()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(3.5);
        var right = new NumberLiteral(2.5);
        var binaryExpression = new BinaryExpression(left, "+", right);

        // Act
        var result = interpreter.Evaluate(binaryExpression, "3.5 + 2.5");

        // Assert
        Assert.Equal(6.0, result);
    }
}
