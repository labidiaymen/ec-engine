// ECEngine Advanced Module Features Demo
// This file demonstrates default exports, re-exports, and export renaming

// ===== DEFAULT EXPORTS =====

// 1. Default export with function declaration
export default function calculateSquare(x) {
    return x * x;
}

// Alternative syntax (export default expression)
// export default function(x) { return x * x; };

// 2. Default export with expression
// export default 42;

// 3. Default export with variable
// let defaultValue = "Hello World";
// export default defaultValue;

// ===== NAMED EXPORTS WITH RENAMING =====

// Declare some variables and functions
let internalValue = 100;
let secretKey = "my-secret-123";

function internalHelper() {
    return "helper function";
}

function utilityMethod(data) {
    return `Processing: ${data}`;
}

// Export with renaming using 'as' keyword
export { 
    internalValue as publicValue,
    secretKey as apiKey,
    internalHelper as helper,
    utilityMethod as process
};

// ===== MIXED EXPORTS =====

// Some exports with renaming, some without
let config = { debug: true, version: "1.0" };
let logger = function(msg) { console.log("[LOG]", msg); };
let maxRetries = 3;

export { 
    config,                    // exported as 'config'
    logger as log,            // exported as 'log' 
    maxRetries               // exported as 'maxRetries'
};

// ===== REGULAR EXPORTS (for comparison) =====

// These are the traditional export declarations
export let normalVariable = "normal export";
export function normalFunction() {
    return "normal function export";
}

// Note: This file demonstrates syntax but won't run standalone 
// since default exports can only have one per module
console.log("Advanced module features loaded!");
