using Xunit;
using ECEngine.AST;
using ECEngine.Runtime;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Interpreter;

[Collection("ConsoleTests")]
public class TypeofOperatorTests
{
    #region Basic Type Tests

    [Fact]
    public void Evaluate_TypeofNumber_ReturnsNumber()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var numberLiteral = new NumberLiteral(42);
        var typeofExpression = new UnaryExpression("typeof", numberLiteral, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof 42");

        // Assert
        Assert.Equal("number", result);
    }

    [Fact]
    public void Evaluate_TypeofString_ReturnsString()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var stringLiteral = new StringLiteral("hello");
        var typeofExpression = new UnaryExpression("typeof", stringLiteral, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof 'hello'");

        // Assert
        Assert.Equal("string", result);
    }

    [Fact]
    public void Evaluate_TypeofBoolean_ReturnsBoolean()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var booleanLiteral = new BooleanLiteral(true);
        var typeofExpression = new UnaryExpression("typeof", booleanLiteral, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof true");

        // Assert
        Assert.Equal("boolean", result);
    }

    [Fact]
    public void Evaluate_TypeofNull_ReturnsObject()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var nullLiteral = new NullLiteral();
        var typeofExpression = new UnaryExpression("typeof", nullLiteral, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof null");

        // Assert
        Assert.Equal("object", result); // JavaScript quirk: typeof null === "object"
    }

    #endregion

    #region Variable Type Tests

    [Fact]
    public void Evaluate_TypeofUndefinedVariable_ReturnsObject()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        // Declare variable without initializer (should be undefined/null)
        interpreter.Evaluate(new VariableDeclaration("var", "x", null), "var x;");
        
        var identifier = new Identifier("x");
        var typeofExpression = new UnaryExpression("typeof", identifier, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof x");

        // Assert
        Assert.Equal("object", result); // In this engine, undefined is represented as null/object
    }

    [Fact]
    public void Evaluate_TypeofVariableWithNumber_ReturnsNumber()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        interpreter.Evaluate(new VariableDeclaration("var", "num", new NumberLiteral(123)), "var num = 123;");
        
        var identifier = new Identifier("num");
        var typeofExpression = new UnaryExpression("typeof", identifier, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof num");

        // Assert
        Assert.Equal("number", result);
    }

    [Fact]
    public void Evaluate_TypeofVariableWithString_ReturnsString()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        interpreter.Evaluate(new VariableDeclaration("var", "str", new StringLiteral("test")), "var str = 'test';");
        
        var identifier = new Identifier("str");
        var typeofExpression = new UnaryExpression("typeof", identifier, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof str");

        // Assert
        Assert.Equal("string", result);
    }

    #endregion

    #region Function Type Tests

    [Fact]
    public void Evaluate_TypeofFunction_ReturnsFunction()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var funcBody = new List<Statement> { new ReturnStatement(new NumberLiteral(42)) };
        var funcDecl = new FunctionDeclaration("testFunc", new List<string>(), funcBody);
        interpreter.Evaluate(funcDecl, "function testFunc() { return 42; }");
        
        var identifier = new Identifier("testFunc");
        var typeofExpression = new UnaryExpression("typeof", identifier, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof testFunc");

        // Assert
        Assert.Equal("function", result);
    }

    #endregion

    #region Object Type Tests

    [Fact]
    public void Evaluate_TypeofObject_ReturnsObject()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var objLiteral = new ObjectLiteral(new List<ObjectProperty>());
        interpreter.Evaluate(new VariableDeclaration("var", "obj", objLiteral), "var obj = {};");
        
        var identifier = new Identifier("obj");
        var typeofExpression = new UnaryExpression("typeof", identifier, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof obj");

        // Assert
        Assert.Equal("object", result);
    }

    [Fact]
    public void Evaluate_TypeofArray_ReturnsObject()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var arrayLiteral = new ArrayLiteral(new List<Expression>());
        interpreter.Evaluate(new VariableDeclaration("var", "arr", arrayLiteral), "var arr = [];");
        
