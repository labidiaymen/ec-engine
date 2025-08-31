using Xunit;
using ECEngine.AST;
using ECEngine.Runtime;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Interpreter;

[Collection("Console Tests")]
public class TemplateLiteralEvaluationTests
{
    [Fact]
    public void Test_SimpleTemplate_EvaluatesToString()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = "`Hello World`";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Test_TemplateWithVariableInterpolation_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = @"
            var name = 'John';
            `Hello ${name}!`
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Hello John!", result);
    }

    [Fact]
    public void Test_TemplateWithMultipleVariables_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = @"
            var firstName = 'John';
            var lastName = 'Doe';
            `Hello ${firstName} ${lastName}!`
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Hello John Doe!", result);
    }

    [Fact]
    public void Test_TemplateWithNumberInterpolation_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = @"
            var age = 25;
            `I am ${age} years old`
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("I am 25 years old", result);
    }

    [Fact]
    public void Test_TemplateWithBooleanInterpolation_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = @"
            var isActive = true;
            `Status: ${isActive}`
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Status: true", result);
    }

    [Fact]
    public void Test_TemplateWithExpressionInterpolation_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = @"
            var x = 10;
            var y = 5;
            `Result: ${x + y * 2}`
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Result: 20", result);
    }

    [Fact]
    public void Test_TemplateWithNullInterpolation_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = @"
            var value = null;
            `Value: ${value}`
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Value: null", result);
    }

    [Fact]
    public void Test_TemplateWithStringLiteralInterpolation_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = @"`Greeting: ${""Hello World""}`";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Greeting: Hello World", result);
    }

    [Fact]
    public void Test_EmptyTemplate_EvaluatesToEmptyString()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = "``";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void Test_TemplateWithEscapeSequences_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = @"`Line 1\nLine 2\tTabbed`";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Line 1\nLine 2\tTabbed", result);
    }

    [Fact]
    public void Test_TemplateAssignedToVariable_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = @"
            var name = 'Alice';
            var greeting = `Hello ${name}!`;
            greeting
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Hello Alice!", result);
    }

    [Fact]
    public void Test_TemplateInConsoleLog_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = @"
            var name = 'Bob';
            console.log(`Welcome ${name}!`)
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert - console.log returns null but should have printed the message
        Assert.Null(result);
    }

    [Fact]
    public void Test_TemplateWithComparisonExpression_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = @"
            var x = 10;
            var y = 5;
            `Is x greater? ${x > y}`
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Is x greater? true", result);
    }

    [Fact]
    public void Test_TemplateWithFunctionCallInterpolation_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = @"
            function getName() {
                return 'Charlie';
            }
            `Hello ${getName()}!`
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Hello Charlie!", result);
    }

    [Fact]
    public void Test_NestedTemplatesInExpression_EvaluatesCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var code = @"
            var name = 'David';
            var greeting = `Hello ${name}!`;
            `Message: ${greeting}`
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Message: Hello David!", result);
    }
}
