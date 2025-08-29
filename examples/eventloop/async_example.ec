// ECEngine Async Programming Example
console.log("Starting async example...");

// Schedule immediate tasks
nextTick(() => {
    console.log("Next tick task 1");
});

nextTick(() => {
    console.log("Next tick task 2");
});

// Schedule delayed tasks
setTimeout(() => {
    console.log("Timeout after 100ms");
}, 100);

setTimeout(() => {
    console.log("Timeout after 50ms");
}, 50);

// Schedule repeating task
var counter = 0;
var intervalId = setInterval(() => {
    counter = counter + 1;
    console.log("Interval tick: " + counter);
    
    if (counter >= 3) {
        clearInterval(intervalId);
        console.log("Interval cleared");
    }
}, 30);

console.log("All tasks scheduled");
