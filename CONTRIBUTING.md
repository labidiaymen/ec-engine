# Contributing to ECEngine

Thank you for your interest in contributing to ECEngine! This document provides guidelines for contributing to the project.

## 🚀 Getting Started

### Prerequisites
- .NET 7.0 or later
- Git
- Text editor or IDE (VS Code, Visual Studio, Rider)

### Setup Development Environment
1. **Fork the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/ec-engine.git
   cd ec-engine
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

## 🔄 Development Workflow

### Creating a Feature Branch
```bash
git checkout -b feature/your-feature-name
```

### Making Changes
1. **Write Tests First**: Follow TDD principles
2. **Implement Feature**: Add the functionality
3. **Update Documentation**: Update relevant README files
4. **Run Tests**: Ensure all tests pass

### Committing Changes
Use conventional commit messages:
```bash
git commit -m "feat: add support for string literals"
git commit -m "fix: handle division by zero error"
git commit -m "docs: update parser documentation"
```

### Submitting Pull Request
1. Push your branch to your fork
2. Create a pull request against the `main` branch
3. Fill out the pull request template
4. Wait for review and address feedback

## 🏗️ Project Architecture

ECEngine is organized into several components:

```
ECEngine/
├── Lexer/          # Tokenization
├── Parser/         # AST construction
├── AST/           # Abstract Syntax Tree nodes
├── Runtime/       # Evaluation engine
└── Tests/         # Component tests
```

### Adding New JavaScript Features

When adding new JavaScript language features, you typically need to:

1. **Lexer**: Add new token types if needed
2. **Parser**: Add parsing rules and AST nodes
3. **Runtime**: Add evaluation logic
4. **Tests**: Add comprehensive test coverage

## 🧪 Testing Guidelines

### Test Organization
Tests are organized by component:
- `Tests/Lexer/` - Tokenization tests
- `Tests/Parser/` - Parsing tests
- `Tests/Interpreter/` - Runtime tests
- `Tests/Integration/` - End-to-end tests

### Writing Tests
```csharp
[Fact]
public void TestMethod_Condition_ExpectedResult()
{
    // Arrange
    var component = new Component();
    var input = "test input";
    
    // Act
    var result = component.Process(input);
    
    // Assert
    Assert.Equal(expectedValue, result);
}
```

### Test Coverage
- Aim for high test coverage
- Test both happy paths and error cases
- Include edge cases and boundary conditions

## 📝 Documentation Standards

### Code Documentation
- Use XML documentation comments for public APIs
- Include usage examples in complex methods
- Document error conditions and exceptions

### README Updates
When adding features, update relevant README files:
- Component README files (Lexer, Parser, AST, Runtime)
- Main project README
- Include usage examples

## 🔍 Code Style

### C# Conventions
- Use PascalCase for public members
- Use camelCase for private fields (with `_` prefix)
- Use meaningful variable and method names
- Keep methods focused and small

### Formatting
Run the formatter before committing:
```bash
dotnet format
```

## 🐛 Bug Reports

When reporting bugs:
1. Use the bug report issue template
2. Include minimal reproduction code
3. Provide expected vs actual behavior
4. Include environment details

## 💡 Feature Requests

When requesting features:
1. Use the feature request template
2. Provide JavaScript code examples
3. Explain the use case and benefits
4. Consider implementation complexity

## 🔒 Security

### Reporting Security Issues
- Do not open public issues for security vulnerabilities
- Email security concerns directly to the maintainers
- Provide detailed reproduction steps

### Security Considerations
- Validate all user inputs
- Handle malicious code gracefully
- Prevent infinite loops and resource exhaustion

## 📋 Pull Request Guidelines

### Before Submitting
- [ ] All tests pass (`dotnet test`)
- [ ] Code is formatted (`dotnet format`)
- [ ] Documentation is updated
- [ ] Commit messages follow conventions

### Review Process
1. **Automated Checks**: CI/CD pipeline runs automatically
2. **Code Review**: Maintainers review code quality and design
3. **Testing**: Manual testing of new features
4. **Merge**: Approved PRs are merged to main

## 🎯 Contribution Areas

### High Priority
- JavaScript language features (variables, functions, control flow)
- Error handling improvements
- Performance optimizations
- Documentation improvements

### Medium Priority
- Built-in object implementations (Math, String, Array)
- Additional operators and expressions
- Developer tooling improvements

### Low Priority
- Advanced language features (classes, modules)
- Browser-specific APIs
- Optimization and compilation

## 🏷️ Labels

Issues and PRs use these labels:
- `bug` - Bug fixes
- `enhancement` - New features  
- `documentation` - Documentation updates
- `good first issue` - Good for newcomers
- `help wanted` - Community help needed
- `dependencies` - Dependency updates

## 🤝 Community

### Getting Help
- Open an issue for questions
- Check existing documentation
- Review component README files

### Code of Conduct
- Be respectful and inclusive
- Focus on constructive feedback
- Help newcomers learn and contribute

## 📈 Release Process

### Versioning
We use Semantic Versioning (SemVer):
- `MAJOR.MINOR.PATCH`
- Major: Breaking changes
- Minor: New features (backward compatible)
- Patch: Bug fixes

### Release Schedule
- Patch releases: As needed for bugs
- Minor releases: Monthly for new features
- Major releases: When breaking changes are needed

Thank you for contributing to ECEngine! 🚀
