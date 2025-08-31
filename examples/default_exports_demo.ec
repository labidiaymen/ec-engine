// ECEngine Default Exports Comprehensive Demo
// This file demonstrates all aspects of default exports

// ===== DEFAULT EXPORT PATTERNS =====

// 1. Function declaration as default export
export default function greet(name) {
    return `Hello, ${name}!`;
}

// 2. Anonymous function as default export
// export default function(x, y) {
//     return x + y;
// };

// 3. Expression as default export
// export default 42;
// export default "Hello World";
// export default { name: "Config", version: 1.0 };
// export default [1, 2, 3, 4, 5];

// 4. Variable as default export
// let calculator = {
//     add: (a, b) => a + b,
//     subtract: (a, b) => a - b,
//     multiply: (a, b) => a * b,
//     divide: (a, b) => b !== 0 ? a / b : null
// };
// export default calculator;

// 5. Class as default export (when classes are implemented)
// export default class MyClass {
//     constructor(value) {
//         this.value = value;
//     }
//     getValue() {
//         return this.value;
//     }
// }

// ===== COMBINING DEFAULT AND NAMED EXPORTS =====

// You can have both default and named exports in the same module

// Named exports
export let version = "2.1.0";
export let author = "ECEngine Team";

export function getInfo() {
    return {
        version: version,
        author: author,
        defaultFunction: "greet"
    };
}

// Configuration object
export let config = {
    debug: true,
    maxRetries: 3,
    timeout: 5000
};

// Utility functions
export function logMessage(message) {
    if (config.debug) {
        console.log(`[${new Date().toISOString()}] ${message}`);
    }
}

export function createTimestamp() {
    return new Date().toISOString();
}

// ===== COMPLEX DEFAULT EXPORTS =====

// Default export with complex logic
// export default function createProcessor(options) {
//     const defaultOptions = { verbose: false, strict: true };
//     const finalOptions = { ...defaultOptions, ...options };
//     
//     return {
//         process: function(data) {
//             if (finalOptions.verbose) {
//                 console.log("Processing data:", data);
//             }
//             
//             if (finalOptions.strict && !data) {
//                 throw new Error("Data is required in strict mode");
//             }
//             
//             return `Processed: ${data || "empty"}`;
//         },
//         
//         getOptions: function() {
//             return finalOptions;
//         }
//     };
// };

// ===== USAGE EXAMPLES (in comments for reference) =====

/*
// How this module would be imported:

// Import default export
import greet from "./default_exports_demo";
// or
import myGreetFunction from "./default_exports_demo"; // Can rename default imports

// Import named exports  
import { version, author, getInfo } from "./default_exports_demo";

// Import both default and named
import greet, { version, config, logMessage } from "./default_exports_demo";

// Import all
import * as MyModule from "./default_exports_demo";
// Then use: MyModule.default() for default export, MyModule.version for named exports
*/

console.log("Default exports demo loaded!");
