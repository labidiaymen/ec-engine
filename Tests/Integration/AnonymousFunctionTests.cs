using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.AST;
using ECEngine.Runtime;
using Xunit;

namespace ECEngine.Tests.Integration;

public class AnonymousFunctionTests
{
    [Fact]
    public void ShouldAssignAnonymousFunctionToVariable()
    {
        var code = @"
            var add = function(a, b) { return a + b; };
            var result = add(3, 5);
        ";
        
        var lexer = new ECEngine.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();
        var parser = new ECEngine.Parser.Parser();
        var ast = parser.Parse(code);
        var interpreter = new ECEngine.Runtime.Interpreter();
        
        interpreter.Evaluate(ast, code);
        
        Assert.True(interpreter.Variables.ContainsKey("add"));
        Assert.True(interpreter.Variables.ContainsKey("result"));
        Assert.Equal(8.0, interpreter.Variables["result"].Value);
        
        var addFunction = interpreter.Variables["add"].Value as Function;
        Assert.NotNull(addFunction);
        Assert.Null(addFunction.Name); // Should be null for anonymous functions
    }

    [Fact]
    public void ShouldPassAnonymousFunctionAsArgument()
    {
        var code = @"
            function applyOperation(a, b, operation) {
                return operation(a, b);
            }
            var result = applyOperation(10, 3, function(x, y) { return x - y; });
        ";
        
        var lexer = new ECEngine.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();
        var parser = new ECEngine.Parser.Parser();
        var ast = parser.Parse(code);
        var interpreter = new ECEngine.Runtime.Interpreter();
        
        interpreter.Evaluate(ast, code);
        
        Assert.True(interpreter.Variables.ContainsKey("applyOperation"));
        Assert.True(interpreter.Variables.ContainsKey("result"));
        Assert.Equal(7.0, interpreter.Variables["result"].Value);
    }

    [Fact]
    public void ShouldHandleAnonymousFunctionWithoutParameters()
    {
        var code = @"
            var getValue = function() { return 42; };
            var result = getValue();
        ";
        
        var lexer = new ECEngine.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();
        var parser = new ECEngine.Parser.Parser();
        var ast = parser.Parse(code);
        var interpreter = new ECEngine.Runtime.Interpreter();
        
        interpreter.Evaluate(ast, code);
        
        Assert.True(interpreter.Variables.ContainsKey("getValue"));
        Assert.True(interpreter.Variables.ContainsKey("result"));
        Assert.Equal(42.0, interpreter.Variables["result"].Value);
    }

    [Fact]
    public void ShouldHandleNestedAnonymousFunctions()
    {
        var code = @"
            var outer = function(x) {
                return function(y) {
                    return x + y;
                };
            };
            var inner = outer(5);
            var result = inner(3);
        ";
        
        var lexer = new ECEngine.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();
        var parser = new ECEngine.Parser.Parser();
        var ast = parser.Parse(code);
        var interpreter = new ECEngine.Runtime.Interpreter();
        
        interpreter.Evaluate(ast, code);
        
        Assert.True(interpreter.Variables.ContainsKey("outer"));
        Assert.True(interpreter.Variables.ContainsKey("inner"));
        Assert.True(interpreter.Variables.ContainsKey("result"));
        Assert.Equal(8.0, interpreter.Variables["result"].Value);
    }
}
