# ECEngine

[![CI/CD Pipeline](https://github.com/labidiaymen/ec-engine/actions/workflows/ci.yml/badge.svg)](https://github.com/labidiaymen/ec-engine/actions/workflows/ci.yml)
[![Code Quality](https://github.com/labidiaymen/ec-engine/actions/workflows/quality.yml/badge.svg)](https://github.com/labidiaymen/ec-engine/actions/workflows/quality.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 7.0](https://img.shields.io/badge/.NET-7.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/7.0)

A lightweight ECMAScript (JavaScript) interpreter engine written in C# that supports standard JavaScript syntax plus modern language features like pipeline operators, switch expressions, and reactive programming patterns.

> **📚 Educational Purpose**: ECEngine is designed primarily for educational purposes to demonstrate JavaScript engine implementation concepts, language design patterns, and reactive programming techniques. It serves as a learning tool for understanding how interpreters work and exploring innovative language features.

## 📋 Features & Progress

For a comprehensive overview of implemented and planned features, see the **[📊 Features Checklist](FEATURES_CHECKLIST.md)** which tracks ECMAScript compatibility and implementation progress.

## Overview

ECEngine is a JavaScript interpreter that extends ECMAScript with modern language features and reactive programming capabilities. Built in C#, it provides a complete JavaScript runtime environment with innovative features like the pipeline operator (`|>`) for functional programming, C#-style switch expressions, and an `observe` pattern for automatic change detection.

## Features

- **ECMAScript Compatibility** - Full JavaScript syntax support including variables, functions, arrays, and modules
- **Pipeline Operator (`|>`)** - Functional programming with clean data transformation workflows and automatic parameter injection
- **C#-Style Switch Expressions** - Modern pattern matching with `=>` syntax and discard patterns (`_`)
- **Alternative Syntax** - More readable operators: `is` for `==`, `and` for `&&`, `or` for `||`
- **Complete String API** - All 70+ JavaScript string methods with Unicode support and full MDN compatibility
- **Method Chaining** - Support for chaining method calls like `text.trim().toUpperCase().replace("old", "new")`
- **Reactive Programming** - Built-in `observe` pattern for automatic variable change detection  
- **Module System** - Complete `import`/`export` support with automatic resolution
- **Interactive REPL** - Advanced console with cursor navigation and command history
- **Developer Tools** - VS Code extension with syntax highlighting and comprehensive examples

## Getting Started

### Installation

**Quick Install (macOS, Linux, WSL):**
```bash
curl -fsSL https://raw.githubusercontent.com/labidiaymen/ec-engine/main/install.sh | bash
```

**Windows (PowerShell):**
```powershell
iwr -useb https://raw.githubusercontent.com/labidiaymen/ec-engine/main/install.ps1 | iex
```

After installation, the binary will be available as `eec`.

### Prerequisites (for building from source)
- .NET 9.0 or later
- Visual Studio, VS Code, or any C# compatible IDE

### Running the Engine

#### Using installed binary
```bash
eec path/to/script.ec     # Execute a file
eec -i                    # Interactive mode
eec --help               # Show help
eec --version            # Show version
```

#### Building and running from source
Execute ECEngine files directly:
```bash
dotnet run path/to/script.ec
```

#### Interactive Mode (REPL)
Start the interactive shell:
```bash
dotnet run -i
```

#### Example Programs
Try the included examples:
```bash
# Variable declarations and observe pattern
dotnet run examples/variables/simple_observe.ec

# Function examples
dotnet run examples/functions/basic_functions.ec

# Pipeline operator demo
dotnet run examples/pipeline_demo.ec

# Expression evaluation
dotnet run examples/expressions/arithmetic.ec
```

### Building the Project

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

## Sample Usage

### Reactive Programming with Observe Pattern
ECEngine extends JavaScript with reactive programming capabilities:

```javascript
// Standard JavaScript approach
var temperature = 20;

// ECEngine - Reactive with observe pattern
observe temperature function() {
    console.log("Temperature changed to: " + temperature);
    if (temperature > 30) {
        console.log("It's getting hot!");
    }
}

// Automatic change detection
temperature = 35;  // Output: "Temperature changed to: 35" + "It's getting hot!"
temperature = 25;  // Output: "Temperature changed to: 25"
```

### Standard JavaScript Features
```javascript
// Variable declarations
var x = 42;
let y = "Hello World";
const z = true;

// Functions
function greet(name) {
    return "Hello, " + name + "!";
}

// Pipeline operator for clean data flow
function double(x) { return x * 2; }
function add10(x) { return x + 10; }

var result = 5 |> double |> add10;  // Result: 20
// Equivalent to: add10(double(5))

// Arrays with methods
var numbers = [1, 2, 3, 4, 5];
numbers.push(6);             // Returns new length: 6
var subset = numbers.slice(1, 3); // [2, 3]
var joined = numbers.join(", ");   // "1, 2, 3, 4, 5"

// Expressions
var calculation = x * 2 + 10;
console.log(greet("ECEngine"));
```

### Alternative Syntax Operators
ECEngine provides more readable alternative operators for common operations:

```javascript
// Traditional JavaScript vs ECEngine alternatives
var age = 25;
var isAdmin = false;
var username = "john";

// Comparison operators
if (age == 25) { }          // Traditional
if (age is 25) { }          // ECEngine alternative

// Logical AND operators  
if (age > 18 && isAdmin) { }     // Traditional
if (age > 18 and isAdmin) { }    // ECEngine alternative

// Logical OR operators
if (age < 18 || username is "admin") { }    // Traditional
if (age < 18 or username is "admin") { }    // ECEngine alternative

// Complex expressions work seamlessly
if (age is 25 and (isAdmin or username is "john")) {
    console.log("Access granted!");
}

// Mixed usage is supported
if (age == 25 and isAdmin or username is "admin") {
    console.log("Welcome!");
}
```

### C#-Style Switch Expressions
ECEngine supports modern C#-style switch expressions with pattern matching and discard patterns:

```javascript
// Switch expressions for cleaner value assignment
var grade = "B";
var description = grade switch {
    "A" => "Excellent",
    "B" => "Good", 
    "C" => "Average",
    "D" => "Below Average",
    "F" => "Fail",
    _ => "Unknown Grade"  // Discard pattern for default case
};

console.log("Grade description:", description); // "Good"

// Switch expressions with numbers
var dayNumber = 3;
var dayName = dayNumber switch {
    1 => "Monday",
    2 => "Tuesday",
    3 => "Wednesday",
    4 => "Thursday",
    5 => "Friday",
    _ => "Weekend"
};

// Complex expressions in switch arms
var number = 15;
var category = number switch {
    1 => "single",
    2 => "double",
    3 => "triple",
    _ => number > 10 ? "large" : "small"
};
```

### Pipeline Operator (`|>`)
ECEngine introduces the pipeline operator for functional programming and clean data transformation workflows:

```javascript
// Basic pipeline chaining
var result = 5 |> double |> add10;  // double(5) |> add10 = 20

// String processing pipeline
var text = "hello world" |> toUpperCase |> addExclamation;  // "HELLO WORLD!"

// Multiline pipelines for better readability
var processed = "javascript programming"
    |> filterLength(5)      // Keep only if >= 5 chars
    |> capitalizeFirst()    // Capitalize first letter  
    |> truncate(10);        // Truncate to 10 chars
// Result: "Javascript..."

// Mathematical operations with parameters
var mathResult = 5
    |> addNumbers(3)        // addNumbers(5, 3) = 8
    |> multiply(4)          // multiply(8, 4) = 32
    |> power(2);            // power(32, 2) = 1024

// AI prompt chaining example
var aiResult = "User wants to learn JavaScript"
    |> analyzeContext       // Analyze the user input context
    |> enhancePrompt        // Enhance with AI insights  
    |> formatForModel       // Format for AI model consumption
    |> processResponse;     // Process the AI response

// Complex workflow with parameters
var workflow = "Help me debug this Python code"
    |> sanitizeInput(50)           // Limit input length to 50 chars
    |> addSystemPrompt("coding assistant")  // Add system role
    |> tokenize(15);               // Limit to 15 tokens max
```

#### How Pipeline Works
The pipeline operator (`|>`) automatically injects the left-hand value as the first parameter to the function on the right:

- `value |> func(param1, param2)` becomes `func(value, param1, param2)`
- `value |> func()` becomes `func(value)`
- `value |> func` becomes `func(value)` (for simple identifiers)

#### Benefits
- **Readable Data Flow**: Left-to-right reading matches data transformation flow
- **Functional Composition**: Clean function chaining without deep nesting
- **Parameter Injection**: Automatic argument passing eliminates wrapper functions
- **Multiline Support**: Break complex pipelines across lines for clarity

### Programmatic Usage
```csharp
// Create and execute ECEngine code
string code = @"
    var message = 'Hello from ECEngine!';
    observe message function() {
        console.log('Message changed: ' + message);
    }
    message = 'Updated message';
";

// Tokenize
var lexer = new Lexer(code);
var tokens = lexer.Tokenize();

// Parse
var parser = new Parser();
var ast = parser.Parse(code);

// Interpret
var interpreter = new Interpreter();
interpreter.Evaluate(ast, code);
```

## Architecture

ECEngine implements a modern interpreter architecture with reactive programming capabilities:

### Core Components
1. **Lexer**: Converts ECMAScript source code into tokens with full JavaScript syntax support
2. **Parser**: Builds an Abstract Syntax Tree from tokens, including observe statements
3. **Interpreter**: Walks the AST and evaluates expressions with reactive observer management
4. **Runtime**: Provides built-in functions, variable management, and console environment

### Reactive Programming
The observe pattern enables automatic change detection without manual event binding:

```javascript
// Multiple observers per variable
var counter = 0;

observe counter function() {
    console.log("Observer 1: " + counter);
}

observe counter function() {
    console.log("Observer 2: " + counter * 2);
}

counter = 5;  // Both observers triggered automatically
```

## Security

ECEngine is designed with security in mind:
- **Sandboxed Environment**: No file system or network access
- **Memory Safe**: Uses .NET's managed memory
- **Input Validation**: Handles malicious input gracefully

## License

MIT License - see LICENSE file for details.

