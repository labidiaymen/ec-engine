using Xunit;
using ECEngine.Lexer;

namespace ECEngine.Tests.Lexer;

public class BasicTokenizationTests
{
    [Fact]
    public void Tokenize_SimpleNumber_ReturnsNumberToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("42");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count); // Number + EOF
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal("42", tokens[0].Value);
        Assert.Equal(TokenType.EOF, tokens[1].Type);
    }

    [Fact]
    public void Tokenize_SimpleIdentifier_ReturnsIdentifierToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("console");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal("console", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_DecimalNumber_ReturnsNumberToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("3.14");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal("3.14", tokens[0].Value);
    }
}
