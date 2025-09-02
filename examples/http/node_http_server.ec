// Node.js-style HTTP server example with node: prefix
const http = require('node:http');

const server = http.createServer((req, res) => {
    // Simple routing
    if (req.url === '/') {
        res.writeHead(200, { 'Content-Type': 'text/html' });
        res.end('<h1>Welcome to ECEngine HTTP Server</h1>');
    } else if (req.url === '/api') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({ message: 'Hello from API', method: req.method }));
    } else {
        res.writeHead(404, { 'Content-Type': 'text/plain' });
        res.end('Not Found');
    }
});

const port = 8080;
server.listen(port, () => {
    console.log(`Server running at http://localhost:${port}/`);
    console.log('Try these endpoints:');
    console.log('  http://localhost:8080/     - HTML response');
    console.log('  http://localhost:8080/api  - JSON response');
});
