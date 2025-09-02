using ECEngine.AST;
using ECEngine.Lexer;

namespace ECEngine.Runtime;

/// <summary>
/// Control flow statements including conditionals, switch, try-catch, and when/otherwise for ECEngine interpreter
/// </summary>
public partial class Interpreter
{
    /// <summary>
    /// Evaluate conditional (ternary) expressions (test ? consequent : alternate)
    /// </summary>
    private object? EvaluateConditionalExpression(ConditionalExpression conditional)
    {
        var testValue = Evaluate(conditional.Test, _sourceCode);
        
        if (IsTruthy(testValue))
        {
            return Evaluate(conditional.Consequent, _sourceCode);
        }
        else
        {
            return Evaluate(conditional.Alternate, _sourceCode);
        }
    }

    /// <summary>
    /// Evaluate if statements with optional else clause
    /// </summary>
    private object? EvaluateIfStatement(IfStatement ifStmt)
    {
        var conditionValue = Evaluate(ifStmt.Condition);
        
        // Check if condition is truthy
        if (IsTruthy(conditionValue))
        {
            return Evaluate(ifStmt.ThenStatement);
        }
        else if (ifStmt.ElseStatement != null)
        {
            return Evaluate(ifStmt.ElseStatement);
        }
        
        return null; // if statement with false condition and no else returns null
    }

    /// <summary>
    /// Evaluate switch statements with case matching and fall-through behavior
    /// </summary>
    private object? EvaluateSwitchStatement(SwitchStatement switchStmt)
    {
        var discriminantValue = Evaluate(switchStmt.Discriminant, _sourceCode);
        bool foundMatch = false;
        bool executeDefault = false;
        object? result = null;
        
        try
        {
            foreach (var switchCase in switchStmt.Cases)
            {
                if (switchCase.Test == null) // default case
                {
                    if (!foundMatch)
                    {
                        executeDefault = true;
                    }
                    
                    if (foundMatch || executeDefault)
                    {
                        foreach (var stmt in switchCase.Consequent)
                        {
                            result = Evaluate(stmt, _sourceCode);
                        }
                    }
                }
                else // case with test value
                {
                    var testValue = Evaluate(switchCase.Test, _sourceCode);
                    
                    if (!foundMatch && IsEqual(discriminantValue, testValue))
                    {
                        foundMatch = true;
                    }
                    
                    if (foundMatch)
                    {
                        foreach (var stmt in switchCase.Consequent)
                        {
                            result = Evaluate(stmt, _sourceCode);
                        }
                    }
                }
            }
        }
        catch (BreakException)
        {
            // Break out of switch statement
        }
        
        return result;
    }
    
    /// <summary>
    /// Evaluate try-catch-finally statements
    /// </summary>
    private object? EvaluateTryStatement(TryStatement tryStmt)
    {
        object? result = null;
        
