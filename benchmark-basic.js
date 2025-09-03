// Very simple performance benchmark for ECEngine
console.log("ECEngine Performance Benchmark");
console.log("==============================");
console.log("Runtime: ECEngine");
console.log("Date: " + new Date().toISOString());

function runBenchmark(name, fn, iterations) {
    console.log("\n=== " + name + " ===");
    const start = Date.now();
    
    for (let i = 0; i < iterations; i++) {
        fn(i);
    }
    
    const end = Date.now();
    const duration = end - start;
    const opsPerSec = Math.round((iterations / duration) * 1000);
    
    console.log("Time: " + duration + "ms");
    console.log("Iterations: " + iterations);
    console.log("Ops/sec: " + opsPerSec);
    
    return opsPerSec;
}

// 1. Basic arithmetic operations
function arithmeticBench(i) {
    let result = 0;
    result += i * 2;
    result -= i / 2;
    result *= i + 1;
    return result;
}

// 2. String operations
function stringBench(i) {
    let str = "hello" + i;
    str = str.toUpperCase();
    str = str.substring(0, 5);
    return str.length;
}

// 3. Simple array operations
function arrayBench(i) {
    let arr = [1, 2, 3, 4, 5];
    arr.push(i);
    let sum = 0;
    for (let j = 0; j < arr.length; j++) {
        sum += arr[j] * 2;
    }
    return sum;
}

// 4. Object operations
function objectBench(i) {
    let obj = { a: 1, b: 2, c: 3 };
    obj.d = i;
    obj.e = i * 2;
    return obj.d + obj.e;
}

// 5. Function calls
function functionBench(i) {
    function inner(x) {
        return x * x;
    }
    
    let result = 0;
    for (let j = 0; j < 5; j++) {
        result += inner(i + j);
    }
    return result;
}

// Run benchmarks
const iterations = 100000;

console.log("\nRunning benchmarks...");

const results = [];
results.push(runBenchmark("Arithmetic Operations", arithmeticBench, iterations));
results.push(runBenchmark("String Operations", stringBench, iterations));
results.push(runBenchmark("Array Operations", arrayBench, iterations));
results.push(runBenchmark("Object Operations", objectBench, iterations));
results.push(runBenchmark("Function Calls", functionBench, iterations));

// Calculate overall score
let totalOps = 0;
for (let i = 0; i < results.length; i++) {
    totalOps += results[i];
}
const averageOps = Math.round(totalOps / results.length);

console.log("\n=== OVERALL SCORE ===");
console.log("Average: " + averageOps + " ops/sec");

console.log("\nBenchmark completed!");
