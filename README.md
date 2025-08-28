# ECEngine

A lightweight JavaScript/TypeScript engine written in C# - a mini V8-style engine that can parse, interpret, and execute JavaScript code.

## Overview

ECEngine is designed to demonstrate the core concepts of JavaScript engine implementation, including:
- **Lexical Analysis** - Tokenizing JavaScript source code
- **Parsing** - Building Abstract Syntax Trees (AST)
- **Interpretation** - Evaluating and executing JavaScript expressions
- **Runtime** - Basic runtime environment with console support

## Project Structure

```
ECEngine/
├── AST/                 # Abstract Syntax Tree definitions
│   └── ASTNode.cs      # Base AST classes and node types
├── Lexer/              # Tokenization components
│   ├── Lexer.cs        # Main lexer implementation
│   └── Token.cs        # Token type definitions
├── Parser/             # Parsing components
│   └── Parser.cs       # Parser implementation
├── Runtime/            # Runtime environment
│   ├── Interpreter.cs  # AST evaluation engine
│   └── ConsoleRuntime.cs # Console.log implementation
├── VM/                 # Virtual Machine (future)
├── Tests/              # Unit tests
└── Program.cs          # Main entry point
```

## Features

### Current Implementation
- ✅ Basic project structure and architecture
- ✅ Lexer stub for tokenization
- ✅ Parser stub for AST generation
- ✅ Interpreter stub for evaluation
- ✅ Console runtime stub

### Planned Features
- 🔄 Complete lexer implementation (identifiers, numbers, operators)
- 🔄 Expression parser (+, -, *, /, parentheses)
- 🔄 Binary expression evaluation
- 🔄 Variable declarations and assignments
- 🔄 Function declarations and calls
- 🔄 Control flow (if/else, loops)
- 🔄 Object and array support
- 🔄 Advanced runtime features

## Getting Started

### Prerequisites
- .NET 7.0 or later
- Visual Studio, VS Code, or any C# compatible IDE

### Running the Engine

1. Clone the repository:
```bash
git clone <repository-url>
cd ec-engine
```

2. Navigate to the project directory:
```bash
cd ECEngine
```

3. Run the engine:
```bash
dotnet run
```

The engine will execute the sample JavaScript code: `console.log(1 + 2);`

### Building the Project

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

## Sample Usage

```csharp
// Sample JS code to execute
string code = "console.log(1 + 2);";

// Tokenize
var lexer = new Lexer(code);
var tokens = lexer.Tokenize();

// Parse
var parser = new Parser();
var ast = parser.Parse(code);

// Interpret
var interpreter = new Interpreter();
interpreter.Evaluate(ast);
```

## Architecture

ECEngine follows a traditional interpreter architecture:

1. **Lexer**: Converts source code into tokens
2. **Parser**: Builds an Abstract Syntax Tree from tokens
3. **Interpreter**: Walks the AST and evaluates expressions
4. **Runtime**: Provides built-in functions and environment

## Contributing

This is an educational project demonstrating JavaScript engine concepts. Contributions are welcome!

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

MIT License - see LICENSE file for details.

