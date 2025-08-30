// ECEngine String Quotes Demo
// Demonstrates single quotes, double quotes, and mixed quote usage

console.log("=== ECEngine String Quotes Demo ===");
console.log("");

// 1. Double quotes (traditional)
console.log("1. Double quotes:");
var doubleQuoteString = "Hello, this is a double-quoted string!";
console.log("Double quotes:", doubleQuoteString);
console.log("");

// 2. Single quotes (new feature)
console.log("2. Single quotes:");
var singleQuoteString = 'Hello, this is a single-quoted string!';
console.log("Single quotes:", singleQuoteString);
console.log("");

// 3. Mixed quotes - single quotes containing double quotes
console.log("3. Single quotes containing double quotes:");
var mixedQuotes1 = 'He said, "Hello world!" to everyone.';
console.log("Mixed 1:", mixedQuotes1);
console.log("");

// 4. Mixed quotes - double quotes containing single quotes
console.log("4. Double quotes containing single quotes:");
var mixedQuotes2 = "It's a beautiful day, isn't it?";
console.log("Mixed 2:", mixedQuotes2);
console.log("");

// 5. Practical examples
console.log("5. Practical examples:");

// HTML attributes in strings
var htmlWithAttributes = 'Click <a href="https://example.com" class="link">here</a> to continue.';
console.log("HTML with attributes:", htmlWithAttributes);

// SQL queries
var sqlQuery = "SELECT * FROM users WHERE name = 'John Doe' AND active = true;";
console.log("SQL query:", sqlQuery);

// JavaScript code in strings
var jsCode = 'const message = "Hello World"; console.log(message);';
console.log("JavaScript code:", jsCode);

// JSON strings with mixed quotes
var jsonWithQuotes = '{"message": "User\'s profile updated", "status": "success"}';
console.log("JSON with quotes:", jsonWithQuotes);
console.log("");

// 6. Quotes in object properties
console.log("6. Quotes in object properties:");
var quotesInObject = {
    "singleQuoted": 'This uses single quotes',
    "doubleQuoted": "This uses double quotes",
    "mixedContent": 'Content with "double quotes" inside',
    "htmlSnippet": '<div class="container">Content here</div>',
    "dialog": "She said, 'I'll be there soon.'"
};
console.log("Object with mixed quotes:", quotesInObject);
console.log("");

// 7. Configuration examples
console.log("7. Configuration file example:");
var webConfig = {
    "siteName": "My Awesome Website",
    "tagline": "Where dreams come true!",
    "footer": "Copyright © 2024 My Company. All rights reserved.",
    "socialLinks": {
        "twitter": "Follow us @mycompany for updates!",
        "facebook": "Like our page 'My Company Official'",
        "instagram": 'Check out our photos @my_company_official'
    },
    "errorMessages": {
        "notFound": "Sorry, the page you're looking for doesn't exist.",
        "serverError": "Oops! Something went wrong. Please try again later.",
        "validation": 'Please check your input and try again.'
    }
};

console.log("Web config:", webConfig);

var configJson = JSON.stringify(webConfig);
console.log("Config as JSON:", configJson);
console.log("");

// 8. Template strings simulation
console.log("8. Template-like strings:");
var userName = "Alice";
var greeting = 'Welcome back, ' + userName + '! Hope you\'re having a great day.';
console.log("Greeting:", greeting);

var notification = "You have 3 new messages in your 'Inbox' folder.";
console.log("Notification:", notification);

console.log("");
console.log("=== String Quotes Demo Complete ===");
