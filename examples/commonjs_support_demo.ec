// CommonJS Module Support Example
// ECEngine supports CommonJS modules with module.exports syntax

console.log("=== CommonJS Module Support in ECEngine ===");
console.log();

// Example 1: URL import of CommonJS module
console.log("1. Importing CommonJS module from URL...");
import leftPad from "https://unpkg.com/left-pad@1.3.0/index.js";

console.log("Module type: CommonJS (uses module.exports = function)");
console.log("Import syntax: import defaultName from 'url'");
console.log();

// Example 2: Test the imported CommonJS function
console.log("2. Testing CommonJS function:");
console.log("leftPad('ECEngine', 15, '*') =", leftPad('ECEngine', 15, '*'));
console.log("leftPad('CommonJS', 12, ' ') =", leftPad('CommonJS', 12, ' '));
console.log("leftPad('Works!', 10, '=') =", leftPad('Works!', 10, '='));
console.log();

console.log("=== CommonJS Features Supported ===");
console.log("✅ module.exports = function");
console.log("✅ module.exports = value");
console.log("✅ exports.name = value");
console.log("✅ Function execution with parameters");
console.log("✅ Mixed with ECEngine module system");
console.log();

console.log("📦 ECEngine bridges CommonJS and ES modules seamlessly!");
