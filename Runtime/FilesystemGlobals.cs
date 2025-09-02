using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ECEngine.Runtime;
using ECEngine.Runtime.Streams;

namespace ECEngine.Runtime;

/// <summary>
/// Filesystem module providing Node.js-like filesystem operations
/// </summary>
public class FilesystemModule
{
    // File read/write operations
    public ReadFileFunction readFile { get; }
    public ReadFileSyncFunction readFileSync { get; }
    public WriteFileFunction writeFile { get; }
    public WriteFileSyncFunction writeFileSync { get; }
    public AppendFileFunction appendFile { get; }
    public AppendFileSyncFunction appendFileSync { get; }
    
    // File status operations
    public ExistsFunction exists { get; }
    public ExistsSyncFunction existsSync { get; }
    public StatFunction stat { get; }
    public StatSyncFunction statSync { get; }
    
    // Directory operations
    public MkdirFunction mkdir { get; }
    public MkdirSyncFunction mkdirSync { get; }
    public RmdirFunction rmdir { get; }
    public RmdirSyncFunction rmdirSync { get; }
    public ReaddirFunction readdir { get; }
    public ReaddirSyncFunction readdirSync { get; }
    
    // File/directory manipulation
    public UnlinkFunction unlink { get; }
    public UnlinkSyncFunction unlinkSync { get; }
    public RenameFunction rename { get; }
    public RenameSyncFunction renameSync { get; }
    public CopyFileFunction copyFile { get; }
    public CopyFileSyncFunction copyFileSync { get; }
    
    // File path operations
    public RealpathFunction realpath { get; }
    public RealpathSyncFunction realpathSync { get; }
    
    // Constants
    public FilesystemConstants constants { get; }

    public FilesystemModule()
    {
        // File read/write operations
        readFile = new ReadFileFunction();
        readFileSync = new ReadFileSyncFunction();
        writeFile = new WriteFileFunction();
        writeFileSync = new WriteFileSyncFunction();
        appendFile = new AppendFileFunction();
        appendFileSync = new AppendFileSyncFunction();
        
        // File status operations
        exists = new ExistsFunction();
        existsSync = new ExistsSyncFunction();
        stat = new StatFunction();
        statSync = new StatSyncFunction();
        
        // Directory operations
        mkdir = new MkdirFunction();
        mkdirSync = new MkdirSyncFunction();
        rmdir = new RmdirFunction();
        rmdirSync = new RmdirSyncFunction();
        readdir = new ReaddirFunction();
        readdirSync = new ReaddirSyncFunction();
        
        // File/directory manipulation
        unlink = new UnlinkFunction();
        unlinkSync = new UnlinkSyncFunction();
        rename = new RenameFunction();
        renameSync = new RenameSyncFunction();
        copyFile = new CopyFileFunction();
        copyFileSync = new CopyFileSyncFunction();
        
        // File permissions and metadata
        realpath = new RealpathFunction();
        realpathSync = new RealpathSyncFunction();
        
        // Constants
        constants = new FilesystemConstants();
    }
}

/// <summary>
/// Filesystem constants similar to Node.js fs.constants
/// </summary>
public class FilesystemConstants
{
    // File access constants
    public double F_OK { get; } = 0; // File exists
    public double R_OK { get; } = 4; // File is readable
    public double W_OK { get; } = 2; // File is writable
    public double X_OK { get; } = 1; // File is executable

    // File copy constants
    public double COPYFILE_EXCL { get; } = 1; // Fail if destination exists
    public double COPYFILE_FICLONE { get; } = 2; // Use copy-on-write
    public double COPYFILE_FICLONE_FORCE { get; } = 4; // Force copy-on-write

    // File open constants
    public double O_RDONLY { get; } = 0; // Read only
    public double O_WRONLY { get; } = 1; // Write only
    public double O_RDWR { get; } = 2; // Read/write
    public double O_CREAT { get; } = 64; // Create file
    public double O_EXCL { get; } = 128; // Fail if file exists
    public double O_TRUNC { get; } = 512; // Truncate file
    public double O_APPEND { get; } = 1024; // Append mode

