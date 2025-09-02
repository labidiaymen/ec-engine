// Process Events and Exit Demo
// Demonstrates process event handling and proper exit behavior

console.log("Process Events Demo Starting...");
console.log("Process ID:", process.pid);
console.log("Platform:", process.platform);
console.log("Node version:", process.version);
console.log("Current working directory:", process.cwd());

// Test event listener registration
console.log("\n--- Testing Event Listeners ---");

// Register beforeExit event
process.on('beforeExit', (code) => {
    console.log('Before exit event triggered with code:', code);
});

// Register exit event  
process.on('exit', (code) => {
    console.log('Exit event triggered with code:', code);
});

// Register a custom event for testing
process.on('test', (data) => {
    console.log('Test event received:', data);
});

// Test event methods
console.log("Event names:", process.eventNames());
console.log("beforeExit listener count:", process.listenerCount('beforeExit'));
console.log("exit listener count:", process.listenerCount('exit'));

// Test event emission
console.log("\n--- Testing Event Emission ---");
process.emit('test', 'Hello from custom event!');

// Test exitCode property
console.log("\n--- Testing Exit Code ---");
console.log("Initial exit code:", process.exitCode);
process.exitCode = 42;
console.log("Set exit code to 42:", process.exitCode);

// Test process.argv
console.log("\n--- Process Arguments ---");
console.log("Arguments:", process.argv);

// Test environment variables
console.log("\n--- Environment Variables ---");
console.log("PATH exists:", process.env.PATH ? "Yes" : "No");
console.log("HOME/USERPROFILE:", process.env.HOME || process.env.USERPROFILE);

// Test memory usage
console.log("\n--- Memory Usage ---");
console.log("Memory usage:", process.memoryUsage());

// Test uptime
console.log("\n--- Process Timing ---");
console.log("Uptime:", process.uptime(), "seconds");

// Test hrtime
let startTime = process.hrtime();
// Simulate some work
let sum = 0;
for (let i = 0; i < 1000000; i++) {
    sum += i;
}
let endTime = process.hrtime(startTime);
console.log("High resolution time for loop:", endTime[0] + "s " + endTime[1] + "ns");

// Test process.nextTick
console.log("\n--- Testing nextTick ---");
console.log("Before nextTick");

process.nextTick(() => {
    console.log("Inside nextTick callback");
});

console.log("After nextTick");

// Test removing event listeners
console.log("\n--- Testing Event Listener Removal ---");
let testHandler = (data) => {
    console.log("This should not be called:", data);
};

process.on('cleanup', testHandler);
console.log("cleanup listener count before removal:", process.listenerCount('cleanup'));
process.off('cleanup', testHandler);
console.log("cleanup listener count after removal:", process.listenerCount('cleanup'));

// Test that removed listener doesn't get called
process.emit('cleanup', 'test data');

console.log("\n--- Preparing to Exit ---");
console.log("About to call process.exit()...");

// Exit with the code we set earlier (42)
// This should trigger both beforeExit and exit events
process.exit();
