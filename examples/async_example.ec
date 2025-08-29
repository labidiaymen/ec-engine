// Test async functionality with event loop

console.log("Starting async test...");

// Define callback functions
function timeout1() {
    console.log("Timeout 1 executed (1000ms)");
}

function timeout2() {
    console.log("Timeout 2 executed (500ms)");
}

function nextTickCallback() {
    console.log("Next tick executed");
}

function finalTimeout() {
    console.log("Final timeout executed (2000ms) - exiting");
}

// Test setTimeout
setTimeout(timeout1, 1000);
setTimeout(timeout2, 500);

// Test nextTick (immediate execution after current frame)
nextTick(nextTickCallback);

console.log("Sync code completed");

// Keep the event loop running for a bit
setTimeout(finalTimeout, 2000);
