// Anonymous Functions
// Functions without names, assigned to variables

var add = function(a, b) {
    return a + b;
};

var square = function(x) {
    return x * x;
};

var getRandomNumber = function() {
    return 42; // "Random" number for demo
};

// Using anonymous functions
var result1 = add(10, 5);
console.log("10 + 5 =", result1);

var result2 = square(6);
console.log("6 squared =", result2);

var random = getRandomNumber();
console.log("Random number:", random);
