// ECEngine For...in and For...of Loops Demo
// This example demonstrates the new for...in and for...of loop constructs

console.log("=== ECEngine For...in and For...of Loops Demo ===");

// 1. For...in Loop with Objects
console.log("\n1. For...in Loop with Objects:");
var person = {
    name: "Alice",
    age: 30,
    city: "New York",
    profession: "Engineer"
};

console.log("Object properties:");
for (key in person) {
    console.log("  " + key + ": " + person[key]);
}

// 2. For...of Loop with Arrays
console.log("\n2. For...of Loop with Arrays:");
var fruits = ["apple", "banana", "orange", "grape"];

console.log("Fruits list:");
for (fruit of fruits) {
    console.log("  - " + fruit);
}

// 3. For...in Loop with Arrays (iterates over indices)
console.log("\n3. For...in Loop with Arrays (indices):");
var numbers = [10, 20, 30, 40, 50];

console.log("Array indices:");
for (index in numbers) {
    console.log("  Index " + index + ": " + numbers[index]);
}

// 4. For...of Loop with Strings
console.log("\n4. For...of Loop with Strings:");
var message = "Hello";

console.log("Characters in '" + message + "':");
for (char of message) {
    console.log("  '" + char + "'");
}

// 5. For...in Loop with Strings (iterates over indices)
console.log("\n5. For...in Loop with Strings (indices):");
var word = "ECEngine";

console.log("Character positions in '" + word + "':");
for (i in word) {
    console.log("  Position " + i + ": '" + word[i] + "'");
}

// 6. Nested For...in and For...of Loops
console.log("\n6. Nested For...in and For...of Loops:");
var library = {
    fiction: ["1984", "Brave New World", "Fahrenheit 451"],
    science: ["Cosmos", "Brief History of Time", "Origin of Species"],
    philosophy: ["Republic", "Meditations", "Being and Time"]
};

console.log("Library catalog:");
for (category in library) {
    console.log("  " + category.toUpperCase() + ":");
    for (book of library[category]) {
        console.log("    - " + book);
    }
}

// 7. Using Break and Continue
console.log("\n7. Using Break and Continue:");
var scores = [85, 92, 78, 95, 88, 76, 99];

console.log("Processing scores (skip below 80, stop at perfect score):");
for (score of scores) {
    if (score < 80) {
        console.log("  Skipping low score: " + score);
        continue;
    }
    if (score >= 99) {
        console.log("  Found perfect score: " + score + " - stopping!");
        break;
    }
    console.log("  Good score: " + score);
}

// 8. Variable Declarations in Loops
console.log("\n8. Variable Declarations in Loops:");

// Using var
var colors = ["red", "green", "blue"];
for (var color of colors) {
    console.log("  VAR: " + color);
}

// Using let (when supported)
var shapes = ["circle", "square", "triangle"];
for (let shape of shapes) {
    console.log("  LET: " + shape);
}

// Using const (when supported) 
var sizes = ["small", "medium", "large"];
for (const size of sizes) {
    console.log("  CONST: " + size);
}

// 9. Working with Complex Objects
console.log("\n9. Working with Complex Objects:");
var company = {
    name: "Tech Corp",
    employees: [
        {name: "John", department: "Engineering"},
        {name: "Jane", department: "Marketing"},
        {name: "Bob", department: "Sales"}
    ],
    founded: 2010
};

console.log("Company information:");
for (property in company) {
    if (property === "employees") {
        console.log("  " + property + ":");
        for (employee of company[property]) {
            console.log("    - " + employee.name + " (" + employee.department + ")");
        }
    } else {
        console.log("  " + property + ": " + company[property]);
    }
}

// 10. Accumulating Values
console.log("\n10. Accumulating Values:");
var data = {
    sales: [100, 150, 200, 175],
    costs: [50, 75, 100, 80],
    profits: [50, 75, 100, 95]
};

var totals = {};
for (category in data) {
    var sum = 0;
    for (value of data[category]) {
        sum += value;
    }
    totals[category] = sum;
    console.log("  Total " + category + ": " + sum);
}

console.log("\n=== Demo Complete ===");
console.log("For...in and For...of loops provide powerful iteration capabilities!");
