using Xunit;
using ECEngine.Runtime;
using ECEngine.Lexer;
using ECEngine.Parser;
using System.Collections.Generic;

namespace ECEngine.Tests.Interpreter
{
    public class PropertyAssignmentTests
    {
        [Fact]
        public void PropertyAssignment_BasicObjectProperty_AssignsCorrectly()
        {
            var code = @"
                var obj = { x: 10, y: 20 };
                obj.x = 42;
                obj.x;
            ";

            var result = ExecuteCode(code);
            Assert.Equal(42.0, result);
        }

        [Fact]
        public void PropertyAssignment_MultipleProperties_AllAssignCorrectly()
        {
            var code = @"
                var obj = { name: 'original', value: 100 };
                obj.name = 'updated';
                obj.value = 200;
                obj.name;
            ";

            var result1 = ExecuteCode(code);
            Assert.Equal("updated", result1);
            
            var code2 = @"
                var obj = { name: 'original', value: 100 };
                obj.name = 'updated';
                obj.value = 200;
                obj.value;
            ";

            var result2 = ExecuteCode(code2);
            Assert.Equal(200.0, result2);
        }

        [Fact]
        public void PropertyAssignment_DynamicPropertyCreation_CreatesNewProperty()
        {
            var code = @"
                var obj = { existing: 'value' };
                obj.newProperty = 'dynamic';
                obj.newProperty;
            ";

            var result = ExecuteCode(code);
            Assert.Equal("dynamic", result);
        }

        [Fact]
        public void PropertyAssignment_ThisKeywordInMethod_ModifiesObjectProperty()
        {
            var code = @"
                var counter = {
                    value: 0,
                    setValue: function(newValue) {
                        this.value = newValue;
                        return this.value;
                    }
                };
                counter.setValue(42);
            ";

            var result = ExecuteCode(code);
            Assert.Equal(42.0, result);
        }

        [Fact]
        public void PropertyAssignment_ThisKeywordIncrement_UpdatesCorrectly()
        {
            var code = @"
                var counter = {
                    count: 0,
                    increment: function() {
                        this.count = this.count + 1;
                        return this.count;
                    }
                };
                
                counter.increment();
                counter.increment();
                counter.count;
            ";

            var result = ExecuteCode(code);
            Assert.Equal(2.0, result);
        }

        [Fact]
        public void PropertyAssignment_NestedObjectAssignment_WorksCorrectly()
        {
            var code = @"
                var nested = {
                    inner: {
                        value: 'original'
                    }
                };
                nested.inner.value = 'modified';
                nested.inner.value;
            ";

            var result = ExecuteCode(code);
            Assert.Equal("modified", result);
        }

        [Fact]
        public void PropertyAssignment_ComplexThisAssignment_UpdatesMultipleProperties()
        {
            var code = @"
                var person = {
                    name: 'Alice',
                    age: 25,
                    
                    updateInfo: function(newName, newAge) {
                        this.name = newName;
                        this.age = newAge;
                        return this.name + ' is ' + this.age + ' years old';
                    }
                };
                person.updateInfo('Bob', 30);
            ";

            var result = ExecuteCode(code);
            Assert.Equal("Bob is 30 years old", result);
        }

        [Fact]
        public void PropertyAssignment_MixedOperations_WorksCorrectly()
        {
            var code = @"
                var data = {
                    x: 10,
                    y: 20,
                    
                    calculate: function(operation) {
                        if (operation == 'add') {
                            this.result = this.x + this.y;
                        } else if (operation == 'multiply') {
                            this.result = this.x * this.y;
                        }
                        return this.result;
                    }
                };
                
                data.calculate('multiply');
            ";

            var result = ExecuteCode(code);
            Assert.Equal(200.0, result);
        }

        [Fact]
        public void PropertyAssignment_StringAssignment_HandlesStringValues()
        {
            var code = @"
                var config = {
                    mode: 'development',
                    
                    setMode: function(newMode) {
                        this.mode = newMode;
                        return this.mode;
                    }
                };
                config.setMode('production');
            ";

            var result = ExecuteCode(code);
            Assert.Equal("production", result);
        }

        [Fact]
        public void PropertyAssignment_BooleanAssignment_HandlesBooleanValues()
        {
            var code = @"
                var settings = {
                    enabled: false,
                    
                    toggle: function() {
                        this.enabled = !this.enabled;
                        return this.enabled;
                    }
                };
                settings.toggle();
            ";

            var result = ExecuteCode(code);
            Assert.Equal(true, result);
        }

        [Fact]
        public void PropertyAssignment_NullAssignment_HandlesNullValues()
        {
            var code = @"
                var obj = {
                    data: 'something',
                    
                    clear: function() {
                        this.data = null;
                        return this.data;
                    }
                };
                obj.clear();
            ";

            var result = ExecuteCode(code);
            Assert.Null(result);
        }

        [Fact]
        public void PropertyAssignment_ChainedAssignments_WorksCorrectly()
        {
            var code = @"
                var state = {
                    x: 0,
                    y: 0,
                    
                    setCoordinates: function(newX, newY) {
                        this.x = newX;
                        this.y = newY;
                        this.lastUpdate = 'coordinates';
                        return this.x;
                    }
                };
                state.setCoordinates(15, 25);
            ";

            var result1 = ExecuteCode(code);
            Assert.Equal(15.0, result1);
            
            // Test y coordinate separately
            var code2 = @"
                var state = {
                    x: 0,
                    y: 0,
                    
                    setCoordinates: function(newX, newY) {
                        this.x = newX;
                        this.y = newY;
                        this.lastUpdate = 'coordinates';
                        return this.y;
                    }
                };
                state.setCoordinates(15, 25);
            ";

            var result2 = ExecuteCode(code2);
            Assert.Equal(25.0, result2);
            
            // Test lastUpdate separately
            var code3 = @"
                var state = {
                    x: 0,
                    y: 0,
                    
                    setCoordinates: function(newX, newY) {
                        this.x = newX;
                        this.y = newY;
                        this.lastUpdate = 'coordinates';
                        return this.lastUpdate;
                    }
                };
                state.setCoordinates(15, 25);
            ";

            var result3 = ExecuteCode(code3);
            Assert.Equal("coordinates", result3);
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
