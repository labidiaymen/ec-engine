// Comprehensive Process Exit Scenarios Demo

console.log("=== Process Exit Scenarios Demo ===");

// Register event listeners
process.on('beforeExit', (code) => {
    console.log('beforeExit event triggered with code:', code);
});

process.on('exit', (code) => {
    console.log('exit event triggered with code:', code);
});

// Get command line arguments to determine which test to run
let testCase = process.argv[2] || "1";

switch (testCase) {
    case "1":
        console.log("\n--- Test 1: process.exit() with no arguments ---");
        console.log("This should trigger beforeExit since exitCode=0 and no explicit code");
        process.exit();
        break;
        
    case "2":
        console.log("\n--- Test 2: process.exit(0) with explicit 0 ---");
        console.log("This should NOT trigger beforeExit since explicit code was provided");
        process.exit(0);
        break;
        
    case "3":
        console.log("\n--- Test 3: process.exit(1) with non-zero code ---");
        console.log("This should NOT trigger beforeExit since exit code is not 0");
        process.exit(1);
        break;
        
    case "4":
        console.log("\n--- Test 4: Set exitCode then call process.exit() ---");
        process.exitCode = 42;
        console.log("Set exitCode to 42, now calling process.exit()");
        console.log("This should NOT trigger beforeExit since exitCode != 0");
        process.exit();
        break;
        
    default:
        console.log("Usage: node script.js [1|2|3|4]");
        console.log("1: process.exit() - should trigger beforeExit");
        console.log("2: process.exit(0) - should NOT trigger beforeExit"); 
        console.log("3: process.exit(1) - should NOT trigger beforeExit");
        console.log("4: exitCode=42 then process.exit() - should NOT trigger beforeExit");
}
