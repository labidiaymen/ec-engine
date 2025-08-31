// ECEngine Object Methods with 'this' Support Test
// Validates object method functionality with 'this' keyword

console.log("=== Object Methods 'this' Support Test ===");

// Test 1: Basic 'this' keyword functionality
var person = {
    name: "Alice",
    age: 30,
    city: "New York",
    
    getName: function() {
        return this.name;
    },
    
    introduce: function() {
        return "Hi, I'm " + this.name + " from " + this.city + ". I'm " + this.age + " years old.";
    }
};

console.log("✅ Basic 'this' access:", person.getName() == "Alice");
console.log("✅ Complex 'this' expression:", person.introduce() != "");

// Test 2: Mathematical operations with 'this'
var calculator = {
    x: 15,
    y: 8,
    
    add: function() {
        return this.x + this.y;
    },
    
    calculate: function(operation) {
        if (operation == "add") return this.add();
        return "Unknown operation";
    }
};

console.log("✅ Math operations:", calculator.add() == 23);
console.log("✅ Method chaining:", calculator.calculate("add") == 23);

// Test 3: Nested objects with 'this'
var company = {
    name: "TechCorp",
    
    hr: {
        department: "Human Resources",
        employees: 25,
        
        getDepartmentInfo: function() {
            return this.department + " - " + this.employees + " employees";
        }
    }
};

console.log("✅ Nested 'this':", company.hr.getDepartmentInfo() == "Human Resources - 25 employees");

// Test 4: Methods with parameters
var formatter = {
    prefix: "[LOG]",
    
    format: function(message) {
        return this.prefix + " " + message;
    }
};

console.log("✅ Parameters with 'this':", formatter.format("Hello") == "[LOG] Hello");

// Test 5: Factory functions with 'this' methods
function createCounter(initialValue) {
    return {
        value: initialValue,
        
        getValue: function() {
            return this.value;
        }
    };
}

var counter = createCounter(42);
console.log("✅ Factory 'this':", counter.getValue() == 42);

// Test 6: Complex conditionals with 'this'
var status = {
    health: 100,
    
    getHealthStatus: function() {
        if (this.health > 80) return "Excellent";
        if (this.health > 50) return "Good";
        return "Poor";
    }
};

console.log("✅ Conditionals with 'this':", status.getHealthStatus() == "Excellent");

// Test 7: Property assignment (now working!)
var mutableObject = {
    count: 0,
    name: "test",
    
    updateCount: function(value) {
        this.count = value;
        return this.count;
    },
    
    updateName: function(newName) {
        this.name = newName;
        return this.name;
    }
};

console.log("✅ Property assignment:", mutableObject.updateCount(42) == 42);
console.log("✅ Property assignment:", mutableObject.updateName("updated") == "updated");