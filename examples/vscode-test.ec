// Test file for VS Code syntax highlighting
// Testing the observe keyword

let x = 5;
let y = 10;

// The observe keyword should be highlighted as a control keyword
observe x function() {
    console.log("x changed to: " + x);
}

observe y function() {
    console.log("y is now: " + y);
}

// Test that other keywords still work
function testFunction() {
    if (x > 0) {
        return x + y;
    } else {
        return 0;
    }
}

// Change variables to trigger observers
x = 15;
y = 20;

console.log("Final result: " + testFunction());
