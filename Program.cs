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
            // Check if running in interactive mode
            if (args.Length == 0 || (args.Length == 1 && (args[0] == "-i" || args[0] == "--interactive")))
            {
                // Start interactive REPL
                var interactiveRuntime = new InteractiveRuntime();
                interactiveRuntime.StartREPL();
                return;
            }

            // Check for help
            if (args.Length == 1 && (args[0] == "-h" || args[0] == "--help"))
            {
                ShowHelp();
                return;
            }

            // Check for file execution
            if (args.Length == 1 && File.Exists(args[0]))
            {
                ExecuteFile(args[0]);
                return;
            }

            // Run demo/test cases (original behavior)
            RunDemoCases();
        }
        catch (Exception ex)
        {
            PrintError(ex);
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("ECEngine - JavaScript-like scripting engine");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  ECEngine                    Start interactive REPL");
        Console.WriteLine("  ECEngine -i, --interactive  Start interactive REPL");
        Console.WriteLine("  ECEngine <file>             Execute a script file");
        Console.WriteLine("  ECEngine -h, --help         Show this help");
        Console.WriteLine();
        Console.WriteLine("In interactive mode:");
        Console.WriteLine("  Type JavaScript-like code and press Enter to execute");
        Console.WriteLine("  Use .help for REPL commands");
        Console.WriteLine("  Use .exit to quit");
    }

    static void ExecuteFile(string filePath)
    {
        try
        {
            var code = File.ReadAllText(filePath);
            Console.WriteLine($"Executing file: {filePath}");
            Console.WriteLine();
            
            var interpreter = new Interpreter();
            
            // Set up module system with the directory of the current file as root
            var moduleSystem = new ModuleSystem(Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? Directory.GetCurrentDirectory());
            interpreter.SetModuleSystem(moduleSystem);
            
            var result = ExecuteCode(code, interpreter);
            
            if (result != null)
            {
                Console.WriteLine($"Final result: {result}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing file '{filePath}': {ex.Message}");
        }
    }

    static void RunDemoCases()
    {
        // Test successful execution
        Console.WriteLine("=== Testing Successful Execution ===");
        ExecuteCode("console.log(1 + 2);");

        Console.WriteLine("\n=== Testing Variable Declarations ===");
        ExecuteCode("var x = 42;");
        ExecuteCode("let y = 100;");
        ExecuteCode("const z = 3.14;");
        
        Console.WriteLine("\n=== Testing Variable Usage ===");
        ExecuteCode("var a = 5; var b = 10; a + b;");
        ExecuteCode("var result = 10 + 20 * 2; result;");
        
        Console.WriteLine("\n=== Testing Variable Assignment ===");
        ExecuteCode("var x = 10; x = 20; x;");
        ExecuteCode("var sum = 5; sum = sum + 10; sum;");

        Console.WriteLine("\n=== Testing Error Handling ===");
        // Test error case
        ExecuteCode("console.log(unknown_variable + 2);");
        ExecuteCode("undeclaredVar;");
        ExecuteCode("notDeclared = 42;");
        ExecuteCode("var x = 5; var x = 10;");
    }

    static void ExecuteCode(string code)
    {
        ExecuteCode(code, new Interpreter());
    }

    static object? ExecuteCode(string code, Interpreter interpreter)
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
            var result = interpreter.Evaluate(ast, code);

            Console.WriteLine($"Result: {result ?? "undefined"}");
            return result;
        }
        catch (ECEngineException ex)
        {
            Console.WriteLine("\nError occurred:");
            Console.WriteLine($"  {ex.Message} at {ex.GetFormattedLocation()}");
            Console.WriteLine($"  Context: {ex.ContextInfo}");
            Console.WriteLine("\nSource:");
            Console.WriteLine(ex.GetSourceCodeWithHighlight());
            return null;
        }
    }

    static void PrintError(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Unexpected error: {ex.Message}");
        Console.ResetColor();
    }
}