    // File type constants
    public double S_IFMT { get; } = 61440; // File type mask
    public double S_IFREG { get; } = 32768; // Regular file
    public double S_IFDIR { get; } = 16384; // Directory
    public double S_IFCHR { get; } = 8192; // Character device
    public double S_IFBLK { get; } = 24576; // Block device
    public double S_IFIFO { get; } = 4096; // FIFO
    public double S_IFLNK { get; } = 40960; // Symbolic link
    public double S_IFSOCK { get; } = 49152; // Socket

    // File permission constants
    public double S_IRWXU { get; } = 448; // Owner read/write/execute
    public double S_IRUSR { get; } = 256; // Owner read
    public double S_IWUSR { get; } = 128; // Owner write
    public double S_IXUSR { get; } = 64; // Owner execute
    public double S_IRWXG { get; } = 56; // Group read/write/execute
    public double S_IRGRP { get; } = 32; // Group read
    public double S_IWGRP { get; } = 16; // Group write
    public double S_IXGRP { get; } = 8; // Group execute
    public double S_IRWXO { get; } = 7; // Others read/write/execute
    public double S_IROTH { get; } = 4; // Others read
    public double S_IWOTH { get; } = 2; // Others write
    public double S_IXOTH { get; } = 1; // Others execute
}

// Additional essential filesystem functions
public class AppendFileFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 3) throw new ECEngineException("appendFile requires path, data, and callback", 1, 1, "", "Missing required arguments for appendFile");
        var path = args[0]?.ToString() ?? throw new ECEngineException("Path is required", 1, 1, "", "Invalid path argument for appendFile");
        var data = args[1]?.ToString() ?? "";
        var callback = args[2];
        
        var normalizedPath = NormalizePath(path);
        
        Task.Run(() =>
        {
            try
            {
                EnsureDirectoryExists(normalizedPath);
                File.AppendAllText(normalizedPath, data);
                
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { null });
                }
            }
            catch (Exception ex)
            {
                var error = new { code = "EUNKNOWN", message = ex.Message };
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { error });
                }
            }
        });
        
        return null;
    }
}

public class AppendFileSyncFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 2) throw new ECEngineException("appendFileSync requires path and data", 1, 1, "", "Missing required arguments for appendFileSync");
        var path = args[0]?.ToString() ?? throw new ECEngineException("Path is required", 1, 1, "", "Invalid path argument for appendFileSync");
        var data = args[1]?.ToString() ?? "";
        
        var normalizedPath = NormalizePath(path);
        
        try
        {
            EnsureDirectoryExists(normalizedPath);
            File.AppendAllText(normalizedPath, data);
            return null;
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Failed to append file: {ex.Message}", 1, 1, "", "Runtime error");
        }
    }
}

public class ExistsSyncFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length == 0) throw new ECEngineException("existsSync requires a path", 1, 1, "", "Runtime error");
        var path = args[0]?.ToString() ?? throw new ECEngineException("Path is required", 1, 1, "", "Runtime error");
        var normalizedPath = NormalizePath(path);
        return File.Exists(normalizedPath) || Directory.Exists(normalizedPath);
    }
}

public class CopyFileFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 3) throw new ECEngineException("copyFile requires src, dest, and callback", 1, 1, "", "Runtime error");
        var src = args[0]?.ToString() ?? throw new ECEngineException("Source path is required", 1, 1, "", "Runtime error");
        var dest = args[1]?.ToString() ?? throw new ECEngineException("Destination path is required", 1, 1, "", "Runtime error");
        var callback = args[args.Length - 1];
        
        var normalizedSrc = NormalizePath(src);
        var normalizedDest = NormalizePath(dest);
        
        Task.Run(() =>
        {
            try
            {
                EnsureDirectoryExists(normalizedDest);
                File.Copy(normalizedSrc, normalizedDest);
                
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { null });
                }
            }
            catch (Exception ex)
            {
                var error = new { code = "EUNKNOWN", message = ex.Message };
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { error });
                }
            }
        });
        
        return null;
    }
}

