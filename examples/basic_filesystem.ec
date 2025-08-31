// Basic filesystem operations using the built-in fs module
// This example shows common filesystem operations

// Import the filesystem module like Node.js
const fs = require('fs');
const path = require('path');

console.log("Basic Filesystem Operations Demo");

// 1. Write a file
const fileName = 'hello.txt';
const content = 'Hello, World!\nThis is written by ECEngine.';

console.log("\n1. Writing file...");
fs.writeFileSync(fileName, content);
console.log(`✓ File '${fileName}' written successfully`);

// 2. Read the file
console.log("\n2. Reading file...");
const readContent = fs.readFileSync(fileName);
console.log("File content:");
console.log(readContent);

// 3. Check if file exists and get stats
console.log("\n3. File information:");
const stats = fs.statSync(fileName);
console.log(`File size: ${stats.size} bytes`);
console.log(`Is file: ${stats.isFile()}`);
console.log(`Modified: ${stats.mtime}`);

// 4. Create a directory
const dirName = 'test-directory';
console.log(`\n4. Creating directory '${dirName}'...`);
try {
    fs.mkdirSync(dirName);
    console.log(`✓ Directory '${dirName}' created`);
} catch (error) {
    console.log("Directory might already exist");
}

// 5. List current directory
console.log("\n5. Current directory contents:");
const files = fs.readdirSync('.');
console.log(files);

// 6. Path operations
console.log("\n6. Path operations:");
const fullPath = path.join(dirName, 'subfile.txt');
console.log(`Joined path: ${fullPath}`);
console.log(`Directory name: ${path.dirname(fullPath)}`);
console.log(`File name: ${path.basename(fullPath)}`);
console.log(`Extension: ${path.extname(fullPath)}`);

// 7. Cleanup
console.log("\n7. Cleaning up...");
fs.unlinkSync(fileName);
console.log(`✓ File '${fileName}' deleted`);

try {
    fs.rmdirSync(dirName);
    console.log(`✓ Directory '${dirName}' removed`);
} catch (error) {
    console.log("Directory might not be empty or doesn't exist");
}

console.log("\nDemo completed!");
