using Xunit;
using ECEngine.Lexer;

namespace ECEngine.Tests.Lexer;

public class OperatorTokenizationTests
{
    [Fact]
    public void Tokenize_PlusOperator_ReturnsPlusToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("+");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Plus, tokens[0].Type);
        Assert.Equal("+", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_MinusOperator_ReturnsMinusToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("-");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Minus, tokens[0].Type);
        Assert.Equal("-", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_MultiplyOperator_ReturnsMultiplyToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("*");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Multiply, tokens[0].Type);
        Assert.Equal("*", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_DivideOperator_ReturnsDivideToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("/");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Divide, tokens[0].Type);
        Assert.Equal("/", tokens[0].Value);
    }
}
