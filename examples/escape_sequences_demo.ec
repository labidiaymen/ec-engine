// ECEngine Escape Sequences Demo
// Demonstrates various escape sequences in strings

console.log("=== ECEngine Escape Sequences Demo ===");
console.log("");

// 1. Basic escape sequences
console.log("1. Basic escape sequences:");

// Newline
var newlineExample = "Line 1\nLine 2\nLine 3";
console.log("Newline (\\n):");
console.log(newlineExample);
console.log("");

// Tab
var tabExample = "Column1\tColumn2\tColumn3";
console.log("Tab (\\t):");
console.log(tabExample);
console.log("");

// Carriage return
var carriageReturn = "Before CR\rAfter CR";
console.log("Carriage return (\\r):");
console.log(carriageReturn);
console.log("");

// 2. Quote escaping
console.log("2. Quote escaping:");

// Escaped double quotes
var doubleQuoteEscape = "He said, \"Hello world!\" with enthusiasm.";
console.log("Escaped double quotes (\\\"): " + doubleQuoteEscape);

// Escaped single quotes
var singleQuoteEscape = 'It\'s a wonderful day, isn\'t it?';
console.log("Escaped single quotes (\\'): " + singleQuoteEscape);
console.log("");

// 3. Backslash escaping
console.log("3. Backslash escaping:");
var backslashExample = "File path: C:\\Users\\Documents\\file.txt";
console.log("Windows path: " + backslashExample);

var regexExample = "Regex pattern: \\d+\\.\\d+ matches decimal numbers";
console.log("Regex pattern: " + regexExample);
console.log("");

// 4. Practical examples
console.log("4. Practical examples:");

// Configuration file paths
var configPaths = {
    "windows": "C:\\Program Files\\MyApp\\config.json",
    "linux": "/usr/local/bin/myapp/config.json",
    "description": "Configuration files are stored in different locations.\nWindows: Uses backslashes\nLinux: Uses forward slashes"
};
console.log("Config paths:", configPaths);
console.log("");

// CSV data with escaped content
var csvData = "Name,Description,Notes\n\"John Doe\",\"Software Engineer\",\"Specializes in \\\"web development\\\"\"\n\"Jane Smith\",\"Product Manager\",\"Loves user experience\\nand design thinking\"";
console.log("CSV data with escapes:");
console.log(csvData);
console.log("");

// 5. Formatted text output
console.log("5. Formatted text output:");
var report = "SALES REPORT\n" +
             "=============\n" +
             "Product\t\tUnits\tRevenue\n" +
             "--------\t\t-----\t-------\n" +
             "Widget A\t\t150\t$1,500.00\n" +
             "Widget B\t\t200\t$3,200.00\n" +
             "Widget C\t\t75\t$900.00\n" +
             "--------\t\t-----\t-------\n" +
             "TOTAL\t\t\t425\t$5,600.00";

console.log("Formatted report:");
console.log(report);
console.log("");

// 6. JSON strings with escapes
console.log("6. JSON with escape sequences:");
var jsonWithEscapes = {
    "message": "Welcome to our app!\nPlease read the \"Terms of Service\" carefully.",
    "path": "C:\\Users\\Public\\Documents",
    "code": "if (user.name === \"admin\") {\n\tconsole.log(\"Access granted\");\n}",
    "description": "This is a multiline\ndescription with\ttabs and \"quotes\"."
};

console.log("Object with escapes:", jsonWithEscapes);

var escapedJson = JSON.stringify(jsonWithEscapes);
console.log("JSON string:", escapedJson);

var parsedBack = JSON.parse(escapedJson);
console.log("Parsed back:", parsedBack);
console.log("");

// 7. Error messages and logs
console.log("7. Error messages and logs:");
var errorLog = {
    "timestamp": Date.now(),
    "level": "ERROR",
    "message": "Failed to parse configuration file",
    "details": "Syntax error at line 15:\n\tUnexpected token '}' after property \"name\"",
    "file": "C:\\MyApp\\config\\database.json",
    "suggestion": "Check that all strings are properly quoted with \\\" characters"
};

console.log("Error log entry:", errorLog);
console.log("");

// 8. Multi-line strings for documentation
console.log("8. Documentation example:");
var documentation = "API DOCUMENTATION\n" +
                   "==================\n\n" +
                   "Endpoint: /api/users\n" +
                   "Method: POST\n\n" +
                   "Request Body:\n" +
                   "{\n" +
                   "\t\"name\": \"User name\",\n" +
                   "\t\"email\": \"user@example.com\",\n" +
                   "\t\"password\": \"secret123\"\n" +
                   "}\n\n" +
                   "Response:\n" +
                   "{\n" +
                   "\t\"success\": true,\n" +
                   "\t\"message\": \"User created successfully\",\n" +
                   "\t\"id\": 123\n" +
                   "}";

console.log("API Documentation:");
console.log(documentation);

console.log("");
console.log("=== Escape Sequences Demo Complete ===");
