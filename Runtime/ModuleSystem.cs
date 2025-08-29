using ECEngine.AST;

namespace ECEngine.Runtime;

// Represents a module with its exports
public class Module
{
    public string FilePath { get; }
    public Dictionary<string, object?> Exports { get; } = new Dictionary<string, object?>();
    public bool IsLoaded { get; set; } = false;
    
    public Module(string filePath)
    {
        FilePath = filePath;
    }
}

// Module loader and resolver
public class ModuleSystem
{
    private Dictionary<string, Module> _modules = new Dictionary<string, Module>();
    private readonly string _rootPath;
    
    public ModuleSystem(string rootPath = "")
    {
        _rootPath = rootPath;
    }
    
    public Module LoadModule(string modulePath, Interpreter interpreter)
    {
        // Resolve the absolute path
        var absolutePath = ResolvePath(modulePath);
        
        // Check if module is already loaded
        if (_modules.ContainsKey(absolutePath))
        {
            return _modules[absolutePath];
        }
        
        // Create new module
        var module = new Module(absolutePath);
        _modules[absolutePath] = module;
        
        // Read and parse the module file
        if (!File.Exists(absolutePath))
        {
            throw new ECEngineException($"Module not found: {modulePath}",
                1, 1, "", $"Could not find module file at {absolutePath}");
        }
        
        var moduleCode = File.ReadAllText(absolutePath);
        var parser = new Parser.Parser();
        var ast = parser.Parse(moduleCode);
        
        // Create a new interpreter context for the module
        var moduleInterpreter = new Interpreter();
        moduleInterpreter.SetModuleSystem(this);
        
        // Execute the module code
        moduleInterpreter.Evaluate(ast, moduleCode);
        
        // Copy exports from module interpreter
        foreach (var export in moduleInterpreter.GetExports())
        {
            module.Exports[export.Key] = export.Value;
        }
        
        module.IsLoaded = true;
        return module;
    }
    
    private string ResolvePath(string modulePath)
    {
        // Handle relative paths
        if (modulePath.StartsWith("./") || modulePath.StartsWith("../"))
        {
            return Path.GetFullPath(Path.Combine(_rootPath, modulePath));
        }
        
        // Add .ec extension if not present
        if (!modulePath.EndsWith(".ec"))
        {
            modulePath += ".ec";
        }
        
        // Resolve relative to current directory or root path
        if (Path.IsPathRooted(modulePath))
        {
            return modulePath;
        }
        
        return Path.GetFullPath(Path.Combine(_rootPath, modulePath));
    }
    
    public void ClearModules()
    {
        _modules.Clear();
    }
}
