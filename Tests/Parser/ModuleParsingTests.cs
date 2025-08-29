using Xunit;
using ECEngine.Parser;
using ECEngine.AST;

namespace ECEngine.Tests.Parser;

public class ModuleParsingTests
{
    private readonly ECEngine.Parser.Parser _parser;

    public ModuleParsingTests()
    {
        _parser = new ECEngine.Parser.Parser();
    }

    [Fact]
    public void Parse_ExportVariableDeclaration_ShouldCreateExportStatement()
    {
        // Arrange
        var code = "export var x = 42;";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var exportStmt = Assert.IsType<ExportStatement>(program.Body[0]);
        var varDecl = Assert.IsType<VariableDeclaration>(exportStmt.Declaration);
        Assert.Equal("x", varDecl.Name);
        Assert.Equal("var", varDecl.Kind);
    }

    [Fact]
    public void Parse_ExportFunctionDeclaration_ShouldCreateExportStatement()
    {
        // Arrange
        var code = "export function test() { return 42; }";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var exportStmt = Assert.IsType<ExportStatement>(program.Body[0]);
        var funcDecl = Assert.IsType<FunctionDeclaration>(exportStmt.Declaration);
        Assert.Equal("test", funcDecl.Name);
    }

    [Fact]
    public void Parse_ImportStatement_ShouldCreateImportStatement()
    {
        // Arrange
        var code = "import { add, multiply } from \"./math.ec\";";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var importStmt = Assert.IsType<ImportStatement>(program.Body[0]);
        Assert.Equal(2, importStmt.ImportedNames.Count);
        Assert.Contains("add", importStmt.ImportedNames);
        Assert.Contains("multiply", importStmt.ImportedNames);
        Assert.Equal("./math.ec", importStmt.ModulePath);
    }

    [Fact]
    public void Parse_ImportSingleItem_ShouldCreateImportStatement()
    {
        // Arrange
        var code = "import { PI } from \"./constants.ec\";";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var importStmt = Assert.IsType<ImportStatement>(program.Body[0]);
        Assert.Single(importStmt.ImportedNames);
        Assert.Equal("PI", importStmt.ImportedNames[0]);
        Assert.Equal("./constants.ec", importStmt.ModulePath);
    }

    [Fact]
    public void Parse_EmptyImportList_ShouldCreateImportStatement()
    {
        // Arrange
        var code = "import { } from \"./empty.ec\";";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var importStmt = Assert.IsType<ImportStatement>(program.Body[0]);
        Assert.Empty(importStmt.ImportedNames);
        Assert.Equal("./empty.ec", importStmt.ModulePath);
    }
}
