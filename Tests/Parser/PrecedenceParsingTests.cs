using Xunit;
using ECEngine.AST;
using ParserClass = ECEngine.Parser.Parser;

namespace ECEngine.Tests.Parser;

/// <summary>
/// Tests to verify that the parser correctly handles operator precedence when building the AST
/// </summary>
public class PrecedenceParsingTests
{
    private readonly ParserClass _parser;

    public PrecedenceParsingTests()
    {
        _parser = new ParserClass();
    }

    #region Precedence Level Verification

    [Fact]
    public void Parse_AdditionAndMultiplication_MultiplicationHasHigherPrecedence()
    {
        // Arrange
        var code = "1 + 2 * 3";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert: Should be parsed as 1 + (2 * 3), not (1 + 2) * 3
        Assert.Equal("+", expression!.Operator);
        Assert.IsType<NumberLiteral>(expression.Left);
        Assert.IsType<BinaryExpression>(expression.Right);
        
        var rightExpression = expression.Right as BinaryExpression;
        Assert.Equal("*", rightExpression!.Operator);
    }

    [Fact]
    public void Parse_SubtractionAndDivision_DivisionHasHigherPrecedence()
    {
        // Arrange
        var code = "10 - 6 / 2";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert: Should be parsed as 10 - (6 / 2), not (10 - 6) / 2
        Assert.Equal("-", expression!.Operator);
        Assert.IsType<NumberLiteral>(expression.Left);
        Assert.IsType<BinaryExpression>(expression.Right);
        
        var rightExpression = expression.Right as BinaryExpression;
        Assert.Equal("/", rightExpression!.Operator);
    }

    [Fact]
    public void Parse_ComparisonAndArithmetic_ArithmeticHasHigherPrecedence()
    {
        // Arrange
        var code = "2 + 3 > 4";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert: Should be parsed as (2 + 3) > 4, not 2 + (3 > 4)
        Assert.Equal(">", expression!.Operator);
        Assert.IsType<BinaryExpression>(expression.Left);
        Assert.IsType<NumberLiteral>(expression.Right);
        
        var leftExpression = expression.Left as BinaryExpression;
        Assert.Equal("+", leftExpression!.Operator);
    }

    #endregion

    #region Left Associativity Tests

    [Fact]
    public void Parse_ChainedAddition_IsLeftAssociative()
    {
        // Arrange
        var code = "1 + 2 + 3";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert: Should be parsed as (1 + 2) + 3
        Assert.Equal("+", expression!.Operator);
        Assert.IsType<BinaryExpression>(expression.Left);
        Assert.IsType<NumberLiteral>(expression.Right);
        
        var leftExpression = expression.Left as BinaryExpression;
        Assert.Equal("+", leftExpression!.Operator);
        Assert.Equal(1.0, (leftExpression.Left as NumberLiteral)!.Value);
        Assert.Equal(2.0, (leftExpression.Right as NumberLiteral)!.Value);
        Assert.Equal(3.0, (expression.Right as NumberLiteral)!.Value);
    }

    [Fact]
    public void Parse_ChainedMultiplication_IsLeftAssociative()
    {
        // Arrange
        var code = "2 * 3 * 4";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert: Should be parsed as (2 * 3) * 4
        Assert.Equal("*", expression!.Operator);
        Assert.IsType<BinaryExpression>(expression.Left);
        Assert.IsType<NumberLiteral>(expression.Right);
        
        var leftExpression = expression.Left as BinaryExpression;
        Assert.Equal("*", leftExpression!.Operator);
        Assert.Equal(2.0, (leftExpression.Left as NumberLiteral)!.Value);
        Assert.Equal(3.0, (leftExpression.Right as NumberLiteral)!.Value);
        Assert.Equal(4.0, (expression.Right as NumberLiteral)!.Value);
    }

    [Fact]
    public void Parse_MixedSamePrecedence_IsLeftAssociative()
    {
        // Arrange
        var code = "10 - 3 + 2";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert: Should be parsed as (10 - 3) + 2
        Assert.Equal("+", expression!.Operator);
        Assert.IsType<BinaryExpression>(expression.Left);
        Assert.IsType<NumberLiteral>(expression.Right);
        
        var leftExpression = expression.Left as BinaryExpression;
        Assert.Equal("-", leftExpression!.Operator);
        Assert.Equal(10.0, (leftExpression.Left as NumberLiteral)!.Value);
        Assert.Equal(3.0, (leftExpression.Right as NumberLiteral)!.Value);
        Assert.Equal(2.0, (expression.Right as NumberLiteral)!.Value);
    }

    #endregion

    #region Complex Precedence Tests

    [Fact]
    public void Parse_ComplexExpression_CorrectPrecedenceOrder()
    {
        // Arrange: 1 + 2 * 3 - 4 / 2
        // Should be: 1 + (2 * 3) - (4 / 2) = ((1 + (2 * 3)) - (4 / 2))
        var code = "1 + 2 * 3 - 4 / 2";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert: Top level should be subtraction
        Assert.Equal("-", expression!.Operator);
        
        // Left side should be addition: 1 + (2 * 3)
        var leftBinary = expression.Left as BinaryExpression;
        Assert.Equal("+", leftBinary!.Operator);
        Assert.Equal(1.0, (leftBinary.Left as NumberLiteral)!.Value);
        
        var leftRight = leftBinary.Right as BinaryExpression;
        Assert.Equal("*", leftRight!.Operator);
        Assert.Equal(2.0, (leftRight.Left as NumberLiteral)!.Value);
        Assert.Equal(3.0, (leftRight.Right as NumberLiteral)!.Value);
        
        // Right side should be division: 4 / 2
        var rightBinary = expression.Right as BinaryExpression;
        Assert.Equal("/", rightBinary!.Operator);
        Assert.Equal(4.0, (rightBinary.Left as NumberLiteral)!.Value);
        Assert.Equal(2.0, (rightBinary.Right as NumberLiteral)!.Value);
    }

