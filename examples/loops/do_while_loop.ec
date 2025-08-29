// Do-while loop examples

// Basic do-while loop
console.log("Basic do-while loop:");
let count = 0;
do {
    console.log("Count:", count);
    count++;
} while (count < 3);

// Do-while loop that executes at least once
console.log("\nDo-while loop with false condition:");
let flag = false;
do {
    console.log("This runs at least once, even though flag is false");
} while (flag);

// Do-while loop with break
console.log("\nDo-while loop with break:");
let num = 0;
do {
    if (num == 2) {
        console.log("Breaking at:", num);
        break;
    }
    console.log("Number:", num);
    num++;
} while (num < 5);

// Do-while loop with continue
console.log("\nDo-while loop with continue:");
let value = 0;
do {
    value++;
    if (value == 2) {
        console.log("Skipping:", value);
        continue;
    }
    console.log("Processing:", value);
} while (value < 4);
