# AST (Abstract Syntax Tree) Component

The AST represents the hierarchical structure of JavaScript code after parsing. It provides a tree-based representation that captures the syntactic relationships between different language elements.

## 🎯 Purpose

The AST serves as:
- **Intermediate Representation**: Bridge between parsing and execution
- **Structural Model**: Tree representation of code structure and semantics
- **Execution Foundation**: Direct input for the interpreter's evaluation engine
- **Error Context**: Maintains source location information for debugging

## 🌳 Node Hierarchy

The AST uses a class hierarchy with all nodes inheriting from `ASTNode`:

```csharp
ASTNode                          // Abstract base class
├── ProgramNode                  // Root node containing all statements
├── Statement                    // Base class for statements
│   └── ExpressionStatement      // Statement wrapping an expression
└── Expression                   // Base class for expressions
    ├── NumberLiteral            // Numeric values: 42, 3.14, -5
    ├── Identifier               // Variable names: console, myVar, func
    ├── BinaryExpression         // Binary operations: +, -, *, /
    ├── MemberExpression         // Property access: obj.prop, console.log
    └── CallExpression           // Function calls: func(args), console.log(42)
```

## 📊 Node Types

### 1. ProgramNode
**Purpose**: Root of the AST containing all top-level statements

```csharp
public class ProgramNode : ASTNode
{
    public List<Statement> Statements { get; }
    
    public ProgramNode(List<Statement> statements)
    {
        Statements = statements;
    }
}
```

**Example**:
```javascript
console.log(42);
1 + 2;
```
Produces:
```
ProgramNode
├── ExpressionStatement(console.log(42))
└── ExpressionStatement(1 + 2)
```

### 2. ExpressionStatement
**Purpose**: Wraps expressions that are used as statements

```csharp
public class ExpressionStatement : Statement
{
    public Expression Expression { get; }
    
    public ExpressionStatement(Expression expression)
    {
        Expression = expression;
    }
}
```

### 3. NumberLiteral
**Purpose**: Represents numeric values in the code

```csharp
public class NumberLiteral : Expression
{
    public double Value { get; }
    public Token OriginalToken { get; }
    
    public NumberLiteral(double value, Token originalToken)
    {
        Value = value;
        OriginalToken = originalToken;
    }
}
```

**Examples**:
- `42` → `NumberLiteral(42.0)`
- `3.14` → `NumberLiteral(3.14)`
- `-5` → `BinaryExpression("-", NumberLiteral(0), NumberLiteral(5))`

### 4. Identifier
**Purpose**: Represents variable and function names

```csharp
public class Identifier : Expression
{
    public string Name { get; }
    public Token OriginalToken { get; }
    
    public Identifier(string name, Token originalToken)
    {
        Name = name;
        OriginalToken = originalToken;
    }
}
```

**Examples**:
- `console` → `Identifier("console")`
- `myVariable` → `Identifier("myVariable")`

### 5. BinaryExpression
**Purpose**: Represents binary operations (two operands with one operator)

```csharp
public class BinaryExpression : Expression
{
    public Expression Left { get; }
    public string Operator { get; }
    public Expression Right { get; }
    
    public BinaryExpression(Expression left, string op, Expression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
}
```

**Examples**:
- `1 + 2` → `BinaryExpression(NumberLiteral(1), "+", NumberLiteral(2))`
- `a * b` → `BinaryExpression(Identifier("a"), "*", Identifier("b"))`

**Precedence in AST**:
```javascript
1 + 2 * 3  // Parsed as: 1 + (2 * 3)
```
```
BinaryExpression("+")
├── NumberLiteral(1)
└── BinaryExpression("*")
    ├── NumberLiteral(2)
    └── NumberLiteral(3)
```

### 6. MemberExpression
**Purpose**: Represents property access (dot notation)

```csharp
public class MemberExpression : Expression
{
    public Expression Object { get; }
    public string Property { get; }
    
    public MemberExpression(Expression obj, string property)
    {
        Object = obj;
        Property = property;
    }
}
```