public class CopyFileSyncFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 2) throw new ECEngineException("copyFileSync requires src and dest", 1, 1, "", "Runtime error");
        var src = args[0]?.ToString() ?? throw new ECEngineException("Source path is required", 1, 1, "", "Runtime error");
        var dest = args[1]?.ToString() ?? throw new ECEngineException("Destination path is required", 1, 1, "", "Runtime error");
        
        var normalizedSrc = NormalizePath(src);
        var normalizedDest = NormalizePath(dest);
        
        try
        {
            EnsureDirectoryExists(normalizedDest);
            File.Copy(normalizedSrc, normalizedDest);
            return null;
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Failed to copy file: {ex.Message}", 1, 1, "", "Runtime error");
        }
    }
}

public class RenameFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 3) throw new ECEngineException("rename requires oldPath, newPath, and callback", 1, 1, "", "Runtime error");
        var oldPath = args[0]?.ToString() ?? throw new ECEngineException("Old path is required", 1, 1, "", "Runtime error");
        var newPath = args[1]?.ToString() ?? throw new ECEngineException("New path is required", 1, 1, "", "Runtime error");
        var callback = args[2];
        
        var normalizedOldPath = NormalizePath(oldPath);
        var normalizedNewPath = NormalizePath(newPath);
        
        Task.Run(() =>
        {
            try
            {
                EnsureDirectoryExists(normalizedNewPath);
                
                if (File.Exists(normalizedOldPath))
                {
                    File.Move(normalizedOldPath, normalizedNewPath);
                }
                else if (Directory.Exists(normalizedOldPath))
                {
                    Directory.Move(normalizedOldPath, normalizedNewPath);
                }
                else
                {
                    throw new FileNotFoundException($"Path not found: {normalizedOldPath}");
                }
                
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { null });
                }
            }
            catch (Exception ex)
            {
                var error = new { code = "EUNKNOWN", message = ex.Message };
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { error });
                }
            }
        });
        
        return null;
    }
}

public class RenameSyncFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 2) throw new ECEngineException("renameSync requires oldPath and newPath", 1, 1, "", "Runtime error");
        var oldPath = args[0]?.ToString() ?? throw new ECEngineException("Old path is required", 1, 1, "", "Runtime error");
        var newPath = args[1]?.ToString() ?? throw new ECEngineException("New path is required", 1, 1, "", "Runtime error");
        
        var normalizedOldPath = NormalizePath(oldPath);
        var normalizedNewPath = NormalizePath(newPath);
        
        try
        {
            EnsureDirectoryExists(normalizedNewPath);
            
            if (File.Exists(normalizedOldPath))
            {
                File.Move(normalizedOldPath, normalizedNewPath);
            }
            else if (Directory.Exists(normalizedOldPath))
            {
                Directory.Move(normalizedOldPath, normalizedNewPath);
            }
            else
            {
                throw new FileNotFoundException($"Path not found: {normalizedOldPath}");
            }
            
            return null;
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Failed to rename: {ex.Message}", 1, 1, "", "Runtime error");
        }
    }
}

public class RealpathFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 2) throw new ECEngineException("realpath requires path and callback", 1, 1, "", "Runtime error");
        var path = args[0]?.ToString() ?? throw new ECEngineException("Path is required", 1, 1, "", "Runtime error");
        var callback = args[1];
        
        Task.Run(() =>
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { null, fullPath });
                }
            }
            catch (Exception ex)
            {
                var error = new { code = "EUNKNOWN", message = ex.Message };
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { error, null });
                }
            }
        });
        
        return null;
    }
}

public class RealpathSyncFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length == 0) throw new ECEngineException("realpathSync requires a path", 1, 1, "", "Runtime error");
        var path = args[0]?.ToString() ?? throw new ECEngineException("Path is required", 1, 1, "", "Runtime error");
        
        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Failed to resolve path: {ex.Message}", 1, 1, "", "Runtime error");
        }
    }
}

/// <summary>
/// Base class for filesystem operations with common utilities
/// </summary>
public abstract class FilesystemOperationFunction
{
    protected string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path cannot be null or empty", 1, 1, "", "Invalid path provided to filesystem operation");

        // Handle relative paths
        if (!Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(path);
        }

        return path;
    }

    protected void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}

