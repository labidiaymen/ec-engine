using ECEngine.AST;

namespace ECEngine.Runtime;

// Interpreter for evaluating AST
public class Interpreter
{
    private string _sourceCode = "";
    private Dictionary<string, VariableInfo> _variables = new Dictionary<string, VariableInfo>();

    /// <summary>
    /// Get read-only access to current variables
    /// </summary>
    public IReadOnlyDictionary<string, VariableInfo> Variables => _variables;

    /// <summary>
    /// Clear all variables and reset interpreter state
    /// </summary>
    public void ClearState()
    {
        _variables.Clear();
        _sourceCode = "";
    }

    public object? Evaluate(ASTNode node, string sourceCode = "")
    {
        _sourceCode = sourceCode;

        if (node is ProgramNode program)
            return EvaluateProgram(program);
        if (node is ExpressionStatement stmt)
            return Evaluate(stmt.Expression, sourceCode);
        if (node is VariableDeclaration varDecl)
            return EvaluateVariableDeclaration(varDecl);
        if (node is FunctionDeclaration funcDecl)
            return EvaluateFunctionDeclaration(funcDecl);
        if (node is FunctionExpression funcExpr)
            return EvaluateFunctionExpression(funcExpr);
        if (node is ReturnStatement returnStmt)
            return EvaluateReturnStatement(returnStmt);
        if (node is BlockStatement blockStmt)
            return EvaluateBlockStatement(blockStmt);
        if (node is ObserveStatement observeStmt)
            return EvaluateObserveStatement(observeStmt);
        if (node is MultiObserveStatement multiObserveStmt)
            return EvaluateMultiObserveStatement(multiObserveStmt);
        if (node is WhenStatement whenStmt)
            return EvaluateWhenStatement(whenStmt);
        if (node is IfStatement ifStmt)
            return EvaluateIfStatement(ifStmt);
        if (node is MemberExpression memberExpr)
            return EvaluateMemberExpression(memberExpr);
        if (node is LogicalExpression logicalExpr)
            return EvaluateLogicalExpression(logicalExpr);
        if (node is NumberLiteral literal)
            return literal.Value;
        if (node is StringLiteral stringLiteral)
            return stringLiteral.Value;
        if (node is BooleanLiteral booleanLiteral)
            return booleanLiteral.Value;
        if (node is Identifier identifier)
            return EvaluateIdentifier(identifier);
        if (node is AssignmentExpression assignment)
            return EvaluateAssignmentExpression(assignment);
        if (node is BinaryExpression binary)
            return EvaluateBinaryExpression(binary);
        if (node is MemberExpression member)
            return EvaluateMemberExpression(member);
        if (node is CallExpression call)
            return EvaluateCallExpression(call);

        throw new ECEngineException($"Unknown node type: {node.GetType().Name}",
            1, 1, _sourceCode, "Unsupported AST node encountered during evaluation");
    }

    private object? EvaluateProgram(ProgramNode program)
    {
        object? lastResult = null;
        foreach (var statement in program.Body)
        {
            lastResult = Evaluate(statement, _sourceCode);
        }
        return lastResult;
    }

    private object? EvaluateIdentifier(Identifier identifier)
    {
        // Check if it's a variable
        if (_variables.TryGetValue(identifier.Name, out var variableInfo))
        {
            return variableInfo.Value;
        }
        
        // Check for built-in identifiers
        var result = identifier.Name switch
        {
            "console" => new ConsoleObject(),
            _ => null
        };

        if (result == null)
        {
            var token = identifier.Token;
            throw new ECEngineException($"Unknown identifier: {identifier.Name}",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                $"The identifier '{identifier.Name}' is not defined in the current scope");
        }

        return result;
    }

    private object? EvaluateVariableDeclaration(VariableDeclaration varDecl)
    {
        // Check if const declaration has an initializer
        if (varDecl.Kind == "const" && varDecl.Initializer == null)
        {
            var token = varDecl.Token;
            throw new ECEngineException($"Missing initializer in const declaration '{varDecl.Name}'",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                "const declarations must be initialized");
        }

        object? value = null;
        
        if (varDecl.Initializer != null)
        {
            value = Evaluate(varDecl.Initializer, _sourceCode);
        }
        
        // Check if variable already exists
        if (_variables.ContainsKey(varDecl.Name))
        {
            var token = varDecl.Token;
            throw new ECEngineException($"Variable '{varDecl.Name}' already declared",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                $"Cannot redeclare variable '{varDecl.Name}'");
        }
        
        _variables[varDecl.Name] = new VariableInfo(varDecl.Kind, value);
        return value;
    }

