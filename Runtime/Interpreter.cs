using ECEngine.AST;
using ECEngine.Lexer;

namespace ECEngine.Runtime;

// Interpreter for evaluating AST
public class Interpreter
{
    private string _sourceCode = "";
    private Stack<Dictionary<string, VariableInfo>> _scopes = new Stack<Dictionary<string, VariableInfo>>();
    private Dictionary<string, VariableInfo> _variables = new Dictionary<string, VariableInfo>(); // Backwards compatibility
    private Dictionary<string, object?> _exports = new Dictionary<string, object?>();
    private ModuleSystem? _moduleSystem;
    private EventLoop? _eventLoop;

    public Interpreter()
    {
        // Initialize with global scope
        _scopes.Push(new Dictionary<string, VariableInfo>());
        SyncVariables();
    }

    /// <summary>
    /// Get read-only access to current variables (flattened view of all scopes)
    /// </summary>
    public IReadOnlyDictionary<string, VariableInfo> Variables 
    {
        get
        {
            var flattened = new Dictionary<string, VariableInfo>();
            // Traverse scopes from bottom to top (global to current)
            foreach (var scope in _scopes.Reverse())
            {
                foreach (var kvp in scope)
                {
                    if (!flattened.ContainsKey(kvp.Key))
                    {
                        flattened[kvp.Key] = kvp.Value;
                    }
                }
            }
            return flattened;
        }
    }

    /// <summary>
    /// Get read-only access to current exports
    /// </summary>
    public IReadOnlyDictionary<string, object?> GetExports() => _exports;

    /// <summary>
    /// Set the module system for this interpreter
    /// </summary>
    public void SetModuleSystem(ModuleSystem moduleSystem)
    {
        _moduleSystem = moduleSystem;
    }

    /// <summary>
    /// Set the event loop for this interpreter
    /// </summary>
    public void SetEventLoop(EventLoop eventLoop)
    {
        _eventLoop = eventLoop;
    }

    /// <summary>
    /// Clear all variables and reset interpreter state
    /// </summary>
    public void ClearState()
    {
        _scopes.Clear();
        _scopes.Push(new Dictionary<string, VariableInfo>()); // Reset to global scope
        _exports.Clear();
        _sourceCode = "";
        SyncVariables();
    }

