using ECEngine.Lexer;

namespace ECEngine.AST;

// Base class for all AST nodes
public abstract class ASTNode
{
    public Token? Token { get; set; }
    
    // Virtual dispatch method for performance optimization
    // Default implementation falls back to the legacy evaluation system
    public virtual object? Accept(ECEngine.Runtime.Interpreter interpreter)
    {
        return interpreter.EvaluateLegacy(this);
    }
}

// Statement node
public abstract class Statement : ASTNode { }

// Expression node
public abstract class Expression : ASTNode { }

// Number literal node - Optimized for direct value return
public class NumberLiteral : Expression
{
    public double Value { get; }
    public NumberLiteral(double value, Token? token = null)
    {
        Value = value;
        Token = token;
    }
    
    // Fast path optimization - direct value return
    public override object? Accept(ECEngine.Runtime.Interpreter interpreter)
    {
        return Value;
    }
}

// String literal node - Optimized for direct value return
public class StringLiteral : Expression
{
    public string Value { get; }
    public StringLiteral(string value, Token? token = null)
    {
        Value = value;
        Token = token;
    }
    
    // Fast path optimization - direct value return
    public override object? Accept(ECEngine.Runtime.Interpreter interpreter)
    {
        return Value;
    }
}

// Boolean literal node - Optimized for direct value return
public class BooleanLiteral : Expression
{
    public bool Value { get; }
    public BooleanLiteral(bool value, Token? token = null)
    {
        Value = value;
        Token = token;
    }
    
    // Fast path optimization - direct value return
    public override object? Accept(ECEngine.Runtime.Interpreter interpreter)
    {
        return Value;
    }
}

// Null literal node
public class NullLiteral : Expression
{
    public NullLiteral(Token? token = null)
    {
        Token = token;
    }
    
    // Fast path optimization - direct null return
    public override object? Accept(ECEngine.Runtime.Interpreter interpreter)
    {
        return null;
    }
}

// This expression node
public class ThisExpression : Expression
{
    public ThisExpression(Token? token = null)
    {
        Token = token;
    }
}

// Object literal node  
public class ObjectLiteral : Expression
{
    public List<ObjectProperty> Properties { get; }
    public ObjectLiteral(List<ObjectProperty> properties, Token? token = null)
    {
        Properties = properties;
        Token = token;
    }
}

// Object property for object literals
public class ObjectProperty : ASTNode
{
    public string Key { get; }
    public Expression Value { get; }
    public ObjectProperty(string key, Expression value, Token? token = null)
    {
        Key = key;
        Value = value;
        Token = token;
    }
}

// Array literal node
public class ArrayLiteral : Expression
{
    public List<Expression> Elements { get; }
    public ArrayLiteral(List<Expression> elements, Token? token = null)
    {
        Elements = elements;
        Token = token;
    }
}

// Template literal node
public class TemplateLiteral : Expression
{
    public List<TemplateElement> Elements { get; }
    public TemplateLiteral(List<TemplateElement> elements, Token? token = null)
    {
        Elements = elements;
        Token = token;
    }
}

// Template element (either text or expression)
public abstract class TemplateElement : ASTNode
{
    protected TemplateElement(Token? token = null)
    {
        Token = token;
    }
}

// Template text element  
public class TemplateText : TemplateElement
{
    public string Value { get; }
    public TemplateText(string value, Token? token = null) : base(token)
    {
        Value = value;
    }
}

// Template expression element
public class TemplateExpression : TemplateElement
{
    public Expression Expression { get; }
    public TemplateExpression(Expression expression, Token? token = null) : base(token)
    {
        Expression = expression;
    }
}

// Identifier node
public class Identifier : Expression
{
    public string Name { get; }
    public Identifier(string name, Token? token = null)
    {
        Name = name;
        Token = token;
    }
    
    // Delegate to interpreter's optimized identifier evaluation
    public override object? Accept(ECEngine.Runtime.Interpreter interpreter)
    {
        return interpreter.EvaluateIdentifierOptimized(this);
    }
}

