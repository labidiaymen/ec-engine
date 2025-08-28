using Xunit;
using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.Runtime;

namespace ECEngine.Tests.Integration;

public class BasicExecutionTests
{
    [Fact]
    public void EndToEnd_SimpleNumber_ExecutesCorrectly()
    {
        // Arrange
        string code = "42";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.Equal(42.0, result);
    }

    [Fact]
    public void EndToEnd_SimpleAddition_ExecutesCorrectly()
    {
        // Arrange
        string code = "1 + 2";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.Equal(3.0, result);
    }

    [Fact]
    public void EndToEnd_ComplexArithmetic_ExecutesCorrectly()
    {
        // Arrange
        string code = "10 * 5 + 3";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.Equal(53.0, result); // 10 * 5 = 50, then 50 + 3 = 53
    }

    [Fact]
    public void EndToEnd_ParenthesesExpression_ExecutesCorrectly()
    {
        // Arrange
        string code = "(1 + 2) * 3";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.Equal(9.0, result); // (1 + 2) = 3, then 3 * 3 = 9
    }

    private static object? ExecuteCode(string code)
    {
        var lexer = new ECEngine.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();
        
        var parser = new ECEngine.Parser.Parser();
        var ast = parser.Parse(code);
        
        var interpreter = new ECEngine.Runtime.Interpreter();
        return interpreter.Evaluate(ast, code);
    }
}
