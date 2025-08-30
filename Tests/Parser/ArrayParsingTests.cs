using Xunit;
using ECEngine.Parser;
using ECEngine.AST;
using ECEngine.Lexer;

namespace ECEngine.Tests.Parser;

/// <summary>
/// Tests for array literal and array access parsing
/// </summary>
[Collection("Console Tests")]
public class ArrayParsingTests
{
    [Fact]
    public void Test_EmptyArray_Parsing()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse("[]");
        
        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var arrayLiteral = Assert.IsType<ArrayLiteral>(expressionStatement.Expression);
        Assert.Empty(arrayLiteral.Elements);
    }

    [Fact]
    public void Test_SimpleArray_Parsing()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse("[1, 2, 3]");
        
        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var arrayLiteral = Assert.IsType<ArrayLiteral>(expressionStatement.Expression);
        Assert.Equal(3, arrayLiteral.Elements.Count);
        
        var first = Assert.IsType<NumberLiteral>(arrayLiteral.Elements[0]);
        Assert.Equal(1.0, first.Value);
        
        var second = Assert.IsType<NumberLiteral>(arrayLiteral.Elements[1]);
        Assert.Equal(2.0, second.Value);
        
        var third = Assert.IsType<NumberLiteral>(arrayLiteral.Elements[2]);
        Assert.Equal(3.0, third.Value);
    }

    [Fact]
    public void Test_MixedTypeArray_Parsing()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse("[1, \"hello\", true, null]");
        
        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var arrayLiteral = Assert.IsType<ArrayLiteral>(expressionStatement.Expression);
        Assert.Equal(4, arrayLiteral.Elements.Count);
        
        Assert.IsType<NumberLiteral>(arrayLiteral.Elements[0]);
        Assert.IsType<StringLiteral>(arrayLiteral.Elements[1]);
        Assert.IsType<BooleanLiteral>(arrayLiteral.Elements[2]);
        Assert.IsType<NullLiteral>(arrayLiteral.Elements[3]);
    }

    [Fact]
    public void Test_NestedArray_Parsing()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse("[[1, 2], [3, 4]]");
        
        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var outerArray = Assert.IsType<ArrayLiteral>(expressionStatement.Expression);
        Assert.Equal(2, outerArray.Elements.Count);
        
        var firstInner = Assert.IsType<ArrayLiteral>(outerArray.Elements[0]);
        Assert.Equal(2, firstInner.Elements.Count);
        
        var secondInner = Assert.IsType<ArrayLiteral>(outerArray.Elements[1]);
        Assert.Equal(2, secondInner.Elements.Count);
    }

    [Fact]
    public void Test_ArrayWithTrailingComma_Parsing()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse("[1, 2, 3,]");
        
        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var arrayLiteral = Assert.IsType<ArrayLiteral>(expressionStatement.Expression);
        Assert.Equal(3, arrayLiteral.Elements.Count); // Trailing comma ignored
    }

    [Fact]
    public void Test_ArrayAccess_Parsing()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse("arr[0]");
        
        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var memberExpr = Assert.IsType<MemberExpression>(expressionStatement.Expression);
        Assert.True(memberExpr.Computed);
        Assert.IsType<Identifier>(memberExpr.Object);
        Assert.IsType<NumberLiteral>(memberExpr.ComputedProperty);
    }

    [Fact]
    public void Test_ArrayAccessWithVariable_Parsing()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse("arr[index]");
        
        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var memberExpr = Assert.IsType<MemberExpression>(expressionStatement.Expression);
        Assert.True(memberExpr.Computed);
        Assert.IsType<Identifier>(memberExpr.Object);
        Assert.IsType<Identifier>(memberExpr.ComputedProperty);
    }

    [Fact]
    public void Test_ChainedArrayAccess_Parsing()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse("arr[0][1]");
        
        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var outerMember = Assert.IsType<MemberExpression>(expressionStatement.Expression);
        Assert.True(outerMember.Computed);
        
        var innerMember = Assert.IsType<MemberExpression>(outerMember.Object);
        Assert.True(innerMember.Computed);
        Assert.IsType<Identifier>(innerMember.Object);
    }

    [Fact]
    public void Test_ArrayAccessWithExpression_Parsing()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse("arr[i + 1]");
        
        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        var expressionStatement = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var memberExpr = Assert.IsType<MemberExpression>(expressionStatement.Expression);
        Assert.True(memberExpr.Computed);
        Assert.IsType<BinaryExpression>(memberExpr.ComputedProperty);
    }
}
