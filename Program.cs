using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.AST;
using ECEngine.Runtime;

namespace ECEngine;

class Program
{
	static void Main(string[] args)
	{
		// Sample JS code to run
		string code = "console.log(1 + 2);";

		// Tokenize
		var lexer = new Lexer.Lexer(code);
		var tokens = lexer.Tokenize();

		// Parse (stub)
		var parser = new Parser.Parser();
		var ast = parser.Parse(code);

		// Interpret (stub)
		var interpreter = new Interpreter();
		interpreter.Evaluate(ast);
	}
}
