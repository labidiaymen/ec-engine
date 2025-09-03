// Comprehensive Example: Using ECEngine's new 'is', 'and', 'or' operators
// This demonstrates practical use cases for the new Python-like operators

console.log("=== ECEngine New Operators Demo ===\n");

// === User Authentication Example ===
console.log("1. User Authentication System:");

function authenticateUser(username, password, isActive) {
    // Using 'is' for equality checks (more readable than ==)
    if (username is 'admin' and password is 'secret123' and isActive) {
        return 'Admin authenticated successfully';
    }
    
    // Using 'or' for multiple conditions
    if (username is 'guest' or password is 'demo') {
        return 'Guest access granted';
    }
    
    return 'Authentication failed';
}

console.log(authenticateUser('admin', 'secret123', true));
console.log(authenticateUser('guest', 'wrongpass', false));
console.log(authenticateUser('user', 'demo', true));
console.log(authenticateUser('hacker', 'exploit', false));
console.log("");

// === Data Validation Example ===
console.log("2. Form Validation:");

function validateForm(name, email, age, terms) {
    var errors = [];
    
    // Check if name is valid (not empty and not null)
    if (name is '' or name is null) {
        errors.push('Name is required');
    }
    
    // Check email format (simplified)
    if (email is '' or !email) {
        errors.push('Email is required');
    }
    
    // Age validation using 'and' for range checking
    if (age < 18 and age is age) { // age is age checks if it's a number
        errors.push('Must be 18 or older');
    }
    
    // Terms acceptance
    if (terms is false or terms is null) {
        errors.push('Must accept terms and conditions');
    }
    
    return errors.length is 0 ? 'Form is valid!' : 'Errors: ' + errors.join(', ');
}

console.log(validateForm('John Doe', 'john@example.com', 25, true));
console.log(validateForm('', 'invalid-email', 16, false));
console.log("");

// === Game Logic Example ===
console.log("3. Simple Game Logic:");

function checkWinCondition(playerScore, enemyScore, timeLeft, hasBonus) {
    // Win conditions using natural language operators
    if (playerScore > enemyScore and timeLeft > 0) {
        if (hasBonus or playerScore > 100) {
            return 'VICTORY with bonus!';
        }
        return 'VICTORY!';
    }
    
    // Draw condition
    if (playerScore is enemyScore and timeLeft is 0) {
        return 'DRAW!';
    }
    
    // Loss condition
    if (playerScore < enemyScore or timeLeft is 0) {
        return 'DEFEAT!';
    }
    
    return 'Game continues...';
}

console.log(checkWinCondition(150, 100, 30, true));
console.log(checkWinCondition(80, 80, 0, false));
console.log(checkWinCondition(50, 100, 0, false));
console.log("");

// === Configuration Checker Example ===
console.log("4. System Configuration:");

function checkSystemConfig(env, debugMode, database, cacheEnabled) {
    console.log('Environment Check:');
    
    // Development environment checks
    if (env is 'development' and debugMode is true) {
        console.log('✓ Development mode with debugging enabled');
    }
    
    // Production safety checks
    if (env is 'production' and debugMode is true) {
        console.log('⚠ WARNING: Debug mode enabled in production!');
    }
    
    // Database configuration
    if (database is 'mongodb' or database is 'postgresql') {
        console.log('✓ Supported database detected: ' + database);
    }
    
    // Performance optimization
    if (cacheEnabled is true and env is 'production') {
        console.log('✓ Cache optimization enabled for production');
    }
    
    // Invalid configuration
    if (env is null or database is null) {
        console.log('✗ Invalid configuration detected');
    }
}

checkSystemConfig('development', true, 'mongodb', false);
console.log('---');
checkSystemConfig('production', true, 'postgresql', true);
console.log('---');
checkSystemConfig(null, false, null, false);
console.log("");

// === API Response Handler Example ===
console.log("5. API Response Handling:");

function handleApiResponse(status, data, errorCode) {
    // Success cases using 'or' for multiple success codes
    if (status is 200 or status is 201 or status is 204) {
        if (data and data.length > 0) {
            return 'Success: Data received (' + data.length + ' items)';
        }
        return 'Success: Operation completed';
    }
    
    // Client errors
    if (status is 400 or status is 401 or status is 403) {
        return 'Client Error: ' + (errorCode or 'Unknown error');
    }
    
    // Server errors
    if (status >= 500 and status < 600) {
        return 'Server Error: Please try again later';
    }
    
    return 'Unknown status: ' + status;
}

console.log(handleApiResponse(200, ['item1', 'item2', 'item3'], null));
console.log(handleApiResponse(201, [], null));
console.log(handleApiResponse(401, null, 'Unauthorized'));
console.log(handleApiResponse(500, null, 'Internal Server Error'));
console.log("");

// === Complex Boolean Logic Example ===
console.log("6. Complex Conditional Logic:");

function processOrder(user, product, quantity, inStock, hasDiscount) {
    // Complex conditions that read like natural language
    if (user and user.isVip is true and hasDiscount is true) {
        console.log('VIP customer with discount detected');
        if (inStock >= quantity or quantity is 1) {
            return 'Order processed with VIP priority and discount';
        }
    }
    
    // Regular customer logic
    if (user and product and quantity > 0) {
        if (inStock >= quantity and product.price > 0) {
            return 'Order processed successfully';
        }
        if (inStock < quantity and inStock > 0) {
            return 'Partial order processed (' + inStock + ' items)';
        }
    }
    
    // Error cases
    if (user is null or product is null) {
        return 'Error: Missing user or product information';
    }
    
    if (quantity <= 0 or inStock is 0) {
        return 'Error: Invalid quantity or out of stock';
    }
    
    return 'Unable to process order';
}

var vipUser = { isVip: true, name: 'Alice' };
var regularUser = { isVip: false, name: 'Bob' };
var product = { name: 'Laptop', price: 999 };

console.log(processOrder(vipUser, product, 2, 5, true));
console.log(processOrder(regularUser, product, 3, 2, false));
console.log(processOrder(null, product, 1, 5, false));
console.log("");

console.log("=== Demo Complete ===");
console.log("The new 'is', 'and', 'or' operators make code more readable!");
console.log("• 'is' replaces '==' for equality checks");
console.log("• 'and' replaces '&&' for logical AND");
console.log("• 'or' replaces '||' for logical OR");
console.log("These operators work exactly like their traditional counterparts but with Python-like syntax.");