// Binary expression node
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
    
    // Delegate to interpreter's optimized binary expression evaluation
    public override object? Accept(ECEngine.Runtime.Interpreter interpreter)
    {
        return interpreter.EvaluateBinaryExpressionOptimized(this);
    }
}

// Unary expression node (e.g., !x, ++x, --x, +x, -x)
public class UnaryExpression : Expression
{
    public string Operator { get; }
    public Expression Operand { get; }
    public bool IsPrefix { get; }  // true for ++x, false for x++
    
    public UnaryExpression(string op, Expression operand, bool isPrefix = true, Token? token = null)
    {
        Operator = op;
        Operand = operand;
        IsPrefix = isPrefix;
        Token = token;
    }
}

// Conditional expression node (ternary operator: condition ? trueExpr : falseExpr)
public class ConditionalExpression : Expression
{
    public Expression Test { get; }
    public Expression Consequent { get; }
    public Expression Alternate { get; }
    
    public ConditionalExpression(Expression test, Expression consequent, Expression alternate, Token? token = null)
    {
        Test = test;
        Consequent = consequent;
        Alternate = alternate;
        Token = token;
    }
}

// Call expression node (e.g., function calls)
public class CallExpression : Expression
{
    public Expression Callee { get; }
    public List<Expression> Arguments { get; }
    public CallExpression(Expression callee, List<Expression> arguments)
    {
        Callee = callee;
        Arguments = arguments;
    }
    
    // Delegate to interpreter's optimized call expression evaluation
    public override object? Accept(ECEngine.Runtime.Interpreter interpreter)
    {
        return interpreter.EvaluateCallExpressionOptimized(this);
    }
}

// Expression statement node
public class ExpressionStatement : Statement
{
    public Expression Expression { get; }
    public ExpressionStatement(Expression expression) => Expression = expression;
}

// Program node (root of AST)
public class ProgramNode : ASTNode
{
    public List<Statement> Body { get; }
    public ProgramNode(List<Statement> body) => Body = body;
}

// Variable declaration statement node (e.g., var x = 5;)
public class VariableDeclaration : Statement
{
    public string Kind { get; }  // "var", "let", or "const"
    public string Name { get; }
    public Expression? Initializer { get; }
    
    public VariableDeclaration(string kind, string name, Expression? initializer = null, Token? token = null)
    {
        Kind = kind;
        Name = name;
        Initializer = initializer;
        Token = token;
    }
}

// Assignment expression node (e.g., x = 5)
public class AssignmentExpression : Expression
{
    public Identifier Left { get; }
    public Expression Right { get; }
    
    public AssignmentExpression(Identifier left, Expression right, Token? token = null)
    {
        Left = left;
        Right = right;
        Token = token;
    }
}

// Compound assignment expression node (e.g., x += 5, y *= 2)
public class CompoundAssignmentExpression : Expression
{
    public Identifier Left { get; }
    public string Operator { get; }  // +=, -=, *=, /=
    public Expression Right { get; }
    
    public CompoundAssignmentExpression(Identifier left, string op, Expression right, Token? token = null)
    {
        Left = left;
        Operator = op;
        Right = right;
        Token = token;
    }
}

// Member assignment expression node (e.g., obj.prop = value, this.x = 10)
public class MemberAssignmentExpression : Expression
{
    public MemberExpression Left { get; }
    public Expression Right { get; }
    
    public MemberAssignmentExpression(MemberExpression left, Expression right, Token? token = null)
    {
        Left = left;
        Right = right;
        Token = token;
    }
}

// Function declaration node (e.g., function add(a, b) { return a + b; })
public class FunctionDeclaration : Statement
{
    public string? Name { get; }
    public List<string> Parameters { get; }
    public List<Statement> Body { get; }
    
    public FunctionDeclaration(string? name, List<string> parameters, List<Statement> body, Token? token = null)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
        Token = token;
    }
}

// Function expression node (e.g., function(a, b) { return a + b; })
public class FunctionExpression : Expression
{
    public List<string> Parameters { get; }
    public List<Statement> Body { get; }
    
