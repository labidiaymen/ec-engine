using Xunit;
using ECEngine.Lexer;

namespace ECEngine.Tests.Lexer;

[Collection("Console Tests")]
public class TemplateLiteralTokenizationTests
{
    [Fact]
    public void Test_SimplePlaceholderTemplate_ReturnsTemplateLiteralToken()
    {
        // Arrange
        var code = "`Hello World`";
        var lexer = new ECEngine.Lexer.Lexer(code);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count); // Template + EOF
        Assert.Equal(TokenType.TemplateLiteral, tokens[0].Type);
        Assert.Equal("Hello World", tokens[0].Value);
    }

    [Fact]
    public void Test_TemplateWithSingleInterpolation_ReturnsCorrectTokens()
    {
        // Arrange
        var code = "`Hello ${name}!`";
        var lexer = new ECEngine.Lexer.Lexer(code);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(4, tokens.Count); // TemplateStart + TemplateExpression + TemplateEnd + EOF
        Assert.Equal(TokenType.TemplateStart, tokens[0].Type);
        Assert.Equal("Hello ", tokens[0].Value);
        Assert.Equal(TokenType.TemplateExpression, tokens[1].Type);
        Assert.Equal("name", tokens[1].Value);
        Assert.Equal(TokenType.TemplateEnd, tokens[2].Type);
        Assert.Equal("!", tokens[2].Value);
    }

    [Fact]
    public void Test_TemplateWithMultipleInterpolations_ReturnsCorrectTokens()
    {
        // Arrange
        var code = "`Hello ${firstName} ${lastName}!`";
        var lexer = new ECEngine.Lexer.Lexer(code);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(6, tokens.Count); // TemplateStart + Expression + TemplateMiddle + Expression + TemplateEnd + EOF
        Assert.Equal(TokenType.TemplateStart, tokens[0].Type);
        Assert.Equal("Hello ", tokens[0].Value);
        Assert.Equal(TokenType.TemplateExpression, tokens[1].Type);
        Assert.Equal("firstName", tokens[1].Value);
        Assert.Equal(TokenType.TemplateMiddle, tokens[2].Type);
        Assert.Equal(" ", tokens[2].Value);
        Assert.Equal(TokenType.TemplateExpression, tokens[3].Type);
        Assert.Equal("lastName", tokens[3].Value);
        Assert.Equal(TokenType.TemplateEnd, tokens[4].Type);
        Assert.Equal("!", tokens[4].Value);
    }

    [Fact]
    public void Test_TemplateWithComplexExpression_ReturnsCorrectTokens()
    {
        // Arrange
        var code = "`Result: ${x + y * 2}`";
        var lexer = new ECEngine.Lexer.Lexer(code);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(4, tokens.Count); // TemplateStart + TemplateExpression + TemplateEnd + EOF
        Assert.Equal(TokenType.TemplateStart, tokens[0].Type);
        Assert.Equal("Result: ", tokens[0].Value);
        Assert.Equal(TokenType.TemplateExpression, tokens[1].Type);
        Assert.Equal("x + y * 2", tokens[1].Value);
        Assert.Equal(TokenType.TemplateEnd, tokens[2].Type);
        Assert.Equal("", tokens[2].Value);
        Assert.Equal(TokenType.EOF, tokens[3].Type);
    }

    [Fact]
    public void Test_TemplateWithEscapeSequences_ReturnsCorrectTokens()
    {
        // Arrange
        var code = "`Line 1\\nLine 2\\t${value}\\``";
        var lexer = new ECEngine.Lexer.Lexer(code);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(4, tokens.Count);
        Assert.Equal(TokenType.TemplateStart, tokens[0].Type);
        Assert.Equal("Line 1\nLine 2\t", tokens[0].Value);
        Assert.Equal(TokenType.TemplateExpression, tokens[1].Type);
        Assert.Equal("value", tokens[1].Value);
        Assert.Equal(TokenType.TemplateEnd, tokens[2].Type);
        Assert.Equal("`", tokens[2].Value);
    }

    [Fact]
    public void Test_TemplateWithNestedBraces_ReturnsCorrectTokens()
    {
        // Arrange
        var code = "`Object: ${obj.getValue()}`";
        var lexer = new ECEngine.Lexer.Lexer(code);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(4, tokens.Count); // TemplateStart + TemplateExpression + TemplateEnd + EOF
        Assert.Equal(TokenType.TemplateStart, tokens[0].Type);
        Assert.Equal("Object: ", tokens[0].Value);
        Assert.Equal(TokenType.TemplateExpression, tokens[1].Type);
        Assert.Equal("obj.getValue()", tokens[1].Value);
        Assert.Equal(TokenType.TemplateEnd, tokens[2].Type);
        Assert.Equal("", tokens[2].Value);
        Assert.Equal(TokenType.EOF, tokens[3].Type);
    }

    [Fact]
    public void Test_EmptyTemplate_ReturnsTemplateLiteralToken()
    {
        // Arrange
        var code = "``";
        var lexer = new ECEngine.Lexer.Lexer(code);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.TemplateLiteral, tokens[0].Type);
        Assert.Equal("", tokens[0].Value);
    }

    [Fact]
    public void Test_TemplateWithEmptyInterpolation_ReturnsCorrectTokens()
    {
        // Arrange
        var code = "`Before${} After`";
        var lexer = new ECEngine.Lexer.Lexer(code);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(4, tokens.Count);
        Assert.Equal(TokenType.TemplateStart, tokens[0].Type);
        Assert.Equal("Before", tokens[0].Value);
        Assert.Equal(TokenType.TemplateExpression, tokens[1].Type);
        Assert.Equal("", tokens[1].Value);
        Assert.Equal(TokenType.TemplateEnd, tokens[2].Type);
        Assert.Equal(" After", tokens[2].Value);
    }

    [Fact]
    public void Test_UnterminatedTemplate_ThrowsException()
    {
        // Arrange
        var code = "`Hello ${name";
        var lexer = new ECEngine.Lexer.Lexer(code);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => lexer.Tokenize());
        Assert.Contains("Unterminated template", exception.Message);
    }

    [Fact]
    public void Test_UnterminatedTemplateExpression_ThrowsException()
    {
        // Arrange
        var code = "`Hello ${name`";
        var lexer = new ECEngine.Lexer.Lexer(code);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => lexer.Tokenize());
        Assert.Contains("Unterminated template", exception.Message);
    }
}
