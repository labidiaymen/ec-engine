// Advanced Observer Examples
// Demonstrating practical use cases for variable observation

// Example 1: Simple counter tracking
var counter = 0;
console.log("Starting counter at:", counter);

observe counter function(oldVal, newVal) {
    console.log("Counter changed from", oldVal, "to", newVal);
}

// Test the counter
counter = 50;
counter = 150;
counter = 75;

// Example 2: Name change tracking
var userName = "guest";
console.log("Initial user:", userName);

observe userName function(old, newName) {
    console.log("User changed from", old, "to", newName);
}

userName = "alice";
userName = "bob";
userName = "charlie";

// Example 3: State machine simulation
var state = "idle";
console.log("Initial state:", state);

observe state function(oldState, newState) {
    console.log("State transition:", oldState, "->", newState);
}

state = "loading";
state = "running";
state = "error";
state = "idle";

// Example 4: Multiple variables with different observers
var x = 1;
var y = 2;

observe x function() {
    console.log("x was modified!");
}

observe y function() {
    console.log("y was modified!");
}

x = 10;
y = 20;
x = 15;
