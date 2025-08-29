// ECEngine - String Concatenation Examples

console.log("=== String Concatenation Demonstration ===");

// Basic string concatenation
var firstName = "John";
var lastName = "Smith";
var fullName = firstName + " " + lastName;

console.log("Full name: " + fullName);

// String and number concatenation
var age = 30;
var message = "I am " + age + " years old";
console.log(message);

// Multiple concatenation
var greeting = "Hello, " + firstName + "! You are " + age + " years old.";
console.log(greeting);

// Concatenation with boolean values
var isActive = true;
var status = "Status: " + isActive;
console.log(status);

// Concatenation with expressions
var x = 10;
var y = 5;
var result = "The sum of " + x + " and " + y + " is " + (x + y);
console.log(result);

// Building complex strings
console.log("=== Complex String Building ===");

var product = "Laptop";
var price = 999.99;
var quantity = 2;
var total = price * quantity;

var invoice = "Product: " + product + 
              ", Price: $" + price + 
              ", Quantity: " + quantity + 
              ", Total: $" + total;

console.log(invoice);

// Dynamic messages based on conditions
console.log("=== Dynamic Messages ===");

var score = 85;
var grade;
var performance;

if (score >= 90) {
    grade = "A";
    performance = "excellent";
} else if (score >= 80) {
    grade = "B";  
    performance = "good";
} else if (score >= 70) {
    grade = "C";
    performance = "satisfactory";
} else {
    grade = "F";
    performance = "needs improvement";
}

var report = "Student scored " + score + "%, received grade " + grade + 
             " which is " + performance + " performance.";

console.log(report);
