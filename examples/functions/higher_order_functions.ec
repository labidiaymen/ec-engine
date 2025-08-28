// Higher-Order Functions
// Functions that take other functions as parameters

function applyOperation(a, b, operation) {
    return operation(a, b);
}

function createMultiplier(factor) {
    return function(x) {
        return x * factor;
    };
}

// Using higher-order functions
var result1 = applyOperation(10, 5, function(x, y) {
    return x - y;
});
console.log("10 - 5 =", result1);

var result2 = applyOperation(8, 3, function(x, y) {
    return x / y;
});
console.log("8 / 3 =", result2);

// Creating and using a multiplier function
var double = createMultiplier(2);
var triple = createMultiplier(3);

console.log("Double 7:", double(7));
console.log("Triple 5:", triple(5));
