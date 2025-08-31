using Xunit;
using ECEngine.Runtime;
using ECEngine.Lexer;
using ECEngine.Parser;

namespace ECEngine.Tests.Interpreter
{
    public class ArrowFunctionTests
    {
        [Fact]
        public void ArrowFunction_ExpressionBody_WithMultipleParameters_WorksCorrectly()
        {
            var code = @"
                var add = (a, b) => a + b;
                add(5, 3);
            ";

            var result = ExecuteCode(code);
            Assert.Equal(8.0, result);
        }

        [Fact]
        public void ArrowFunction_ExpressionBody_WithSingleParameter_WorksCorrectly()
        {
            var code = @"
                var square = x => x * x;
                square(4);
            ";

            var result = ExecuteCode(code);
            Assert.Equal(16.0, result);
        }

        [Fact]
        public void ArrowFunction_ExpressionBody_WithNoParameters_WorksCorrectly()
        {
            var code = @"
                var getMessage = () => 'Hello World';
                getMessage();
            ";

            var result = ExecuteCode(code);
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void ArrowFunction_BlockBody_WithReturn_WorksCorrectly()
        {
            var code = @"
                var multiply = (a, b) => {
                    var result = a * b;
                    return result;
                };
                multiply(6, 7);
            ";

            var result = ExecuteCode(code);
            Assert.Equal(42.0, result);
        }

        [Fact]
        public void ArrowFunction_BlockBody_WithMultipleStatements_WorksCorrectly()
        {
            var code = @"
                var calculate = (x, y) => {
                    var sum = x + y;
                    var product = x * y;
                    return sum + product;
                };
                calculate(3, 4);
            ";

            var result = ExecuteCode(code);
            Assert.Equal(19.0, result); // (3 + 4) + (3 * 4) = 7 + 12 = 19
        }

        [Fact]
        public void ArrowFunction_NestedArrowFunctions_WorksCorrectly()
        {
            var code = @"
                var createAdder = x => y => x + y;
                var add10 = createAdder(10);
                add10(5);
            ";

            var result = ExecuteCode(code);
            Assert.Equal(15.0, result);
        }

        [Fact]
        public void ArrowFunction_WithConditionalExpression_WorksCorrectly()
        {
            var code = @"
                var isGreater = (a, b) => a > b;
                isGreater(8, 3);
            ";

            var result = ExecuteCode(code);
            Assert.Equal(true, result);
        }

        [Fact]
        public void ArrowFunction_WithStringConcatenation_WorksCorrectly()
        {
            var code = @"
                var greet = name => 'Hello, ' + name + '!';
                greet('Alice');
            ";

            var result = ExecuteCode(code);
            Assert.Equal("Hello, Alice!", result);
        }

        [Fact]
        public void ArrowFunction_AsCallback_WorksCorrectly()
        {
            var code = @"
                function applyOperation(a, b, operation) {
                    return operation(a, b);
                }
                
                var subtract = (x, y) => x - y;
                applyOperation(10, 4, subtract);
            ";

            var result = ExecuteCode(code);
            Assert.Equal(6.0, result);
        }

        [Fact]
        public void ArrowFunction_WithObjectReturn_WorksCorrectly()
        {
            var code = @"
                var createPerson = name => {
                    return {
                        name: name,
                        greet: function() {
                            return 'Hi, I am ' + this.name;
                        }
                    };
                };
                
                var person = createPerson('Bob');
                person.greet();
            ";

            var result = ExecuteCode(code);
            Assert.Equal("Hi, I am Bob", result);
        }

        [Fact]
        public void ArrowFunction_WithClosure_WorksCorrectly()
        {
            var code = @"
                function createCounter() {
                    var count = 0;
                    return () => {
                        count = count + 1;
                        return count;
                    };
                }
                
                var counter = createCounter();
                counter();
                counter();
            ";

            var result = ExecuteCode(code);
            Assert.Equal(2.0, result);
        }

        [Fact]
        public void ArrowFunction_ComplexNesting_WorksCorrectly()
        {
            var code = @"
                var createMath = () => {
                    return {
                        add: (a, b) => a + b,
                        multiply: (a, b) => a * b,
                        calculate: (x, y) => {
                            var sum = x + y;
                            var product = x * y;
                            return sum + product;
                        }
                    };
                };
                
                var math = createMath();
                math.calculate(2, 3);
            ";

            var result = ExecuteCode(code);
            Assert.Equal(11.0, result); // (2 + 3) + (2 * 3) = 5 + 6 = 11
        }

        [Fact]
        public void ArrowFunction_WithThisKeyword_DoesNotBindThis()
        {
            var code = @"
                var obj = {
                    name: 'Test Object',
                    regularFunction: function() {
                        return this.name;
                    },
                    getValue: function() {
                        return this.name;
                    }
                };
                
                obj.getValue();
            ";

            var result = ExecuteCode(code);
            Assert.Equal("Test Object", result);
        }

        [Fact]
        public void ArrowFunction_MultipleArrowsInExpression_WorksCorrectly()
        {
            var code = @"
                var triple = x => y => z => x + y + z;
                var partiallyApplied = triple(1)(2);
                partiallyApplied(3);
            ";

            var result = ExecuteCode(code);
            Assert.Equal(6.0, result);
        }

        [Fact]
        public void ArrowFunction_MixedWithRegularFunctions_WorksCorrectly()
        {
            var code = @"
                function regularFunc(callback) {
                    return callback(5);
                }
                
                var arrowFunc = x => x * 2;
                regularFunc(arrowFunc);
            ";

            var result = ExecuteCode(code);
            Assert.Equal(10.0, result);
        }

        private static object? ExecuteCode(string code)
        {
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();

            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);

            var interpreter = new ECEngine.Runtime.Interpreter();
            return interpreter.Evaluate(ast, code);
        }
    }
}
