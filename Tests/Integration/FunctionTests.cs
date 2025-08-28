using Xunit;
using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.Runtime;

namespace ECEngine.Tests.Integration
{
    public class FunctionTests
    {
        [Fact]
        public void ShouldDeclareAndCallSimpleFunction()
        {
            var code = @"
                function greet() {
                    return ""Hello World"";
                }
                var result = greet();
            ";
            
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            var interpreter = new ECEngine.Runtime.Interpreter();
            
            interpreter.Evaluate(ast, code);
            
            Assert.True(interpreter.Variables.ContainsKey("greet"));
            Assert.True(interpreter.Variables.ContainsKey("result"));
            Assert.Equal("Hello World", interpreter.Variables["result"].Value);
        }

        [Fact]
        public void ShouldHandleFunctionWithParameters()
        {
            var code = @"
                function add(a, b) {
                    return a + b;
                }
                var sum = add(5, 3);
            ";
            
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            var interpreter = new ECEngine.Runtime.Interpreter();
            
            interpreter.Evaluate(ast, code);
            
            Assert.True(interpreter.Variables.ContainsKey("add"));
            Assert.True(interpreter.Variables.ContainsKey("sum"));
            Assert.Equal(8.0, interpreter.Variables["sum"].Value);
        }

        [Fact]
        public void ShouldHandleFunctionWithoutReturn()
        {
            var code = @"
                function doSomething() {
                    var x = 42;
                }
                var result = doSomething();
            ";
            
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            var interpreter = new ECEngine.Runtime.Interpreter();
            
            interpreter.Evaluate(ast, code);
            
            Assert.True(interpreter.Variables.ContainsKey("doSomething"));
            Assert.True(interpreter.Variables.ContainsKey("result"));
            Assert.Null(interpreter.Variables["result"].Value);
        }

        [Fact]
        public void ShouldHandleNestedFunctionCalls()
        {
            var code = @"
                function multiply(a, b) {
                    return a * b;
                }
                function square(x) {
                    return multiply(x, x);
                }
                var result = square(4);
            ";
            
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            var interpreter = new ECEngine.Runtime.Interpreter();
            
            interpreter.Evaluate(ast, code);
            
            Assert.Equal(16.0, interpreter.Variables["result"].Value);
        }

        [Fact]
        public void ShouldHandleFunctionScope()
        {
            var code = @"
                var globalVar = 10;
                function test() {
                    var localVar = 20;
                    return globalVar + localVar;
                }
                var result = test();
            ";
            
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            var interpreter = new ECEngine.Runtime.Interpreter();
            
            interpreter.Evaluate(ast, code);
            
            Assert.Equal(30.0, interpreter.Variables["result"].Value);
            // localVar should not exist in global scope
            Assert.False(interpreter.Variables.ContainsKey("localVar"));
        }
    }
}
