// Basic For...in and For...of Examples
// Simple examples showing the fundamental usage patterns

console.log("=== Basic For...in and For...of Examples ===");

// Basic For...in with object
console.log("\n1. Basic For...in with Object:");
var obj = {a: 1, b: 2, c: 3};
for (key in obj) {
    console.log(key + " = " + obj[key]);
}

// Basic For...of with array
console.log("\n2. Basic For...of with Array:");
var arr = [10, 20, 30];
for (item of arr) {
    console.log("Item: " + item);
}

// For...of with string
console.log("\n3. For...of with String:");
var str = "ABC";
for (char of str) {
    console.log("Character: " + char);
}

// For...in with array (shows indices)
console.log("\n4. For...in with Array (shows indices):");
var colors = ["red", "green", "blue"];
for (index in colors) {
    console.log("Index " + index + ": " + colors[index]);
}

console.log("\n=== Basic Examples Complete ===");
