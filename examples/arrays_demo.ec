// Array feature demonstration
// This file demonstrates ECEngine's array support

// Basic array creation and access
console.log("=== Array Creation and Access ===");
let numbers = [1, 2, 3, 4, 5];
console.log("Array:", numbers);
console.log("Length:", numbers.length);
console.log("First element:", numbers[0]);
console.log("Last element:", numbers[numbers.length - 1]);

// Mixed type arrays
console.log("\n=== Mixed Type Arrays ===");
let mixed = [42, "hello", true, null];
console.log("Mixed array:", mixed);
for (let i = 0; i < mixed.length; i++) {
    console.log("Element", i + ":", mixed[i]);
}

// Array methods
console.log("\n=== Array Methods ===");
let fruits = ["apple", "banana"];
console.log("Initial fruits:", fruits);

// Push method
let newLength = fruits.push("cherry", "date");
console.log("After push:", fruits);
console.log("New length:", newLength);

// Pop method
let removed = fruits.pop();
console.log("After pop:", fruits);
console.log("Removed element:", removed);

// Slice method
let citrus = ["orange", "lemon", "lime", "grapefruit"];
let sliced = citrus.slice(1, 3);
console.log("Original citrus:", citrus);
console.log("Sliced (1, 3):", sliced);

// Join method
let words = ["ECEngine", "supports", "arrays"];
let sentence = words.join(" ");
console.log("Words:", words);
console.log("Joined sentence:", sentence);

// IndexOf method
let colors = ["red", "green", "blue", "green"];
console.log("Colors:", colors);
console.log("Index of 'green':", colors.indexOf("green"));
console.log("Index of 'yellow':", colors.indexOf("yellow"));

// Nested arrays
console.log("\n=== Nested Arrays ===");
let matrix = [[1, 2, 3], [4, 5, 6], [7, 8, 9]];
console.log("Matrix:", matrix);
console.log("Element at [1][2]:", matrix[1][2]);

// Dynamic access
console.log("\n=== Dynamic Access ===");
let data = [10, 20, 30, 40, 50];
let index = 2;
console.log("Data:", data);
console.log("Element at dynamic index", index + ":", data[index]);
console.log("Element at expression index:", data[index + 1]);

// Empty arrays
console.log("\n=== Empty Arrays ===");
let empty = [];
console.log("Empty array:", empty);
console.log("Empty length:", empty.length);
empty.push("first");
console.log("After adding element:", empty);

console.log("\n=== Array Demo Complete ===");
