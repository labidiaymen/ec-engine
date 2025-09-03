using System;
using System.Collections.Generic;
using Xunit;
using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.Runtime;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Lexer
{
    public class NewOperatorTokenTests
    {
        [Fact]
        public void Lexer_IsKeyword_ShouldGenerateIsToken()
        {
            // Arrange
            var lexer = new ECEngine.Lexer.Lexer("5 is 5");
            
            // Act
            var tokens = lexer.Tokenize();
            
            // Assert
            Assert.Equal(TokenType.Number, tokens[0].Type);
            Assert.Equal("5", tokens[0].Value);
            Assert.Equal(TokenType.Is, tokens[1].Type);
            Assert.Equal("is", tokens[1].Value);
            Assert.Equal(TokenType.Number, tokens[2].Type);
            Assert.Equal("5", tokens[2].Value);
        }

        [Fact]
        public void Lexer_AndKeyword_ShouldGenerateAndToken()
        {
            // Arrange
            var lexer = new ECEngine.Lexer.Lexer("true and false");
            
            // Act
            var tokens = lexer.Tokenize();
            
            // Assert
            Assert.Equal(TokenType.True, tokens[0].Type);
            Assert.Equal(TokenType.And, tokens[1].Type);
            Assert.Equal("and", tokens[1].Value);
            Assert.Equal(TokenType.False, tokens[2].Type);
        }

        [Fact]
        public void Lexer_OrKeyword_ShouldGenerateOrToken()
        {
            // Arrange
            var lexer = new ECEngine.Lexer.Lexer("true or false");
            
            // Act
            var tokens = lexer.Tokenize();
            
            // Assert
            Assert.Equal(TokenType.True, tokens[0].Type);
            Assert.Equal(TokenType.Or, tokens[1].Type);
            Assert.Equal("or", tokens[1].Value);
            Assert.Equal(TokenType.False, tokens[2].Type);
        }
    }
}

namespace ECEngine.Tests.Interpreter
{
    public class NewOperatorEvaluationTests
    {
        private object? ExecuteCode(string code)
        {
            var interpreter = new RuntimeInterpreter();
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            return interpreter.Evaluate(ast, code);
        }

        [Fact]
        public void Interpreter_IsOperator_ShouldEvaluateEquality()
        {
            // Act & Assert
            var result1 = ExecuteCode("5 is 5");
            Assert.True((bool)result1);
            
            var result2 = ExecuteCode("5 is 3");
            Assert.False((bool)result2);
            
            var result3 = ExecuteCode("'hello' is 'hello'");
            Assert.True((bool)result3);
        }

        [Fact]
        public void Interpreter_AndOperator_ShouldEvaluateLogicalAnd()
        {
            // Act & Assert
            var result1 = ExecuteCode("true and true");
            Assert.True((bool)result1);
            
            var result2 = ExecuteCode("true and false");
            Assert.False((bool)result2);
            
            var result3 = ExecuteCode("5 and 3");
            Assert.Equal(3.0, (double)result3);
            
            var result4 = ExecuteCode("0 and 5");
            Assert.Equal(0.0, (double)result4);
        }

        [Fact]
        public void Interpreter_OrOperator_ShouldEvaluateLogicalOr()
        {
            // Act & Assert
            var result1 = ExecuteCode("true or false");
            Assert.True((bool)result1);
            
            var result2 = ExecuteCode("false or false");
            Assert.False((bool)result2);
            
            var result3 = ExecuteCode("5 or 3");
            Assert.Equal(5.0, (double)result3);
            
            var result4 = ExecuteCode("0 or 5");
            Assert.Equal(5.0, (double)result4);
        }

        [Fact]
        public void Interpreter_MixedOperators_ShouldWorkWithTraditionalOperators()
        {
            // Act & Assert
            var result1 = ExecuteCode("5 == 5 and true");
            Assert.True((bool)result1);
            
            var result2 = ExecuteCode("5 is 5 && true");
            Assert.True((bool)result2);
            
            var result3 = ExecuteCode("false or 3 is 3");
            Assert.True((bool)result3);
            
            var result4 = ExecuteCode("false || 3 is 3");
            Assert.True((bool)result4);
        }
    }
}
