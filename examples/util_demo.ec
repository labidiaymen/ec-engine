// Node.js util Module Demo - Comprehensive utilities for ECEngine
console.log("🛠️  Node.js util Module Demo");
console.log("============================\n");

// Test util.inspect()
console.log("1. Testing util.inspect()...");
var obj = { name: "ECEngine", version: "1.0", features: ["modules", "events", "utils"] };
var arr = [1, 2, 3, "test", true];
var func = function(x) { return x * 2; };

console.log("   inspect(object):", util.inspect(obj));
console.log("   inspect(array):", util.inspect(arr));
console.log("   inspect(function):", util.inspect(func));
console.log("   inspect(null):", util.inspect(null));
console.log("   inspect(undefined):", util.inspect());
console.log();

// Test util.format()
console.log("2. Testing util.format()...");
console.log("   format('%s %d', 'Number:', 42):", util.format('%s %d', 'Number:', 42));
console.log("   format('%j', obj):", util.format('%j', obj));
console.log("   format('Hello %s!', 'World'):", util.format('Hello %s!', 'World'));
console.log("   format('No placeholders', 'extra', 'args'):", util.format('No placeholders', 'extra', 'args'));
console.log();

// Test type checking functions
console.log("3. Testing type checking functions...");

// Simple undefined placeholder
var undefined;

var testValues = [
    { name: "number", value: 42 },
    { name: "string", value: "test" },
    { name: "array", value: [1, 2, 3] },
    { name: "object", value: obj },
    { name: "null", value: null },
    { name: "undefined", value: undefined }
];

for (var i = 0; i < testValues.length; i++) {
    var test = testValues[i];
    console.log("   " + test.name + ":");
    console.log("     isArray:", util.isArray(test.value));
    console.log("     isFunction:", util.isFunction(test.value));
    console.log("     isNumber:", util.isNumber(test.value));
    console.log("     isObject:", util.isObject(test.value));
    console.log("     isPrimitive:", util.isPrimitive(test.value));
    console.log("     isString:", util.isString(test.value));
    console.log("     isUndefined:", util.isUndefined(test.value));
    console.log("     isNullOrUndefined:", util.isNullOrUndefined(test.value));
    console.log();
}

// Test util.isDeepStrictEqual()
console.log("4. Testing util.isDeepStrictEqual()...");
var obj1 = { a: 1, b: { c: 2 } };
var obj2 = { a: 1, b: { c: 2 } };
var obj3 = { a: 1, b: { c: 3 } };
var arr1 = [1, 2, [3, 4]];
var arr2 = [1, 2, [3, 4]];
var arr3 = [1, 2, [3, 5]];

console.log("   obj1 === obj2:", util.isDeepStrictEqual(obj1, obj2));
console.log("   obj1 === obj3:", util.isDeepStrictEqual(obj1, obj3));
console.log("   arr1 === arr2:", util.isDeepStrictEqual(arr1, arr2));
console.log("   arr1 === arr3:", util.isDeepStrictEqual(arr1, arr3));
console.log("   string === string:", util.isDeepStrictEqual("hello", "hello"));
console.log("   42 === 42:", util.isDeepStrictEqual(42, 42));
console.log();

// Test util.debuglog()
console.log("5. Testing util.debuglog()...");
var debug = util.debuglog("app");
console.log("   Created debug function for 'app' section");
// debug("This is a debug message");
// debug("Another debug message with", "multiple", "arguments");
console.log();

// Test util.types
console.log("6. Testing util.types module...");
console.log("   types.isDate(Date.now()):", util.types.isDate(Date.now()));
console.log("   types.isDate('string'):", util.types.isDate('string'));
console.log("   types.isRegExp('test'):", util.types.isRegExp('test'));
console.log("   types.isArrayBuffer('test'):", util.types.isArrayBuffer('test'));
console.log();

// Test placeholder functions (Promise-related)
console.log("7. Testing placeholder functions...");
function testCallback(err, result) {
    if (err) console.log("   Callback error:", err);
    else console.log("   Callback result:", result);
}

var promisifiedFunc = util.promisify(testCallback);
console.log("   promisify() placeholder: function");

var callbackifiedFunc = util.callbackify(function() { return "test"; });
console.log("   callbackify() placeholder: function");
console.log();

// Practical examples
console.log("8. Practical examples...");

// Object inspection for debugging
function debugObject(name, obj) {
    console.log("   DEBUG [" + name + "]:", util.inspect(obj));
}

debugObject("user", { id: 1, name: "Alice", preferences: { theme: "dark" } });
debugObject("config", { host: "localhost", port: 3000, ssl: false });

// Formatted logging
function logInfo(template, arg1, arg2) {
    console.log("   INFO:", util.format(template, arg1, arg2));
}

function logError(template, arg1, arg2) {
    console.log("   ERROR:", util.format(template, arg1, arg2));
}

logInfo("Server started on port %d", 3000);
logError("Failed to connect to %s (code: %d)", "database", 500);

// Type validation
function validateInput(input) {
    if (util.isNullOrUndefined(input)) {
        return { valid: false, reason: "Input is null or undefined" };
    }
    if (!util.isObject(input)) {
        return { valid: false, reason: "Input must be an object" };
    }
    if (!util.isString(input.name)) {
        return { valid: false, reason: "Name must be a string" };
    }
    if (!util.isNumber(input.age)) {
        return { valid: false, reason: "Age must be a number" };
    }
    return { valid: true };
}

console.log("   Validating valid input:", util.inspect(validateInput({ name: "Bob", age: 25 })));
console.log("   Validating invalid input:", util.inspect(validateInput({ name: "Bob" })));
console.log("   Validating null input:", util.inspect(validateInput(null)));
console.log();

console.log("✅ Node.js util Module Demo Complete!");
console.log("Available utilities:");
console.log("  • util.inspect() - Object inspection for debugging");
console.log("  • util.format() - Printf-style string formatting");
console.log("  • util.isArray/isFunction/isNumber/etc. - Type checking");
console.log("  • util.isDeepStrictEqual() - Deep object comparison");
console.log("  • util.debuglog() - Debug logging with sections");
console.log("  • util.types.* - Advanced type checking");
console.log("  • util.promisify/callbackify() - Promise utilities (placeholders)");
console.log("  • Comprehensive Node.js util API compatibility!");
