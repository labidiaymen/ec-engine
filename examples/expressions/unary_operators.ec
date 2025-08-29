// Unary operators test

// Logical NOT operator
console.log("=== Logical NOT (!) ===");
let truth = true;
let falsy = false;
console.log("!true =", !truth);
console.log("!false =", !falsy);
console.log("!5 =", !5);    // Should be false (5 is truthy)
console.log("!0 =", !0);    // Should be true (0 is falsy)

// Unary plus and minus
console.log("\n=== Unary Plus/Minus ===");
let num = 42;
console.log("+42 =", +num);
console.log("-42 =", -num);
console.log("+(-5) =", +(-5));
console.log("-(-10) =", -(-10));

// Prefix increment and decrement
console.log("\n=== Prefix Increment/Decrement ===");
let x = 5;
console.log("x =", x);
console.log("++x =", ++x);  // Should be 6, x becomes 6
console.log("x after ++x =", x);

let y = 10;
console.log("y =", y);
console.log("--y =", --y);  // Should be 9, y becomes 9  
console.log("y after --y =", y);

// Postfix increment and decrement
console.log("\n=== Postfix Increment/Decrement ===");
let a = 7;
console.log("a =", a);
console.log("a++ =", a++);  // Should be 7, then a becomes 8
console.log("a after a++ =", a);

let b = 3;
console.log("b =", b);
console.log("b-- =", b--);  // Should be 3, then b becomes 2
console.log("b after b-- =", b);

// Complex expressions with unary operators
console.log("\n=== Complex Expressions ===");
let result1 = -(+5);
console.log("-(+5) =", result1);

let result2 = !!(42);
console.log("!!(42) =", result2);  // Double negation

let counter = 0;
console.log("counter =", counter);
console.log("++counter + counter++ =", ++counter + counter++);
console.log("counter after expression =", counter);
