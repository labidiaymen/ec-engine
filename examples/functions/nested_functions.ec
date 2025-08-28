// Nested Functions and Closures
// Functions defined inside other functions

function outer(x) {
    var outerVar = x + 10;
    
    function inner(y) {
        return outerVar + y; // Accessing outer scope variable
    }
    
    return inner;
}

function calculator() {
    var memory = 0;
    
    return function(operation, value) {
        if (operation == "add") {
            memory = memory + value;
        }
        if (operation == "multiply") {
            memory = memory * value;
        }
        return memory;
    };
}

// Using nested functions
var closure = outer(5);
var result = closure(3);
console.log("Closure result:", result); // Should be 18 (5 + 10 + 3)

// Using calculator with closure
var calc = calculator();
console.log("Add 10:", calc("add", 10));
console.log("Add 5:", calc("add", 5));
console.log("Multiply by 2:", calc("multiply", 2));
