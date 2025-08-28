using Xunit;
using ECEngine.Lexer;

namespace ECEngine.Tests.Lexer
{
    public class FunctionTokenizationTests
    {
        [Fact]
        public void ShouldTokenizeFunctionKeyword()
        {
            var lexer = new ECEngine.Lexer.Lexer("function");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(2, tokens.Count);
            Assert.Equal(TokenType.Function, tokens[0].Type);
            Assert.Equal("function", tokens[0].Value);
            Assert.Equal(TokenType.EOF, tokens[1].Type);
        }

        [Fact]
        public void ShouldTokenizeReturnKeyword()
        {
            var lexer = new ECEngine.Lexer.Lexer("return");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(2, tokens.Count);
            Assert.Equal(TokenType.Return, tokens[0].Type);
            Assert.Equal("return", tokens[0].Value);
            Assert.Equal(TokenType.EOF, tokens[1].Type);
        }

        [Fact]
        public void ShouldTokenizeComma()
        {
            var lexer = new ECEngine.Lexer.Lexer("a, b, c");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(6, tokens.Count);
            Assert.Equal(TokenType.Identifier, tokens[0].Type);
            Assert.Equal(TokenType.Comma, tokens[1].Type);
            Assert.Equal(TokenType.Identifier, tokens[2].Type);
            Assert.Equal(TokenType.Comma, tokens[3].Type);
            Assert.Equal(TokenType.Identifier, tokens[4].Type);
            Assert.Equal(TokenType.EOF, tokens[5].Type);
        }

        [Fact]
        public void ShouldTokenizeSimpleFunctionDeclaration()
        {
            var lexer = new ECEngine.Lexer.Lexer("function add(a, b) { return a + b; }");
            var tokens = lexer.Tokenize();
            
            Assert.Equal(15, tokens.Count);
            Assert.Equal(TokenType.Function, tokens[0].Type);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
            Assert.Equal("add", tokens[1].Value);
            Assert.Equal(TokenType.LeftParen, tokens[2].Type);
            Assert.Equal(TokenType.Identifier, tokens[3].Type);
            Assert.Equal("a", tokens[3].Value);
            Assert.Equal(TokenType.Comma, tokens[4].Type);
            Assert.Equal(TokenType.Identifier, tokens[5].Type);
            Assert.Equal("b", tokens[5].Value);
            Assert.Equal(TokenType.RightParen, tokens[6].Type);
            Assert.Equal(TokenType.LeftBrace, tokens[7].Type);
            Assert.Equal(TokenType.Return, tokens[8].Type);
            Assert.Equal(TokenType.Identifier, tokens[9].Type);
            Assert.Equal("a", tokens[9].Value);
            Assert.Equal(TokenType.Plus, tokens[10].Type);
            Assert.Equal(TokenType.Identifier, tokens[11].Type);
            Assert.Equal("b", tokens[11].Value);
            Assert.Equal(TokenType.Semicolon, tokens[12].Type);
            Assert.Equal(TokenType.RightBrace, tokens[13].Type);
            Assert.Equal(TokenType.EOF, tokens[14].Type);
        }
    }
}
