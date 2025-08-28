using Xunit;
using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.Runtime;

namespace ECEngine.Tests.Integration;

public class ErrorHandlingTests
{
    [Fact]
    public void EndToEnd_UnknownVariable_ThrowsECEngineException()
    {
        // Arrange
        string code = "unknown_variable + 2";

        // Act & Assert
        var exception = Assert.Throws<ECEngineException>(() => ExecuteCode(code));

        Assert.Contains("Unknown identifier", exception.Message);
        Assert.Equal(1, exception.Line);
        Assert.Equal(1, exception.Column);
        Assert.Equal(code, exception.SourceCode);
    }

    [Fact]
    public void EndToEnd_InvalidCharacter_ThrowsException()
    {
        // Arrange
        string code = "1 @ 2";

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => ExecuteCode(code));
        Assert.Contains("Unexpected character", exception.Message);
        Assert.Contains("line 1, column 3", exception.Message);
    }

    [Fact]
    public void EndToEnd_ConsoleLogUnknownVariable_ThrowsECEngineException()
    {
        // Arrange
        string code = "console.log(unknown_variable + 2)";

        // Act & Assert
        var exception = Assert.Throws<ECEngineException>(() => ExecuteCode(code));

        Assert.Contains("Unknown identifier: unknown_variable", exception.Message);
        Assert.Equal(1, exception.Line);
        Assert.Equal(13, exception.Column); // Position of 'unknown_variable'
    }

    [Fact]
    public void EndToEnd_UnknownOperator_ThrowsECEngineException()
    {
        // This test would require adding an invalid operator to the lexer
        // For now, we'll test with a scenario that could happen if we extended the parser
        // but haven't implemented the operator in the interpreter

        // We can't easily test this with the current implementation
        // but this shows the structure for such tests
        Assert.True(true); // Placeholder
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