/// <summary>
/// fs.readFile(path, [options], callback)
/// </summary>
public class ReadFileFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 2)
            throw new ECEngineException("fs.readFile requires at least 2 arguments: path and callback", 1, 1, "", "Missing required arguments for fs.readFile");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.readFile");

        // Handle optional encoding parameter
        string? encoding = "utf8";
        object? callback = null;

        if (args.Length == 2)
        {
            callback = args[1];
        }
        else if (args.Length == 3)
        {
            if (args[1] is string enc)
            {
                encoding = enc;
            }
            callback = args[2];
        }

        var normalizedPath = NormalizePath(path);

        // Simulate async operation
        Task.Run(() =>
        {
            try
            {
                if (!File.Exists(normalizedPath))
                {
                    var error = new { code = "ENOENT", message = $"no such file or directory, open '{normalizedPath}'" };
                    // Call callback with error
                    if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                    {
                        callbackFunc(new object?[] { error, null });
                    }
                    return;
                }

                var content = File.ReadAllText(normalizedPath);
                
                // Call callback with success
                if (callback is VariableInfo callbackVar2 && callbackVar2.Value is Func<object?[], object?> callbackFunc2)
                {
                    callbackFunc2(new object?[] { null, content });
                }
            }
            catch (Exception ex)
            {
                var error = new { code = "EUNKNOWN", message = ex.Message };
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { error, null });
                }
            }
        });

        return null; // fs.readFile returns undefined
    }
}

/// <summary>
/// fs.readFileSync(path, [options])
/// </summary>
public class ReadFileSyncFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 1)
            throw new ECEngineException("fs.readFileSync requires at least 1 argument: path", 1, 1, "", "Missing required path argument for fs.readFileSync");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.readFileSync");

        var normalizedPath = NormalizePath(path);

        try
        {
            if (!File.Exists(normalizedPath))
                throw new ECEngineException($"ENOENT: no such file or directory, open '{normalizedPath}'", 1, 1, "", "File not found");

            return File.ReadAllText(normalizedPath);
        }
        catch (ECEngineException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Error reading file: {ex.Message}", 1, 1, "", ex.Message);
        }
    }
}

/// <summary>
/// fs.writeFile(path, data, [options], callback)
/// </summary>
public class WriteFileFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 3)
            throw new ECEngineException("fs.writeFile requires at least 3 arguments: path, data, and callback", 1, 1, "", "Missing required arguments for fs.writeFile");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.writeFile");

        var data = args[1]?.ToString() ?? "";
        var callback = args[^1]; // Last argument is always callback

        var normalizedPath = NormalizePath(path);

        // Simulate async operation
        Task.Run(() =>
        {
            try
            {
                EnsureDirectoryExists(normalizedPath);
                File.WriteAllText(normalizedPath, data);
                
                // Call callback with success
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { null });
                }
            }
            catch (Exception ex)
            {
                var error = new { code = "EUNKNOWN", message = ex.Message };
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { error });
                }
            }
        });

        return null; // fs.writeFile returns undefined
    }
}

/// <summary>
/// fs.writeFileSync(path, data, [options])
/// </summary>
public class WriteFileSyncFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 2)
            throw new ECEngineException("fs.writeFileSync requires at least 2 arguments: path and data", 1, 1, "", "Missing required arguments for fs.writeFileSync");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.writeFileSync");

        var data = args[1]?.ToString() ?? "";
        var normalizedPath = NormalizePath(path);

        try
        {
            EnsureDirectoryExists(normalizedPath);
            File.WriteAllText(normalizedPath, data);
            return null; // fs.writeFileSync returns undefined
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Error writing file: {ex.Message}", 1, 1, "", ex.Message);
        }
    }
}

/// <summary>
/// fs.exists(path, callback) - deprecated but commonly used
/// </summary>
public class ExistsFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 2)
            throw new ECEngineException("fs.exists requires 2 arguments: path and callback", 1, 1, "", "Missing required arguments for fs.exists");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.exists");

        var callback = args[1];
        var normalizedPath = NormalizePath(path);

        // Simulate async operation
        Task.Run(() =>
        {
            try
            {
                var exists = File.Exists(normalizedPath) || Directory.Exists(normalizedPath);
                
                // Call callback with result
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { exists });
                }
            }
            catch
            {
                // In case of error, assume it doesn't exist
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { false });
                }
            }
        });

        return null;
    }
}

