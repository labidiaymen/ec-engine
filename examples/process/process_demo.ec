// Node.js Process Object Demo
console.log('=== Node.js Process Object Demo ===');
console.log('');

// Basic process information
console.log('1. Process Information:');
console.log('   Process ID:', process.pid);
console.log('   Parent Process ID:', process.ppid);
console.log('   Platform:', process.platform);
console.log('   Architecture:', process.arch);
console.log('   Node.js Version:', process.version);
console.log('   Executable Path:', process.execPath);
console.log('');

// Command line arguments
console.log('2. Command Line Arguments:');
console.log('   argv length:', process.argv.length);
for (var i = 0; i < process.argv.length; i++) {
    console.log('   argv[' + i + ']:', process.argv[i]);
}
console.log('');

// Environment variables (show first few)
console.log('3. Environment Variables (sample):');
var envKeys = Object.keys(process.env);
console.log('   Total environment variables:', envKeys.length);
for (var i = 0; i < Math.min(5, envKeys.length); i++) {
    var key = envKeys[i];
    console.log('   ' + key + ':', process.env[key]);
}
console.log('');

// Current working directory
console.log('4. Current Working Directory:');
console.log('   cwd():', process.cwd());
console.log('');

// Process versions
console.log('5. Process Versions:');
console.log('   Node.js:', process.versions.node);
console.log('   V8:', process.versions.v8);
console.log('   ECEngine:', process.versions.ecengine);
console.log('');

// Memory usage
console.log('6. Memory Usage:');
var memUsage = process.memoryUsage();
console.log('   RSS:', memUsage.rss, 'bytes');
console.log('   Heap Total:', memUsage.heapTotal, 'bytes');
console.log('   Heap Used:', memUsage.heapUsed, 'bytes');
console.log('');

// Process uptime
console.log('7. Process Uptime:');
console.log('   Uptime:', process.uptime(), 'seconds');
console.log('');

// High resolution time
console.log('8. High Resolution Time:');
var hrtime = process.hrtime();
console.log('   hrtime:', hrtime[0], 'seconds,', hrtime[1], 'nanoseconds');
console.log('');

// Next tick example
console.log('9. Process NextTick Example:');
console.log('   Before nextTick');
process.nextTick(function() {
    console.log('   Inside nextTick callback');
});
console.log('   After nextTick');
console.log('');

console.log('Process demo completed!');
