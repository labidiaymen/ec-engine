namespace ECEngine.Runtime;

/// <summary>
/// Global HTTP server functions for ECEngine scripts
/// </summary>

public class CreateServerFunction
{
    private readonly EventLoop _eventLoop;
    private readonly Interpreter _interpreter;

    public CreateServerFunction(EventLoop eventLoop, Interpreter interpreter)
    {
        _eventLoop = eventLoop;
        _interpreter = interpreter;
    }

    public object? Call(List<object?> arguments)
    {
        // For observable servers, we accept a port number as the first argument
        int port = 3000; // default port
        
        if (arguments.Count > 0)
        {
            // Check if first argument is a port number
            if (arguments[0] is double portDouble)
            {
                port = (int)portDouble;
            }
            else if (arguments[0] is int portInt)
            {
                port = portInt;
            }
            else if (arguments[0] is Function handler)
            {
                // Legacy mode: createServer(handler) - use default port
                port = 3000;
            }
            else if (arguments[0] != null)
            {
                throw new ECEngineException("createServer: argument must be a port number or function", 0, 0, "", "Runtime error");
            }
        }

        // Return observable server object 
        var observableServer = new ObservableServerObject(_eventLoop, _interpreter);
        observableServer.SetPort(port);
        return observableServer;
    }
}

/// <summary>
/// Server object exposed to ECEngine scripts
/// </summary>
public class ServerObject
{
    private readonly HttpServer _server;
    private readonly Function? _handler;

    public ServerObject(HttpServer server, Function? handler)
    {
        _server = server;
        _handler = handler;
    }

    public object? Listen(List<object?> arguments)
    {
        if (arguments.Count < 1)
            throw new ECEngineException("listen requires at least 1 argument: port", 0, 0, "", "Runtime error");

        var port = Convert.ToInt32(arguments[0]);
        
        Function? callback = null;
        if (arguments.Count > 1)
        {
            callback = arguments[1] as Function;
        }

        try
        {
            _server.Listen(port, _handler);
            
            // Call callback if provided
            if (callback != null)
            {
                // Use NextTick to call callback asynchronously
                var eventLoop = _server.GetType().GetField("_eventLoop", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_server) as EventLoop;
                
                eventLoop?.NextTick(() => {
                    try
                    {
                        var interpreter = _server.GetType().GetField("_interpreter", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_server) as Interpreter;
                        interpreter?.CallUserFunctionPublic(callback, new List<object?>());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in server listen callback: {ex.Message}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Failed to start server: {ex.Message}", 0, 0, "", "Runtime error");
        }

        return this;
    }

    public object? Close(List<object?> arguments)
    {
        _server.Stop();
        
        Function? callback = null;
        if (arguments.Count > 0)
        {
            callback = arguments[0] as Function;
            if (callback != null)
            {
                // Call callback asynchronously
                var eventLoop = _server.GetType().GetField("_eventLoop", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_server) as EventLoop;
                
                eventLoop?.NextTick(() => {
                    try
                    {
                        var interpreter = _server.GetType().GetField("_interpreter", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_server) as Interpreter;
                        interpreter?.CallUserFunctionPublic(callback, new List<object?>());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in server close callback: {ex.Message}");
                    }
                });
            }
        }

        return null;
    }
}

/// <summary>
/// HTTP module object - provides createServer function
/// </summary>
public class HttpModule
{
    private readonly CreateServerFunction _createServerFunction;

    public HttpModule(EventLoop eventLoop, Interpreter interpreter)
    {
        _createServerFunction = new CreateServerFunction(eventLoop, interpreter);
    }

    public CreateServerFunction CreateServer => _createServerFunction;
}
