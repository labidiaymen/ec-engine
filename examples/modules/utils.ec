// Another test module
export const greeting = "Hello";
export const version = "1.0.0";

export function format(name) {
    return `${greeting}, ${name}!`;
}

export function getInfo() {
    return {
        version: version,
        author: "ECEngine"
    };
}
