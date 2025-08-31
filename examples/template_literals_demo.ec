// ECEngine Template Literals Demo
// Demonstrates the new template literal feature with string interpolation

console.log("=== ECEngine Template Literals Demo ===");
console.log("");

// 1. Basic template literals
console.log("1. Basic Template Literals:");
var basicTemplate = `Hello, World!`;
console.log("Basic template:", basicTemplate);

var multiLineTemplate = `This is a
multi-line
template literal`;
console.log("Multi-line template:", multiLineTemplate);
console.log("");

// 2. Variable interpolation
console.log("2. Variable Interpolation:");
var name = "Alice";
var age = 25;
var greeting = `Hello, my name is ${name} and I am ${age} years old.`;
console.log("Greeting:", greeting);

var firstName = "John";
var lastName = "Smith";
var fullName = `${firstName} ${lastName}`;
console.log("Full name:", fullName);
console.log("");

// 3. Expression interpolation
console.log("3. Expression Interpolation:");
var x = 10;
var y = 5;
var mathResult = `The sum of ${x} and ${y} is ${x + y}`;
console.log("Math result:", mathResult);

var price = 19.99;
var quantity = 3;
var totalCost = `Total cost: $${price * quantity}`;
console.log("Shopping:", totalCost);
console.log("");

// 4. Boolean and null interpolation
console.log("4. Boolean and Null Interpolation:");
var isActive = true;
var statusMessage = `User status: ${isActive ? "Active" : "Inactive"}`;
console.log("Status:", statusMessage);

var value = null;
var nullCheck = `Value is: ${value}`;
console.log("Null check:", nullCheck);
console.log("");

// 5. Comparison expressions
console.log("5. Comparison Expressions:");
var score = 85;
var passed = `Did you pass? ${score >= 60}`;
console.log("Pass/Fail:", passed);

var temperature = 22;
var weather = `It's ${temperature > 25 ? "hot" : "pleasant"} today (${temperature}°C)`;
console.log("Weather:", weather);
console.log("");

// 6. Function call interpolation
console.log("6. Function Call Interpolation:");
function getCurrentTime() {
    return "2024-01-15 10:30:00";
}

function formatCurrency(amount) {
    return "$" + amount;
}

var timeMessage = `Current time: ${getCurrentTime()}`;
console.log("Time:", timeMessage);

var budget = 1000;
var budgetMessage = `Your budget is ${formatCurrency(budget)}`;
console.log("Budget:", budgetMessage);
console.log("");

// 7. Nested template literals
console.log("7. Nested Template Literals:");
var user = "Bob";
var action = "logged in";
var timestamp = "10:30 AM";
var innerTemplate = `${user} ${action}`;
var outerTemplate = `[${timestamp}] ${innerTemplate}`;
console.log("Log entry:", outerTemplate);
console.log("");

// 8. Template literals with escape sequences
console.log("8. Escape Sequences:");
var escapedTemplate = `Line 1\nLine 2\tTabbed content`;
console.log("Escaped template:", escapedTemplate);

var quotedTemplate = `He said: "Hello, World!"`;
console.log("Quoted template:", quotedTemplate);
console.log("");

// 9. Dynamic content generation
console.log("9. Dynamic Content Generation:");
var items = ["apple", "banana", "orange"];
var itemCount = 3;
var inventory = `We have ${itemCount} items: ${items[0]}, ${items[1]}, and ${items[2]}`;
console.log("Inventory:", inventory);

// 10. Template literals in function returns
console.log("10. Template Literals in Functions:");
function createGreeting(name, timeOfDay) {
    return `Good ${timeOfDay}, ${name}! Welcome to ECEngine.`;
}

function generateReport(sales, target) {
    var percentage = (sales / target) * 100;
    return `Sales Report: ${sales}/${target} (${percentage}% of target)`;
}

console.log("Greeting function:", createGreeting("Charlie", "morning"));
console.log("Report function:", generateReport(850, 1000));
console.log("");

console.log("=== Template Literals Demo Complete ===");
console.log("Template literals provide a powerful and readable way to create strings with embedded expressions!");
