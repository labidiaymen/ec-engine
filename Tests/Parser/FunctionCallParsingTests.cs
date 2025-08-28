using Xunit;
using ECEngine.Parser;
using ECEngine.AST;

namespace ECEngine.Tests.Parser;

public class FunctionCallParsingTests
{
    [Fact]
    public void Parse_SimpleConsoleLog_ReturnsCallExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("console.log(42)");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var callExpression = Assert.IsType<CallExpression>(expressionStatement.Expression);

        // Verify the callee is a member expression (console.log)
        var memberExpression = Assert.IsType<MemberExpression>(callExpression.Callee);
        var consoleIdentifier = Assert.IsType<Identifier>(memberExpression.Object);
        Assert.Equal("console", consoleIdentifier.Name);
        Assert.Equal("log", memberExpression.Property);

        // Verify the argument
        Assert.Single(callExpression.Arguments);
        var argument = Assert.IsType<NumberLiteral>(callExpression.Arguments[0]);
        Assert.Equal(42, argument.Value);
    }

    [Fact]
    public void Parse_ConsoleLogWithExpression_ReturnsCallExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("console.log(1 + 2)");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var callExpression = Assert.IsType<CallExpression>(expressionStatement.Expression);

        // Verify the argument is a binary expression
        Assert.Single(callExpression.Arguments);
        var binaryExpression = Assert.IsType<BinaryExpression>(callExpression.Arguments[0]);
        Assert.Equal("+", binaryExpression.Operator);
    }

    [Fact]
    public void Parse_MemberExpression_ReturnsMemberExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("console.log");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var memberExpression = Assert.IsType<MemberExpression>(expressionStatement.Expression);

        var consoleIdentifier = Assert.IsType<Identifier>(memberExpression.Object);
        Assert.Equal("console", consoleIdentifier.Name);
        Assert.Equal("log", memberExpression.Property);
    }
}
