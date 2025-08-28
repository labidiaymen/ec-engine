# ECEngine Examples

This directory contains comprehensive examples demonstrating the various features of the ECEngine scripting language.

## Directory Structure

### 📁 variables/
Examples showing different types of variable declarations:
- `var_declarations.ec` - Using `var` keyword for variable declarations
- `let_declarations.ec` - Using `let` keyword for block-scoped variables  
- `const_declarations.ec` - Using `const` keyword for immutable variables
- `mixed_declarations.ec` - Combining different variable types

### 📁 functions/
Examples demonstrating function features:
- `basic_functions.ec` - Named function declarations and calls
- `anonymous_functions.ec` - Anonymous functions assigned to variables
- `higher_order_functions.ec` - Functions that accept other functions as parameters
- `nested_functions.ec` - Functions defined inside other functions with closures

### 📁 expressions/
Examples showing different types of expressions:
- `arithmetic.ec` - Mathematical operations (+, -, *, /)
- `strings.ec` - String literals and string handling
- `assignments.ec` - Variable assignment patterns
- `precedence.ec` - Operator precedence and parentheses

### 📁 comments/
Examples of comment syntax:
- `single_line.ec` - Single-line comments using //
- `multi_line.ec` - Multi-line comments using /* */
- `mixed_comments.ec` - Combining both comment styles

### 📁 console/
Examples of console output:
- `basic_output.ec` - Basic console.log usage
- `debugging.ec` - Using console.log for debugging
- `variable_inspection.ec` - Inspecting variable values

## Running Examples

To run any example, use the ECEngine executable:

```bash
# Run an example file
dotnet run examples/variables/var_declarations.ec

# Or use the interactive REPL
dotnet run
```

## Language Features Demonstrated

### Variables
- **var**: Function-scoped, can be reassigned, can be declared without initializer
- **let**: Block-scoped, can be reassigned, can be declared without initializer  
- **const**: Block-scoped, cannot be reassigned, must have initializer

### Functions
- **Named Functions**: `function name() { ... }`
- **Anonymous Functions**: `var fn = function() { ... }`
- **Parameters**: Functions can accept multiple parameters
- **Return Values**: Functions can return values using `return`
- **Closures**: Inner functions can access outer function variables
- **Higher-Order**: Functions can accept and return other functions

### Expressions
- **Arithmetic**: `+`, `-`, `*`, `/` with proper precedence
- **Parentheses**: `()` for grouping and precedence control
- **Assignments**: `=` for assigning values to variables
- **String Literals**: `"text"` with escape sequence support

### Comments
- **Single-line**: `// comment`
- **Multi-line**: `/* comment */`
- **Inline**: Comments can appear at end of lines or inline

### Console Output
- **console.log()**: Output values to the console
- **Multiple Arguments**: Pass multiple values to console.log
- **Debugging**: Use console.log for tracing execution

## Interactive Features

The ECEngine also provides an interactive REPL (Read-Eval-Print Loop) with commands:
- `.help` - Show available commands
- `.vars` - Show current variables
- `.clear` - Clear the console
- `.reset` - Reset the interpreter state
- `.exit` - Exit the REPL

## Example Workflow

1. Start with `variables/var_declarations.ec` to understand variable basics
2. Explore `expressions/arithmetic.ec` for mathematical operations
3. Try `functions/basic_functions.ec` for function fundamentals
4. Advance to `functions/anonymous_functions.ec` for functional programming
5. Use `console/debugging.ec` to learn debugging techniques

Each example is self-contained and can be run independently to explore specific language features.
