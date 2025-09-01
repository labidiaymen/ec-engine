// Test both new and typeof operators together
console.log("Testing new and typeof operators together...");

// Test typeof with new operator results
var obj = new Object();
console.log("typeof new Object():", typeof obj);

var arr = new Array();
console.log("typeof new Array():", typeof arr);

var str = new String("hello");
console.log("typeof new String('hello'):", typeof str);

var num = new Number(42);
console.log("typeof new Number(42):", typeof num);

var bool = new Boolean(true);
console.log("typeof new Boolean(true):", typeof bool);

// Test typeof in expressions with new
console.log("Complex expression:", typeof (new Array(1, 2, 3)));

console.log("Both operators working together successfully!");
