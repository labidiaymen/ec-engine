using Xunit;
using ECEngine.Lexer;

namespace ECEngine.Tests.Lexer;

public class LocationTrackingTests
{
    [Fact]
    public void Tokenize_SingleToken_HasCorrectLocation()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("42");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(1, tokens[0].Line);
        Assert.Equal(1, tokens[0].Column);
    }

    [Fact]
    public void Tokenize_MultipleTokens_HasCorrectLocations()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("1 + 2");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        // First token (1) at line 1, column 1
        Assert.Equal(1, tokens[0].Line);
        Assert.Equal(1, tokens[0].Column);

        // Second token (+) at line 1, column 3
        Assert.Equal(1, tokens[1].Line);
        Assert.Equal(3, tokens[1].Column);

        // Third token (2) at line 1, column 5
        Assert.Equal(1, tokens[2].Line);
        Assert.Equal(5, tokens[2].Column);
    }

    [Fact]
    public void Tokenize_InvalidCharacter_ThrowsWithLocation()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("1 @ 2");

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => lexer.Tokenize());
        Assert.Contains("line 1, column 3", exception.Message);
    }
}
