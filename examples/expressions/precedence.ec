// Parentheses and Operator Precedence
// How grouping affects expression evaluation

var a = 2;
var b = 3;
var c = 4;

// Without parentheses (following operator precedence)
var result1 = a + b * c;
console.log("a + b * c =", result1); // Should be 14 (2 + 12)

// With parentheses to change order
var result2 = (a + b) * c;
console.log("(a + b) * c =", result2); // Should be 20 (5 * 4)

// Complex nested expressions
var result3 = ((a + b) * c) - (a * b);
console.log("((a + b) * c) - (a * b) =", result3); // 20 - 6 = 14

// Multiple levels of nesting
var result4 = (a + (b * c)) / (a + b);
console.log("(a + (b * c)) / (a + b) =", result4); // (2 + 12) / 5 = 2.8
