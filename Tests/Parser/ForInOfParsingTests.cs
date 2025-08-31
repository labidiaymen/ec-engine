using ECEngine.AST;
using ECEngine.Parser;
using Xunit;

namespace ECEngine.Tests.Parser;

[Collection("Console Tests")]
public class ForInOfParsingTests
{
    [Fact]
    public void TestForInLoopParsing()
    {
        // Arrange
        var code = "for (key in object) { console.log(key); }";
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse(code);
        
        // Assert
        Assert.IsType<ProgramNode>(ast);
        var program = (ProgramNode)ast;
        Assert.Single(program.Body);
        
        var forInStmt = Assert.IsType<ForInStatement>(program.Body[0]);
        Assert.Equal("key", forInStmt.Variable);
        Assert.IsType<Identifier>(forInStmt.Object);
        Assert.Equal("object", ((Identifier)forInStmt.Object).Name);
        Assert.IsType<BlockStatement>(forInStmt.Body);
    }
    
    [Fact]
    public void TestForOfLoopParsing()
    {
        // Arrange
        var code = "for (item of array) { console.log(item); }";
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse(code);
        
        // Assert
        Assert.IsType<ProgramNode>(ast);
        var program = (ProgramNode)ast;
        Assert.Single(program.Body);
        
        var forOfStmt = Assert.IsType<ForOfStatement>(program.Body[0]);
        Assert.Equal("item", forOfStmt.Variable);
        Assert.IsType<Identifier>(forOfStmt.Iterable);
        Assert.Equal("array", ((Identifier)forOfStmt.Iterable).Name);
        Assert.IsType<BlockStatement>(forOfStmt.Body);
    }
    
    [Fact]
    public void TestForInWithVarDeclaration()
    {
        // Arrange
        var code = "for (var key in obj) { key = key + 1; }";
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse(code);
        
        // Assert
        var program = (ProgramNode)ast;
        var forInStmt = Assert.IsType<ForInStatement>(program.Body[0]);
        Assert.Equal("key", forInStmt.Variable);
        Assert.Equal("obj", ((Identifier)forInStmt.Object).Name);
    }
    
    [Fact]
    public void TestForOfWithLetDeclaration()
    {
        // Arrange
        var code = "for (let item of items) { console.log(item); }";
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse(code);
        
        // Assert
        var program = (ProgramNode)ast;
        var forOfStmt = Assert.IsType<ForOfStatement>(program.Body[0]);
        Assert.Equal("item", forOfStmt.Variable);
        Assert.Equal("items", ((Identifier)forOfStmt.Iterable).Name);
    }
    
    [Fact]
    public void TestForOfWithConstDeclaration()
    {
        // Arrange
        var code = "for (const element of elements) { process(element); }";
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse(code);
        
        // Assert
        var program = (ProgramNode)ast;
        var forOfStmt = Assert.IsType<ForOfStatement>(program.Body[0]);
        Assert.Equal("element", forOfStmt.Variable);
        Assert.Equal("elements", ((Identifier)forOfStmt.Iterable).Name);
    }
    
    [Fact]
    public void TestTraditionalForLoopStillWorks()
    {
        // Arrange
        var code = "for (var i = 0; i < 10; i++) { console.log(i); }";
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse(code);
        
        // Assert
        var program = (ProgramNode)ast;
        var forStmt = Assert.IsType<ForStatement>(program.Body[0]);
        Assert.NotNull(forStmt.Init);
        Assert.NotNull(forStmt.Condition);
        Assert.NotNull(forStmt.Update);
    }
    
    [Fact]
    public void TestForInWithMemberExpression()
    {
        // Arrange
        var code = "for (key in obj.properties) { console.log(key); }";
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse(code);
        
        // Assert
        var program = (ProgramNode)ast;
        var forInStmt = Assert.IsType<ForInStatement>(program.Body[0]);
        Assert.Equal("key", forInStmt.Variable);
        Assert.IsType<MemberExpression>(forInStmt.Object);
    }
    
    [Fact]
    public void TestForOfWithArrayLiteral()
    {
        // Arrange
        var code = "for (item of [1, 2, 3]) { console.log(item); }";
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse(code);
        
        // Assert
        var program = (ProgramNode)ast;
        var forOfStmt = Assert.IsType<ForOfStatement>(program.Body[0]);
        Assert.Equal("item", forOfStmt.Variable);
        Assert.IsType<ArrayLiteral>(forOfStmt.Iterable);
    }
    
    [Fact]
    public void TestNestedForInOfLoops()
    {
        // Arrange
        var code = @"
            for (key in obj) {
                for (item of arr) {
                    console.log(key, item);
                }
            }";
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse(code);
        
        // Assert
        var program = (ProgramNode)ast;
        var outerForIn = Assert.IsType<ForInStatement>(program.Body[0]);
        
        var outerBody = Assert.IsType<BlockStatement>(outerForIn.Body);
        var innerForOf = Assert.IsType<ForOfStatement>(outerBody.Body[0]);
        
        Assert.Equal("key", outerForIn.Variable);
        Assert.Equal("item", innerForOf.Variable);
    }
    
    [Fact]
    public void TestForInWithoutDeclaration()
    {
        // Arrange
        var code = "for (existingVar in obj) { console.log(existingVar); }";
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse(code);
        
        // Assert
        var program = (ProgramNode)ast;
        var forInStmt = Assert.IsType<ForInStatement>(program.Body[0]);
        Assert.Equal("existingVar", forInStmt.Variable);
    }
    
    [Fact]
    public void TestForOfWithoutDeclaration()
    {
        // Arrange
        var code = "for (existingVar of arr) { console.log(existingVar); }";
        var parser = new ECEngine.Parser.Parser();
        
        // Act
        var ast = parser.Parse(code);
        
        // Assert
        var program = (ProgramNode)ast;
        var forOfStmt = Assert.IsType<ForOfStatement>(program.Body[0]);
        Assert.Equal("existingVar", forOfStmt.Variable);
    }
}
