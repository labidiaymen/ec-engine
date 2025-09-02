// EventEmitter Basic Usage Demo
// This example demonstrates the core functionality of Node.js EventEmitter

const events = require('events');
const EventEmitter = events.EventEmitter;

console.log('=== EventEmitter Basic Demo ===\n');

// Create a new EventEmitter instance
const emitter = new EventEmitter();

// 1. Basic event handling
console.log('1. Basic Event Handling:');
emitter.on('greet', function(name) {
    console.log('Hello,', name + '!');
});

emitter.emit('greet', 'World');
emitter.emit('greet', 'ECEngine');
console.log();

// 2. Once listeners (auto-remove after first call)
console.log('2. Once Listeners:');
emitter.once('startup', function() {
    console.log('✓ Application started!');
});

emitter.emit('startup'); // Will log
emitter.emit('startup'); // Won't log (already removed)
console.log();

// 3. Multiple listeners for same event
console.log('3. Multiple Listeners:');
emitter.on('data', function(data) {
    console.log('  Logger:', data);
});

emitter.on('data', function(data) {
    console.log('  Processor:', data);
});

emitter.emit('data', 'User clicked button');
console.log();

// 4. Event with multiple arguments
console.log('4. Multiple Arguments:');
emitter.on('user-action', function(action, userId, timestamp) {
    console.log('Action:', action, '| User:', userId, '| Time:', timestamp);
});

emitter.emit('user-action', 'login', 'user123', Date.now());
console.log();

// 5. Listener management
console.log('5. Listener Management:');
function tempListener() {
    console.log('This is a temporary listener');
}

emitter.on('temp', tempListener);
console.log('Listeners before removal:', emitter.listenerCount('temp'));

emitter.removeListener('temp', tempListener);
console.log('Listeners after removal:', emitter.listenerCount('temp'));
console.log();

// 6. Error handling
console.log('6. Error Handling:');
emitter.on('error', function(err) {
    console.log('Caught error:', err);
});

emitter.emit('error', 'Something went wrong!');
console.log();

// 7. Event inspection
console.log('7. Event Inspection:');
console.log('All event names:', emitter.eventNames());
console.log();

console.log('🎉 EventEmitter demo completed!');
