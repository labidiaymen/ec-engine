using Xunit;
using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.Runtime;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Runtime;

/// <summary>
/// Tests for string methods implementation
/// </summary>
[Collection("Console Tests")]
public class StringMethodsTests
{
    private readonly RuntimeInterpreter _interpreter;

    public StringMethodsTests()
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
    public void TestStringLength()
    {
        var code = """
            var str = "Hello World";
            var length = str.length;
            """;

        ExecuteCode(code);
        var length = _interpreter.GetVariable("length");
        
        Assert.Equal(11.0, length);
    }

    [Fact]
    public void TestCharAt()
    {
        var code = """
            var str = "Hello";
            var first = str.charAt(0);
            var last = str.charAt(4);
            var outOfBounds = str.charAt(10);
            """;

        ExecuteCode(code);
        
        Assert.Equal("H", _interpreter.GetVariable("first"));
        Assert.Equal("o", _interpreter.GetVariable("last"));
        Assert.Equal("", _interpreter.GetVariable("outOfBounds"));
    }

    [Fact]
    public void TestCharCodeAt()
    {
        var code = """
            var str = "ABC";
            var codeA = str.charCodeAt(0);
            var codeB = str.charCodeAt(1);
            var outOfBounds = str.charCodeAt(10);
            """;

        ExecuteCode(code);
        
        Assert.Equal(65.0, _interpreter.GetVariable("codeA"));
        Assert.Equal(66.0, _interpreter.GetVariable("codeB"));
        Assert.True(double.IsNaN((double)_interpreter.GetVariable("outOfBounds")));
    }

    [Fact]
    public void TestIndexOf()
    {
        var code = """
            var str = "Hello World Hello";
            var first = str.indexOf("Hello");
            var second = str.indexOf("Hello", 1);
            var notFound = str.indexOf("xyz");
            """;

        ExecuteCode(code);
        
        Assert.Equal(0.0, _interpreter.GetVariable("first"));
        Assert.Equal(12.0, _interpreter.GetVariable("second"));
        Assert.Equal(-1.0, _interpreter.GetVariable("notFound"));
    }

    [Fact]
    public void TestLastIndexOf()
    {
        var code = """
            var str = "Hello World Hello";
            var last = str.lastIndexOf("Hello");
            var fromPos = str.lastIndexOf("Hello", 10);
            var notFound = str.lastIndexOf("xyz");
            """;

        ExecuteCode(code);
        
        Assert.Equal(12.0, _interpreter.GetVariable("last"));
        Assert.Equal(0.0, _interpreter.GetVariable("fromPos"));
        Assert.Equal(-1.0, _interpreter.GetVariable("notFound"));
    }

    [Fact]
    public void TestIncludes()
    {
        var code = """
            var str = "Hello World";
            var hasHello = str.includes("Hello");
            var hasWorld = str.includes("World");
            var hasXyz = str.includes("xyz");
            """;

        ExecuteCode(code);
        
        Assert.True((bool)_interpreter.GetVariable("hasHello"));
        Assert.True((bool)_interpreter.GetVariable("hasWorld"));
        Assert.False((bool)_interpreter.GetVariable("hasXyz"));
    }

    [Fact]
    public void TestStartsWithEndsWith()
    {
        var code = """
            var str = "Hello World";
            var startsHello = str.startsWith("Hello");
            var startsWorld = str.startsWith("World");
            var endsWorld = str.endsWith("World");
            var endsHello = str.endsWith("Hello");
            """;

        ExecuteCode(code);
        
        Assert.True((bool)_interpreter.GetVariable("startsHello"));
        Assert.False((bool)_interpreter.GetVariable("startsWorld"));
        Assert.True((bool)_interpreter.GetVariable("endsWorld"));
        Assert.False((bool)_interpreter.GetVariable("endsHello"));
    }

    [Fact]
    public void TestSlice()
    {
        var code = """
            var str = "Hello World";
            var hello = str.slice(0, 5);
            var world = str.slice(6);
            var negative = str.slice(-5);
            """;

        ExecuteCode(code);
        
        Assert.Equal("Hello", _interpreter.GetVariable("hello"));
        Assert.Equal("World", _interpreter.GetVariable("world"));
        Assert.Equal("World", _interpreter.GetVariable("negative"));
    }

    [Fact]
    public void TestSubstring()
    {
        var code = """
            var str = "Hello World";
            var hello = str.substring(0, 5);
            var world = str.substring(6, 11);
            var swapped = str.substring(5, 0);
            """;

        ExecuteCode(code);
        
        Assert.Equal("Hello", _interpreter.GetVariable("hello"));
        Assert.Equal("World", _interpreter.GetVariable("world"));
        Assert.Equal("Hello", _interpreter.GetVariable("swapped"));
    }

    [Fact]
    public void TestCaseMethods()
    {
        var code = """
            var str = "Hello World";
            var lower = str.toLowerCase();
            var upper = str.toUpperCase();
            """;

        ExecuteCode(code);
        
        Assert.Equal("hello world", _interpreter.GetVariable("lower"));
        Assert.Equal("HELLO WORLD", _interpreter.GetVariable("upper"));
    }