    /// <summary>
    /// Sync the backwards compatibility _variables with current scopes
    /// </summary>
    private void SyncVariables()
    {
        _variables.Clear();
        // Traverse scopes from bottom to top (global to current)
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!_variables.ContainsKey(kvp.Key))
                {
                    _variables[kvp.Key] = kvp.Value;
                }
            }
        }
    }

    /// <summary>
    /// Push a new scope onto the stack
    /// </summary>
    private void PushScope()
    {
        _scopes.Push(new Dictionary<string, VariableInfo>());
        SyncVariables();
    }

    /// <summary>
    /// Pop the current scope from the stack
    /// </summary>
    private void PopScope()
    {
        if (_scopes.Count > 1) // Always keep at least the global scope
        {
            _scopes.Pop();
            SyncVariables();
        }
    }

    /// <summary>
    /// Find a variable in the scope chain
    /// </summary>
    private VariableInfo? FindVariable(string name)
    {
        // Search from current scope up to global scope
        foreach (var scope in _scopes)
        {
            if (scope.ContainsKey(name))
            {
                return scope[name];
            }
        }
        return null;
    }

    /// <summary>
    /// Declare a variable in the appropriate scope
    /// </summary>
    private void DeclareVariable(string kind, string name, object? value)
    {
        var currentScope = _scopes.Peek();
        
        // Check if variable already exists in current scope
        if (currentScope.ContainsKey(name))
        {
            throw new ECEngineException($"Variable '{name}' already declared", 
                0, 0, _sourceCode, "Variable redeclaration error");
        }
        
        // For 'var', check if it exists in any scope and use function scoping rules
        if (kind == "var")
        {
            // var has function scope, so declare it in the current scope
            // (which would be the function scope when inside a function)
            currentScope[name] = new VariableInfo(kind, value);
        }
        else
        {
            // let and const have block scope
            currentScope[name] = new VariableInfo(kind, value);
        }
    }

    /// <summary>
    /// Set a variable value (for assignments)
    /// </summary>
    private void SetVariable(string name, object? value)
    {
        // Search from current scope up to global scope
        foreach (var scope in _scopes)
        {
            if (scope.ContainsKey(name))
            {
                var variable = scope[name];
                if (variable.IsConstant)
                {
                    throw new ECEngineException($"Cannot assign to const variable '{name}'", 
                        0, 0, _sourceCode, "Const assignment error");
                }
                variable.Value = value;
                return;
            }
        }
        
        // Variable not found, create in global scope (like JavaScript's implicit global behavior)
        var globalScope = _scopes.Last();
        globalScope[name] = new VariableInfo("var", value);
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
        if (node is ExportStatement exportStmt)
            return EvaluateExportStatement(exportStmt);
        if (node is ImportStatement importStmt)
            return EvaluateImportStatement(importStmt);
        if (node is ObserveStatement observeStmt)
            return EvaluateObserveStatement(observeStmt);
        if (node is MultiObserveStatement multiObserveStmt)
            return EvaluateMultiObserveStatement(multiObserveStmt);
        if (node is WhenStatement whenStmt)
            return EvaluateWhenStatement(whenStmt);
        if (node is OtherwiseStatement otherwiseStmt)
            return EvaluateOtherwiseStatement(otherwiseStmt);
        if (node is IfStatement ifStmt)
            return EvaluateIfStatement(ifStmt);
        if (node is ForStatement forStmt)
            return EvaluateForStatement(forStmt);
        if (node is WhileStatement whileStmt)
            return EvaluateWhileStatement(whileStmt);
        if (node is DoWhileStatement doWhileStmt)
            return EvaluateDoWhileStatement(doWhileStmt);
        if (node is BreakStatement breakStmt)
            return EvaluateBreakStatement(breakStmt);
        if (node is ContinueStatement continueStmt)
            return EvaluateContinueStatement(continueStmt);
        if (node is SwitchStatement switchStmt)
            return EvaluateSwitchStatement(switchStmt);
        if (node is TryStatement tryStmt)
            return EvaluateTryStatement(tryStmt);
        if (node is ThrowStatement throwStmt)
            return EvaluateThrowStatement(throwStmt);
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
        if (node is ObjectLiteral objectLiteral)
            return EvaluateObjectLiteral(objectLiteral);
        if (node is Identifier identifier)
            return EvaluateIdentifier(identifier);
        if (node is AssignmentExpression assignment)
            return EvaluateAssignmentExpression(assignment);
        if (node is BinaryExpression binary)
            return EvaluateBinaryExpression(binary);
        if (node is UnaryExpression unary)
            return EvaluateUnaryExpression(unary);
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

    private object? EvaluateObjectLiteral(ObjectLiteral objectLiteral)
    {
        var result = new Dictionary<string, object?>();
        
        foreach (var property in objectLiteral.Properties)
        {
            var value = Evaluate(property.Value);
            result[property.Key] = value;
        }
        
        return result;
    }

    private object? EvaluateIdentifier(Identifier identifier)
    {
        // Check if it's a variable using scope-aware lookup
        var variableInfo = FindVariable(identifier.Name);
        if (variableInfo != null)
        {
            return variableInfo.Value;
        }
        
        // Check for built-in identifiers
        var result = identifier.Name switch
        {
            "console" => (object?)new ConsoleObject(),
            "setTimeout" => _eventLoop != null ? new SetTimeoutFunction(_eventLoop, this) : null,
            "setInterval" => _eventLoop != null ? new SetIntervalFunction(_eventLoop, this) : null,
            "clearTimeout" => _eventLoop != null ? new ClearTimeoutFunction(_eventLoop) : null,
            "clearInterval" => _eventLoop != null ? new ClearIntervalFunction(_eventLoop) : null,
            "nextTick" => _eventLoop != null ? new NextTickFunction(_eventLoop, this) : null,
            "http" => _eventLoop != null ? new HttpModule(_eventLoop, this) : null,
            "createServer" => _eventLoop != null ? new CreateServerFunction(_eventLoop, this) : null,
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
        
        // Use new scope-aware declaration method
        DeclareVariable(varDecl.Kind, varDecl.Name, value);
        return value;
    }

    private object? EvaluateAssignmentExpression(AssignmentExpression assignment)
    {
        // Use scope-aware variable lookup and assignment
        var value = Evaluate(assignment.Right, _sourceCode);
        
        // Find the variable in the scope chain
        var variableInfo = FindVariable(assignment.Left.Name);
        if (variableInfo == null)
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
        SetVariable(assignment.Left.Name, value);
        
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

    private object? EvaluateUnaryExpression(UnaryExpression unary)
    {
        var operand = Evaluate(unary.Operand, _sourceCode);
        var token = unary.Token;

        switch (unary.Operator)
        {
            case "!":
                return !IsTruthy(operand);
                
            case "unary+":
                if (operand is double num)
                    return num;
                throw new ECEngineException($"Unary + can only be applied to numbers",
                    token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                    $"Cannot apply unary + to {operand?.GetType().Name}");
                    
            case "unary-":
                if (operand is double num2)
                    return -num2;
                throw new ECEngineException($"Unary - can only be applied to numbers",
                    token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                    $"Cannot apply unary - to {operand?.GetType().Name}");
                    
            case "++":
                return EvaluateIncrementDecrement(unary, true);
                
            case "--":
                return EvaluateIncrementDecrement(unary, false);
                
            default:
                throw new ECEngineException($"Unknown unary operator: {unary.Operator}",
                    token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                    $"The unary operator '{unary.Operator}' is not supported");
        }
    }

    private object? EvaluateIncrementDecrement(UnaryExpression unary, bool isIncrement)
    {
        var token = unary.Token;
        
        if (unary.Operand is not Identifier identifier)
        {
            throw new ECEngineException($"++ and -- can only be applied to variables",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                "Increment/decrement operators require a variable");
        }

        var variableInfo = FindVariable(identifier.Name);
        if (variableInfo == null)
        {
            throw new ECEngineException($"Variable '{identifier.Name}' is not defined",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                $"Cannot increment/decrement undefined variable '{identifier.Name}'");
        }

        if (variableInfo.Type == "const")
        {
            throw new ECEngineException($"Cannot modify const variable '{identifier.Name}'",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                "Const variables cannot be modified");
        }

        if (variableInfo.Value is not double currentValue)
        {
            throw new ECEngineException($"++ and -- can only be applied to numbers",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                $"Cannot increment/decrement {variableInfo.Value?.GetType().Name}");
        }

        var newValue = isIncrement ? currentValue + 1 : currentValue - 1;
        SetVariable(identifier.Name, newValue);

        // Return old value for postfix, new value for prefix
        return unary.IsPrefix ? newValue : currentValue;
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
        
        if (obj is HttpModule httpModule && member.Property == "createServer")
        {
            return httpModule.CreateServer;
        }
        
        if (obj is ServerObject serverObj)
        {
            return member.Property switch
            {
                "listen" => new ServerMethodFunction(serverObj, "listen"),
                "close" => new ServerMethodFunction(serverObj, "close"),
                _ => throw new ECEngineException($"Property {member.Property} not found on ServerObject",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the server object")
            };
        }
        
        if (obj is ObservableServerObject observableServer)
        {
            return member.Property switch
            {
                "listen" => new ObservableServerMethodFunction(observableServer, "listen"),
                "close" => new ObservableServerMethodFunction(observableServer, "close"),
                _ => throw new ECEngineException($"Property {member.Property} not found on ObservableServerObject",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the observable server object")
            };
        }
        
        if (obj is HttpRequestObject request)
        {
            return member.Property switch
            {
                "method" => request.Method,
                "url" => request.Url,
                "path" => request.Path,
                "headers" => request.Headers,
                _ => null
            };
        }
        
        if (obj is HttpResponseObject response)
        {
            return member.Property switch
            {
                "statusCode" => response.StatusCode,
                "setHeader" => new ResponseMethodFunction(response, "setHeader"),
                "writeHead" => new ResponseMethodFunction(response, "writeHead"),
                "write" => new ResponseMethodFunction(response, "write"),
                "end" => new ResponseMethodFunction(response, "end"),
                _ => null
            };
        }
        
        if (obj is HttpRequestEvent requestEvent)
        {
            return member.Property switch
            {
                "method" => requestEvent.method,
                "url" => requestEvent.url,
                "headers" => requestEvent.headers,
                "body" => requestEvent.body,
                "response" => requestEvent.response,
                _ => throw new ECEngineException($"Property {member.Property} not found on HttpRequestEvent",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the request event object")
            };
        }
        
        if (obj is ResponseObject responseObj)
        {
            return member.Property switch
            {
                "send" => new ResponseWrapperFunction(responseObj, "send"),
                "json" => new ResponseWrapperFunction(responseObj, "json"),
                "status" => new ResponseWrapperFunction(responseObj, "status"),
                "setHeader" => new ResponseWrapperFunction(responseObj, "setHeader"),
                "redirect" => new ResponseWrapperFunction(responseObj, "redirect"),
                _ => throw new ECEngineException($"Property {member.Property} not found on ResponseObject",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the response object")
            };
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

        // Handle async functions
        if (function is SetTimeoutFunction setTimeoutFunc)
        {
            return setTimeoutFunc.Call(arguments);
        }

        if (function is SetIntervalFunction setIntervalFunc)
        {
            return setIntervalFunc.Call(arguments);
        }

        if (function is ClearTimeoutFunction clearTimeoutFunc)
        {
            return clearTimeoutFunc.Call(arguments);
        }

        if (function is ClearIntervalFunction clearIntervalFunc)
        {
            return clearIntervalFunc.Call(arguments);
        }

        if (function is NextTickFunction nextTickFunc)
        {
            return nextTickFunc.Call(arguments);
        }

        // Handle HTTP functions
        if (function is CreateServerFunction createServerFunc)
        {
            return createServerFunc.Call(arguments);
        }

        if (function is ServerMethodFunction serverMethodFunc)
        {
            return serverMethodFunc.Call(arguments);
        }

        if (function is ResponseMethodFunction responseMethodFunc)
        {
            return responseMethodFunc.Call(arguments);
        }

        if (function is ObservableServerMethodFunction observableServerMethodFunc)
        {
            return observableServerMethodFunc.Call(arguments);
        }

        if (function is ResponseWrapperFunction responseWrapperFunc)
        {
            return responseWrapperFunc.Call(arguments);
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
        // Create closure with current scope variables (flattened view)
        var closure = new Dictionary<string, VariableInfo>();
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!closure.ContainsKey(kvp.Key))
                {
                    closure[kvp.Key] = kvp.Value;
                }
            }
        }
        
        var function = new Function(funcDecl.Name, funcDecl.Parameters, funcDecl.Body, closure);
        DeclareVariable("function", funcDecl.Name, function);
        return function;
    }

    private object? EvaluateFunctionExpression(FunctionExpression funcExpr)
    {
        // Create closure with current scope variables (flattened view)
        var closure = new Dictionary<string, VariableInfo>();
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!closure.ContainsKey(kvp.Key))
                {
                    closure[kvp.Key] = kvp.Value;
                }
            }
        }
        
        // Anonymous function - no name, just return the function object
        return new Function(null, funcExpr.Parameters, funcExpr.Body, closure);
    }

    private object? EvaluateReturnStatement(ReturnStatement returnStmt)
    {
        var value = returnStmt.Argument != null ? Evaluate(returnStmt.Argument) : null;
        throw new ReturnException(value);
    }

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
        // Handle different types of observation targets
        if (observeStmt.Target is Identifier identifier)
        {
            // Simple variable observation: observe variable function(old, new) { ... }
            return EvaluateVariableObservation(identifier.Name, observeStmt.Handler, observeStmt.Token);
        }
        else if (observeStmt.Target is MemberExpression memberExpr)
        {
            // Property chain observation: observe server.requests function(ev) { ... }
            return EvaluatePropertyObservation(memberExpr, observeStmt.Handler, observeStmt.Token);
        }
        else
        {
            var token = observeStmt.Token;
            throw new ECEngineException($"Invalid observe target: {observeStmt.Target.GetType().Name}",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                "Observe target must be a variable or property chain");
        }
    }

    private object? EvaluateVariableObservation(string variableName, FunctionExpression handler, Token? token)
    {
        // Check if variable exists
        var variableInfo = FindVariable(variableName);
        if (variableInfo == null)
        {
            throw new ECEngineException($"Cannot observe undeclared variable '{variableName}'",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                $"Variable '{variableName}' must be declared before observing");
        }
        
        // Create closure with current scope variables (flattened view)
        var closure = new Dictionary<string, VariableInfo>();
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!closure.ContainsKey(kvp.Key))
                {
                    closure[kvp.Key] = kvp.Value;
                }
            }
        }
        
        // Create observer function
        var observerFunction = new Function(null, handler.Parameters, handler.Body, closure);
        
        // Add observer to the variable
        variableInfo.Observers.Add(observerFunction);
        
        return null; // observe statements don't return a value
    }

    private object? EvaluatePropertyObservation(MemberExpression memberExpr, FunctionExpression handler, Token? token)
    {
        // Evaluate the object part (e.g., 'server' in 'server.requests')
        var obj = Evaluate(memberExpr.Object, _sourceCode);
        var property = memberExpr.Property;

        // Handle different types of observable objects
        if (obj is ObservableServerObject observableServer)
        {
            // Create observer function
            var closure = new Dictionary<string, VariableInfo>();
            foreach (var scope in _scopes.Reverse())
            {
                foreach (var kvp in scope)
                {
                    if (!closure.ContainsKey(kvp.Key))
                    {
                        closure[kvp.Key] = kvp.Value;
                    }
                }
            }
            
            var observerFunction = new Function(null, handler.Parameters, handler.Body, closure);
            
            // Register observer based on property
            switch (property)
            {
                case "requests":
                    observableServer.AddRequestObserver(observerFunction);
                    break;
                case "errors":
                    observableServer.AddErrorObserver(observerFunction);
                    break;
                default:
                    throw new ECEngineException($"Property '{property}' is not observable on server object",
                        token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                        "Available observable properties: requests, errors");
            }
            
            return null;
        }
        else
        {
            throw new ECEngineException($"Object of type '{obj?.GetType().Name}' does not support property observation",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                "Only certain objects like servers support property observation");
        }
    }

    private void TriggerObservers(string variableName, object? oldValue, object? newValue, List<Function> observers)
    {
        foreach (var observer in observers)
        {
            // Capture the observer in a closure for async execution
            var currentObserver = observer;
            
            // Schedule observer execution on next tick to make it non-blocking
            _eventLoop.NextTick(() =>
            {
                try
                {
                    // Call observer with oldValue, newValue, and variableName as arguments
                    var arguments = new List<object?> { oldValue, newValue, variableName };
                    CallUserFunction(currentObserver, arguments);
                }
                catch (Exception ex)
                {
                    // Log observer error but don't stop execution
                    Console.WriteLine($"Warning: Observer for variable '{variableName}' threw an error: {ex.Message}");
                }
            });
        }
    }

    public object? CallUserFunction(Function function, List<object?> arguments)
    {
        // Check if this is an observable server proxy function
        if (function.IsObservableProxy && function.ObservableServer is ObservableServerObject observableServer)
        {
            // Handle HTTP requests by emitting to observers
            if (arguments.Count >= 2 && 
                arguments[0] is HttpRequestObject request && 
                arguments[1] is HttpResponseObject response)
            {
                observableServer.HandleRequest(request, response);
            }
            return null;
        }
        
        // Push new scope for function execution
        PushScope();
        
        try
        {
            // Add closure variables to current scope first
            foreach (var variable in function.Closure)
            {
                SetVariable(variable.Key, variable.Value.Value);
            }
            
            // Bind arguments to parameters (these override closure variables)
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                var paramName = function.Parameters[i];
                var paramValue = i < arguments.Count ? arguments[i] : null;
                SetVariable(paramName, paramValue);
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
            // Pop function scope
            PopScope();
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
        // When statements are now handled in EvaluateBlockStatement
        // This method is kept for compatibility but shouldn't be called directly
        var conditionValue = Evaluate(whenStmt.Condition);
        
        if (IsTruthy(conditionValue))
        {
            return EvaluateBlockStatement(whenStmt.Body);
        }
        
        return null;
    }

    private object? EvaluateOtherwiseStatement(OtherwiseStatement otherwiseStmt)
    {
        // Otherwise statements are now handled in EvaluateBlockStatement
        // This method is kept for compatibility but shouldn't be called directly
        return EvaluateBlockStatement(otherwiseStmt.Body);
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

    private object? EvaluateExportStatement(ExportStatement exportStmt)
    {
        // Evaluate the declaration first
        var result = Evaluate(exportStmt.Declaration, _sourceCode);
        
        // Extract the exported name and value
        if (exportStmt.Declaration is VariableDeclaration varDecl)
        {
            var value = _variables.ContainsKey(varDecl.Name) ? _variables[varDecl.Name].Value : null;
            _exports[varDecl.Name] = value;
        }
        else if (exportStmt.Declaration is FunctionDeclaration funcDecl)
        {
            var value = _variables.ContainsKey(funcDecl.Name) ? _variables[funcDecl.Name].Value : null;
            _exports[funcDecl.Name] = value;
        }
        
        return result;
    }

    private object? EvaluateImportStatement(ImportStatement importStmt)
    {
        if (_moduleSystem == null)
        {
            var token = importStmt.Token;
            throw new ECEngineException("Module system not available",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                "Import statements require a module system to be configured");
        }
        
        try
        {
            // Load the module
            var module = _moduleSystem.LoadModule(importStmt.ModulePath, this);
            
            // Import the requested names
            foreach (var name in importStmt.ImportedNames)
            {
                if (module.Exports.ContainsKey(name))
                {
                    var value = module.Exports[name];
                    _variables[name] = new VariableInfo("const", value);
                }
                else
                {
                    var token = importStmt.Token;
                    throw new ECEngineException($"'{name}' is not exported by module '{importStmt.ModulePath}'",
                        token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                        $"Module '{importStmt.ModulePath}' does not export '{name}'");
                }
            }
            
            return null;
        }
        catch (ECEngineException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var token = importStmt.Token;
            throw new ECEngineException($"Failed to import module '{importStmt.ModulePath}': {ex.Message}",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                ex.Message);
        }
    }
    
    private object? EvaluateForStatement(ForStatement forStmt)
    {
        // Create new scope for the for loop (similar to function scope)
        var originalVariables = new Dictionary<string, VariableInfo>(_variables);
        
        try
        {
            // Initialize (if present)
            if (forStmt.Init != null)
            {
                Evaluate(forStmt.Init, _sourceCode);
            }
            
            object? result = null;
            
            // Loop
            while (true)
            {
                // Check condition (if present)
                if (forStmt.Condition != null)
                {
                    var conditionResult = Evaluate(forStmt.Condition, _sourceCode);
                    if (!IsTruthy(conditionResult))
                        break;
                }
                
                try
                {
                    // Execute body
                    result = Evaluate(forStmt.Body, _sourceCode);
                }
                catch (BreakException)
                {
                    break;
                }
                catch (ContinueException)
                {
                    // Continue to update step
                }
                
                // Update (if present)
                if (forStmt.Update != null)
                {
                    Evaluate(forStmt.Update, _sourceCode);
                }
            }
            
            return result;
        }
        finally
        {
            // Restore original scope
            _variables = originalVariables;
        }
    }
    
    private object? EvaluateWhileStatement(WhileStatement whileStmt)
    {
        object? result = null;
        
        while (true)
        {
            // Check condition
            var conditionResult = Evaluate(whileStmt.Condition, _sourceCode);
            if (!IsTruthy(conditionResult))
                break;
                
            try
            {
                // Execute body
                result = Evaluate(whileStmt.Body, _sourceCode);
            }
            catch (BreakException)
            {
                break;
            }
            catch (ContinueException)
            {
                // Continue to next iteration
                continue;
            }
        }
        
        return result;
    }
    
    private object? EvaluateDoWhileStatement(DoWhileStatement doWhileStmt)
    {
        object? result = null;
        
        do
        {
            try
            {
                // Execute body
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
            
            // Check condition
            var conditionResult = Evaluate(doWhileStmt.Condition, _sourceCode);
            if (!IsTruthy(conditionResult))
                break;
                
        } while (true);
        
        return result;
    }
    
    private object? EvaluateBreakStatement(BreakStatement breakStmt)
    {
        throw new BreakException();
    }
    
    private object? EvaluateContinueStatement(ContinueStatement continueStmt)
    {
        throw new ContinueException();
    }
    
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
                Evaluate(tryStmt.Finalizer, _sourceCode);
            }
        }
        
        return result;
    }
    
    private object? EvaluateThrowStatement(ThrowStatement throwStmt)
    {
        var value = Evaluate(throwStmt.Argument, _sourceCode);
        var message = value?.ToString() ?? "Unspecified error";
        
        throw new ECEngineException($"Thrown: {message}", 
            throwStmt.Token?.Line ?? 0, throwStmt.Token?.Column ?? 0, _sourceCode, "User thrown exception");
    }
    
    // Helper method for switch case equality comparison
    private bool IsEqual(object? left, object? right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        
        // Handle numeric comparisons
        if (left is double leftNum && right is double rightNum)
            return Math.Abs(leftNum - rightNum) < double.Epsilon;
            
        // Handle string comparisons
        if (left is string && right is string)
            return left.Equals(right);
            
        // Handle boolean comparisons
        if (left is bool && right is bool)
            return left.Equals(right);
            
        return left.Equals(right);
    }
}

