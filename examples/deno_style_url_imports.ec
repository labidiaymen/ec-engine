// Deno-style URL Imports Example
// ECEngine supports importing modules directly from URLs like Deno.js

console.log("=== Deno-style URL Imports in ECEngine ===");

// Example 1: Import from unpkg.com (CommonJS module)
console.log("1. Importing left-pad from unpkg.com...");
import leftPad from "https://unpkg.com/left-pad@1.3.0/index.js";

console.log("✅ Successfully imported left-pad!");
console.log("leftPad('Hello', 10, '-') =", leftPad('Hello', 10, '-'));
console.log("leftPad('World', 8, '0') =", leftPad('World', 8, '0'));

// Example 2: Demonstrate caching (second import is instant)
console.log("2. Second import (cached) - instant loading:");
import leftPad2 from "https://unpkg.com/left-pad@1.3.0/index.js";
console.log("leftPad2('Fast', 12, '.') =", leftPad2('Fast', 12, '.'));
console.log();



console.log("🚀 ECEngine now supports Deno-style URL imports!");