    [Fact]
    public void TestConcat()
    {
        var code = """
            var str = "Hello";
            var result = str.concat(" ", "World", "!");
            """;

        ExecuteCode(code);
        
        Assert.Equal("Hello World!", _interpreter.GetVariable("result"));
    }

    [Fact]
    public void TestRepeat()
    {
        var code = """
            var str = "Hi";
            var repeated = str.repeat(3);
            var zero = str.repeat(0);
            """;

        ExecuteCode(code);
        
        Assert.Equal("HiHiHi", _interpreter.GetVariable("repeated"));
        Assert.Equal("", _interpreter.GetVariable("zero"));
    }

    [Fact]
    public void TestPadMethods()
    {
        var code = """
            var str = "Hello";
            var padded = str.padStart(10, "*");
            var paddedEnd = str.padEnd(10, "*");
            """;

        ExecuteCode(code);
        
        Assert.Equal("*****Hello", _interpreter.GetVariable("padded"));
        Assert.Equal("Hello*****", _interpreter.GetVariable("paddedEnd"));
    }

    [Fact]
    public void TestTrimMethods()
    {
        var code = """
            var str = "  Hello World  ";
            var trimmed = str.trim();
            var trimStart = str.trimStart();
            var trimEnd = str.trimEnd();
            """;

        ExecuteCode(code);
        
        Assert.Equal("Hello World", _interpreter.GetVariable("trimmed"));
        Assert.Equal("Hello World  ", _interpreter.GetVariable("trimStart"));
        Assert.Equal("  Hello World", _interpreter.GetVariable("trimEnd"));
    }

    [Fact]
    public void TestReplace()
    {
        var code = """
            var str = "Hello World Hello";
            var replaced = str.replace("Hello", "Hi");
            var replaceAll = str.replaceAll("Hello", "Hi");
            """;

        ExecuteCode(code);
        
        Assert.Equal("Hi World Hello", _interpreter.GetVariable("replaced"));
        Assert.Equal("Hi World Hi", _interpreter.GetVariable("replaceAll"));
    }

    [Fact]
    public void TestSplit()
    {
        var code = """
            var str = "Hello World";
            var words = str.split(" ");
            var chars = str.split("");
            var whole = str.split(null);
            """;

        ExecuteCode(code);
        
        var words = (List<object?>)_interpreter.GetVariable("words");
        var chars = (List<object?>)_interpreter.GetVariable("chars");
        var whole = (List<object?>)_interpreter.GetVariable("whole");
        
        Assert.Equal(2, words.Count);
        Assert.Equal("Hello", words[0]);
        Assert.Equal("World", words[1]);
        
        Assert.Equal(11, chars.Count);
        Assert.Equal("H", chars[0]);
        Assert.Equal(" ", chars[5]);
        
        Assert.Single(whole);
        Assert.Equal("Hello World", whole[0]);
    }

    [Fact]
    public void TestStringStaticMethods()
    {
        var code = """
            var fromCharCode = String.fromCharCode(72, 101, 108, 108, 111);
            var fromCodePoint = String.fromCodePoint(8364);
            """;

        ExecuteCode(code);
        
        Assert.Equal("Hello", _interpreter.GetVariable("fromCharCode"));
        Assert.Equal("€", _interpreter.GetVariable("fromCodePoint"));
    }

    [Fact]
    public void TestStringConstructor()
    {
        var code = """
            var str1 = String();
            var str2 = String("hello");
            var str3 = String(123);
            var str4 = String(true);
            var str5 = String(null);
            """;

        ExecuteCode(code);
        
        Assert.Equal("", _interpreter.GetVariable("str1"));
        Assert.Equal("hello", _interpreter.GetVariable("str2"));
        Assert.Equal("123", _interpreter.GetVariable("str3"));
        Assert.Equal("true", _interpreter.GetVariable("str4"));
        Assert.Equal("null", _interpreter.GetVariable("str5"));
    }

    [Fact]
    public void TestStringAt()
    {
        var code = """
            var str = "Hello";
            var first = str.at(0);
            var last = str.at(-1);
            var outOfBounds = str.at(10);
            """;

        ExecuteCode(code);
        
        Assert.Equal("H", _interpreter.GetVariable("first"));
        Assert.Equal("o", _interpreter.GetVariable("last"));
        Assert.Null(_interpreter.GetVariable("outOfBounds"));
    }

    [Fact]
    public void TestStringNormalization()
    {
        var code = """
            var str = "café";
            var normalized = str.normalize();
            var isWellFormed = str.isWellFormed();
            var wellFormed = str.toWellFormed();
            """;

        ExecuteCode(code);
        
        Assert.Equal("café", _interpreter.GetVariable("normalized"));
        Assert.True((bool)_interpreter.GetVariable("isWellFormed"));
        Assert.Equal("café", _interpreter.GetVariable("wellFormed"));
    }

    [Fact]
    public void TestHtmlWrapperMethods()
    {
        var code = """
            var str = "Hello";
            var bold = str.bold();
            var italics = str.italics();
            var link = str.link("http://example.com");
            """;

        ExecuteCode(code);
        
        Assert.Equal("<b>Hello</b>", _interpreter.GetVariable("bold"));
        Assert.Equal("<i>Hello</i>", _interpreter.GetVariable("italics"));
        Assert.Equal("<a href=\"http://example.com\">Hello</a>", _interpreter.GetVariable("link"));
    }
}
