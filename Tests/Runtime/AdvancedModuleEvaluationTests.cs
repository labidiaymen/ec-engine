using Xunit;
using ECEngine.AST;
using ECEngine.Runtime;
using ECEngine.Lexer;
using ECEngine.Parser;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Runtime;

public class AdvancedModuleEvaluationTests
{
    private object? ExecuteCode(string code)
    {
        var lexer = new ECEngine.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();
        var parser = new ECEngine.Parser.Parser();
        var ast = parser.Parse(code);
        var interpreter = new RuntimeInterpreter();
        return interpreter.Evaluate(ast, code);
    }

    private RuntimeInterpreter CreateInterpreterWithCode(string code)
    {
        var lexer = new ECEngine.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();
        var parser = new ECEngine.Parser.Parser();
        var ast = parser.Parse(code);
        var interpreter = new RuntimeInterpreter();
        interpreter.Evaluate(ast, code);
        return interpreter;
    }

    [Fact]
    public void Evaluate_DefaultExportExpression_ShouldAddToExports()
    {
        // Arrange
        var code = "export default 42;";

        // Act
        var interpreter = CreateInterpreterWithCode(code);

        // Assert
        Assert.True(interpreter.GetExports().ContainsKey("default"));
        Assert.Equal(42.0, interpreter.GetExports()["default"]); // ECEngine stores numbers as double
    }

    [Fact]
    public void Evaluate_NamedExportWithoutRenaming_ShouldAddToExports()
    {
        // Arrange
        var code = @"
            let value1 = 10;
            let value2 = 'hello';
            export { value1, value2 };
        ";

        // Act
        var interpreter = CreateInterpreterWithCode(code);

        // Assert
        var exports = interpreter.GetExports();
        Assert.True(exports.ContainsKey("value1"));
        Assert.Equal(10.0, exports["value1"]); // ECEngine stores numbers as double
        
        Assert.True(exports.ContainsKey("value2"));
        Assert.Equal("hello", exports["value2"]);
    }

    [Fact]
    public void Evaluate_NamedExportWithRenaming_ShouldAddToExports()
    {
        // Arrange
        var code = @"
            let value1 = 10;
            let value2 = 'hello';
            export { value1 as renamed1, value2 as renamed2 };
        ";

        // Act
        var interpreter = CreateInterpreterWithCode(code);

        // Assert
        var exports = interpreter.GetExports();
        Assert.True(exports.ContainsKey("renamed1"));
        Assert.Equal(10.0, exports["renamed1"]); // ECEngine stores numbers as double
        
        Assert.True(exports.ContainsKey("renamed2"));
        Assert.Equal("hello", exports["renamed2"]);
        
        // Original names should not be exported
        Assert.False(exports.ContainsKey("value1"));
        Assert.False(exports.ContainsKey("value2"));
    }

    [Fact]
    public void Evaluate_NamedExportWithMixedRenaming_ShouldAddToExports()
    {
        // Arrange
        var code = @"
            let value1 = 10;
            let value2 = 'hello';
            let value3 = true;
            export { value1, value2 as renamed2, value3 };
        ";

        // Act
        var interpreter = CreateInterpreterWithCode(code);

        // Assert
        var exports = interpreter.GetExports();
        Assert.True(exports.ContainsKey("value1"));
        Assert.Equal(10.0, exports["value1"]); // ECEngine stores numbers as double
        
        Assert.True(exports.ContainsKey("renamed2"));
        Assert.Equal("hello", exports["renamed2"]);
        
        Assert.True(exports.ContainsKey("value3"));
        Assert.Equal(true, exports["value3"]);
        
        // value2 should not be exported under its original name
        Assert.False(exports.ContainsKey("value2"));
    }

    [Fact]
    public void Evaluate_NamedExportUndefinedVariable_ShouldThrowException()
    {
        // Arrange
        var code = "export { undefinedVar };";

        // Act & Assert
        Assert.Throws<ECEngine.Runtime.ECEngineException>(() =>
        {
            ExecuteCode(code);
        });
    }

    [Fact]
    public void Evaluate_CombinedExports_ShouldAddAllToExports()
    {
        // Arrange
        var code = @"
            let regularVar = 42;
            function regularFunc() { return 'func'; }
            
            export let namedVar = 'named';
            export function namedFunc() { return 'namedFunc'; }
            export { regularVar as renamed };
            export default regularFunc;
        ";

        // Act
        var interpreter = CreateInterpreterWithCode(code);

        // Assert
        var exports = interpreter.GetExports();
        
        // Regular export
        Assert.True(exports.ContainsKey("namedVar"));
        Assert.Equal("named", exports["namedVar"]);
        
        Assert.True(exports.ContainsKey("namedFunc"));
        Assert.NotNull(exports["namedFunc"]);
        
        // Named export with renaming
        Assert.True(exports.ContainsKey("renamed"));
        Assert.Equal(42.0, exports["renamed"]); // ECEngine stores numbers as double
        
        // Default export
        Assert.True(exports.ContainsKey("default"));
        Assert.NotNull(exports["default"]);
    }

    [Fact]
    public void Evaluate_MultipleDefaultExports_ShouldOverwritePrevious()
    {
        // Arrange
        var code = @"
            export default 42;
            export default 'hello';
        ";

        // Act
        var interpreter = CreateInterpreterWithCode(code);

        // Assert
        var exports = interpreter.GetExports();
        // Second default export should overwrite the first
        Assert.True(exports.ContainsKey("default"));
        Assert.Equal("hello", exports["default"]);
    }
}
