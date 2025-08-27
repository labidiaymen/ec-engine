# ECEngine VS Code Extension

A Visual Studio Code extension that provides comprehensive syntax highlighting and language support for ECEngine (`.ec`) files - a JavaScript-like programming language.

## 🚀 Features

### Syntax Highlighting
- **Keywords**: Full support for ECEngine keywords including `let`, `const`, `var`, `function`, `if`, `else`, `for`, `while`, `return`, `try`, `catch`, `finally`, `class`, `new`, `this`, and more
- **Data Types**: Highlighting for strings, numbers, booleans, null, undefined
- **Operators**: Support for arithmetic, logical, comparison, and assignment operators
- **Comments**: Single-line (`//`) and multi-line (`/* */`) comment highlighting
- **Functions**: Function declarations, expressions, and arrow functions
- **Control Flow**: Conditional statements, loops, and error handling
- **Objects and Arrays**: Proper highlighting for object literals and array syntax

### Language Features
- **Auto-completion**: IntelliSense support for ECEngine syntax
- **Bracket Matching**: Automatic bracket, brace, and parenthesis matching
- **Comment Toggling**: Easy comment/uncomment with `Cmd+/` (Mac) or `Ctrl+/` (Windows/Linux)
- **Code Folding**: Collapse and expand code blocks
- **File Icons**: Custom file icons for `.ec` files in the explorer

### Supported ECEngine Constructs
```javascript
// Variable declarations
let name = "ECEngine";
const version = 1.0;
var isActive = true;

// Functions
function greet(name) {
    return `Hello, ${name}!`;
}

// Arrow functions
const add = (a, b) => a + b;

// Control flow
if (condition) {
    console.log("True branch");
} else {
    console.log("False branch");
}

// Loops
for (let i = 0; i < 10; i++) {
    console.log(i);
}

// Objects and arrays
const person = {
    name: "John",
    age: 30,
    hobbies: ["reading", "coding"]
};
```

## 📦 Installation

### From VSIX (Local Installation)
1. Download or build the `.vsix` file
2. Open VS Code
3. Go to Extensions view (`Ctrl+Shift+X` or `Cmd+Shift+X`)
4. Click the "..." menu and select "Install from VSIX..."
5. Select the `ecengine-syntax-1.0.0.vsix` file

### Command Line Installation
```bash
code --install-extension ecengine-syntax-1.0.0.vsix
```

## 🛠️ Development

### Building the Extension
```bash
# Install dependencies
npm install

# Install VSCE (VS Code Extension CLI)
npm install -g @vscode/vsce

# Build the extension
npm run compile

# Package the extension
vsce package
```

### Project Structure
```
vscode-extension/
├── src/
│   └── extension.ts          # Main extension logic
├── syntaxes/
│   └── ecengine.tmLanguage.json  # TextMate grammar
├── fileicons/
│   ├── ec-light.svg         # Light theme file icon
│   └── ec-dark.svg          # Dark theme file icon
├── package.json             # Extension manifest
├── language-configuration.json  # Language configuration
└── README.md               # This file
```

## 📋 Requirements

- **VS Code**: Version 1.103.0 or higher
- **Node.js**: Version 20.8.0 or higher (for development)

## 🎨 Language Configuration

The extension includes comprehensive language configuration:

- **Comment Support**: 
  - Line comments: `//`
  - Block comments: `/* */`
- **Brackets**: Auto-closing and matching for `()`, `[]`, `{}`
- **Auto-closing Pairs**: Quotes, brackets, and braces
- **Surrounding Pairs**: Quick wrapping of selected text
- **Word Pattern**: Optimized for ECEngine identifiers

## 🔧 Customization

### Themes
The syntax highlighting works with all VS Code themes. For the best experience, we recommend:
- **Dark Themes**: Dark+, Monokai, Dracula
- **Light Themes**: Light+, Solarized Light

### Settings
You can customize the extension behavior in VS Code settings:

```json
{
    "files.associations": {
        "*.ec": "ecengine"
    },
    "editor.semanticHighlighting.enabled": true
}
```

## 📝 Supported File Extensions

- `.ec` - ECEngine source files

## 🐛 Known Issues

- Complex nested template literals may not highlight perfectly
- Some edge cases in regex literals might not be handled

## 🤝 Contributing

Contributions are welcome! If you find bugs or want to add features:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test the extension locally
5. Submit a pull request

## 📄 License

This extension is part of the ECEngine project. See the main project repository for license information.

## 🔗 Related Links

- [ECEngine Main Repository](https://github.com/labidiaymen/ec-engine)
- [VS Code Extension API](https://code.visualstudio.com/api)
- [TextMate Grammars](https://macromates.com/manual/en/language_grammars)

## 📊 Version History

### 1.0.0
- Initial release
- Complete syntax highlighting for ECEngine
- File icon support
- Language configuration
- Auto-completion and bracket matching

---

**Enjoy coding with ECEngine! 🚀**
