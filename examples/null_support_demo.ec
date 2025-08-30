// ECEngine Null Support Demo
// Demonstrates null keyword and null value handling

console.log("=== ECEngine Null Support Demo ===");
console.log("");

// 1. Basic null assignment
console.log("1. Basic null assignment:");
var nullValue = null;
console.log("nullValue =", nullValue);
console.log("");

// 2. Null in objects
console.log("2. Null in objects:");
var userProfile = {
    "name": "John Doe",
    "email": "john@example.com",
    "phone": null,
    "avatar": null,
    "age": 30
};
console.log("User profile:", userProfile);
console.log("");

// 3. Null behavior
console.log("3. Null behavior:");
var data = {
    "defined": "value",
    "nullField": null
};
console.log("Data object:", data);
console.log("");

// 4. JSON with null values
console.log("4. JSON serialization with null:");
var apiResponse = {
    "success": true,
    "data": {
        "id": 123,
        "name": "Product A",
        "description": "A great product",
        "discount": null,
        "category": {
            "id": 5,
            "name": "Electronics",
            "parent": null
        }
    },
    "error": null,
    "timestamp": Date.now()
};

console.log("API Response:", apiResponse);

var responseJson = JSON.stringify(apiResponse);
console.log("JSON Response:", responseJson);

var parsedResponse = JSON.parse(responseJson);
console.log("Parsed Response:", parsedResponse);
console.log("");

// 5. Database record example
console.log("5. Database record example:");
var databaseRecord = {
    "id": 1001,
    "username": "alice_smith",
    "email": "alice@example.com",
    "firstName": "Alice",
    "lastName": "Smith",
    "middleName": null,
    "dateOfBirth": null,
    "profileImage": null,
    "phoneNumber": "+1-555-0123",
    "address": {
        "street": "123 Main St",
        "apartment": null,
        "city": "New York",
        "state": "NY",
        "zipCode": "10001",
        "country": "USA"
    },
    "preferences": {
        "theme": "dark",
        "language": "en",
        "timezone": null,
        "notifications": {
            "email": true,
            "sms": false,
            "push": null
        }
    },
    "createdAt": Date.now(),
    "lastLogin": null,
    "deletedAt": null
};

console.log("Database record:", databaseRecord);

// Convert to JSON for storage/transmission
var recordJson = JSON.stringify(databaseRecord);
console.log("Record as JSON:", recordJson);

// Parse back from JSON
var retrievedRecord = JSON.parse(recordJson);
console.log("Retrieved record:", retrievedRecord);

console.log("");
console.log("=== Null Support Demo Complete ===");
