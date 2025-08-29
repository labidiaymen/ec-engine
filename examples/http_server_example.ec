// Simple HTTP Server Example for ECEngine
console.log("Starting ECEngine HTTP Server...");

// Create a simple HTTP server
var server = createServer(function(req, res) {
    console.log("Request received:", req.method, req.path);
    
    // Set response headers
    res.setHeader("Content-Type", "text/html");
    
    // Write response
    res.writeHead(200);
    res.write("<h1>Hello from ECEngine!</h1>");
    res.write("<p>Method: " + req.method + "</p>");
    res.write("<p>Path: " + req.path + "</p>");
    res.write("<p>URL: " + req.url + "</p>");
    res.end();
});

// Start the server on port 3000
server.listen(3000, function() {
    console.log("Server is running on http://localhost:3000");
    console.log("Try visiting the URL in your browser!");
});

console.log("Server starting...");