/// <summary>
/// fs.stat(path, callback)
/// </summary>
public class StatFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 2)
            throw new ECEngineException("fs.stat requires 2 arguments: path and callback", 1, 1, "", "Missing required arguments for fs.stat");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.stat");

        var callback = args[1];
        var normalizedPath = NormalizePath(path);

        // Simulate async operation
        Task.Run(() =>
        {
            try
            {
                if (!File.Exists(normalizedPath) && !Directory.Exists(normalizedPath))
                {
                    var error = new { code = "ENOENT", message = $"no such file or directory, stat '{normalizedPath}'" };
                    if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                    {
                        callbackFunc(new object?[] { error, null });
                    }
                    return;
                }

                var info = File.Exists(normalizedPath) ? new FileInfo(normalizedPath) : new DirectoryInfo(normalizedPath) as FileSystemInfo;
                var stats = new FileStats(normalizedPath, info);
                
                if (callback is VariableInfo callbackVar2 && callbackVar2.Value is Func<object?[], object?> callbackFunc2)
                {
                    callbackFunc2(new object?[] { null, stats });
                }
            }
            catch (Exception ex)
            {
                var error = new { code = "EUNKNOWN", message = ex.Message };
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { error, null });
                }
            }
        });

        return null;
    }
}

/// <summary>
/// fs.statSync(path)
/// </summary>
public class StatSyncFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 1)
            throw new ECEngineException("fs.statSync requires 1 argument: path", 1, 1, "", "Missing required path argument for fs.statSync");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.statSync");

        var normalizedPath = NormalizePath(path);

        try
        {
            if (!File.Exists(normalizedPath) && !Directory.Exists(normalizedPath))
                throw new ECEngineException($"ENOENT: no such file or directory, stat '{normalizedPath}'", 1, 1, "", "File not found");

            var info = File.Exists(normalizedPath) ? new FileInfo(normalizedPath) : new DirectoryInfo(normalizedPath) as FileSystemInfo;
            return new FileStats(normalizedPath, info);
        }
        catch (ECEngineException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Error getting file stats: {ex.Message}", 1, 1, "", ex.Message);
        }
    }
}

/// <summary>
/// fs.mkdir(path, [options], callback)
/// </summary>
public class MkdirFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 2)
            throw new ECEngineException("fs.mkdir requires at least 2 arguments: path and callback", 1, 1, "", "Missing required arguments for fs.mkdir");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.mkdir");

        var callback = args[^1]; // Last argument is callback
        var normalizedPath = NormalizePath(path);

        // Simulate async operation
        Task.Run(() =>
        {
            try
            {
                if (!Directory.Exists(normalizedPath))
                {
                    Directory.CreateDirectory(normalizedPath);
                }
                
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { null });
                }
            }
            catch (Exception ex)
            {
                var error = new { code = "EUNKNOWN", message = ex.Message };
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { error });
                }
            }
        });

        return null;
    }
}

/// <summary>
/// fs.mkdirSync(path, [options])
/// </summary>
public class MkdirSyncFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 1)
            throw new ECEngineException("fs.mkdirSync requires 1 argument: path", 1, 1, "", "Missing required path argument for fs.mkdirSync");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.mkdirSync");

        var normalizedPath = NormalizePath(path);

        try
        {
            if (!Directory.Exists(normalizedPath))
            {
                Directory.CreateDirectory(normalizedPath);
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Error creating directory: {ex.Message}", 1, 1, "", ex.Message);
        }
    }
}

/// <summary>
/// fs.rmdir(path, callback)
/// </summary>
public class RmdirFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 2)
            throw new ECEngineException("fs.rmdir requires 2 arguments: path and callback", 1, 1, "", "Missing required arguments for fs.rmdir");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.rmdir");

        var callback = args[1];
        var normalizedPath = NormalizePath(path);

        // Simulate async operation
        Task.Run(() =>
        {
            try
            {
                if (Directory.Exists(normalizedPath))
                {
                    Directory.Delete(normalizedPath, false); // Don't delete recursively by default
                }
                
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { null });
                }
            }
            catch (Exception ex)
            {
                var error = new { code = "EUNKNOWN", message = ex.Message };
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { error });
                }
            }
        });

        return null;
    }
}

