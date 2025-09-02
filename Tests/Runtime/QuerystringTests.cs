using Xunit;
using ECEngine.Runtime;
using ECEngine.Parser;
using ECEngine.Lexer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Runtime;

public class QuerystringTests
{
    private RuntimeInterpreter GetInterpreter()
    {
        var interpreter = new RuntimeInterpreter();
        var moduleSystem = new ModuleSystem();
        interpreter.SetModuleSystem(moduleSystem);
        return interpreter;
    }

    private object? EvaluateCode(string code)
    {
        var interpreter = GetInterpreter();
        var parser = new ECEngine.Parser.Parser();
        var ast = parser.Parse(code);
        return interpreter.Evaluate(ast, code);
    }

    [Fact]
    public void QuerystringParse_BasicString_ReturnsCorrectObject()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const result = querystring.parse('name=John&age=30&city=New York');
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        Assert.Equal("John", dict["name"]);
        Assert.Equal("30", dict["age"]);
        Assert.Equal("New York", dict["city"]);
    }

    [Fact]
    public void QuerystringParse_MultipleValues_ReturnsArray()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const result = querystring.parse('color=red&color=blue&color=green');
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        var colorList = Assert.IsType<List<object?>>(dict["color"]);
        Assert.Equal(3, colorList.Count);
        Assert.Equal("red", colorList[0]);
        Assert.Equal("blue", colorList[1]);
        Assert.Equal("green", colorList[2]);
    }

    [Fact]
    public void QuerystringParse_CustomSeparators_ParsesCorrectly()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const result = querystring.parse('a=1;b=2;c=3', ';');
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        Assert.Equal("1", dict["a"]);
        Assert.Equal("2", dict["b"]);
        Assert.Equal("3", dict["c"]);
    }

    [Fact]
    public void QuerystringParse_CustomSeparatorsAndEquals_ParsesCorrectly()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const result = querystring.parse('a:1;b:2;c:3', ';', ':');
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        Assert.Equal("1", dict["a"]);
        Assert.Equal("2", dict["b"]);
        Assert.Equal("3", dict["c"]);
    }

    [Fact]
    public void QuerystringParse_EmptyString_ReturnsEmptyObject()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const result = querystring.parse('');
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        Assert.Empty(dict);
    }

    [Fact]
    public void QuerystringParse_ValueWithoutEquals_SetsEmptyValue()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const result = querystring.parse('foo=bar&baz');
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        Assert.Equal("bar", dict["foo"]);
        Assert.Equal("", dict["baz"]);
    }

    [Fact]
    public void QuerystringStringify_BasicObject_ReturnsCorrectString()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const obj = { name: 'John', age: 30, city: 'New York' };
            const result = querystring.stringify(obj);
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var str = Assert.IsType<string>(result);
        Assert.Contains("name=John", str);
        Assert.Contains("age=30", str);
        Assert.Contains("city=New+York", str); // URL encoded
    }

    [Fact]
    public void QuerystringStringify_ArrayValues_RepeatsKey()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const obj = { color: ['red', 'blue', 'green'] };
            const result = querystring.stringify(obj);
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var str = Assert.IsType<string>(result);
        Assert.Contains("color=red", str);
        Assert.Contains("color=blue", str);
        Assert.Contains("color=green", str);
    }

    [Fact]
    public void QuerystringStringify_CustomSeparators_UsesCustomSeparators()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const obj = { x: 'hello', y: 'world' };
            const result = querystring.stringify(obj, ';', ':');
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var str = Assert.IsType<string>(result);
        Assert.Contains("x:hello", str);
        Assert.Contains("y:world", str);
        Assert.Contains(";", str);
    }

    [Fact]
    public void QuerystringStringify_NullObject_ReturnsEmptyString()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const result = querystring.stringify(null);
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var str = Assert.IsType<string>(result);
        Assert.Equal("", str);
    }

    [Fact]
    public void QuerystringEscape_SpecialCharacters_EncodesCorrectly()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const result = querystring.escape('hello world & foo=bar');
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var str = Assert.IsType<string>(result);
        Assert.Contains("hello+world", str);
        Assert.Contains("%26", str); // & encoded
        Assert.Contains("%3d", str); // = encoded
    }

    [Fact]
    public void QuerystringUnescape_EncodedString_DecodesCorrectly()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const escaped = querystring.escape('hello world & foo=bar');
            const result = querystring.unescape(escaped);
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var str = Assert.IsType<string>(result);
        Assert.Equal("hello world & foo=bar", str);
    }

    [Fact]
    public void QuerystringEscape_EmptyString_ReturnsEmptyString()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const result = querystring.escape('');
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var str = Assert.IsType<string>(result);
        Assert.Equal("", str);
    }

    [Fact]
    public void QuerystringUnescape_EmptyString_ReturnsEmptyString()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const result = querystring.unescape('');
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var str = Assert.IsType<string>(result);
        Assert.Equal("", str);
    }

    [Fact]
    public void QuerystringModule_ExportsCorrectMethods()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const hasAllMethods = 
                typeof querystring.parse === 'function' &&
                typeof querystring.stringify === 'function' &&
                typeof querystring.escape === 'function' &&
                typeof querystring.unescape === 'function';
            hasAllMethods;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var hasAllMethods = Assert.IsType<bool>(result);
        Assert.True(hasAllMethods, "Querystring module should export parse, stringify, escape, and unescape methods");
    }

    [Fact]
    public void QuerystringParse_WithMaxKeysOption_LimitsKeys()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const result = querystring.parse('a=1&b=2&c=3&d=4&e=5', '&', '=', { maxKeys: 3 });
            result;
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        Assert.True(dict.Count <= 3, $"Expected at most 3 keys, but got {dict.Count}");
    }

    [Fact]
    public void QuerystringParse_RoundTrip_PreservesData()
    {
        // Arrange
        var code = @"
            const querystring = require('querystring');
            const original = { name: 'John Doe', age: 30, colors: ['red', 'blue'] };
            const stringified = querystring.stringify(original);
            const parsed = querystring.parse(stringified);
            [stringified, parsed];
        ";

        // Act
        var result = EvaluateCode(code);

        // Assert
        Assert.NotNull(result);
        var array = Assert.IsType<List<object?>>(result);
        Assert.Equal(2, array.Count);
        
        var stringified = Assert.IsType<string>(array[0]);
        var parsed = Assert.IsType<Dictionary<string, object?>>(array[1]);
        
        Assert.Contains("name=John+Doe", stringified);
        Assert.Equal("John Doe", parsed["name"]);
    }
}
