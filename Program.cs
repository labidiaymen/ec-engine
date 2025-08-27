using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.AST;
using ECEngine.Runtime;

namespace ECEngine;

class Program
{
	static void Main(string[] args)
	{
		try
		{
			// Test successful execution
			Console.WriteLine("=== Testing Successful Execution ===");
			ExecuteCode("console.log(1 + 2);");
			
			Console.WriteLine("\n=== Testing Error Handling ===");
			// Test error case
			ExecuteCode("console.log(unknown_variable + 2);");
		}
		catch (Exception ex)
		{
			PrintError(ex);
		}
	}

	static void ExecuteCode(string code)
	{
		try
		{
			Console.WriteLine($"Executing: {code}");
			
			// Tokenize
			var lexer = new Lexer.Lexer(code);
			var tokens = lexer.Tokenize();

			// Parse
			var parser = new Parser.Parser();
			var ast = parser.Parse(code);

			// Interpret
			var interpreter = new Interpreter();
			var result = interpreter.Evaluate(ast, code);
			
			Console.WriteLine($"Result: {result ?? "undefined"}");
		}
		catch (ECEngineException ex)
		{
			Console.WriteLine("\nError occurred:");
			Console.WriteLine($"  {ex.Message} at {ex.GetFormattedLocation()}");
			Console.WriteLine($"  Context: {ex.ContextInfo}");
			Console.WriteLine("\nSource:");
			Console.WriteLine(ex.GetSourceCodeWithHighlight());
		}
	}

	static void PrintError(Exception ex)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine($"Unexpected error: {ex.Message}");
		Console.ResetColor();
	}
}
