// Node.js Events Module Advanced Features Demo
// Demonstrates prepend listeners, max listeners, static methods, and error handling

const events = require('events');

console.log('=== Node.js Events Module Advanced Demo ===\n');

// 1. EventEmitter Constructor Patterns
console.log('1. Constructor Patterns:');
const emitter1 = new events.EventEmitter();
const emitter2 = new events.EventEmitter();
console.log('✓ Created multiple EventEmitter instances');
console.log();

// 2. Prepend Listeners (add to beginning of listener array)
console.log('2. Prepend Listeners:');
emitter1.on('order', function() {
    console.log('  → Second listener');
});

emitter1.prependListener('order', function() {
    console.log('  → First listener (prepended)');
});

emitter1.on('order', function() {
    console.log('  → Third listener');
});

console.log('Emitting "order" event:');
emitter1.emit('order');
console.log();

// 3. Prepend Once Listeners
console.log('3. Prepend Once Listeners:');
emitter1.on('sequence', function() {
    console.log('  → Regular listener');
});

emitter1.prependOnceListener('sequence', function() {
    console.log('  → Once listener (prepended, auto-removes)');
});

console.log('First emit:');
emitter1.emit('sequence');
console.log('Second emit (once listener removed):');
emitter1.emit('sequence');
console.log();

// 4. Max Listeners Configuration
console.log('4. Max Listeners:');
console.log('Default max listeners:', events.defaultMaxListeners);
console.log('Emitter max listeners:', emitter1.getMaxListeners());

emitter1.setMaxListeners(5);
console.log('Updated max listeners:', emitter1.getMaxListeners());
console.log();

// 5. Static Utility Functions
console.log('5. Static Utility Functions:');
emitter1.on('static-test', function() { console.log('Listener 1'); });
emitter1.on('static-test', function() { console.log('Listener 2'); });

console.log('Listener count (static):', events.listenerCount(emitter1, 'static-test'));

const listeners = events.getEventListeners(emitter1, 'static-test');
console.log('Get listeners (static):', listeners.length, 'functions');
console.log();

// 6. Error Event Special Handling
console.log('6. Error Event Handling:');
emitter1.on('error', function(err) {
    console.log('  Error caught:', err);
});

console.log('Emitting error event:');
emitter1.emit('error', 'Test error message');
console.log();

// 7. Raw Listeners vs Regular Listeners
console.log('7. Raw vs Regular Listeners:');
function namedFunction() {
    console.log('  Named function called');
}

emitter1.once('raw-test', namedFunction);
console.log('Listeners:', emitter1.listenerCount('raw-test'));
console.log('Raw listeners:', emitter1.rawListeners('raw-test').length);

emitter1.emit('raw-test');
console.log('After emit, listeners:', emitter1.listenerCount('raw-test'));
console.log();

// 8. Module Import Patterns
console.log('8. Module Import Patterns:');
const standardEvents = require('events');
const nodeEvents = require('node:events');

console.log('Standard import - EventEmitter:', typeof standardEvents.EventEmitter);
console.log('Node-style import - EventEmitter:', typeof nodeEvents.EventEmitter);
console.log('Both are identical:', standardEvents.EventEmitter === nodeEvents.EventEmitter);
console.log();

// 9. Event Inspection and Management
console.log('9. Event Inspection:');
emitter1.on('inspect1', function() {});
emitter1.on('inspect2', function() {});

console.log('All event names:', emitter1.eventNames());
console.log('Total events:', emitter1.eventNames().length);
console.log();

// 10. Listener Removal Patterns
console.log('10. Listener Removal:');
function removableListener() {
    console.log('  This listener will be removed');
}

emitter1.on('removal-test', removableListener);
emitter1.on('removal-test', function() {
    console.log('  This listener will stay');
});

console.log('Before removal:', emitter1.listenerCount('removal-test'));
emitter1.removeListener('removal-test', removableListener);
console.log('After removal:', emitter1.listenerCount('removal-test'));

console.log('Emitting after removal:');
emitter1.emit('removal-test');
console.log();

console.log('🚀 Advanced Events Module demo completed!');
