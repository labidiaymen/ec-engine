using Xunit;
using ECEngine.Parser;
using ECEngine.AST;

namespace ECEngine.Tests.Parser;

[Collection("Console Tests")]
public class TemplateLiteralParsingTests
{
    [Fact]
    public void Test_SimpleTemplate_ParsesCorrectly()
    {
        // Arrange
        var code = "`Hello World`";
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var exprStmt = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var templateLiteral = Assert.IsType<TemplateLiteral>(exprStmt.Expression);
        
        Assert.Single(templateLiteral.Elements);
        var textElement = Assert.IsType<TemplateText>(templateLiteral.Elements[0]);
        Assert.Equal("Hello World", textElement.Value);
    }

    [Fact]
    public void Test_TemplateWithSingleInterpolation_ParsesCorrectly()
    {
        // Arrange
        var code = "`Hello ${name}!`";
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var exprStmt = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var templateLiteral = Assert.IsType<TemplateLiteral>(exprStmt.Expression);
        
        Assert.Equal(3, templateLiteral.Elements.Count);
        
        var textElement1 = Assert.IsType<TemplateText>(templateLiteral.Elements[0]);
        Assert.Equal("Hello ", textElement1.Value);
        
        var exprElement = Assert.IsType<TemplateExpression>(templateLiteral.Elements[1]);
        var identifier = Assert.IsType<Identifier>(exprElement.Expression);
        Assert.Equal("name", identifier.Name);
        
        var textElement2 = Assert.IsType<TemplateText>(templateLiteral.Elements[2]);
        Assert.Equal("!", textElement2.Value);
    }

    [Fact]
    public void Test_TemplateWithMultipleInterpolations_ParsesCorrectly()
    {
        // Arrange
        var code = "`Hello ${firstName} ${lastName}!`";
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var exprStmt = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var templateLiteral = Assert.IsType<TemplateLiteral>(exprStmt.Expression);
        
        Assert.Equal(5, templateLiteral.Elements.Count);
        
        var textElement1 = Assert.IsType<TemplateText>(templateLiteral.Elements[0]);
        Assert.Equal("Hello ", textElement1.Value);
        
        var exprElement1 = Assert.IsType<TemplateExpression>(templateLiteral.Elements[1]);
        var identifier1 = Assert.IsType<Identifier>(exprElement1.Expression);
        Assert.Equal("firstName", identifier1.Name);
        
        var textElement2 = Assert.IsType<TemplateText>(templateLiteral.Elements[2]);
        Assert.Equal(" ", textElement2.Value);
        
        var exprElement2 = Assert.IsType<TemplateExpression>(templateLiteral.Elements[3]);
        var identifier2 = Assert.IsType<Identifier>(exprElement2.Expression);
        Assert.Equal("lastName", identifier2.Name);
        
        var textElement3 = Assert.IsType<TemplateText>(templateLiteral.Elements[4]);
        Assert.Equal("!", textElement3.Value);
    }

    [Fact]
    public void Test_TemplateWithComplexExpression_ParsesCorrectly()
    {
        // Arrange
        var code = "`Result: ${x + y * 2}`";
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var exprStmt = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var templateLiteral = Assert.IsType<TemplateLiteral>(exprStmt.Expression);
        
        Assert.Equal(3, templateLiteral.Elements.Count);
        
        var textElement = Assert.IsType<TemplateText>(templateLiteral.Elements[0]);
        Assert.Equal("Result: ", textElement.Value);
        
        var exprElement = Assert.IsType<TemplateExpression>(templateLiteral.Elements[1]);
        var binaryExpr = Assert.IsType<BinaryExpression>(exprElement.Expression);
        Assert.Equal("+", binaryExpr.Operator);
        
        var endTextElement = Assert.IsType<TemplateText>(templateLiteral.Elements[2]);
        Assert.Equal("", endTextElement.Value);
    }

    [Fact]
    public void Test_EmptyTemplate_ParsesCorrectly()
    {
        // Arrange
        var code = "``";
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var exprStmt = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var templateLiteral = Assert.IsType<TemplateLiteral>(exprStmt.Expression);
        
        Assert.Single(templateLiteral.Elements);
        var textElement = Assert.IsType<TemplateText>(templateLiteral.Elements[0]);
        Assert.Equal("", textElement.Value);
    }

    [Fact]
    public void Test_TemplateWithNumberInterpolation_ParsesCorrectly()
    {
        // Arrange
        var code = "`Value: ${42}`";
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var exprStmt = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var templateLiteral = Assert.IsType<TemplateLiteral>(exprStmt.Expression);
        
        Assert.Equal(3, templateLiteral.Elements.Count);
        
        var textElement = Assert.IsType<TemplateText>(templateLiteral.Elements[0]);
        Assert.Equal("Value: ", textElement.Value);
        
        var exprElement = Assert.IsType<TemplateExpression>(templateLiteral.Elements[1]);
        var numberLiteral = Assert.IsType<NumberLiteral>(exprElement.Expression);
        Assert.Equal(42, numberLiteral.Value);
        
        var endTextElement = Assert.IsType<TemplateText>(templateLiteral.Elements[2]);
        Assert.Equal("", endTextElement.Value);
    }

    [Fact]
    public void Test_TemplateWithStringInterpolation_ParsesCorrectly()
    {
        // Arrange
        var code = "`Greeting: ${\"Hello\"}`";
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var exprStmt = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var templateLiteral = Assert.IsType<TemplateLiteral>(exprStmt.Expression);
        
        Assert.Equal(3, templateLiteral.Elements.Count);
        
        var textElement = Assert.IsType<TemplateText>(templateLiteral.Elements[0]);
        Assert.Equal("Greeting: ", textElement.Value);
        
        var exprElement = Assert.IsType<TemplateExpression>(templateLiteral.Elements[1]);
        var stringLiteral = Assert.IsType<StringLiteral>(exprElement.Expression);
        Assert.Equal("Hello", stringLiteral.Value);
        
        var endTextElement = Assert.IsType<TemplateText>(templateLiteral.Elements[2]);
        Assert.Equal("", endTextElement.Value);
    }

    [Fact]
    public void Test_TemplateAssignedToVariable_ParsesCorrectly()
    {
        // Arrange
        var code = "var message = `Hello ${name}!`;";
        var parser = new ECEngine.Parser.Parser();

        // Act
        var ast = parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var varDecl = Assert.IsType<VariableDeclaration>(program.Body[0]);
        var templateLiteral = Assert.IsType<TemplateLiteral>(varDecl.Initializer);
        
        Assert.Equal("message", varDecl.Name);
        Assert.Equal(3, templateLiteral.Elements.Count);
    }
}
