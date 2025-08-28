using Xunit;
using ECEngine.Lexer;

namespace ECEngine.Tests.Lexer;

public class DebugTokenizationTests
{
    [Fact]
    public void Debug_SimpleCommentTest()
    {
        // Arrange
        var lexer = new ECEngine.Lexer.Lexer("// Comment\nvar x = 42;");

        // Act
        var tokens = lexer.Tokenize();

        // Debug: Print all tokens
        foreach (var token in tokens)
        {
            System.Console.WriteLine($"{token.Type}: '{token.Value}'");
        }

        // Assert - Let's see what we actually get
        Assert.True(tokens.Count > 0);
    }
}
