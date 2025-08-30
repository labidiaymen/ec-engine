using Xunit;
using ECEngine.AST;
using ECEngine.Runtime;
using System.IO;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Runtime;

public class MathObjectTests
{
    #region Constants Tests

    [Fact]
    public void Evaluate_MathPI_ReturnsCorrectValue()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "PI");

        // Act
        var result = interpreter.Evaluate(memberExpression, "Math.PI");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(Math.PI, (double)result, 15); // 15 decimal places precision
    }

    [Fact]
    public void Evaluate_MathE_ReturnsCorrectValue()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "E");

        // Act
        var result = interpreter.Evaluate(memberExpression, "Math.E");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(Math.E, (double)result, 15);
    }

    [Theory]
    [InlineData("LN2", 0.6931471805599453)]
    [InlineData("LN10", 2.302585092994046)]
    [InlineData("SQRT2", 1.4142135623730951)]
    [InlineData("SQRT1_2", 0.7071067811865476)]
    public void Evaluate_MathConstants_ReturnCorrectValues(string constantName, double expectedValue)
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, constantName);

        // Act
        var result = interpreter.Evaluate(memberExpression, $"Math.{constantName}");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(expectedValue, (double)result, 15);
    }

    #endregion

    #region Basic Math Functions Tests

    [Theory]
    [InlineData(-5, 5)]
    [InlineData(5, 5)]
    [InlineData(0, 0)]
    [InlineData(-3.14, 3.14)]
    public void Evaluate_MathAbs_ReturnsAbsoluteValue(double input, double expected)
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "abs");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(input) });

        // Act
        var result = interpreter.Evaluate(callExpression, $"Math.abs({input})");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(expected, (double)result, 10);
    }

    [Theory]
    [InlineData(4.2, 5)]
    [InlineData(4.8, 5)]
    [InlineData(-4.2, -4)]
    [InlineData(5, 5)]
    public void Evaluate_MathCeil_ReturnsCeilingValue(double input, double expected)
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "ceil");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(input) });

        // Act
        var result = interpreter.Evaluate(callExpression, $"Math.ceil({input})");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(expected, (double)result);
    }

    [Theory]
    [InlineData(4.2, 4)]
    [InlineData(4.8, 4)]
    [InlineData(-4.2, -5)]
    [InlineData(5, 5)]
    public void Evaluate_MathFloor_ReturnsFloorValue(double input, double expected)
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "floor");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(input) });

        // Act
        var result = interpreter.Evaluate(callExpression, $"Math.floor({input})");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(expected, (double)result);
    }

    [Theory]
    [InlineData(4.2, 4)]
    [InlineData(4.5, 4)]
    [InlineData(4.6, 5)]
    [InlineData(-4.5, -4)]
    public void Evaluate_MathRound_ReturnsRoundedValue(double input, double expected)
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "round");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(input) });

        // Act
        var result = interpreter.Evaluate(callExpression, $"Math.round({input})");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(expected, (double)result);
    }

    [Theory]
    [InlineData(4.9, 4)]
    [InlineData(-4.9, -4)]
    [InlineData(5, 5)]
    public void Evaluate_MathTrunc_ReturnsTruncatedValue(double input, double expected)
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "trunc");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(input) });

        // Act
        var result = interpreter.Evaluate(callExpression, $"Math.trunc({input})");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(expected, (double)result);
    }

    #endregion

    #region Trigonometric Functions Tests

    [Fact]
    public void Evaluate_MathSin_ReturnsCorrectValue()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "sin");
        var piDiv2 = Math.PI / 2;
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(piDiv2) });

        // Act
        var result = interpreter.Evaluate(callExpression, $"Math.sin({piDiv2})");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(1.0, (double)result, 10);
    }

    [Fact]
    public void Evaluate_MathCos_ReturnsCorrectValue()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "cos");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(0) });

        // Act
        var result = interpreter.Evaluate(callExpression, "Math.cos(0)");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(1.0, (double)result, 10);
    }

    [Fact]
    public void Evaluate_MathTan_ReturnsCorrectValue()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "tan");
        var piDiv4 = Math.PI / 4;
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(piDiv4) });

        // Act
        var result = interpreter.Evaluate(callExpression, $"Math.tan({piDiv4})");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(Math.Tan(piDiv4), (double)result, 10);
    }

    #endregion

    #region Power and Root Functions Tests

    [Theory]
    [InlineData(16, 4)]
    [InlineData(25, 5)]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    public void Evaluate_MathSqrt_ReturnsSquareRoot(double input, double expected)
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "sqrt");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(input) });

        // Act
        var result = interpreter.Evaluate(callExpression, $"Math.sqrt({input})");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(expected, (double)result, 10);
    }

    [Theory]
    [InlineData(2, 3, 8)]
    [InlineData(5, 2, 25)]
    [InlineData(10, 0, 1)]
    [InlineData(2, -1, 0.5)]
    public void Evaluate_MathPow_ReturnsPowerResult(double baseValue, double exponent, double expected)
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "pow");
        var args = new List<Expression> { new NumberLiteral(baseValue), new NumberLiteral(exponent) };
        var callExpression = new CallExpression(memberExpression, args);

        // Act
        var result = interpreter.Evaluate(callExpression, $"Math.pow({baseValue}, {exponent})");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(expected, (double)result, 10);
    }

    #endregion

    #region Logarithmic Functions Tests

    [Fact]
    public void Evaluate_MathLog_ReturnsNaturalLogarithm()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "log");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(Math.E) });

        // Act
        var result = interpreter.Evaluate(callExpression, $"Math.log({Math.E})");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(1.0, (double)result, 10);
    }

    [Fact]
    public void Evaluate_MathExp_ReturnsExponential()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "exp");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(1) });

        // Act
        var result = interpreter.Evaluate(callExpression, "Math.exp(1)");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(Math.E, (double)result, 10);
    }

    #endregion

    #region Min/Max Functions Tests

    [Fact]
    public void Evaluate_MathMin_ReturnsMinimumValue()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "min");
        var args = new List<Expression> { new NumberLiteral(1), new NumberLiteral(2), new NumberLiteral(3) };
        var callExpression = new CallExpression(memberExpression, args);

        // Act
        var result = interpreter.Evaluate(callExpression, "Math.min(1, 2, 3)");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(1.0, (double)result);
    }

    [Fact]
    public void Evaluate_MathMax_ReturnsMaximumValue()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "max");
        var args = new List<Expression> { new NumberLiteral(1), new NumberLiteral(2), new NumberLiteral(3) };
        var callExpression = new CallExpression(memberExpression, args);

        // Act
        var result = interpreter.Evaluate(callExpression, "Math.max(1, 2, 3)");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(3.0, (double)result);
    }

    [Fact]
    public void Evaluate_MathMinWithNoArgs_ReturnsPositiveInfinity()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "min");
        var callExpression = new CallExpression(memberExpression, new List<Expression>());

        // Act
        var result = interpreter.Evaluate(callExpression, "Math.min()");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(double.PositiveInfinity, (double)result);
    }

    [Fact]
    public void Evaluate_MathMaxWithNoArgs_ReturnsNegativeInfinity()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "max");
        var callExpression = new CallExpression(memberExpression, new List<Expression>());

        // Act
        var result = interpreter.Evaluate(callExpression, "Math.max()");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(double.NegativeInfinity, (double)result);
    }

    #endregion

    #region Random Function Tests

    [Fact]
    public void Evaluate_MathRandom_ReturnsValueBetweenZeroAndOne()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "random");
        var callExpression = new CallExpression(memberExpression, new List<Expression>());

        // Act
        var result = interpreter.Evaluate(callExpression, "Math.random()");

        // Assert
        Assert.IsType<double>(result);
        var randomValue = (double)result;
        Assert.True(randomValue >= 0.0);
        Assert.True(randomValue < 1.0);
    }

    [Fact]
    public void Evaluate_MathRandom_MultipleCallsReturnDifferentValues()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "random");
        var callExpression = new CallExpression(memberExpression, new List<Expression>());

        // Act
        var result1 = interpreter.Evaluate(callExpression, "Math.random()");
        var result2 = interpreter.Evaluate(callExpression, "Math.random()");
        var result3 = interpreter.Evaluate(callExpression, "Math.random()");

        // Assert
        Assert.IsType<double>(result1);
        Assert.IsType<double>(result2);
        Assert.IsType<double>(result3);
        
        // Very unlikely that three consecutive random calls return the same value
        Assert.False((double)result1 == (double)result2 && (double)result2 == (double)result3);
    }

    #endregion

    #region Inverse Trigonometric Functions Tests

    [Fact]
    public void Evaluate_MathAsin_ReturnsArcsine()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "asin");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(0.5) });

        // Act
        var result = interpreter.Evaluate(callExpression, "Math.asin(0.5)");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(Math.Asin(0.5), (double)result, 10);
    }

    [Fact]
    public void Evaluate_MathAcos_ReturnsArccosine()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "acos");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(0.5) });

        // Act
        var result = interpreter.Evaluate(callExpression, "Math.acos(0.5)");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(Math.Acos(0.5), (double)result, 10);
    }

    [Fact]
    public void Evaluate_MathAtan_ReturnsArctangent()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "atan");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(1) });

        // Act
        var result = interpreter.Evaluate(callExpression, "Math.atan(1)");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(Math.PI / 4, (double)result, 10);
    }

    [Fact]
    public void Evaluate_MathAtan2_ReturnsArctangent2()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "atan2");
        var args = new List<Expression> { new NumberLiteral(1), new NumberLiteral(1) };
        var callExpression = new CallExpression(memberExpression, args);

        // Act
        var result = interpreter.Evaluate(callExpression, "Math.atan2(1, 1)");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(Math.PI / 4, (double)result, 10);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Evaluate_MathFunctionWithNoArgs_ReturnsNaN()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "abs");
        var callExpression = new CallExpression(memberExpression, new List<Expression>());

        // Act
        var result = interpreter.Evaluate(callExpression, "Math.abs()");

        // Assert
        Assert.IsType<double>(result);
        Assert.True(double.IsNaN((double)result));
    }

    [Fact]
    public void Evaluate_MathSqrtWithNegative_ReturnsNaN()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var mathIdentifier = new Identifier("Math");
        var memberExpression = new MemberExpression(mathIdentifier, "sqrt");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(-1) });

        // Act
        var result = interpreter.Evaluate(callExpression, "Math.sqrt(-1)");

        // Assert
        Assert.IsType<double>(result);
        Assert.True(double.IsNaN((double)result));
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Performance_ManyMathOperations_CompletesInReasonableTime()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        const int iterations = 1000;

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var mathIdentifier = new Identifier("Math");
            var memberExpression = new MemberExpression(mathIdentifier, "abs");
            var callExpression = new CallExpression(memberExpression, new List<Expression> { new NumberLiteral(-i) });
            interpreter.Evaluate(callExpression, $"Math.abs(-{i})");
        }
        
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
            $"Performing {iterations} Math.abs() operations took {stopwatch.ElapsedMilliseconds}ms, expected less than 2000ms");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_ComplexMathExpression_WorksCorrectly()
    {
        // Test simpler expression: Math.sqrt(Math.pow(2, 2)) should equal 2
        
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // Math.pow(2, 2)
        var powExpr = new CallExpression(
            new MemberExpression(new Identifier("Math"), "pow"),
            new List<Expression> { new NumberLiteral(2), new NumberLiteral(2) }
        );
        
        // Math.sqrt(Math.pow(2, 2))
        var sqrtExpr = new CallExpression(
            new MemberExpression(new Identifier("Math"), "sqrt"),
            new List<Expression> { powExpr }
        );

        // Act
        var result = interpreter.Evaluate(sqrtExpr, "Math.sqrt(Math.pow(2, 2))");

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(2.0, (double)result, 10);
    }

    #endregion
}
