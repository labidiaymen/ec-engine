using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.AST;

namespace ECEngine.Runtime;

/// <summary>
/// Interactive runtime for EC Engine
/// Maintains state between commands and provides immediate feedback
/// </summary>
public class InteractiveRuntime
{
    private readonly Interpreter _interpreter;
    private readonly List<string> _history;
    private bool _isRunning;

    public InteractiveRuntime()
    {
        _interpreter = new Interpreter();
        _history = new List<string>();
        _isRunning = false;
    }

    /// <summary>
    /// Start the interactive REPL session
    /// </summary>
    public void StartREPL()
    {
        _isRunning = true;
        ShowWelcomeMessage();

        while (_isRunning)
        {
            try
            {
                // Show prompt
                Console.Write("ec> ");
                
                // Read input
                var input = Console.ReadLine();
                
                // Handle special commands
                if (HandleSpecialCommands(input))
                    continue;
                
                // Skip empty input
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                // Add to history
                _history.Add(input);

                // Execute the input
                ExecuteInteractive(input);
            }
            catch (Exception ex)
            {
                PrintError($"Unexpected error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Execute a single line of code interactively
    /// </summary>
    public object? ExecuteInteractive(string code)
    {
        try
        {
            // Tokenize
            var lexer = new Lexer.Lexer(code);
            var tokens = lexer.Tokenize();

            // Parse
            var parser = new Parser.Parser();
            var ast = parser.Parse(code);

            // Interpret
            var result = _interpreter.Evaluate(ast, code);

            // Display result 
            if (result != null)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"{FormatResult(result)}");
                Console.ResetColor();
            }

            return result;
        }
        catch (ECEngineException ex)
        {
            PrintError($"{ex.Message} at {ex.GetFormattedLocation()}");
            if (!string.IsNullOrEmpty(ex.ContextInfo))
            {
                PrintError($"Context: {ex.ContextInfo}");
            }
            return null;
        }
        catch (Exception ex)
        {
            PrintError($"Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Handle special REPL commands
    /// </summary>
    private bool HandleSpecialCommands(string? input)
    {
        if (input == null) return false;

        var trimmed = input.Trim();

        switch (trimmed.ToLower())
        {
            case ".exit":
            case ".quit":
            case "exit":
            case "quit":
                _isRunning = false;
                Console.WriteLine("Goodbye!");
                return true;

            case ".help":
                ShowHelp();
                return true;

            case ".clear":
                Console.Clear();
                ShowWelcomeMessage();
                return true;

            case ".history":
                ShowHistory();
                return true;

            case ".vars":
                ShowVariables();
                return true;

            case ".reset":
                ResetState();
                return true;

            default:
                if (trimmed.StartsWith("."))
                {
                    PrintError($"Unknown command: {trimmed}. Type .help for available commands.");
                    return true;
                }
                return false;
        }
    }

    /// <summary>
    /// Format the result for display
    /// </summary>
    private string FormatResult(object result)
    {
        return result switch
        {
            null => "undefined",
            string str => $"'{str}'",
            double d => d.ToString("G"),
            int i => i.ToString(),
            bool b => b.ToString().ToLower(),
            _ => result.ToString() ?? "undefined"
        };
    }

    /// <summary>
    /// Show welcome message
    /// </summary>
    private void ShowWelcomeMessage()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("ECEngine Interactive Shell");
        Console.WriteLine("Type .help for available commands or .exit to quit");
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Show help information
    /// </summary>
    private void ShowHelp()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("ECEngine REPL Commands:");
        Console.WriteLine("  .exit, .quit  - Exit the REPL");
        Console.WriteLine("  .help         - Show this help message");
        Console.WriteLine("  .clear        - Clear the screen");
        Console.WriteLine("  .history      - Show command history");
        Console.WriteLine("  .vars         - Show current variables");
        Console.WriteLine("  .reset        - Reset interpreter state");
        Console.WriteLine();
        Console.WriteLine("JavaScript-like syntax examples:");
        Console.WriteLine("  var x = 42;");
        Console.WriteLine("  let y = x + 10;");
        Console.WriteLine("  console.log('Hello World');");
        Console.WriteLine("  x * y");
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Show command history
    /// </summary>
    private void ShowHistory()
    {
        if (_history.Count == 0)
        {
            Console.WriteLine("No command history");
            return;
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        for (int i = 0; i < _history.Count; i++)
        {
            Console.WriteLine($"  {i + 1}: {_history[i]}");
        }
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Show current variables
    /// </summary>
    private void ShowVariables()
    {
        var variables = _interpreter.Variables;
        
        if (variables.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("No variables defined");
            Console.ResetColor();
            Console.WriteLine();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Current variables:");
        Console.ResetColor();
        
        foreach (var kvp in variables)
        {
            var variableInfo = kvp.Value;
            
            // Show variable type
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"  {variableInfo.Type} ");
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{kvp.Key}");
            Console.ResetColor();
            Console.Write(" = ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(FormatResult(variableInfo.Value ?? "null"));
            Console.ResetColor();
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Reset interpreter state
    /// </summary>
    private void ResetState()
    {
        _interpreter.ClearState();
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Interpreter state reset. All variables cleared.");
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Print error message
    /// </summary>
    private void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
