using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace ECEngine.Runtime;

public class NodePathModule
{
    public string? Separator { get; }
    public string? Delimiter { get; }
    private readonly bool _isWindows;

    public NodePathModule(bool isWindows = false)
    {
        _isWindows = isWindows;
        Separator = isWindows ? "\\" : "/";
        Delimiter = isWindows ? ";" : ":";
    }

    public string Basename(string path, string? suffix = null)
    {
        if (string.IsNullOrEmpty(path))
            return "";

        // Remove trailing separators
        path = path.TrimEnd('/', '\\');
        if (string.IsNullOrEmpty(path))
            return _isWindows ? "\\" : "/";

        // Find the last separator
        int lastSep = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
        string basename = lastSep >= 0 ? path.Substring(lastSep + 1) : path;

        // Remove suffix if provided
        if (!string.IsNullOrEmpty(suffix) && basename.EndsWith(suffix))
        {
            basename = basename.Substring(0, basename.Length - suffix.Length);
        }

        return basename;
    }

    public string Dirname(string path)
    {
        if (string.IsNullOrEmpty(path))
            return ".";

        // Remove trailing separators
        path = path.TrimEnd('/', '\\');
        if (string.IsNullOrEmpty(path))
            return _isWindows ? "\\" : "/";

        // Find the last separator
        int lastSep = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
        
        if (lastSep < 0)
            return ".";

        if (lastSep == 0)
            return _isWindows ? "\\" : "/";

        // Handle Windows drive roots like "C:\"
        if (_isWindows && lastSep == 2 && path.Length >= 3 && path[1] == ':')
            return path.Substring(0, 3);

        return path.Substring(0, lastSep);
    }

    public string Extname(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "";

        string basename = Basename(path);
        int lastDot = basename.LastIndexOf('.');

        // No extension if no dot, or dot is first character, or only dot at end
        if (lastDot <= 0 || lastDot == basename.Length - 1)
            return "";

        return basename.Substring(lastDot);
    }

    public string Format(Dictionary<string, object?> pathObject)
    {
        var dir = pathObject.TryGetValue("dir", out var dirVal) ? dirVal?.ToString() : null;
        var root = pathObject.TryGetValue("root", out var rootVal) ? rootVal?.ToString() : null;
        var base_ = pathObject.TryGetValue("base", out var baseVal) ? baseVal?.ToString() : null;
        var name = pathObject.TryGetValue("name", out var nameVal) ? nameVal?.ToString() : null;
        var ext = pathObject.TryGetValue("ext", out var extVal) ? extVal?.ToString() : null;

        string result = "";

        // If base is provided, use it
        if (!string.IsNullOrEmpty(base_))
        {
            if (!string.IsNullOrEmpty(dir))
            {
                result = dir + Separator + base_;
            }
            else if (!string.IsNullOrEmpty(root))
            {
                result = root + (root.EndsWith(Separator!) ? "" : Separator) + base_;
            }
            else
            {
                result = base_;
            }
        }
        else
        {
            // Build from name and ext
            string filename = name ?? "";
            if (!string.IsNullOrEmpty(ext))
            {
                if (!ext.StartsWith("."))
                    ext = "." + ext;
                filename += ext;
            }

            if (!string.IsNullOrEmpty(dir))
            {
                result = dir + Separator + filename;
            }
            else if (!string.IsNullOrEmpty(root))
            {
                result = root + (root.EndsWith(Separator!) ? "" : Separator) + filename;
            }
            else
            {
                result = filename;
            }
        }

        return result;
    }

    public bool IsAbsolute(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        if (_isWindows)
        {
            // UNC paths
            if (path.StartsWith("\\\\") || path.StartsWith("//"))
                return true;

            // Drive paths like C:\ or C:/
            if (path.Length >= 3 && char.IsLetter(path[0]) && path[1] == ':' && 
                (path[2] == '\\' || path[2] == '/'))
                return true;

            return false;
        }
        else
        {
            return path.StartsWith("/");
        }
    }

