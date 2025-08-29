using ECEngine.Lexer;

namespace ECEngine.AST;

// Base class for all AST nodes
public abstract class ASTNode
{
    public Token? Token { get; set; }
}

// Statement node
public abstract class Statement : ASTNode { }

// Expression node
public abstract class Expression : ASTNode { }

// Number literal node
public class NumberLiteral : Expression
{
    public double Value { get; }
    public NumberLiteral(double value, Token? token = null)
    {
        Value = value;
        Token = token;
    }
}

// String literal node
public class StringLiteral : Expression
{
    public string Value { get; }
    public StringLiteral(string value, Token? token = null)
    {
        Value = value;
        Token = token;
    }
}

// Boolean literal node
public class BooleanLiteral : Expression
{
    public bool Value { get; }
    public BooleanLiteral(bool value, Token? token = null)
    {
        Value = value;
        Token = token;
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

// Function declaration node (e.g., function add(a, b) { return a + b; })
public class FunctionDeclaration : Statement
{
    public string Name { get; }
    public List<string> Parameters { get; }
    public List<Statement> Body { get; }
    
    public FunctionDeclaration(string name, List<string> parameters, List<Statement> body, Token? token = null)
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

// Observe statement node (e.g., observe x function() { console.log("x changed!"); })
public class ObserveStatement : Statement
{
    public string VariableName { get; }
    public FunctionExpression Handler { get; }
    
    public ObserveStatement(string variableName, FunctionExpression handler, Token? token = null)
    {
        VariableName = variableName;
        Handler = handler;
        Token = token;
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

// Member access expression node (e.g., changes.x.old)
public class MemberExpression : Expression
{
    public Expression Object { get; }
    public string Property { get; }
    
    public MemberExpression(Expression objectExpr, string property, Token? token = null)
    {
        Object = objectExpr;
        Property = property;
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