    private object? EvaluateAssignmentExpression(AssignmentExpression assignment)
    {
        // Check if variable exists
        if (!_variables.TryGetValue(assignment.Left.Name, out var variableInfo))
        {
            var token = assignment.Token;
            throw new ECEngineException($"Variable '{assignment.Left.Name}' not declared",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                $"Cannot assign to undeclared variable '{assignment.Left.Name}'");
        }

        // Check if trying to reassign a const variable
        if (variableInfo.IsConstant)
        {
            var token = assignment.Token;
            throw new ECEngineException($"Cannot assign to const variable '{assignment.Left.Name}'",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                $"const variables cannot be reassigned after declaration");
        }
        
        var oldValue = variableInfo.Value;
        var value = Evaluate(assignment.Right, _sourceCode);
        variableInfo.Value = value;
        
        // Trigger observers if value changed
        if (!Equals(oldValue, value))
        {
            TriggerObservers(assignment.Left.Name, oldValue, value, variableInfo.Observers);
        }
        
        return value;
    }

    private object? EvaluateBinaryExpression(BinaryExpression binary)
    {
        var left = Evaluate(binary.Left, _sourceCode);
        var right = Evaluate(binary.Right, _sourceCode);

        // Handle comparison operators (work with any comparable types)
        switch (binary.Operator)
        {
            case "==":
                return AreEqual(left, right);
            case "!=":
                return !AreEqual(left, right);
            case "<":
                return IsLessThan(left, right);
            case "<=":
                return IsLessThan(left, right) || AreEqual(left, right);
            case ">":
                return IsGreaterThan(left, right);
            case ">=":
                return IsGreaterThan(left, right) || AreEqual(left, right);
        }

        // Handle string concatenation and arithmetic addition
        if (binary.Operator == "+")
        {
            // String concatenation: if either operand is a string, convert both to strings and concatenate
            if (left is string || right is string)
            {
                var leftStr = left?.ToString() ?? "null";
                var rightStr = right?.ToString() ?? "null";
                return leftStr + rightStr;
            }
            
            // Numeric addition: both operands must be numbers
            if (left is double leftNum && right is double rightNum)
            {
                return leftNum + rightNum;
            }
            
            // If we reach here, it's an invalid + operation
            var token = binary.Token;
            throw new ECEngineException($"Cannot perform + on {left?.GetType().Name} and {right?.GetType().Name}",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                "The + operator can only be used for string concatenation or numeric addition");
        }

        // Handle other arithmetic operators (numbers only)
        if (left is double leftNum2 && right is double rightNum2)
        {
            return binary.Operator switch
            {
                "-" => leftNum2 - rightNum2,
                "*" => leftNum2 * rightNum2,
                "/" => leftNum2 / rightNum2,
                _ => throw new ECEngineException($"Unknown operator: {binary.Operator}",
                    binary.Token?.Line ?? 1, binary.Token?.Column ?? 1, _sourceCode,
                    $"The operator '{binary.Operator}' is not supported")
            };
        }

        var token2 = binary.Token;
        throw new ECEngineException($"Cannot perform {binary.Operator} on {left?.GetType().Name} and {right?.GetType().Name}",
            token2?.Line ?? 1, token2?.Column ?? 1, _sourceCode,
            "Type mismatch in binary operation");
    }

    private bool AreEqual(object? left, object? right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        
        // Convert numbers to the same type for comparison
        if (left is double || right is double)
        {
            var leftNum = Convert.ToDouble(left);
            var rightNum = Convert.ToDouble(right);
            return Math.Abs(leftNum - rightNum) < double.Epsilon;
        }
        
        return left.Equals(right);
    }

    private bool IsLessThan(object? left, object? right)
    {
        if (left is double leftNum && right is double rightNum)
        {
            return leftNum < rightNum;
        }
        if (left is string leftStr && right is string rightStr)
        {
            return string.Compare(leftStr, rightStr, StringComparison.Ordinal) < 0;
        }
        
        throw new ECEngineException("Cannot compare these types",
            1, 1, _sourceCode, "Only numbers and strings can be compared with < operator");
    }

    private bool IsGreaterThan(object? left, object? right)
    {
        if (left is double leftNum && right is double rightNum)
        {
            return leftNum > rightNum;
        }
        if (left is string leftStr && right is string rightStr)
        {
            return string.Compare(leftStr, rightStr, StringComparison.Ordinal) > 0;
        }
        
        throw new ECEngineException("Cannot compare these types",
            1, 1, _sourceCode, "Only numbers and strings can be compared with > operator");
    }

