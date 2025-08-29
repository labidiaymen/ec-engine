// While loop examples

// Basic while loop
console.log("Basic while loop:");
let count = 0;
while (count < 5) {
    console.log("Count:", count);
    count++;
}

// While loop with break
console.log("\nWhile loop with break:");
let num = 0;
while (true) {
    if (num == 3) {
        console.log("Breaking at:", num);
        break;
    }
    console.log("Number:", num);
    num++;
}

// While loop with continue
console.log("\nWhile loop with continue:");
let value = 0;
while (value < 5) {
    value++;
    if (value == 3) {
        console.log("Skipping:", value);
        continue;
    }
    console.log("Processing:", value);
}
