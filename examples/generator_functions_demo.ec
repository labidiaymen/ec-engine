// Generator Functions - Comprehensive Demo
// Demonstrates ECEngine's generator function capabilities

console.log("🚀 ECEngine Generator Functions Demo\n");

// =============================================================================
// 1. BASIC GENERATOR FUNCTIONS
// =============================================================================

console.log("=== 1. Basic Generator Functions ===\n");

// Simple generator that yields multiple values
function* simpleNumbers() {
    yield 1;
    yield 2;
    yield 3;
    return "finished";
}

var gen1 = simpleNumbers();
console.log("Generator created:", gen1);
console.log("First .next():", gen1.next());   // { value: 1, done: false }
console.log("Second .next():", gen1.next());  // { value: 2, done: false }
console.log("Third .next():", gen1.next());   // { value: 3, done: false }
console.log("Fourth .next():", gen1.next());  // { value: "finished", done: true }
console.log("Fifth .next():", gen1.next());   // { value: null, done: true }

// =============================================================================
// 2. GENERATOR FUNCTIONS WITH PARAMETERS
// =============================================================================

console.log("\n=== 2. Generator Functions with Parameters ===\n");

function* countdown(start) {
    while (start > 0) {
        yield start;
        start = start - 1;
    }
    return "Countdown complete!";
}

var counter = countdown(5);
console.log("Countdown from 5:");
var result1 = counter.next();
console.log("→", result1.value); // 5
var result2 = counter.next();
console.log("→", result2.value); // 4
var result3 = counter.next();
console.log("→", result3.value); // 3
var result4 = counter.next();
console.log("→", result4.value); // 2
var result5 = counter.next();
console.log("→", result5.value); // 1
var final = counter.next();
console.log("→", final.value, "(done:", final.done + ")");

// =============================================================================
// 3. GENERATOR FUNCTIONS WITH COMPLEX LOGIC
// =============================================================================

console.log("\n=== 3. Complex Generator Logic ===\n");

function* fibonacci(max) {
    var a = 0;
    var b = 1;
    
    while (a <= max) {
        yield a;
        var temp = a + b;
        a = b;
        b = temp;
    }
}

var fib = fibonacci(20);
console.log("Fibonacci sequence up to 20:");
var fibResult = fib.next();
while (!fibResult.done) {
    console.log("→", fibResult.value);
    fibResult = fib.next();
}

// =============================================================================
// 4. GENERATOR FUNCTIONS AS OBJECT METHODS
// =============================================================================

console.log("\n=== 4. Generator Functions as Object Methods ===\n");

var numberSequence = {
    start: 10,
    end: 13,
    
    range: function*() {
        var current = this.start;
        while (current <= this.end) {
            yield current;
            current = current + 1;
        }
    }
};

var rangeGen = numberSequence.range();
console.log("Range from", numberSequence.start, "to", numberSequence.end + ":");
var rangeResult = rangeGen.next();
while (!rangeResult.done) {
    console.log("→", rangeResult.value);
    rangeResult = rangeGen.next();
}

// =============================================================================
// 5. GENERATOR FUNCTION EXPRESSIONS
// =============================================================================

console.log("\n=== 5. Generator Function Expressions ===\n");

var evenNumbers = function*(limit) {
    var current = 0;
    while (current <= limit) {
        // Check if current is even by using division and multiplication
        var half = current / 2;
        var doubled = Math.floor(half) * 2;
        if (current === doubled) {
            yield current;
        }
        current = current + 1;
    }
};

var evens = evenNumbers(10);
console.log("Even numbers up to 10:");
var evenResult = evens.next();
while (!evenResult.done) {
    console.log("→", evenResult.value);
    evenResult = evens.next();
}

// =============================================================================
// 6. GENERATOR FUNCTIONS WITH EARLY RETURN
// =============================================================================

console.log("\n=== 6. Generator Functions with Early Return ===\n");

function* conditionalGenerator(condition) {
    yield "start";
    
    if (condition) {
        yield "condition was true";
        return "early exit";
    }
    
    yield "condition was false";
    yield "continuing";
    return "normal exit";
}

console.log("With condition = true:");
var condGen1 = conditionalGenerator(true);
var condResult1 = condGen1.next();
while (!condResult1.done) {
    console.log("→", condResult1.value);
    condResult1 = condGen1.next();
}
console.log("Final return:", condResult1.value);

console.log("\nWith condition = false:");
var condGen2 = conditionalGenerator(false);
var condResult2 = condGen2.next();
while (!condResult2.done) {
    console.log("→", condResult2.value);
    condResult2 = condGen2.next();
}
console.log("Final return:", condResult2.value);

// =============================================================================
// 7. GENERATOR FUNCTIONS WITH NESTED CALLS
// =============================================================================

console.log("\n=== 7. Nested Generator Usage ===\n");

function* letters() {
    yield "A";
    yield "B";
    yield "C";
}

function* numbersAndLetters() {
    yield 1;
    yield 2;
    
    // Manual delegation (since yield* is not implemented yet)
    var letterGen = letters();
    var letterResult = letterGen.next();
    while (!letterResult.done) {
        yield letterResult.value;
        letterResult = letterGen.next();
    }
    
    yield 3;
}

var mixed = numbersAndLetters();
console.log("Numbers and letters combined:");
var mixedResult = mixed.next();
while (!mixedResult.done) {
    console.log("→", mixedResult.value);
    mixedResult = mixed.next();
}

// =============================================================================
// 8. PRACTICAL EXAMPLE: ID GENERATOR
// =============================================================================

console.log("\n=== 8. Practical Example: ID Generator ===\n");

function* idGenerator(prefix) {
    var idCounter = 1;
    while (true) {
        yield prefix + idCounter;
        idCounter = idCounter + 1;
        if (idCounter > 5) { // Limit for demo
            return "ID generator exhausted";
        }
    }
}

var userIds = idGenerator("USER_");
console.log("Generated user IDs:");
for (var i = 0; i < 6; i++) {
    var idResult = userIds.next();
    if (idResult.done) {
        console.log("Generator finished:", idResult.value);
        break;
    }
    console.log("→", idResult.value);
}

console.log("\n✅ Generator Functions Demo Complete!");
console.log("\n📋 Key Features Demonstrated:");
console.log("• Basic generator function syntax: function*() {}");
console.log("• Yield statements for pausing execution");
console.log("• Generator.next() method for resuming");
console.log("• Return statements to end generators");
console.log("• Parameters and complex logic in generators");
console.log("• Generator functions as object methods");
console.log("• Generator function expressions");
console.log("• Conditional flow and early returns");
console.log("• Practical use cases and patterns");
