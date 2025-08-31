// Test function calls step by step
console.log("Testing function calls step by step...");

import { greet } from "simple-utils";
console.log("Imported greet function");
console.log("About to call greet function...");

// Try calling it
var result = greet("Test");
console.log("Function call result:", result);
