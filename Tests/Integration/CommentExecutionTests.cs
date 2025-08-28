using Xunit;
using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.Runtime;

namespace ECEngine.Tests.Integration;

public class CommentExecutionTests
{
    [Fact]
    public void Execute_CodeWithSingleLineComments_ExecutesCorrectly()
    {
        // Arrange
        var code = @"
            // This is a comment
            var x = 42; // Another comment
            // More comments
            x + 10;
        ";
        var lexer = new ECEngine.Lexer.Lexer(code);
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();

        // Act
        var tokens = lexer.Tokenize();
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal(52.0, result);
    }

    [Fact]
    public void Execute_CodeWithMultiLineComments_ExecutesCorrectly()
    {
        // Arrange
        var code = @"
            /* This is a 
               multi-line comment */
            var x = 42;
            /* Another comment
               with multiple lines */
            let y = x + 10;
            /* Final comment */ y;
        ";
        var lexer = new ECEngine.Lexer.Lexer(code);
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();

        // Act
        var tokens = lexer.Tokenize();
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal(52.0, result);
    }

    [Fact]
    public void Execute_CodeWithMixedComments_ExecutesCorrectly()
    {
        // Arrange
        var code = @"
            // Single line comment
            var x = 10;
            /* Multi-line comment */
            let y = 20; // End of line comment
            /*
             * Block comment with asterisks
             */
            const z = x + y; // Final comment
            z;
        ";
        var lexer = new ECEngine.Lexer.Lexer(code);
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();

        // Act
        var tokens = lexer.Tokenize();
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal(30.0, result);
    }

    [Fact]
    public void Execute_OnlyComments_ReturnsNull()
    {
        // Arrange
        var code = @"
            // This is just a comment
            /* And this is another comment */
            // No actual code here
        ";
        var lexer = new ECEngine.Lexer.Lexer(code);
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();

        // Act
        var tokens = lexer.Tokenize();
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Execute_CommentedOutCode_DoesNotExecute()
    {
        // Arrange
        var code = @"
            var x = 10;
            // var y = 20; // This line is commented out
            /* 
            var z = 30;
            */
            x;
        ";
        var lexer = new ECEngine.Lexer.Lexer(code);
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();

        // Act
        var tokens = lexer.Tokenize();
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal(10.0, result);
        // Verify that y and z variables don't exist
        Assert.False(interpreter.Variables.ContainsKey("y"));
        Assert.False(interpreter.Variables.ContainsKey("z"));
    }

    [Fact]
    public void Execute_DivisionAfterComment_ExecutesCorrectly()
    {
        // Arrange
        var code = @"
            // This is a comment
            var result = 20 / 4;
            result;
        ";
        var lexer = new ECEngine.Lexer.Lexer(code);
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();

        // Act
        var tokens = lexer.Tokenize();
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal(5.0, result);
    }

    [Fact]
    public void Execute_CommentWithSpecialCharacters_ExecutesCorrectly()
    {
        // Arrange
        var code = @"
            // Comment with special chars: @#$%^&*(){}[]<>?/\|`~
            var x = 42;
            /* Multi-line with special chars:
               !@#$%^&*()_+-={}[]|\:;""'<>?,./ */
            x;
        ";
        var lexer = new ECEngine.Lexer.Lexer(code);
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();

        // Act
        var tokens = lexer.Tokenize();
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal(42.0, result);
    }

    [Fact]
    public void Execute_NestedSlashesInComment_ExecutesCorrectly()
    {
        // Arrange
        var code = @"
            /* This comment has // inside it */
            var x = 10;
            // This comment has /* inside it
            var y = 20;
            x + y;
        ";
        var lexer = new ECEngine.Lexer.Lexer(code);
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();

        // Act
        var tokens = lexer.Tokenize();
        var ast = parser.Parse(code);
        var result = interpreter.Evaluate(ast, code);

        // Assert
        Assert.Equal(30.0, result);
    }
}
