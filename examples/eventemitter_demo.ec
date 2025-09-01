// EventEmitter Example - demonstrates event-driven programming in ECEngine
console.log("🎯 EventEmitter Example");
console.log("======================\n");

// Create a new EventEmitter instance
console.log("1. Creating EventEmitter instance...");
var emitter = EventEmitter.createEventEmitter();
console.log("   EventEmitter created successfully!");
console.log();

// Add event listeners
console.log("2. Adding event listeners...");

emitter.on("hello", function(name) {
    console.log("   Hello listener called with:", name);
});

emitter.on("hello", function(name) {
    console.log("   Second hello listener called with:", name);
});

emitter.on("data", function(value) {
    console.log("   Data received:", value);
});

emitter.on("calculate", function(a, b) {
    var result = a + b;
    console.log("   Calculate result:", a, "+", b, "=", result);
});

console.log("   Event listeners added!");
console.log();

// Test event emission
console.log("3. Testing event emission...");

console.log("   Emitting 'hello' event:");
var hadListeners1 = emitter.emit("hello", "ECEngine");
console.log("   Had listeners:", hadListeners1);
console.log();

console.log("   Emitting 'data' event:");
var hadListeners2 = emitter.emit("data", { type: "test", value: 42 });
console.log("   Had listeners:", hadListeners2);
console.log();

console.log("   Emitting 'calculate' event:");
var hadListeners3 = emitter.emit("calculate", 10, 5);
console.log("   Had listeners:", hadListeners3);
console.log();

// Test non-existent event
console.log("   Emitting non-existent 'missing' event:");
var hadListeners4 = emitter.emit("missing", "test");
console.log("   Had listeners:", hadListeners4);
console.log();

// Test EventEmitter methods
console.log("4. Testing EventEmitter methods...");

console.log("   Listener count for 'hello':", emitter.listenerCount("hello"));
console.log("   Listener count for 'data':", emitter.listenerCount("data"));
console.log("   Listener count for 'missing':", emitter.listenerCount("missing"));
console.log();

console.log("   Event names:", JSON.stringify(emitter.eventNames()));
console.log();

// Test removeListener
console.log("5. Testing listener removal...");

function namedListener(message) {
    console.log("   Named listener called with:", message);
}

emitter.on("test", namedListener);
console.log("   Added named listener to 'test' event");

console.log("   Emitting 'test' event:");
emitter.emit("test", "before removal");

emitter.off("test", namedListener);
console.log("   Removed named listener from 'test' event");

console.log("   Emitting 'test' event again:");
var hadListeners5 = emitter.emit("test", "after removal");
console.log("   Had listeners:", hadListeners5);
console.log();

// Test removeAllListeners
console.log("6. Testing removeAllListeners...");
console.log("   Before removal - 'hello' listener count:", emitter.listenerCount("hello"));

emitter.removeAllListeners("hello");
console.log("   Removed all 'hello' listeners");
console.log("   After removal - 'hello' listener count:", emitter.listenerCount("hello"));

console.log("   Emitting 'hello' event after removal:");
var hadListeners6 = emitter.emit("hello", "should have no listeners");
console.log("   Had listeners:", hadListeners6);
console.log();

console.log("✅ EventEmitter Example Complete!");
console.log("Event-driven programming working in ECEngine!");
