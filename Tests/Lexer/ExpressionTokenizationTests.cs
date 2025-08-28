using Xunit;
using ECEngine.Lexer;

namespace ECEngine.Tests.Lexer;

public class ExpressionTokenizationTests
{
    [Fact]
    public void Tokenize_SimpleAddition_ReturnsCorrectTokens()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("1 + 2");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(4, tokens.Count); // Number, Plus, Number, EOF
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal("1", tokens[0].Value);
        Assert.Equal(TokenType.Plus, tokens[1].Type);
        Assert.Equal("+", tokens[1].Value);
        Assert.Equal(TokenType.Number, tokens[2].Type);
        Assert.Equal("2", tokens[2].Value);
        Assert.Equal(TokenType.EOF, tokens[3].Type);
    }

    [Fact]
    public void Tokenize_ComplexExpression_ReturnsCorrectTokens()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("10 * 5 + 3");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(6, tokens.Count); // Number, Multiply, Number, Plus, Number, EOF
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal("10", tokens[0].Value);
        Assert.Equal(TokenType.Multiply, tokens[1].Type);
        Assert.Equal(TokenType.Number, tokens[2].Type);
        Assert.Equal("5", tokens[2].Value);
        Assert.Equal(TokenType.Plus, tokens[3].Type);
        Assert.Equal(TokenType.Number, tokens[4].Type);
        Assert.Equal("3", tokens[4].Value);
    }

    [Fact]
    public void Tokenize_ParenthesesExpression_ReturnsCorrectTokens()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("(1 + 2)");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(6, tokens.Count); // LeftParen, Number, Plus, Number, RightParen, EOF
        Assert.Equal(TokenType.LeftParen, tokens[0].Type);
        Assert.Equal(TokenType.Number, tokens[1].Type);
        Assert.Equal(TokenType.Plus, tokens[2].Type);
        Assert.Equal(TokenType.Number, tokens[3].Type);
        Assert.Equal(TokenType.RightParen, tokens[4].Type);
    }
}
