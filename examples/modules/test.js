// JavaScript module test file
export const jsValue = "Hello from JavaScript!";
export function jsFunction() {
    return "This is a JS function";
}

let internalValue = 42;
export { internalValue as number };
