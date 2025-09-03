using Xunit;
using ECEngine.Parser;
using ECEngine.AST;

namespace ECEngine.Tests.Parser;

public class PipelineParsingTests
{
    [Fact]
    public void Parse_SimplePipeline_ReturnsPipelineExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("5 |> double");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var pipelineExpression = Assert.IsType<PipelineExpression>(expressionStatement.Expression);

        var left = Assert.IsType<NumberLiteral>(pipelineExpression.Left);
        Assert.Equal(5, left.Value);

        var right = Assert.IsType<Identifier>(pipelineExpression.Right);
        Assert.Equal("double", right.Name);
    }

    [Fact]
    public void Parse_ChainedPipeline_ReturnsNestedPipelineExpressions()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("5 |> double |> add10");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var outerPipeline = Assert.IsType<PipelineExpression>(expressionStatement.Expression);

        // Check outer pipeline (5 |> double) |> add10
        var innerPipeline = Assert.IsType<PipelineExpression>(outerPipeline.Left);
        var rightFunction = Assert.IsType<Identifier>(outerPipeline.Right);
        Assert.Equal("add10", rightFunction.Name);

        // Check inner pipeline 5 |> double
        var leftValue = Assert.IsType<NumberLiteral>(innerPipeline.Left);
        var innerRightFunction = Assert.IsType<Identifier>(innerPipeline.Right);
        Assert.Equal(5, leftValue.Value);
        Assert.Equal("double", innerRightFunction.Name);
    }

    [Fact]
    public void Parse_PipelineWithFunctionCall_ReturnsPipelineWithCallExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("5 |> add(3)");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var pipelineExpression = Assert.IsType<PipelineExpression>(expressionStatement.Expression);

        var left = Assert.IsType<NumberLiteral>(pipelineExpression.Left);
        Assert.Equal(5, left.Value);

        var right = Assert.IsType<CallExpression>(pipelineExpression.Right);
        var callee = Assert.IsType<Identifier>(right.Callee);
        Assert.Equal("add", callee.Name);
        Assert.Single(right.Arguments);
        
        var argument = Assert.IsType<NumberLiteral>(right.Arguments[0]);
        Assert.Equal(3, argument.Value);
    }

    [Fact]
    public void Parse_MultilinePipeline_ReturnsPipelineExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var code = @"5
    |> double
    |> add10";

        // Act
        var ast = parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var outerPipeline = Assert.IsType<PipelineExpression>(expressionStatement.Expression);

        // Should parse as (5 |> double) |> add10
        var innerPipeline = Assert.IsType<PipelineExpression>(outerPipeline.Left);
        var leftValue = Assert.IsType<NumberLiteral>(innerPipeline.Left);
        Assert.Equal(5, leftValue.Value);
    }

    [Fact]
    public void Parse_PipelineWithMultipleParameters_ReturnsPipelineWithCallExpression()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse("'hello' |> padString(10, '0')");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var pipelineExpression = Assert.IsType<PipelineExpression>(expressionStatement.Expression);

        var left = Assert.IsType<StringLiteral>(pipelineExpression.Left);
        Assert.Equal("hello", left.Value);

        var right = Assert.IsType<CallExpression>(pipelineExpression.Right);
        var callee = Assert.IsType<Identifier>(right.Callee);
        Assert.Equal("padString", callee.Name);
        Assert.Equal(2, right.Arguments.Count);
        
        var arg1 = Assert.IsType<NumberLiteral>(right.Arguments[0]);
        Assert.Equal(10, arg1.Value);
        
        var arg2 = Assert.IsType<StringLiteral>(right.Arguments[1]);
        Assert.Equal("0", arg2.Value);
    }

    [Fact]
    public void Parse_PipelinePrecedence_CorrectlyHandlesOperatorPrecedence()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();

        // Act - Pipeline should have lower precedence than arithmetic
        var ast = parser.Parse("2 + 3 |> double");

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var pipelineExpression = Assert.IsType<PipelineExpression>(expressionStatement.Expression);

        // Left side should be (2 + 3), not 2 + (3 |> double)
        var left = Assert.IsType<BinaryExpression>(pipelineExpression.Left);
        Assert.Equal("+", left.Operator);
        
        var right = Assert.IsType<Identifier>(pipelineExpression.Right);
        Assert.Equal("double", right.Name);
    }
}
