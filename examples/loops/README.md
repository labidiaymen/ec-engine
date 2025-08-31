# For...in and For...of Loops Examples

This directory contains examples demonstrating ECEngine's support for ES6-style `for...in` and `for...of` loops.

## Files

### `basic_for_in_of.ec`
Simple examples showing the fundamental usage patterns:
- Basic `for...in` with objects
- Basic `for...of` with arrays
- `for...of` with strings
- `for...in` with arrays (showing indices)

### `for_in_of_demo.ec`
Comprehensive demonstration including:
- Object property iteration with `for...in`
- Array value iteration with `for...of`
- String character and index iteration
- Nested loops combining both constructs
- Break and continue statements
- Variable declarations (var, let, const)
- Complex object processing
- Data accumulation patterns

## Syntax

### For...in Loop
Iterates over enumerable properties of an object or indices of an array:

```javascript
// With objects - iterates over property names
for (key in object) {
    console.log(key + ": " + object[key]);
}

// With arrays - iterates over indices
for (index in array) {
    console.log("Index " + index + ": " + array[index]);
}

// With variable declaration
for (var key in object) {
    // key is declared in this scope
}
```

### For...of Loop
Iterates over iterable values (arrays, strings):

```javascript
// With arrays - iterates over values
for (item of array) {
    console.log("Value: " + item);
}

// With strings - iterates over characters
for (char of string) {
    console.log("Character: " + char);
}

// With variable declaration
for (const item of array) {
    // item is declared in this scope
}
```

## Key Differences

| Feature | for...in | for...of |
|---------|----------|----------|
| Objects | ✅ Property names | ❌ Not iterable |
| Arrays | ✅ Indices (as strings) | ✅ Values |
| Strings | ✅ Indices (as strings) | ✅ Characters |
| Purpose | Enumerate properties | Iterate values |

## Running Examples

```bash
# Run basic examples
dotnet run examples/loops/basic_for_in_of.ec

# Run comprehensive demo
dotnet run examples/loops/for_in_of_demo.ec
```

## Browser Compatibility

These loop constructs follow ES6 standards and behave identically to JavaScript's `for...in` and `for...of` loops.
