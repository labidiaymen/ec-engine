using Xunit;
using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.AST;
using ECEngine.Runtime;

namespace ECEngine.Tests;

public class EngineTests
{
    [Fact]
    public void Lexer_Should_Tokenize_Basic_Expression()
    {
        var lexer = new Lexer("1 + 2");
        var tokens = lexer.Tokenize();
        Assert.NotNull(tokens);
    }

    [Fact]
    public void Parser_Should_Return_ASTNode()
    {
        var parser = new Parser.Parser();
        var ast = parser.Parse("1 + 2");
        Assert.NotNull(ast);
    }
}
