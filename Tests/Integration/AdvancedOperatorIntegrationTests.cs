using Xunit;
using ECEngine.Parser;
using System.IO;
using System;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Integration;

[Collection("ConsoleTests")]
public class AdvancedOperatorIntegrationTests
{
    private static object? ExecuteCode(string code)
    {
        var lexer = new ECEngine.Lexer.Lexer(code);
        var tokens = lexer.Tokenize();

        var parser = new ECEngine.Parser.Parser();
        var ast = parser.Parse(code);

        var interpreter = new RuntimeInterpreter();
        return interpreter.Evaluate(ast, code);
    }
    #region End-to-End Strict Comparison Tests

    [Fact]
    public void ExecuteScript_StrictEqualComparison_ProducesCorrectOutput()
    {
        // Arrange
        var script = @"
            console.log(5 === 5);
            console.log(5 === '5');
            console.log(true === true);
            console.log(null === null);
        ";

        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            ExecuteCode(script);

            // Assert
            var output = stringWriter.ToString();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            Assert.Equal("True", lines[0].Trim());  // 5 === 5
            Assert.Equal("False", lines[1].Trim()); // 5 === '5'
            Assert.Equal("True", lines[2].Trim());  // true === true
            Assert.Equal("True", lines[3].Trim());  // null === null
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    [Fact]
    public void ExecuteScript_StrictNotEqualComparison_ProducesCorrectOutput()
    {
        // Arrange
        var script = @"
            console.log(5 !== '5');
            console.log(5 !== 5);
            console.log(true !== false);
        ";

        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            ExecuteCode(script);

            // Assert
            var output = stringWriter.ToString();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            Assert.Equal("True", lines[0].Trim());  // 5 !== '5'
            Assert.Equal("False", lines[1].Trim()); // 5 !== 5
            Assert.Equal("True", lines[2].Trim());  // true !== false
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    #endregion

    #region End-to-End Compound Assignment Tests

    [Fact]
    public void ExecuteScript_CompoundAssignmentOperators_ProducesCorrectOutput()
    {
        // Arrange
        var script = @"
            var x = 10;
            console.log('Initial:', x);
            
            x += 5;
            console.log('After +=:', x);
            
            x -= 3;
            console.log('After -=:', x);
            
            x *= 2;
            console.log('After *=:', x);
            
            x /= 4;
            console.log('After /=:', x);
        ";

        
        
        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            ExecuteCode(script);

            // Assert
            var output = stringWriter.ToString();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            Assert.Contains("Initial:", lines[0]);
            Assert.Contains("10", lines[1]);
            Assert.Contains("After +=:", lines[2]);
            Assert.Contains("15", lines[3]);
            Assert.Contains("After -=:", lines[4]);
            Assert.Contains("12", lines[5]);
            Assert.Contains("After *=:", lines[6]);
            Assert.Contains("24", lines[7]);
            Assert.Contains("After /=:", lines[8]);
            Assert.Contains("6", lines[9]);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    [Fact]
    public void ExecuteScript_StringConcatenationWithCompoundAssignment_ProducesCorrectOutput()
    {
        // Arrange
        var script = @"
            var str = 'Hello';
            console.log('Initial:', str);
            
            str += ' World';
            console.log('After +=:', str);
            
            str += '!';
            console.log('Final:', str);
        ";

        
        
        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            ExecuteCode(script);

            // Assert
            var output = stringWriter.ToString();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            Assert.Contains("Initial:", lines[0]);
            Assert.Contains("Hello", lines[1]);
            Assert.Contains("After +=:", lines[2]);
            Assert.Contains("Hello World", lines[3]);
            Assert.Contains("Final:", lines[4]);
            Assert.Contains("Hello World!", lines[5]);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    #endregion

    #region End-to-End Bitwise Operator Tests

    [Fact]
    public void ExecuteScript_BitwiseOperators_ProducesCorrectOutput()
    {
        // Arrange
        var script = @"
            var a = 12;  // 1100 in binary
            var b = 10;  // 1010 in binary
            
            console.log('a & b:', a & b);  // 8 (1000)
            console.log('a | b:', a | b);  // 14 (1110)
            console.log('a ^ b:', a ^ b);  // 6 (0110)
            console.log('~a:', ~a);        // -13
        ";

        
        
        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            ExecuteCode(script);

            // Assert
            var output = stringWriter.ToString();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            Assert.Contains("a & b:", lines[0]);
            Assert.Contains("8", lines[1]);
            Assert.Contains("a | b:", lines[2]);
            Assert.Contains("14", lines[3]);
            Assert.Contains("a ^ b:", lines[4]);
            Assert.Contains("6", lines[5]);
            Assert.Contains("~a:", lines[6]);
            Assert.Contains("-13", lines[7]);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    [Fact]
    public void ExecuteScript_BitShiftOperators_ProducesCorrectOutput()
    {
        // Arrange
        var script = @"
            var num = 8;
            
            console.log('num << 2:', num << 2);    // 32
            console.log('num >> 1:', num >> 1);    // 4
            console.log('num >>> 1:', num >>> 1);  // 4
        ";

        
        
        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            ExecuteCode(script);

            // Assert
            var output = stringWriter.ToString();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            Assert.Contains("num << 2:", lines[0]);
            Assert.Contains("32", lines[1]);
            Assert.Contains("num >> 1:", lines[2]);
            Assert.Contains("4", lines[3]);
            Assert.Contains("num >>> 1:", lines[4]);
            Assert.Contains("4", lines[5]);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    #endregion

    #region End-to-End Ternary Operator Tests

    [Fact]
    public void ExecuteScript_TernaryOperator_ProducesCorrectOutput()
    {
        // Arrange
        var script = @"
            var age = 25;
            var status = age >= 18 ? 'adult' : 'minor';
            console.log('Status:', status);
            
            var score = 85;
            var grade = score >= 90 ? 'A' : score >= 80 ? 'B' : score >= 70 ? 'C' : 'F';
            console.log('Grade:', grade);
            
            var isLoggedIn = true;
            var message = isLoggedIn ? 'Welcome!' : null;
            console.log('Message:', message);
        ";

        
        
        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            ExecuteCode(script);

            // Assert
            var output = stringWriter.ToString();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            Assert.Contains("Status:", lines[0]);
            Assert.Contains("adult", lines[1]);
            Assert.Contains("Grade:", lines[2]);
            Assert.Contains("B", lines[3]);
            Assert.Contains("Message:", lines[4]);
            Assert.Contains("Welcome!", lines[5]);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    #endregion

    #region End-to-End Mixed Operator Tests

    [Fact]
    public void ExecuteScript_CompoundAssignmentWithTernary_ProducesCorrectOutput()
    {
        // Arrange
        var script = @"
            var counter = 0;
            var increment = true;
            
            console.log('Initial counter:', counter);
            
            counter += increment ? 1 : -1;
            console.log('After increment:', counter);
            
            increment = false;
            counter += increment ? 1 : -1;
            console.log('After decrement:', counter);
        ";

        
        
        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            ExecuteCode(script);

            // Assert
            var output = stringWriter.ToString();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            Assert.Contains("Initial counter:", lines[0]);
            Assert.Contains("0", lines[1]);
            Assert.Contains("After increment:", lines[2]);
            Assert.Contains("1", lines[3]);
            Assert.Contains("After decrement:", lines[4]);
            Assert.Contains("0", lines[5]);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    [Fact]
    public void ExecuteScript_ComplexExpressionWithAllOperators_ProducesCorrectOutput()
    {
        // Arrange
        var script = @"
            // Test complex expression with all new operators
            var result = (5 === 5) ? (10 | 5) + (3 << 1) : ~0;
            console.log('Complex result:', result);
            
            // Test bitwise flag checking
            var flags = 7;  // 111 in binary
            var hasFlag = (flags & 4) !== 0 ? 'yes' : 'no';
            console.log('Has flag 4:', hasFlag);
            
            // Test strict vs regular comparison
            var num = 0;
            var str = '0';
            console.log('Regular comparison:', num == str);
            console.log('Strict comparison:', num === str);
        ";

        
        
        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            ExecuteCode(script);

            // Assert
            var output = stringWriter.ToString();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            Assert.Contains("Complex result:", lines[0]);
            Assert.Contains("21", lines[1]);
            Assert.Contains("Has flag 4:", lines[2]);
            Assert.Contains("yes", lines[3]);
            Assert.Contains("Regular comparison:", lines[4]);
            Assert.Contains("True", lines[5]);
            Assert.Contains("Strict comparison:", lines[6]);
            Assert.Contains("False", lines[7]);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void ExecuteScript_DivideByZeroInCompoundAssignment_ReturnsInfinity()
    {
        // Arrange
        var script = @"
            var x = 10;
            x /= 0;
            console.log('Result:', x);
        ";

        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            ExecuteCode(script);

            // Assert
            var output = stringWriter.ToString();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            Assert.Contains("Result:", lines[0]);
            Assert.Contains("∞", lines[1]);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    [Fact]
    public void ExecuteScript_CompoundAssignmentToUndefinedVariable_ThrowsException()
    {
        // Arrange
        var script = @"
            undefinedVar += 5;
        ";

        // Act & Assert
        Assert.Throws<ECEngine.Runtime.ECEngineException>(() => ExecuteCode(script));
    }

    #endregion

    #region Performance and Edge Case Tests

    [Theory]
    [InlineData("var x = 0; x += 1; x += 1; x += 1; console.log(x);", "3")]
    [InlineData("var result = true ? 'yes' : 'no'; console.log(result);", "yes")]
    [InlineData("var result = false ? 'yes' : 'no'; console.log(result);", "no")]
    [InlineData("var result = 15 & 7; console.log(result);", "7")]
    [InlineData("var result = 8 << 1; console.log(result);", "16")]
    [InlineData("console.log(null === null);", "True")]
    [InlineData("console.log(5 !== '5');", "True")]
    public void ExecuteScript_VariousAdvancedOperatorScenarios_ProducesExpectedOutput(string script, string expectedOutput)
    {
        // Arrange
        
        
        var originalConsoleOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            ExecuteCode(script);

            // Assert
            var output = stringWriter.ToString().Trim();
            Assert.Contains(expectedOutput, output);
        }
        finally
        {
            Console.SetOut(originalConsoleOut);
        }
    }

    #endregion
}
