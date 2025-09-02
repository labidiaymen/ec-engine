// Process Exit Handling Demo
// Demonstrates different exit scenarios

console.log("=== Process Exit Demo ===");

// Register event listeners
process.on('beforeExit', (code) => {
    console.log('beforeExit event: Process is about to exit with code', code);
});

process.on('exit', (code) => {
    console.log('exit event: Process is exiting with code', code);
});

// Test 1: Normal exit with no code (should trigger beforeExit)
console.log("\n--- Test 1: Normal exit (no code) ---");
console.log("Current exitCode:", process.exitCode);
console.log("Calling process.exit() with no arguments...");
console.log("This should trigger beforeExit since exitCode is 0");

// This should trigger both beforeExit and exit events
process.exit();
