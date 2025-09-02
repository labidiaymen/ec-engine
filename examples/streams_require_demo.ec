// Test file: streams_require_demo.ec
// This demonstrates using Node.js-style streams with require()

const stream = require('node:stream');

console.log('Stream module loaded:', typeof stream);
console.log('Readable constructor:', typeof stream.Readable);
console.log('Writable constructor:', typeof stream.Writable);
console.log('Transform constructor:', typeof stream.Transform);
console.log('PassThrough constructor:', typeof stream.PassThrough);

// Test 1: Create a simple readable stream
console.log('\n=== Test 1: Readable Stream ===');
const readable = new stream.Readable();

console.log('Readable stream created:', typeof readable);
console.log('Readable property:', readable.readable);

// Test 2: Create a writable stream  
console.log('\n=== Test 2: Writable Stream ===');
const writable = new stream.Writable();

console.log('Writable stream created:', typeof writable);
console.log('Writable property:', writable.writable);

// Test 3: Create transform stream
console.log('\n=== Test 3: Transform Stream ===');
const transform = new stream.Transform();

console.log('Transform stream created:', typeof transform);

// Test 4: Create PassThrough stream
console.log('\n=== Test 4: PassThrough Stream ===');
const passthrough = new stream.PassThrough();

console.log('PassThrough stream created:', typeof passthrough);

// Test 5: Stream utilities
console.log('\n=== Test 5: Stream Utilities ===');
console.log('isReadable function:', typeof stream.isReadable);
console.log('isWritable function:', typeof stream.isWritable);
console.log('pipeline function:', typeof stream.pipeline);
console.log('finished function:', typeof stream.finished);
console.log('compose function:', typeof stream.compose);

console.log('\n=== Streams Demo Complete ===');
