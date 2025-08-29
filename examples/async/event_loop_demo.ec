// Event Loop Example - demonstrates asynchronous execution
// Shows how the event loop handles setTimeout-like behavior

console.log("=== Event Loop Demo ===");

console.log("1. Synchronous code starts");

// This would be asynchronous in a real implementation
console.log("2. Scheduling async task...");

console.log("3. More synchronous code");

console.log("4. Synchronous code ends");

// For now, let's demonstrate with observable patterns
// which can show async-like behavior with variable changes

var counter = 0;
console.log("Starting counter:", counter);

observe counter function(oldVal, newVal) {
    console.log("Counter changed from", oldVal, "to", newVal);
    
    when (newVal < 5) {
        console.log("  -> Counter is less than 5, continuing...");
    }
    
    when (newVal >= 5) {
        console.log("  -> Counter reached 5, stopping!");
    }
}

// Simulate async-like behavior by changing counter over time
counter = 1;
counter = 2;
counter = 3;
counter = 4;
counter = 5;

console.log("Final counter:", counter);
console.log("=== Demo Complete ===");