// Helper classes for runtime objects
public class ConsoleObject { }

public class ConsoleLogFunction { }

/// <summary>
/// Helper class for server method function calls
/// </summary>
public class ServerMethodFunction
{
    private readonly ServerObject _server;
    private readonly string _methodName;

    public ServerMethodFunction(ServerObject server, string methodName)
    {
        _server = server;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        return _methodName switch
        {
            "listen" => _server.Listen(arguments),
            "close" => _server.Close(arguments),
            _ => throw new ECEngineException($"Unknown server method: {_methodName}", 0, 0, "", "Runtime error")
        };
    }
}

/// <summary>
/// Helper class for response method function calls
/// </summary>
public class ResponseMethodFunction
{
    private readonly HttpResponseObject _response;
    private readonly string _methodName;

    public ResponseMethodFunction(HttpResponseObject response, string methodName)
    {
        _response = response;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        try
        {
            switch (_methodName)
            {
                case "setHeader":
                    if (arguments.Count >= 2)
                        _response.SetHeader(arguments[0]?.ToString() ?? "", arguments[1]?.ToString() ?? "");
                    return null;
                
                case "writeHead":
                    if (arguments.Count >= 1)
                    {
                        var statusCode = Convert.ToInt32(arguments[0]);
                        Dictionary<string, string>? headers = null;
                        
                        if (arguments.Count >= 2 && arguments[1] is Dictionary<string, object?> headerDict)
                        {
                            headers = headerDict.ToDictionary(
                                kvp => kvp.Key, 
                                kvp => kvp.Value?.ToString() ?? ""
                            );
                        }
                        
                        _response.WriteHead(statusCode, headers);
                    }
                    return null;
                
                case "write":
                    if (arguments.Count >= 1)
                        _response.Write(arguments[0]?.ToString() ?? "");
                    return null;
                
                case "end":
                    var data = arguments.Count > 0 ? arguments[0]?.ToString() : null;
                    _response.End(data);
                    return null;
                
                default:
                    throw new ECEngineException($"Unknown response method: {_methodName}", 0, 0, "", "Runtime error");
            }
        }
        catch (Exception ex)
        {
            throw new ECEngineException($"Error in response.{_methodName}: {ex.Message}", 0, 0, "", "Runtime error");
        }
    }
}

