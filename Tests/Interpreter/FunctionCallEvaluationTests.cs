using Xunit;
using ECEngine.AST;
using ECEngine.Runtime;
using System.IO;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Interpreter;

public class FunctionCallEvaluationTests
{
    [Fact]
    public void Evaluate_ConsoleLogWithNumber_ReturnsNull()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var consoleIdentifier = new Identifier("console");
        var memberExpression = new MemberExpression(consoleIdentifier, "log");
        var argument = new NumberLiteral(42);
        var callExpression = new CallExpression(memberExpression, new List<Expression> { argument });
        
        // Capture console output
        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        
        try
        {
            // Act
            var result = interpreter.Evaluate(callExpression, "console.log(42)");
            
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
    public void Evaluate_ConsoleLogWithExpression_ReturnsNull()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var consoleIdentifier = new Identifier("console");
        var memberExpression = new MemberExpression(consoleIdentifier, "log");
        var binaryExpression = new BinaryExpression(new NumberLiteral(1), "+", new NumberLiteral(2));
        var callExpression = new CallExpression(memberExpression, new List<Expression> { binaryExpression });
        
        // Capture console output
        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        
        try
        {
            // Act
            var result = interpreter.Evaluate(callExpression, "console.log(1 + 2)");
            
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
    public void Evaluate_MemberExpression_ReturnsFunction()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var consoleIdentifier = new Identifier("console");
        var memberExpression = new MemberExpression(consoleIdentifier, "log");
        
        // Act
        var result = interpreter.Evaluate(memberExpression, "console.log");
        
        // Assert
        Assert.IsType<ConsoleLogFunction>(result);
    }
}
