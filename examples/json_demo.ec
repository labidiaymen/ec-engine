// ECEngine JSON Demo
// Demonstrates JSON.parse() and JSON.stringify() functionality

console.log("=== ECEngine JSON Demo ===");
console.log("");

// 1. Basic JSON stringification
console.log("1. Basic JSON.stringify():");
var simpleObject = {
    "name": "Alice",
    "age": 30,
    "active": true
};
var jsonString = JSON.stringify(simpleObject);
console.log("Object:", simpleObject);
console.log("JSON string:", jsonString);
console.log("");

// 2. JSON parsing
console.log("2. JSON.parse():");
var jsonData = '{"name": "Bob", "score": 95.5, "passed": true}';
var parsedObject = JSON.parse(jsonData);
console.log("JSON string:", jsonData);
console.log("Parsed object:", parsedObject);
console.log("");

// 3. Complex nested objects
console.log("3. Complex nested objects:");
var complexData = {
    "user": {
        "id": 123,
        "profile": {
            "firstName": "John",
            "lastName": "Doe",
            "email": "john@example.com"
        }
    },
    "preferences": {
        "theme": "dark",
        "notifications": true,
        "language": "en"
    },
    "metadata": {
        "created": Date.now(),
        "version": "1.0.0"
    }
};

var complexJson = JSON.stringify(complexData);
console.log("Complex object:", complexData);
console.log("Complex JSON:", complexJson);

var parsedComplex = JSON.parse(complexJson);
console.log("Parsed back:", parsedComplex);
console.log("");

// 4. Null values
console.log("4. Null values in JSON:");
var dataWithNulls = {
    "name": "Test",
    "value": null,
    "optional": null,
    "count": 0
};

var nullJson = JSON.stringify(dataWithNulls);
console.log("Object with nulls:", dataWithNulls);
console.log("JSON with nulls:", nullJson);

var parsedNulls = JSON.parse(nullJson);
console.log("Parsed nulls:", parsedNulls);
console.log("");

// 5. Configuration example (practical use case)
console.log("5. Practical example - Configuration:");
var config = {
    "database": {
        "host": "localhost",
        "port": 5432,
        "name": "myapp",
        "ssl": true
    },
    "cache": {
        "enabled": true,
        "ttl": 3600,
        "provider": "redis"
    },
    "features": "auth,analytics,notifications",
    "version": "2.1.0"
};

var configJson = JSON.stringify(config);
console.log("Configuration:", config);
console.log("Config as JSON:", configJson);

// Simulate loading config from JSON
var loadedConfig = JSON.parse(configJson);
console.log("Loaded config:", loadedConfig);

console.log("");
console.log("=== JSON Demo Complete ===");
