// Module System Demonstration - examples/modules/main.ec
// Shows how to import and use functions and constants from multiple modules

console.log("=== ECEngine Module System Demo ===");

// Import mathematical utilities
import { PI, E, add, multiply, square, circle_area, max, min, absolute } from "./math.ec";

// Import string utilities  
import { GREETING, repeat, make_greeting, create_title, format_number } from "./strings.ec";

console.log(create_title("Module System Features"));

// Demonstrate using imported constants
console.log("Mathematical Constants:");
console.log("  PI = " + PI);
console.log("  E = " + E);
console.log("  Golden ratio approximation: " + (PI / E));

console.log("");
console.log(create_title("Basic Math Operations"));

// Use imported math functions
var num1 = 15;
var num2 = 7;

console.log("Numbers: " + format_number(num1) + ", " + format_number(num2));
console.log("Addition: " + num1 + " + " + num2 + " = " + add(num1, num2));
console.log("Multiplication: " + num1 + " * " + num2 + " = " + multiply(num1, num2));
console.log("Maximum: " + max(num1, num2));
console.log("Minimum: " + min(num1, num2));

console.log("");
console.log(create_title("Advanced Calculations"));

// More complex calculations using multiple functions
var radius = 4;
var area = circle_area(radius);
console.log("Circle with radius " + radius + ":");
console.log("  Area = " + area);
console.log("  Area squared = " + square(area));

// Demonstrate absolute value
var negative = 0 - 25;  // Create negative number by subtraction
console.log("Absolute value of " + negative + " = " + absolute(negative));

console.log("");
console.log(create_title("String Operations"));

// Use imported string functions
var userName = "Alice";
var greeting = make_greeting(userName);
console.log(greeting);

var message = "Welcome";
var repeated = repeat(message + " ", 3);
console.log("Repeated message: " + repeated);

console.log("");
console.log(create_title("Combining Modules"));

// Combine math and string operations
var calculation = add(multiply(PI, 10), 5);
var result_message = "Result of (PI * 10) + 5 = " + format_number(calculation);
console.log(result_message);

// Create a formatted report
var score1 = 85;
var score2 = 92;
var average = add(score1, score2) / 2;

console.log("");
console.log("Score Report:");
console.log("  Score 1: " + format_number(score1));
console.log("  Score 2: " + format_number(score2));
console.log("  Average: " + format_number(average));
console.log("  Maximum: " + format_number(max(score1, score2)));

console.log("");
console.log(create_title("Module Demo Complete"));
console.log("Successfully demonstrated:");
console.log("  ✓ Importing constants and functions");
console.log("  ✓ Using multiple modules together");
console.log("  ✓ Cross-module function composition");
console.log("  ✓ Module initialization messages");