    public string Join(params string[] paths)
    {
        if (paths == null || paths.Length == 0)
            return ".";

        var result = new StringBuilder();
        bool first = true;

        foreach (string path in paths)
        {
            if (string.IsNullOrEmpty(path))
                continue;

            if (!first)
            {
                // Add separator if the previous path doesn't end with one
                string currentResult = result.ToString();
                if (!currentResult.EndsWith(Separator!) && !currentResult.EndsWith("/") && !currentResult.EndsWith("\\"))
                {
                    result.Append(Separator);
                }
            }

            // Remove leading separators from non-first paths to avoid double separators
            string pathToAdd = path;
            if (!first)
            {
                pathToAdd = path.TrimStart('/', '\\');
            }

            result.Append(pathToAdd);
            first = false;
        }

        string joined = result.ToString();
        return string.IsNullOrEmpty(joined) ? "." : Normalize(joined);
    }

    public string Normalize(string path)
    {
        if (string.IsNullOrEmpty(path))
            return ".";

        bool isAbsolute = IsAbsolute(path);
        bool trailingSeparator = path.EndsWith("/") || path.EndsWith("\\");

        // Split path into segments
        var segments = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        var normalized = new List<string>();

        // For Windows paths, skip the drive part (e.g., "C:") when processing segments
        int startIndex = 0;
        bool isWindowsDrive = _isWindows && segments.Count > 0 && segments[0].Length == 2 && 
                             char.IsLetter(segments[0][0]) && segments[0][1] == ':';
        if (isWindowsDrive)
        {
            startIndex = 1; // Skip the drive segment (e.g., "C:")
        }

        for (int i = startIndex; i < segments.Count; i++)
        {
            string segment = segments[i];
            if (segment == "." || string.IsNullOrEmpty(segment))
                continue;

            if (segment == "..")
            {
                if (normalized.Count > 0 && normalized.Last() != "..")
                {
                    normalized.RemoveAt(normalized.Count - 1);
                }
                else if (!isAbsolute)
                {
                    normalized.Add("..");
                }
            }
            else
            {
                normalized.Add(segment);
            }
        }

        string result;
        if (isAbsolute)
        {
            if (_isWindows && path.Length >= 3 && char.IsLetter(path[0]) && path[1] == ':' && 
                (path[2] == '\\' || path[2] == '/'))
            {
                // Windows drive path - extract drive with separator (e.g., "C:\")
                string driveWithSeparator = path.Substring(0, 3);
                // Replace forward slash with backslash if needed
                if (driveWithSeparator[2] == '/')
                    driveWithSeparator = driveWithSeparator.Substring(0, 2) + "\\";
                result = driveWithSeparator + string.Join(Separator, normalized);
            }
            else if (_isWindows && (path.StartsWith("\\\\") || path.StartsWith("//")))
            {
                // UNC path
                result = "\\\\" + string.Join(Separator, normalized);
            }
            else
            {
                result = Separator + string.Join(Separator, normalized);
            }
        }
        else
        {
            result = normalized.Count > 0 ? string.Join(Separator, normalized) : ".";
        }

        if (trailingSeparator && !result.EndsWith(Separator!))
            result += Separator;

        return result;
    }

    public Dictionary<string, object?> Parse(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return new Dictionary<string, object?>
            {
                ["root"] = "",
                ["dir"] = "",
                ["base"] = "",
                ["ext"] = "",
                ["name"] = ""
            };
        }

        string root = "";
        string dir = "";
        string base_ = "";
        string ext = "";
        string name = "";

        // Determine root
        if (_isWindows)
        {
            if (path.StartsWith("\\\\") || path.StartsWith("//"))
            {
                // UNC path
                int nextSep = Math.Max(path.IndexOf('\\', 2), path.IndexOf('/', 2));
                if (nextSep > 0)
                {
                    nextSep = Math.Max(path.IndexOf('\\', nextSep + 1), path.IndexOf('/', nextSep + 1));
                    root = nextSep > 0 ? path.Substring(0, nextSep + 1) : path + "\\";
                }
                else
                {
                    root = path + "\\";
                }
            }
            else if (path.Length >= 3 && char.IsLetter(path[0]) && path[1] == ':' && 
                     (path[2] == '\\' || path[2] == '/'))
            {
                root = path.Substring(0, 3);
            }
        }
        else if (path.StartsWith("/"))
        {
            root = "/";
        }

        // Get basename and directory
        base_ = Basename(path);
        dir = Dirname(path);

        // Get extension and name
        ext = Extname(path);
        name = string.IsNullOrEmpty(ext) ? base_ : base_.Substring(0, base_.Length - ext.Length);

