// Node.js Streams API Demo - Testing ECEngine Stream Implementation

const stream = require('node:stream');

console.log("=== ECEngine Streams Demo ===");

// Test 1: Basic Readable Stream
console.log("\n1. Creating Readable Stream:");
const readable = new stream.Readable();
console.log("Readable stream created:", readable);
console.log("Is readable?", readable.readable);

// Test 2: Basic Writable Stream
console.log("\n2. Creating Writable Stream:");
const writable = new stream.Writable();
console.log("Writable stream created:", writable);
console.log("Is writable?", writable.writable);

// Test 3: Basic Duplex Stream
console.log("\n3. Creating Duplex Stream:");
const duplex = new stream.Duplex();
console.log("Duplex stream created:", duplex);
console.log("Is readable?", duplex.readable);
console.log("Is writable?", duplex.writable);

// Test 4: Transform Stream
console.log("\n4. Creating Transform Stream:");
const transform = new stream.Transform();
console.log("Transform stream created:", transform);

// Test 5: PassThrough Stream
console.log("\n5. Creating PassThrough Stream:");
const passthrough = new stream.PassThrough();
console.log("PassThrough stream created:", passthrough);

// Test 6: Stream Module
console.log("\n6. Testing stream module:");
console.log("stream module:", stream);
console.log("stream.Readable:", stream.Readable);
console.log("stream.pipeline:", stream.pipeline);

// Test 7: Stream Utilities
console.log("\n7. Testing stream utilities:");
console.log("stream.isReadable(readable):", stream.isReadable(readable));
console.log("stream.isWritable(writable):", stream.isWritable(writable));

console.log("\n=== Stream Demo Complete ===");
