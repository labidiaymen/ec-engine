using System.Collections.Concurrent;
using System.Text;

namespace ECEngine.Runtime;

/// <summary>
/// Node.js HTTP module implementation for ECEngine
/// </summary>
public class HttpModule
{
    private readonly EventLoop? _eventLoop;
    private readonly Interpreter? _interpreter;

    public HttpModule(EventLoop? eventLoop = null, Interpreter? interpreter = null)
    {
        _eventLoop = eventLoop;
        _interpreter = interpreter;
    }

    public object? createServer => new CreateServerFunction(_eventLoop, _interpreter);
    public object? request => new RequestFunction(_eventLoop, _interpreter);
    public object? get => new GetFunction(_eventLoop, _interpreter);
    
    // HTTP Status codes
    public object? STATUS_CODES => new Dictionary<string, object?>
    {
        { "100", "Continue" },
        { "101", "Switching Protocols" },
        { "200", "OK" },
        { "201", "Created" },
        { "202", "Accepted" },
        { "204", "No Content" },
        { "301", "Moved Permanently" },
        { "302", "Found" },
        { "304", "Not Modified" },
        { "400", "Bad Request" },
        { "401", "Unauthorized" },
        { "403", "Forbidden" },
        { "404", "Not Found" },
        { "405", "Method Not Allowed" },
        { "500", "Internal Server Error" },
        { "502", "Bad Gateway" },
        { "503", "Service Unavailable" }
    };

    // HTTP Methods
    public object? METHODS => new string[] 
    { 
        "GET", "HEAD", "POST", "PUT", "DELETE", "CONNECT", "OPTIONS", "TRACE", "PATCH" 
    };
}

/// <summary>
/// Node.js http.createServer() function
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
        Function? requestListener = null;
        
        // http.createServer([options], [requestListener])
        if (arguments.Count > 0 && arguments[0] is Function handler)
        {
            requestListener = handler;
        }

        // Return a proper HTTP server object
        return new NodeHttpServer(_eventLoop, _interpreter, requestListener);
    }
}

/// <summary>
/// Node.js http.request() function
/// </summary>
public class RequestFunction
{
    private readonly EventLoop? _eventLoop;
    private readonly Interpreter? _interpreter;

    public RequestFunction(EventLoop? eventLoop, Interpreter? interpreter)
    {
        _eventLoop = eventLoop;
        _interpreter = interpreter;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            throw new ECEngineException("http.request requires at least one argument", 0, 0, "", "Runtime error");

        string url = "";
        Dictionary<string, object?>? options = null;
        Function? callback = null;

        // Parse arguments: http.request(url[, options][, callback]) or http.request(options[, callback])
        if (arguments[0] is string urlString)
        {
            url = urlString;
            if (arguments.Count > 1 && arguments[1] is Dictionary<string, object?> opts)
            {
                options = opts;
            }
            if (arguments.Count > 2 && arguments[2] is Function callbackFunc)
            {
                callback = callbackFunc;
            }
            else if (arguments.Count > 1 && arguments[1] is Function callbackFunc2)
            {
                callback = callbackFunc2;
            }
        }
        else if (arguments[0] is Dictionary<string, object?> opts)
        {
            options = opts;
            if (options.ContainsKey("hostname") && options.ContainsKey("port"))
            {
                var hostname = options["hostname"]?.ToString() ?? "localhost";
                var port = options["port"]?.ToString() ?? "80";
                var path = options.ContainsKey("path") ? options["path"]?.ToString() : "/";
                var protocol = options.ContainsKey("protocol") ? options["protocol"]?.ToString() : "http:";
                url = $"{protocol}//{hostname}:{port}{path}";
            }
            if (arguments.Count > 1 && arguments[1] is Function callbackFunc)
            {
                callback = callbackFunc;
            }
        }

        return new ClientRequest(url, options, callback, _eventLoop, _interpreter);
    }
}

/// <summary>
/// Node.js http.get() function (convenience method for GET requests)
/// </summary>
public class GetFunction
{
    private readonly EventLoop? _eventLoop;
    private readonly Interpreter? _interpreter;