/// <summary>
/// fs.rmdirSync(path)
/// </summary>
public class RmdirSyncFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 1)
            throw new ECEngineException("fs.rmdirSync requires 1 argument: path", 1, 1, "", "Missing required path argument for fs.rmdirSync");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.rmdirSync");

        var normalizedPath = NormalizePath(path);

        try
        {
            if (Directory.Exists(normalizedPath))
            {
                Directory.Delete(normalizedPath, false);
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Error removing directory: {ex.Message}", 1, 1, "", ex.Message);
        }
    }
}

/// <summary>
/// fs.unlink(path, callback)
/// </summary>
public class UnlinkFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 2)
            throw new ECEngineException("fs.unlink requires 2 arguments: path and callback", 1, 1, "", "Missing required arguments for fs.unlink");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.unlink");

        var callback = args[1];
        var normalizedPath = NormalizePath(path);

        // Simulate async operation
        Task.Run(() =>
        {
            try
            {
                if (File.Exists(normalizedPath))
                {
                    File.Delete(normalizedPath);
                }
                
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { null });
                }
            }
            catch (Exception ex)
            {
                var error = new { code = "EUNKNOWN", message = ex.Message };
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { error });
                }
            }
        });

        return null;
    }
}

/// <summary>
/// fs.unlinkSync(path)
/// </summary>
public class UnlinkSyncFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 1)
            throw new ECEngineException("fs.unlinkSync requires 1 argument: path", 1, 1, "", "Missing required path argument for fs.unlinkSync");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.unlinkSync");

        var normalizedPath = NormalizePath(path);

        try
        {
            if (File.Exists(normalizedPath))
            {
                File.Delete(normalizedPath);
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Error deleting file: {ex.Message}", 1, 1, "", ex.Message);
        }
    }
}

/// <summary>
/// fs.readdir(path, callback)
/// </summary>
public class ReaddirFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 2)
            throw new ECEngineException("fs.readdir requires 2 arguments: path and callback", 1, 1, "", "Missing required arguments for fs.readdir");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.readdir");

        var callback = args[1];
        var normalizedPath = NormalizePath(path);

        // Simulate async operation
        Task.Run(() =>
        {
            try
            {
                if (!Directory.Exists(normalizedPath))
                {
                    var error = new { code = "ENOENT", message = $"no such file or directory, scandir '{normalizedPath}'" };
                    if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                    {
                        callbackFunc(new object?[] { error, null });
                    }
                    return;
                }

                var entries = Directory.GetFileSystemEntries(normalizedPath)
                    .Select(Path.GetFileName)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToArray();
                
                if (callback is VariableInfo callbackVar2 && callbackVar2.Value is Func<object?[], object?> callbackFunc2)
                {
                    callbackFunc2(new object?[] { null, entries });
                }
            }
            catch (Exception ex)
            {
                var error = new { code = "EUNKNOWN", message = ex.Message };
                if (callback is VariableInfo callbackVar && callbackVar.Value is Func<object?[], object?> callbackFunc)
                {
                    callbackFunc(new object?[] { error, null });
                }
            }
        });

        return null;
    }
}

/// <summary>
/// fs.readdirSync(path)
/// </summary>
public class ReaddirSyncFunction : FilesystemOperationFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 1)
            throw new ECEngineException("fs.readdirSync requires 1 argument: path", 1, 1, "", "Missing required path argument for fs.readdirSync");

        var path = args[0]?.ToString();
        if (string.IsNullOrEmpty(path))
            throw new ECEngineException("Path must be a string", 1, 1, "", "Invalid path type for fs.readdirSync");

        var normalizedPath = NormalizePath(path);

        try
        {
            if (!Directory.Exists(normalizedPath))
                throw new ECEngineException($"ENOENT: no such file or directory, scandir '{normalizedPath}'", 1, 1, "", "Directory not found");

            return Directory.GetFileSystemEntries(normalizedPath)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray();
        }
        catch (ECEngineException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Error reading directory: {ex.Message}", 1, 1, "", ex.Message);
        }
    }
}

