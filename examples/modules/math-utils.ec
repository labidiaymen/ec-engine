// Test module for advanced imports - exports named functions and default
export function add(a, b) {
    return a + b;
}

export function multiply(a, b) {
    return a * b;
}

export const PI = 3.14159;

// Default export
export default function calculate(x, y) {
    return add(x, y) * multiply(x, y);
}
