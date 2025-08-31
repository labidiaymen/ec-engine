// Simplified Node.js-style Module Resolution Test
console.log("=== ECEngine Node.js Module Resolution Test ===");
console.log("");

// Test 1: Module with package.json main field
console.log("1. Testing module with package.json main field...");
import { greet, version } from "simple-utils";
console.log("   Imported functions from simple-utils:");
var result1 = greet("World");
console.log("   - greet('World'):", result1);
console.log("   - version:", version);
console.log("");

// Test 2: Module with package.json pointing to .ec file
console.log("2. Testing module with package.json pointing to .ec file...");
import { add, multiply, PI } from "math-helpers";
console.log("   Imported functions from math-helpers:");
var result2 = add(5, 3);
var result3 = multiply(4, 7);
console.log("   - add(5, 3):", result2);
console.log("   - multiply(4, 7):", result3);
console.log("   - PI:", PI);
console.log("");

console.log("=== Node.js Module Resolution Working! ===");
