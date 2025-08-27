using Xunit;
using ECEngine.AST;
using ECEngine.Runtime;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Interpreter;

public class IdentifierEvaluationTests
{
    [Fact]
    public void Evaluate_ConsoleIdentifier_ReturnsConsoleObject()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var identifier = new Identifier("console");
        
        // Act
        var result = interpreter.Evaluate(identifier, "console");
        
        // Assert
        Assert.IsType<ConsoleObject>(result);
    }

    [Fact]
    public void Evaluate_UnknownIdentifier_ThrowsECEngineException()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var identifier = new Identifier("unknown_variable");
        
        // Act & Assert
        var exception = Assert.Throws<ECEngineException>(() => 
            interpreter.Evaluate(identifier, "unknown_variable"));
        
        Assert.Contains("Unknown identifier: unknown_variable", exception.Message);
        Assert.Equal(1, exception.Line);
        Assert.Equal(1, exception.Column);
    }

    [Fact]
    public void Evaluate_UndefinedVariable_ThrowsECEngineException()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var identifier = new Identifier("myVariable");
        
        // Act & Assert
        var exception = Assert.Throws<ECEngineException>(() => 
            interpreter.Evaluate(identifier, "myVariable"));
        
        Assert.Contains("Unknown identifier: myVariable", exception.Message);
        Assert.Contains("not defined in the current scope", exception.ContextInfo);
    }
}