    public GetFunction(EventLoop? eventLoop, Interpreter? interpreter)
    {
        _eventLoop = eventLoop;
        _interpreter = interpreter;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            throw new ECEngineException("http.get requires at least one argument", 0, 0, "", "Runtime error");

        // http.get is just http.request with method set to GET
        var requestFunc = new RequestFunction(_eventLoop, _interpreter);
        var req = requestFunc.Call(arguments) as ClientRequest;
        
        // Automatically end the request for GET
        req?.InternalEnd();
        
        return req;
    }
}

/// <summary>
/// HTTP Server implementation following Node.js API
/// </summary>
public class NodeHttpServer
{
    private readonly EventLoop? _eventLoop;
    private readonly Interpreter? _interpreter;
    private readonly Function? _requestListener;
    private readonly Dictionary<string, List<Function>> _eventHandlers = new();
    private HttpServer? _realServer;
    private bool _listening = false;

    public NodeHttpServer(EventLoop? eventLoop, Interpreter? interpreter, Function? requestListener)
    {
        _eventLoop = eventLoop;
        _interpreter = interpreter;
        _requestListener = requestListener;
        
        if (_requestListener != null)
        {
            AddEventHandler("request", _requestListener);
        }
    }

    public object? listen => new ServerListenFunction(this);
    public object? close => new ServerCloseFunction(this);
    public object? on => new ServerOnFunction(this);
    public object? emit => new ServerEmitFunction(this);
    
    public void Listen(int port, string? hostname = null, Function? callback = null)
    {
        if (_listening)
            throw new ECEngineException("Server is already listening", 0, 0, "", "Runtime error");

        if (_eventLoop == null || _interpreter == null)
            throw new ECEngineException("Event loop and interpreter are required for HTTP server", 0, 0, "", "Runtime error");

        // Create a real HTTP server that will keep the process alive
        _realServer = new HttpServer(_eventLoop, _interpreter);
        
        // Get the request handler if available
        Function? requestHandler = null;
        if (HasListener("request"))
        {
            requestHandler = GetRequestHandler();
        }

        _listening = true;
        
        // Start the real HTTP server
        _realServer.Listen(port, requestHandler);
        
        // Emit 'listening' event
        EmitEvent("listening", new List<object?>());
        
        if (callback != null)
        {
            _eventLoop.NextTick(() => {
                try
                {
                    _interpreter.CallUserFunctionPublic(callback, new List<object?>());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in server listen callback: {ex.Message}");
                }
            });
        }
    }

    public void Close(Function? callback = null)
    {
        if (!_listening)
            return;

        _realServer?.Stop();
        _listening = false;
        
        // Emit 'close' event
        EmitEvent("close", new List<object?>());
        
        if (callback != null && _eventLoop != null && _interpreter != null)
        {
            _eventLoop.NextTick(() => {
                try
                {
                    _interpreter.CallUserFunctionPublic(callback, new List<object?>());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in server close callback: {ex.Message}");
                }
            });
        }
    }

    internal void AddEventHandler(string eventName, Function handler)
    {
        if (!_eventHandlers.ContainsKey(eventName))
        {
            _eventHandlers[eventName] = new List<Function>();
        }
        _eventHandlers[eventName].Add(handler);
    }

    internal void EmitEvent(string eventName, List<object?> args)
    {
        if (_eventHandlers.ContainsKey(eventName))
        {
            foreach (var handler in _eventHandlers[eventName])
            {
                if (_eventLoop != null && _interpreter != null)
                {
                    _eventLoop.NextTick(() => {
                        try
                        {
                            _interpreter.CallUserFunctionPublic(handler, args);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error in event handler for '{eventName}': {ex.Message}");
                        }
                    });
                }
            }
        }
    }

    private bool HasListener(string eventName)
    {
        return _eventHandlers.ContainsKey(eventName) && _eventHandlers[eventName].Count > 0;
    }

    private Function? GetRequestHandler()
    {
        if (HasListener("request"))
        {
            return _eventHandlers["request"][0]; // Use first registered handler
        }
        return null;
    }
}

/// <summary>
/// HTTP Client Request implementation following Node.js API
/// </summary>
public class ClientRequest
{
    private readonly string _url;
    private readonly Dictionary<string, object?>? _options;
    private readonly Function? _callback;
    private readonly EventLoop? _eventLoop;
    private readonly Interpreter? _interpreter;
    private readonly Dictionary<string, List<Function>> _eventHandlers = new();
    private bool _ended = false;

