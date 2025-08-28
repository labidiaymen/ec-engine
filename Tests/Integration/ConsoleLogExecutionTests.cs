using Xunit;
using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.Runtime;
using System.IO;

namespace ECEngine.Tests.Integration;

public class ConsoleLogExecutionTests
{
    [Fact]
    public void EndToEnd_ConsoleLogNumber_ExecutesWithoutError()
    {
        // Arrange
        string code = "console.log(42)";

        // Capture console output
        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            var result = ExecuteCode(code);

            // Assert
            Assert.Null(result); // console.log returns undefined (null)
            var output = stringWriter.ToString().Trim();
            Assert.Equal("42", output);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    [Fact]
    public void EndToEnd_ConsoleLogExpression_ExecutesCorrectly()
    {
        // Arrange
        string code = "console.log(1 + 2)";

        // Capture console output
        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            var result = ExecuteCode(code);

            // Assert
            Assert.Null(result);
            var output = stringWriter.ToString().Trim();
            Assert.Equal("3", output);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    [Fact]
    public void EndToEnd_ConsoleLogComplexExpression_ExecutesCorrectly()
    {
        // Arrange
        string code = "console.log(10 * 5 + 3)";

        // Capture console output
        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            var result = ExecuteCode(code);

            // Assert
            Assert.Null(result);
            var output = stringWriter.ToString().Trim();
            Assert.Equal("53", output);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
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
