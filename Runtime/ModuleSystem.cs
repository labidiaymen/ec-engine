using ECEngine.AST;
using System.Text.Json;

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
    
    public Module? LoadModule(string modulePath, Interpreter interpreter)
    {
        // Resolve the absolute path
        var absolutePath = ResolvePath(modulePath);
        
        // If module couldn't be resolved, return null
        if (absolutePath == null)
        {
            return null;
        }
        
        // Check if module is already loaded
        if (_modules.ContainsKey(absolutePath))
        {
            return _modules[absolutePath];
        }
        
        // Read and parse the module file
        if (!File.Exists(absolutePath))
        {
            return null;
        }
        
        // Create new module
        var module = new Module(absolutePath);
        _modules[absolutePath] = module;
        
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
    
    private string? ResolvePath(string modulePath)
    {
        // Handle relative paths
        if (modulePath.StartsWith("./") || modulePath.StartsWith("../"))
        {
            var relativePath = Path.GetFullPath(Path.Combine(_rootPath, modulePath));
            var resolvedRelative = ResolveWithExtensions(relativePath);
            return File.Exists(resolvedRelative) ? resolvedRelative : null;
        }
        
        // If already has a supported extension, use as-is
        if (HasSupportedExtension(modulePath))
        {
            string fullPath;
            // Resolve relative to current directory or root path
            if (Path.IsPathRooted(modulePath))
            {
                fullPath = modulePath;
            }
            else
            {
                fullPath = Path.GetFullPath(Path.Combine(_rootPath, modulePath));
            }
            return File.Exists(fullPath) ? fullPath : null;
        }
        
        // Try Node.js-style resolution for non-relative paths
        var nodeStylePath = ResolveNodeStyleModule(modulePath, _rootPath);
        if (nodeStylePath != null)
        {
            return nodeStylePath;
        }
        
        // Fallback to original resolution
        string basePath;
        if (Path.IsPathRooted(modulePath))
        {
            basePath = modulePath;
        }
        else
        {
            basePath = Path.GetFullPath(Path.Combine(_rootPath, modulePath));
        }
        
        var resolvedFallback = ResolveWithExtensions(basePath);
        return File.Exists(resolvedFallback) ? resolvedFallback : null;
    }
    
    private bool HasSupportedExtension(string path)
    {
        return path.EndsWith(".ec") || path.EndsWith(".js") || path.EndsWith(".mjs");
    }
    
    /// <summary>
    /// Implements Node.js-style module resolution
    /// </summary>
    private string? ResolveNodeStyleModule(string moduleName, string startDir)
    {
        // Start from specified directory and walk up the tree
        var currentDir = startDir;
        
        while (currentDir != null)
        {
            // Try to resolve in node_modules of current directory
            var nodeModulesDir = Path.Combine(currentDir, "node_modules");
            if (Directory.Exists(nodeModulesDir))
            {
                // First try as a directory: node_modules/moduleName/
                var moduleDir = Path.Combine(nodeModulesDir, moduleName);
                var resolved = ResolveNodeModule(moduleDir);
                if (resolved != null)
                {
                    return resolved;
                }
                
                // Then try as a direct file: node_modules/moduleName.js, etc.
                var moduleFile = Path.Combine(nodeModulesDir, moduleName);
                resolved = ResolveNodeModuleAsFile(moduleFile);
                if (resolved != null)
                {
                    return resolved;
                }
            }
            
            // Move up one directory
            var parent = Directory.GetParent(currentDir);
            if (parent == null || parent.FullName == currentDir)
            {
                break;
            }
            currentDir = parent.FullName;
        }
        
        return null;
    }
    
    /// <summary>
    /// Resolves a module in a specific node_modules directory
    /// </summary>
    private string? ResolveNodeModule(string modulePath)
    {
        // Check if it's a directory with package.json
        if (Directory.Exists(modulePath))
        {
            var packageJsonPath = Path.Combine(modulePath, "package.json");
            if (File.Exists(packageJsonPath))
            {
                var mainFile = GetMainFromPackageJson(packageJsonPath);
                if (mainFile != null)
                {
                    var mainPath = Path.Combine(modulePath, mainFile);
                    var resolvedMain = ResolveWithExtensions(mainPath);
                    if (File.Exists(resolvedMain))
                    {
                        return resolvedMain;
                    }
                }
            }
            
            // Try index files
            var indexPath = Path.Combine(modulePath, "index");
            var resolvedIndex = ResolveWithExtensions(indexPath);
            if (File.Exists(resolvedIndex))
            {
                return resolvedIndex;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Resolves a module as a direct file in node_modules
    /// </summary>
    private string? ResolveNodeModuleAsFile(string modulePath)
    {
        // Check if it's a file with extension
        if (HasSupportedExtension(modulePath) && File.Exists(modulePath))
        {
            return modulePath;
        }
        
        // Try adding extensions
        var withExtension = ResolveWithExtensions(modulePath);
        if (File.Exists(withExtension))
        {
            return withExtension;
        }
        
        return null;
    }
    
    /// <summary>
    /// Extracts the main field from package.json
    /// </summary>
    private string? GetMainFromPackageJson(string packageJsonPath)
    {
        try
        {
            var json = File.ReadAllText(packageJsonPath);
            var document = JsonDocument.Parse(json);
            
            if (document.RootElement.TryGetProperty("main", out var mainElement))
            {
                return mainElement.GetString();
            }
            
            // Default to index.js if no main field
            return "index.js";
        }
        catch
        {
            // If package.json is invalid, return null
            return null;
        }
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
