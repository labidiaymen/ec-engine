# ECEngine

[![CI/CD Pipeline](https://github.com/labidiaymen/ec-engine/actions/workflows/ci.yml/badge.svg)](https://github.com/labidiaymen/ec-engine/actions/workflows/ci.yml)
[![Code Quality](https://github.com/labidiaymen/ec-engine/actions/workflows/quality.yml/badge.svg)](https://github.com/labidiaymen/ec-engine/actions/workflows/quality.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 7.0](https://img.shields.io/badge/.NET-7.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/7.0)

A lightweight ECMAScript (JavaScript) interpreter engine written in C# that supports standard JavaScript syntax plus additional design patterns and reactive programming features.

## Overview

ECEngine is a modern JavaScript interpreter that implements core ECMAScript features while introducing innovative design patterns for reactive programming. The engine supports:

- **Standard ECMAScript** - Full JavaScript syntax compatibility (variables, functions, expressions, etc.)
- **Reactive Programming** - Built-in `observe` pattern for variable change detection
- **Interactive REPL** - Advanced console with cursor navigation and command history
- **Modern Architecture** - Clean separation of lexing, parsing, and interpretation phases

Key capabilities include:
- **Lexical Analysis** - Complete tokenization of JavaScript source code
- **Parsing** - Robust Abstract Syntax Tree (AST) generation
- **Interpretation** - Full evaluation and execution of JavaScript expressions
- **Runtime** - Comprehensive runtime environment with console support
- **Observe Pattern** - Reactive programming with automatic change detection

## Project Structure

```
ECEngine/
├── AST/                 # Abstract Syntax Tree definitions
│   └── ASTNode.cs      # Complete AST classes including ObserveStatement
├── Lexer/              # Tokenization components
│   ├── Lexer.cs        # Full lexer implementation with all JS tokens
│   └── Token.cs        # Comprehensive token type definitions
├── Parser/             # Parsing components
│   └── Parser.cs       # Complete parser for JavaScript + observe syntax
├── Runtime/            # Runtime environment
│   ├── Interpreter.cs  # Full AST evaluation engine with observe support
│   ├── InteractiveRuntime.cs # REPL with cursor navigation
│   ├── ConsoleInputHandler.cs # Advanced input handling
│   └── ConsoleRuntime.cs # Console.log implementation
├── Tests/              # Comprehensive unit tests
├── examples/           # Example ECEngine programs
│   ├── variables/      # Variable declaration examples
│   ├── functions/      # Function examples
│   ├── expressions/    # Expression examples
│   └── console/        # Console usage examples
├── vscode-extension/   # VS Code syntax highlighting
└── Program.cs          # Main entry point
```

## Features

### ✅ Implemented Features
- **Complete ECMAScript Support**
  - Variable declarations (`var`, `let`, `const`)
  - All data types (numbers, strings, booleans, undefined)
  - Arithmetic and assignment expressions
  - Function declarations and calls
  - Anonymous and higher-order functions
  - Comprehensive operator support

- **Advanced Design Patterns**
  - **Observe Pattern**: `observe variable function() { ... }` for reactive programming
  - Automatic change detection and callback execution
  - Multiple observers per variable support

- **Interactive Development**
  - Full-featured REPL with cursor navigation (←→↑↓)
  - Command history with navigation
  - Line editing (Home, End, Backspace, Delete)
  - Variable inspection (`.vars` command)
  - Interactive debugging support

- **Developer Experience**
  - VS Code extension with syntax highlighting
  - Comprehensive example library
  - Built-in help system
  - Error reporting with location information

### 🔄 Planned Enhancements
- Control flow statements (if/else, loops)
- Object and array literal support
- Advanced error handling
- Module system
- More reactive programming patterns

## Getting Started

### Prerequisites
- .NET 7.0 or later
- Visual Studio, VS Code, or any C# compatible IDE

### Running the Engine

#### File Execution
Execute ECEngine files directly:
```bash
dotnet run path/to/script.ec
```

#### Interactive Mode (REPL)
Start the interactive shell with full cursor support:
```bash
dotnet run -i
```

Features in interactive mode:
- ←/→ arrows for cursor movement
- ↑/↓ arrows for command history
- Home/End for line navigation
- Tab completion (planned)
- Multi-line input support

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

### Basic ECMAScript Features
```javascript
// Variable declarations
var x = 42;
let y = "Hello World";
const z = true;

// Functions
function greet(name) {
    return "Hello, " + name + "!";
}

// Expressions
var result = x * 2 + 10;
console.log(greet("ECEngine"));
```

### Observe Pattern (Reactive Programming)
```javascript
// Declare a variable
var temperature = 20;

// Set up an observer
observe temperature function() {
    console.log("Temperature changed to: " + temperature);
    if (temperature > 30) {
        console.log("It's getting hot!");
    }
}

// Trigger the observer
temperature = 35;  // Output: "Temperature changed to: 35" + "It's getting hot!"
temperature = 25;  // Output: "Temperature changed to: 25"
```

### Interactive Session Example
```bash
$ dotnet run -i
ECEngine Interactive Shell
Type .help for available commands or .exit to quit

ec> var x = 10
10
ec> observe x function() { console.log("x is now: " + x); }
ec> x = 20
x is now: 20
20
ec> .vars
Current variables:
  var x = 20
ec> .exit
Goodbye!
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

### Reactive Programming Architecture
- **Observer Registry**: Tracks variable observers and their associated callback functions
- **Change Detection**: Automatically triggers observers when variable values change
- **Memory Management**: Efficient cleanup of observers and variables
- **Event Propagation**: Handles multiple observers per variable with proper execution order

### Design Patterns
- **Observer Pattern**: Core reactive programming implementation
- **Visitor Pattern**: AST traversal and evaluation
- **Strategy Pattern**: Different evaluation strategies for various node types
- **Factory Pattern**: Token and AST node creation

## ECMAScript Compatibility

ECEngine implements a subset of ECMAScript (JavaScript) with full syntax compatibility for supported features:

### Supported ECMAScript Features
- ✅ **Variables**: `var`, `let`, `const` declarations
- ✅ **Data Types**: Numbers, strings, booleans, undefined
- ✅ **Operators**: Arithmetic (`+`, `-`, `*`, `/`), assignment (`=`)
- ✅ **Functions**: Function declarations, calls, anonymous functions
- ✅ **Expressions**: Binary expressions, parentheses, operator precedence
- ✅ **Console**: `console.log()` for output

### ECEngine Extensions
ECEngine extends standard JavaScript with additional design patterns:

#### Observe Pattern
```javascript
// Standard JavaScript - NOT reactive
var counter = 0;
function onCounterChange() {
    console.log("Counter: " + counter);
}
// Manual calls required: onCounterChange();

// ECEngine - Reactive with observe
var counter = 0;
observe counter function() {
    console.log("Counter: " + counter);  // Automatically called when counter changes
}
counter = 5;  // Automatically triggers the observer
```

This pattern enables:
- **Automatic Change Detection**: No manual event binding
- **Declarative Reactive Programming**: Clean, readable code
- **Multiple Observers**: Multiple functions can observe the same variable
- **Zero Configuration**: Works out of the box without setup

## 🔄 CI/CD & Automation

ECEngine uses GitHub Actions for continuous integration and deployment:

### Workflows
- **CI/CD Pipeline** (`ci.yml`): Runs tests on every push/PR across multiple .NET versions
- **Code Quality** (`quality.yml`): Performs static analysis, formatting checks, and coverage reporting  
- **Release** (`release.yml`): Automated releases with cross-platform binaries when tags are pushed

### Features
- ✅ **Multi-platform Testing**: Tests run on Ubuntu with .NET 7.0 and 8.0
- ✅ **Security Scanning**: CodeQL analysis and dependency vulnerability checks
- ✅ **Code Coverage**: Automated coverage reporting with Codecov integration
- ✅ **Automated Releases**: Cross-platform binaries (Linux, Windows, macOS) on version tags
- ✅ **Dependency Management**: Dependabot automatically updates dependencies weekly

### Release Process
To create a new release:
```bash
git tag v1.0.0
git push origin v1.0.0
```
This triggers automatic building and publishing of release artifacts.

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Quick Start
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes and add tests
4. Ensure all tests pass (`dotnet test`)
5. Submit a pull request

### Development Workflow
- **All PRs** trigger automated testing and quality checks
- **Code formatting** is enforced with `dotnet format`
- **Test coverage** is tracked and reported
- **Security scanning** runs on all commits

### Areas for Contribution
- 🚀 **ECMAScript Features**: Control flow, objects, arrays, modules
- 🔮 **Reactive Patterns**: Additional design patterns beyond observe
- 🐛 **Bug Fixes**: Parser edge cases, runtime improvements  
- 📚 **Documentation**: Component guides, usage examples, tutorials
- 🧪 **Testing**: Additional test cases, performance tests
- 🎨 **VS Code Extension**: Enhanced syntax highlighting, IntelliSense

## 🔒 Security

ECEngine is designed with security in mind:
- **No File System Access**: Cannot read/write files
- **No Network Access**: Cannot make external requests
- **Memory Safe**: Uses .NET's memory management
- **Input Validation**: Handles malicious input gracefully

For security concerns, please see our [Security Policy](SECURITY.md).

## 📄 License

MIT License - see LICENSE file for details.

