# ECEngine

[![CI/CD Pipeline](https://github.com/labidiaymen/ec-engine/actions/workflows/ci.yml/badge.svg)](https://github.com/labidiaymen/ec-engine/actions/workflows/ci.yml)
[![Code Quality](https://github.com/labidiaymen/ec-engine/actions/workflows/quality.yml/badge.svg)](https://github.com/labidiaymen/ec-engine/actions/workflows/quality.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 7.0](https://img.shields.io/badge/.NET-7.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/7.0)

A lightweight ECMAScript (JavaScript) interpreter engine written in C# that supports standard JavaScript syntax plus additional design patterns and reactive programming features.

> **📚 Educational Purpose**: ECEngine is designed primarily for educational purposes to demonstrate JavaScript engine implementation concepts, language design patterns, and reactive programming techniques. It serves as a learning tool for understanding how interpreters work and exploring innovative language features.

## 📋 Features & Progress

For a comprehensive overview of implemented and planned features, see the **[📊 Features Checklist](FEATURES_CHECKLIST.md)** which tracks ECMAScript compatibility and implementation progress.

## Overview

ECEngine is a JavaScript interpreter that extends ECMAScript with reactive programming capabilities. Built in C#, it provides a complete JavaScript runtime environment with an innovative `observe` pattern for automatic change detection and reactive programming.

## Features

- **ECMAScript Compatibility** - Full JavaScript syntax support including variables, functions, arrays, and modules
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

// Arrays with methods
var numbers = [1, 2, 3, 4, 5];
numbers.push(6);             // Returns new length: 6
var subset = numbers.slice(1, 3); // [2, 3]
var joined = numbers.join(", ");   // "1, 2, 3, 4, 5"

// Expressions
var result = x * 2 + 10;
console.log(greet("ECEngine"));
```

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

