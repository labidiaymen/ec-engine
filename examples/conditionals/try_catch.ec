// Try-catch-finally examples in ECEngine

// Basic try-catch
console.log("=== Basic Try-Catch ===");

try {
    console.log("Trying some operation...");
    throw "Something went wrong!";
    console.log("This won't be executed");
} catch (error) {
    console.log("Caught error: " + error);
}

console.log("Execution continues after try-catch");

// Try-catch with variable binding
console.log("\n=== Try-Catch with Error Binding ===");

function riskyOperation(shouldFail) {
    if (shouldFail) {
        throw "Operation failed as requested";
    }
    return "Operation succeeded";
}

try {
    var result = riskyOperation(true);
    console.log("Result: " + result);
} catch (e) {
    console.log("Error occurred: " + e);
}

// Try-finally (without catch)
console.log("\n=== Try-Finally ===");

try {
    console.log("Executing some code...");
    console.log("Code executed successfully");
} finally {
    console.log("Cleanup code in finally block");
}

// Try-catch-finally
console.log("\n=== Try-Catch-Finally ===");

function processData(data) {
    if (data == null) {
        throw "Data cannot be null";
    }
    return "Processed: " + data;
}

try {
    console.log("Processing data...");
    var processed = processData(null);
    console.log(processed);
} catch (error) {
    console.log("Error during processing: " + error);
} finally {
    console.log("Cleaning up resources...");
}

// Nested try-catch
console.log("\n=== Nested Try-Catch ===");

try {
    console.log("Outer try block");
    
    try {
        console.log("Inner try block");
        throw "Inner error";
    } catch (innerError) {
        console.log("Inner catch: " + innerError);
        throw "Re-throwing from inner catch";
    }
    
} catch (outerError) {
    console.log("Outer catch: " + outerError);
}

// Multiple catch scenarios
console.log("\n=== Error Handling in Functions ===");

function divide(a, b) {
    try {
        if (b == 0) {
            throw "Division by zero error";
        }
        return a / b;
    } catch (error) {
        console.log("Error in divide function: " + error);
        return null;
    } finally {
        console.log("Division operation completed");
    }
}

var result1 = divide(10, 2);
console.log("10 / 2 = " + result1);

var result2 = divide(10, 0);
console.log("10 / 0 = " + result2);
