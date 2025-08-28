using Xunit;
using ECEngine.Parser;
using ECEngine.AST;

namespace ECEngine.Tests.Parser;

public class BinaryExpressionParsingTests
{
    [Fact]
    public void Parse_SimpleAddition_ReturnsBinaryExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("1 + 2");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

        Assert.Equal("+", binaryExpression.Operator);

        var left = Assert.IsType<NumberLiteral>(binaryExpression.Left);
        Assert.Equal(1, left.Value);

        var right = Assert.IsType<NumberLiteral>(binaryExpression.Right);
        Assert.Equal(2, right.Value);
    }

    [Fact]
    public void Parse_Subtraction_ReturnsBinaryExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("5 - 3");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

        Assert.Equal("-", binaryExpression.Operator);
        Assert.IsType<NumberLiteral>(binaryExpression.Left);
        Assert.IsType<NumberLiteral>(binaryExpression.Right);
    }

    [Fact]
    public void Parse_Multiplication_ReturnsBinaryExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("3 * 4");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

        Assert.Equal("*", binaryExpression.Operator);
    }

    [Fact]
    public void Parse_Division_ReturnsBinaryExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("8 / 2");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var binaryExpression = Assert.IsType<BinaryExpression>(expressionStatement.Expression);

        Assert.Equal("/", binaryExpression.Operator);
    }
}
