// Test C#-style switch expressions
let value = 2;

// Basic switch expression
let result = value switch {
    1 => "one",
    2 => "two", 
    3 => "three",
    _ => "other"
};

console.log("Result:", result); // Should print "two"

// Switch expression with more complex values
let grade = "B";
let description = grade switch {
    "A" => "Excellent",
    "B" => "Good", 
    "C" => "Average",
    "D" => "Below Average",
    "F" => "Fail",
    _ => "Unknown Grade"
};

console.log("Grade description:", description); // Should print "Good"

// Switch expression with numbers and calculations
let number = 5;
let category = number switch {
    1 => "single",
    2 => "double",
    3 => "triple", 
    _ => number > 10 ? "large" : "small"
};

console.log("Category:", category); // Should print "small"
