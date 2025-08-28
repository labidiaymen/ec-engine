using ECEngine.Lexer;

namespace ECEngine.AST;

// Base class for AST nodes
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

// Member expression node (e.g., console.log)
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
