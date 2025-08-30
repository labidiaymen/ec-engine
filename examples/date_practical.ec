// Practical Date usage examples
console.log("=== Practical Date Examples ===");

// Get current timestamp for performance timing
var startTime = Date.now();
console.log("Operation started at:", startTime);

// Create a specific date
var launchDate = Date(2023, 11, 25, 10, 30, 0); // Christmas 2023, 10:30 AM
console.log("Launch date:", launchDate.toDateString());
console.log("Launch time:", launchDate.toTimeString());

// Calculate time difference
var currentTime = Date.now();
var elapsed = currentTime - startTime;
console.log("Elapsed time:", elapsed, "milliseconds");

// Working with dates
var birthday = Date(1990, 4, 15); // May 15, 1990 (month is 0-based)
console.log("Birthday:", birthday.toLocaleDateString());
console.log("Birth year:", birthday.getFullYear());
console.log("Birth month:", birthday.getMonth() + 1); // Add 1 for human-readable month

// ISO date string (useful for APIs)
var apiDate = Date();
console.log("API timestamp:", apiDate.toISOString());

// Parse date strings
var parsedTimestamp = Date.parse("2024-01-01T00:00:00Z");
console.log("Parsed New Year 2024:", parsedTimestamp);

console.log("=== Date Examples Complete ===");
