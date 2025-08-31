using Xunit;
using ECEngine.Parser;
using ECEngine.AST;

namespace ECEngine.Tests.Parser;

public class AdvancedModuleParsingTests
{
    private readonly ECEngine.Parser.Parser _parser;

    public AdvancedModuleParsingTests()
    {
        _parser = new ECEngine.Parser.Parser();
    }

    [Fact]
    public void Parse_DefaultExportFunction_ShouldCreateDefaultExportStatement()
    {
        // Arrange
        var code = "export default function() { return 42; }";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var defaultExport = Assert.IsType<DefaultExportStatement>(program.Body[0]);
        Assert.IsType<FunctionDeclaration>(defaultExport.Value);
    }

    [Fact]
    public void Parse_DefaultExportExpression_ShouldCreateDefaultExportStatement()
    {
        // Arrange
        var code = "export default 42;";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var defaultExport = Assert.IsType<DefaultExportStatement>(program.Body[0]);
        Assert.IsType<NumberLiteral>(defaultExport.Value);
    }

    [Fact]
    public void Parse_NamedExportWithoutRenaming_ShouldCreateNamedExportStatement()
    {
        // Arrange
        var code = "export { name1, name2 };";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var namedExport = Assert.IsType<NamedExportStatement>(program.Body[0]);
        Assert.Equal(2, namedExport.ExportedNames.Count);
        
        Assert.Equal("name1", namedExport.ExportedNames[0].LocalName);
        Assert.Null(namedExport.ExportedNames[0].ExportName);
        
        Assert.Equal("name2", namedExport.ExportedNames[1].LocalName);
        Assert.Null(namedExport.ExportedNames[1].ExportName);
    }

    [Fact]
    public void Parse_NamedExportWithRenaming_ShouldCreateNamedExportStatement()
    {
        // Arrange
        var code = "export { name1 as alias1, name2 as alias2 };";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var namedExport = Assert.IsType<NamedExportStatement>(program.Body[0]);
        Assert.Equal(2, namedExport.ExportedNames.Count);
        
        Assert.Equal("name1", namedExport.ExportedNames[0].LocalName);
        Assert.Equal("alias1", namedExport.ExportedNames[0].ExportName);
        
        Assert.Equal("name2", namedExport.ExportedNames[1].LocalName);
        Assert.Equal("alias2", namedExport.ExportedNames[1].ExportName);
    }

    [Fact]
    public void Parse_NamedExportWithMixedRenaming_ShouldCreateNamedExportStatement()
    {
        // Arrange
        var code = "export { name1, name2 as alias2, name3 };";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var namedExport = Assert.IsType<NamedExportStatement>(program.Body[0]);
        Assert.Equal(3, namedExport.ExportedNames.Count);
        
        Assert.Equal("name1", namedExport.ExportedNames[0].LocalName);
        Assert.Null(namedExport.ExportedNames[0].ExportName);
        
        Assert.Equal("name2", namedExport.ExportedNames[1].LocalName);
        Assert.Equal("alias2", namedExport.ExportedNames[1].ExportName);
        
        Assert.Equal("name3", namedExport.ExportedNames[2].LocalName);
        Assert.Null(namedExport.ExportedNames[2].ExportName);
    }

    [Fact]
    public void Parse_ReExportWithoutRenaming_ShouldCreateReExportStatement()
    {
        // Arrange
        var code = "export { name1, name2 } from \"./module\";";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var reExport = Assert.IsType<ReExportStatement>(program.Body[0]);
        Assert.Equal("./module", reExport.ModulePath);
        Assert.Equal(2, reExport.ExportedNames.Count);
        
        Assert.Equal("name1", reExport.ExportedNames[0].LocalName);
        Assert.Null(reExport.ExportedNames[0].ExportName);
        
        Assert.Equal("name2", reExport.ExportedNames[1].LocalName);
        Assert.Null(reExport.ExportedNames[1].ExportName);
    }

    [Fact]
    public void Parse_ReExportWithRenaming_ShouldCreateReExportStatement()
    {
        // Arrange
        var code = "export { name1 as alias1, name2 as alias2 } from \"./module\";";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var reExport = Assert.IsType<ReExportStatement>(program.Body[0]);
        Assert.Equal("./module", reExport.ModulePath);
        Assert.Equal(2, reExport.ExportedNames.Count);
        
        Assert.Equal("name1", reExport.ExportedNames[0].LocalName);
        Assert.Equal("alias1", reExport.ExportedNames[0].ExportName);
        
        Assert.Equal("name2", reExport.ExportedNames[1].LocalName);
        Assert.Equal("alias2", reExport.ExportedNames[1].ExportName);
    }

    [Fact]
    public void Parse_ReExportWithMixedRenaming_ShouldCreateReExportStatement()
    {
        // Arrange
        var code = "export { name1, name2 as alias2, name3 } from \"./other-module\";";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var reExport = Assert.IsType<ReExportStatement>(program.Body[0]);
        Assert.Equal("./other-module", reExport.ModulePath);
        Assert.Equal(3, reExport.ExportedNames.Count);
        
        Assert.Equal("name1", reExport.ExportedNames[0].LocalName);
        Assert.Null(reExport.ExportedNames[0].ExportName);
        
        Assert.Equal("name2", reExport.ExportedNames[1].LocalName);
        Assert.Equal("alias2", reExport.ExportedNames[1].ExportName);
        
        Assert.Equal("name3", reExport.ExportedNames[2].LocalName);
        Assert.Null(reExport.ExportedNames[2].ExportName);
    }

    [Fact]
    public void Parse_EmptyNamedExport_ShouldCreateNamedExportStatement()
    {
        // Arrange
        var code = "export { };";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var namedExport = Assert.IsType<NamedExportStatement>(program.Body[0]);
        Assert.Empty(namedExport.ExportedNames);
    }

    [Fact]
    public void Parse_EmptyReExport_ShouldCreateReExportStatement()
    {
        // Arrange
        var code = "export { } from \"./module\";";

        // Act
        var ast = _parser.Parse(code);

        // Assert
        var program = Assert.IsType<ProgramNode>(ast);
        var reExport = Assert.IsType<ReExportStatement>(program.Body[0]);
        Assert.Equal("./module", reExport.ModulePath);
        Assert.Empty(reExport.ExportedNames);
    }
}
