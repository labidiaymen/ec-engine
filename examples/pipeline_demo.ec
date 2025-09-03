// Pipeline Operator Demo - Function Chaining with |>

// Basic math functions
function double(x) {
    return x * 2;
}

function add10(x) {
    return x + 10;
}

function square(x) {
    return x * x;
}

// String processing functions
function toUpperCase(str) {
    return str.toUpperCase();
}

function addExclamation(str) {
    return str + "!";
}

function reverseString(str) {
    var result = "";
    for (var i = str.length - 1; i >= 0; i--) {
        result += str[i];
    }
    return result;
}

// Basic pipeline chaining
console.log("=== Basic Pipeline Examples ===");

var result1 = 5 |> double |> add10;
console.log("5 |> double |> add10 =", result1); // Should be 20

var result2 = 3 |> square |> double;
console.log("3 |> square |> double =", result2); // Should be 18

// String processing pipeline
var text = "hello world";
var processed = text |> toUpperCase |> addExclamation;
console.log("Pipeline result:", processed); // Should be "HELLO WORLD!"

// Complex pipeline
var number = 2;
var final = number |> double |> square |> add10;
console.log("2 |> double |> square |> add10 =", final); // Should be 26

// Multiple pipelines
console.log("\n=== Multiple Pipeline Examples ===");

var a = 1 |> double;      // 2
var b = a |> add10;       // 12
var c = b |> square;      // 144

console.log("Step by step: a =", a, "b =", b, "c =", c);

// Compare with traditional nested calls
console.log("\n=== Comparison with Traditional Syntax ===");

// Traditional nested approach
var traditional = add10(square(double(5)));
console.log("Traditional: add10(square(double(5))) =", traditional);

// Pipeline approach
var pipeline = 5 |> double |> square |> add10;
console.log("Pipeline:    5 |> double |> square |> add10 =", pipeline);

console.log("Both results equal:", traditional === pipeline);

// Multiline pipeline examples
console.log("\n=== Multiline Pipeline Examples ===");

// Example 1: Complex data transformation across multiple lines
var data = "hello world"
    |> toUpperCase
    |> addExclamation
    |> reverseString;

console.log("Multiline string transformation:", data);

// Example 2: Mathematical computation with better readability
var complexCalculation = 5
    |> double      // 10
    |> add10       // 20
    |> square      // 400
    |> double;     // 800

console.log("Complex calculation result:", complexCalculation);

// Functions with multiple parameters
function addNumbers(x, y) {
    return x + y;
}

function multiply(x, factor) {
    return x * factor;
}

function power(base, exponent) {
    return Math.pow(base, exponent);
}

function substring(str, start, length) {
    return str.substring(start, start + length);
}

function padString(str, length, char) {
    while (str.length < length) {
        str = char + str;
    }
    return str;
}

function filterLength(str, minLength) {
    return str.length >= minLength ? str : "";
}

function truncate(str, maxLength) {
    return str.length > maxLength ? str.substring(0, maxLength) + "..." : str;
}

function capitalizeFirst(str) {
    return str.charAt(0).toUpperCase() + str.slice(1);
}

// Pipeline with parameters examples
console.log("\n=== Pipeline with Parameters Examples ===");

// Direct function calls with parameters - the modern way!
var advancedResult = "javascript programming"
    |> filterLength(5)      // filterLength(value, 5) - Keep only if >= 5 chars
    |> capitalizeFirst()    // capitalizeFirst(value) - Capitalize first letter  
    |> truncate(10);        // truncate(value, 10) - Truncate to 10 chars

console.log("Advanced pipeline result:", advancedResult);

// Math operations with parameters
var mathResult = 5
    |> addNumbers(3)        // addNumbers(5, 3) = 8
    |> multiply(4)          // multiply(8, 4) = 32
    |> power(2);            // power(32, 2) = 1024

console.log("Math pipeline with parameters:", mathResult);

// String manipulation with multiple parameters
var stringResult = "hello"
    |> padString(10, "0")   // padString("hello", 10, "0") = "00000hello"
    |> substring(2, 5);     // substring("00000hello", 2, 5) = "000he"

console.log("String manipulation pipeline:", stringResult);

// AI Prompt Chaining Example
console.log("\n=== AI Prompt Chaining Example ===");

// AI processing functions
function analyzeContext(text) {
    return "Context: " + text + " (analyzed)";
}

function enhancePrompt(context) {
    return context + " -> Enhanced with AI insights";
}

function formatForModel(prompt) {
    return "[AI_MODEL]: " + prompt;
}

function processResponse(response) {
    return response.replace("[AI_MODEL]: ", "").toUpperCase();
}

function addMetadata(result) {
    return {
        content: result,
        timestamp: "2025-09-03",
        processed: true
    };
}

// AI prompt processing pipeline
var aiResult = "User wants to learn JavaScript"
    |> analyzeContext       // Analyze the user input context
    |> enhancePrompt        // Enhance with AI insights  
    |> formatForModel       // Format for AI model consumption
    |> processResponse      // Process the AI response
    |> addMetadata;         // Add metadata to result

console.log("AI Pipeline Result:", aiResult);

// Multi-step AI workflow
var complexAI = "Write a function to sort arrays"
    |> analyzeContext
    |> enhancePrompt
    |> formatForModel;

console.log("Complex AI prompt:", complexAI);

// Advanced AI workflow with parameters
function sanitizeInput(text, maxLength) {
    return text.length > maxLength ? text.substring(0, maxLength) + "..." : text;
}

function addSystemPrompt(userInput, role) {
    return "System: You are a " + role + ". User: " + userInput;
}

function tokenize(prompt, maxTokens) {
    var tokens = prompt.split(" ");
    return tokens.length > maxTokens ? tokens.slice(0, maxTokens).join(" ") + " [TRUNCATED]" : prompt;
}

// Real-world AI processing pipeline
var aiWorkflow = "Help me debug this Python code that crashes"
    |> sanitizeInput(50)           // sanitizeInput(text, 50) - Limit input length
    |> addSystemPrompt("coding assistant")  // addSystemPrompt(text, "coding assistant")
    |> tokenize(15);               // tokenize(prompt, 15) - Limit to 15 tokens

console.log("AI Workflow Result:", aiWorkflow);
