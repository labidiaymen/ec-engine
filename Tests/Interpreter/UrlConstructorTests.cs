using Xunit;
using ECEngine.Runtime;
using ECEngine.AST;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;
using System.Collections.Generic;

namespace ECEngine.Tests.Interpreter
{
    public class UrlConstructorTests
    {
        [Fact]
        public void New_URL_Should_Create_URL_Object()
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
        public void New_URL_With_Base_Should_Work()
        {
            var interpreter = new RuntimeInterpreter();
            var constructor = new Identifier("URL");
            var args = new List<Expression> 
            { 
                new StringLiteral("/api/data"),
                new StringLiteral("https://api.example.com")
            };
            var newExpression = new NewExpression(constructor, args);

            var result = interpreter.Evaluate(newExpression, "new URL('/api/data', 'https://api.example.com')");

            Assert.NotNull(result);
            Assert.IsType<UrlClass>(result);
            var url = result as UrlClass;
            Assert.Equal("https://api.example.com/api/data", url.ToString());
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
        public void New_URLSearchParams_Empty_Should_Work()
        {
            var interpreter = new RuntimeInterpreter();
            var constructor = new Identifier("URLSearchParams");
            var args = new List<Expression>();
            var newExpression = new NewExpression(constructor, args);

            var result = interpreter.Evaluate(newExpression, "new URLSearchParams()");

            Assert.NotNull(result);
            Assert.IsType<URLSearchParams>(result);
        }

        [Fact]
        public void URL_Properties_Should_Be_Accessible()
        {
            var interpreter = new RuntimeInterpreter();
            var constructor = new Identifier("URL");
            var args = new List<Expression> { new StringLiteral("https://user:pass@example.com:8080/path?query=value#hash") };
            var newExpression = new NewExpression(constructor, args);

            var result = interpreter.Evaluate(newExpression, "") as UrlClass;

            Assert.NotNull(result);
            Assert.Equal("https:", result.Protocol);
            Assert.Equal("user", result.Username);
            Assert.Equal("pass", result.Password);
            Assert.Equal("example.com", result.Hostname);
            Assert.Equal("8080", result.Port);
            Assert.Equal("/path", result.Pathname);
            Assert.Equal("?query=value", result.Search);
            Assert.Equal("#hash", result.Hash);
        }

        [Fact]
        public void URL_Property_Assignment_Should_Update_URL()
        {
            var interpreter = new RuntimeInterpreter();
            var constructor = new Identifier("URL");
            var args = new List<Expression> { new StringLiteral("https://example.com/oldpath") };
            var newExpression = new NewExpression(constructor, args);

            var url = interpreter.Evaluate(newExpression, "") as UrlClass;
            Assert.NotNull(url);

            // Test property assignment
            url.Pathname = "/newpath";
            url.Search = "?newquery=newvalue";

            Assert.Equal("/newpath", url.Pathname);
            Assert.Equal("?newquery=newvalue", url.Search);
            Assert.Contains("/newpath", url.ToString());
            Assert.Contains("?newquery=newvalue", url.ToString());
        }

        [Fact]
        public void URLSearchParams_Methods_Should_Work()
        {
            var interpreter = new RuntimeInterpreter();
            var constructor = new Identifier("URLSearchParams");
            var args = new List<Expression> { new StringLiteral("foo=bar") };
            var newExpression = new NewExpression(constructor, args);

            var searchParams = interpreter.Evaluate(newExpression, "") as URLSearchParams;
            Assert.NotNull(searchParams);

            // Test methods
            Assert.Equal("bar", searchParams.Get("foo"));
            Assert.True(searchParams.Has("foo"));
            Assert.False(searchParams.Has("nonexistent"));

            searchParams.Append("new", "value");
            Assert.True(searchParams.Has("new"));

            searchParams.Set("foo", "newbar");
            Assert.Equal("newbar", searchParams.Get("foo"));

            searchParams.Delete("new");
            Assert.False(searchParams.Has("new"));
        }

        [Fact]
        public void URL_Constructor_Should_Throw_For_Invalid_URL()
        {
            var interpreter = new RuntimeInterpreter();
            var constructor = new Identifier("URL");
            var args = new List<Expression> { new StringLiteral("invalid-url") };
            var newExpression = new NewExpression(constructor, args);

            Assert.Throws<ECEngineException>(() =>
            {
                interpreter.Evaluate(newExpression, "new URL('invalid-url')");
            });
        }

        [Fact]
        public void URL_ToString_Should_Return_Full_URL()
        {
            var interpreter = new RuntimeInterpreter();
            var constructor = new Identifier("URL");
            var args = new List<Expression> { new StringLiteral("https://example.com:8080/path?query=value#hash") };
            var newExpression = new NewExpression(constructor, args);

            var url = interpreter.Evaluate(newExpression, "") as UrlClass;
            Assert.NotNull(url);

            var result = url.ToString();
            Assert.Equal("https://example.com:8080/path?query=value#hash", result);
        }

        [Fact]
        public void URLSearchParams_ToString_Should_Return_Query_String()
        {
            var interpreter = new RuntimeInterpreter();
            var constructor = new Identifier("URLSearchParams");
            var args = new List<Expression> { new StringLiteral("foo=bar&baz=qux") };
            var newExpression = new NewExpression(constructor, args);

            var searchParams = interpreter.Evaluate(newExpression, "") as URLSearchParams;
            Assert.NotNull(searchParams);

            var result = searchParams.ToString();
            Assert.Contains("foo=bar", result);
            Assert.Contains("baz=qux", result);
        }
    }
}
