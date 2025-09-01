// Test the typeof operator
console.log("Testing typeof operator...");

console.log("typeof 42:", typeof 42);
console.log("typeof 'hello':", typeof "hello");
console.log("typeof true:", typeof true);
console.log("typeof null:", typeof null);

// Test undefined (undefined variable)
var x;
console.log("typeof undefined var:", typeof x);

// Test function
function testFunc() { return 42; }
console.log("typeof function:", typeof testFunc);

// Test object
var obj = {};
console.log("typeof object:", typeof obj);

// Test array (should be object in JavaScript)
var arr = [];
console.log("typeof array:", typeof arr);

console.log("Typeof operator tests complete!");
