// Console Debugging
// Using console.log for debugging and tracing

function calculateArea(width, height) {
    console.log("calculateArea called with:", width, height);
    
    var area = width * height;
    console.log("Calculated area:", area);
    
    return area;
}

function processRectangle(w, h) {
    console.log("Processing rectangle...");
    
    var area = calculateArea(w, h);
    var perimeter = 2 * (w + h);
    
    console.log("Perimeter:", perimeter);
    console.log("Processing complete");
    
    return area;
}

// Test the functions with debug output
console.log("Starting rectangle calculations");

var result1 = processRectangle(5, 3);
console.log("Final result 1:", result1);

var result2 = processRectangle(10, 7);
console.log("Final result 2:", result2);

console.log("All calculations done");
