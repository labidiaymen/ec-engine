// Advanced Multi-Variable Observe Pattern
// Demonstrates reactive programming with multiple variables and change tracking

// Initialize variables
let x = 0 ;  
let y = 0;

observe (x, y) function(changes) {
    console.log("Always: got change from", changes.triggered);

    when x {
        console.log("x changed from", changes.x.old, "to", changes.x.new);
    }

    when y {
        console.log("y changed from", changes.y.old, "to", changes.y.new);
    }

    when (changes.values.x && changes.values.y) {
        console.log("Both truthy!");
    }

    console.log("Final snapshot:", changes.values);
}

// Test the multi-variable observer
console.log("Setting x = 1");
x = 1;
// -> Always: got change from [x]
// -> x changed from 0 to 1
// -> Final snapshot: {x:1, y:0}

console.log("Setting y = 2");
y = 2;
// -> Always: got change from [y]
// -> y changed from 0 to 2
// -> Both truthy!
// -> Final snapshot: {x:1, y:2}

console.log("Setting x = 0");
x = 0;
// -> Always: got change from [x]
// -> x changed from 1 to 0
// -> Final snapshot: {x:0, y:2}
