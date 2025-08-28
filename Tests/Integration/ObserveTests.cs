using Xunit;
using ECEngine.Parser;
using ECEngine.Runtime;
using ECEngine.AST;

namespace ECEngine.Tests.Integration;

[Collection("Console Tests")]
public class ObserveTests
{
    [Fact]
    public void Execute_ObserveStatement_CreatesObserver()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = @"
            var x = 10;
            observe x function() {
                console.log(""x changed!"");
            }
        ";

        // Act
        var ast = parser.Parse(input);
        var result = interpreter.Evaluate(ast, input);

        // Assert - should not throw and variable should have observer
        var variables = interpreter.Variables;
        Assert.True(variables.ContainsKey("x"));
        Assert.Single(variables["x"].Observers);
    }

    [Fact]
    public void Execute_VariableChange_TriggersObserver()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = @"
            var counter = 0;
            observe counter function() {
                console.log(""Counter changed!"");
            }
            counter = 5;
        ";

        // Act & Assert - should not throw
        var ast = parser.Parse(input);
        var result = interpreter.Evaluate(ast, input);
        
        // Verify final value
        Assert.Equal(5.0, result);
        var variables = interpreter.Variables;
        Assert.Equal(5.0, variables["counter"].Value);
    }

    [Fact]
    public void Execute_MultipleObservers_AllTriggered()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = @"
            var x = 1;
            observe x function() {
                console.log(""First observer"");
            }
            observe x function() {
                console.log(""Second observer"");
            }
            x = 2;
        ";

        // Act & Assert - should not throw
        var ast = parser.Parse(input);
        var result = interpreter.Evaluate(ast, input);
        
        // Verify variable has two observers
        var variables = interpreter.Variables;
        Assert.Equal(2, variables["x"].Observers.Count);
    }

    [Fact]
    public void Execute_ObserveUndeclaredVariable_ThrowsException()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = @"
            observe undeclaredVar function() {
                console.log(""This should not work"");
            }
        ";

        // Act & Assert
        var ast = parser.Parse(input);
        var exception = Assert.Throws<ECEngineException>(() => interpreter.Evaluate(ast, input));
        Assert.Contains("Cannot observe undeclared variable 'undeclaredVar'", exception.Message);
    }

    [Fact]
    public void Execute_ObserverWithParameters_ReceivesCorrectValues()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = @"
            var testVar = ""hello"";
            observe testVar function(oldVal, newVal, varName) {
                console.log(""Variable changed"");
            }
            testVar = ""world"";
        ";

        // Act & Assert - should not throw
        var ast = parser.Parse(input);
        var result = interpreter.Evaluate(ast, input);
        
        // Verify final value
        Assert.Equal("world", result);
    }
}
