using Xunit;
using ECEngine.Runtime;

namespace ECEngine.Tests.Interpreter
{
    public class GeneratorFunctionTests
    {
        private static object? ExecuteCode(string code)
        {
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();

            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);

            var interpreter = new ECEngine.Runtime.Interpreter();
            return interpreter.Evaluate(ast, code);
        }

        [Fact]
        public void GeneratorFunction_BasicDeclaration_CreatesGeneratorFunction()
        {
            var code = @"
                function* simpleGenerator() {
                    yield 1;
                }
                simpleGenerator;
            ";

            var result = ExecuteCode(code);
            Assert.IsType<GeneratorFunction>(result);
        }

        [Fact]
        public void GeneratorFunction_CallingGeneratorFunction_ReturnsGenerator()
        {
            var code = @"
                function* simpleGenerator() {
                    yield 1;
                }
                simpleGenerator();
            ";

            var result = ExecuteCode(code);
            Assert.IsType<Generator>(result);
        }

        [Fact]
        public void GeneratorFunction_BasicYield_ReturnsCorrectValue()
        {
            var code = @"
                function* simpleGenerator() {
                    yield 42;
                }
                var gen = simpleGenerator();
                gen.next();
            ";

            var result = ExecuteCode(code);
            Assert.IsType<Dictionary<string, object?>>(result);
            var dict = (Dictionary<string, object?>)result;
            Assert.Equal(42.0, dict["value"]);
            Assert.Equal(false, dict["done"]);
        }

        [Fact]
        public void GeneratorFunction_MultipleYields_ReturnsSequentialValues()
        {
            var code = @"
                function* multipleYields() {
                    yield 1;
                    yield 2;
                    yield 3;
                }
                var gen = multipleYields();
                var result1 = gen.next();
                var result2 = gen.next();
                var result3 = gen.next();
                [result1.value, result2.value, result3.value];
            ";

            var result = ExecuteCode(code);
            Assert.IsType<List<object?>>(result);
            var list = (List<object?>)result;
            Assert.Equal(1.0, list[0]);
            Assert.Equal(2.0, list[1]);
            Assert.Equal(3.0, list[2]);
        }

        [Fact]
        public void GeneratorFunction_WithParameters_UsesParametersCorrectly()
        {
            var code = @"
                function* withParameters(start, increment) {
                    yield start;
                    yield start + increment;
                }
                var gen = withParameters(10, 5);
                var result1 = gen.next();
                var result2 = gen.next();
                [result1.value, result2.value];
            ";

            var result = ExecuteCode(code);
            Assert.IsType<List<object?>>(result);
            var list = (List<object?>)result;
            Assert.Equal(10.0, list[0]);
            Assert.Equal(15.0, list[1]);
        }

        [Fact]
        public void GeneratorFunction_WithReturn_EndsGeneratorWithReturnValue()
        {
            var code = @"
                function* withReturn() {
                    yield ""first"";
                    return ""done"";
                    yield ""unreachable"";
                }
                var gen = withReturn();
                var first = gen.next();
                var second = gen.next();
                [first.value, first.done, second.value, second.done];
            ";

            var result = ExecuteCode(code);
            Assert.IsType<List<object?>>(result);
            var list = (List<object?>)result;
            Assert.Equal("first", list[0]);
            Assert.Equal(false, list[1]);
            Assert.Equal("done", list[2]);
            Assert.Equal(true, list[3]);
        }

        [Fact]
        public void GeneratorFunction_ExhaustedGenerator_ReturnsDoneTrue()
        {
            var code = @"
                function* shortGenerator() {
                    yield 1;
                }
                var gen = shortGenerator();
                gen.next(); // Consume the only value
                var afterExhausted = gen.next();
                [afterExhausted.value, afterExhausted.done];
            ";

            var result = ExecuteCode(code);
            Assert.IsType<List<object?>>(result);
            var list = (List<object?>)result;
            Assert.Null(list[0]); // value should be null/undefined
            Assert.Equal(true, list[1]); // done should be true
        }

        [Fact]
        public void GeneratorFunction_YieldExpressions_ReturnsCorrectValues()
        {
            var code = @"
                function* expressionYields() {
                    yield 5 + 3;
                    yield ""hello"" + "" world"";
                }
                var gen = expressionYields();
                var result1 = gen.next();
                var result2 = gen.next();
                [result1.value, result2.value];
            ";

            var result = ExecuteCode(code);
            Assert.IsType<List<object?>>(result);
            var list = (List<object?>)result;
            Assert.Equal(8.0, list[0]);
            Assert.Equal("hello world", list[1]);
        }

        [Fact]
        public void GeneratorFunction_YieldUndefined_ReturnsNull()
        {
            var code = @"
                function* yieldUndefined() {
                    yield;
                }
                var gen = yieldUndefined();
                var result = gen.next();
                result.value;
            ";

            var result = ExecuteCode(code);
            Assert.Null(result);
        }

