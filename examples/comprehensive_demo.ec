// ECEngine Comprehensive Demo: User Management System
// Demonstrates JSON, null support, single quotes, escape sequences, Date, and Math

console.log("=== ECEngine User Management System Demo ===\n");

// Sample user data with various data types and edge cases
var users = [
    {
        "id": 1,
        "name": "Alice Johnson",
        "email": "alice@example.com",
        "bio": "Software engineer who loves\ncoding and coffee ☕",
        "isActive": true,
        "lastLogin": null,
        "preferences": {
            "theme": "dark",
            "notifications": true,
            "language": "en"
        }
    },
    {
        "id": 2,
        "name": "Bob O'Connor",
        "email": 'bob.oconnor@company.org',
        "bio": 'Marketing specialist with expertise in\n"digital campaigns" and analytics',
        "isActive": false,
        "lastLogin": Date.now() - (7 * 24 * 60 * 60 * 1000), // 7 days ago
        "preferences": {
            "theme": "light",
            "notifications": false,
            "language": "fr"
        }
    },
    {
        "id": 3,
        "name": "Charlie Smith",
        "email": "charlie@startup.io",
        "bio": "Full-stack developer\n\tSkills: JS, Python, Go\n\tLocation: C:\\Users\\Charlie\\Documents",
        "isActive": true,
        "lastLogin": Date.now() - (2 * 60 * 60 * 1000), // 2 hours ago
        "preferences": null
    }
];

console.log("📊 User Database:");
console.log("Total users:", users.length);
console.log("Users data:", users);

// JSON serialization and storage simulation
console.log("\n💾 Saving to JSON storage:");
var jsonData = JSON.stringify(users);
console.log("Serialized data length:", jsonData.length, "characters");
console.log("JSON preview:", jsonData.substring(0, 100) + "...");

// JSON parsing simulation (loading from storage)
console.log("\n📖 Loading from JSON storage:");
var loadedUsers = JSON.parse(jsonData);
console.log("Loaded users:", loadedUsers.length, "records");

// User statistics with Math operations
console.log("\n📈 User Statistics:");
var activeUsers = 0;
var inactiveUsers = 0;
var usersWithPreferences = 0;
var totalBioLength = 0;

// Process each user (demonstrating null handling and string operations)
for (var i = 0; i < loadedUsers.length; i++) {
    var user = loadedUsers[i];
    
    // Count active/inactive users
    if (user.isActive) {
        activeUsers++;
    } else {
        inactiveUsers++;
    }
    
    // Count users with preferences (null check)
    if (user.preferences !== null) {
        usersWithPreferences++;
    }
    
    // Calculate bio statistics
    if (user.bio !== null) {
        totalBioLength += user.bio.length;
    }
}

console.log("Active users:", activeUsers);
console.log("Inactive users:", inactiveUsers);
console.log("Users with preferences:", usersWithPreferences);
console.log("Average bio length:", Math.round(totalBioLength / loadedUsers.length), "characters");
console.log("Activity rate:", Math.round((activeUsers / loadedUsers.length) * 100) + "%");

// Date and time analysis
console.log("\n⏰ Login Analysis:");
var now = Date.now();
var currentDate = new Date(now);
console.log("Current time:", currentDate.toISOString());

for (var j = 0; j < loadedUsers.length; j++) {
    var user = loadedUsers[j];
    console.log("\nUser:", user.name);
    
    if (user.lastLogin === null) {
        console.log("  Last login: Never logged in");
    } else {
        var lastLoginDate = new Date(user.lastLogin);
        var timeDiff = now - user.lastLogin;
        var daysDiff = Math.floor(timeDiff / (24 * 60 * 60 * 1000));
        var hoursDiff = Math.floor(timeDiff / (60 * 60 * 1000));
        
        console.log("  Last login:", lastLoginDate.toISOString());
        
        if (daysDiff > 0) {
            console.log("  Time since:", daysDiff, "days ago");
        } else {
            console.log("  Time since:", hoursDiff, "hours ago");
        }
    }
}

// Configuration and settings demo (showing escape sequences)
console.log("\n⚙️ System Configuration:");
var config = {
    "appName": "ECEngine User Manager",
    "version": "1.0.0",
    "database": {
        "host": "localhost",
        "path": "C:\\data\\users.json",
        "backup_path": "/var/backups/users_backup.json"
    },
    "logging": {
        "level": "INFO",
        "format": "%(asctime)s - %(name)s - %(levelname)s - %(message)s",
        "file": "/var/log/app.log"
    },
    "features": {
        "authentication": true,
        "analytics": true,
        "notifications": false,
        "beta_features": null
    }
};

console.log("Configuration:");
console.log(config);

console.log("\nConfiguration JSON:");
var configJson = JSON.stringify(config);
console.log(configJson);

// Validation and error handling demo
console.log("\n🔍 Data Validation:");

function validateUser(user) {
    var errors = [];
    
    if (!user.name || user.name === "") {
        errors.push("Name is required");
    }
    
    if (!user.email || user.email === "") {
        errors.push("Email is required");
    }
    
    if (user.email && !user.email.includes("@")) {
        errors.push("Invalid email format");
    }
    
    if (user.isActive !== true && user.isActive !== false) {
        errors.push("isActive must be true or false");
    }
    
    return errors;
}

// Test validation with good and bad data
var testUsers = [
    { "name": "Valid User", "email": "valid@example.com", "isActive": true },
    { "name": "", "email": "invalid-email", "isActive": null },
    { "name": null, "email": null, "isActive": "maybe" }
];

for (var k = 0; k < testUsers.length; k++) {
    var testUser = testUsers[k];
    var validationErrors = validateUser(testUser);
    
    console.log("Testing user:", testUser);
    if (validationErrors.length === 0) {
        console.log("✅ Valid user");
    } else {
        console.log("❌ Validation errors:", validationErrors);
    }
}

// Performance metrics
console.log("\n⚡ Performance Metrics:");
var startTime = Date.now();

// Simulate some processing
var result = 0;
for (var m = 0; m < 1000; m++) {
    result += Math.sqrt(m) * Math.sin(m / 100);
}

var endTime = Date.now();
var processingTime = endTime - startTime;

console.log("Processing result:", Math.round(result * 100) / 100);
console.log("Processing time:", processingTime, "milliseconds");
console.log("Operations per second:", Math.round(1000 / processingTime * 1000));

// Summary report
console.log("\n📋 Summary Report:");
var report = {
    "timestamp": new Date().toISOString(),
    "total_users": loadedUsers.length,
    "active_users": activeUsers,
    "inactive_users": inactiveUsers,
    "activity_rate": Math.round((activeUsers / loadedUsers.length) * 100),
    "data_size": jsonData.length,
    "processing_time_ms": processingTime,
    "status": "completed",
    "notes": "All users processed successfully\nNo errors encountered"
};

console.log("Final Report:");
console.log(report);

console.log("\nReport as JSON:");
console.log(JSON.stringify(report));

console.log("\n🎉 ECEngine Demo Complete!");
console.log("Features demonstrated:");
console.log("✅ JSON.parse() and JSON.stringify()");
console.log("✅ null values and null checking");
console.log("✅ Single quotes and double quotes");
console.log("✅ Escape sequences (\\n, \\t, \\\", \\', \\\\)");
console.log("✅ Date object and Date.now()");
console.log("✅ Math operations and functions");
console.log("✅ Object and array manipulation");
console.log("✅ String operations and validation");
console.log("✅ Performance measurement");
console.log("✅ Real-world data processing scenarios");