        return new Dictionary<string, object?>
        {
            ["root"] = root,
            ["dir"] = dir,
            ["base"] = base_,
            ["ext"] = ext,
            ["name"] = name
        };
    }

    public string Relative(string from, string to)
    {
        if (string.IsNullOrEmpty(from))
            from = ".";
        if (string.IsNullOrEmpty(to))
            to = ".";

        string resolvedFrom = Resolve(from);
        string resolvedTo = Resolve(to);

        if (resolvedFrom == resolvedTo)
            return "";

        // Split paths into segments
        var fromSegments = resolvedFrom.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        var toSegments = resolvedTo.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

        // Find common prefix
        int commonLength = 0;
        int minLength = Math.Min(fromSegments.Length, toSegments.Length);

        for (int i = 0; i < minLength; i++)
        {
            if (_isWindows ? 
                string.Equals(fromSegments[i], toSegments[i], StringComparison.OrdinalIgnoreCase) :
                fromSegments[i] == toSegments[i])
            {
                commonLength++;
            }
            else
            {
                break;
            }
        }

        // Build relative path
        var result = new List<string>();

        // Add ".." for each remaining segment in from
        for (int i = commonLength; i < fromSegments.Length; i++)
        {
            result.Add("..");
        }

        // Add remaining segments from to
        for (int i = commonLength; i < toSegments.Length; i++)
        {
            result.Add(toSegments[i]);
        }

        return result.Count > 0 ? string.Join(Separator, result) : ".";
    }

    public string Resolve(params string[] paths)
    {
        if (paths == null || paths.Length == 0)
            return Directory.GetCurrentDirectory();

        string resolved = "";
        bool isAbsolute = false;

        // Process paths from left to right (Node.js behavior)
        foreach (string path in paths)
        {
            if (string.IsNullOrEmpty(path))
                continue;

            if (IsAbsolute(path))
            {
                // Absolute path resets the resolution
                resolved = path;
                isAbsolute = true;
            }
            else
            {
                // Relative path appends to current resolution
                if (string.IsNullOrEmpty(resolved))
                {
                    resolved = path;
                }
                else
                {
                    resolved = resolved + Separator + path;
                }
            }
        }

        // If no absolute path was found, prepend current working directory
        if (!isAbsolute)
        {
            resolved = Directory.GetCurrentDirectory() + Separator + resolved;
        }

        return Normalize(resolved);
    }

    public string ToNamespacedPath(string path)
    {
        if (!_isWindows || string.IsNullOrEmpty(path))
            return path;

        // Already namespaced
        if (path.StartsWith("\\\\?\\"))
            return path;

        // UNC paths
        if (path.StartsWith("\\\\"))
            return "\\\\?\\UNC\\" + path.Substring(2);

        // Absolute paths
        if (IsAbsolute(path))
            return "\\\\?\\" + path;

        return path;
    }

    public bool MatchesGlob(string path, string pattern)
    {
        // Simple glob matching implementation
        // Convert glob pattern to regex
        string regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".")
            + "$";

        var options = _isWindows ? RegexOptions.IgnoreCase : RegexOptions.None;
        return Regex.IsMatch(path, regexPattern, options);
    }
}

public class PathMethodFunction
{
    private readonly NodePathModule _pathModule;
    private readonly string _methodName;

    public PathMethodFunction(NodePathModule pathModule, string methodName)
    {
        _pathModule = pathModule;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        return _methodName switch
        {
            "basename" => _pathModule.Basename(
                arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : "",
                arguments.Count > 1 ? arguments[1]?.ToString() : null),
            
            "dirname" => _pathModule.Dirname(
                arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : ""),
            
            "extname" => _pathModule.Extname(
                arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : ""),
            
            "format" => _pathModule.Format(
                arguments.Count > 0 && arguments[0] is Dictionary<string, object?> dict 
                    ? dict 
                    : new Dictionary<string, object?>()),
            
            "isAbsolute" => _pathModule.IsAbsolute(
                arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : ""),
            
            "join" => _pathModule.Join(
                arguments.Select(arg => arg?.ToString() ?? "").ToArray()),
            
            "normalize" => _pathModule.Normalize(
                arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : ""),
            
            "parse" => _pathModule.Parse(
                arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : ""),
            
            "relative" => _pathModule.Relative(
                arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : "",
                arguments.Count > 1 ? arguments[1]?.ToString() ?? "" : ""),
            
            "resolve" => _pathModule.Resolve(
                arguments.Select(arg => arg?.ToString() ?? "").ToArray()),
            
            "toNamespacedPath" => _pathModule.ToNamespacedPath(
                arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : ""),
            
            "matchesGlob" => _pathModule.MatchesGlob(
                arguments.Count > 0 ? arguments[0]?.ToString() ?? "" : "",
                arguments.Count > 1 ? arguments[1]?.ToString() ?? "" : ""),
            
            _ => throw new ECEngineException($"Unknown path method: {_methodName}", 0, 0, "", "Runtime error")
        };
    }
}

