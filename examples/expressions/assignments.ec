// Variable Assignment Expressions
// Different ways to assign values to variables

var x = 5;
console.log("Initial x:", x);

// Simple assignment
x = 10;
console.log("After assignment x:", x);

// Assignment with expression
x = x + 5;
console.log("After x = x + 5:", x);

// Multiple variables
var a = 1;
var b = 2;
var c = 3;

console.log("a:", a, "b:", b, "c:", c);

// Chain assignments through expressions
a = b + c;
b = a * 2;
c = b - a;

console.log("After calculations - a:", a, "b:", b, "c:", c);
