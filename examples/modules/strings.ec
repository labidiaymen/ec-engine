// String utility module - examples/modules/strings.ec
// Demonstrates string manipulation functions and constants

// Export string constants
export var GREETING = "Hello, World!";
export var FAREWELL = "Goodbye!";
export var EMPTY_STRING = "";

// Export string manipulation functions
export function repeat(str, times) {
    console.log("Repeating '" + str + "' " + times + " times");
    var result = "";
    
    // Simple repeat implementation
    if (times >= 1) result = result + str;
    if (times >= 2) result = result + str;
    if (times >= 3) result = result + str;
    if (times >= 4) result = result + str;
    if (times >= 5) result = result + str;
    
    return result;
}

export function concat(str1, str2) {
    return str1 + str2;
}

export function concat_three(str1, str2, str3) {
    return str1 + str2 + str3;
}

export function make_greeting(name) {
    return "Hello, " + name + "!";
}

export function make_farewell(name) {
    return "Goodbye, " + name + "!";
}

// Export string formatting functions
export function format_number(num) {
    return "Number: " + num;
}

export function format_boolean(bool) {
    return "Boolean: " + bool;
}

export function create_sentence(subject, verb, object) {
    return subject + " " + verb + " " + object + ".";
}

// Export string building utilities
export function build_list(item1, item2, item3) {
    return "List: " + item1 + ", " + item2 + ", " + item3;
}

export function create_title(text) {
    return "=== " + text + " ===";
}

export function add_quotes(text) {
    return "\"" + text + "\"";
}

export function create_message(level, text) {
    return "[" + level + "] " + text;
}

// Module initialization
console.log("String utilities module loaded");