    public ClientRequest(string url, Dictionary<string, object?>? options, Function? callback, EventLoop? eventLoop, Interpreter? interpreter)
    {
        _url = url;
        _options = options;
        _callback = callback;
        _eventLoop = eventLoop;
        _interpreter = interpreter;

        if (_callback != null)
        {
            ClientRequestOn("response", _callback);
        }
    }

    public object? write => new ClientRequestWriteFunction(this);
    public object? end => new ClientRequestEndFunction(this);
    public object? on => new ClientRequestOnFunction(this);

    public void InternalWrite(object? chunk, string? encoding = null)
    {
        if (_ended)
            throw new ECEngineException("Cannot write after end", 0, 0, "", "Runtime error");
        
        // In a real implementation, this would buffer the data
        // For now, we'll just store it for when end() is called
    }

    public void InternalEnd(object? chunk = null, string? encoding = null)
    {
        if (_ended)
            return;

        _ended = true;

        if (chunk != null)
        {
            InternalWrite(chunk, encoding);
        }

        // Simulate HTTP request execution
        if (_eventLoop != null && _interpreter != null)
        {
            _eventLoop.NextTick(() => {
                try
                {
                    // Create a mock response for now
                    var response = new IncomingMessage();
                    EmitEvent("response", new List<object?> { response });
                }
                catch (Exception ex)
                {
                    EmitEvent("error", new List<object?> { ex.Message });
                }
            });
        }
    }

    internal void ClientRequestOn(string eventName, Function handler)
    {
        if (!_eventHandlers.ContainsKey(eventName))
        {
            _eventHandlers[eventName] = new List<Function>();
        }
        _eventHandlers[eventName].Add(handler);
    }

    private void EmitEvent(string eventName, List<object?> args)
    {
        if (_eventHandlers.ContainsKey(eventName))
        {
            foreach (var handler in _eventHandlers[eventName])
            {
                if (_eventLoop != null && _interpreter != null)
                {
                    _eventLoop.NextTick(() => {
                        try
                        {
                            _interpreter.CallUserFunctionPublic(handler, args);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error in client request event handler for '{eventName}': {ex.Message}");
                        }
                    });
                }
            }
        }
    }
}

/// <summary>
/// HTTP IncomingMessage (response) implementation
/// </summary>
public class IncomingMessage
{
    public int statusCode { get; set; } = 200;
    public string statusMessage { get; set; } = "OK";
    public Dictionary<string, object?> headers { get; set; } = new();
    public string httpVersion { get; set; } = "1.1";
    public string method { get; set; } = "GET";
    public string url { get; set; } = "/";
    
    private readonly List<object?> _data = new();
    private readonly Dictionary<string, List<Function>> _eventHandlers = new();

    public object? on => new IncomingMessageOnFunction(this);
    public object? setEncoding => new IncomingMessageSetEncodingFunction(this);

    internal void IncomingMessageOn(string eventName, Function handler)
    {
        if (!_eventHandlers.ContainsKey(eventName))
        {
            _eventHandlers[eventName] = new List<Function>();
        }
        _eventHandlers[eventName].Add(handler);
    }

    internal void EmitData(object? chunk)
    {
        _data.Add(chunk);
        EmitEvent("data", new List<object?> { chunk });
    }

    internal void EmitEnd()
    {
        EmitEvent("end", new List<object?>());
    }

    private void EmitEvent(string eventName, List<object?> args)
    {
        if (_eventHandlers.ContainsKey(eventName))
        {
            foreach (var handler in _eventHandlers[eventName])
            {
                // Note: In a real implementation, we'd use the event loop here
                // For now, we'll call synchronously
                try
                {
                    // This would need the interpreter context to call properly
                    // handler.Call(args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in incoming message event handler for '{eventName}': {ex.Message}");
                }
            }
        }
    }
}

// Helper functions for the HTTP Server
public class ServerListenFunction
{
    private readonly NodeHttpServer _server;

    public ServerListenFunction(NodeHttpServer server)
    {
        _server = server;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            throw new ECEngineException("listen requires at least one argument", 0, 0, "", "Runtime error");

        var port = Convert.ToInt32(arguments[0]);
        string? hostname = null;
        Function? callback = null;

        // Parse arguments: listen(port[, hostname][, callback])
        if (arguments.Count > 1)
        {
            if (arguments[1] is string host)
            {
                hostname = host;
                if (arguments.Count > 2 && arguments[2] is Function cb)
                {
                    callback = cb;
                }
            }
            else if (arguments[1] is Function cb)
            {
                callback = cb;
            }
        }

        _server.Listen(port, hostname, callback);
        return _server;
    }
}

