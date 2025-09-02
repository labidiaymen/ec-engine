using ECEngine.AST;
using ECEngine.Lexer;
using System.Reflection;

namespace ECEngine.Runtime;

/// <summary>
/// Loop statements and iteration control for ECEngine interpreter
/// </summary>
public partial class Interpreter
{
    /// <summary>
    /// Evaluate for statements with initialization, condition, and update
    /// </summary>
    private object? EvaluateForStatement(ForStatement forStmt)
    {
        // Push new scope for loop variables
        PushScope();
        
        try
        {
            object? result = null;
            
            // Initialize
            if (forStmt.Init != null)
            {
                Evaluate(forStmt.Init, _sourceCode);
            }
            
            // Loop with condition and update
            while (true)
            {
                // Check condition
                if (forStmt.Condition != null)
                {
                    var conditionValue = Evaluate(forStmt.Condition, _sourceCode);
                    if (!IsTruthy(conditionValue))
                        break;
                }
                
                // Execute body
                try
                {
                    result = Evaluate(forStmt.Body, _sourceCode);
                }
                catch (BreakException)
                {
                    break;
                }
                catch (ContinueException)
                {
                    // Continue to update
                }
                
                // Update
                if (forStmt.Update != null)
                {
                    Evaluate(forStmt.Update, _sourceCode);
                }
            }
            
            return result;
        }
        finally
        {
            PopScope();
        }
    }
    
    /// <summary>
    /// Evaluate while statements
    /// </summary>
    private object? EvaluateWhileStatement(WhileStatement whileStmt)
    {
        object? result = null;
        
        while (true)
        {
            var conditionValue = Evaluate(whileStmt.Condition, _sourceCode);
            if (!IsTruthy(conditionValue))
                break;
                
            try
            {
                result = Evaluate(whileStmt.Body, _sourceCode);
            }
            catch (BreakException)
            {
                break;
            }
            catch (ContinueException)
            {
                continue;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Evaluate do-while statements
    /// </summary>
    private object? EvaluateDoWhileStatement(DoWhileStatement doWhileStmt)
    {
        object? result = null;
        
        do
        {
            try
            {
                result = Evaluate(doWhileStmt.Body, _sourceCode);
            }
            catch (BreakException)
            {
                break;
            }
            catch (ContinueException)
            {
                // Continue to condition check
            }
            
            var conditionValue = Evaluate(doWhileStmt.Condition, _sourceCode);
            if (!IsTruthy(conditionValue))
                break;
                
        } while (true);
        
        return result;
    }
    
    /// <summary>
    /// Evaluate for-in statements (for iterating object properties)
    /// </summary>
    private object? EvaluateForInStatement(ForInStatement forInStmt)
    {
        var obj = Evaluate(forInStmt.Object, _sourceCode);
        
        if (obj == null)
            return null;
            
        // Push new scope for loop variable
        PushScope();
        
        try
        {
            object? result = null;
            
            // Get property names to iterate over
            IEnumerable<string> propertyNames = obj switch
            {
                Dictionary<string, object?> dict => dict.Keys,
                System.Collections.IDictionary dict => dict.Keys.Cast<object?>().Select(k => k?.ToString() ?? ""),
                List<object?> list => Enumerable.Range(0, list.Count).Select(i => i.ToString()),
                string str => Enumerable.Range(0, str.Length).Select(i => i.ToString()),
                _ => GetObjectPropertyNames(obj)
            };
            
            // Declare loop variable once in the loop scope
            DeclareVariable("var", forInStmt.Variable, null);
            
            foreach (var propertyName in propertyNames)
            {
                // Assign current property name to loop variable
                SetVariable(forInStmt.Variable, propertyName);
                
                // Execute body
                try
                {
                    result = Evaluate(forInStmt.Body, _sourceCode);
                }
                catch (BreakException)
                {
                    break;
                }
                catch (ContinueException)
                {
                    continue;
                }
            }
            
            return result;
        }
        finally
        {
            PopScope();
        }
    }
    
    /// <summary>
    /// Evaluate for-of statements (for iterating array values)
    /// </summary>
    private object? EvaluateForOfStatement(ForOfStatement forOfStmt)
    {
        var iterable = Evaluate(forOfStmt.Iterable, _sourceCode);
        
        if (iterable == null)
            return null;
            
        // Push new scope for loop variable
        PushScope();
        
        try
        {
            object? result = null;
            
            // Get values to iterate over
            IEnumerable<object?> values = iterable switch
            {
                string str => str.Select(c => c.ToString()).Cast<object?>(),
                System.Collections.IEnumerable enumerable => enumerable.Cast<object?>(),
                _ => throw new ECEngineException($"Object is not iterable",
                    forOfStmt.Token?.Line ?? 1, forOfStmt.Token?.Column ?? 1, _sourceCode,
                    "for-of can only iterate over arrays, strings, or other iterable objects")
            };
            
            // Declare loop variable once in loop scope
            DeclareVariable("var", forOfStmt.Variable, null);
            
            foreach (var value in values)
            {
                // Assign current value to loop variable
                SetVariable(forOfStmt.Variable, value);
                
                // Execute body
                try
                {
                    result = Evaluate(forOfStmt.Body, _sourceCode);
                }
                catch (BreakException)
                {
                    break;
                }
                catch (ContinueException)
                {
                    continue;
                }
            }
            
            return result;
        }
        finally
        {
            PopScope();
        }
    }
    
    /// <summary>
    /// Get property names from an object using reflection
    /// </summary>
    private IEnumerable<string> GetObjectPropertyNames(object obj)
    {
        var type = obj.GetType();
        
        // Get public properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .Select(p => p.Name);
        
        // Get public fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                         .Select(f => f.Name);
        
        return properties.Concat(fields);
    }
}
