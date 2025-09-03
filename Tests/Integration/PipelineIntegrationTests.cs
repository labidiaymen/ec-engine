using Xunit;
using ECEngine.Parser;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Integration;

public class PipelineIntegrationTests
{
    [Fact]
    public void Execute_SimplePipeline_ProducesCorrectResult()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new RuntimeInterpreter();
        
        var code = @"
            function double(x) {
                return x * 2;
            }
            
            var result = 5 |> double;
            result;
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal(10.0, result);
    }

    [Fact]
    public void Execute_ChainedPipeline_ProducesCorrectResult()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new RuntimeInterpreter();
        
        var code = @"
            function double(x) {
                return x * 2;
            }
            
            function add10(x) {
                return x + 10;
            }
            
            function square(x) {
                return x * x;
            }
            
            var result = 5 |> double |> add10 |> square;
            result;
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert - (5 * 2 + 10)^2 = 20^2 = 400
        Assert.Equal(400.0, result);
    }

    [Fact]
    public void Execute_PipelineWithParameters_ProducesCorrectResult()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new RuntimeInterpreter();
        
        var code = @"
            function add(x, y) {
                return x + y;
            }
            
            function multiply(x, factor) {
                return x * factor;
            }
            
            var result = 5 |> add(3) |> multiply(2);
            result;
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert - ((5 + 3) * 2) = 16
        Assert.Equal(16.0, result);
    }

    [Fact]
    public void Execute_MultilinePipeline_ProducesCorrectResult()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new RuntimeInterpreter();
        
        var code = @"
            function double(x) {
                return x * 2;
            }
            
            function add10(x) {
                return x + 10;
            }
            
            var result = 5
                |> double
                |> add10;
            result;
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert - (5 * 2) + 10 = 20
        Assert.Equal(20.0, result);
    }

    [Fact]
    public void Execute_PipelineWithStringFunctions_WorksCorrectly()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new RuntimeInterpreter();
        
        var code = @"
            function addExclamation(str) {
                return str + '!';
            }
            
            function capitalize(str) {
                return str.charAt(0).toUpperCase() + str.slice(1);
            }
            
            var result = 'hello world' |> capitalize |> addExclamation;
            result;
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Hello world!", result);
    }

    [Fact]
    public void Execute_PipelineWithMixedTypes_HandlesCorrectly()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new RuntimeInterpreter();
        
        var code = @"
            function toString(x) {
                return '' + x;
            }
            
            function addPrefix(str, prefix) {
                return prefix + str;
            }
            
            var result = 42 |> toString |> addPrefix('Number: ');
            result;
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal("Number: 42", result);
    }

    [Fact]
    public void Execute_PipelineWithComplexExpressions_EvaluatesCorrectly()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new RuntimeInterpreter();
        
        var code = @"
            function double(x) {
                return x * 2;
            }
            
            function add(x, y) {
                return x + y;
            }
            
            var base = 3;
            var result = (base + 2) |> double |> add(base * 2);
            result;
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert - ((3 + 2) * 2) + (3 * 2) = (5 * 2) + 6 = 10 + 6 = 16
        Assert.Equal(16.0, result);
    }

    [Fact]
    public void Execute_PipelineVsTraditional_ProducesSameResult()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new RuntimeInterpreter();
        
        var code = @"
            function double(x) {
                return x * 2;
            }
            
            function add10(x) {
                return x + 10;
            }
            
            function square(x) {
                return x * x;
            }
            
            // Traditional nested calls
            var traditional = square(add10(double(5)));
            
            // Pipeline version
            var pipeline = 5 |> double |> add10 |> square;
            
            // Return both for comparison
            [traditional, pipeline];
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        var resultArray = Assert.IsType<List<object>>(result);
        Assert.Equal(2, resultArray.Count);
        Assert.Equal(resultArray[0], resultArray[1]); // Both should be equal
        Assert.Equal(400.0, resultArray[0]); // (5 * 2 + 10)^2 = 400
        Assert.Equal(400.0, resultArray[1]);
    }

    [Fact] 
    public void Execute_PipelineInVariableAssignments_WorksCorrectly()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new RuntimeInterpreter();
        
        var code = @"
            function double(x) {
                return x * 2;
            }
            
            function add(x, y) {
                return x + y;
            }
            
            var step1 = 5 |> double;
            var step2 = step1 |> add(10);
            var step3 = step2 |> double;
            
            [step1, step2, step3];
        ";

        // Act
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        var resultArray = Assert.IsType<List<object>>(result);
        Assert.Equal(3, resultArray.Count);
        Assert.Equal(10.0, resultArray[0]); // 5 * 2 = 10
        Assert.Equal(20.0, resultArray[1]); // 10 + 10 = 20  
        Assert.Equal(40.0, resultArray[2]); // 20 * 2 = 40
    }
}