/// <summary>
/// require(module) function for Node.js-style module loading
/// </summary>
public class RequireFunction
{
    private readonly ModuleSystem _moduleSystem;
    private readonly Interpreter _interpreter;

    public RequireFunction(ModuleSystem moduleSystem, Interpreter interpreter)
    {
        _moduleSystem = moduleSystem;
        _interpreter = interpreter;
    }

    public object? Call(object?[] args)
    {
        if (args.Length < 1)
            throw new ECEngineException("require() expects 1 argument", 1, 1, "", "Missing module name argument for require()");

        var moduleName = args[0]?.ToString();
        if (string.IsNullOrEmpty(moduleName))
            throw new ECEngineException("Module name must be a string", 1, 1, "", "Invalid module name type for require()");

        // Handle built-in modules
        if (IsBuiltInModule(moduleName))
        {
            return GetBuiltInModule(moduleName);
        }

        // Handle regular file modules through the module system
        var module = _moduleSystem.LoadModule(moduleName, _interpreter);
        if (module == null)
        {
            throw new ECEngineException($"Cannot find module '{moduleName}'", 1, 1, "", "Module not found");
        }

        // Return the module's exports
        if (module.Exports.ContainsKey("default"))
        {
            return module.Exports["default"];
        }

        return module.Exports;
    }

    private bool IsBuiltInModule(string moduleName)
    {
        var baseModuleName = moduleName.StartsWith("node:") ? moduleName.Substring(5) : moduleName;
        
        return baseModuleName switch
        {
            "fs" => true,
            "path" => true,
            "os" => true,
            "util" => true,
            "url" => true,
            "stream" => true,
            "console" => true,
            "events" => true,
            "process" => true,
            "querystring" => true,
            "http" => true,
            _ => false
        };
    }

    private object? GetBuiltInModule(string moduleName)
    {
        // Strip node: prefix if present for Node.js compatibility
        var baseModuleName = moduleName.StartsWith("node:") ? moduleName.Substring(5) : moduleName;
        
        return baseModuleName switch
        {
            "fs" => new FilesystemModule(),
            "path" => GetPathModule(),
            "os" => new OSModule(),
            "util" => new Runtime.UtilModule(),
            "url" => new Runtime.UrlModule(),
            "stream" => new StreamModule(),
            "console" => GetConsoleModule(),
            "events" => GetEventsModule(),
            "process" => GetProcessModule(),
            "querystring" => GetQuerystringModule(),
            "http" => GetHttpModule(),
            _ => throw new ECEngineException($"Built-in module '{moduleName}' is not implemented", 1, 1, "", "Module not implemented")
        };
    }

    private object? GetConsoleModule()
    {
        // Return an object with console methods
        var console = new Dictionary<string, object?>
        {
            { "log", new ConsoleLogFunction() }
        };
        return console;
    }

    private object? GetEventsModule()
    {
        // Return EventEmitter constructor
        return new Dictionary<string, object?>
        {
            { "EventEmitter", new EventEmitterCreateFunction(new EventEmitterModule()) }
        };
    }

    private object? GetProcessModule()
    {
        // Return the global process object
        return ProcessGlobals.CreateProcessObject(null);
    }

    private object? GetQuerystringModule()
    {
        // Return querystring module
        return new Runtime.QuerystringModule();
    }

    private object? GetHttpModule()
    {
        // Return HTTP module
        return new Dictionary<string, object?>
        {
            { "createServer", new CreateServerFunction(null, null) }
        };
    }

    private object? GetPathModule()
    {
        // Return path module
        return new Runtime.PathModule();
    }
}

/// <summary>
/// Node.js path module equivalent
/// </summary>
public class PathModule
{
    public JoinFunction join { get; }
    public ResolveFunction resolve { get; }
    public DirnameFunction dirname { get; }
    public BasenameFunction basename { get; }
    public ExtnameFunction extname { get; }

