// Math utility module - examples/modules/math.ec
// Demonstrates exporting constants, variables, and functions

// Export mathematical constants
export var PI = 3.14159;
export var E = 2.71828;
export var GOLDEN_RATIO = 1.618;

// Export basic arithmetic functions
export function add(a, b) {
    console.log("Math.add called with: " + a + ", " + b);
    return a + b;
}

export function subtract(a, b) {
    return a - b;
}

export function multiply(a, b) {
    return a * b;
}

export function divide(a, b) {
    if (b == 0) {
        console.log("Warning: Division by zero!");
        return 0;
    }
    return a / b;
}

// Export advanced mathematical functions
export function square(x) {
    return multiply(x, x);
}

export function power(base, exponent) {
    var result = 1;
    var i = 0;
    
    // Simple power calculation (limited implementation)
    if (exponent >= 1) result = multiply(result, base);
    if (exponent >= 2) result = multiply(result, base);
    if (exponent >= 3) result = multiply(result, base);
    if (exponent >= 4) result = multiply(result, base);
    
    return result;
}

export function circle_area(radius) {
    console.log("Calculating area for radius: " + radius);
    return multiply(PI, square(radius));
}

export function circle_circumference(radius) {
    return multiply(multiply(2, PI), radius);
}

// Export utility functions
export function max(a, b) {
    if (a > b) {
        return a;
    } else {
        return b;
    }
}

export function min(a, b) {
    if (a < b) {
        return a;
    } else {
        return b;
    }
}

export function absolute(x) {
    if (x < 0) {
        return subtract(0, x);
    } else {
        return x;
    }
}

// Module initialization message
console.log("Math module loaded with " + "PI=" + PI + ", E=" + E);
