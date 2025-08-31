// Test direct function calls in console.log
console.log("Testing direct function calls in console.log...");

import { greet } from "simple-utils";
console.log("Imported greet function");

// This might be the problematic pattern:
console.log("Direct call:", greet("World"));
