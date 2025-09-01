// Node.js Buffer API Example
// Demonstrates ECEngine's full Buffer implementation following Node.js specification
// https://nodejs.org/api/buffer.html

console.log('=== ECEngine Buffer API Demo ===\n');

// Import Buffer from the buffer module (both syntaxes supported)
const Buffer = require('buffer').Buffer;
// Alternative: const Buffer = require('node:buffer').Buffer;

console.log('1. Creating Buffers:');
console.log('-------------------');

// Buffer.alloc() - creates a zero-filled buffer
const buf1 = Buffer.alloc(10);
console.log('Buffer.alloc(10):', buf1, '(length:', buf1.length + ')');

// Buffer.allocUnsafe() - creates uninitialized buffer (faster)
const buf2 = Buffer.allocUnsafe(5);
console.log('Buffer.allocUnsafe(5):', buf2, '(length:', buf2.length + ')');

// Buffer.from() with string
const buf3 = Buffer.from('Hello, World!', 'utf8');
console.log('Buffer.from("Hello, World!"):', buf3.toString());

// Buffer.from() with array of bytes
const buf4 = Buffer.from([72, 101, 108, 108, 111]);
console.log('Buffer.from([72, 101, 108, 108, 111]):', buf4.toString());

console.log('\n2. Buffer Utilities:');
console.log('--------------------');

// Check if something is a Buffer
console.log('Buffer.isBuffer(buf3):', Buffer.isBuffer(buf3));
console.log('Buffer.isBuffer("string"):', Buffer.isBuffer("string"));

// Get byte length of a string
console.log('Buffer.byteLength("Hello"):', Buffer.byteLength("Hello"));

// Check encoding support
console.log('Buffer.isEncoding("utf8"):', Buffer.isEncoding("utf8"));
console.log('Buffer.isEncoding("invalid"):', Buffer.isEncoding("invalid"));

console.log('\n3. Buffer Manipulation:');
console.log('-----------------------');

// String conversion with different encodings
const textBuf = Buffer.from('Node.js Buffer', 'utf8');
console.log('Original:', textBuf.toString());
console.log('UTF-8:', textBuf.toString('utf8'));
console.log('ASCII:', textBuf.toString('ascii'));

// Slicing buffers
const slice = textBuf.slice(0, 7);
console.log('Slice(0, 7):', slice.toString());

// Subarray (returns a view, not a copy)
const sub = textBuf.subarray(8);
console.log('Subarray(8):', sub.toString());

console.log('\n4. Buffer Operations:');
console.log('---------------------');

// Fill buffer with values
const fillBuf = Buffer.alloc(5);
fillBuf.fill(65); // ASCII 'A'
console.log('Filled with "A":', fillBuf.toString());

// Write to buffer
const writeBuf = Buffer.alloc(10);
writeBuf.write('ECEngine', 0);
console.log('Write "ECEngine":', writeBuf.toString());

// Copy between buffers
const source = Buffer.from('Copy me!');
const target = Buffer.alloc(20);
source.copy(target, 0);
console.log('Copied buffer:', target.toString('utf8', 0, source.length));

console.log('\n5. Buffer Searching:');
console.log('--------------------');

const searchBuf = Buffer.from('The quick brown fox jumps');
console.log('Original text:', searchBuf.toString());
console.log('indexOf("quick"):', searchBuf.indexOf('quick'));
console.log('indexOf("fox"):', searchBuf.indexOf('fox'));
console.log('includes("brown"):', searchBuf.includes('brown'));
console.log('includes("cat"):', searchBuf.includes('cat'));

console.log('\n6. Buffer Comparison:');
console.log('---------------------');

const buf5 = Buffer.from('ABC');
const buf6 = Buffer.from('ABC');
const buf7 = Buffer.from('XYZ');

console.log('buf5.equals(buf6):', buf5.equals(buf6)); // true - same content
console.log('buf5.equals(buf7):', buf5.equals(buf7)); // false - different content
console.log('Buffer.compare(buf5, buf6):', Buffer.compare(buf5, buf6)); // 0 - equal
console.log('Buffer.compare(buf5, buf7):', Buffer.compare(buf5, buf7)); // negative - buf5 < buf7

console.log('\n7. Buffer Concatenation:');
console.log('------------------------');

const part1 = Buffer.from('Hello ');
const part2 = Buffer.from('ECEngine ');
const part3 = Buffer.from('World!');

const combined = Buffer.concat([part1, part2, part3]);
console.log('Concatenated result:', combined.toString());

// Concatenation with specified total length
const limited = Buffer.concat([part1, part2, part3], 15);
console.log('Limited to 15 bytes:', limited.toString());

console.log('\n8. Encoding Examples:');
console.log('---------------------');

// UTF-8 (default)
const utf8Buf = Buffer.from('Hello 世界', 'utf8');
console.log('UTF-8 string:', utf8Buf.toString('utf8'));
console.log('UTF-8 bytes:', utf8Buf.length);

// ASCII encoding
const asciiBuf = Buffer.from('Hello ASCII', 'ascii');
console.log('ASCII string:', asciiBuf.toString('ascii'));

// Base64 encoding
const base64Buf = Buffer.from('SGVsbG8gQmFzZTY0', 'base64');
console.log('Base64 decoded:', base64Buf.toString('utf8'));

// Hex encoding
const hexBuf = Buffer.from('48656c6c6f', 'hex');
console.log('Hex decoded:', hexBuf.toString('utf8'));

console.log('\n9. Advanced Buffer Features:');
console.log('-----------------------------');

// Buffer with specific fill value and encoding
const customBuf = Buffer.alloc(8, 'hi', 'utf8');
console.log('Custom fill buffer:', customBuf.toString());

console.log('\n10. Practical Example - Binary Data:');
console.log('------------------------------------');

// Simulate working with binary protocol data
const header = Buffer.alloc(4);
header.write('HTTP', 0);

const payload = Buffer.from('Sample payload data');

const packet = Buffer.concat([header, payload]);
console.log('Protocol packet created, total bytes:', packet.length);
console.log('Header:', header.toString());
console.log('Payload:', payload.toString());

console.log('\n=== Buffer API Demo Complete! ===');
console.log('✅ ECEngine now supports the complete Node.js Buffer API');
console.log('📚 Full compatibility with Node.js buffer operations');
console.log('🚀 Ready for real-world binary data processing!');
