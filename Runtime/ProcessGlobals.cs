using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ECEngine.Runtime
{
    /// <summary>
    /// Node.js process global object implementation
    /// Provides information about the current process and environment
    /// </summary>
    public class ProcessObject
    {
        private readonly Dictionary<string, object?> _env;
        private readonly List<object?> _argv;
        private readonly Interpreter _interpreter;
        private readonly string _execPath;
        private readonly string _platform;
        private readonly string _arch;
        private readonly string _version;
        private readonly Dictionary<string, object?> _versions;
        private readonly Dictionary<string, List<Function>> _eventListeners;
        private int _exitCode = 0;
        private bool _isExiting = false;

        public ProcessObject(Interpreter interpreter, string[]? commandLineArgs = null)
        {
            _interpreter = interpreter;
            _eventListeners = new Dictionary<string, List<Function>>();
            
            // Initialize environment variables
            _env = new Dictionary<string, object?>();
            foreach (var kvp in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>())
            {
                _env[kvp.Key.ToString() ?? ""] = kvp.Value?.ToString();
            }

            // Initialize argv (command line arguments)
            _argv = new List<object?>();
            _execPath = GetExecutablePath();
            _argv.Add(_execPath); // argv[0] is the executable path
            _argv.Add(""); // argv[1] is the script path (set later if needed)
            
            if (commandLineArgs != null)
            {
                foreach (var arg in commandLineArgs)
                {
                    _argv.Add(arg);
                }
            }

            // Platform information
            _platform = GetPlatform();
            _arch = GetArchitecture();
            _version = "v18.17.0"; // ECEngine version mimicking Node.js
            
            // Version information
            _versions = new Dictionary<string, object?>
            {
                ["node"] = "18.17.0",
                ["v8"] = "10.2.154.26",
                ["uv"] = "1.44.2",
                ["zlib"] = "1.2.11",
                ["openssl"] = "3.0.5",
                ["modules"] = "108",
                ["ecengine"] = "1.0.0"
            };
        }

        /// <summary>
        /// Get the process environment variables
        /// </summary>
        public Dictionary<string, object?> Env => _env;

        /// <summary>
        /// Get the command line arguments
        /// </summary>
        public List<object?> Argv => _argv;

        /// <summary>
        /// Get the executable path
        /// </summary>
        public string ExecPath => _execPath;

        /// <summary>
        /// Get the platform identifier
        /// </summary>
        public string Platform => _platform;

        /// <summary>
        /// Get the CPU architecture
        /// </summary>
        public string Arch => _arch;

        /// <summary>
        /// Get the Node.js version
        /// </summary>
        public string Version => _version;

        /// <summary>
        /// Get version information for Node.js and its dependencies
        /// </summary>
        public Dictionary<string, object?> Versions => _versions;

        /// <summary>
        /// Get or set the process exit code
        /// </summary>
        public int ExitCode 
        { 
            get => _exitCode;
            set => _exitCode = value;
        }

        /// <summary>
        /// Get the process ID
        /// </summary>
        public int Pid => Process.GetCurrentProcess().Id;

        /// <summary>
        /// Get the parent process ID
        /// </summary>
        public int Ppid 
        { 
            get 
            {
                try
                {
                    var currentProcess = Process.GetCurrentProcess();
                    // Try to get parent process ID (platform-specific)
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        return GetParentProcessIdWindows(currentProcess.Id);
                    }
                    else
                    {
                        return GetParentProcessIdUnix();
                    }
                }
                catch
                {
                    return 0; // Return 0 if unable to determine
                }
            }
        }

        /// <summary>
        /// Get the current working directory
        /// </summary>
        public string Cwd() => Directory.GetCurrentDirectory();

        /// <summary>
        /// Change the current working directory
        /// </summary>
        public void Chdir(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new ECEngineException($"ENOENT: no such file or directory, chdir '{directory}'", 1, 1, "", "Directory not found");
            }
            Directory.SetCurrentDirectory(directory);
        }

        /// <summary>
        /// Exit the process with optional exit code
        /// </summary>
        public void Exit(int? code = null)
        {
            var exitCode = code ?? _exitCode;
            
            if (_isExiting)
                return; // Prevent multiple exit calls
                
            _isExiting = true;
            _exitCode = exitCode;
            
            try
            {
                // Emit 'beforeExit' event if no exit code was provided and exit code is 0
                if (code == null && _exitCode == 0)
                {
                    EmitEvent("beforeExit", new List<object?> { _exitCode });
                }
                
                // Emit 'exit' event - this should be the last event
                EmitEvent("exit", new List<object?> { _exitCode });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during process exit: {ex.Message}");
            }
            finally
            {
                Environment.Exit(_exitCode);
            }
        }

        /// <summary>
        /// Get process memory usage
        /// </summary>
        public Dictionary<string, object?> MemoryUsage()
        {
            var process = Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;
            
            return new Dictionary<string, object?>
            {
                ["rss"] = workingSet, // Resident Set Size
                ["heapTotal"] = GC.GetTotalMemory(false), // Approximate heap total
                ["heapUsed"] = GC.GetTotalMemory(false), // Approximate heap used
                ["external"] = privateMemory - workingSet, // External memory
                ["arrayBuffers"] = 0 // Array buffers (not tracked in .NET)
            };
        }

        /// <summary>
        /// Get process uptime in seconds
        /// </summary>
        public double Uptime()
        {
            var process = Process.GetCurrentProcess();
            return (DateTime.Now - process.StartTime).TotalSeconds;
        }

        /// <summary>
        /// Get high-resolution time
        /// </summary>
        public List<object?> Hrtime(List<object?>? time = null)
        {
            var ticks = DateTime.UtcNow.Ticks;
            var seconds = ticks / TimeSpan.TicksPerSecond;
            var nanoseconds = (ticks % TimeSpan.TicksPerSecond) * 100; // Convert to nanoseconds

            if (time != null && time.Count >= 2)
            {
                // Calculate difference from previous time
                var prevSeconds = Convert.ToInt64(time[0]);
                var prevNanoseconds = Convert.ToInt64(time[1]);
                
                var totalPrevNanos = prevSeconds * 1_000_000_000 + prevNanoseconds;
                var totalCurrentNanos = seconds * 1_000_000_000 + nanoseconds;
                var diffNanos = totalCurrentNanos - totalPrevNanos;
                
                var diffSeconds = diffNanos / 1_000_000_000;
                var remainingNanos = diffNanos % 1_000_000_000;
                
                return new List<object?> { diffSeconds, remainingNanos };
            }

            return new List<object?> { seconds, nanoseconds };
        }

        /// <summary>
        /// Get the next tick queue
        /// </summary>
        public void NextTick(object callback, params object[] args)
        {
            if (callback is Function func)
            {
                // Schedule the callback to run on the next tick
                _interpreter.EventLoop?.NextTick(() =>
                {
                    try
                    {
                        _interpreter.CallUserFunctionPublic(func, args.ToList());
                    }
                    catch (Exception ex)
                    {
                        // Handle errors in next tick callbacks
                        Console.Error.WriteLine($"Error in nextTick callback: {ex.Message}");
                    }
                });
            }
            else
            {
                throw new ECEngineException("Callback must be a function", 1, 1, "", "Invalid callback");
            }
        }

        /// <summary>
        /// Add an event listener for the specified event
        /// </summary>
        public void On(string eventName, Function listener)
        {
            if (!_eventListeners.ContainsKey(eventName))
            {
                _eventListeners[eventName] = new List<Function>();
            }
            _eventListeners[eventName].Add(listener);
        }

        /// <summary>
        /// Remove an event listener for the specified event
        /// </summary>
        public void Off(string eventName, Function? listener = null)
        {
            if (!_eventListeners.ContainsKey(eventName))
                return;

            if (listener == null)
            {
                // Remove all listeners for this event
                _eventListeners[eventName].Clear();
            }
            else
            {
                // Remove specific listener
                _eventListeners[eventName].Remove(listener);
            }
        }

        /// <summary>
        /// Emit an event to all registered listeners
        /// </summary>
        public void EmitEvent(string eventName, List<object?> args)
        {
            if (!_eventListeners.ContainsKey(eventName))
                return;

            var listeners = _eventListeners[eventName].ToList(); // Copy to avoid modification during iteration

            foreach (var listener in listeners)
            {
                try
                {
                    _interpreter.CallUserFunctionPublic(listener, args);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error in process event listener for '{eventName}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get the list of event names that have listeners
        /// </summary>
        public List<string> EventNames()
        {
            return _eventListeners.Keys.Where(key => _eventListeners[key].Count > 0).ToList();
        }

        /// <summary>
        /// Get the number of listeners for a specific event
        /// </summary>
        public int ListenerCount(string eventName)
        {
            return _eventListeners.ContainsKey(eventName) ? _eventListeners[eventName].Count : 0;
        }

        // Helper methods
        private static string GetExecutablePath()
        {
            try
            {
                return Process.GetCurrentProcess().MainModule?.FileName ?? "ecengine";
            }
            catch
            {
                return "ecengine";
            }
        }

        private static string GetPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "win32";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "darwin";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "linux";
            else
                return "unknown";
        }

        private static string GetArchitecture()
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.X86 => "ia32",
                Architecture.Arm => "arm",
                Architecture.Arm64 => "arm64",
                _ => "unknown"
            };
        }

        private static int GetParentProcessIdWindows(int processId)
        {
            // This is a simplified implementation
            // In a real implementation, you'd use Windows APIs
            return 0;
        }

        private static int GetParentProcessIdUnix()
        {
            try
            {
                // Try to read from /proc/self/stat on Linux
                if (File.Exists("/proc/self/stat"))
                {
                    var stat = File.ReadAllText("/proc/self/stat");
                    var parts = stat.Split(' ');
                    if (parts.Length > 3)
                    {
                        return int.Parse(parts[3]);
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
            return 0;
        }
    }

    /// <summary>
    /// Function wrapper for process methods
    /// </summary>
    public class ProcessMethodFunction
    {
        private readonly ProcessObject _process;
        private readonly string _methodName;

        public ProcessMethodFunction(ProcessObject process, string methodName)
        {
            _process = process;
            _methodName = methodName;
        }

        public object? Call(List<object?> arguments)
        {
            switch (_methodName)
            {
                case "cwd":
                    return _process.Cwd();

                case "chdir":
                    if (arguments.Count == 0)
                        throw new ECEngineException("chdir requires a directory argument", 1, 1, "", "Missing argument");
                    _process.Chdir(arguments[0]?.ToString() ?? "");
                    return null;

                case "exit":
                    int? code = arguments.Count > 0 ? Convert.ToInt32(arguments[0]) : null;
                    _process.Exit(code);
                    return null;

                case "memoryUsage":
                    return _process.MemoryUsage();

                case "uptime":
                    return _process.Uptime();

                case "hrtime":
                    var time = arguments.Count > 0 && arguments[0] is List<object?> timeList ? timeList : null;
                    return _process.Hrtime(time);

                case "nextTick":
                    if (arguments.Count == 0)
                        throw new ECEngineException("nextTick requires a callback function", 1, 1, "", "Missing callback");
                    
                    var callback = arguments[0];
                    var args = arguments.Skip(1).ToArray();
                    _process.NextTick(callback, args);
                    return null;

                case "on":
                    if (arguments.Count < 2)
                        throw new ECEngineException("on requires eventName and listener arguments", 1, 1, "", "Missing arguments");
                    
                    var eventName = arguments[0]?.ToString() ?? "";
                    var listener = arguments[1] as Function;
                    if (listener == null)
                        throw new ECEngineException("Listener must be a function", 1, 1, "", "Invalid listener");
                    
                    _process.On(eventName, listener);
                    return null;

                case "off":
                case "removeListener":
                    if (arguments.Count == 0)
                        throw new ECEngineException("off requires at least eventName argument", 1, 1, "", "Missing arguments");
                    
                    var offEventName = arguments[0]?.ToString() ?? "";
                    var offListener = arguments.Count > 1 ? arguments[1] as Function : null;
                    _process.Off(offEventName, offListener);
                    return null;

                case "emit":
                    if (arguments.Count == 0)
                        throw new ECEngineException("emit requires eventName argument", 1, 1, "", "Missing arguments");
                    
                    var emitEventName = arguments[0]?.ToString() ?? "";
                    var emitArgs = arguments.Skip(1).ToList();
                    _process.EmitEvent(emitEventName, emitArgs);
                    return null;

                case "eventNames":
                    return _process.EventNames();

                case "listenerCount":
                    if (arguments.Count == 0)
                        throw new ECEngineException("listenerCount requires eventName argument", 1, 1, "", "Missing arguments");
                    
                    var countEventName = arguments[0]?.ToString() ?? "";
                    return _process.ListenerCount(countEventName);

                default:
                    throw new ECEngineException($"Unknown process method: {_methodName}", 1, 1, "", "Method not found");
            }
        }
    }

    /// <summary>
    /// Global access to process functionality
    /// </summary>
    public static class ProcessGlobals
    {
        private static ProcessObject? _processInstance;
        private static readonly object _lock = new object();

        public static ProcessObject CreateProcessObject(Interpreter interpreter, string[]? commandLineArgs = null)
        {
            if (_processInstance == null)
            {
                lock (_lock)
                {
                    if (_processInstance == null)
                    {
                        _processInstance = new ProcessObject(interpreter, commandLineArgs);
                    }
                }
            }
            return _processInstance;
        }
    }
}
