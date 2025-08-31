// Node.js-style Module Resolution Test
// This example demonstrates all the Node.js module resolution features in ECEngine

console.log("=== ECEngine Node.js Module Resolution Test ===");
console.log("");

// Test 1: Module with package.json main field
console.log("1. Testing module with package.json main field...");
import { greet, capitalize, version } from "simple-utils";
console.log("   Imported functions from simple-utils:");
console.log("   - greet('World'):", greet("World"));
console.log("   - capitalize('hello'):", capitalize("hello"));
console.log("   - version:", version);
console.log("");

// Test 2: Module with package.json pointing to .ec file
console.log("2. Testing module with package.json pointing to .ec file...");
import { add, multiply, square, PI } from "math-helpers";
console.log("   Imported functions from math-helpers:");
console.log("   - add(5, 3):", add(5, 3));
console.log("   - multiply(4, 7):", multiply(4, 7));
console.log("   - square(6):", square(6));
console.log("   - PI:", PI);
console.log("");

// Test 3: Module without package.json (index.js fallback)
console.log("3. Testing module without package.json (index.js fallback)...");
import { formatDate, getCurrentTime } from "direct-module";
console.log("   Imported functions from direct-module:");
console.log("   - formatDate():", formatDate());
console.log("   - getCurrentTime():", getCurrentTime());
console.log("");

// Test 4: Direct file module with .mjs extension
console.log("4. Testing direct file module with .mjs extension...");
import { colors, getRandomColor } from "color-utils";
console.log("   Imported from color-utils.mjs:");
console.log("   - colors.red:", colors.red);
console.log("   - colors.green:", colors.green);
console.log("   - colors.blue:", colors.blue);
console.log("   - getRandomColor():", getRandomColor());
console.log("");

console.log("=== All Node.js Module Resolution Tests Completed ===");
console.log("");
console.log("Features demonstrated:");
console.log("✅ package.json main field resolution");
console.log("✅ Multi-extension support (.js, .ec, .mjs)");
console.log("✅ index.js fallback for directories");
console.log("✅ Direct file module resolution");
console.log("✅ node_modules directory lookup");
console.log("✅ Named imports from modules");
