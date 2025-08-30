using Xunit;
using ECEngine.Parser;
using ECEngine.AST;

namespace ECEngine.Tests.Parser;

public class AdvancedOperatorParsingTests
{
    #region Strict Comparison Operators

    [Fact]
    public void Parse_StrictEqualExpression_ReturnsBinaryExpressionWithStrictEqual()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("5 === 5");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

        Assert.Equal("===", binaryExpression.Operator);

        var left = Assert.IsType<NumberLiteral>(binaryExpression.Left);
        Assert.Equal(5, left.Value);

        var right = Assert.IsType<NumberLiteral>(binaryExpression.Right);
        Assert.Equal(5, right.Value);
    }

    [Fact]
    public void Parse_StrictNotEqualExpression_ReturnsBinaryExpressionWithStrictNotEqual()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("5 !== \"5\"");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

        Assert.Equal("!==", binaryExpression.Operator);

        var left = Assert.IsType<NumberLiteral>(binaryExpression.Left);
        Assert.Equal(5, left.Value);

        var right = Assert.IsType<StringLiteral>(binaryExpression.Right);
        Assert.Equal("5", right.Value);
    }

    #endregion

    #region Compound Assignment Operators

    [Fact]
    public void Parse_PlusAssignExpression_ReturnsCompoundAssignmentExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("x += 5");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var compoundAssignment = Assert.IsType<CompoundAssignmentExpression>(expressionStatement.Expression);

        Assert.Equal("+=", compoundAssignment.Operator);

        var left = Assert.IsType<Identifier>(compoundAssignment.Left);
        Assert.Equal("x", left.Name);

        var right = Assert.IsType<NumberLiteral>(compoundAssignment.Right);
        Assert.Equal(5, right.Value);
    }

    [Fact]
    public void Parse_MinusAssignExpression_ReturnsCompoundAssignmentExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("y -= 3");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var compoundAssignment = Assert.IsType<CompoundAssignmentExpression>(expressionStatement.Expression);

        Assert.Equal("-=", compoundAssignment.Operator);

        var left = Assert.IsType<Identifier>(compoundAssignment.Left);
        Assert.Equal("y", left.Name);

        var right = Assert.IsType<NumberLiteral>(compoundAssignment.Right);
        Assert.Equal(3, right.Value);
    }

    [Fact]
    public void Parse_MultiplyAssignExpression_ReturnsCompoundAssignmentExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("z *= 2");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var compoundAssignment = Assert.IsType<CompoundAssignmentExpression>(expressionStatement.Expression);

        Assert.Equal("*=", compoundAssignment.Operator);

        var left = Assert.IsType<Identifier>(compoundAssignment.Left);
        Assert.Equal("z", left.Name);

        var right = Assert.IsType<NumberLiteral>(compoundAssignment.Right);
        Assert.Equal(2, right.Value);
    }

    [Fact]
    public void Parse_DivideAssignExpression_ReturnsCompoundAssignmentExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("w /= 4");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var compoundAssignment = Assert.IsType<CompoundAssignmentExpression>(expressionStatement.Expression);

        Assert.Equal("/=", compoundAssignment.Operator);

        var left = Assert.IsType<Identifier>(compoundAssignment.Left);
        Assert.Equal("w", left.Name);

        var right = Assert.IsType<NumberLiteral>(compoundAssignment.Right);
        Assert.Equal(4, right.Value);
    }

    #endregion

    #region Bitwise Operators

    [Fact]
    public void Parse_BitwiseAndExpression_ReturnsBinaryExpressionWithBitwiseAnd()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("12 & 10");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

        Assert.Equal("&", binaryExpression.Operator);

        var left = Assert.IsType<NumberLiteral>(binaryExpression.Left);
        Assert.Equal(12, left.Value);

        var right = Assert.IsType<NumberLiteral>(binaryExpression.Right);
        Assert.Equal(10, right.Value);
    }

    [Fact]
    public void Parse_BitwiseOrExpression_ReturnsBinaryExpressionWithBitwiseOr()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("12 | 10");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

        Assert.Equal("|", binaryExpression.Operator);

        var left = Assert.IsType<NumberLiteral>(binaryExpression.Left);
        Assert.Equal(12, left.Value);

        var right = Assert.IsType<NumberLiteral>(binaryExpression.Right);
        Assert.Equal(10, right.Value);
    }

    [Fact]
    public void Parse_BitwiseXorExpression_ReturnsBinaryExpressionWithBitwiseXor()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("12 ^ 10");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

        Assert.Equal("^", binaryExpression.Operator);

        var left = Assert.IsType<NumberLiteral>(binaryExpression.Left);
        Assert.Equal(12, left.Value);

        var right = Assert.IsType<NumberLiteral>(binaryExpression.Right);
        Assert.Equal(10, right.Value);
    }

    [Fact]
    public void Parse_BitwiseNotExpression_ReturnsUnaryExpressionWithBitwiseNot()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("~12");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var unaryExpression = Assert.IsType<UnaryExpression>(expressionStatement.Expression);

        Assert.Equal("~", unaryExpression.Operator);

        var operand = Assert.IsType<NumberLiteral>(unaryExpression.Operand);
        Assert.Equal(12, operand.Value);
    }

    [Fact]
    public void Parse_LeftShiftExpression_ReturnsBinaryExpressionWithLeftShift()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("8 << 2");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

        Assert.Equal("<<", binaryExpression.Operator);

        var left = Assert.IsType<NumberLiteral>(binaryExpression.Left);
        Assert.Equal(8, left.Value);

        var right = Assert.IsType<NumberLiteral>(binaryExpression.Right);
        Assert.Equal(2, right.Value);
    }

    [Fact]
    public void Parse_RightShiftExpression_ReturnsBinaryExpressionWithRightShift()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("8 >> 1");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

        Assert.Equal(">>", binaryExpression.Operator);

        var left = Assert.IsType<NumberLiteral>(binaryExpression.Left);
        Assert.Equal(8, left.Value);

        var right = Assert.IsType<NumberLiteral>(binaryExpression.Right);
        Assert.Equal(1, right.Value);
    }

    [Fact]
    public void Parse_UnsignedRightShiftExpression_ReturnsBinaryExpressionWithUnsignedRightShift()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("8 >>> 1");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

        Assert.Equal(">>>", binaryExpression.Operator);

        var left = Assert.IsType<NumberLiteral>(binaryExpression.Left);
        Assert.Equal(8, left.Value);

        var right = Assert.IsType<NumberLiteral>(binaryExpression.Right);
        Assert.Equal(1, right.Value);
    }

    #endregion

    #region Ternary Operator

    [Fact]
    public void Parse_SimpleTernaryExpression_ReturnsConditionalExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("true ? 1 : 2");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var conditionalExpression = Assert.IsType<ConditionalExpression>(expressionStatement.Expression);

        var test = Assert.IsType<BooleanLiteral>(conditionalExpression.Test);
        Assert.True(test.Value);

        var consequent = Assert.IsType<NumberLiteral>(conditionalExpression.Consequent);
        Assert.Equal(1, consequent.Value);

        var alternate = Assert.IsType<NumberLiteral>(conditionalExpression.Alternate);
        Assert.Equal(2, alternate.Value);
    }

    [Fact]
    public void Parse_NestedTernaryExpression_ReturnsNestedConditionalExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("x > 0 ? 1 : x < 0 ? -1 : 0");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var conditionalExpression = Assert.IsType<ConditionalExpression>(expressionStatement.Expression);

        // Test condition (x > 0)
        var test = Assert.IsType<BinaryExpression>(conditionalExpression.Test);
        Assert.Equal(">", test.Operator);

        // Consequent (1)
        var consequent = Assert.IsType<NumberLiteral>(conditionalExpression.Consequent);
        Assert.Equal(1, consequent.Value);

        // Alternate (nested ternary: x < 0 ? -1 : 0)
        var nestedConditional = Assert.IsType<ConditionalExpression>(conditionalExpression.Alternate);
        var nestedTest = Assert.IsType<BinaryExpression>(nestedConditional.Test);
        Assert.Equal("<", nestedTest.Operator);
    }

    #endregion

    #region Operator Precedence Tests

    [Fact]
    public void Parse_BitwiseOperatorPrecedence_ReturnsCorrectASTStructure()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act - Testing that | has lower precedence than ^, which has lower than &
        var ast = parser.Parse("a | b ^ c & d");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        
        // Should parse as: a | (b ^ (c & d))
        var orExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);
        Assert.Equal("|", orExpression.Operator);

        var xorExpression = Assert.IsType<BinaryExpression>(orExpression.Right);
        Assert.Equal("^", xorExpression.Operator);

        var andExpression = Assert.IsType<BinaryExpression>(xorExpression.Right);
        Assert.Equal("&", andExpression.Operator);
    }

    [Fact]
    public void Parse_ShiftOperatorPrecedence_ReturnsCorrectASTStructure()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act - Testing that shift operators have higher precedence than bitwise operators
        var ast = parser.Parse("a & b << 2");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        
        // Should parse as: a & (b << 2)
        var andExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);
        Assert.Equal("&", andExpression.Operator);

        var shiftExpression = Assert.IsType<BinaryExpression>(andExpression.Right);
        Assert.Equal("<<", shiftExpression.Operator);
    }

    [Fact]
    public void Parse_CompoundAssignmentWithTernary_ReturnsCorrectASTStructure()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("x += condition ? 1 : -1");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var compoundAssignment = Assert.IsType<CompoundAssignmentExpression>(expressionStatement.Expression);

        Assert.Equal("+=", compoundAssignment.Operator);

        var left = Assert.IsType<Identifier>(compoundAssignment.Left);
        Assert.Equal("x", left.Name);

        // Right side should be a conditional expression
        var conditionalExpression = Assert.IsType<ConditionalExpression>(compoundAssignment.Right);
        var test = Assert.IsType<Identifier>(conditionalExpression.Test);
        Assert.Equal("condition", test.Name);
    }

    #endregion

    #region Complex Expression Tests

    [Theory]
    [InlineData("5 === 5 && 3 !== \"3\"")]
    [InlineData("x += y *= z")]
    [InlineData("a | b & c ^ d")]
    [InlineData("x << 2 >> 1 >>> 1")]
    [InlineData("condition ? true : false")]
    [InlineData("~x & 255")]
    public void Parse_ComplexAdvancedOperatorExpressions_ParsesWithoutErrors(string expression)
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act & Assert - Should not throw any exceptions
        var ast = parser.Parse(expression);
        
        // Verify we got a valid AST
        Assert.NotNull(ast);
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
    }

    #endregion
}