    private object? EvaluateMemberExpression(MemberExpression member)
    {
        var obj = Evaluate(member.Object, _sourceCode);

        if (obj is ConsoleObject && member.Property == "log")
        {
            return new ConsoleLogFunction();
        }
        
        if (obj is ChangeInfo changeInfo)
        {
            return GetChangeInfoProperty(changeInfo, member.Property);
        }
        
        if (obj is Dictionary<string, object?> dict)
        {
            return dict.ContainsKey(member.Property) ? dict[member.Property] : null;
        }

        var token = member.Token;
        throw new ECEngineException($"Property {member.Property} not found on {obj?.GetType().Name}",
            token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
            $"The property '{member.Property}' does not exist on the object");
    }

    private object? EvaluateCallExpression(CallExpression call)
    {
        var function = Evaluate(call.Callee, _sourceCode);
        var arguments = call.Arguments.Select(arg => Evaluate(arg, _sourceCode)).ToList();

        if (function is ConsoleLogFunction)
        {
            foreach (var arg in arguments)
            {
                Console.WriteLine(arg);
            }
            return null; // console.log returns undefined
        }

        if (function is Function userFunction)
        {
            return CallUserFunction(userFunction, arguments);
        }

        var token = call.Token;
        throw new ECEngineException($"Cannot call {function?.GetType().Name}",
            token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
            "Attempted to call a non-function value");
    }

    private object? EvaluateFunctionDeclaration(FunctionDeclaration funcDecl)
    {
        var function = new Function(funcDecl.Name, funcDecl.Parameters, funcDecl.Body, _variables);
        _variables[funcDecl.Name] = new VariableInfo("function", function);
        return function;
    }

    private object? EvaluateFunctionExpression(FunctionExpression funcExpr)
    {
        // Anonymous function - no name, just return the function object
        return new Function(null, funcExpr.Parameters, funcExpr.Body, _variables);
    }

    private object? EvaluateReturnStatement(ReturnStatement returnStmt)
    {
        var value = returnStmt.Argument != null ? Evaluate(returnStmt.Argument) : null;
        throw new ReturnException(value);
    }

    private object? EvaluateBlockStatement(BlockStatement blockStmt)
    {
        object? lastValue = null;
        
        foreach (var statement in blockStmt.Body)
        {
            lastValue = Evaluate(statement);
        }
        
        return lastValue;
    }

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

    private bool IsTruthy(object? value)
    {
        if (value == null) return false;
        if (value is bool boolValue) return boolValue;
        if (value is double doubleValue) return doubleValue != 0.0;
        if (value is string stringValue) return !string.IsNullOrEmpty(stringValue);
        return true; // All other values are truthy
    }

    private object? EvaluateObserveStatement(ObserveStatement observeStmt)
    {
        // Check if variable exists
        if (!_variables.TryGetValue(observeStmt.VariableName, out var variableInfo))
        {
            var token = observeStmt.Token;
            throw new ECEngineException($"Cannot observe undeclared variable '{observeStmt.VariableName}'",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                $"Variable '{observeStmt.VariableName}' must be declared before observing");
        }
        
        // Create observer function
        var observerFunction = new Function(null, observeStmt.Handler.Parameters, observeStmt.Handler.Body, _variables);
        
        // Add observer to the variable
        variableInfo.Observers.Add(observerFunction);
        
        return null; // observe statements don't return a value
    }

    private void TriggerObservers(string variableName, object? oldValue, object? newValue, List<Function> observers)
    {
        foreach (var observer in observers)
        {
            try
            {
                // Call observer with oldValue, newValue, and variableName as arguments
                var arguments = new List<object?> { oldValue, newValue, variableName };
                CallUserFunction(observer, arguments);
            }
            catch (Exception ex)
            {
                // Log observer error but don't stop execution
                Console.WriteLine($"Warning: Observer for variable '{variableName}' threw an error: {ex.Message}");
            }
        }
    }

    private object? CallUserFunction(Function function, List<object?> arguments)
    {
        // Create new scope with function parameters
        var originalVariables = new Dictionary<string, VariableInfo>(_variables);
        
        try
        {
            // Add closure variables to current scope
            foreach (var variable in function.Closure)
            {
                _variables[variable.Key] = variable.Value;
            }
            
            // Bind arguments to parameters
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                var paramName = function.Parameters[i];
                var paramValue = i < arguments.Count ? arguments[i] : null;
                _variables[paramName] = new VariableInfo("var", paramValue);
            }
            
