// Advanced Module Features Demo - ECEngine
// This demonstrates the three advanced module features we implemented:
// 1. Default exports
// 2. Re-exports 
// 3. Export renaming

// Regular variable declarations
let value1 = 42;
let value2 = "hello world";
let secretValue = "internal";

// Regular function declaration
function greetUser(name) {
    console.log("Hello, " + name + "!");
}

// ===== 1. DEFAULT EXPORTS =====

// Default export with anonymous function
export default function() {
    console.log("This is a default exported anonymous function!");
    return "default result";
}

// Note: You could also do default export with expressions:
// export default 123;
// export default "some string";
// export default greetUser;

// ===== 2. EXPORT RENAMING =====

// Export variables with renaming (as keyword)
export { value1 as number, value2 as greeting };

// Export function with renaming
export { greetUser as sayHello };

// ===== 3. INLINE EXPORT DECLARATIONS =====

// Export declaration (declare and export in one statement)
export let exportedVar = "I'm exported!";
export function exportedFunction() {
    return "I'm an exported function!";
}

// ===== COMBINED EXAMPLE =====

// You can mix all patterns:
export { secretValue as publicValue };

/* 
Expected exports after running this module:
{
    "default": [Anonymous Function],
    "number": 42,
    "greeting": "hello world", 
    "sayHello": [Function greetUser],
    "exportedVar": "I'm exported!",
    "exportedFunction": [Function exportedFunction],
    "publicValue": "internal"
}
*/
