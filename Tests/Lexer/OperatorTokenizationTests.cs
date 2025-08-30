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

    #region Strict Comparison Operators

    [Fact]
    public void Tokenize_StrictEqualOperator_ReturnsStrictEqualToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("===");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.StrictEqual, tokens[0].Type);
        Assert.Equal("===", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_StrictNotEqualOperator_ReturnsStrictNotEqualToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("!==");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.StrictNotEqual, tokens[0].Type);
        Assert.Equal("!==", tokens[0].Value);
    }

    #endregion

    #region Compound Assignment Operators

    [Fact]
    public void Tokenize_PlusAssignOperator_ReturnsPlusAssignToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("+=");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.PlusAssign, tokens[0].Type);
        Assert.Equal("+=", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_MinusAssignOperator_ReturnsMinusAssignToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("-=");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.MinusAssign, tokens[0].Type);
        Assert.Equal("-=", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_MultiplyAssignOperator_ReturnsMultiplyAssignToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("*=");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.MultiplyAssign, tokens[0].Type);
        Assert.Equal("*=", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_DivideAssignOperator_ReturnsDivideAssignToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("/=");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.DivideAssign, tokens[0].Type);
        Assert.Equal("/=", tokens[0].Value);
    }

    #endregion

    #region Bitwise Operators

    [Fact]
    public void Tokenize_BitwiseAndOperator_ReturnsBitwiseAndToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("&");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.BitwiseAnd, tokens[0].Type);
        Assert.Equal("&", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_BitwiseOrOperator_ReturnsBitwiseOrToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("|");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.BitwiseOr, tokens[0].Type);
        Assert.Equal("|", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_BitwiseXorOperator_ReturnsBitwiseXorToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("^");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.BitwiseXor, tokens[0].Type);
        Assert.Equal("^", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_BitwiseNotOperator_ReturnsBitwiseNotToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("~");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.BitwiseNot, tokens[0].Type);
        Assert.Equal("~", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_LeftShiftOperator_ReturnsLeftShiftToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("<<");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.LeftShift, tokens[0].Type);
        Assert.Equal("<<", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_RightShiftOperator_ReturnsRightShiftToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer(">>");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.RightShift, tokens[0].Type);
        Assert.Equal(">>", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_UnsignedRightShiftOperator_ReturnsUnsignedRightShiftToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer(">>>");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.UnsignedRightShift, tokens[0].Type);
        Assert.Equal(">>>", tokens[0].Value);
    }

    #endregion

    #region Ternary Operator

    [Fact]
    public void Tokenize_QuestionMarkOperator_ReturnsQuestionToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("?");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Question, tokens[0].Type);
        Assert.Equal("?", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_ColonOperator_ReturnsColonToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer(":");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Colon, tokens[0].Type);
        Assert.Equal(":", tokens[0].Value);
    }

    #endregion

    #region Complex Operator Combinations

    [Theory]
    [InlineData("5 === 5", new[] { TokenType.Number, TokenType.StrictEqual, TokenType.Number, TokenType.EOF })]
    [InlineData("x += 5", new[] { TokenType.Identifier, TokenType.PlusAssign, TokenType.Number, TokenType.EOF })]
    [InlineData("a & b", new[] { TokenType.Identifier, TokenType.BitwiseAnd, TokenType.Identifier, TokenType.EOF })]
    [InlineData("x << 2", new[] { TokenType.Identifier, TokenType.LeftShift, TokenType.Number, TokenType.EOF })]
    [InlineData("x ? y : z", new[] { TokenType.Identifier, TokenType.Question, TokenType.Identifier, TokenType.Colon, TokenType.Identifier, TokenType.EOF })]
    public void Tokenize_ComplexOperatorExpressions_ReturnsCorrectTokenSequence(string input, TokenType[] expectedTypes)
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer(input);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(expectedTypes.Length, tokens.Count);
        for (int i = 0; i < expectedTypes.Length; i++)
        {
            Assert.Equal(expectedTypes[i], tokens[i].Type);
        }
    }

    #endregion
}