public class ServerCloseFunction
{
    private readonly NodeHttpServer _server;

    public ServerCloseFunction(NodeHttpServer server)
    {
        _server = server;
    }

    public object? Call(List<object?> arguments)
    {
        Function? callback = null;
        if (arguments.Count > 0 && arguments[0] is Function cb)
        {
            callback = cb;
        }

        _server.Close(callback);
        return _server;
    }
}

public class ServerOnFunction
{
    private readonly NodeHttpServer _server;

    public ServerOnFunction(NodeHttpServer server)
    {
        _server = server;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
            throw new ECEngineException("on requires eventName and handler", 0, 0, "", "Runtime error");

        var eventName = arguments[0]?.ToString() ?? "";
        if (arguments[1] is Function handler)
        {
            _server.AddEventHandler(eventName, handler);
        }

        return _server;
    }
}

public class ServerEmitFunction
{
    private readonly NodeHttpServer _server;

    public ServerEmitFunction(NodeHttpServer server)
    {
        _server = server;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            return false;

        var eventName = arguments[0]?.ToString() ?? "";
        var args = arguments.Skip(1).ToList();
        
        _server.EmitEvent(eventName, args);
        return true;
    }
}

// Helper functions for Client Request
public class ClientRequestWriteFunction
{
    private readonly ClientRequest _request;

    public ClientRequestWriteFunction(ClientRequest request)
    {
        _request = request;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            return _request;

        var chunk = arguments[0];
        string? encoding = arguments.Count > 1 ? arguments[1]?.ToString() : null;
        
        _request.InternalWrite(chunk, encoding);
        return _request;
    }
}

public class ClientRequestEndFunction
{
    private readonly ClientRequest _request;

    public ClientRequestEndFunction(ClientRequest request)
    {
        _request = request;
    }

    public object? Call(List<object?> arguments)
    {
        object? chunk = arguments.Count > 0 ? arguments[0] : null;
        string? encoding = arguments.Count > 1 ? arguments[1]?.ToString() : null;
        
        _request.InternalEnd(chunk, encoding);
        return _request;
    }
}

public class ClientRequestOnFunction
{
    private readonly ClientRequest _request;

    public ClientRequestOnFunction(ClientRequest request)
    {
        _request = request;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
            throw new ECEngineException("on requires eventName and handler", 0, 0, "", "Runtime error");

        var eventName = arguments[0]?.ToString() ?? "";
        if (arguments[1] is Function handler)
        {
            _request.ClientRequestOn(eventName, handler);
        }

        return _request;
    }
}

// Helper functions for IncomingMessage
public class IncomingMessageOnFunction
{
    private readonly IncomingMessage _message;

    public IncomingMessageOnFunction(IncomingMessage message)
    {
        _message = message;
    }

    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
            throw new ECEngineException("on requires eventName and handler", 0, 0, "", "Runtime error");

        var eventName = arguments[0]?.ToString() ?? "";
        if (arguments[1] is Function handler)
        {
            _message.IncomingMessageOn(eventName, handler);
        }

        return _message;
    }
}

public class IncomingMessageSetEncodingFunction
{
    private readonly IncomingMessage _message;

    public IncomingMessageSetEncodingFunction(IncomingMessage message)
    {
        _message = message;
    }

    public object? Call(List<object?> arguments)
    {
        // In a real implementation, this would set the encoding for data events
        return _message;
    }
}

/// <summary>
/// Static helper to get HTTP module for the module system
/// </summary>
public static class HttpGlobals
{
    public static Dictionary<string, object?> GetHttpModule(EventLoop? eventLoop, Interpreter? interpreter)
    {
        var httpModule = new HttpModule(eventLoop, interpreter);
        return new Dictionary<string, object?>
        {
            { "createServer", httpModule.createServer },
            { "request", httpModule.request },
            { "get", httpModule.get },
            { "STATUS_CODES", httpModule.STATUS_CODES },
            { "METHODS", httpModule.METHODS }
        };
    }
}


