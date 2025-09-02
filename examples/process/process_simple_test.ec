// Simple Process Test
console.log("=== Process Basic Test ===");

// Test basic properties
console.log("Process exists:", typeof process);
console.log("Process PID:", process.pid);

// Test exitCode property access
console.log("Initial exitCode:", process.exitCode);

// Test event methods exist
console.log("on method:", typeof process.on);
console.log("emit method:", typeof process.emit);
console.log("eventNames method:", typeof process.eventNames);

// Try to set exitCode
try {
    process.exitCode = 123;
    console.log("After setting exitCode to 123:", process.exitCode);
} catch (e) {
    console.log("Error setting exitCode:", e.message);
}

// Try to add event listener
try {
    process.on('test', function(data) {
        console.log("Test event received:", data);
    });
    console.log("Event listener added successfully");
    console.log("Event names after adding listener:", process.eventNames());
    console.log("Test listener count:", process.listenerCount('test'));
} catch (e) {
    console.log("Error adding event listener:", e.message);
}

// Try to emit event
try {
    process.emit('test', 'hello world');
} catch (e) {
    console.log("Error emitting event:", e.message);
}

console.log("=== Test Complete ===");