    public FunctionExpression(List<string> parameters, List<Statement> body, Token? token = null)
    {
        Parameters = parameters;
        Body = body;
        Token = token;
    }
}

// Arrow function expression node (e.g., (a, b) => a + b, x => x * 2)
public class ArrowFunctionExpression : Expression
{
    public List<string> Parameters { get; }
    public Expression? Body { get; } // For expression body
    public List<Statement>? BlockBody { get; } // For block body
    public bool IsExpressionBody { get; }
    
    // Constructor for expression body (e.g., x => x * 2)
    public ArrowFunctionExpression(List<string> parameters, Expression body, Token? token = null)
    {
        Parameters = parameters;
        Body = body;
        BlockBody = null;
        IsExpressionBody = true;
        Token = token;
    }
    
    // Constructor for block body (e.g., x => { return x * 2; })
    public ArrowFunctionExpression(List<string> parameters, List<Statement> blockBody, Token? token = null)
    {
        Parameters = parameters;
        Body = null;
        BlockBody = blockBody;
        IsExpressionBody = false;
        Token = token;
    }
}

// Return statement node (e.g., return x + y;)
public class ReturnStatement : Statement
{
    public Expression? Argument { get; }
    
    public ReturnStatement(Expression? argument = null, Token? token = null)
    {
        Argument = argument;
        Token = token;
    }
}

// Yield statement node (e.g., yield x + y;)
public class YieldStatement : Statement
{
    public Expression? Argument { get; }
    
    public YieldStatement(Expression? argument = null, Token? token = null)
    {
        Argument = argument;
        Token = token;
    }
}

// Generator function declaration node (e.g., function* generate() { yield 1; })
public class GeneratorFunctionDeclaration : Statement
{
    public string? Name { get; }
    public List<string> Parameters { get; }
    public List<Statement> Body { get; }
    
    public GeneratorFunctionDeclaration(string? name, List<string> parameters, List<Statement> body, Token? token = null)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
        Token = token;
    }
}

// Generator function expression node (e.g., function*() { yield 1; })
public class GeneratorFunctionExpression : Expression
{
    public List<string> Parameters { get; }
    public List<Statement> Body { get; }
    
    public GeneratorFunctionExpression(List<string> parameters, List<Statement> body, Token? token = null)
    {
        Parameters = parameters;
        Body = body;
        Token = token;
    }
}

// Block statement node (e.g., { statement1; statement2; })
public class BlockStatement : Statement
{
    public List<Statement> Body { get; }
    
    public BlockStatement(List<Statement> body, Token? token = null)
    {
        Body = body;
        Token = token;
    }
}

// Observe statement node (e.g., observe x function() { console.log("x changed!"); } or observe server.requests function(ev) { ... })
public class ObserveStatement : Statement
{
    public Expression Target { get; }  // Can be Identifier or MemberExpression
    public FunctionExpression Handler { get; }
    
    // Legacy property for backward compatibility
    public string VariableName => Target is Identifier id ? id.Name : Target.ToString() ?? "";
    
    public ObserveStatement(Expression target, FunctionExpression handler, Token? token = null)
    {
        Target = target;
        Handler = handler;
        Token = token;
    }
    
    // Legacy constructor for backward compatibility
    public ObserveStatement(string variableName, FunctionExpression handler, Token? token = null) 
        : this(new Identifier(variableName), handler, token)
    {
    }
}

// Multi-variable observe statement node (e.g., observe (x, y) function(changes) { ... })
public class MultiObserveStatement : Statement
{
    public List<string> VariableNames { get; }
    public FunctionExpression Handler { get; }
    
    public MultiObserveStatement(List<string> variableNames, FunctionExpression handler, Token? token = null)
    {
        VariableNames = variableNames;
        Handler = handler;
        Token = token;
    }
}

// If statement node (e.g., if (condition) { ... } else { ... })
public class IfStatement : Statement
{
    public Expression Condition { get; }
    public Statement ThenStatement { get; }
    public Statement? ElseStatement { get; }
    