    public PathModule()
    {
        join = new JoinFunction();
        resolve = new ResolveFunction();
        dirname = new DirnameFunction();
        basename = new BasenameFunction();
        extname = new ExtnameFunction();
    }
}

public class JoinFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length == 0)
            return ".";

        var paths = args.Where(arg => !string.IsNullOrEmpty(arg?.ToString()))
                        .Select(arg => arg!.ToString()!)
                        .ToArray();

        if (paths.Length == 0)
            return ".";

        return Path.Combine(paths);
    }
}

public class ResolveFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length == 0)
            return Directory.GetCurrentDirectory();

        var paths = args.Select(arg => arg?.ToString() ?? "").ToArray();
        var basePath = Directory.GetCurrentDirectory();

        foreach (var path in paths)
        {
            if (Path.IsPathRooted(path))
            {
                basePath = path;
            }
            else
            {
                basePath = Path.Combine(basePath, path);
            }
        }

        return Path.GetFullPath(basePath);
    }
}

public class DirnameFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 1 || string.IsNullOrEmpty(args[0]?.ToString()))
            throw new ECEngineException("path.dirname requires a path argument", 1, 1, "", "Missing path argument");

        var path = args[0]!.ToString()!;
        var directory = Path.GetDirectoryName(path);
        return string.IsNullOrEmpty(directory) ? "." : directory;
    }
}

public class BasenameFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 1 || string.IsNullOrEmpty(args[0]?.ToString()))
            throw new ECEngineException("path.basename requires a path argument", 1, 1, "", "Missing path argument");

        var path = args[0]!.ToString()!;
        var ext = args.Length > 1 ? args[1]?.ToString() : null;
        var basename = Path.GetFileName(path);

        if (!string.IsNullOrEmpty(ext) && basename.EndsWith(ext))
        {
            basename = basename.Substring(0, basename.Length - ext.Length);
        }

        return basename;
    }
}

public class ExtnameFunction
{
    public object? Call(object?[] args)
    {
        if (args.Length < 1 || string.IsNullOrEmpty(args[0]?.ToString()))
            throw new ECEngineException("path.extname requires a path argument", 1, 1, "", "Missing path argument");

        var path = args[0]!.ToString()!;
        return Path.GetExtension(path);
    }
}

/// <summary>
/// Node.js os module equivalent
/// </summary>
public class OSModule
{
    public string platform => GetPlatform();
    public string hostname => Environment.MachineName;
    public string tmpdir => Path.GetTempPath();
    public string homedir => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    private string GetPlatform()
    {
        if (OperatingSystem.IsWindows()) return "win32";
        if (OperatingSystem.IsMacOS()) return "darwin";
        if (OperatingSystem.IsLinux()) return "linux";
        return "unknown";
    }
}

/// <summary>
/// File stats object that mimics Node.js fs.Stats
/// </summary>
public class FileStats
{
    private readonly string _path;
    private readonly FileSystemInfo _info;
    
    public IsFileFunction isFile { get; }
    public IsDirectoryFunction isDirectory { get; }
    public long size => File.Exists(_path) ? new FileInfo(_path).Length : 0;
    public DateTime mtime => _info.LastWriteTime;
    public DateTime ctime => _info.CreationTime;
    public DateTime atime => _info.LastAccessTime;
    
    public FileStats(string path, FileSystemInfo info)
    {
        _path = path;
        _info = info;
        isFile = new IsFileFunction(_path);
        isDirectory = new IsDirectoryFunction(_path);
    }
}

/// <summary>
/// Function for stats.isFile()
/// </summary>
public class IsFileFunction
{
    private readonly string _path;
    
    public IsFileFunction(string path)
    {
        _path = path;
    }
    
    public object? Call(object?[] args)
    {
        return File.Exists(_path);
    }
}

/// <summary>
/// Function for stats.isDirectory()
/// </summary>
public class IsDirectoryFunction
{
    private readonly string _path;
    
    public IsDirectoryFunction(string path)
    {
        _path = path;
    }
    
    public object? Call(object?[] args)
    {
        return Directory.Exists(_path);
    }
}
