// Advanced EventEmitter Example - File Processing Simulation
console.log("📁 Advanced EventEmitter Example - File Processing Simulation");
console.log("===============================================================\n");

// Create a file processor that emits events during processing
function createFileProcessor() {
    var processor = EventEmitter.createEventEmitter();
    
    // Add a process method to simulate file processing
    processor.processFile = function(filename) {
        console.log("🔄 Starting to process:", filename);
        
        // Emit start event
        processor.emit("start", { filename: filename, timestamp: "2025-09-01T10:00:00Z" });
        
        // Simulate reading file (after 100ms delay simulation)
        processor.emit("reading", { filename: filename, stage: "reading headers" });
        
        // Simulate validation
        processor.emit("validating", { filename: filename, stage: "validating format" });
        
        // Simulate processing
        processor.emit("processing", { 
            filename: filename, 
            stage: "transforming data",
            progress: 50 
        });
        
        // Simulate completion
        processor.emit("complete", { 
            filename: filename, 
            status: "success",
            result: "File processed successfully",
            timestamp: "2025-09-01T10:00:05Z"
        });
    };
    
    return processor;
}

// Create the processor
console.log("1. Creating file processor...");
var fileProcessor = createFileProcessor();
console.log("   File processor created!");
console.log();

// Set up event listeners for different stages
console.log("2. Setting up event listeners...");

fileProcessor.on("start", function(data) {
    console.log("   📋 STARTED:", data.filename, "at", data.timestamp);
});

fileProcessor.on("reading", function(data) {
    console.log("   📖 READING:", data.stage, "for", data.filename);
});

fileProcessor.on("validating", function(data) {
    console.log("   ✅ VALIDATING:", data.stage, "for", data.filename);
});

fileProcessor.on("processing", function(data) {
    console.log("   ⚙️  PROCESSING:", data.stage, "for", data.filename, "- Progress:", data.progress + "%");
});

fileProcessor.on("complete", function(data) {
    console.log("   🎉 COMPLETED:", data.filename, "-", data.result);
    console.log("   📊 Status:", data.status, "at", data.timestamp);
});

// Set up error handling
fileProcessor.on("error", function(error) {
    console.log("   ❌ ERROR:", error.message, "in file:", error.filename);
});

console.log("   Event listeners configured!");
console.log();

// Process multiple files
console.log("3. Processing files...");
console.log();

fileProcessor.processFile("document.pdf");
console.log();

fileProcessor.processFile("data.csv");
console.log();

fileProcessor.processFile("image.jpg");
console.log();

// Demonstrate event listener management
console.log("4. Demonstrating listener management...");

console.log("   Current event names:", JSON.stringify(fileProcessor.eventNames()));
console.log("   'start' listeners:", fileProcessor.listenerCount("start"));
console.log("   'complete' listeners:", fileProcessor.listenerCount("complete"));
console.log();

// Add a logger that captures all events
console.log("5. Adding universal logger...");

function universalLogger(eventName, data) {
    console.log("   📝 LOG [" + eventName + "]:", JSON.stringify(data));
}

// Add logger to multiple events
fileProcessor.on("start", function(data) { universalLogger("start", data); });
fileProcessor.on("complete", function(data) { universalLogger("complete", data); });

console.log("   Universal logger added for start and complete events");
console.log();

console.log("6. Processing with logging...");
fileProcessor.processFile("report.xlsx");
console.log();

// Demonstrate event-driven architecture pattern
console.log("7. Event-driven notification system...");

var notificationSystem = EventEmitter.createEventEmitter();

// Set up different notification channels
notificationSystem.on("user-login", function(user) {
    console.log("   📧 Email: Welcome back,", user.name + "!");
});

notificationSystem.on("user-login", function(user) {
    console.log("   📱 Push: User", user.name, "logged in from", user.location);
});

notificationSystem.on("user-login", function(user) {
    console.log("   📊 Analytics: Login event recorded for user ID", user.id);
});

notificationSystem.on("order-placed", function(order) {
    console.log("   📧 Email: Order confirmation for $" + order.amount);
    console.log("   📦 Shipping: Preparing order #" + order.id);
    console.log("   💳 Payment: Processing payment of $" + order.amount);
});

// Trigger notifications
console.log("   Triggering user login...");
notificationSystem.emit("user-login", {
    id: 123,
    name: "Alice",
    location: "New York"
});
console.log();

console.log("   Triggering order placement...");
notificationSystem.emit("order-placed", {
    id: "ORD-001",
    amount: 99.99,
    customer: "Alice"
});
console.log();

console.log("✅ Advanced EventEmitter Example Complete!");
console.log("Event-driven architecture enables:");
console.log("  • Decoupled components");
console.log("  • Flexible notification systems");
console.log("  • Real-time processing updates");
console.log("  • Easy extensibility");
console.log("  • Clean separation of concerns");