**Examples**:
- `console.log` → `MemberExpression(Identifier("console"), "log")`
- `obj.prop` → `MemberExpression(Identifier("obj"), "prop")`

### 7. CallExpression
**Purpose**: Represents function calls

```csharp
public class CallExpression : Expression
{
    public Expression Callee { get; }
    public List<Expression> Arguments { get; }
    
    public CallExpression(Expression callee, List<Expression> arguments)
    {
        Callee = callee;
        Arguments = arguments;
    }
}
```

**Examples**:
- `console.log(42)` → 
```
CallExpression
├── MemberExpression(console.log)
└── Arguments[NumberLiteral(42)]
```

## 🔄 AST Transformation Examples

### Simple Expression
```javascript
// Input: "42"
NumberLiteral(42.0)
```

### Complex Expression
```javascript
// Input: "console.log(1 + 2 * 3)"
ProgramNode
└── ExpressionStatement
    └── CallExpression
        ├── Callee: MemberExpression
        │   ├── Object: Identifier("console")
        │   └── Property: "log"
        └── Arguments[1]
            └── BinaryExpression("+")
                ├── Left: NumberLiteral(1.0)
                └── Right: BinaryExpression("*")
                    ├── Left: NumberLiteral(2.0)
                    └── Right: NumberLiteral(3.0)
```

### Multiple Statements
```javascript
// Input: "console.log(42); 1 + 2;"
ProgramNode
├── ExpressionStatement
│   └── CallExpression
│       ├── MemberExpression(console.log)
│       └── Arguments[NumberLiteral(42)]
└── ExpressionStatement
    └── BinaryExpression("+")
        ├── NumberLiteral(1)
        └── NumberLiteral(2)
```

## 🛠️ AST Utilities

### AST Visitor Pattern
For traversing and processing AST nodes:

```csharp
public abstract class ASTVisitor<T>
{
    public abstract T Visit(ASTNode node);
    public abstract T VisitProgram(ProgramNode node);
    public abstract T VisitExpressionStatement(ExpressionStatement node);
    public abstract T VisitNumberLiteral(NumberLiteral node);
    public abstract T VisitIdentifier(Identifier node);
    public abstract T VisitBinaryExpression(BinaryExpression node);
    public abstract T VisitMemberExpression(MemberExpression node);
    public abstract T VisitCallExpression(CallExpression node);
}
```

### AST Pretty Printer
```csharp
public static string PrintAST(ASTNode node, int indent = 0)
{
    var spaces = new string(' ', indent * 2);
    
    return node switch
    {
        ProgramNode program => 
            $"{spaces}ProgramNode\n" + 
            string.Join("\n", program.Statements.Select(s => PrintAST(s, indent + 1))),
            
        ExpressionStatement stmt => 
            $"{spaces}ExpressionStatement\n{PrintAST(stmt.Expression, indent + 1)}",
            
        NumberLiteral number => 
            $"{spaces}NumberLiteral({number.Value})",
            
        Identifier identifier => 
            $"{spaces}Identifier(\"{identifier.Name}\")",
            
        BinaryExpression binary => 
            $"{spaces}BinaryExpression(\"{binary.Operator}\")\n" +
            $"{PrintAST(binary.Left, indent + 1)}\n" +
            $"{PrintAST(binary.Right, indent + 1)}",
            
        MemberExpression member => 
            $"{spaces}MemberExpression\n" +
            $"{PrintAST(member.Object, indent + 1)}\n" +
            $"{spaces}  Property: \"{member.Property}\"",
            
        CallExpression call => 
            $"{spaces}CallExpression\n" +
            $"{PrintAST(call.Callee, indent + 1)}\n" +
            $"{spaces}  Arguments:\n" +
            string.Join("\n", call.Arguments.Select(arg => PrintAST(arg, indent + 2))),
            
        _ => $"{spaces}Unknown({node.GetType().Name})"
    };
}
```

