using Xunit;
using ECEngine.Lexer;

namespace ECEngine.Tests.Lexer
{
    public class CommentTokenizationTests
    {
        [Fact]
        public void ShouldSkipSingleLineComment()
        {
            var lexer = new ECEngine.Lexer.Lexer("var x = 42; // this is a comment");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(6, tokens.Count);
            Assert.Equal(TokenType.Var, tokens[0].Type);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
            Assert.Equal(TokenType.Assign, tokens[2].Type);
            Assert.Equal(TokenType.Number, tokens[3].Type);
            Assert.Equal(TokenType.Semicolon, tokens[4].Type);
            Assert.Equal(TokenType.EOF, tokens[5].Type);
        }

        [Fact]
        public void ShouldSkipSingleLineCommentAtEndOfLine()
        {
            var lexer = new ECEngine.Lexer.Lexer("let y = 10;//comment at end");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(6, tokens.Count);
            Assert.Equal(TokenType.Let, tokens[0].Type);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
            Assert.Equal(TokenType.Assign, tokens[2].Type);
            Assert.Equal(TokenType.Number, tokens[3].Type);
            Assert.Equal(TokenType.Semicolon, tokens[4].Type);
            Assert.Equal(TokenType.EOF, tokens[5].Type);
        }

        [Fact]
        public void ShouldSkipMultiLineComment()
        {
            var lexer = new ECEngine.Lexer.Lexer("/* This is a\n   multi-line comment */\nvar x = 42;");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(6, tokens.Count); // var, x, =, 42, ;, EOF
            Assert.Equal(TokenType.Var, tokens[0].Type);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
            Assert.Equal("x", tokens[1].Value);
            Assert.Equal(TokenType.Assign, tokens[2].Type);
            Assert.Equal(TokenType.Number, tokens[3].Type);
            Assert.Equal("42", tokens[3].Value);
            Assert.Equal(TokenType.Semicolon, tokens[4].Type);
            Assert.Equal(TokenType.EOF, tokens[5].Type);
        }

        [Fact]
        public void ShouldSkipMultiLineCommentOnSingleLine()
        {
            var lexer = new ECEngine.Lexer.Lexer("var x = /* comment */ 42;");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(6, tokens.Count); // var, x, =, 42, ;, EOF
            Assert.Equal(TokenType.Var, tokens[0].Type);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
            Assert.Equal("x", tokens[1].Value);
            Assert.Equal(TokenType.Assign, tokens[2].Type);
            Assert.Equal(TokenType.Number, tokens[3].Type);
            Assert.Equal("42", tokens[3].Value);
            Assert.Equal(TokenType.Semicolon, tokens[4].Type);
            Assert.Equal(TokenType.EOF, tokens[5].Type);
        }

        [Fact]
        public void ShouldSkipOnlyCommentLine()
        {
            var lexer = new ECEngine.Lexer.Lexer("// just a comment");
            var tokens = lexer.Tokenize();
            
            Assert.Single(tokens);
            Assert.Equal(TokenType.EOF, tokens[0].Type);
        }

        [Fact]
        public void ShouldSkipOnlyMultiLineComment()
        {
            var lexer = new ECEngine.Lexer.Lexer("/* just a\n comment */");
            var tokens = lexer.Tokenize();
            
            Assert.Single(tokens);
            Assert.Equal(TokenType.EOF, tokens[0].Type);
        }

        [Fact]
        public void ShouldHandleMultipleComments()
        {
            var lexer = new ECEngine.Lexer.Lexer("var x = 42; // first comment\nlet y = 10; /* second comment */");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(11, tokens.Count); // var, x, =, 42, ;, let, y, =, 10, ;, EOF
            Assert.Equal(TokenType.Var, tokens[0].Type);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
            Assert.Equal(TokenType.Assign, tokens[2].Type);
            Assert.Equal(TokenType.Number, tokens[3].Type);
            Assert.Equal(TokenType.Semicolon, tokens[4].Type);
            Assert.Equal(TokenType.Let, tokens[5].Type);
            Assert.Equal(TokenType.Identifier, tokens[6].Type);
            Assert.Equal(TokenType.Assign, tokens[7].Type);
            Assert.Equal(TokenType.Number, tokens[8].Type);
            Assert.Equal(TokenType.Semicolon, tokens[9].Type);
            Assert.Equal(TokenType.EOF, tokens[10].Type);
        }

        [Fact]
        public void ShouldHandleCommentWithCodeAfterNewline()
        {
            var lexer = new ECEngine.Lexer.Lexer("// comment line\nvar x = 42;");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(6, tokens.Count); // var, x, =, 42, ;, EOF
            Assert.Equal(TokenType.Var, tokens[0].Type);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
            Assert.Equal(TokenType.Assign, tokens[2].Type);
            Assert.Equal(TokenType.Number, tokens[3].Type);
            Assert.Equal(TokenType.Semicolon, tokens[4].Type);
            Assert.Equal(TokenType.EOF, tokens[5].Type);
        }

        [Fact]
        public void ShouldHandleMultiLineCommentWithCodeAfter()
        {
            var lexer = new ECEngine.Lexer.Lexer("/* multi\nline\ncomment */\nvar result = 100;");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(6, tokens.Count); // var, result, =, 100, ;, EOF
            Assert.Equal(TokenType.Var, tokens[0].Type);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
            Assert.Equal("result", tokens[1].Value);
            Assert.Equal(TokenType.Assign, tokens[2].Type);
            Assert.Equal(TokenType.Number, tokens[3].Type);
            Assert.Equal("100", tokens[3].Value);
            Assert.Equal(TokenType.Semicolon, tokens[4].Type);
            Assert.Equal(TokenType.EOF, tokens[5].Type);
        }

        [Fact]
        public void ShouldHandleConsecutiveComments()
        {
            var lexer = new ECEngine.Lexer.Lexer("// first comment\n// second comment\nvar x = 1;");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(6, tokens.Count); // var, x, =, 1, ;, EOF
            Assert.Equal(TokenType.Var, tokens[0].Type);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
            Assert.Equal(TokenType.Assign, tokens[2].Type);
            Assert.Equal(TokenType.Number, tokens[3].Type);
            Assert.Equal(TokenType.Semicolon, tokens[4].Type);
            Assert.Equal(TokenType.EOF, tokens[5].Type);
        }

        [Fact]
        public void ShouldHandleMixedCommentTypes()
        {
            var lexer = new ECEngine.Lexer.Lexer("/* block */ var x = 42; // line");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(6, tokens.Count); // var, x, =, 42, ;, EOF
            Assert.Equal(TokenType.Var, tokens[0].Type);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
            Assert.Equal(TokenType.Assign, tokens[2].Type);
            Assert.Equal(TokenType.Number, tokens[3].Type);
            Assert.Equal(TokenType.Semicolon, tokens[4].Type);
            Assert.Equal(TokenType.EOF, tokens[5].Type);
        }

        [Fact]
        public void ShouldHandleEmptyMultiLineComment()
        {
            var lexer = new ECEngine.Lexer.Lexer("var x = /**/42;");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(6, tokens.Count); // var, x, =, 42, ;, EOF
            Assert.Equal(TokenType.Var, tokens[0].Type);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
            Assert.Equal(TokenType.Assign, tokens[2].Type);
            Assert.Equal(TokenType.Number, tokens[3].Type);
            Assert.Equal(TokenType.Semicolon, tokens[4].Type);
            Assert.Equal(TokenType.EOF, tokens[5].Type);
        }
    }
}
