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
            var relativePath = Path.GetFullPath(Path.Combine(_rootPath, modulePath));
            return ResolveWithExtensions(relativePath);
        }
        
        // If already has a supported extension, use as-is
        if (HasSupportedExtension(modulePath))
        {
            // Resolve relative to current directory or root path
            if (Path.IsPathRooted(modulePath))
            {
                return modulePath;
            }
            return Path.GetFullPath(Path.Combine(_rootPath, modulePath));
        }
        
        // Try to resolve with supported extensions
        string basePath;
        if (Path.IsPathRooted(modulePath))
        {
            basePath = modulePath;
        }
        else
        {
            basePath = Path.GetFullPath(Path.Combine(_rootPath, modulePath));
        }
        
        return ResolveWithExtensions(basePath);
    }
    
    private bool HasSupportedExtension(string path)
    {
        return path.EndsWith(".ec") || path.EndsWith(".js") || path.EndsWith(".mjs");
    }
    
    private string ResolveWithExtensions(string basePath)
    {
        // If path already has extension and file exists, return it
        if (HasSupportedExtension(basePath) && File.Exists(basePath))
        {
            return basePath;
        }
        
        // If no extension, try supported extensions in order of preference
        if (!HasSupportedExtension(basePath))
        {
            string[] extensions = { ".ec", ".js", ".mjs" };
            
            foreach (string ext in extensions)
            {
                string pathWithExt = basePath + ext;
                if (File.Exists(pathWithExt))
                {
                    return pathWithExt;
                }
            }
            
            // Fallback: add .ec extension (for backwards compatibility)
            return basePath + ".ec";
        }
        
        return basePath;
    }
    
    public void ClearModules()
    {
        _modules.Clear();
    }
}
