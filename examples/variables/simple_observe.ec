// Simple Observer Demo
// Shows basic usage of the observe feature

var x = 42;
console.log("x starts as:", x);

// Observer that logs changes
observe x function() {
    console.log("x changed!");
}

// Change x to trigger observer
x = 100;
console.log("x is now:", x);
