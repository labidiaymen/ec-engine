using ECEngine.Lexer;
using Xunit;

namespace ECEngine.Tests.Lexer;

[Collection("Console Tests")]
public class ForInOfTokenizationTests
{
    [Fact]
    public void TestForInLoopTokenization()
    {
        // Arrange
        var code = "for (key in object) { console.log(key); }";
        var lexer = new ECEngine.Lexer.Lexer(code);
        
        // Act
        var tokens = lexer.Tokenize();
        
        // Assert
        Assert.Equal(TokenType.For, tokens[0].Type);
        Assert.Equal(TokenType.LeftParen, tokens[1].Type);
        Assert.Equal(TokenType.Identifier, tokens[2].Type);
        Assert.Equal("key", tokens[2].Value);
        Assert.Equal(TokenType.In, tokens[3].Type);
        Assert.Equal("in", tokens[3].Value);
        Assert.Equal(TokenType.Identifier, tokens[4].Type);
        Assert.Equal("object", tokens[4].Value);
        Assert.Equal(TokenType.RightParen, tokens[5].Type);
    }
    
    [Fact]
    public void TestForOfLoopTokenization()
    {
        // Arrange
        var code = "for (item of array) { console.log(item); }";
        var lexer = new ECEngine.Lexer.Lexer(code);
        
        // Act
        var tokens = lexer.Tokenize();
        
        // Assert
        Assert.Equal(TokenType.For, tokens[0].Type);
        Assert.Equal(TokenType.LeftParen, tokens[1].Type);
        Assert.Equal(TokenType.Identifier, tokens[2].Type);
        Assert.Equal("item", tokens[2].Value);
        Assert.Equal(TokenType.Of, tokens[3].Type);
        Assert.Equal("of", tokens[3].Value);
        Assert.Equal(TokenType.Identifier, tokens[4].Type);
        Assert.Equal("array", tokens[4].Value);
        Assert.Equal(TokenType.RightParen, tokens[5].Type);
    }
    
    [Fact]
    public void TestForInWithVarDeclaration()
    {
        // Arrange
        var code = "for (var key in obj) {}";
        var lexer = new ECEngine.Lexer.Lexer(code);
        
        // Act
        var tokens = lexer.Tokenize();
        
        // Assert
        Assert.Equal(TokenType.For, tokens[0].Type);
        Assert.Equal(TokenType.LeftParen, tokens[1].Type);
        Assert.Equal(TokenType.Var, tokens[2].Type);
        Assert.Equal(TokenType.Identifier, tokens[3].Type);
        Assert.Equal("key", tokens[3].Value);
        Assert.Equal(TokenType.In, tokens[4].Type);
        Assert.Equal(TokenType.Identifier, tokens[5].Type);
        Assert.Equal("obj", tokens[5].Value);
        Assert.Equal(TokenType.RightParen, tokens[6].Type);
    }
    
    [Fact]
    public void TestForOfWithLetDeclaration()
    {
        // Arrange
        var code = "for (let item of items) {}";
        var lexer = new ECEngine.Lexer.Lexer(code);
        
        // Act
        var tokens = lexer.Tokenize();
        
        // Assert
        Assert.Equal(TokenType.For, tokens[0].Type);
        Assert.Equal(TokenType.LeftParen, tokens[1].Type);
        Assert.Equal(TokenType.Let, tokens[2].Type);
        Assert.Equal(TokenType.Identifier, tokens[3].Type);
        Assert.Equal("item", tokens[3].Value);
        Assert.Equal(TokenType.Of, tokens[4].Type);
        Assert.Equal(TokenType.Identifier, tokens[5].Type);
        Assert.Equal("items", tokens[5].Value);
        Assert.Equal(TokenType.RightParen, tokens[6].Type);
    }
    
    [Fact]
    public void TestForOfWithConstDeclaration()
    {
        // Arrange
        var code = "for (const element of elements) {}";
        var lexer = new ECEngine.Lexer.Lexer(code);
        
        // Act
        var tokens = lexer.Tokenize();
        
        // Assert
        Assert.Equal(TokenType.For, tokens[0].Type);
        Assert.Equal(TokenType.LeftParen, tokens[1].Type);
        Assert.Equal(TokenType.Const, tokens[2].Type);
        Assert.Equal(TokenType.Identifier, tokens[3].Type);
        Assert.Equal("element", tokens[3].Value);
        Assert.Equal(TokenType.Of, tokens[4].Type);
        Assert.Equal(TokenType.Identifier, tokens[5].Type);
        Assert.Equal("elements", tokens[5].Value);
        Assert.Equal(TokenType.RightParen, tokens[6].Type);
    }
    
    [Fact]
    public void TestInKeywordStandalone()
    {
        // Arrange
        var code = "in";
        var lexer = new ECEngine.Lexer.Lexer(code);
        
        // Act
        var tokens = lexer.Tokenize();
        
        // Assert
        Assert.Equal(TokenType.In, tokens[0].Type);
        Assert.Equal("in", tokens[0].Value);
    }
    
    [Fact]
    public void TestOfKeywordStandalone()
    {
        // Arrange
        var code = "of";
        var lexer = new ECEngine.Lexer.Lexer(code);
        
        // Act
        var tokens = lexer.Tokenize();
        
        // Assert
        Assert.Equal(TokenType.Of, tokens[0].Type);
        Assert.Equal("of", tokens[0].Value);
    }
    
    [Fact]
    public void TestInOfAsIdentifiers()
    {
        // Arrange - when 'in' and 'of' are used in different contexts, they should still be keywords
        var code = "var inVar = 5; var ofVar = 10;";
        var lexer = new ECEngine.Lexer.Lexer(code);
        
        // Act
        var tokens = lexer.Tokenize();
        
        // Assert
        Assert.Equal(TokenType.Var, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal("inVar", tokens[1].Value); // inVar is identifier, not 'in' keyword
        Assert.Equal(TokenType.Assign, tokens[2].Type);
        Assert.Equal(TokenType.Number, tokens[3].Type);
        Assert.Equal("5", tokens[3].Value);
    }
    
    [Fact]
    public void TestComplexForInOfTokenization()
    {
        // Arrange
        var code = @"
            for (let key in obj) {
                for (const item of arr) {
                    console.log(key, item);
                }
            }";
        var lexer = new ECEngine.Lexer.Lexer(code);
        
        // Act
        var tokens = lexer.Tokenize();
        
        // Assert - Just verify key tokens are present
        var tokenTypes = tokens.Select(t => t.Type).ToList();
        Assert.Contains(TokenType.For, tokenTypes);
        Assert.Contains(TokenType.Let, tokenTypes);
        Assert.Contains(TokenType.In, tokenTypes);
        Assert.Contains(TokenType.Const, tokenTypes);
        Assert.Contains(TokenType.Of, tokenTypes);
    }
}