## 📍 Source Location Tracking

Most AST nodes preserve their original tokens for error reporting:

```csharp
public abstract class Expression : ASTNode
{
    public virtual Token? OriginalToken { get; protected set; }
    
    public int Line => OriginalToken?.Line ?? 0;
    public int Column => OriginalToken?.Column ?? 0;
}
```

This enables precise error reporting:
```
Runtime Error at Line 1, Column 8: Cannot read property 'nonexistent' of undefined
Context: Property access on undefined object

Source:
>>>   1: console.nonexistent(42)
                ^
```

## 🔍 AST Analysis Examples

### Detecting Function Calls
```csharp
public List<CallExpression> FindFunctionCalls(ASTNode node)
{
    var calls = new List<CallExpression>();
    
    if (node is CallExpression call)
        calls.Add(call);
    
    // Recursively search child nodes
    foreach (var child in GetChildNodes(node))
        calls.AddRange(FindFunctionCalls(child));
        
    return calls;
}
```

### Extracting Identifiers
```csharp
public HashSet<string> ExtractIdentifiers(ASTNode node)
{
    var identifiers = new HashSet<string>();
    
    if (node is Identifier id)
        identifiers.Add(id.Name);
    
    foreach (var child in GetChildNodes(node))
        foreach (var childId in ExtractIdentifiers(child))
            identifiers.Add(childId);
            
    return identifiers;
}
```

## ✅ Design Principles

The AST design follows these principles:

- ✅ **Immutability**: Nodes are immutable after creation
- ✅ **Type Safety**: Strong typing for all node types and properties
- ✅ **Source Preservation**: Original tokens maintained for error reporting
- ✅ **Hierarchical**: Clear parent-child relationships
- ✅ **Extensible**: Easy to add new node types
- ✅ **Traversable**: Support for visitor pattern and tree walking

## 🧪 Testing

The AST is tested through parser tests that verify correct tree construction:

### Example Test
```csharp
[Fact]
public void Parse_BinaryExpression_CreatesCorrectAST()
{
    // Arrange
    var parser = new Parser();
    var input = "1 + 2";
    
    // Act
    var ast = parser.Parse(input);
    
    // Assert
    var program = Assert.IsType<ProgramNode>(ast);
    var statement = Assert.IsType<ExpressionStatement>(program.Statements[0]);
    var expression = Assert.IsType<BinaryExpression>(statement.Expression);
    
    Assert.Equal("+", expression.Operator);
    Assert.IsType<NumberLiteral>(expression.Left);
    Assert.IsType<NumberLiteral>(expression.Right);
}
```

## 🚀 Future Extensions

### Additional Statement Types
- **VariableDeclaration**: `var x = 5;`
- **FunctionDeclaration**: `function add(a, b) { return a + b; }`
- **IfStatement**: `if (condition) { ... }`
- **WhileStatement**: `while (condition) { ... }`
- **ReturnStatement**: `return value;`

### Additional Expression Types
- **AssignmentExpression**: `x = 5`, `x += 3`
- **UpdateExpression**: `x++`, `--y`
- **LogicalExpression**: `a && b`, `x || y`
- **ConditionalExpression**: `condition ? true : false`
- **ArrayExpression**: `[1, 2, 3]`
- **ObjectExpression**: `{key: value}`

### Advanced Features
- **Block Statements**: `{ ... }`
- **Function Expressions**: `function() { ... }`
- **Arrow Functions**: `(x) => x * 2`
- **Template Literals**: `` `Hello ${name}` ``

## 📚 Resources

The AST implementation provides the foundation for:
- **Static Analysis**: Code analysis without execution
- **Code Transformation**: AST manipulation and rewriting
- **Optimization**: Tree transformations for performance
- **Debugging**: Source mapping and error reporting
- **Language Tools**: Syntax highlighting, auto-completion
