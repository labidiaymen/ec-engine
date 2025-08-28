using Xunit;
using ECEngine.Parser;
using ECEngine.AST;

namespace ECEngine.Tests.Parser;

public class VariableParsingTests
{
    [Fact]
    public void Parse_VarDeclarationWithInitializer_CreatesCorrectAST()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var input = "var x = 42;";

        // Act
        var ast = parser.Parse(input);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        
        var varDecl = Assert.IsType<VariableDeclaration>(program.Body[0]);
        Assert.Equal("var", varDecl.Kind);
        Assert.Equal("x", varDecl.Name);
        Assert.NotNull(varDecl.Initializer);
        
        var initializer = Assert.IsType<NumberLiteral>(varDecl.Initializer);
        Assert.Equal(42.0, initializer.Value);
    }

    [Fact]
    public void Parse_LetDeclarationWithoutInitializer_CreatesCorrectAST()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var input = "let y;";

        // Act
        var ast = parser.Parse(input);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        
        var varDecl = Assert.IsType<VariableDeclaration>(program.Body[0]);
        Assert.Equal("let", varDecl.Kind);
        Assert.Equal("y", varDecl.Name);
        Assert.Null(varDecl.Initializer);
    }

    [Fact]
    public void Parse_ConstDeclarationWithInitializer_CreatesCorrectAST()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var input = "const z = 3.14;";

        // Act
        var ast = parser.Parse(input);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        
        var varDecl = Assert.IsType<VariableDeclaration>(program.Body[0]);
        Assert.Equal("const", varDecl.Kind);
        Assert.Equal("z", varDecl.Name);
        Assert.NotNull(varDecl.Initializer);
        
        var initializer = Assert.IsType<NumberLiteral>(varDecl.Initializer);
        Assert.Equal(3.14, initializer.Value);
    }

    [Fact]
    public void Parse_Assignment_CreatesCorrectAST()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var input = "x = 100;";

        // Act
        var ast = parser.Parse(input);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        
        var exprStmt = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var assignment = Assert.IsType<AssignmentExpression>(exprStmt.Expression);
        
        Assert.Equal("x", assignment.Left.Name);
        
        var right = Assert.IsType<NumberLiteral>(assignment.Right);
        Assert.Equal(100.0, right.Value);
    }

    [Fact]
    public void Parse_ComplexAssignment_CreatesCorrectAST()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var input = "x = y + 5;";

        // Act
        var ast = parser.Parse(input);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        
        var exprStmt = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var assignment = Assert.IsType<AssignmentExpression>(exprStmt.Expression);
        
        Assert.Equal("x", assignment.Left.Name);
        
        var right = Assert.IsType<BinaryExpression>(assignment.Right);
        Assert.Equal("+", right.Operator);
        
        var leftOperand = Assert.IsType<Identifier>(right.Left);
        Assert.Equal("y", leftOperand.Name);
        
        var rightOperand = Assert.IsType<NumberLiteral>(right.Right);
        Assert.Equal(5.0, rightOperand.Value);
    }

    [Fact]
    public void Parse_MultipleVariableDeclarations_CreatesCorrectAST()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var input = "var a = 1; let b = 2; const c = 3;";

        // Act
        var ast = parser.Parse(input);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Equal(3, program.Body.Count);
        
        // First declaration
        var varDecl1 = Assert.IsType<VariableDeclaration>(program.Body[0]);
        Assert.Equal("var", varDecl1.Kind);
        Assert.Equal("a", varDecl1.Name);
        
        // Second declaration
        var varDecl2 = Assert.IsType<VariableDeclaration>(program.Body[1]);
        Assert.Equal("let", varDecl2.Kind);
        Assert.Equal("b", varDecl2.Name);
        
        // Third declaration
        var varDecl3 = Assert.IsType<VariableDeclaration>(program.Body[2]);
        Assert.Equal("const", varDecl3.Kind);
        Assert.Equal("c", varDecl3.Name);
    }

    [Fact]
    public void Parse_VariableDeclarationWithExpression_CreatesCorrectAST()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var input = "var result = 10 + 20 * 2;";

        // Act
        var ast = parser.Parse(input);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        
        var varDecl = Assert.IsType<VariableDeclaration>(program.Body[0]);
        Assert.Equal("var", varDecl.Kind);
        Assert.Equal("result", varDecl.Name);
        
        var initializer = Assert.IsType<BinaryExpression>(varDecl.Initializer);
        Assert.Equal("+", initializer.Operator);
    }

    [Fact]
    public void Parse_ChainedAssignment_CreatesCorrectAST()
    {
        // Arrange
        var parser = new ECEngine.Parser.Parser();
        var input = "x = y = 42;";

        // Act
        var ast = parser.Parse(input);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        Assert.Single(program.Body);
        
        var exprStmt = Assert.IsType<ExpressionStatement>(program.Body[0]);
        var outerAssignment = Assert.IsType<AssignmentExpression>(exprStmt.Expression);
        
        Assert.Equal("x", outerAssignment.Left.Name);
        
        var innerAssignment = Assert.IsType<AssignmentExpression>(outerAssignment.Right);
        Assert.Equal("y", innerAssignment.Left.Name);
        
        var value = Assert.IsType<NumberLiteral>(innerAssignment.Right);
        Assert.Equal(42.0, value.Value);
    }
}
