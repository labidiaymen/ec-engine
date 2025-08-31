using ECEngine.Runtime;
using Xunit;

namespace ECEngine.Tests.Interpreter;

[Collection("Console Tests")]
public class ForInOfEvaluationTests
{
    private object? ExecuteCode(string code)
    {
        var parser = new ECEngine.Parser.Parser();
        var interpreter = new ECEngine.Runtime.Interpreter();
        var ast = parser.Parse(code);
        return interpreter.Evaluate(ast, code);
    }
    
    [Fact]
    public void TestForInLoopWithObject()
    {
        // Arrange
        var code = @"
            var obj = {a: 1, b: 2, c: 3};
            var keys = [];
            for (key in obj) {
                keys.push(key);
            }
            keys;
        ";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.IsType<List<object?>>(result);
        var keys = (List<object?>)result;
        Assert.Contains("a", keys);
        Assert.Contains("b", keys);
        Assert.Contains("c", keys);
        Assert.Equal(3, keys.Count);
    }
    
    [Fact]
    public void TestForOfLoopWithArray()
    {
        // Arrange
        var code = @"
            var arr = [10, 20, 30];
            var values = [];
            for (item of arr) {
                values.push(item);
            }
            values;
        ";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.IsType<List<object?>>(result);
        var values = (List<object?>)result;
        Assert.Contains(10.0, values);
        Assert.Contains(20.0, values);
        Assert.Contains(30.0, values);
        Assert.Equal(3, values.Count);
    }
    
    [Fact]
    public void TestForInLoopWithArray()
    {
        // Arrange
        var code = @"
            var arr = ['a', 'b', 'c'];
            var indices = [];
            for (index in arr) {
                indices.push(index);
            }
            indices;
        ";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.IsType<List<object?>>(result);
        var indices = (List<object?>)result;
        Assert.Contains("0", indices);
        Assert.Contains("1", indices);
        Assert.Contains("2", indices);
        Assert.Equal(3, indices.Count);
    }
    
    [Fact]
    public void TestForOfLoopWithString()
    {
        // Arrange
        var code = @"
            var str = 'abc';
            var chars = [];
            for (char of str) {
                chars.push(char);
            }
            chars;
        ";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.IsType<List<object?>>(result);
        var chars = (List<object?>)result;
        Assert.Contains("a", chars);
        Assert.Contains("b", chars);
        Assert.Contains("c", chars);
        Assert.Equal(3, chars.Count);
    }
    
    [Fact]
    public void TestForInLoopWithString()
    {
        // Arrange
        var code = @"
            var str = 'hello';
            var indices = [];
            for (i in str) {
                indices.push(i);
            }
            indices;
        ";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.IsType<List<object?>>(result);
        var indices = (List<object?>)result;
        Assert.Contains("0", indices);
        Assert.Contains("1", indices);
        Assert.Contains("2", indices);
        Assert.Contains("3", indices);
        Assert.Contains("4", indices);
        Assert.Equal(5, indices.Count);
    }
    
    [Fact]
    public void TestForInOfWithVarDeclaration()
    {
        // Arrange
        var code = @"
            var obj = {x: 1, y: 2};
            var arr = [10, 20];
            var result = 0;
            
            for (var key in obj) {
                result += obj[key];
            }
            
            for (var item of arr) {
                result += item;
            }
            
            result;
        ";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.Equal(33.0, result); // (1 + 2) + (10 + 20) = 33
    }
    
    [Fact]
    public void TestForInOfBreakAndContinue()
    {
        // Arrange
        var code = @"
            var arr = [1, 2, 3, 4, 5];
            var result = [];
            
            for (item of arr) {
                if (item === 3) continue;
                if (item === 5) break;
                result.push(item);
            }
            
            result;
        ";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.IsType<List<object?>>(result);
        var values = (List<object?>)result;
        Assert.Contains(1.0, values);
        Assert.Contains(2.0, values);
        Assert.Contains(4.0, values);
        Assert.DoesNotContain(3.0, values);
        Assert.DoesNotContain(5.0, values);
        Assert.Equal(3, values.Count);
    }
    
    [Fact]
    public void TestNestedForInOfLoops()
    {
        // Arrange
        var code = @"
            var obj = {a: [1, 2], b: [3, 4]};
            var result = [];
            
            for (key in obj) {
                for (item of obj[key]) {
                    result.push(key + ':' + item);
                }
            }
            
            result;
        ";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.IsType<List<object?>>(result);
        var values = (List<object?>)result;
        Assert.Contains("a:1", values);
        Assert.Contains("a:2", values);
        Assert.Contains("b:3", values);
        Assert.Contains("b:4", values);
        Assert.Equal(4, values.Count);
    }
    
    [Fact]
    public void TestForInOfScopeIsolation()
    {
        // Arrange
        var code = @"
            var globalVar = 'global';
            var arr = [1, 2, 3];
            
            for (globalVar of arr) {
                // Loop variable shadows global
            }
            
            globalVar; // Should still be 'global' after loop
        ";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.Equal("global", result);
    }
    
    [Fact]
    public void TestForInOfWithEmptyCollections()
    {
        // Arrange
        var code = @"
            var emptyObj = {};
            var emptyArr = [];
            var emptyStr = '';
            var executed = false;
            
            for (key in emptyObj) {
                executed = true;
            }
            
            for (item of emptyArr) {
                executed = true;
            }
            
            for (char of emptyStr) {
                executed = true;
            }
            
            executed;
        ";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.Equal(false, result); // Loop bodies should never execute
    }
    
    [Fact]
    public void TestForInOfWithNullValues()
    {
        // Arrange
        var code = @"
            var executed = false;
            
            for (key in null) {
                executed = true;
            }
            
            for (item of null) {
                executed = true;
            }
            
            executed;
        ";
        
        // Act
        var result = ExecuteCode(code);
        
        // Assert
        Assert.Equal(false, result); // Should not execute with null
    }
}
