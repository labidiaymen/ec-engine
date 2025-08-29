// For loop examples

// Basic for loop
console.log("Basic for loop:");
for (let i = 0; i < 5; i++) {
    console.log("Iteration:", i);
}

// For loop with break
console.log("\nFor loop with break:");
for (let i = 0; i < 10; i++) {
    if (i == 3) {
        console.log("Breaking at:", i);
        break;
    }
    console.log("Value:", i);
}

// For loop with continue
console.log("\nFor loop with continue:");
for (let i = 0; i < 5; i++) {
    if (i == 2) {
        console.log("Skipping:", i);
        continue;
    }
    console.log("Processing:", i);
}

// Nested for loops
console.log("\nNested for loops:");
for (let i = 1; i <= 3; i++) {
    for (let j = 1; j <= 2; j++) {
        console.log("i =", i, ", j =", j);
    }
}
