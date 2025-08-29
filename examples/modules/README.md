# ECEngine Module System

ECEngine supports a comprehensive module system with `export` and `import` statements, enabling code organization into reusable, maintainable modules.

## Features

- **Export statements**: Export variables, constants, and functions from modules
- **Import statements**: Import specific exports from other modules using destructuring syntax
- **Module resolution**: Automatic file resolution with `.ec` extension
- **Error handling**: Comprehensive error messages for missing modules or exports
- **Relative paths**: Support for relative module paths (e.g., `./math.ec`)
- **Module initialization**: Modules can execute initialization code when loaded

## Syntax

### Export Statement

```javascript
// Export constants and variables
export var PI = 3.14159;
export const MAX_SIZE = 100;

// Export functions
export function add(a, b) {
    return a + b;
}

// Export multiple items
export function multiply(a, b) { return a * b; }
export function divide(a, b) { return a / b; }
```

### Import Statement

```javascript
// Import specific exports using destructuring
import { PI, add, multiply } from "./math.ec";

// Import multiple items from different modules
import { GREETING, repeat, make_greeting } from "./strings.ec";
```

## Examples

### Math Module (`math.ec`)

A comprehensive mathematical utilities module:

```javascript
// Mathematical constants
export var PI = 3.14159;
export var E = 2.71828;
export var GOLDEN_RATIO = 1.618;

// Basic arithmetic
export function add(a, b) { return a + b; }
export function multiply(a, b) { return a * b; }
export function square(x) { return multiply(x, x); }

// Geometric calculations
export function circle_area(radius) {
    return multiply(PI, square(radius));
}

// Utility functions
export function max(a, b) { return a > b ? a : b; }
export function absolute(x) { return x < 0 ? -x : x; }
```

### String Utilities (`strings.ec`)

String manipulation and formatting functions:

```javascript
// String constants
export var GREETING = "Hello, World!";
export var FAREWELL = "Goodbye!";

// String operations
export function repeat(str, times) { /* implementation */ }
export function make_greeting(name) { return "Hello, " + name + "!"; }
export function create_title(text) { return "=== " + text + " ==="; }

// Formatting utilities
export function format_number(num) { return "Number: " + num; }
export function add_quotes(text) { return '"' + text + '"'; }
```

### Main Application (`main.ec`)

Demonstrates module usage and composition:

```javascript
// Import from multiple modules
import { PI, add, multiply, circle_area, max } from "./math.ec";
import { GREETING, make_greeting, create_title, format_number } from "./strings.ec";

console.log(create_title("Math Operations"));

// Use imported constants
console.log("PI = " + PI);

// Combine imported functions
var area = circle_area(5);
var result = add(multiply(PI, 10), 5);

console.log("Circle area: " + format_number(area));
console.log("Calculation: " + format_number(result));

// Create formatted output
var greeting = make_greeting("Developer");
console.log(greeting);
```

## Module Resolution

- **Relative paths**: `./module.ec` resolves relative to the importing file
- **Automatic extension**: `.ec` is added automatically if not specified
- **Directory resolution**: Modules are resolved relative to the importing file's directory

## Error Handling

The module system provides detailed error messages:

### Module Not Found
```
Error: Module not found: ./nonexistent.ec
Context: Could not find module file at /path/to/nonexistent.ec
```

### Export Not Found
```
Error: 'undefined_function' is not exported by module './math.ec'
Context: Module './math.ec' does not export 'undefined_function'
```

## Running Examples

```bash
# Run the main demonstration
dotnet run examples/modules/main.ec

# Test individual modules
dotnet run examples/modules/math.ec     # Shows math module initialization
dotnet run examples/modules/strings.ec # Shows string module initialization
```

## Module Features Demonstrated

1. **Constants Export/Import**: Mathematical and string constants
2. **Function Export/Import**: Utility functions across modules
3. **Module Composition**: Using multiple modules together
4. **Cross-module Dependencies**: Functions calling other imported functions
5. **Module Initialization**: Console output during module loading
6. **Error Handling**: Graceful handling of missing modules/exports

## Best Practices

- **Clear naming**: Use descriptive names for exported functions and constants
- **Logical grouping**: Group related functionality in the same module
- **Documentation**: Include comments explaining module purpose and usage
- **Initialization messages**: Optional console output to confirm module loading
- **Error handling**: Check for missing dependencies and provide helpful messages

## Implementation Notes

- Modules are cached after first load to prevent re-execution
- Each module has its own scope and exports dictionary
- Import statements are processed before other code execution
- Module system is automatically configured when running files with ECEngine
