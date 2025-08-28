using Xunit;
using ECEngine.AST;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Interpreter;

public class LiteralEvaluationTests
{
    [Fact]
    public void Evaluate_NumberLiteral_ReturnsNumber()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var numberLiteral = new NumberLiteral(42);
        
        // Act
        var result = interpreter.Evaluate(numberLiteral, "42");
        
        // Assert
        Assert.Equal(42.0, result);
    }

    [Fact]
    public void Evaluate_DecimalNumberLiteral_ReturnsDecimal()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var numberLiteral = new NumberLiteral(3.14);
        
        // Act
        var result = interpreter.Evaluate(numberLiteral, "3.14");
        
        // Assert
        Assert.Equal(3.14, result);
    }

    [Fact]
    public void Evaluate_ZeroLiteral_ReturnsZero()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var numberLiteral = new NumberLiteral(0);
        
        // Act
        var result = interpreter.Evaluate(numberLiteral, "0");
        
        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Evaluate_NegativeNumberLiteral_ReturnsNegativeNumber()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var numberLiteral = new NumberLiteral(-5);
        
        // Act
        var result = interpreter.Evaluate(numberLiteral, "-5");
        
        // Assert
        Assert.Equal(-5.0, result);
    }
}