/// <summary>
/// Helper class for observable server method function calls
/// </summary>
public class ObservableServerMethodFunction
{
    private readonly ObservableServerObject _server;
    private readonly string _methodName;

    public ObservableServerMethodFunction(ObservableServerObject server, string methodName)
    {
        _server = server;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        return _methodName switch
        {
            "listen" => _server.Listen(arguments),
            "close" => _server.Close(arguments),
            _ => throw new ECEngineException($"Unknown server method: {_methodName}", 0, 0, "", "Runtime error")
        };
    }
}

/// <summary>
/// Helper class for response wrapper method function calls
/// </summary>
public class ResponseWrapperFunction
{
    private readonly ResponseObject _responseObj;
    private readonly string _methodName;

    public ResponseWrapperFunction(ResponseObject responseObj, string methodName)
    {
        _responseObj = responseObj;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        switch (_methodName)
        {
            case "send":
                return _responseObj.Send(arguments.FirstOrDefault()?.ToString() ?? "");
            case "json":
                return _responseObj.Json(arguments.FirstOrDefault());
            case "status":
                return _responseObj.Status(Convert.ToInt32(arguments.FirstOrDefault()));
            case "setHeader":
                _responseObj.SetHeader(arguments[0]?.ToString() ?? "", arguments[1]?.ToString() ?? "");
                return null;
            case "redirect":
                _responseObj.Redirect(arguments[0]?.ToString() ?? "", arguments.Count > 1 ? Convert.ToInt32(arguments[1]) : 302);
                return null;
            default:
                throw new ECEngineException($"Unknown response method: {_methodName}", 0, 0, "", "Runtime error");
        }
    }
}
