// Simple startup benchmark for ECEngine
const start = Date.now();

console.log("Hello from ECEngine!");
const arr = [1, 2, 3, 4, 5];
let sum = 0;
for (let i = 0; i < arr.length; i++) {
    sum += arr[i] * 2;
}

const end = Date.now();
const startupTime = end - start;

console.log("Startup time: " + startupTime + "ms");
console.log("Result: " + sum);