        try
        {
            result = Evaluate(tryStmt.Block, _sourceCode);
        }
        catch (ECEngineException ex) when (tryStmt.Handler != null)
        {
            // Bind exception to catch parameter if provided
            if (tryStmt.Handler.Param != null)
            {
                var errorInfo = new VariableInfo("var", ex.Message);
                var originalValue = _variables.ContainsKey(tryStmt.Handler.Param.Name) ? 
                    _variables[tryStmt.Handler.Param.Name] : null;
                
                _variables[tryStmt.Handler.Param.Name] = errorInfo;
                
                try
                {
                    result = Evaluate(tryStmt.Handler.Body, _sourceCode);
                }
                finally
                {
                    // Restore original value or remove if it didn't exist
                    if (originalValue != null)
                        _variables[tryStmt.Handler.Param.Name] = originalValue;
                    else
                        _variables.Remove(tryStmt.Handler.Param.Name);
                }
            }
            else
            {
                result = Evaluate(tryStmt.Handler.Body, _sourceCode);
            }
        }
        catch (Exception ex) when (tryStmt.Handler != null)
        {
            // Handle other .NET exceptions
            if (tryStmt.Handler.Param != null)
            {
                var errorInfo = new VariableInfo("var", ex.Message);
                var originalValue = _variables.ContainsKey(tryStmt.Handler.Param.Name) ? 
                    _variables[tryStmt.Handler.Param.Name] : null;
                
                _variables[tryStmt.Handler.Param.Name] = errorInfo;
                
                try
                {
                    result = Evaluate(tryStmt.Handler.Body, _sourceCode);
                }
                finally
                {
                    // Restore original value or remove if it didn't exist
                    if (originalValue != null)
                        _variables[tryStmt.Handler.Param.Name] = originalValue;
                    else
                        _variables.Remove(tryStmt.Handler.Param.Name);
                }
            }
            else
            {
                result = Evaluate(tryStmt.Handler.Body, _sourceCode);
            }
        }
        finally
        {
            // Execute finally block if present
            if (tryStmt.Finalizer != null)
            {
                try
                {
                    Evaluate(tryStmt.Finalizer, _sourceCode);
                }
                catch
                {
                    // Finally block exceptions are generally ignored unless no other exception occurred
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// Evaluate when statements (ECEngine-specific conditional blocks)
    /// </summary>
    private object? EvaluateWhenStatement(WhenStatement whenStmt)
    {
        // When statements are now handled in EvaluateBlockStatement
        // This method is kept for compatibility but shouldn't be called directly
        var conditionValue = Evaluate(whenStmt.Condition);
        
        if (IsTruthy(conditionValue))
        {
            return EvaluateBlockStatement(whenStmt.Body);
        }
        
        return null;
    }

    /// <summary>
    /// Evaluate otherwise statements (ECEngine-specific default blocks)
    /// </summary>
    private object? EvaluateOtherwiseStatement(OtherwiseStatement otherwiseStmt)
    {
        // Otherwise statements are now handled in EvaluateBlockStatement
        // This method is kept for compatibility but shouldn't be called directly
        return EvaluateBlockStatement(otherwiseStmt.Body);
    }

    /// <summary>
    /// Evaluate logical expressions with short-circuit evaluation (&& and ||)
    /// </summary>
    private object? EvaluateLogicalExpression(LogicalExpression logicalExpr)
    {
        var left = Evaluate(logicalExpr.Left);
        
        if (logicalExpr.Operator == "&&")
        {
            if (!IsTruthy(left))
                return left; // Short-circuit: return left if falsy
            return Evaluate(logicalExpr.Right);
        }
        
        if (logicalExpr.Operator == "||")
        {
            if (IsTruthy(left))
                return left; // Short-circuit: return left if truthy
            return Evaluate(logicalExpr.Right);
        }
        
        throw new ECEngineException($"Unknown logical operator: {logicalExpr.Operator}",
            logicalExpr.Token?.Line ?? 1, logicalExpr.Token?.Column ?? 1, _sourceCode,
            "Supported logical operators are && and ||");
    }

    /// <summary>
    /// Evaluate block statements with scope management and when/otherwise logic
    /// </summary>
    private object? EvaluateBlockStatement(BlockStatement blockStmt)
    {
        // Push new scope for block-scoped variables (let, const)
        PushScope();
        
        try
        {
            object? lastValue = null;
            bool anyWhenExecuted = false;
            
            foreach (var statement in blockStmt.Body)
            {
                // Handle when/otherwise logic
                if (statement is WhenStatement whenStmt)
                {
                    var conditionValue = Evaluate(whenStmt.Condition);
                    if (IsTruthy(conditionValue))
                    {
                        anyWhenExecuted = true;
                        lastValue = EvaluateBlockStatement(whenStmt.Body);
                        // Important: Once a when condition is true and executed,
                        // we should skip the rest of the statements in this block
                        // to prevent otherwise from executing
                        break;
                    }
                }
                else if (statement is OtherwiseStatement otherwiseStmt)
                {
                    // Only execute otherwise if no when conditions were true
                    if (!anyWhenExecuted)
                    {
                        lastValue = EvaluateBlockStatement(otherwiseStmt.Body);
                    }
                }
                else
                {
                    // Regular statement evaluation
                    lastValue = Evaluate(statement);
                }
            }
            
            return lastValue;
        }
        finally
        {
            // Always pop the scope, even if an exception occurred
            PopScope();
        }
    }

    /// <summary>
    /// Check equality for switch case matching (uses strict equality ===)
    /// </summary>
    private bool IsEqual(object? left, object? right)
    {
        // Use strict equality for switch case matching
        return StrictEquals(left, right);
    }

    /// <summary>
    /// Evaluate return statements and throw ReturnException
    /// </summary>
    private object? EvaluateReturnStatement(ReturnStatement returnStmt)
    {
        var value = returnStmt.Argument != null ? Evaluate(returnStmt.Argument) : null;
        throw new ReturnException(value);
    }

    /// <summary>
    /// Evaluate break statements and throw BreakException
    /// </summary>
    private object? EvaluateBreakStatement(BreakStatement breakStmt)
    {
        throw new BreakException();
    }

    /// <summary>
    /// Evaluate continue statements and throw ContinueException
    /// </summary>
    private object? EvaluateContinueStatement(ContinueStatement continueStmt)
    {
        throw new ContinueException();
    }

    /// <summary>
    /// Evaluate throw statements and throw the specified exception
    /// </summary>
    private object? EvaluateThrowStatement(ThrowStatement throwStmt)
    {
        var value = Evaluate(throwStmt.Argument, _sourceCode);
        var message = value?.ToString() ?? "Thrown value";
        
        throw new ECEngineException(message, 
            throwStmt.Token?.Line ?? 1, throwStmt.Token?.Column ?? 1, _sourceCode,
            "User-thrown exception");
    }
}
