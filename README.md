# ECEngine

[![CI/CD Pipeline](https://github.com/labidiaymen/ec-engine/actions/workflows/ci.yml/badge.svg)](https://github.com/labidiaymen/ec-engine/actions/workflows/ci.yml)
[![Code Quality](https://github.com/labidiaymen/ec-engine/actions/workflows/quality.yml/badge.svg)](https://github.com/labidiaymen/ec-engine/actions/workflows/quality.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 7.0](https://img.shields.io/badge/.NET-7.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/7.0)

A lightweight JavaScript interpreter engine written in C# - a mini V8-style engine that can parse, interpret, and execute JavaScript code.

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
- 🚀 **JavaScript Features**: Variables, functions, control flow
- 🐛 **Bug Fixes**: Parser edge cases, runtime improvements  
- 📚 **Documentation**: Component guides, usage examples
- 🧪 **Testing**: Additional test cases, performance tests

## 🔒 Security

ECEngine is designed with security in mind:
- **No File System Access**: Cannot read/write files
- **No Network Access**: Cannot make external requests
- **Memory Safe**: Uses .NET's memory management
- **Input Validation**: Handles malicious input gracefully

For security concerns, please see our [Security Policy](SECURITY.md).

## 📄 License

MIT License - see LICENSE file for details.

