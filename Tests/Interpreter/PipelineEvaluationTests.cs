using Xunit;
using ECEngine.AST;
using ECEngine.Runtime;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Interpreter;

public class PipelineEvaluationTests
{
    [Fact]
    public void Evaluate_SimplePipelineWithFunction_CallsFunctionWithPipedValue()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // Define a simple function: function double(x) { return x * 2; }
        var param = new Identifier("x");
        var body = new BinaryExpression(param, "*", new NumberLiteral(2));
        var returnStmt = new ReturnStatement(body);
        var doubleFunc = new FunctionDeclaration("double", new List<string> { "x" }, new List<Statement> { returnStmt });
        
        // Create pipeline: 5 |> double
        var pipelineValue = new NumberLiteral(5);
        var functionRef = new Identifier("double");
        var pipeline = new PipelineExpression(pipelineValue, functionRef);

        // Act
        interpreter.Evaluate(doubleFunc, "function double(x) { return x * 2; }");
        var result = interpreter.Evaluate(pipeline, "5 |> double");

        // Assert
        Assert.Equal(10.0, result);
    }

    [Fact]
    public void Evaluate_PipelineWithFunctionCall_InjectsPipedValueAsFirstArgument()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // Define function: function add(x, y) { return x + y; }
        var paramX = new Identifier("x");
        var paramY = new Identifier("y");
        var body = new BinaryExpression(paramX, "+", paramY);
        var returnStmt = new ReturnStatement(body);
        var addFunc = new FunctionDeclaration("add", new List<string> { "x", "y" }, new List<Statement> { returnStmt });
        
        // Create pipeline: 5 |> add(3) - should become add(5, 3)
        var pipelineValue = new NumberLiteral(5);
        var functionCall = new CallExpression(new Identifier("add"), new List<Expression> { new NumberLiteral(3) });
        var pipeline = new PipelineExpression(pipelineValue, functionCall);

        // Act
        interpreter.Evaluate(addFunc, "function add(x, y) { return x + y; }");
        var result = interpreter.Evaluate(pipeline, "5 |> add(3)");

        // Assert
        Assert.Equal(8.0, result);
    }

    [Fact]
    public void Evaluate_ChainedPipeline_ExecutesInCorrectOrder()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // Define functions
        var doubleParam = new Identifier("x");
        var doubleBody = new BinaryExpression(doubleParam, "*", new NumberLiteral(2));
        var doubleReturn = new ReturnStatement(doubleBody);
        var doubleFunc = new FunctionDeclaration("double", new List<string> { "x" }, new List<Statement> { doubleReturn });
        
        var add10Param = new Identifier("x");
        var add10Body = new BinaryExpression(add10Param, "+", new NumberLiteral(10));
        var add10Return = new ReturnStatement(add10Body);
        var add10Func = new FunctionDeclaration("add10", new List<string> { "x" }, new List<Statement> { add10Return });
        
        // Create chained pipeline: 5 |> double |> add10
        var innerPipeline = new PipelineExpression(new NumberLiteral(5), new Identifier("double"));
        var outerPipeline = new PipelineExpression(innerPipeline, new Identifier("add10"));

        // Act
        interpreter.Evaluate(doubleFunc, "function double(x) { return x * 2; }");
        interpreter.Evaluate(add10Func, "function add10(x) { return x + 10; }");
        var result = interpreter.Evaluate(outerPipeline, "5 |> double |> add10");

        // Assert - (5 * 2) + 10 = 20
        Assert.Equal(20.0, result);
    }

    [Fact]
    public void Evaluate_PipelineWithMultipleParameters_InjectsValueCorrectly()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // Define function: function multiply(x, factor) { return x * factor; }
        var paramX = new Identifier("x");
        var paramFactor = new Identifier("factor");
        var body = new BinaryExpression(paramX, "*", paramFactor);
        var returnStmt = new ReturnStatement(body);
        var multiplyFunc = new FunctionDeclaration("multiply", new List<string> { "x", "factor" }, new List<Statement> { returnStmt });
        
        // Create pipeline: 5 |> multiply(3) - should become multiply(5, 3)
        var pipelineValue = new NumberLiteral(5);
        var functionCall = new CallExpression(
            new Identifier("multiply"), 
            new List<Expression> { new NumberLiteral(3) }
        );
        var pipeline = new PipelineExpression(pipelineValue, functionCall);

        // Act
        interpreter.Evaluate(multiplyFunc, "function multiply(x, factor) { return x * factor; }");
        var result = interpreter.Evaluate(pipeline, "5 |> multiply(3)");

        // Assert
        Assert.Equal(15.0, result);
    }

    [Fact]
    public void Evaluate_StringPipeline_WorksWithStringFunctions()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var moduleSystem = new ModuleSystem();
        interpreter.SetModuleSystem(moduleSystem);
        
        // First define a simple string transformation function
        var functionCode = "function addHello(str) { return 'hello ' + str; }";
        var lexer1 = new ECEngine.Lexer.Lexer(functionCode);
        var tokens1 = lexer1.Tokenize();
        var parser1 = new ECEngine.Parser.Parser();
        var ast1 = parser1.Parse(functionCode);
        interpreter.Evaluate(ast1, functionCode);
        
        // Create pipeline: "world" |> addHello
        var pipelineValue = new StringLiteral("world");
        var functionRef = new Identifier("addHello");
        var pipeline = new PipelineExpression(pipelineValue, functionRef);

        // Act
        var result = interpreter.Evaluate(pipeline, "\"world\" |> addHello");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("hello world", result.ToString());
    }

    [Fact]
    public void Evaluate_PipelineWithComplexExpression_EvaluatesLeftSideFirst()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        
        // Define function
        var doubleParam = new Identifier("x");
        var doubleBody = new BinaryExpression(doubleParam, "*", new NumberLiteral(2));
        var doubleReturn = new ReturnStatement(doubleBody);
        var doubleFunc = new FunctionDeclaration("double", new List<string> { "x" }, new List<Statement> { doubleReturn });
        
        // Create pipeline: (2 + 3) |> double
        var leftExpression = new BinaryExpression(new NumberLiteral(2), "+", new NumberLiteral(3));
        var pipeline = new PipelineExpression(leftExpression, new Identifier("double"));

        // Act
        interpreter.Evaluate(doubleFunc, "function double(x) { return x * 2; }");
        var result = interpreter.Evaluate(pipeline, "(2 + 3) |> double");

        // Assert - (2 + 3) * 2 = 10
        Assert.Equal(10.0, result);
    }
}
