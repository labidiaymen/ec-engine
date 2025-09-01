using Xunit;
using ECEngine.Runtime;
using ECEngine.AST;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;
using System.Collections.Generic;

namespace ECEngine.Tests.Runtime
{
    public class UrlModuleTests
    {
        [Fact]
        public void URL_Constructor_Should_Be_Available()
        {
            var interpreter = new RuntimeInterpreter();
            var identifier = new Identifier("URL");
            
            var result = interpreter.Evaluate(identifier, "URL");
            
            Assert.NotNull(result);
            Assert.IsType<UrlConstructorFunction>(result);
        }

        [Fact]
        public void URLSearchParams_Constructor_Should_Be_Available()
        {
            var interpreter = new RuntimeInterpreter();
            var identifier = new Identifier("URLSearchParams");
            
            var result = interpreter.Evaluate(identifier, "URLSearchParams");
            
            Assert.NotNull(result);
            Assert.IsType<URLSearchParamsConstructorFunction>(result);
        }

        [Fact]
        public void New_URL_Should_Create_UrlClass_Object()
        {
            var interpreter = new RuntimeInterpreter();
            var constructor = new Identifier("URL");
            var args = new List<Expression> { new StringLiteral("https://example.com") };
            var newExpression = new NewExpression(constructor, args);

            var result = interpreter.Evaluate(newExpression, "new URL('https://example.com')");

            Assert.NotNull(result);
            Assert.IsType<UrlClass>(result);
        }

        [Fact]
        public void New_URLSearchParams_Should_Create_URLSearchParams_Object()
        {
            var interpreter = new RuntimeInterpreter();
            var constructor = new Identifier("URLSearchParams");
            var args = new List<Expression> { new StringLiteral("foo=bar&baz=qux") };
            var newExpression = new NewExpression(constructor, args);

            var result = interpreter.Evaluate(newExpression, "new URLSearchParams('foo=bar&baz=qux')");

            Assert.NotNull(result);
            Assert.IsType<URLSearchParams>(result);
        }

        [Fact]
        public void URL_Properties_Should_Be_Accessible()
        {
            var interpreter = new RuntimeInterpreter();
            
            // Create URL object
            var constructor = new Identifier("URL");
            var args = new List<Expression> { new StringLiteral("https://example.com:8080/path?query=value#hash") };
            var newExpression = new NewExpression(constructor, args);
            var urlObj = interpreter.Evaluate(newExpression, "") as UrlClass;
            
            Assert.NotNull(urlObj);
            Assert.Equal("https:", urlObj.Protocol);
            Assert.Equal("example.com", urlObj.Hostname);
            Assert.Equal("8080", urlObj.Port);
            Assert.Equal("/path", urlObj.Pathname);
            Assert.Equal("?query=value", urlObj.Search);
            Assert.Equal("#hash", urlObj.Hash);
        }

        [Fact]
        public void URLSearchParams_Methods_Should_Work()
        {
            var interpreter = new RuntimeInterpreter();
            
            // Create URLSearchParams object
            var constructor = new Identifier("URLSearchParams");
            var args = new List<Expression> { new StringLiteral("foo=bar") };
            var newExpression = new NewExpression(constructor, args);
            var searchParams = interpreter.Evaluate(newExpression, "") as URLSearchParams;
            
            Assert.NotNull(searchParams);
            
            // Test get method
            Assert.Equal("bar", searchParams.Get("foo"));
            
            // Test has method
            Assert.True(searchParams.Has("foo"));
            Assert.False(searchParams.Has("nonexistent"));
            
            // Test append method
            searchParams.Append("new", "value");
            Assert.True(searchParams.Has("new"));
            
            // Test set method
            searchParams.Set("foo", "newbar");
            Assert.Equal("newbar", searchParams.Get("foo"));
            
            // Test delete method
            searchParams.Delete("new");
            Assert.False(searchParams.Has("new"));
        }

        [Fact]
        public void URL_Module_Functions_Should_Work()
        {
            // Test the URL function classes directly
            var urlParseFunction = new UrlParseFunction();
            var args = new List<object?> { "https://example.com:8080/path?query=value#hash" };
            
            var result = urlParseFunction.Call(args) as Dictionary<string, object>;
            
            Assert.NotNull(result);
            Assert.Equal("https:", result["protocol"]);
            Assert.Equal("example.com", result["hostname"]);
            Assert.Equal("8080", result["port"]);
            Assert.Equal("/path", result["pathname"]);
            Assert.Equal("?query=value", result["search"]);
            Assert.Equal("#hash", result["hash"]);
            Assert.Equal(true, result["slashes"]);
        }

        [Fact]
        public void UrlParseFunction_Should_Parse_URL_Correctly()
        {
            var urlParseFunction = new UrlParseFunction();
            var args = new List<object?> { "https://example.com:8080/path?query=value#hash" };
            
            var result = urlParseFunction.Call(args) as Dictionary<string, object>;
            
            Assert.NotNull(result);
            Assert.Equal("https:", result["protocol"]);
            Assert.Equal("example.com", result["hostname"]);
            Assert.Equal("8080", result["port"]);
            Assert.Equal("/path", result["pathname"]);
            Assert.Equal("?query=value", result["search"]);
            Assert.Equal("#hash", result["hash"]);
            Assert.Equal(true, result["slashes"]);
        }

        [Fact]
        public void UrlFormatFunction_Should_Format_URL_Object()
        {
            var urlFormatFunction = new UrlFormatFunction();
            var urlObject = new Dictionary<string, object>
            {
                { "protocol", "https:" },
                { "hostname", "example.com" },
                { "pathname", "/test" },
                { "search", "?q=test" }
            };
            var args = new List<object?> { urlObject };
            
            var result = urlFormatFunction.Call(args) as string;
            
            Assert.Equal("https://example.com/test?q=test", result);
        }

        [Fact]
        public void UrlResolveFunction_Should_Resolve_Relative_URLs()
        {
            var urlResolveFunction = new UrlResolveFunction();
            var args = new List<object?> { "https://example.com/", "api/data" };
            
            var result = urlResolveFunction.Call(args) as string;
            
            Assert.Equal("https://example.com/api/data", result);
        }

        [Fact]
        public void DomainToASCII_Function_Should_Work()
        {
            var domainToASCIIFunction = new DomainToASCIIFunction();
            var args = new List<object?> { "bücher.de" };
            
            var result = domainToASCIIFunction.Call(args) as string;
            
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void DomainToUnicode_Function_Should_Work()
        {
            var domainToUnicodeFunction = new DomainToUnicodeFunction();
            var args = new List<object?> { "xn--bcher-kva.de" };
            
            var result = domainToUnicodeFunction.Call(args) as string;
            
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void URLSearchParams_ToString_Should_Return_Query_String()
        {
            var searchParams = new URLSearchParams("foo=bar&baz=qux");
            
            var result = searchParams.ToString();
            
            Assert.Contains("foo=bar", result);
            Assert.Contains("baz=qux", result);
        }

        [Fact]
        public void URL_ToString_Should_Return_Full_URL()
        {
            var url = new UrlClass("https://example.com:8080/path?query=value#hash");
            
            var result = url.ToString();
            
            Assert.Equal("https://example.com:8080/path?query=value#hash", result);
        }

        [Fact]
        public void URLSearchParams_GetAll_Should_Return_All_Values()
        {
            var searchParams = new URLSearchParams("key=value1&key=value2&other=test");
            
            var result = searchParams.GetAll("key");
            
            Assert.Equal(2, result.Count);
            Assert.Contains("value1", result);
            Assert.Contains("value2", result);
        }
    }
}
