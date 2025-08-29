// Observable HTTP Server Example using ECEngine's observe pattern
console.log("Starting Observable HTTP Server...");

// Create an observable server
let server = createServer(8080);

// Use observe pattern to reactively handle requests
observe server.requests function(ev) {
    console.log("Received " + ev.method + " request to " + ev.url);
    
    when (ev.method == "GET") {
        when (ev.url == "/") {
            ev.response.send("Hello from ECEngine Observable Server!");
        }
        when (ev.url == "/api/status") {
            ev.response.json({ status: "running", timestamp: 12345 });
        }
        
        when (ev.url == "/api/echo") {
            ev.response.send("Echo: " + ev.url);
        }
        
        otherwise {
            ev.response.status(404);
            ev.response.send("Not Found");
        }
    }
    
    when (ev.method == "POST") {
        when (ev.url == "/api/data") {
            ev.response.json({ message: "Data received", method: ev.method });
        }
        
        otherwise {
            ev.response.status(405);
            ev.response.send("Method Not Allowed");
        }
    }
    
    otherwise {
        ev.response.status(405);
        ev.response.send("Method Not Allowed");
    }
}

console.log("Server is now observing requests on port 8080");
console.log("Try visiting:");
console.log("  http://localhost:8080/");
console.log("  http://localhost:8080/api/status");
console.log("  http://localhost:8080/api/echo");

// Keep the server running
server.listen();
