using Xunit;
using ECEngine.AST;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Interpreter;

[Collection("ConsoleTests")]
public class AdvancedOperatorEvaluationTests
{
    #region Strict Comparison Operators

    [Fact]
    public void Evaluate_StrictEqualWithSameTypeAndValue_ReturnsTrue()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(5);
        var right = new NumberLiteral(5);
        var expression = new BinaryExpression(left, "===", right);

        // Act
        var result = interpreter.Evaluate(expression, "5 === 5");

        // Assert
        Assert.True((bool)result);
    }

    [Fact]
    public void Evaluate_StrictEqualWithSameValueDifferentType_ReturnsFalse()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(5);
        var right = new StringLiteral("5");
        var expression = new BinaryExpression(left, "===", right);

        // Act
        var result = interpreter.Evaluate(expression, "5 === \"5\"");

        // Assert
        Assert.False((bool)result);
    }

    [Fact]
    public void Evaluate_StrictEqualWithNull_ReturnsCorrectResult()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NullLiteral();
        var right = new NullLiteral();
        var expression = new BinaryExpression(left, "===", right);

        // Act
        var result = interpreter.Evaluate(expression, "null === null");

        // Assert
        Assert.True((bool)result);
    }

    [Fact]
    public void Evaluate_StrictNotEqualWithDifferentTypes_ReturnsTrue()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(5);
        var right = new StringLiteral("5");
        var expression = new BinaryExpression(left, "!==", right);

        // Act
        var result = interpreter.Evaluate(expression, "5 !== \"5\"");

        // Assert
        Assert.True((bool)result);
    }

    [Fact]
    public void Evaluate_StrictNotEqualWithSameTypeAndValue_ReturnsFalse()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(5);
        var right = new NumberLiteral(5);
        var expression = new BinaryExpression(left, "!==", right);

        // Act
        var result = interpreter.Evaluate(expression, "5 !== 5");

        // Assert
        Assert.False((bool)result);
    }

    #endregion

    #region Compound Assignment Operators

    [Fact]
    public void Evaluate_PlusAssignWithNumbers_UpdatesVariableCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // First declare the variable
        var declaration = new VariableDeclaration("var", "x", new NumberLiteral(10));
        interpreter.Evaluate(declaration, "var x = 10");

        // Then test compound assignment
        var identifier = new Identifier("x");
        var value = new NumberLiteral(5);
        var expression = new CompoundAssignmentExpression(identifier, "+=", value);

        // Act
        var result = interpreter.Evaluate(expression, "x += 5");

        // Assert
        Assert.Equal(15.0, result);
        
        // Verify variable was updated
        var variableValue = interpreter.Evaluate(new Identifier("x"), "x");
        Assert.Equal(15.0, variableValue);
    }

    [Fact]
    public void Evaluate_PlusAssignWithStrings_ConcatenatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // First declare the variable
        var declaration = new VariableDeclaration("var", "str", new StringLiteral("Hello"));
        interpreter.Evaluate(declaration, "var str = \"Hello\"");

        // Then test compound assignment
        var identifier = new Identifier("str");
        var value = new StringLiteral(" World");
        var expression = new CompoundAssignmentExpression(identifier, "+=", value);

        // Act
        var result = interpreter.Evaluate(expression, "str += \" World\"");

        // Assert
        Assert.Equal("Hello World", result);
        
        // Verify variable was updated
        var variableValue = interpreter.Evaluate(new Identifier("str"), "str");
        Assert.Equal("Hello World", variableValue);
    }

    [Fact]
    public void Evaluate_MinusAssignWithNumbers_UpdatesVariableCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // First declare the variable
        var declaration = new VariableDeclaration("var", "x", new NumberLiteral(10));
        interpreter.Evaluate(declaration, "var x = 10");

        // Then test compound assignment
        var identifier = new Identifier("x");
        var value = new NumberLiteral(3);
        var expression = new CompoundAssignmentExpression(identifier, "-=", value);

        // Act
        var result = interpreter.Evaluate(expression, "x -= 3");

        // Assert
        Assert.Equal(7.0, result);
        
        // Verify variable was updated
        var variableValue = interpreter.Evaluate(new Identifier("x"), "x");
        Assert.Equal(7.0, variableValue);
    }

    [Fact]
    public void Evaluate_MultiplyAssignWithNumbers_UpdatesVariableCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // First declare the variable
        var declaration = new VariableDeclaration("var", "x", new NumberLiteral(10));
        interpreter.Evaluate(declaration, "var x = 10");

        // Then test compound assignment
        var identifier = new Identifier("x");
        var value = new NumberLiteral(2);
        var expression = new CompoundAssignmentExpression(identifier, "*=", value);

        // Act
        var result = interpreter.Evaluate(expression, "x *= 2");

        // Assert
        Assert.Equal(20.0, result);
        
        // Verify variable was updated
        var variableValue = interpreter.Evaluate(new Identifier("x"), "x");
        Assert.Equal(20.0, variableValue);
    }

    [Fact]
    public void Evaluate_DivideAssignWithNumbers_UpdatesVariableCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // First declare the variable
        var declaration = new VariableDeclaration("var", "x", new NumberLiteral(20));
        interpreter.Evaluate(declaration, "var x = 20");

        // Then test compound assignment
        var identifier = new Identifier("x");
        var value = new NumberLiteral(4);
        var expression = new CompoundAssignmentExpression(identifier, "/=", value);

        // Act
        var result = interpreter.Evaluate(expression, "x /= 4");

        // Assert
        Assert.Equal(5.0, result);
        
        // Verify variable was updated
        var variableValue = interpreter.Evaluate(new Identifier("x"), "x");
        Assert.Equal(5.0, variableValue);
    }

    [Fact]
    public void Evaluate_CompoundAssignmentWithTernary_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // First declare the variables
        var xDeclaration = new VariableDeclaration("var", "x", new NumberLiteral(0));
        var conditionDeclaration = new VariableDeclaration("var", "condition", new BooleanLiteral(true));
        interpreter.Evaluate(xDeclaration, "var x = 0");
        interpreter.Evaluate(conditionDeclaration, "var condition = true");

        // Create ternary expression: condition ? 1 : -1
        var ternary = new ConditionalExpression(
            new Identifier("condition"),
            new NumberLiteral(1),
            new NumberLiteral(-1)
        );

        // Test compound assignment: x += condition ? 1 : -1
        var identifier = new Identifier("x");
        var expression = new CompoundAssignmentExpression(identifier, "+=", ternary);

        // Act
        var result = interpreter.Evaluate(expression, "x += condition ? 1 : -1");

        // Assert
        Assert.Equal(1.0, result);
        
        // Verify variable was updated
        var variableValue = interpreter.Evaluate(new Identifier("x"), "x");
        Assert.Equal(1.0, variableValue);
    }

    #endregion

    #region Bitwise Operators

    [Fact]
    public void Evaluate_BitwiseAndOperator_ReturnsCorrectResult()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(12);  // 1100 in binary
        var right = new NumberLiteral(10); // 1010 in binary
        var expression = new BinaryExpression(left, "&", right);

        // Act
        var result = interpreter.Evaluate(expression, "12 & 10");

        // Assert
        Assert.Equal(8.0, result); // 1000 in binary
    }

    [Fact]
    public void Evaluate_BitwiseOrOperator_ReturnsCorrectResult()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(12);  // 1100 in binary
        var right = new NumberLiteral(10); // 1010 in binary
        var expression = new BinaryExpression(left, "|", right);

        // Act
        var result = interpreter.Evaluate(expression, "12 | 10");

        // Assert
        Assert.Equal(14.0, result); // 1110 in binary
    }

    [Fact]
    public void Evaluate_BitwiseXorOperator_ReturnsCorrectResult()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(12);  // 1100 in binary
        var right = new NumberLiteral(10); // 1010 in binary
        var expression = new BinaryExpression(left, "^", right);

        // Act
        var result = interpreter.Evaluate(expression, "12 ^ 10");

        // Assert
        Assert.Equal(6.0, result); // 0110 in binary
    }

    [Fact]
    public void Evaluate_BitwiseNotOperator_ReturnsCorrectResult()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var operand = new NumberLiteral(12);
        var expression = new UnaryExpression("~", operand);

        // Act
        var result = interpreter.Evaluate(expression, "~12");

        // Assert
        Assert.Equal(-13.0, result); // Bitwise complement of 12
    }

    [Fact]
    public void Evaluate_LeftShiftOperator_ReturnsCorrectResult()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(8);
        var right = new NumberLiteral(2);
        var expression = new BinaryExpression(left, "<<", right);

        // Act
        var result = interpreter.Evaluate(expression, "8 << 2");

        // Assert
        Assert.Equal(32.0, result); // 8 * 2^2 = 32
    }

    [Fact]
    public void Evaluate_RightShiftOperator_ReturnsCorrectResult()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(8);
        var right = new NumberLiteral(1);
        var expression = new BinaryExpression(left, ">>", right);

        // Act
        var result = interpreter.Evaluate(expression, "8 >> 1");

        // Assert
        Assert.Equal(4.0, result); // 8 / 2^1 = 4
    }

    [Fact]
    public void Evaluate_UnsignedRightShiftOperator_ReturnsCorrectResult()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(8);
        var right = new NumberLiteral(1);
        var expression = new BinaryExpression(left, ">>>", right);

        // Act
        var result = interpreter.Evaluate(expression, "8 >>> 1");

        // Assert
        Assert.Equal(4.0, result); // For positive numbers, same as >>
    }

    [Fact]
    public void Evaluate_UnsignedRightShiftWithNegativeNumber_ReturnsCorrectResult()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(-1);
        var right = new NumberLiteral(1);
        var expression = new BinaryExpression(left, ">>>", right);

        // Act
        var result = interpreter.Evaluate(expression, "-1 >>> 1");

        // Assert
        // -1 >>> 1 should give a large positive number in JavaScript-like behavior
        Assert.True((double)result > 0);
    }

    #endregion

    #region Ternary Operator

    [Fact]
    public void Evaluate_TernaryWithTrueCondition_ReturnsConsequent()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var test = new BooleanLiteral(true);
        var consequent = new StringLiteral("yes");
        var alternate = new StringLiteral("no");
        var expression = new ConditionalExpression(test, consequent, alternate);

        // Act
        var result = interpreter.Evaluate(expression, "true ? \"yes\" : \"no\"");

        // Assert
        Assert.Equal("yes", result);
    }

    [Fact]
    public void Evaluate_TernaryWithFalseCondition_ReturnsAlternate()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var test = new BooleanLiteral(false);
        var consequent = new StringLiteral("yes");
        var alternate = new StringLiteral("no");
        var expression = new ConditionalExpression(test, consequent, alternate);

        // Act
        var result = interpreter.Evaluate(expression, "false ? \"yes\" : \"no\"");

        // Assert
        Assert.Equal("no", result);
    }

    [Fact]
    public void Evaluate_TernaryWithNumberCondition_EvaluatesTruthiness()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var test = new NumberLiteral(5);
        var consequent = new StringLiteral("truthy");
        var alternate = new StringLiteral("falsy");
        var expression = new ConditionalExpression(test, consequent, alternate);

        // Act
        var result = interpreter.Evaluate(expression, "5 ? \"truthy\" : \"falsy\"");

        // Assert
        Assert.Equal("truthy", result);
    }

    [Fact]
    public void Evaluate_TernaryWithZeroCondition_EvaluatesTruthiness()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var test = new NumberLiteral(0);
        var consequent = new StringLiteral("truthy");
        var alternate = new StringLiteral("falsy");
        var expression = new ConditionalExpression(test, consequent, alternate);

        // Act
        var result = interpreter.Evaluate(expression, "0 ? \"truthy\" : \"falsy\"");

        // Assert
        Assert.Equal("falsy", result);
    }

    [Fact]
    public void Evaluate_NestedTernaryExpression_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // Create x > 0 ? 1 : x < 0 ? -1 : 0
        var xDeclaration = new VariableDeclaration("var", "x", new NumberLiteral(5));
        interpreter.Evaluate(xDeclaration, "var x = 5");

        var x = new Identifier("x");
        var zero = new NumberLiteral(0);
        var one = new NumberLiteral(1);
        var negativeOne = new NumberLiteral(-1);

        var xGreaterThanZero = new BinaryExpression(x, ">", zero);
        var xLessThanZero = new BinaryExpression(x, "<", zero);
        
        var innerTernary = new ConditionalExpression(xLessThanZero, negativeOne, zero);
        var outerTernary = new ConditionalExpression(xGreaterThanZero, one, innerTernary);

        // Act
        var result = interpreter.Evaluate(outerTernary, "x > 0 ? 1 : x < 0 ? -1 : 0");

        // Assert
        Assert.Equal(1.0, result);
    }

    #endregion

    #region Complex Expression Tests

    [Fact]
    public void Evaluate_ComplexExpressionWithMultipleAdvancedOperators_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // Test: (5 === 5) ? (10 | 5) + (3 << 1) : ~0
        var strictEqual = new BinaryExpression(new NumberLiteral(5), "===", new NumberLiteral(5));
        var bitwiseOr = new BinaryExpression(new NumberLiteral(10), "|", new NumberLiteral(5));
        var leftShift = new BinaryExpression(new NumberLiteral(3), "<<", new NumberLiteral(1));
        var addition = new BinaryExpression(bitwiseOr, "+", leftShift);
        var bitwiseNot = new UnaryExpression("~", new NumberLiteral(0));
        
        var ternary = new ConditionalExpression(strictEqual, addition, bitwiseNot);

        // Act
        var result = interpreter.Evaluate(ternary, "(5 === 5) ? (10 | 5) + (3 << 1) : ~0");

        // Assert
        // (5 === 5) is true, so we evaluate (10 | 5) + (3 << 1)
        // 10 | 5 = 15, 3 << 1 = 6, 15 + 6 = 21
        Assert.Equal(21.0, result);
    }

    [Fact]
    public void Evaluate_BitwiseOperatorPrecedence_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // Test: 4 | 2 & 3 (should be 4 | (2 & 3) = 4 | 2 = 6)
        var four = new NumberLiteral(4);
        var two = new NumberLiteral(2);
        var three = new NumberLiteral(3);
        
        var bitwiseAnd = new BinaryExpression(two, "&", three);
        var bitwiseOr = new BinaryExpression(four, "|", bitwiseAnd);

        // Act
        var result = interpreter.Evaluate(bitwiseOr, "4 | 2 & 3");

        // Assert
        Assert.Equal(6.0, result); // 4 | (2 & 3) = 4 | 2 = 6
    }

    [Theory]
    [InlineData(5, 5, true)]   // Same value and type
    [InlineData(5, "5", false)] // Same value, different type
    [InlineData(true, true, true)] // Same boolean values
    [InlineData(null, null, true)] // Both null
    [InlineData(0, false, false)] // Different types (number vs boolean)
    public void Evaluate_StrictComparisonVariations_ReturnsExpectedResults(object leftValue, object rightValue, bool expected)
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        Expression leftExpr = leftValue switch
        {
            int i => new NumberLiteral(i),
            string s => new StringLiteral(s),
            bool b => new BooleanLiteral(b),
            null => new NullLiteral(),
            _ => throw new ArgumentException("Unsupported type")
        };
        
        Expression rightExpr = rightValue switch
        {
            int i => new NumberLiteral(i),
            string s => new StringLiteral(s),
            bool b => new BooleanLiteral(b),
            null => new NullLiteral(),
            _ => throw new ArgumentException("Unsupported type")
        };
        
        var expression = new BinaryExpression(leftExpr, "===", rightExpr);

        // Act
        var result = interpreter.Evaluate(expression, $"{leftValue} === {rightValue}");

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion
}
