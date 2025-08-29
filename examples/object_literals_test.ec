// Test object literals in ECEngine
console.log("Testing object literals...");

// Simple object
let person = {
    name: "John",
    age: 30,
    isActive: true
};

console.log("Person object created:");
console.log(person);

// Nested object
let config = {
    server: {
        port: 8080,
        host: "localhost"
    },
    database: {
        name: "myapp",
        timeout: 5000
    }
};

console.log("Config object created:");
console.log(config);

// Object with function call result
let status = {
    timestamp: Date.now(),
    message: "System is running"
};

console.log("Status object:");
console.log(status);
