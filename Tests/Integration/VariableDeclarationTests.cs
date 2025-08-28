using Xunit;
using ECEngine.Parser;
using ECEngine.Runtime;
using ECEngine.AST;

namespace ECEngine.Tests.Integration;

[Collection("Console Tests")]
public class VariableDeclarationTests
{
    [Fact]
    public void Execute_VarDeclarationWithInitializer_SetsVariableValue()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = "var x = 42;";

        // Act
        var ast = parser.Parse(input);
        var result = interpreter.Evaluate(ast, input);

        // Assert
        Assert.Equal(42.0, result);
    }

    [Fact]
    public void Execute_VarDeclarationWithoutInitializer_SetsVariableToNull()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = "var x;";

        // Act
        var ast = parser.Parse(input);
        var result = interpreter.Evaluate(ast, input);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Execute_LetDeclarationWithInitializer_SetsVariableValue()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = "let y = 100;";

        // Act
        var ast = parser.Parse(input);
        var result = interpreter.Evaluate(ast, input);

        // Assert
        Assert.Equal(100.0, result);
    }

    [Fact]
    public void Execute_ConstDeclarationWithInitializer_SetsVariableValue()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = "const z = 3.14;";

        // Act
        var ast = parser.Parse(input);
        var result = interpreter.Evaluate(ast, input);

        // Assert
        Assert.Equal(3.14, result);
    }

    [Fact]
    public void Execute_VariableReference_ReturnsValue()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = "var x = 42; x;";

        // Act
        var ast = parser.Parse(input);
        var result = interpreter.Evaluate(ast, input);

        // Assert
        Assert.Equal(42.0, result);
    }

    [Fact]
    public void Execute_AssignmentExpression_UpdatesVariableValue()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = "var x = 10; x = 20;";

        // Act
        var ast = parser.Parse(input);
        var result = interpreter.Evaluate(ast, input);

        // Assert
        Assert.Equal(20.0, result);
    }

    [Fact]
    public void Execute_ComplexExpression_WithVariables_EvaluatesCorrectly()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = "var a = 5; var b = 10; a + b * 2;";

        // Act
        var ast = parser.Parse(input);
        var result = interpreter.Evaluate(ast, input);

        // Assert
        Assert.Equal(25.0, result); // 5 + (10 * 2) = 25
    }

    [Fact]
    public void Execute_UndeclaredVariable_ThrowsException()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = "x;";

        // Act & Assert
        var ast = parser.Parse(input);
        var exception = Assert.Throws<ECEngineException>(() => interpreter.Evaluate(ast, input));
        Assert.Contains("Unknown identifier: x", exception.Message);
    }

    [Fact]
    public void Execute_AssignmentToUndeclaredVariable_ThrowsException()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = "x = 42;";

        // Act & Assert
        var ast = parser.Parse(input);
        var exception = Assert.Throws<ECEngineException>(() => interpreter.Evaluate(ast, input));
        Assert.Contains("Variable 'x' not declared", exception.Message);
    }

    [Fact]
    public void Execute_RedeclareVariable_ThrowsException()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var input = "var x = 5; var x = 10;";

        // Act & Assert
        var ast = parser.Parse(input);
        var exception = Assert.Throws<ECEngineException>(() => interpreter.Evaluate(ast, input));
        Assert.Contains("Variable 'x' already declared", exception.Message);
    }
}
