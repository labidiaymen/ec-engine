// Switch statement examples in ECEngine

var day = 3;
var dayName;

// Basic switch statement
switch (day) {
    case 1:
        dayName = "Monday";
        break;
    case 2:
        dayName = "Tuesday";
        break;
    case 3:
        dayName = "Wednesday";
        break;
    case 4:
        dayName = "Thursday";
        break;
    case 5:
        dayName = "Friday";
        break;
    default:
        dayName = "Weekend";
        break;
}

console.log("Day " + day + " is " + dayName);

// Switch with fall-through behavior
var grade = "B";
var message;

switch (grade) {
    case "A":
    case "B":
        message = "Excellent work!";
        break;
    case "C":
        message = "Good job!";
        break;
    case "D":
        message = "You can do better";
        break;
    case "F":
        message = "Please study more";
        break;
    default:
        message = "Invalid grade";
        break;
}

console.log("Grade " + grade + ": " + message);

// Switch with multiple operations per case
var operation = "+";
var a = 10;
var b = 5;
var result;

switch (operation) {
    case "+":
        result = a + b;
        console.log("Addition: " + a + " + " + b + " = " + result);
        break;
    case "-":
        result = a - b;
        console.log("Subtraction: " + a + " - " + b + " = " + result);
        break;
    case "*":
        result = a * b;
        console.log("Multiplication: " + a + " * " + b + " = " + result);
        break;
    case "/":
        if (b != 0) {
            result = a / b;
            console.log("Division: " + a + " / " + b + " = " + result);
        } else {
            console.log("Error: Division by zero");
        }
        break;
    default:
        console.log("Unknown operation: " + operation);
        break;
}