    public IfStatement(Expression condition, Statement thenStatement, Statement? elseStatement = null, Token? token = null)
    {
        Condition = condition;
        ThenStatement = thenStatement;
        ElseStatement = elseStatement;
        Token = token;
    }
}

// When statement node (e.g., when x { ... })
public class WhenStatement : Statement
{
    public Expression Condition { get; }
    public BlockStatement Body { get; }
    
    public WhenStatement(Expression condition, BlockStatement body, Token? token = null)
    {
        Condition = condition;
        Body = body;
        Token = token;
    }
}

// Otherwise statement node (e.g., otherwise { ... })
public class OtherwiseStatement : Statement
{
    public BlockStatement Body { get; }
    
    public OtherwiseStatement(BlockStatement body, Token? token = null)
    {
        Body = body;
        Token = token;
    }
}

// Member access expression node (e.g., obj.prop or obj[key])
public class MemberExpression : Expression
{
    public Expression Object { get; }
    public string Property { get; }
    public Expression? ComputedProperty { get; }
    public bool Computed { get; }
    
    // For dot notation: obj.prop
    public MemberExpression(Expression objectExpr, string property, Token? token = null)
    {
        Object = objectExpr;
        Property = property;
        ComputedProperty = null;
        Computed = false;
        Token = token;
    }
    
    // For bracket notation: obj[key]
    public MemberExpression(Expression objectExpr, Expression property, Token? token = null)
    {
        Object = objectExpr;
        Property = "";
        ComputedProperty = property;
        Computed = true;
        Token = token;
    }
}

// Logical expression node (e.g., x && y)
public class LogicalExpression : Expression
{
    public Expression Left { get; }
    public string Operator { get; }
    public Expression Right { get; }
    
    public LogicalExpression(Expression left, string op, Expression right, Token? token = null)
    {
        Left = left;
        Operator = op;
        Right = right;
        Token = token;
    }
}

// Export statement node
public class ExportStatement : Statement
{
    public Statement Declaration { get; }
    
    public ExportStatement(Statement declaration, Token? token = null)
    {
        Declaration = declaration;
        Token = token;
    }
}

// Import statement node
public class ImportStatement : Statement
{
    public List<string> ImportedNames { get; }
    public string ModulePath { get; }
    public string? DefaultImportName { get; set; } // For default imports like: import name from "module"
    public string? NamespaceImportName { get; set; } // For namespace imports like: import * as name from "module"
    public Dictionary<string, string> ImportAliases { get; set; } = new(); // For renamed imports like: import { foo as bar } from "module"
    public bool IsNamespaceImport { get; set; } = false; // True for import * as name
    
    public ImportStatement(List<string> importedNames, string modulePath, Token? token = null)
    {
        ImportedNames = importedNames;
        ModulePath = modulePath;
        Token = token;
    }
}

// Dynamic import expression node
public class DynamicImportExpression : Expression
{
    public Expression ModulePath { get; }
    
    public DynamicImportExpression(Expression modulePath, Token? token = null)
    {
        ModulePath = modulePath;
        Token = token;
    }
}

// Default export statement node
public class DefaultExportStatement : Statement
{
    public ASTNode Value { get; }  // Can be a function, class, expression, etc.
    
    public DefaultExportStatement(ASTNode value, Token? token = null)
    {
        Value = value;
        Token = token;
    }
}

// Named export with optional renaming
public class NamedExport
{
    public string LocalName { get; }
    public string? ExportName { get; }  // If null, same as LocalName
    
    public NamedExport(string localName, string? exportName = null)
    {
        LocalName = localName;
        ExportName = exportName;
    }
}

// Re-export statement node (export { name } from "./module")
public class ReExportStatement : Statement
{
    public List<NamedExport> ExportedNames { get; }
    public string ModulePath { get; }
    
    public ReExportStatement(List<NamedExport> exportedNames, string modulePath, Token? token = null)
    {
        ExportedNames = exportedNames;
        ModulePath = modulePath;
        Token = token;
    }
}

// Named export statement node (export { name as newName })
public class NamedExportStatement : Statement
{
    public List<NamedExport> ExportedNames { get; }
    
