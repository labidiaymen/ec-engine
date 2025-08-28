using Xunit;
using ECEngine.Lexer;

namespace ECEngine.Tests.Lexer;

public class VariableTokenizationTests
{
    [Fact]
    public void Tokenize_VarKeyword_ProducesVarToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("var");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count); // var + EOF
        Assert.Equal(TokenType.Var, tokens[0].Type);
        Assert.Equal("var", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_LetKeyword_ProducesLetToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("let");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count); // let + EOF
        Assert.Equal(TokenType.Let, tokens[0].Type);
        Assert.Equal("let", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_ConstKeyword_ProducesConstToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("const");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count); // const + EOF
        Assert.Equal(TokenType.Const, tokens[0].Type);
        Assert.Equal("const", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_AssignmentOperator_ProducesAssignToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("=");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count); // = + EOF
        Assert.Equal(TokenType.Assign, tokens[0].Type);
        Assert.Equal("=", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_VariableDeclaration_ProducesCorrectTokens()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("var x = 42;");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(6, tokens.Count); // var + x + = + 42 + ; + EOF
        
        Assert.Equal(TokenType.Var, tokens[0].Type);
        Assert.Equal("var", tokens[0].Value);
        
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal("x", tokens[1].Value);
        
        Assert.Equal(TokenType.Assign, tokens[2].Type);
        Assert.Equal("=", tokens[2].Value);
        
        Assert.Equal(TokenType.Number, tokens[3].Type);
        Assert.Equal("42", tokens[3].Value);
        
        Assert.Equal(TokenType.Semicolon, tokens[4].Type);
        Assert.Equal(";", tokens[4].Value);
        
        Assert.Equal(TokenType.EOF, tokens[5].Type);
    }

    [Fact]
    public void Tokenize_Assignment_ProducesCorrectTokens()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("x = 100");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(4, tokens.Count); // x + = + 100 + EOF
        
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal("x", tokens[0].Value);
        
        Assert.Equal(TokenType.Assign, tokens[1].Type);
        Assert.Equal("=", tokens[1].Value);
        
        Assert.Equal(TokenType.Number, tokens[2].Type);
        Assert.Equal("100", tokens[2].Value);
        
        Assert.Equal(TokenType.EOF, tokens[3].Type);
    }

    [Fact]
    public void Tokenize_MultipleVariableDeclarations_ProducesCorrectTokens()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("let a = 1; const b = 2;");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(11, tokens.Count); // let + a + = + 1 + ; + const + b + = + 2 + ; + EOF
        
        Assert.Equal(TokenType.Let, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal(TokenType.Assign, tokens[2].Type);
        Assert.Equal(TokenType.Number, tokens[3].Type);
        Assert.Equal(TokenType.Semicolon, tokens[4].Type);
        Assert.Equal(TokenType.Const, tokens[5].Type);
        Assert.Equal(TokenType.Identifier, tokens[6].Type);
        Assert.Equal(TokenType.Assign, tokens[7].Type);
        Assert.Equal(TokenType.Number, tokens[8].Type);
        Assert.Equal(TokenType.Semicolon, tokens[9].Type);
        Assert.Equal(TokenType.EOF, tokens[10].Type);
    }

    [Fact]
    public void Tokenize_VariableWithUnderscore_ProducesIdentifierToken()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("var my_var = 5;");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(6, tokens.Count); // var + my_var + = + 5 + ; + EOF
        
        Assert.Equal(TokenType.Var, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal("my_var", tokens[1].Value);
    }
}
