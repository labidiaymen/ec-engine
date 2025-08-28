// Variable Observer Example
// Demonstrates how to observe variable changes and react to them

// Declare a variable to observe
var x = 10;
console.log("Initial x:", x);

// Set up an observer that triggers when x changes
observe x function(oldValue, newValue, variableName) {
    console.log("Variable", variableName, "changed from", oldValue, "to", newValue);
}

// Change x - this should trigger the observer
x = 20;
console.log("x is now:", x);

// Change x again
x = 35;
console.log("x is now:", x);

// Let's observe another variable
var y = "hello";
console.log("Initial y:", y);

observe y function(oldVal, newVal) {
    console.log("y changed! Old:", oldVal, "New:", newVal);
}

// Change y
y = "world";
console.log("y is now:", y);

// Multiple observers on the same variable
observe x function() {
    console.log("Another observer detected x changed!");
}

// This should trigger both observers for x
x = 100;
console.log("Final x:", x);
