using Xunit;
using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.Runtime;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Runtime;

/// <summary>
/// Tests for method chaining functionality
/// </summary>
[Collection("Console Tests")]
public class MethodChainingTests
{
    private readonly RuntimeInterpreter _interpreter;

    public MethodChainingTests()
    {
        _interpreter = new RuntimeInterpreter();
    }

    private void ExecuteCode(string code)
    {
        var lexer = new ECEngine.Lexer.Lexer(code);
        var parser = new ECEngine.Parser.Parser();
        var ast = parser.Parse(code);
        _interpreter.Evaluate(ast, code);
    }

    [Fact]
    public void TestStringMethodChaining()
    {
        var code = """
            var text = "  Hello World  ";
            var result = text.trim().toUpperCase();
            """;

        ExecuteCode(code);
        var result = _interpreter.GetVariable("result");
        
        Assert.Equal("HELLO WORLD", result);
    }

    [Fact]
    public void TestComplexStringChaining()
    {
        var code = """
            var text = "  JavaScript is AWESOME  ";
            var result = text.trim().toLowerCase().replace("javascript", "ECEngine").toUpperCase();
            """;

        ExecuteCode(code);
        var result = _interpreter.GetVariable("result");
        
        Assert.Equal("ECENGINE IS AWESOME", result);
    }

    [Fact]
    public void TestArrayMethodChaining()
    {
        var code = """
            var text = "Hello ECEngine World";
            var result = text.split(" ").slice(0, 2);
            """;

        ExecuteCode(code);
        var result = (List<object?>)_interpreter.GetVariable("result");
        
        Assert.Equal(2, result.Count);
        Assert.Equal("Hello", result[0]);
        Assert.Equal("ECEngine", result[1]);
    }

    [Fact]
    public void TestMixedChaining()
    {
        var code = """
            var numbers = [1, 2, 3, 4, 5];
            var sliced = numbers.slice(1, 4);
            var joined = sliced.join("-");
            """;

        ExecuteCode(code);
        var sliced = (List<object?>)_interpreter.GetVariable("sliced");
        var joined = _interpreter.GetVariable("joined");
        
        Assert.Equal(3, sliced.Count);
        Assert.Equal("2-3-4", joined);
    }

    [Fact]
    public void TestStringReplaceChaining()
    {
        var code = """
            var text = "Hello World Hello";
            var result = text.replace("Hello", "Hi").replace("World", "Universe");
            """;

        ExecuteCode(code);
        var result = _interpreter.GetVariable("result");
        
        Assert.Equal("Hi Universe Hello", result);
    }

    [Fact]
    public void TestStringPaddingChaining()
    {
        var code = """
            var num = "42";
            var result = num.padStart(5, "0").padEnd(8, ".");
            """;

        ExecuteCode(code);
        var result = _interpreter.GetVariable("result");
        
        Assert.Equal("00042...", result);
    }
}