    public NamedExportStatement(List<NamedExport> exportedNames, Token? token = null)
    {
        ExportedNames = exportedNames;
        Token = token;
    }
}

// For loop statement node
public class ForStatement : Statement
{
    public Statement? Init { get; }       // for (init; condition; update)
    public Expression? Condition { get; }
    public Expression? Update { get; }
    public Statement Body { get; }
    
    public ForStatement(Statement? init, Expression? condition, Expression? update, Statement body, Token? token = null)
    {
        Init = init;
        Condition = condition;
        Update = update;
        Body = body;
        Token = token;
    }
}

// For...in loop statement node (e.g., for (key in object) { ... })
public class ForInStatement : Statement
{
    public string Variable { get; }      // The iteration variable name
    public Expression Object { get; }    // The object to iterate over
    public Statement Body { get; }       // The loop body
    
    public ForInStatement(string variable, Expression obj, Statement body, Token? token = null)
    {
        Variable = variable;
        Object = obj;
        Body = body;
        Token = token;
    }
}

// For...of loop statement node (e.g., for (item of iterable) { ... })
public class ForOfStatement : Statement
{
    public string Variable { get; }      // The iteration variable name
    public Expression Iterable { get; }  // The iterable to iterate over
    public Statement Body { get; }       // The loop body
    
    public ForOfStatement(string variable, Expression iterable, Statement body, Token? token = null)
    {
        Variable = variable;
        Iterable = iterable;
        Body = body;
        Token = token;
    }
}

// While loop statement node
public class WhileStatement : Statement
{
    public Expression Condition { get; }
    public Statement Body { get; }
    
    public WhileStatement(Expression condition, Statement body, Token? token = null)
    {
        Condition = condition;
        Body = body;
        Token = token;
    }
}

// Do-while loop statement node
public class DoWhileStatement : Statement
{
    public Statement Body { get; }
    public Expression Condition { get; }
    
    public DoWhileStatement(Statement body, Expression condition, Token? token = null)
    {
        Body = body;
        Condition = condition;
        Token = token;
    }
}

// Break statement node
public class BreakStatement : Statement
{
    public BreakStatement(Token? token = null)
    {
        Token = token;
    }
}

// Continue statement node
public class ContinueStatement : Statement
{
    public ContinueStatement(Token? token = null)
    {
        Token = token;
    }
}

// Switch statement node
public class SwitchStatement : Statement
{
    public Expression Discriminant { get; }
    public List<SwitchCase> Cases { get; }
    
    public SwitchStatement(Expression discriminant, List<SwitchCase> cases, Token? token = null)
    {
        Discriminant = discriminant;
        Cases = cases;
        Token = token;
    }
}

// Switch case node (includes both case and default)
public class SwitchCase : ASTNode
{
    public Expression? Test { get; }  // null for default case
    public List<Statement> Consequent { get; }
    
    public SwitchCase(Expression? test, List<Statement> consequent, Token? token = null)
    {
        Test = test;
        Consequent = consequent;
        Token = token;
    }
}

// Try statement node
public class TryStatement : Statement
{
    public BlockStatement Block { get; }
    public CatchClause? Handler { get; }
    public BlockStatement? Finalizer { get; }
    
    public TryStatement(BlockStatement block, CatchClause? handler = null, BlockStatement? finalizer = null, Token? token = null)
    {
        Block = block;
        Handler = handler;
        Finalizer = finalizer;
        Token = token;
    }
}

// Catch clause node
public class CatchClause : ASTNode
{
    public Identifier? Param { get; }  // catch parameter (e.g., 'e' in catch(e))
    public BlockStatement Body { get; }
    
    public CatchClause(BlockStatement body, Identifier? param = null, Token? token = null)
    {
        Param = param;
        Body = body;
        Token = token;
    }
}

// Throw statement node
public class ThrowStatement : Statement
{
    public Expression Argument { get; }
    
    public ThrowStatement(Expression argument, Token? token = null)
    {
        Argument = argument;
        Token = token;
    }
}