        var identifier = new Identifier("arr");
        var typeofExpression = new UnaryExpression("typeof", identifier, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof arr");

        // Assert
        Assert.Equal("object", result); // Arrays are objects in JavaScript
    }

    #endregion

    #region Constructor Type Tests

    [Fact]
    public void Evaluate_TypeofNewObject_ReturnsObject()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("Object");
        var newExpression = new NewExpression(constructor, new List<Expression>());
        interpreter.Evaluate(new VariableDeclaration("var", "newObj", newExpression), "var newObj = new Object();");
        
        var identifier = new Identifier("newObj");
        var typeofExpression = new UnaryExpression("typeof", identifier, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof newObj");

        // Assert
        Assert.Equal("object", result);
    }

    [Fact]
    public void Evaluate_TypeofNewArray_ReturnsObject()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("Array");
        var newExpression = new NewExpression(constructor, new List<Expression>());
        interpreter.Evaluate(new VariableDeclaration("var", "newArr", newExpression), "var newArr = new Array();");
        
        var identifier = new Identifier("newArr");
        var typeofExpression = new UnaryExpression("typeof", identifier, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof newArr");

        // Assert
        Assert.Equal("object", result);
    }

    [Fact]
    public void Evaluate_TypeofNewString_ReturnsString()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("String");
        var arg = new StringLiteral("hello");
        var newExpression = new NewExpression(constructor, new List<Expression> { arg });
        interpreter.Evaluate(new VariableDeclaration("var", "newStr", newExpression), "var newStr = new String('hello');");
        
        var identifier = new Identifier("newStr");
        var typeofExpression = new UnaryExpression("typeof", identifier, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof newStr");

        // Assert
        Assert.Equal("string", result); // In this engine, new String() returns a primitive string
    }

    [Fact]
    public void Evaluate_TypeofNewNumber_ReturnsNumber()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("Number");
        var arg = new NumberLiteral(42);
        var newExpression = new NewExpression(constructor, new List<Expression> { arg });
        interpreter.Evaluate(new VariableDeclaration("var", "newNum", newExpression), "var newNum = new Number(42);");
        
        var identifier = new Identifier("newNum");
        var typeofExpression = new UnaryExpression("typeof", identifier, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof newNum");

        // Assert
        Assert.Equal("number", result); // In this engine, new Number() returns a primitive number
    }

    [Fact]
    public void Evaluate_TypeofNewBoolean_ReturnsBoolean()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var constructor = new Identifier("Boolean");
        var arg = new BooleanLiteral(true);
        var newExpression = new NewExpression(constructor, new List<Expression> { arg });
        interpreter.Evaluate(new VariableDeclaration("var", "newBool", newExpression), "var newBool = new Boolean(true);");
        
        var identifier = new Identifier("newBool");
        var typeofExpression = new UnaryExpression("typeof", identifier, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof newBool");

        // Assert
        Assert.Equal("boolean", result); // In this engine, new Boolean() returns a primitive boolean
    }

    #endregion

    #region Expression Type Tests

    [Fact]
    public void Evaluate_TypeofArithmeticExpression_ReturnsNumber()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new NumberLiteral(10);
        var right = new NumberLiteral(5);
        var addExpression = new BinaryExpression(left, "+", right);
        var typeofExpression = new UnaryExpression("typeof", addExpression, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof (10 + 5)");

        // Assert
        Assert.Equal("number", result);
    }

    [Fact]
    public void Evaluate_TypeofStringConcatenation_ReturnsString()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var left = new StringLiteral("hello");
        var right = new StringLiteral("world");
        var addExpression = new BinaryExpression(left, "+", right);
        var typeofExpression = new UnaryExpression("typeof", addExpression, true);

        // Act
        var result = interpreter.Evaluate(typeofExpression, "typeof ('hello' + 'world')");

        // Assert
        Assert.Equal("string", result);
    }

    #endregion
}
