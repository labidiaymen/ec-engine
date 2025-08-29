using System.Net;
using System.Text;

namespace ECEngine.Runtime;

/// <summary>
/// Simple HTTP server implementation for ECEngine
/// Provides basic web server functionality with minimal footprint
/// </summary>
public class HttpServer
{
    private readonly HttpListener _listener;
    private readonly EventLoop _eventLoop;
    private readonly Interpreter _interpreter;
    private bool _isRunning = false;
    private Function? _requestHandler;

    public HttpServer(EventLoop eventLoop, Interpreter interpreter)
    {
        _listener = new HttpListener();
        _eventLoop = eventLoop;
        _interpreter = interpreter;
    }

    /// <summary>
    /// Start the HTTP server on the specified port
    /// </summary>
    public void Listen(int port, Function? handler = null)
    {
        if (_isRunning)
            throw new ECEngineException("Server is already running", 0, 0, "", "Runtime error");

        _requestHandler = handler;
        _listener.Prefixes.Add($"http://localhost:{port}/");
        
        try
        {
            _listener.Start();
            _isRunning = true;
            
            // Start accepting requests asynchronously
            _eventLoop.NextTick(() => StartAcceptingRequests());
            
            // Keep the event loop alive while server is running
            KeepEventLoopAlive();
            
            Console.WriteLine($"Server listening on http://localhost:{port}/");
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Failed to start server: {ex.Message}", 0, 0, "", "Runtime error");
        }
    }

    /// <summary>
    /// Stop the HTTP server
    /// </summary>
    public void Stop()
    {
        if (!_isRunning) return;

        _listener.Stop();
        _isRunning = false;
        Console.WriteLine("Server stopped");
    }

    private void KeepEventLoopAlive()
    {
        // Schedule a recurring task to keep the event loop running while server is active
        if (_isRunning)
        {
            _eventLoop.SetTimeout(() => KeepEventLoopAlive(), 1000); // Check every second
        }
    }

    private async void StartAcceptingRequests()
    {
        while (_isRunning && _listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                
                // Handle request on next tick to avoid blocking
                _eventLoop.NextTick(() => HandleRequest(context));
            }
            catch (Exception ex)
            {
                if (_isRunning) // Only log if we're still supposed to be running
                {
                    Console.WriteLine($"Error accepting request: {ex.Message}");
                }
                break;
            }
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;

            // Create request object for ECEngine
            var requestObj = new HttpRequestObject
            {
                Method = request.HttpMethod,
                Url = request.Url?.ToString() ?? "",
                Path = request.Url?.AbsolutePath ?? "/",
                Headers = CreateHeadersObject(request.Headers)
            };

            // Create response object for ECEngine
            var responseObj = new HttpResponseObject(response);

            if (_requestHandler != null)
            {
                // Call the user-defined request handler
                try
                {
                    _interpreter.CallUserFunction(_requestHandler, new List<object?> { requestObj, responseObj });
                }
                catch (Exception ex)
                {
                    // If user handler fails, send error response
                    SendErrorResponse(response, 500, $"Handler error: {ex.Message}");
                }
            }
            else
            {
                // Default handler - simple "Hello World"
                SendTextResponse(response, "Hello from ECEngine HTTP Server!");
            }

            // Ensure response is closed
            if (!responseObj.HasEnded)
            {
                responseObj.End();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling request: {ex.Message}");
            try
            {
                SendErrorResponse(context.Response, 500, "Internal Server Error");
            }
            catch
            {
                // Ignore errors when sending error response
            }
        }
    }

    private Dictionary<string, string> CreateHeadersObject(System.Collections.Specialized.NameValueCollection headers)
    {
        var result = new Dictionary<string, string>();
        foreach (string key in headers.AllKeys)
        {
            if (key != null)
            {
                result[key.ToLower()] = headers[key] ?? "";
            }
        }
        return result;
    }

    private void SendTextResponse(HttpListenerResponse response, string text)
    {
        response.ContentType = "text/plain";
        response.StatusCode = 200;
        
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        response.ContentLength64 = buffer.Length;
        
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private void SendErrorResponse(HttpListenerResponse response, int statusCode, string message)
    {
        try
        {
            response.StatusCode = statusCode;
            response.ContentType = "text/plain";
            
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            response.ContentLength64 = buffer.Length;
            
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
        catch
        {
            // Ignore errors when sending error response
        }
    }
}

/// <summary>
/// HTTP request object exposed to ECEngine scripts
/// </summary>
public class HttpRequestObject
{
    public string Method { get; set; } = "";
    public string Url { get; set; } = "";
    public string Path { get; set; } = "";
    public Dictionary<string, string> Headers { get; set; } = new();
}

/// <summary>
/// HTTP response object exposed to ECEngine scripts
/// </summary>
public class HttpResponseObject
{
    private readonly HttpListenerResponse _response;
    private bool _headersSent = false;
    private bool _hasEnded = false;

    public HttpResponseObject(HttpListenerResponse response)
    {
        _response = response;
        StatusCode = 200;
        Headers = new Dictionary<string, string>();
    }

    public int StatusCode { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public bool HasEnded => _hasEnded;

    public void SetHeader(string name, string value)
    {
        if (_headersSent)
            throw new ECEngineException("Cannot set headers after they are sent", 0, 0, "", "Runtime error");
        
        Headers[name.ToLower()] = value;
    }

    public void WriteHead(int statusCode, Dictionary<string, string>? headers = null)
    {
        if (_headersSent)
            throw new ECEngineException("Headers already sent", 0, 0, "", "Runtime error");

        StatusCode = statusCode;
        
        if (headers != null)
        {
            foreach (var header in headers)
            {
                Headers[header.Key.ToLower()] = header.Value;
            }
        }

        SendHeaders();
    }

    public void Write(string data)
    {
        if (_hasEnded)
            throw new ECEngineException("Cannot write after response has ended", 0, 0, "", "Runtime error");

        if (!_headersSent)
        {
            SendHeaders();
        }

        byte[] buffer = Encoding.UTF8.GetBytes(data);
        _response.OutputStream.Write(buffer, 0, buffer.Length);
    }

    public void End(string? data = null)
    {
        if (_hasEnded) return;

        if (data != null)
        {
            Write(data);
        }

        if (!_headersSent)
        {
            SendHeaders();
        }

        _response.OutputStream.Close();
        _hasEnded = true;
    }

    private void SendHeaders()
    {
        if (_headersSent) return;

        _response.StatusCode = StatusCode;

        // Set custom headers
        foreach (var header in Headers)
        {
            try
            {
                if (header.Key.ToLower() == "content-type")
                {
                    _response.ContentType = header.Value;
                }
                else
                {
                    _response.Headers.Add(header.Key, header.Value);
                }
            }
            catch
            {
                // Ignore invalid headers
            }
        }

        _headersSent = true;
    }
}
