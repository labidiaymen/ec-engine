using Xunit;
using ECEngine.Runtime;
using ECEngine.Lexer;
using ECEngine.Parser;
using System.Collections.Generic;

namespace ECEngine.Tests.Interpreter
{
    public class ThisKeywordTests
    {
        [Fact]
        public void ThisKeyword_BasicPropertyAccess_ReturnsCorrectValue()
        {
            var code = @"
                var obj = {
                    name: 'Alice',
                    getName: function() {
                        return this.name;
                    }
                };
                obj.getName();
            ";

            var result = ExecuteCode(code);
            Assert.Equal("Alice", result);
        }

        [Fact]
        public void ThisKeyword_MultipleProperties_AccessesCorrectValues()
        {
            var code = @"
                var person = {
                    name: 'Bob',
                    age: 30,
                    city: 'New York',
                    
                    getInfo: function() {
                        return this.name + ' is ' + this.age + ' years old from ' + this.city;
                    }
                };
                person.getInfo();
            ";

            var result = ExecuteCode(code);
            Assert.Equal("Bob is 30 years old from New York", result);
        }

        [Fact]
        public void ThisKeyword_NestedObjects_ContextIsCorrect()
        {
            var code = @"
                var company = {
                    name: 'TechCorp',
                    department: {
                        name: 'Engineering',
                        employees: 50,
                        
                        getDepartmentInfo: function() {
                            return this.name + ' has ' + this.employees + ' employees';
                        }
                    }
                };
                company.department.getDepartmentInfo();
            ";

            var result = ExecuteCode(code);
            Assert.Equal("Engineering has 50 employees", result);
        }

        [Fact]
        public void ThisKeyword_WithParameters_WorksCorrectly()
        {
            var code = @"
                var calculator = {
                    value: 10,
                    
                    add: function(amount) {
                        return this.value + amount;
                    }
                };
                calculator.add(5);
            ";

            var result = ExecuteCode(code);
            Assert.Equal(15.0, result);
        }

        [Fact]
        public void ThisKeyword_InFactoryFunction_ReturnsCorrectObject()
        {
            var code = @"
                function createCounter(initialValue) {
                    return {
                        value: initialValue,
                        getValue: function() {
                            return this.value;
                        }
                    };
                }
                
                var counter = createCounter(42);
                counter.getValue();
            ";

            var result = ExecuteCode(code);
            Assert.Equal(42.0, result);
        }

        [Fact]
        public void ThisKeyword_ConditionalLogic_WorksWithThisContext()
        {
            var code = @"
                var status = {
                    health: 85,
                    
                    getStatus: function() {
                        if (this.health > 80) {
                            return 'Excellent';
                        } else if (this.health > 50) {
                            return 'Good';
                        } else {
                            return 'Poor';
                        }
                    }
                };
                status.getStatus();
            ";

            var result = ExecuteCode(code);
            Assert.Equal("Excellent", result);
        }

        [Fact]
        public void ThisKeyword_MethodChaining_MaintainsCorrectContext()
        {
            var code = @"
                var math = {
                    x: 10,
                    y: 5,
                    
                    add: function() {
                        return this.x + this.y;
                    },
                    
                    calculate: function(operation) {
                        if (operation == 'add') {
                            return this.add();
                        }
                        return 0;
                    }
                };
                math.calculate('add');
            ";

            var result = ExecuteCode(code);
            Assert.Equal(15.0, result);
        }

        [Fact]
        public void ThisKeyword_MultipleObjectInstances_IsolatedContexts()
        {
            var code = @"
                function createPerson(name) {
                    return {
                        name: name,
                        getName: function() {
                            return this.name;
                        }
                    };
                }
                
                var person1 = createPerson('Alice');
                var person2 = createPerson('Bob');
                
                // Test first person
                person1.getName();
            ";

            var result = ExecuteCode(code);
            Assert.Equal("Alice", result);
            
            // Test second person separately
            var code2 = @"
                function createPerson(name) {
                    return {
                        name: name,
                        getName: function() {
                            return this.name;
                        }
                    };
                }
                
                var person2 = createPerson('Bob');
                person2.getName();
            ";

            var result2 = ExecuteCode(code2);
            Assert.Equal("Bob", result2);
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
