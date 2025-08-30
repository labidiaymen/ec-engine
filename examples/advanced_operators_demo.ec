// ECEngine Advanced Operators Demo
// Demonstrates strict comparison, compound assignment, bitwise, and ternary operators

console.log("=== ECEngine Advanced Operators Demo ===");
console.log("");

// 1. Strict Comparison (===, !==)
console.log("1. Strict Comparison:");
var a = 5;
var b = "5";
var c = 5;

console.log("a === b:", a === b);  // false (number vs string)
console.log("a == b:", a == b);    // true (type coercion)
console.log("a === c:", a === c);  // true (same type and value)
console.log("a !== b:", a !== b);  // true (different types)
console.log("");

// 2. Compound Assignment (+=, -=, *=, /=)
console.log("2. Compound Assignment:");
var x = 10;
console.log("Initial x:", x);

x += 5;
console.log("After x += 5:", x);

x -= 3;
console.log("After x -= 3:", x);

x *= 2;
console.log("After x *= 2:", x);

x /= 4;
console.log("After x /= 4:", x);

// String concatenation with +=
var str = "Hello";
str += " World";
console.log("String concatenation:", str);
console.log("");

// 3. Bitwise Operators (&, |, ^, ~, <<, >>, >>>)
console.log("3. Bitwise Operators:");
var num1 = 12;  // 1100 in binary
var num2 = 10;  // 1010 in binary

console.log("num1 (12):", num1);
console.log("num2 (10):", num2);
console.log("num1 & num2 (AND):", num1 & num2);  // 8 (1000)
console.log("num1 | num2 (OR):", num1 | num2);   // 14 (1110)
console.log("num1 ^ num2 (XOR):", num1 ^ num2);  // 6 (0110)
console.log("~num1 (NOT):", ~num1);              // -13 (bitwise complement)

// Bit shifting
var shiftNum = 8;
console.log("shiftNum (8):", shiftNum);
console.log("shiftNum << 2:", shiftNum << 2);    // 32 (shift left)
console.log("shiftNum >> 1:", shiftNum >> 1);    // 4 (shift right)
console.log("shiftNum >>> 1:", shiftNum >>> 1);  // 4 (unsigned shift right)
console.log("");

// 4. Ternary Operator (? :)
console.log("4. Ternary Operator:");
var age = 25;
var status = age >= 18 ? "adult" : "minor";
console.log("Age:", age);
console.log("Status:", status);

var score = 85;
var grade = score >= 90 ? "A" : score >= 80 ? "B" : score >= 70 ? "C" : "F";
console.log("Score:", score);
console.log("Grade:", grade);

// Ternary with different data types
var isLoggedIn = true;
var message = isLoggedIn ? "Welcome back!" : null;
console.log("Message:", message);
console.log("");

// 5. Mixed Examples
console.log("5. Mixed Examples:");

// Ternary with compound assignment
var counter = 0;
var increment = true;
counter += increment ? 1 : -1;
console.log("Counter after conditional increment:", counter);

// Bitwise operations with ternary
var flags = 7;  // 111 in binary
var hasFlag = (flags & 4) !== 0 ? "yes" : "no";  // Check if bit 2 is set
console.log("Has flag 4:", hasFlag);

// Complex expression combining multiple operators
var result = (5 === 5) ? (10 | 5) + (3 << 1) : ~0;
console.log("Complex expression result:", result);
console.log("");

console.log("=== Advanced Operators Demo Complete ===");
