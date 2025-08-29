using ECEngine.AST;
using System.IO;

namespace ECEngine.Runtime;

/// <summary>
/// Observable server object that emits events for HTTP requests
/// Integrates with ECEngine's reactive observe pattern
/// </summary>
public class ObservableServerObject
{
    private readonly HttpServer _httpServer;
    private readonly EventLoop _eventLoop;
    private readonly Interpreter _interpreter;
    private readonly List<Function> _requestObservers = new();
    private readonly List<Function> _errorObservers = new();
    private int _port = 8080;

    public ObservableServerObject(EventLoop eventLoop, Interpreter interpreter)
    {
        _eventLoop = eventLoop;
        _interpreter = interpreter;
        _httpServer = new HttpServer(eventLoop, interpreter);
    }

    /// <summary>
    /// Set the port for the server
    /// </summary>
    public void SetPort(int port)
    {
        _port = port;
    }

    /// <summary>
    /// Add an observer for HTTP requests
    /// </summary>
    public void AddRequestObserver(Function observer)
    {
        Console.WriteLine($"[DEBUG] Adding request observer: {observer.Name}");
        _requestObservers.Add(observer);
        Console.WriteLine($"[DEBUG] Total observers: {_requestObservers.Count}");
    }

    /// <summary>
    /// Add an observer for server errors
    /// </summary>
    public void AddErrorObserver(Function observer)
    {
        _errorObservers.Add(observer);
    }

    /// <summary>
    /// Start listening for HTTP requests
    /// </summary>
    public object? Listen(List<object?> arguments = null!)
    {
        var port = arguments?.Count > 0 ? Convert.ToInt32(arguments[0]) : _port;
        
        // Create a request handler that will emit events to observers
        var handler = CreateRequestHandler();
        
        // Start the underlying HTTP server
        _httpServer.Listen(port, handler);
        
        return null;
    }

    /// <summary>
    /// Stop the server
    /// </summary>
    public object? Close(List<object?> arguments = null!)
    {
        _httpServer.Stop();
        return null;
    }

    /// <summary>
    /// Create a request handler that emits events to observers
    /// </summary>
    private Function CreateRequestHandler()
    {
        // Create a function that when called by HttpServer, will emit to our observers
        // This function will be called with (request, response) arguments
        var proxyHandler = new Function(
            name: "__observable_request_handler",
            parameters: new List<string> { "request", "response" },
            body: new List<ECEngine.AST.Statement>(), // Empty body - we handle this in C#
            closure: new Dictionary<string, VariableInfo>()
        );
        
        // Store reference to this ObservableServerObject so we can call HandleRequest
        // We'll use a special property to identify this as our proxy handler
        proxyHandler.IsObservableProxy = true;
        proxyHandler.ObservableServer = this;
        
        return proxyHandler;
    }

    /// <summary>
    /// Handle an incoming HTTP request by emitting it to observers
    /// </summary>
    public void HandleRequest(HttpRequestObject request, HttpResponseObject response)
    {
        Console.WriteLine($"[DEBUG] HandleRequest called - URL: {request.Url}, Method: {request.Method}");
        Console.WriteLine($"[DEBUG] Number of observers: {_requestObservers.Count}");
        
        try
        {
            // Create an HTTP request event
            var requestEvent = new HttpRequestEvent(request, response);
            
            // Emit to all request observers
            foreach (var observer in _requestObservers)
            {
                Console.WriteLine($"[DEBUG] Calling observer: {observer.Name}");
                try
                {
                    _interpreter.CallUserFunction(observer, new List<object?> { requestEvent });
                    Console.WriteLine($"[DEBUG] Observer {observer.Name} completed successfully");
                }
                catch (Exception ex)
                {
                    // Log observer error but don't stop processing
                    Console.WriteLine($"Warning: Request observer threw an error: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            // Emit error to error observers
            EmitError(ex);
        }
    }

    private void EmitError(Exception error)
    {
        foreach (var observer in _errorObservers)
        {
            try
            {
                _interpreter.CallUserFunction(observer, new List<object?> { error.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error observer threw an error: {ex.Message}");
            }
        }
    }
}

/// <summary>
/// HTTP request event object passed to observers
/// </summary>
public class HttpRequestEvent
{
    private readonly HttpRequestObject _request;
    private readonly HttpResponseObject _response;

    public HttpRequestEvent(HttpRequestObject request, HttpResponseObject response)
    {
        _request = request;
        _response = response;
    }

    // Request properties
    public string Method => _request.Method;
    public string Url => _request.Url;
    public string Path => _request.Path;
    public Dictionary<string, string> Headers => _request.Headers;
    public string Body => ""; // TODO: Add body support to HttpRequestObject

    // Lowercase aliases for ECEngine compatibility
    public string method => _request.Method;
    public string url => _request.Path;  // Use Path instead of Url for routing
    public string path => _request.Path;
    public Dictionary<string, string> headers => _request.Headers;
    public string body => ""; // TODO: Add body support to HttpRequestObject

    // Response object for sending responses
    public ResponseObject Response => new ResponseObject(_response);
    public ResponseObject response => new ResponseObject(_response);
}

/// <summary>
/// Response object with methods for sending HTTP responses
/// </summary>
public class ResponseObject
{
    private readonly HttpResponseObject _response;

    public ResponseObject(HttpResponseObject response)
    {
        _response = response;
    }

    public object Send(string content)
    {
        _response.SetHeader("Content-Type", "text/plain; charset=utf-8");
        _response.Write(content);
        _response.End();
        return null!;
    }

    public object Json(object data)
    {
        var json = SimpleJsonSerializer.Serialize(data);
        _response.SetHeader("Content-Type", "application/json; charset=utf-8");
        _response.Write(json);
        _response.End();
        return null!;
    }

    public object Html(string html)
    {
        _response.SetHeader("Content-Type", "text/html; charset=utf-8");
        _response.Write(html);
        _response.End();
        return null!;
    }

    public ResponseObject Status(int statusCode)
    {
        _response.StatusCode = statusCode;
        return this;
    }

    public object End()
    {
        _response.End();
        return null!;
    }

    public void SetHeader(string name, string value)
    {
        _response.SetHeader(name, value);
    }

    public void Redirect(string url, int statusCode = 302)
    {
        _response.SetHeader("Location", url);
        _response.StatusCode = statusCode;
        _response.End();
    }

    public bool Responded => _response.HasEnded;
}

/// <summary>
/// Simple JSON serializer for basic objects
/// </summary>
public static class SimpleJsonSerializer
{
    public static string Serialize(object? obj)
    {
        if (obj == null) return "null";
        
        if (obj is string str)
            return $"\"{EscapeString(str)}\"";
            
        if (obj is bool b)
            return b ? "true" : "false";
            
        if (obj is double d)
            return d.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
            
        if (obj is Dictionary<string, object?> dict)
        {
            var pairs = dict.Select(kvp => $"\"{EscapeString(kvp.Key)}\":{Serialize(kvp.Value)}");
            return "{" + string.Join(",", pairs) + "}";
        }
        
        return $"\"{obj}\"";
    }

    private static string EscapeString(string str)
    {
        return str.Replace("\\", "\\\\")
                  .Replace("\"", "\\\"")
                  .Replace("\n", "\\n")
                  .Replace("\r", "\\r")
                  .Replace("\t", "\\t");
    }
}
