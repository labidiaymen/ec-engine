using Xunit;
using ECEngine.Parser;
using ECEngine.AST;

namespace ECEngine.Tests.Parser;

public class LiteralParsingTests
{
    [Fact]
    public void Parse_SimpleNumber_ReturnsNumberLiteral()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("42");

        // Assert
        Assert.IsType<ProgramNode>(ast);
        var program = (ProgramNode)ast;
        Assert.Single(program.Body);

        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var numberLiteral = Assert.IsType<NumberLiteral>(expressionStatement.Expression);
        Assert.Equal(42, numberLiteral.Value);
    }

    [Fact]
    public void Parse_DecimalNumber_ReturnsNumberLiteral()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("3.14");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var numberLiteral = Assert.IsType<NumberLiteral>(expressionStatement.Expression);
        Assert.Equal(3.14, numberLiteral.Value);
    }

    [Fact]
    public void Parse_Identifier_ReturnsIdentifier()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("console");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var identifier = Assert.IsType<Identifier>(expressionStatement.Expression);
        Assert.Equal("console", identifier.Name);
    }
}
