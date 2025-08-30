// ECEngine Math Object Demo
// Demonstrates the complete Math API implementation

console.log("=== ECEngine Math Object Demo ===");
console.log("");

// Mathematical Constants
console.log("📊 Mathematical Constants:");
console.log("Math.PI =", Math.PI);
console.log("Math.E =", Math.E);
console.log("Math.LN2 =", Math.LN2);
console.log("Math.LN10 =", Math.LN10);
console.log("Math.SQRT2 =", Math.SQRT2);
console.log("Math.SQRT1_2 =", Math.SQRT1_2);
console.log("");

// Basic Math Operations
console.log("🔢 Basic Math Functions:");
console.log("Math.abs(-42) =", Math.abs(-42));
console.log("Math.ceil(4.3) =", Math.ceil(4.3));
console.log("Math.floor(4.9) =", Math.floor(4.9));
console.log("Math.round(4.5) =", Math.round(4.5));
console.log("Math.trunc(4.9) =", Math.trunc(4.9));
console.log("");

// Trigonometry
console.log("📐 Trigonometric Functions:");
console.log("Math.sin(Math.PI/2) =", Math.sin(Math.PI / 2));
console.log("Math.cos(0) =", Math.cos(0));
console.log("Math.tan(Math.PI/4) =", Math.tan(Math.PI / 4));
console.log("Math.asin(1) =", Math.asin(1));
console.log("Math.acos(1) =", Math.acos(1));
console.log("Math.atan(1) =", Math.atan(1));
console.log("Math.atan2(1, 1) =", Math.atan2(1, 1));
console.log("");

// Powers and Roots
console.log("⚡ Power and Root Functions:");
console.log("Math.sqrt(16) =", Math.sqrt(16));
console.log("Math.pow(2, 8) =", Math.pow(2, 8));
console.log("Math.exp(1) =", Math.exp(1));
console.log("Math.log(Math.E) =", Math.log(Math.E));
console.log("");

// Min/Max
console.log("🔢 Min/Max Functions:");
console.log("Math.min(5, 3, 9, 1) =", Math.min(5, 3, 9, 1));
console.log("Math.max(5, 3, 9, 1) =", Math.max(5, 3, 9, 1));
console.log("");

// Random Numbers
console.log("🎲 Random Number Generation:");
console.log("Math.random() =", Math.random());
console.log("Random integer 1-10:", Math.floor(Math.random() * 10) + 1);
console.log("Random integer 1-100:", Math.floor(Math.random() * 100) + 1);
console.log("");

// Practical Examples
console.log("🌟 Practical Examples:");

// Calculate circle area
var radius = 5;
var area = Math.PI * Math.pow(radius, 2);
console.log("Circle area (radius=" + radius + "):", area);

// Calculate distance between two points
var x1 = 0;
var y1 = 0;
var x2 = 3;
var y2 = 4;
var distance = Math.sqrt(Math.pow(x2 - x1, 2) + Math.pow(y2 - y1, 2));
console.log("Distance between (0,0) and (3,4):", distance);

// Calculate degrees to radians
var degrees = 45;
var radians = degrees * Math.PI / 180;
console.log(degrees + " degrees in radians:", radians);

// Calculate compound interest
var principal = 1000;
var rate = 0.05;
var time = 10;
var amount = principal * Math.pow(1 + rate, time);
console.log("Compound interest: $" + principal + " at " + (rate * 100) + "% for " + time + " years = $" + Math.round(amount));

console.log("");
console.log("✅ Math object implementation complete!");
console.log("Available: All standard JavaScript Math constants and functions");
