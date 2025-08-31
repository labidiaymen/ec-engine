using ECEngine.Lexer;
using ECEngine.Parser;
using ECEngine.AST;
using ECEngine.Runtime;

namespace ECEngine;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Check for package management commands
            if (args.Length >= 1 && args[0] == "install")
            {
                await HandleInstallCommandAsync(args);
                return;
            }

            if (args.Length >= 1 && args[0] == "init")
            {
                await HandleInitCommandAsync(args);
                return;
            }

            if (args.Length >= 1 && args[0] == "list")
            {
                await HandleListCommandAsync();
                return;
            }

            if (args.Length >= 1 && args[0] == "run")
            {
                await HandleRunScriptCommandAsync(args);
                return;
            }

            if (args.Length >= 1 && args[0] == "link")
            {
                await HandleLinkCommandAsync(args);
                return;
            }

            if (args.Length >= 1 && args[0] == "workspace")
            {
                await HandleWorkspaceCommandAsync(args);
                return;
            }

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

            // Check for version
            if (args.Length == 1 && (args[0] == "-v" || args[0] == "--version"))
            {
                ShowVersion();
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

    static async Task HandleInstallCommandAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: ECEngine install <package-name> [version] [flags]");
            Console.WriteLine("       ECEngine install <url>");
            Console.WriteLine("       ECEngine install <path>");
            Console.WriteLine();
            Console.WriteLine("Flags:");
            Console.WriteLine("  --dev                         Install as dev dependency");
            Console.WriteLine("  --optional                    Install as optional dependency");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  ECEngine install lodash");
            Console.WriteLine("  ECEngine install lodash@4.17.21");
            Console.WriteLine("  ECEngine install typescript --dev");
            Console.WriteLine("  ECEngine install https://cdn.skypack.dev/lodash");
            Console.WriteLine("  ECEngine install ./my-local-package");
            return;
        }

        var packageManager = new PackageManager(Directory.GetCurrentDirectory());
        var packageSpec = args[1];
        string? version = null;
        bool isDev = false;
        bool isOptional = false;

        // Parse remaining arguments
        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--dev":
                case "-D":
                    isDev = true;
                    break;
                case "--optional":
                case "-O":
                    isOptional = true;
                    break;
                default:
                    // Assume it's a version if not a flag
                    if (!args[i].StartsWith("--") && version == null)
                    {
                        version = args[i];
                    }
                    break;
            }
        }

        var success = await packageManager.InstallPackageAsync(packageSpec, version, isDev, isOptional);
        if (!success)
        {
            Environment.Exit(1);
        }
    }

    static async Task HandleInitCommandAsync(string[] args)
    {
        var packageManager = new PackageManager(Directory.GetCurrentDirectory());
        
        string? name = null;
        string? version = null;

        // Parse optional arguments
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--name" && i + 1 < args.Length)
            {
                name = args[i + 1];
                i++;
            }
            else if (args[i] == "--version" && i + 1 < args.Length)
            {
                version = args[i + 1];
                i++;
            }
        }

        var success = await packageManager.InitializePackageAsync(name, version);
        if (!success)
        {
            Environment.Exit(1);
        }
    }

    static async Task HandleListCommandAsync()
    {
        var packageManager = new PackageManager(Directory.GetCurrentDirectory());
        await packageManager.ListPackagesAsync();
    }

    static async Task HandleRunScriptCommandAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: ECEngine run <script-name> [args...]");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  ECEngine run build");
            Console.WriteLine("  ECEngine run test --verbose");
            return;
        }

        var packageManager = new PackageManager(Directory.GetCurrentDirectory());
        var scriptName = args[1];
        var scriptArgs = args.Length > 2 ? args[2..] : null;

        var success = await packageManager.RunScriptAsync(scriptName, scriptArgs);
        if (!success)
        {
            Environment.Exit(1);
        }
    }

    static async Task HandleLinkCommandAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: ECEngine link <package-path> [link-name]");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  ECEngine link ./my-local-package");
            Console.WriteLine("  ECEngine link ../shared-lib my-lib");
            return;
        }

        var packageManager = new PackageManager(Directory.GetCurrentDirectory());
        var packagePath = args[1];
        var linkName = args.Length > 2 ? args[2] : null;

        var success = await packageManager.LinkPackageAsync(packagePath, linkName);
        if (!success)
        {
            Environment.Exit(1);
        }
    }

    static async Task HandleWorkspaceCommandAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: ECEngine workspace <command>");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  init [patterns...]    Initialize workspace");
            Console.WriteLine("  install              Install all workspace dependencies");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  ECEngine workspace init packages/*");
            Console.WriteLine("  ECEngine workspace install");
            return;
        }

        var packageManager = new PackageManager(Directory.GetCurrentDirectory());
        var command = args[1];

        switch (command)
        {
            case "init":
                var patterns = args.Length > 2 ? args[2..] : null;
                var success = await packageManager.InitWorkspaceAsync(patterns);
                if (!success)
                {
                    Environment.Exit(1);
                }
                break;

            case "install":
                var installSuccess = await packageManager.InstallWorkspaceAsync();
                if (!installSuccess)
                {
                    Environment.Exit(1);
                }
                break;

            default:
                Console.WriteLine($"Unknown workspace command: {command}");
                Environment.Exit(1);
                break;
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("ECEngine - JavaScript-like scripting engine with package management");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  ECEngine                      Start interactive REPL");
        Console.WriteLine("  ECEngine -i, --interactive    Start interactive REPL");
        Console.WriteLine("  ECEngine <file>               Execute a script file");
        Console.WriteLine("  ECEngine -h, --help           Show this help");
        Console.WriteLine("  ECEngine -v, --version        Show version information");
        Console.WriteLine();
        Console.WriteLine("Package Management:");
        Console.WriteLine("  ECEngine init [--name <name>] [--version <version>]");
        Console.WriteLine("                                Initialize a new package");
        Console.WriteLine("  ECEngine install <package>    Install a package from npm");
        Console.WriteLine("  ECEngine install <url>        Install a package from URL");
        Console.WriteLine("  ECEngine install <path>       Install a local package");
        Console.WriteLine("  ECEngine list                 List installed packages");
        Console.WriteLine("  ECEngine run <script>         Run a package script");
        Console.WriteLine("  ECEngine link <path>          Link a local package");
        Console.WriteLine();
        Console.WriteLine("Workspace Management:");
        Console.WriteLine("  ECEngine workspace init       Initialize workspace");
        Console.WriteLine("  ECEngine workspace install    Install workspace dependencies");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  ECEngine init --name my-project --version 1.0.0");
        Console.WriteLine("  ECEngine install lodash");
        Console.WriteLine("  ECEngine install lodash@4.17.21");
        Console.WriteLine("  ECEngine install https://cdn.skypack.dev/date-fns");
        Console.WriteLine("  ECEngine install ./my-local-package");
        Console.WriteLine("  ECEngine run build");
        Console.WriteLine("  ECEngine run test --verbose");
        Console.WriteLine("  ECEngine link ../shared-library");
        Console.WriteLine("  ECEngine workspace init packages/*");
        Console.WriteLine();
        Console.WriteLine("In interactive mode:");
        Console.WriteLine("  Type JavaScript-like code and press Enter to execute");
        Console.WriteLine("  Use .help for REPL commands");
        Console.WriteLine("  Use .exit to quit");
    }

    static void ShowVersion()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        Console.WriteLine($"ECEngine v{version?.ToString(3) ?? "1.0.0"}");
        Console.WriteLine("A lightweight ECMAScript (JavaScript) interpreter engine");
        Console.WriteLine("https://github.com/labidiaymen/ec-engine");
    }

    static void ExecuteFile(string filePath)
    {
        try
        {
            var code = File.ReadAllText(filePath);
            Console.WriteLine($"Executing file: {filePath}");
            Console.WriteLine();
            
            var interpreter = new Interpreter();
            var eventLoop = new EventLoop();
            
            // Set up module system with the directory of the current file as root
            var moduleSystem = new ModuleSystem(Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? Directory.GetCurrentDirectory());
            interpreter.SetModuleSystem(moduleSystem);
            interpreter.SetEventLoop(eventLoop);
            
            var result = ExecuteCode(code, interpreter);
            
            // Run the event loop to handle async operations
            eventLoop.Run();
            
            if (result != null)
            {
                Console.WriteLine($"Result: {result}");
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
