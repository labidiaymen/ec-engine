using Xunit;
using ECEngine.AST;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;
using ParserClass = ECEngine.Parser.Parser;

namespace ECEngine.Tests.Interpreter;

/// <summary>
/// Tests to verify operator precedence is handled correctly in both parsing and evaluation
/// </summary>
public class OperatorPrecedenceTests
{
    private readonly ParserClass _parser;
    private readonly RuntimeInterpreter _interpreter;

    public OperatorPrecedenceTests()
    {
        _parser = new ParserClass();
        _interpreter = new RuntimeInterpreter();
    }

    #region Basic Arithmetic Precedence Tests

    [Fact]
    public void Evaluate_MultiplicationBeforeAddition_ReturnsCorrectResult()
    {
        // Arrange: 2 + 3 * 4 should be 2 + (3 * 4) = 2 + 12 = 14
        var code = "2 + 3 * 4";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(14.0, result);
    }

    [Fact]
    public void Evaluate_MultiplicationBeforeSubtraction_ReturnsCorrectResult()
    {
        // Arrange: 10 - 2 * 3 should be 10 - (2 * 3) = 10 - 6 = 4
        var code = "10 - 2 * 3";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(4.0, result);
    }

    [Fact]
    public void Evaluate_DivisionBeforeAddition_ReturnsCorrectResult()
    {
        // Arrange: 1 + 8 / 2 should be 1 + (8 / 2) = 1 + 4 = 5
        var code = "1 + 8 / 2";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(5.0, result);
    }

    [Fact]
    public void Evaluate_DivisionBeforeSubtraction_ReturnsCorrectResult()
    {
        // Arrange: 10 - 6 / 3 should be 10 - (6 / 3) = 10 - 2 = 8
        var code = "10 - 6 / 3";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(8.0, result);
    }

    #endregion

    #region Complex Expression Precedence Tests

    [Fact]
    public void Evaluate_MultipleOperators_ReturnsCorrectResult()
    {
        // Arrange: 2 + 3 * 4 - 1 should be 2 + (3 * 4) - 1 = 2 + 12 - 1 = 13
        var code = "2 + 3 * 4 - 1";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(13.0, result);
    }

    [Fact]
    public void Evaluate_MixedMultiplicationDivision_ReturnsCorrectResult()
    {
        // Arrange: 2 * 6 / 3 should be (2 * 6) / 3 = 12 / 3 = 4 (left-to-right)
        var code = "2 * 6 / 3";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(4.0, result);
    }

    [Fact]
    public void Evaluate_MixedAdditionSubtraction_ReturnsCorrectResult()
    {
        // Arrange: 10 - 3 + 2 should be (10 - 3) + 2 = 7 + 2 = 9 (left-to-right)
        var code = "10 - 3 + 2";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(9.0, result);
    }

    [Fact]
    public void Evaluate_ComplexPrecedence_ReturnsCorrectResult()
    {
        // Arrange: 1 + 2 * 3 - 4 / 2 should be 1 + (2 * 3) - (4 / 2) = 1 + 6 - 2 = 5
        var code = "1 + 2 * 3 - 4 / 2";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(5.0, result);
    }

    #endregion

    #region Parentheses Override Precedence Tests

    [Fact]
    public void Evaluate_ParenthesesOverridePrecedence_ReturnsCorrectResult()
    {
        // Arrange: (2 + 3) * 4 should be 5 * 4 = 20
        var code = "(2 + 3) * 4";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(20.0, result);
    }

    [Fact]
    public void Evaluate_NestedParentheses_ReturnsCorrectResult()
    {
        // Arrange: ((2 + 3) * 4) - (1 * 2) should be (5 * 4) - 2 = 20 - 2 = 18
        var code = "((2 + 3) * 4) - (1 * 2)";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(18.0, result);
    }

    [Fact]
    public void Evaluate_ParenthesesInDivision_ReturnsCorrectResult()
    {
        // Arrange: 12 / (2 + 1) should be 12 / 3 = 4
        var code = "12 / (2 + 1)";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(4.0, result);
    }

    #endregion

    #region Comparison Operator Precedence Tests

    [Fact]
    public void Evaluate_ComparisonLowerThanArithmetic_ReturnsCorrectResult()
    {
        // Arrange: 2 + 3 > 4 should be (2 + 3) > 4 = 5 > 4 = true
        var code = "2 + 3 > 4";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(true, result);
    }

    [Fact]
    public void Evaluate_ArithmeticInComparison_ReturnsCorrectResult()
    {
        // Arrange: 2 * 3 == 3 + 3 should be (2 * 3) == (3 + 3) = 6 == 6 = true
        var code = "2 * 3 == 3 + 3";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(true, result);
    }

    #endregion

    #region AST Structure Verification Tests

    [Fact]
    public void Parse_MultiplicationBeforeAddition_CreatesCorrectAST()
    {
        // Arrange: 2 + 3 * 4 should parse as BinaryExpression(2, "+", BinaryExpression(3, "*", 4))
        var code = "2 + 3 * 4";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert
        Assert.NotNull(expression);
        Assert.Equal("+", expression!.Operator);
        
        // Left side should be NumberLiteral(2)
        var leftLiteral = expression.Left as NumberLiteral;
        Assert.NotNull(leftLiteral);
        Assert.Equal(2.0, leftLiteral!.Value);
        
        // Right side should be BinaryExpression(3, "*", 4)
        var rightBinary = expression.Right as BinaryExpression;
        Assert.NotNull(rightBinary);
        Assert.Equal("*", rightBinary!.Operator);
        
        var rightLeft = rightBinary.Left as NumberLiteral;
        var rightRight = rightBinary.Right as NumberLiteral;
        Assert.Equal(3.0, rightLeft!.Value);
        Assert.Equal(4.0, rightRight!.Value);
    }

    [Fact]
    public void Parse_ParenthesesOverridePrecedence_CreatesCorrectAST()
    {
        // Arrange: (2 + 3) * 4 should parse as BinaryExpression(BinaryExpression(2, "+", 3), "*", 4)
        var code = "(2 + 3) * 4";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert
        Assert.NotNull(expression);
        Assert.Equal("*", expression!.Operator);
        
        // Left side should be BinaryExpression(2, "+", 3)
        var leftBinary = expression.Left as BinaryExpression;
        Assert.NotNull(leftBinary);
        Assert.Equal("+", leftBinary!.Operator);
        
        var leftLeft = leftBinary.Left as NumberLiteral;
        var leftRight = leftBinary.Right as NumberLiteral;
        Assert.Equal(2.0, leftLeft!.Value);
        Assert.Equal(3.0, leftRight!.Value);
        
        // Right side should be NumberLiteral(4)
        var rightLiteral = expression.Right as NumberLiteral;
        Assert.NotNull(rightLiteral);
        Assert.Equal(4.0, rightLiteral!.Value);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Evaluate_UnaryMinusWithPrecedence_ReturnsCorrectResult()
    {
        // Arrange: -2 * 3 should be (-2) * 3 = -6
        var code = "-2 * 3";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(-6.0, result);
    }

    [Fact]
    public void Evaluate_UnaryMinusInExpression_ReturnsCorrectResult()
    {
        // Arrange: 1 + -2 * 3 should be 1 + ((-2) * 3) = 1 + (-6) = -5
        var code = "1 + -2 * 3";
        
        // Act
        var ast = _parser.Parse(code);
        var result = _interpreter.Evaluate(ast, code);
        
        // Assert
        Assert.Equal(-5.0, result);
    }

    #endregion
}