    [Fact]
    public void Parse_MultipleComparisonAndArithmetic_CorrectPrecedence()
    {
        // Arrange: 2 * 3 == 3 + 3
        // Should be: (2 * 3) == (3 + 3)
        var code = "2 * 3 == 3 + 3";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert: Top level should be equality comparison
        Assert.Equal("==", expression!.Operator);
        
        // Left side should be multiplication: 2 * 3
        var leftBinary = expression.Left as BinaryExpression;
        Assert.Equal("*", leftBinary!.Operator);
        Assert.Equal(2.0, (leftBinary.Left as NumberLiteral)!.Value);
        Assert.Equal(3.0, (leftBinary.Right as NumberLiteral)!.Value);
        
        // Right side should be addition: 3 + 3
        var rightBinary = expression.Right as BinaryExpression;
        Assert.Equal("+", rightBinary!.Operator);
        Assert.Equal(3.0, (rightBinary.Left as NumberLiteral)!.Value);
        Assert.Equal(3.0, (rightBinary.Right as NumberLiteral)!.Value);
    }

    #endregion

    #region Parentheses Override Tests

    [Fact]
    public void Parse_ParenthesesOverridePrecedence_CorrectAST()
    {
        // Arrange
        var code = "(1 + 2) * 3";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert: Should be parsed as (1 + 2) * 3
        Assert.Equal("*", expression!.Operator);
        Assert.IsType<BinaryExpression>(expression.Left);
        Assert.IsType<NumberLiteral>(expression.Right);
        
        var leftExpression = expression.Left as BinaryExpression;
        Assert.Equal("+", leftExpression!.Operator);
        Assert.Equal(1.0, (leftExpression.Left as NumberLiteral)!.Value);
        Assert.Equal(2.0, (leftExpression.Right as NumberLiteral)!.Value);
        Assert.Equal(3.0, (expression.Right as NumberLiteral)!.Value);
    }

    [Fact]
    public void Parse_NestedParentheses_CorrectAST()
    {
        // Arrange
        var code = "((1 + 2) * 3) - 4";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert: Top level should be subtraction
        Assert.Equal("-", expression!.Operator);
        Assert.Equal(4.0, (expression.Right as NumberLiteral)!.Value);
        
        // Left side should be multiplication
        var leftBinary = expression.Left as BinaryExpression;
        Assert.Equal("*", leftBinary!.Operator);
        Assert.Equal(3.0, (leftBinary.Right as NumberLiteral)!.Value);
        
        // Left side of multiplication should be addition
        var additionBinary = leftBinary.Left as BinaryExpression;
        Assert.Equal("+", additionBinary!.Operator);
        Assert.Equal(1.0, (additionBinary.Left as NumberLiteral)!.Value);
        Assert.Equal(2.0, (additionBinary.Right as NumberLiteral)!.Value);
    }

    #endregion

    #region Unary Operator Precedence Tests

    [Fact]
    public void Parse_UnaryMinusWithBinaryOperator_UnaryHasHigherPrecedence()
    {
        // Arrange: -2 * 3 should be (-2) * 3, not -(2 * 3)
        var code = "-2 * 3";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert: Should be parsed as (-2) * 3
        Assert.Equal("*", expression!.Operator);
        Assert.IsType<UnaryExpression>(expression.Left);
        Assert.IsType<NumberLiteral>(expression.Right);
        
        var unaryExpression = expression.Left as UnaryExpression;
        Assert.Equal("unary-", unaryExpression!.Operator);
        Assert.Equal(2.0, (unaryExpression.Operand as NumberLiteral)!.Value);
        Assert.Equal(3.0, (expression.Right as NumberLiteral)!.Value);
    }

    [Fact]
    public void Parse_UnaryPlusInExpression_CorrectPrecedence()
    {
        // Arrange: 1 + +2 * 3 should be 1 + ((+2) * 3)
        var code = "1 + +2 * 3";
        
        // Act
        var ast = _parser.Parse(code) as ProgramNode;
        var expression = (ast!.Body[0] as ExpressionStatement)!.Expression as BinaryExpression;
        
        // Assert: Top level should be addition
        Assert.Equal("+", expression!.Operator);
        Assert.Equal(1.0, (expression.Left as NumberLiteral)!.Value);
        
        // Right side should be multiplication with unary plus
        var rightBinary = expression.Right as BinaryExpression;
        Assert.Equal("*", rightBinary!.Operator);
        Assert.Equal(3.0, (rightBinary.Right as NumberLiteral)!.Value);
        
        var unaryExpression = rightBinary.Left as UnaryExpression;
        Assert.Equal("unary+", unaryExpression!.Operator);
        Assert.Equal(2.0, (unaryExpression.Operand as NumberLiteral)!.Value);
    }

    #endregion
}
