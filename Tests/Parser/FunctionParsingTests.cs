using Xunit;
using ECEngine.AST;
using ECEngine.Parser;

namespace ECEngine.Tests.Parser
{
    public class FunctionParsingTests
    {
        [Fact]
        public void ShouldParseFunctionDeclaration()
        {
            var code = "function test() { return 42; }";
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            
            Assert.IsType<ProgramNode>(ast);
            var program = (ProgramNode)ast;
            Assert.Single(program.Body);
            
            var statement = program.Body[0];
            Assert.IsType<FunctionDeclaration>(statement);
            
            var funcDecl = (FunctionDeclaration)statement;
            Assert.Equal("test", funcDecl.Name);
            Assert.Empty(funcDecl.Parameters);
            Assert.Single(funcDecl.Body);
        }

        [Fact]
        public void ShouldParseFunctionWithParameters()
        {
            var code = "function add(a, b, c) { return a + b + c; }";
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            
            var program = (ProgramNode)ast;
            var funcDecl = (FunctionDeclaration)program.Body[0];
            
            Assert.Equal("add", funcDecl.Name);
            Assert.Equal(3, funcDecl.Parameters.Count);
            Assert.Equal("a", funcDecl.Parameters[0]);
            Assert.Equal("b", funcDecl.Parameters[1]);
            Assert.Equal("c", funcDecl.Parameters[2]);
        }

        [Fact]
        public void ShouldParseReturnStatement()
        {
            var code = "function test() { return 42; }";
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            
            var program = (ProgramNode)ast;
            var funcDecl = (FunctionDeclaration)program.Body[0];
            var returnStmt = funcDecl.Body[0];
            
            Assert.IsType<ReturnStatement>(returnStmt);
            var ret = (ReturnStatement)returnStmt;
            Assert.NotNull(ret.Argument);
            Assert.IsType<NumberLiteral>(ret.Argument);
            
            var literal = (NumberLiteral)ret.Argument;
            Assert.Equal(42.0, literal.Value);
        }

        [Fact]
        public void ShouldParseEmptyReturn()
        {
            var code = "function test() { return; }";
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            
            var program = (ProgramNode)ast;
            var funcDecl = (FunctionDeclaration)program.Body[0];
            var returnStmt = funcDecl.Body[0];
            
            Assert.IsType<ReturnStatement>(returnStmt);
            var ret = (ReturnStatement)returnStmt;
            Assert.Null(ret.Argument);
        }

        [Fact]
        public void ShouldParseFunctionCall()
        {
            var code = "test(1, 2, 3);";
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            
            var program = (ProgramNode)ast;
            var exprStmt = (ExpressionStatement)program.Body[0];
            var callExpr = (CallExpression)exprStmt.Expression;
            
            Assert.IsType<Identifier>(callExpr.Callee);
            var callee = (Identifier)callExpr.Callee;
            Assert.Equal("test", callee.Name);
            
            Assert.Equal(3, callExpr.Arguments.Count);
            Assert.All(callExpr.Arguments, arg => Assert.IsType<NumberLiteral>(arg));
        }
    }
}
