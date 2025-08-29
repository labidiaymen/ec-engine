// Block scoping test for let and const in ECEngine

console.log("=== Block Scoping Tests ===");

// Test 1: let block scoping
console.log("\n--- Test 1: let block scoping ---");
var x = "global x";
console.log("Before block: x = " + x);

{
    let x = "block x";
    console.log("Inside block: x = " + x);
    
    let y = "block y";
    console.log("Inside block: y = " + y);
}

console.log("After block: x = " + x);
// console.log("After block: y = " + y); // This should cause an error

// Test 2: const block scoping
console.log("\n--- Test 2: const block scoping ---");
const globalConst = "global const";
console.log("Global const: " + globalConst);

{
    const blockConst = "block const";
    console.log("Block const: " + blockConst);
    
    // Test const immutability
    // blockConst = "modified"; // This should cause an error
}

// console.log("Block const outside: " + blockConst); // This should cause an error

// Test 3: var function scoping (should not be block scoped)
console.log("\n--- Test 3: var function scoping ---");
var a = "global a";
console.log("Before block: a = " + a);

{
    var a = "block a";
    console.log("Inside block: a = " + a);
    
    var b = "block b";
    console.log("Inside block: b = " + b);
}

console.log("After block: a = " + a);
console.log("After block: b = " + b); // var should be accessible outside block

// Test 4: Nested blocks
console.log("\n--- Test 4: Nested blocks ---");
let outer = "outer";
console.log("Outer scope: outer = " + outer);

{
    let outer = "middle";
    console.log("Middle scope: outer = " + outer);
    
    {
        let outer = "inner";
        console.log("Inner scope: outer = " + outer);
        
        let innerOnly = "inner only";
        console.log("Inner scope: innerOnly = " + innerOnly);
    }
    
    console.log("Back to middle scope: outer = " + outer);
    // console.log("Inner variable: " + innerOnly); // Should cause error
}

console.log("Back to outer scope: outer = " + outer);

// Test 5: Mixed declarations in same block
console.log("\n--- Test 5: Mixed declarations ---");
{
    var varInBlock = "var in block";
    let letInBlock = "let in block";
    const constInBlock = "const in block";
    
    console.log("var: " + varInBlock);
    console.log("let: " + letInBlock);
    console.log("const: " + constInBlock);
}

console.log("var outside block: " + varInBlock); // Should work
// console.log("let outside block: " + letInBlock); // Should fail
// console.log("const outside block: " + constInBlock); // Should fail

console.log("\n=== Block Scoping Tests Complete ===");
