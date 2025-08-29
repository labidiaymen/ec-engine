// ECEngine - Comparison Operators and If Statements Examples

var x = 10;
var y = 5;
var name = "Alice";
var age = 25;

console.log("=== Basic Comparisons with String Concatenation ===");

if (x > y) {
    console.log("x (" + x + ") is greater than y (" + y + ")");
}

if (y < x) {
    console.log("y (" + y + ") is less than x (" + x + ")");
}

if (x >= 10) {
    console.log("x is greater than or equal to 10");
}

if (y <= 5) {
    console.log("y is less than or equal to 5");
}

console.log("=== Equality Tests ===");

if (x == 10) {
    console.log("x equals 10");
}

if (y != x) {
    console.log("y is not equal to x");
}

console.log("=== Boolean Logic ===");

var isAdult = true;
var hasLicense = false;

if (isAdult == true) {
    console.log("Person is an adult");
    
    if (hasLicense == false) {
        console.log("But they don't have a license");
    } else {
        console.log("And they have a license");
    }
}

console.log("=== String Comparisons ===");

var name1 = "Alice";
var name2 = "Bob";

if (name1 < name2) {
    console.log(name1 + " comes before " + name2 + " alphabetically");
}

console.log("=== Grade Calculator ===");

var score = 85;
console.log("Score: " + score);

if (score >= 90) {
    console.log("Grade: A - Excellent!");
} else if (score >= 80) {
    console.log("Grade: B - Good job!");
} else if (score >= 70) {
    console.log("Grade: C - Acceptable");
} else if (score >= 60) {
    console.log("Grade: D - Needs improvement");
} else {
    console.log("Grade: F - Failed");
}

console.log("=== Complex Nested Conditions ===");

var income = 50000;

if (age >= 18) {
    console.log("Age requirement met (" + age + " years old)");
    
    if (income >= 30000) {
        console.log("Income requirement met ($" + income + ")");
        console.log("Loan approved!");
    } else {
        console.log("Income too low ($" + income + " < $30000)");
        console.log("Loan denied");
    }
} else {
    console.log("Too young (" + age + " < 18)");
    console.log("Loan denied");
}
