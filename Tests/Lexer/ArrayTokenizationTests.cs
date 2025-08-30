using Xunit;
using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.Runtime;

namespace ECEngine.Tests.Lexer;

/// <summary>
/// Tests for array bracket tokenization
/// </summary>
[Collection("Console Tests")]
public class ArrayTokenizationTests
{
    [Fact]
    public void Test_LeftBracket_Tokenization()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("[");
        
        // Act
        var tokens = lexer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count); // [ and EOF
        Assert.Equal(TokenType.LeftBracket, tokens[0].Type);
        Assert.Equal("[", tokens[0].Value);
    }

    [Fact]
    public void Test_RightBracket_Tokenization()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("]");
        
        // Act
        var tokens = lexer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count); // ] and EOF
        Assert.Equal(TokenType.RightBracket, tokens[0].Type);
        Assert.Equal("]", tokens[0].Value);
    }

    [Fact]
    public void Test_ArrayLiteral_Tokenization()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("[1, 2, 3]");
        
        // Act
        var tokens = lexer.Tokenize();
        
        // Assert
        Assert.Equal(8, tokens.Count); // [, 1, ,, 2, ,, 3, ], EOF
        Assert.Equal(TokenType.LeftBracket, tokens[0].Type);
        Assert.Equal(TokenType.Number, tokens[1].Type);
        Assert.Equal(TokenType.Comma, tokens[2].Type);
        Assert.Equal(TokenType.Number, tokens[3].Type);
        Assert.Equal(TokenType.Comma, tokens[4].Type);
        Assert.Equal(TokenType.Number, tokens[5].Type);
        Assert.Equal(TokenType.RightBracket, tokens[6].Type);
        Assert.Equal(TokenType.EOF, tokens[7].Type);
    }

    [Fact]
    public void Test_ArrayAccess_Tokenization()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("arr[0]");
        
        // Act
        var tokens = lexer.Tokenize();
        
        // Assert
        Assert.Equal(5, tokens.Count); // arr, [, 0, ], EOF
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal("arr", tokens[0].Value);
        Assert.Equal(TokenType.LeftBracket, tokens[1].Type);
        Assert.Equal(TokenType.Number, tokens[2].Type);
        Assert.Equal(TokenType.RightBracket, tokens[3].Type);
        Assert.Equal(TokenType.EOF, tokens[4].Type);
    }
}