            // Execute function body
            try
            {
                foreach (var statement in function.Body)
                {
                    Evaluate(statement);
                }
                return null; // No explicit return
            }
            catch (ReturnException returnEx)
            {
                return returnEx.Value;
            }
        }
        finally
        {
            // Restore original scope
            _variables = originalVariables;
        }
    }

    private object? EvaluateMultiObserveStatement(MultiObserveStatement multiObserveStmt)
    {
        // For each variable, add this multi-observer
        foreach (var variableName in multiObserveStmt.VariableNames)
        {
            if (!_variables.ContainsKey(variableName))
            {
                throw new ECEngineException($"Variable '{variableName}' is not defined",
                    multiObserveStmt.Token?.Line ?? 1, multiObserveStmt.Token?.Column ?? 1, _sourceCode,
                    "Cannot observe undefined variable");
            }

            var variable = _variables[variableName];
            
            // Create a special multi-observer entry
            var multiObserver = new MultiVariableObserver
            {
                VariableNames = multiObserveStmt.VariableNames,
                Handler = multiObserveStmt.Handler,
                Statement = multiObserveStmt
            };
            
            variable.MultiObservers.Add(multiObserver);
        }

        return null;
    }

    private object? EvaluateWhenStatement(WhenStatement whenStmt)
    {
        // When statements are only evaluated within observer context
        // This should only be called when we're inside an observer function
        var conditionValue = Evaluate(whenStmt.Condition);
        
        if (IsTruthy(conditionValue))
        {
            return EvaluateBlockStatement(whenStmt.Body);
        }
        
        return null;
    }

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

    private object? GetChangeInfoProperty(ChangeInfo changeInfo, string property)
    {
        return property switch
        {
            "triggered" => changeInfo.Triggered,
            "values" => changeInfo.Values,
            _ => changeInfo.Variables.ContainsKey(property) ? changeInfo.Variables[property] : null
        };
    }

    // Helper class for change information
    public class ChangeInfo
    {
        public List<string> Triggered { get; set; } = new();
        public Dictionary<string, object?> Values { get; set; } = new();
        public Dictionary<string, VariableChangeInfo> Variables { get; set; } = new();
    }

    public class VariableChangeInfo
    {
        public object? Old { get; set; }
        public object? New { get; set; }
    }

    // Helper class for multi-variable observers
    public class MultiVariableObserver
    {
        public List<string> VariableNames { get; set; } = new();
        public FunctionExpression Handler { get; set; } = null!;
        public MultiObserveStatement Statement { get; set; } = null!;
    }

    // Update TriggerObservers to handle multi-variable observers
    private void TriggerMultiObservers(string triggeredVariable, object? oldValue, object? newValue)
    {
        var triggeredObservers = new HashSet<MultiVariableObserver>();
        
        // Find all multi-observers that watch this variable
        foreach (var kvp in _variables)
        {
            foreach (var multiObserver in kvp.Value.MultiObservers)
            {
                if (multiObserver.VariableNames.Contains(triggeredVariable))
                {
                    triggeredObservers.Add(multiObserver);
                }
            }
        }
        
        // Execute each multi-observer
        foreach (var observer in triggeredObservers)
        {
            ExecuteMultiObserver(observer, triggeredVariable, oldValue, newValue);
        }
    }

    private void ExecuteMultiObserver(MultiVariableObserver observer, string triggeredVariable, object? oldValue, object? newValue)
    {
        // Create the changes object
        var changeInfo = new ChangeInfo();
        changeInfo.Triggered.Add(triggeredVariable);
        
        // Add current values for all observed variables
        foreach (var varName in observer.VariableNames)
        {
            if (_variables.ContainsKey(varName))
            {
                changeInfo.Values[varName] = _variables[varName].Value;
            }
        }
        
        // Add specific change information for the triggered variable
        changeInfo.Variables[triggeredVariable] = new VariableChangeInfo
        {
            Old = oldValue,
            New = newValue
        };
        
        // Create a new scope with the changes parameter
        var originalVariables = new Dictionary<string, VariableInfo>(_variables);
        
        try
        {
            // Add the changes parameter if the function expects it
            if (observer.Handler.Parameters.Count > 0)
            {
                var paramName = observer.Handler.Parameters[0];
                _variables[paramName] = new VariableInfo("const", changeInfo);
            }
            
            // Execute the handler body
            foreach (var statement in observer.Handler.Body)
            {
                Evaluate(statement);
            }
        }
        finally
        {
            // Restore original scope
            _variables = originalVariables;
        }
    }
}

// Helper classes for runtime objects
public class ConsoleObject { }

public class ConsoleLogFunction { }