public static class PathGlobals
{
    public static Dictionary<string, object?> GetPathModule()
    {
        // Detect current platform
        bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows);
        
        var pathModule = new NodePathModule(isWindows);
        var posixModule = new NodePathModule(false);
        var win32Module = new NodePathModule(true);

        var result = new Dictionary<string, object?>
        {
            ["basename"] = new PathMethodFunction(pathModule, "basename"),
            ["dirname"] = new PathMethodFunction(pathModule, "dirname"),
            ["extname"] = new PathMethodFunction(pathModule, "extname"),
            ["format"] = new PathMethodFunction(pathModule, "format"),
            ["isAbsolute"] = new PathMethodFunction(pathModule, "isAbsolute"),
            ["join"] = new PathMethodFunction(pathModule, "join"),
            ["normalize"] = new PathMethodFunction(pathModule, "normalize"),
            ["parse"] = new PathMethodFunction(pathModule, "parse"),
            ["relative"] = new PathMethodFunction(pathModule, "relative"),
            ["resolve"] = new PathMethodFunction(pathModule, "resolve"),
            ["toNamespacedPath"] = new PathMethodFunction(pathModule, "toNamespacedPath"),
            ["matchesGlob"] = new PathMethodFunction(pathModule, "matchesGlob"),
            ["delimiter"] = pathModule.Delimiter,
            ["sep"] = pathModule.Separator,
            ["posix"] = new Dictionary<string, object?>
            {
                ["basename"] = new PathMethodFunction(posixModule, "basename"),
                ["dirname"] = new PathMethodFunction(posixModule, "dirname"),
                ["extname"] = new PathMethodFunction(posixModule, "extname"),
                ["format"] = new PathMethodFunction(posixModule, "format"),
                ["isAbsolute"] = new PathMethodFunction(posixModule, "isAbsolute"),
                ["join"] = new PathMethodFunction(posixModule, "join"),
                ["normalize"] = new PathMethodFunction(posixModule, "normalize"),
                ["parse"] = new PathMethodFunction(posixModule, "parse"),
                ["relative"] = new PathMethodFunction(posixModule, "relative"),
                ["resolve"] = new PathMethodFunction(posixModule, "resolve"),
                ["toNamespacedPath"] = new PathMethodFunction(posixModule, "toNamespacedPath"),
                ["matchesGlob"] = new PathMethodFunction(posixModule, "matchesGlob"),
                ["delimiter"] = posixModule.Delimiter,
                ["sep"] = posixModule.Separator
            },
            ["win32"] = new Dictionary<string, object?>
            {
                ["basename"] = new PathMethodFunction(win32Module, "basename"),
                ["dirname"] = new PathMethodFunction(win32Module, "dirname"),
                ["extname"] = new PathMethodFunction(win32Module, "extname"),
                ["format"] = new PathMethodFunction(win32Module, "format"),
                ["isAbsolute"] = new PathMethodFunction(win32Module, "isAbsolute"),
                ["join"] = new PathMethodFunction(win32Module, "join"),
                ["normalize"] = new PathMethodFunction(win32Module, "normalize"),
                ["parse"] = new PathMethodFunction(win32Module, "parse"),
                ["relative"] = new PathMethodFunction(win32Module, "relative"),
                ["resolve"] = new PathMethodFunction(win32Module, "resolve"),
                ["toNamespacedPath"] = new PathMethodFunction(win32Module, "toNamespacedPath"),
                ["matchesGlob"] = new PathMethodFunction(win32Module, "matchesGlob"),
                ["delimiter"] = win32Module.Delimiter,
                ["sep"] = win32Module.Separator
            }
        };
        
        return result;
    }
}