        [Fact]
        public void GeneratorFunction_AsObjectMethod_WorksCorrectly()
        {
            var code = @"
                function* generator() {
                    yield ""from object"";
                }
                var obj = {
                    generator: generator
                };
                var gen = obj.generator();
                var result = gen.next();
                result.value;
            ";

            var result = ExecuteCode(code);
            Assert.Equal("from object", result);
        }

        [Fact]
        public void GeneratorFunction_Expression_WorksCorrectly()
        {
            var code = @"
                var genFunc = function*() {
                    yield ""anonymous generator"";
                };
                var gen = genFunc();
                var result = gen.next();
                result.value;
            ";

            var result = ExecuteCode(code);
            Assert.Equal("anonymous generator", result);
        }

        [Fact]
        public void GeneratorFunction_WithVariables_MaintainsState()
        {
            var code = @"
                function* statefulGenerator() {
                    var count = 0;
                    yield count;
                    count = count + 1;
                    yield count;
                    count = count + 1;
                    yield count;
                }
                var gen = statefulGenerator();
                var result1 = gen.next();
                var result2 = gen.next();
                var result3 = gen.next();
                [result1.value, result2.value, result3.value];
            ";

            var result = ExecuteCode(code);
            Assert.IsType<List<object?>>(result);
            var list = (List<object?>)result;
            Assert.Equal(0.0, list[0]);
            Assert.Equal(1.0, list[1]);
            Assert.Equal(2.0, list[2]);
        }

        [Fact]
        public void GeneratorFunction_WithClosure_AccessesClosureVariables()
        {
            var code = @"
                function createGenerator(multiplier) {
                    return function*() {
                        yield 1 * multiplier;
                        yield 2 * multiplier;
                    };
                }
                var gen = createGenerator(5)();
                var result1 = gen.next();
                var result2 = gen.next();
                [result1.value, result2.value];
            ";

            var result = ExecuteCode(code);
            Assert.IsType<List<object?>>(result);
            var list = (List<object?>)result;
            Assert.Equal(5.0, list[0]);
            Assert.Equal(10.0, list[1]);
        }

        [Fact]
        public void GeneratorFunction_MultipleGenerators_IndependentState()
        {
            var code = @"
                function* counter() {
                    var n = 0;
                    yield n;
                    n = n + 1;
                    yield n;
                }
                var gen1 = counter();
                var gen2 = counter();
                var result1 = gen1.next();
                var result2 = gen2.next();
                var result3 = gen1.next();
                var result4 = gen2.next();
                [result1.value, result2.value, result3.value, result4.value];
            ";

            var result = ExecuteCode(code);
            Assert.IsType<List<object?>>(result);
            var list = (List<object?>)result;
            Assert.Equal(0.0, list[0]); // gen1 first
            Assert.Equal(0.0, list[1]); // gen2 first  
            Assert.Equal(1.0, list[2]); // gen1 second
            Assert.Equal(1.0, list[3]); // gen2 second
        }

        [Fact]
        public void GeneratorFunction_YieldInConditional_WorksCorrectly()
        {
            var code = @"
                function* conditionalYield(flag) {
                    if (flag) {
                        yield ""true branch"";
                    } else {
                        yield ""false branch"";
                    }
                }
                var genTrue = conditionalYield(true);
                var genFalse = conditionalYield(false);
                var result1 = genTrue.next();
                var result2 = genFalse.next();
                [result1.value, result2.value];
            ";

            var result = ExecuteCode(code);
            Assert.IsType<List<object?>>(result);
            var list = (List<object?>)result;
            Assert.Equal("true branch", list[0]);
            Assert.Equal("false branch", list[1]);
        }

        [Fact]
        public void GeneratorFunction_YieldVariousTypes_HandlesAllTypes()
        {
            var code = @"
                function* mixedTypes() {
                    yield 42;
                    yield ""string"";
                    yield true;
                    yield null;
                    yield [1, 2, 3];
                    yield {key: ""value""};
                }
                var gen = mixedTypes();
                var r1 = gen.next();
                var r2 = gen.next();
                var r3 = gen.next();
                var r4 = gen.next();
                var r5 = gen.next();
                var r6 = gen.next();
                [r1.value, r2.value, r3.value, r4.value, r5.value, r6.value];
            ";

            var result = ExecuteCode(code);
            Assert.IsType<List<object?>>(result);
            var list = (List<object?>)result;
            Assert.Equal(42.0, list[0]);
            Assert.Equal("string", list[1]);
            Assert.Equal(true, list[2]);
            Assert.Null(list[3]);
            Assert.IsType<List<object?>>(list[4]);
            Assert.IsType<Dictionary<string, object?>>(list[5]);
        }
    }
}
