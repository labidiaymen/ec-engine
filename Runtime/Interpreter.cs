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
    private Stack<object?> _thisStack = new Stack<object?>(); // Stack for 'this' context

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
    /// Get a variable value by name for testing purposes
    /// </summary>
    public object? GetVariable(string name)
    {
        // Search through scopes from current to global
        foreach (var scope in _scopes)
        {
            if (scope.TryGetValue(name, out var variableInfo))
            {
                return variableInfo.Value;
            }
        }
        return null;
    }

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
    /// <summary>
    /// Push a new scope onto the stack (public for Generator support)
    /// </summary>
    public void PushScope()
    {
        _scopes.Push(new Dictionary<string, VariableInfo>());
        SyncVariables();
    }

    /// <summary>
    /// Pop the current scope from the stack (public for Generator support)
    /// </summary>
    public void PopScope()
    {
        if (_scopes.Count > 1) // Always keep at least the global scope
        {
            _scopes.Pop();
            SyncVariables();
        }
    }

    public void PushThisContext(object? thisContext)
    {
        _thisStack.Push(thisContext);
    }

    public void PopThisContext()
    {
        if (_thisStack.Count > 0)
        {
            _thisStack.Pop();
        }
    }

    public Stack<Dictionary<string, VariableInfo>> GetScopes()
    {
        return _scopes;
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
    /// Set a variable value (for assignments, public for Generator support)
    /// </summary>
    public void SetVariable(string name, object? value)
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

        // NEW: Use virtual dispatch for performance optimization
        return node.Accept(this);
    }

    /// <summary>
    /// Legacy evaluation method for backwards compatibility and fallback
    /// </summary>
    public object? EvaluateLegacy(ASTNode node)
    {
        if (node is ProgramNode program)
            return EvaluateProgram(program);
        if (node is ExpressionStatement stmt)
            return Evaluate(stmt.Expression, _sourceCode);
        if (node is VariableDeclaration varDecl)
            return EvaluateVariableDeclaration(varDecl);
        if (node is FunctionDeclaration funcDecl)
            return EvaluateFunctionDeclaration(funcDecl);
        if (node is FunctionExpression funcExpr)
            return EvaluateFunctionExpression(funcExpr);
        if (node is ArrowFunctionExpression arrowFuncExpr)
            return EvaluateArrowFunctionExpression(arrowFuncExpr);
        if (node is GeneratorFunctionDeclaration genFuncDecl)
            return EvaluateGeneratorFunctionDeclaration(genFuncDecl);
        if (node is GeneratorFunctionExpression genFuncExpr)
            return EvaluateGeneratorFunctionExpression(genFuncExpr);
        if (node is ReturnStatement returnStmt)
            return EvaluateReturnStatement(returnStmt);
        if (node is YieldStatement yieldStmt)
            return EvaluateYieldStatement(yieldStmt);
        if (node is BlockStatement blockStmt)
            return EvaluateBlockStatement(blockStmt);
        if (node is ExportStatement exportStmt)
            return EvaluateExportStatement(exportStmt);
        if (node is ImportStatement importStmt)
            return EvaluateImportStatement(importStmt);
        if (node is DefaultExportStatement defaultExportStmt)
            return EvaluateDefaultExportStatement(defaultExportStmt);
        if (node is NamedExportStatement namedExportStmt)
            return EvaluateNamedExportStatement(namedExportStmt);
        if (node is ReExportStatement reExportStmt)
            return EvaluateReExportStatement(reExportStmt);
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
        if (node is ForInStatement forInStmt)
            return EvaluateForInStatement(forInStmt);
        if (node is ForOfStatement forOfStmt)
            return EvaluateForOfStatement(forOfStmt);
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
        if (node is TemplateLiteral templateLiteral)
            return EvaluateTemplateLiteral(templateLiteral);
        if (node is BooleanLiteral booleanLiteral)
            return booleanLiteral.Value;
        if (node is NullLiteral nullLiteral)
            return null;
        if (node is ThisExpression thisExpression)
            return EvaluateThisExpression(thisExpression);
        if (node is ObjectLiteral objectLiteral)
            return EvaluateObjectLiteral(objectLiteral);
        if (node is ArrayLiteral arrayLiteral)
            return EvaluateArrayLiteral(arrayLiteral);
        if (node is Identifier identifier)
            return EvaluateIdentifier(identifier);
        if (node is AssignmentExpression assignment)
            return EvaluateAssignmentExpression(assignment);
        if (node is CompoundAssignmentExpression compoundAssignment)
            return EvaluateCompoundAssignmentExpression(compoundAssignment);
        if (node is MemberAssignmentExpression memberAssignment)
            return EvaluateMemberAssignmentExpression(memberAssignment);
        if (node is ConditionalExpression conditional)
            return EvaluateConditionalExpression(conditional);
        if (node is BinaryExpression binary)
            return EvaluateBinaryExpression(binary);
        if (node is UnaryExpression unary)
            return EvaluateUnaryExpression(unary);
        if (node is MemberExpression member)
            return EvaluateMemberExpression(member);
        if (node is DynamicImportExpression dynamicImport)
            return EvaluateDynamicImportExpression(dynamicImport);
        if (node is CallExpression call)
            return EvaluateCallExpression(call);
        if (node is NewExpression newExpr)
            return EvaluateNewExpression(newExpr);

        throw new ECEngineException($"Unknown node type: {node.GetType().Name}",
            1, 1, _sourceCode, "Unsupported AST node encountered during evaluation");
    }

    #region Optimized Evaluation Methods

    /// <summary>
    /// Optimized identifier evaluation with variable caching
    /// </summary>
    public object? EvaluateIdentifierOptimized(Identifier identifier)
    {
        // TODO: Add variable cache optimization here
        // For now, delegate to existing implementation
        return EvaluateIdentifier(identifier);
    }

    /// <summary>
    /// Optimized binary expression evaluation with fast paths for numbers
    /// </summary>
    public object? EvaluateBinaryExpressionOptimized(BinaryExpression binary)
    {
        // Fast path for number literals
        if (binary.Left is NumberLiteral leftNum && binary.Right is NumberLiteral rightNum)
        {
            var fastResult = EvaluateNumberBinaryOperation(leftNum.Value, binary.Operator, rightNum.Value);
            if (fastResult != null)
            {
                return fastResult;
            }
        }

        // Fall back to general case
        return EvaluateBinaryExpression(binary);
    }

    /// <summary>
    /// Fast path for number-only binary operations
    /// </summary>
    private object? EvaluateNumberBinaryOperation(double left, string op, double right)
    {
        return op switch
        {
            "+" => left + right,
            "-" => left - right,
            "*" => left * right,
            "/" => left / right,
            "%" => left % right,
            "<" => left < right,
            "<=" => left <= right,
            ">" => left > right,
            ">=" => left >= right,
            "==" => Math.Abs(left - right) < double.Epsilon,
            "!=" => Math.Abs(left - right) >= double.Epsilon,
            "===" => Math.Abs(left - right) < double.Epsilon,
            "!==" => Math.Abs(left - right) >= double.Epsilon,
            // Bitwise operators (convert to integers for bitwise operations)
            "&" => (double)((int)left & (int)right),
            "|" => (double)((int)left | (int)right),
            "^" => (double)((int)left ^ (int)right),
            "<<" => (double)((int)left << (int)right),
            ">>" => (double)((int)left >> (int)right),
            ">>>" => (double)((uint)(int)left >> (int)right),
            _ => null // Fall back to general evaluation
        };
    }

    /// <summary>
    /// Optimized call expression evaluation
    /// </summary>
    public object? EvaluateCallExpressionOptimized(CallExpression call)
    {
        // TODO: Add function call caching and optimization here
        // For now, delegate to existing implementation
        return EvaluateCallExpression(call);
    }

    #endregion

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

    private object? EvaluateThisExpression(ThisExpression thisExpression)
    {
        if (_thisStack.Count > 0)
        {
            return _thisStack.Peek();
        }
        
        // If no 'this' context, return null (or could throw an error)
        return null;
    }

    private object? EvaluateArrayLiteral(ArrayLiteral arrayLiteral)
    {
        var result = new List<object?>();
        
        foreach (var element in arrayLiteral.Elements)
        {
            var value = Evaluate(element);
            result.Add(value);
        }
        
        return result;
    }

    private object? EvaluateTemplateLiteral(TemplateLiteral templateLiteral)
    {
        var resultBuilder = new System.Text.StringBuilder();
        
        foreach (var element in templateLiteral.Elements)
        {
            if (element is TemplateText textElement)
            {
                resultBuilder.Append(textElement.Value);
            }
            else if (element is TemplateExpression exprElement)
            {
                var expressionValue = Evaluate(exprElement.Expression);
                var stringValue = ConvertToString(expressionValue);
                resultBuilder.Append(stringValue);
            }
        }
        
        return resultBuilder.ToString();
    }

    /// <summary>
    /// Convert a value to string representation (for template literal interpolation)
    /// </summary>
    private string ConvertToString(object? value)
    {
        if (value == null)
            return "null";
            
        if (value is string str)
            return str;
            
        if (value is bool b)
            return b ? "true" : "false";
            
        if (value is double d)
        {
            if (double.IsPositiveInfinity(d))
                return "Infinity";
            if (double.IsNegativeInfinity(d))
                return "-Infinity";
            if (double.IsNaN(d))
                return "NaN";
                
            // Format numbers similar to JavaScript
            if (d == Math.Floor(d) && d >= -9007199254740991 && d <= 9007199254740991)
                return ((long)d).ToString();
            return d.ToString();
        }
        
        if (value is List<object?> list)
        {
            var items = list.Select(item => ConvertToString(item));
            return string.Join(",", items);
        }
        
        if (value is Dictionary<string, object?> dict)
        {
            return "[object Object]";
        }
        
        return value.ToString() ?? "";
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
            "require" => _moduleSystem != null ? new RequireFunction(_moduleSystem, this) : null,
            "Date" => (object?)new DateModule(),
            "Math" => (object?)new MathModule(),
            "JSON" => (object?)new JsonModule(),
            "String" => (object?)new StringModule(),
            "Object" => (object?)new ObjectModule(),
            "Array" => (object?)"Array", // Constructor name for new operator
            "Number" => (object?)"Number", // Constructor name for new operator
            "Boolean" => (object?)"Boolean", // Constructor name for new operator
            "EventEmitter" => (object?)new EventEmitterModule(),
            "util" => (object?)new Runtime.UtilModule(),
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

    private object? EvaluateCompoundAssignmentExpression(CompoundAssignmentExpression assignment)
    {
        var variableInfo = FindVariable(assignment.Left.Name);
        if (variableInfo == null)
        {
            var token = assignment.Token;
            throw new ECEngineException($"Variable '{assignment.Left.Name}' is not defined",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                "Cannot perform compound assignment on undefined variable");
        }

        var oldValue = variableInfo.Value;
        var rightValue = Evaluate(assignment.Right, _sourceCode);  // Evaluate right side first

        // Create the binary operation
        var binaryOp = assignment.Operator[..^1]; // Remove '=' from operator (e.g., "+=" -> "+")
        var value = EvaluateCompoundOperation(oldValue, rightValue, binaryOp, assignment.Token);

        SetVariable(assignment.Left.Name, value);
        
        // Trigger observers if value changed
        if (!Equals(oldValue, value))
        {
            TriggerObservers(assignment.Left.Name, oldValue, value, variableInfo.Observers);
        }
        
        return value;
    }

    private object? EvaluateMemberAssignmentExpression(MemberAssignmentExpression assignment)
    {
        var value = Evaluate(assignment.Right, _sourceCode);
        var target = Evaluate(assignment.Left.Object, _sourceCode);
        
        if (target == null)
        {
            var assignmentToken = assignment.Token;
            throw new ECEngineException("Cannot assign property on null object",
                assignmentToken?.Line ?? 1, assignmentToken?.Column ?? 1, _sourceCode,
                "Property assignment target cannot be null");
        }
        
        if (target is Dictionary<string, object?> obj)
        {
            // Get property name based on member expression type
            string propertyName;
            if (assignment.Left.Computed)
            {
                // Bracket notation: obj[key]
                var propValue = Evaluate(assignment.Left.ComputedProperty!, _sourceCode);
                propertyName = propValue?.ToString() ?? "";
            }
            else
            {
                // Dot notation: obj.prop
                propertyName = assignment.Left.Property;
            }
            
            obj[propertyName] = value;
            return value;
        }
        
        var assignmentToken2 = assignment.Token;
        throw new ECEngineException("Cannot assign property on non-object",
            assignmentToken2?.Line ?? 1, assignmentToken2?.Column ?? 1, _sourceCode,
            $"Property assignment is only supported on objects, got {target?.GetType().Name ?? "null"}");
    }

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

    private object? EvaluateCompoundOperation(object? left, object? right, string op, Token? token)
    {
        // Handle string concatenation for +=
        if (op == "+" && (left is string || right is string))
        {
            var leftStr = left?.ToString() ?? "null";
            var rightStr = right?.ToString() ?? "null";
            return leftStr + rightStr;
        }

        // Handle numeric operations
        if (left is double leftNum && right is double rightNum)
        {
            return op switch
            {
                "+" => leftNum + rightNum,
                "-" => leftNum - rightNum,
                "*" => leftNum * rightNum,
                "/" => leftNum / rightNum,
                _ => throw new ECEngineException($"Unknown compound operator: {op}",
                    token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                    $"The compound operator '{op}=' is not supported")
            };
        }

        throw new ECEngineException($"Cannot perform {op}= on {left?.GetType().Name} and {right?.GetType().Name}",
            token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
            "Type mismatch in compound assignment operation");
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
            case "===":
                return IsStrictEqual(left, right);
            case "!==":
                return !IsStrictEqual(left, right);
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

        // Handle bitwise operators (convert values to integers for bitwise operations)
        if (IsBitwiseOperator(binary.Operator))
        {
            var leftInt = ConvertToInt(left);
            var rightInt = ConvertToInt(right);
            
            return binary.Operator switch
            {
                "&" => (double)(leftInt & rightInt),
                "|" => (double)(leftInt | rightInt),
                "^" => (double)(leftInt ^ rightInt),
                "<<" => (double)(leftInt << (rightInt & 0x1F)), // Mask to 5 bits like JavaScript
                ">>" => (double)(leftInt >> (rightInt & 0x1F)),
                ">>>" => (double)((uint)leftInt >> (rightInt & 0x1F)), // Unsigned right shift
                _ => throw new ECEngineException($"Unknown bitwise operator: {binary.Operator}",
                    binary.Token?.Line ?? 1, binary.Token?.Column ?? 1, _sourceCode,
                    $"The bitwise operator '{binary.Operator}' is not supported")
            };
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
                
            case "~":
                var intValue = ConvertToInt(operand);
                return (double)(~intValue);
                
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

        // Handle computed properties (array indexing and dynamic object access)
        if (member.Computed && member.ComputedProperty != null)
        {
            var key = Evaluate(member.ComputedProperty, _sourceCode);
            
            // Handle array indexing
            if (obj is List<object?> list)
            {
                if (key is double index)
                {
                    var intIndex = (int)index;
                    if (intIndex >= 0 && intIndex < list.Count)
                    {
                        return list[intIndex];
                    }
                    return null; // Out of bounds returns undefined in JavaScript
                }
                throw new ECEngineException("Array index must be a number",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    "Only numeric indices are allowed for array access");
            }
            
            // Handle native array indexing
            if (obj is Array arr)
            {
                if (key is double index)
                {
                    var intIndex = (int)index;
                    if (intIndex >= 0 && intIndex < arr.Length)
                    {
                        return arr.GetValue(intIndex);
                    }
                    return null; // Out of bounds returns undefined in JavaScript
                }
                throw new ECEngineException("Array index must be a number",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    "Only numeric indices are allowed for array access");
            }
            
            // Handle dictionary/object property access with computed key
            if (obj is Dictionary<string, object?> objDict)
            {
                var keyStr = key?.ToString() ?? "";
                return objDict.ContainsKey(keyStr) ? objDict[keyStr] : null;
            }
            
            throw new ECEngineException($"Cannot access property on {obj?.GetType().Name}",
                member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                "Computed property access is only supported on arrays and objects");
        }

        // Handle array properties and methods (for dot notation on arrays)
        if (obj is List<object?> array && !member.Computed)
        {
            return member.Property switch
            {
                "length" => (double)array.Count,
                "push" => new ArrayMethodFunction(array, "push"),
                "pop" => new ArrayMethodFunction(array, "pop"),
                "shift" => new ArrayMethodFunction(array, "shift"),
                "unshift" => new ArrayMethodFunction(array, "unshift"),
                "slice" => new ArrayMethodFunction(array, "slice"),
                "splice" => new ArrayMethodFunction(array, "splice"),
                "join" => new ArrayMethodFunction(array, "join"),
                "concat" => new ArrayMethodFunction(array, "concat"),
                "indexOf" => new ArrayMethodFunction(array, "indexOf"),
                "lastIndexOf" => new ArrayMethodFunction(array, "lastIndexOf"),
                "reverse" => new ArrayMethodFunction(array, "reverse"),
                "sort" => new ArrayMethodFunction(array, "sort"),
                _ => throw new ECEngineException($"Property {member.Property} not found on Array",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on arrays")
            };
        }

        // Handle native arrays (like string[], object[], etc.)
        if (obj is Array arr2 && !member.Computed)
        {
            return member.Property switch
            {
                "length" => (double)arr2.Length,
                _ => throw new ECEngineException($"Property {member.Property} not found on Array",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on arrays")
            };
        }

        if (obj is ConsoleObject && member.Property == "log")
        {
            return new ConsoleLogFunction();
        }
        
        if (obj is HttpModule httpModule && member.Property == "createServer")
        {
            return httpModule.CreateServer;
        }
        
        // Handle Date static methods
        if (obj is DateModule dateModule)
        {
            return member.Property switch
            {
                "now" => dateModule.Now,
                "parse" => dateModule.Parse,
                "UTC" => dateModule.UTC,
                _ => throw new ECEngineException($"Property {member.Property} not found on Date",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the Date object")
            };
        }
        
        // Handle Date instance methods
        if (obj is DateObject dateObj)
        {
            return member.Property switch
            {
                "getTime" => new DateMethodFunction(dateObj, "getTime"),
                "getFullYear" => new DateMethodFunction(dateObj, "getFullYear"),
                "getMonth" => new DateMethodFunction(dateObj, "getMonth"),
                "getDate" => new DateMethodFunction(dateObj, "getDate"),
                "getDay" => new DateMethodFunction(dateObj, "getDay"),
                "getHours" => new DateMethodFunction(dateObj, "getHours"),
                "getMinutes" => new DateMethodFunction(dateObj, "getMinutes"),
                "getSeconds" => new DateMethodFunction(dateObj, "getSeconds"),
                "getMilliseconds" => new DateMethodFunction(dateObj, "getMilliseconds"),
                "getUTCFullYear" => new DateMethodFunction(dateObj, "getUTCFullYear"),
                "getUTCMonth" => new DateMethodFunction(dateObj, "getUTCMonth"),
                "getUTCDate" => new DateMethodFunction(dateObj, "getUTCDate"),
                "getUTCDay" => new DateMethodFunction(dateObj, "getUTCDay"),
                "getUTCHours" => new DateMethodFunction(dateObj, "getUTCHours"),
                "getUTCMinutes" => new DateMethodFunction(dateObj, "getUTCMinutes"),
                "getUTCSeconds" => new DateMethodFunction(dateObj, "getUTCSeconds"),
                "getUTCMilliseconds" => new DateMethodFunction(dateObj, "getUTCMilliseconds"),
                "toString" => new DateMethodFunction(dateObj, "toString"),
                "toDateString" => new DateMethodFunction(dateObj, "toDateString"),
                "toTimeString" => new DateMethodFunction(dateObj, "toTimeString"),
                "toISOString" => new DateMethodFunction(dateObj, "toISOString"),
                "toUTCString" => new DateMethodFunction(dateObj, "toUTCString"),
                "toLocaleDateString" => new DateMethodFunction(dateObj, "toLocaleDateString"),
                "toLocaleTimeString" => new DateMethodFunction(dateObj, "toLocaleTimeString"),
                "toLocaleString" => new DateMethodFunction(dateObj, "toLocaleString"),
                "valueOf" => new DateMethodFunction(dateObj, "valueOf"),
                _ => throw new ECEngineException($"Property {member.Property} not found on Date instance",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the Date instance")
            };
        }
        
        // Handle Math static methods and constants
        if (obj is MathModule mathModule)
        {
            return member.Property switch
            {
                // Constants
                "E" => mathModule.E,
                "PI" => mathModule.PI,
                "LN2" => mathModule.LN2,
                "LN10" => mathModule.LN10,
                "LOG2E" => mathModule.LOG2E,
                "LOG10E" => mathModule.LOG10E,
                "SQRT1_2" => mathModule.SQRT1_2,
                "SQRT2" => mathModule.SQRT2,
                
                // Functions
                "abs" => new MathMethodFunction(mathModule.Abs, "abs"),
                "acos" => new MathMethodFunction(mathModule.Acos, "acos"),
                "asin" => new MathMethodFunction(mathModule.Asin, "asin"),
                "atan" => new MathMethodFunction(mathModule.Atan, "atan"),
                "atan2" => new MathMethodFunction(mathModule.Atan2, "atan2"),
                "ceil" => new MathMethodFunction(mathModule.Ceil, "ceil"),
                "cos" => new MathMethodFunction(mathModule.Cos, "cos"),
                "exp" => new MathMethodFunction(mathModule.Exp, "exp"),
                "floor" => new MathMethodFunction(mathModule.Floor, "floor"),
                "log" => new MathMethodFunction(mathModule.Log, "log"),
                "max" => new MathMethodFunction(mathModule.Max, "max"),
                "min" => new MathMethodFunction(mathModule.Min, "min"),
                "pow" => new MathMethodFunction(mathModule.Pow, "pow"),
                "random" => new MathMethodFunction(mathModule.Random, "random"),
                "round" => new MathMethodFunction(mathModule.Round, "round"),
                "sin" => new MathMethodFunction(mathModule.Sin, "sin"),
                "sqrt" => new MathMethodFunction(mathModule.Sqrt, "sqrt"),
                "tan" => new MathMethodFunction(mathModule.Tan, "tan"),
                "trunc" => new MathMethodFunction(mathModule.Trunc, "trunc"),
                _ => throw new ECEngineException($"Property {member.Property} not found on Math",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the Math object")
            };
        }
        
        // Handle JSON static methods
        if (obj is JsonModule jsonModule)
        {
            return member.Property switch
            {
                "parse" => new JsonMethodFunction(jsonModule.Parse, "parse"),
                "stringify" => new JsonMethodFunction(jsonModule.Stringify, "stringify"),
                _ => throw new ECEngineException($"Property {member.Property} not found on JSON",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the JSON object")
            };
        }
        
        // Handle String static methods
        if (obj is StringModule stringModule)
        {
            return member.Property switch
            {
                "fromCharCode" => stringModule.fromCharCode,
                "fromCodePoint" => stringModule.fromCodePoint,
                "raw" => stringModule.raw,
                _ => throw new ECEngineException($"Property {member.Property} not found on String",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the String object")
            };
        }
        
        // Handle Object static methods
        if (obj is ObjectModule objectModule)
        {
            return member.Property switch
            {
                "keys" => new ObjectMethodFunction(objectModule, "keys"),
                "values" => new ObjectMethodFunction(objectModule, "values"),
                "entries" => new ObjectMethodFunction(objectModule, "entries"),
                "hasOwnProperty" => new ObjectMethodFunction(objectModule, "hasOwnProperty"),
                "assign" => new ObjectMethodFunction(objectModule, "assign"),
                "create" => new ObjectMethodFunction(objectModule, "create"),
                "freeze" => new ObjectMethodFunction(objectModule, "freeze"),
                "seal" => new ObjectMethodFunction(objectModule, "seal"),
                _ => throw new ECEngineException($"Property {member.Property} not found on Object",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the Object object")
            };
        }
        
        // Handle Filesystem module methods
        if (obj is FilesystemModule fsModule)
        {
            return member.Property switch
            {
                "readFile" => fsModule.readFile,
                "readFileSync" => fsModule.readFileSync,
                "writeFile" => fsModule.writeFile,
                "writeFileSync" => fsModule.writeFileSync,
                "appendFile" => fsModule.appendFile,
                "appendFileSync" => fsModule.appendFileSync,
                "exists" => fsModule.exists,
                "existsSync" => fsModule.existsSync,
                "stat" => fsModule.stat,
                "statSync" => fsModule.statSync,
                "mkdir" => fsModule.mkdir,
                "mkdirSync" => fsModule.mkdirSync,
                "rmdir" => fsModule.rmdir,
                "rmdirSync" => fsModule.rmdirSync,
                "unlink" => fsModule.unlink,
                "unlinkSync" => fsModule.unlinkSync,
                "readdir" => fsModule.readdir,
                "readdirSync" => fsModule.readdirSync,
                "rename" => fsModule.rename,
                "renameSync" => fsModule.renameSync,
                "copyFile" => fsModule.copyFile,
                "copyFileSync" => fsModule.copyFileSync,
                "realpath" => fsModule.realpath,
                "realpathSync" => fsModule.realpathSync,
                "constants" => fsModule.constants,
                _ => throw new ECEngineException($"Property {member.Property} not found on FilesystemModule",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the filesystem module")
            };
        }
        
        // Handle Path module methods
        if (obj is PathModule pathModule)
        {
            return member.Property switch
            {
                "join" => pathModule.join,
                "resolve" => pathModule.resolve,
                "dirname" => pathModule.dirname,
                "basename" => pathModule.basename,
                "extname" => pathModule.extname,
                _ => throw new ECEngineException($"Property {member.Property} not found on PathModule",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the path module")
            };
        }
        
        // Handle OS module properties
        if (obj is OSModule osModule)
        {
            return member.Property switch
            {
                "platform" => osModule.platform,
                "hostname" => osModule.hostname,
                "tmpdir" => osModule.tmpdir,
                "homedir" => osModule.homedir,
                _ => throw new ECEngineException($"Property {member.Property} not found on OSModule",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the os module")
            };
        }
        
        // Handle Util module methods
        if (obj is Runtime.UtilModule utilModule)
        {
            return member.Property switch
            {
                "inspect" => new UtilMethodFunction(utilModule.inspect, "inspect"),
                "format" => new UtilMethodFunction(utilModule.format, "format"),
                "isArray" => new UtilMethodFunction(utilModule.isArray, "isArray"),
                "isDate" => new UtilMethodFunction(utilModule.isDate, "isDate"),
                "isError" => new UtilMethodFunction(utilModule.isError, "isError"),
                "isFunction" => new UtilMethodFunction(utilModule.isFunction, "isFunction"),
                "isNullOrUndefined" => new UtilMethodFunction(utilModule.isNullOrUndefined, "isNullOrUndefined"),
                "isNumber" => new UtilMethodFunction(utilModule.isNumber, "isNumber"),
                "isObject" => new UtilMethodFunction(utilModule.isObject, "isObject"),
                "isPrimitive" => new UtilMethodFunction(utilModule.isPrimitive, "isPrimitive"),
                "isString" => new UtilMethodFunction(utilModule.isString, "isString"),
                "isSymbol" => new UtilMethodFunction(utilModule.isSymbol, "isSymbol"),
                "isUndefined" => new UtilMethodFunction(utilModule.isUndefined, "isUndefined"),
                "isRegExp" => new UtilMethodFunction(utilModule.isRegExp, "isRegExp"),
                "isDeepStrictEqual" => new UtilMethodFunction(utilModule.isDeepStrictEqual, "isDeepStrictEqual"),
                "debuglog" => new UtilMethodFunction(utilModule.debuglog, "debuglog"),
                "inherits" => new UtilMethodFunction(utilModule.inherits, "inherits"),
                "promisify" => new UtilMethodFunction(utilModule.promisify, "promisify"),
                "callbackify" => new UtilMethodFunction(utilModule.callbackify, "callbackify"),
                "types" => utilModule.types,
                _ => throw new ECEngineException($"Property {member.Property} not found on UtilModule",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the util module")
            };
        }
        
        // Handle util.types module methods
        if (obj is TypesModule typesModule)
        {
            return member.Property switch
            {
                "isArrayBuffer" => new UtilMethodFunction(typesModule.isArrayBuffer, "isArrayBuffer"),
                "isDate" => new UtilMethodFunction(typesModule.isDate, "isDate"),
                "isMap" => new UtilMethodFunction(typesModule.isMap, "isMap"),
                "isSet" => new UtilMethodFunction(typesModule.isSet, "isSet"),
                "isRegExp" => new UtilMethodFunction(typesModule.isRegExp, "isRegExp"),
                _ => throw new ECEngineException($"Property {member.Property} not found on util.types",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the util.types module")
            };
        }
        
        // Handle EventEmitter module methods
        if (obj is EventEmitterModule eventEmitterModule)
        {
            return member.Property switch
            {
                "createEventEmitter" => new EventEmitterCreateFunction(eventEmitterModule),
                _ => throw new ECEngineException($"Property {member.Property} not found on EventEmitter",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the EventEmitter module")
            };
        }
        
        // Handle FileStats object
        if (obj is FileStats fileStats)
        {
            return member.Property switch
            {
                "isFile" => fileStats.isFile,
                "isDirectory" => fileStats.isDirectory,
                "size" => fileStats.size,
                "mtime" => fileStats.mtime,
                "ctime" => fileStats.ctime,
                "atime" => fileStats.atime,
                _ => throw new ECEngineException($"Property {member.Property} not found on FileStats",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the file stats object")
            };
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
        
        // Handle Generator instances
        if (obj is Generator generator)
        {
            return member.Property switch
            {
                "next" => new GeneratorMethodFunction(generator, "next"),
                _ => throw new ECEngineException($"Property {member.Property} not found on Generator",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the generator object")
            };
        }
        
        if (obj is FilesystemConstants fsConstants)
        {
            return member.Property switch
            {
                "F_OK" => fsConstants.F_OK,
                "R_OK" => fsConstants.R_OK,
                "W_OK" => fsConstants.W_OK,
                "X_OK" => fsConstants.X_OK,
                "O_RDONLY" => fsConstants.O_RDONLY,
                "O_WRONLY" => fsConstants.O_WRONLY,
                "O_RDWR" => fsConstants.O_RDWR,
                "O_CREAT" => fsConstants.O_CREAT,
                "O_EXCL" => fsConstants.O_EXCL,
                "O_TRUNC" => fsConstants.O_TRUNC,
                "O_APPEND" => fsConstants.O_APPEND,
                "S_IFMT" => fsConstants.S_IFMT,
                "S_IFREG" => fsConstants.S_IFREG,
                "S_IFDIR" => fsConstants.S_IFDIR,
                "S_IFCHR" => fsConstants.S_IFCHR,
                "S_IFBLK" => fsConstants.S_IFBLK,
                "S_IFIFO" => fsConstants.S_IFIFO,
                "S_IFLNK" => fsConstants.S_IFLNK,
                "S_IFSOCK" => fsConstants.S_IFSOCK,
                "S_IRUSR" => fsConstants.S_IRUSR,
                "S_IWUSR" => fsConstants.S_IWUSR,
                "S_IXUSR" => fsConstants.S_IXUSR,
                "S_IRGRP" => fsConstants.S_IRGRP,
                "S_IWGRP" => fsConstants.S_IWGRP,
                "S_IXGRP" => fsConstants.S_IXGRP,
                "S_IROTH" => fsConstants.S_IROTH,
                "S_IWOTH" => fsConstants.S_IWOTH,
                "S_IXOTH" => fsConstants.S_IXOTH,
                _ => throw new ECEngineException($"Property {member.Property} not found on FilesystemConstants",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on the filesystem constants object")
            };
        }
        
        // Handle string methods and properties
        if (obj is string stringValue)
        {
            return member.Property switch
            {
                // Properties
                "length" => (double)stringValue.Length,
                
                // Instance methods - Character access
                "charAt" => new StringMethodFunction(stringValue, "charAt"),
                "charCodeAt" => new StringMethodFunction(stringValue, "charCodeAt"),
                "codePointAt" => new StringMethodFunction(stringValue, "codePointAt"),
                "at" => new StringMethodFunction(stringValue, "at"),
                
                // Search methods
                "indexOf" => new StringMethodFunction(stringValue, "indexOf"),
                "lastIndexOf" => new StringMethodFunction(stringValue, "lastIndexOf"),
                "search" => new StringMethodFunction(stringValue, "search"),
                "includes" => new StringMethodFunction(stringValue, "includes"),
                "startsWith" => new StringMethodFunction(stringValue, "startsWith"),
                "endsWith" => new StringMethodFunction(stringValue, "endsWith"),
                
                // Extraction methods
                "slice" => new StringMethodFunction(stringValue, "slice"),
                "substring" => new StringMethodFunction(stringValue, "substring"),
                "substr" => new StringMethodFunction(stringValue, "substr"),
                
                // Case methods
                "toLowerCase" => new StringMethodFunction(stringValue, "toLowerCase"),
                "toUpperCase" => new StringMethodFunction(stringValue, "toUpperCase"),
                "toLocaleLowerCase" => new StringMethodFunction(stringValue, "toLocaleLowerCase"),
                "toLocaleUpperCase" => new StringMethodFunction(stringValue, "toLocaleUpperCase"),
                
                // String building methods
                "concat" => new StringMethodFunction(stringValue, "concat"),
                "repeat" => new StringMethodFunction(stringValue, "repeat"),
                "padStart" => new StringMethodFunction(stringValue, "padStart"),
                "padEnd" => new StringMethodFunction(stringValue, "padEnd"),
                
                // Modification methods
                "trim" => new StringMethodFunction(stringValue, "trim"),
                "trimStart" => new StringMethodFunction(stringValue, "trimStart"),
                "trimEnd" => new StringMethodFunction(stringValue, "trimEnd"),
                "replace" => new StringMethodFunction(stringValue, "replace"),
                "replaceAll" => new StringMethodFunction(stringValue, "replaceAll"),
                
                // Splitting
                "split" => new StringMethodFunction(stringValue, "split"),
                
                // Pattern matching
                "match" => new StringMethodFunction(stringValue, "match"),
                "matchAll" => new StringMethodFunction(stringValue, "matchAll"),
                
                // Unicode methods
                "normalize" => new StringMethodFunction(stringValue, "normalize"),
                "isWellFormed" => new StringMethodFunction(stringValue, "isWellFormed"),
                "toWellFormed" => new StringMethodFunction(stringValue, "toWellFormed"),
                
                // Comparison
                "localeCompare" => new StringMethodFunction(stringValue, "localeCompare"),
                
                // Object methods
                "toString" => new StringMethodFunction(stringValue, "toString"),
                "valueOf" => new StringMethodFunction(stringValue, "valueOf"),
                
                // HTML wrapper methods (deprecated but included for compatibility)
                "anchor" => new StringMethodFunction(stringValue, "anchor"),
                "big" => new StringMethodFunction(stringValue, "big"),
                "blink" => new StringMethodFunction(stringValue, "blink"),
                "bold" => new StringMethodFunction(stringValue, "bold"),
                "fixed" => new StringMethodFunction(stringValue, "fixed"),
                "fontcolor" => new StringMethodFunction(stringValue, "fontcolor"),
                "fontsize" => new StringMethodFunction(stringValue, "fontsize"),
                "italics" => new StringMethodFunction(stringValue, "italics"),
                "link" => new StringMethodFunction(stringValue, "link"),
                "small" => new StringMethodFunction(stringValue, "small"),
                "strike" => new StringMethodFunction(stringValue, "strike"),
                "sub" => new StringMethodFunction(stringValue, "sub"),
                "sup" => new StringMethodFunction(stringValue, "sup"),
                
                _ => throw new ECEngineException($"Property {member.Property} not found on String",
                    member.Token?.Line ?? 1, member.Token?.Column ?? 1, _sourceCode,
                    $"The property '{member.Property}' does not exist on strings")
            };
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

        // Detect if this is a method call on an object (callee is MemberExpression)
        object? thisContext = null;
        if (call.Callee is MemberExpression memberExpr)
        {
            thisContext = Evaluate(memberExpr.Object, _sourceCode);
        }

        if (function is ConsoleLogFunction)
        {
            foreach (var arg in arguments)
            {
                Console.WriteLine(FormatObjectForConsole(arg));
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

        // Handle Date functions
        if (function is DateModule dateModuleAsFunc)
        {
            // Date() called as function (same as new Date())
            return dateModuleAsFunc.Constructor.Call(arguments);
        }

        if (function is DateConstructorFunction dateConstructorFunc)
        {
            return dateConstructorFunc.Call(arguments);
        }

        if (function is DateNowFunction dateNowFunc)
        {
            return dateNowFunc.Call(arguments);
        }

        if (function is DateParseFunction dateParseFunc)
        {
            return dateParseFunc.Call(arguments);
        }

        if (function is DateUTCFunction dateUTCFunc)
        {
            return dateUTCFunc.Call(arguments);
        }

        if (function is DateMethodFunction dateMethodFunc)
        {
            return dateMethodFunc.Call(arguments);
        }

        if (function is MathMethodFunction mathMethodFunc)
        {
            return mathMethodFunc.Call(arguments);
        }

        if (function is JsonMethodFunction jsonMethodFunc)
        {
            return jsonMethodFunc.Call(arguments);
        }

        if (function is ObjectMethodFunction objectMethodFunc)
        {
            return objectMethodFunc.Call(arguments);
        }

        // Handle EventEmitter functions
        if (function is EventEmitterCreateFunction eventEmitterCreateFunc)
        {
            return eventEmitterCreateFunc.Call(arguments);
        }

        if (function is EventEmitterOnFunction eventEmitterOnFunc)
        {
            return eventEmitterOnFunc.Call(arguments);
        }

        if (function is EventEmitterEmitFunction eventEmitterEmitFunc)
        {
            // Special handling for emit to call user functions
            if (arguments.Count < 1)
            {
                return eventEmitterEmitFunc.Call(arguments);
            }

            var (eventName, eventArgs, listeners) = eventEmitterEmitFunc.GetEventData(arguments);
            
            if (listeners.Count == 0)
            {
                return false;
            }

            // Call all listeners with the provided arguments
            foreach (var listener in listeners.ToList())
            {
                try
                {
                    CallUserFunction(listener, eventArgs);
                }
                catch (Exception ex)
                {
                    // In Node.js, EventEmitter errors are handled specially
                    Console.WriteLine($"Warning: EventEmitter listener error: {ex.Message}");
                }
            }

            return true;
        }

        if (function is EventEmitterOffFunction eventEmitterOffFunc)
        {
            return eventEmitterOffFunc.Call(arguments);
        }

        if (function is EventEmitterRemoveAllFunction eventEmitterRemoveAllFunc)
        {
            return eventEmitterRemoveAllFunc.Call(arguments);
        }

        if (function is EventEmitterListenerCountFunction eventEmitterListenerCountFunc)
        {
            return eventEmitterListenerCountFunc.Call(arguments);
        }

        if (function is EventEmitterListenersFunction eventEmitterListenersFunc)
        {
            return eventEmitterListenersFunc.Call(arguments);
        }

        if (function is EventEmitterEventNamesFunction eventEmitterEventNamesFunc)
        {
            return eventEmitterEventNamesFunc.Call(arguments);
        }

        // Handle String constructor and static methods
        if (function is StringModule stringModule)
        {
            return stringModule.Call(arguments);
        }

        if (function is StringStaticMethodFunction stringStaticFunc)
        {
            return stringStaticFunc.Call(arguments);
        }

        if (function is StringMethodFunction stringMethodFunc)
        {
            return stringMethodFunc.Call(arguments);
        }

        if (function is ArrayMethodFunction arrayMethodFunc)
        {
            return arrayMethodFunc.Call(arguments);
        }

        // Handle Util module functions
        if (function is UtilMethodFunction utilMethodFunc)
        {
            return utilMethodFunc.Call(arguments);
        }

        // Handle require function
        if (function is RequireFunction requireFunc)
        {
            return requireFunc.Call(arguments.ToArray());
        }

        // Handle filesystem functions
        if (function is ReadFileFunction readFileFunc)
        {
            return readFileFunc.Call(arguments.ToArray());
        }

        if (function is ReadFileSyncFunction readFileSyncFunc)
        {
            return readFileSyncFunc.Call(arguments.ToArray());
        }

        if (function is WriteFileFunction writeFileFunc)
        {
            return writeFileFunc.Call(arguments.ToArray());
        }

        if (function is WriteFileSyncFunction writeFileSyncFunc)
        {
            return writeFileSyncFunc.Call(arguments.ToArray());
        }

        if (function is ExistsFunction existsFunc)
        {
            return existsFunc.Call(arguments.ToArray());
        }

        if (function is StatFunction statFunc)
        {
            return statFunc.Call(arguments.ToArray());
        }

        if (function is StatSyncFunction statSyncFunc)
        {
            return statSyncFunc.Call(arguments.ToArray());
        }

        if (function is MkdirFunction mkdirFunc)
        {
            return mkdirFunc.Call(arguments.ToArray());
        }

        if (function is MkdirSyncFunction mkdirSyncFunc)
        {
            return mkdirSyncFunc.Call(arguments.ToArray());
        }

        if (function is RmdirFunction rmdirFunc)
        {
            return rmdirFunc.Call(arguments.ToArray());
        }

        if (function is RmdirSyncFunction rmdirSyncFunc)
        {
            return rmdirSyncFunc.Call(arguments.ToArray());
        }

        if (function is UnlinkFunction unlinkFunc)
        {
            return unlinkFunc.Call(arguments.ToArray());
        }

        if (function is UnlinkSyncFunction unlinkSyncFunc)
        {
            return unlinkSyncFunc.Call(arguments.ToArray());
        }

        if (function is ReaddirFunction readdirFunc)
        {
            return readdirFunc.Call(arguments.ToArray());
        }

        if (function is ReaddirSyncFunction readdirSyncFunc)
        {
            return readdirSyncFunc.Call(arguments.ToArray());
        }

        // Handle additional filesystem functions
        if (function is AppendFileFunction appendFileFunc)
        {
            return appendFileFunc.Call(arguments.ToArray());
        }

        if (function is AppendFileSyncFunction appendFileSyncFunc)
        {
            return appendFileSyncFunc.Call(arguments.ToArray());
        }

        if (function is ExistsSyncFunction existsSyncFunc)
        {
            return existsSyncFunc.Call(arguments.ToArray());
        }

        if (function is CopyFileFunction copyFileFunc)
        {
            return copyFileFunc.Call(arguments.ToArray());
        }

        if (function is CopyFileSyncFunction copyFileSyncFunc)
        {
            return copyFileSyncFunc.Call(arguments.ToArray());
        }

        if (function is RenameFunction renameFunc)
        {
            return renameFunc.Call(arguments.ToArray());
        }

        if (function is RenameSyncFunction renameSyncFunc)
        {
            return renameSyncFunc.Call(arguments.ToArray());
        }

        if (function is RealpathFunction realpathFunc)
        {
            return realpathFunc.Call(arguments.ToArray());
        }

        if (function is RealpathSyncFunction realpathSyncFunc)
        {
            return realpathSyncFunc.Call(arguments.ToArray());
        }

        // Handle path functions
        if (function is JoinFunction joinFunc)
        {
            return joinFunc.Call(arguments.ToArray());
        }

        if (function is ResolveFunction resolveFunc)
        {
            return resolveFunc.Call(arguments.ToArray());
        }

        if (function is DirnameFunction dirnameFunc)
        {
            return dirnameFunc.Call(arguments.ToArray());
        }

        if (function is BasenameFunction basenameFunc)
        {
            return basenameFunc.Call(arguments.ToArray());
        }

        if (function is ExtnameFunction extnameFunc)
        {
            return extnameFunc.Call(arguments.ToArray());
        }

        // Handle util functions
        if (function is InspectFunction inspectFunc)
        {
            return inspectFunc.Call(arguments);
        }
        
        // Handle file stats functions
        if (function is IsFileFunction isFileFunc)
        {
            return isFileFunc.Call(arguments.ToArray());
        }
        
        if (function is IsDirectoryFunction isDirFunc)
        {
            return isDirFunc.Call(arguments.ToArray());
        }

        // Handle C# delegates from CommonJS modules
        if (function is Func<object[], object> delegateFunc)
        {
            return delegateFunc(arguments.ToArray());
        }

        if (function is GeneratorMethodFunction generatorMethodFunc)
        {
            return generatorMethodFunc.Call(arguments);
        }

        if (function is GeneratorFunction generatorFunction)
        {
            return CallGeneratorFunction(generatorFunction, arguments, thisContext);
        }

        if (function is Function userFunction)
        {
            return CallUserFunction(userFunction, arguments, thisContext);
        }

        var token = call.Token;
        throw new ECEngineException($"Cannot call {function?.GetType().Name}",
            token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
            "Attempted to call a non-function value");
    }

    /// <summary>
    /// Evaluates a new expression (constructor call)
    /// </summary>
    private object? EvaluateNewExpression(NewExpression newExpr)
    {
        var constructor = Evaluate(newExpr.Callee, _sourceCode);
        var arguments = newExpr.Arguments.Select(arg => Evaluate(arg, _sourceCode)).ToList();
        
        // Handle identifier-based constructor names
        string? constructorName = null;
        if (newExpr.Callee is Identifier identifier)
        {
            constructorName = identifier.Name;
        }
        
        // Handle string constructor names (for Array, Number, Boolean)
        if (constructor is string strConstructor)
        {
            constructorName = strConstructor;
        }
        
        // Handle module objects as constructors
        if (constructor is ObjectModule)
        {
            constructorName = "Object";
        }
        else if (constructor is StringModule)
        {
            constructorName = "String";
        }
        
        // Handle built-in constructors by name
        if (constructorName != null)
        {
            switch (constructorName)
            {
                case "Object":
                    if (arguments.Count == 0)
                        return new Dictionary<string, object?>();
                    return arguments[0]; // Object(value) returns the value
                    
                case "Array":
                    if (arguments.Count == 0)
                        return new List<object?>();
                    if (arguments.Count == 1 && arguments[0] is double length)
                        return new List<object?>(new object?[(int)length]);
                    return arguments.ToList();
                    
                case "Date":
                    if (arguments.Count == 0)
                        return DateTime.Now;
                    // Add more Date constructor logic as needed
                    return DateTime.Now;
                    
                case "String":
                    if (arguments.Count == 0)
                        return "";
                    return arguments[0]?.ToString() ?? "";
                    
                case "Number":
                    if (arguments.Count == 0)
                        return 0.0;
                    // Simple number conversion - just try to parse as double
                    if (arguments[0] is double num) return num;
                    if (double.TryParse(arguments[0]?.ToString(), out var parsed)) return parsed;
                    return double.NaN;
                    
                case "Boolean":
                    if (arguments.Count == 0)
                        return false;
                    return IsTruthy(arguments[0]);
                    
                default:
                    // For unknown constructor names, check if it's a user-defined function
                    if (constructor is Function constructorFunc)
                    {
                        return CallUserFunction(constructorFunc, arguments);
                    }
                    throw new ECEngineException($"Constructor not found: {constructorName}",
                        newExpr.Token?.Line ?? 1, newExpr.Token?.Column ?? 1, _sourceCode,
                        $"Cannot find constructor '{constructorName}'");
            }
        }
        
        // Handle function constructors
        if (constructor is Function constructorFunc2)
        {
            return CallUserFunction(constructorFunc2, arguments);
        }
        
        throw new ECEngineException($"'{constructor?.GetType().Name}' is not a constructor",
            newExpr.Token?.Line ?? 1, newExpr.Token?.Column ?? 1, _sourceCode,
            "Cannot use 'new' with non-constructor value");
    }

    private object? EvaluateFunctionDeclaration(FunctionDeclaration funcDecl)
    {
        // Create closure with current scope variables (maintain references, not copies)
        var closure = new Dictionary<string, VariableInfo>();
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!closure.ContainsKey(kvp.Key))
                {
                    // Store reference to the actual VariableInfo, not a copy
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
        // Create closure with current scope variables (maintain references, not copies)
        var closure = new Dictionary<string, VariableInfo>();
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!closure.ContainsKey(kvp.Key))
                {
                    // Store reference to the actual VariableInfo, not a copy
                    closure[kvp.Key] = kvp.Value;
                }
            }
        }
        
        // Anonymous function - no name, just return the function object
        return new Function(null, funcExpr.Parameters, funcExpr.Body, closure);
    }

    private object? EvaluateArrowFunctionExpression(ArrowFunctionExpression arrowFuncExpr)
    {
        // Create closure with current scope variables (maintain references, not copies)
        var closure = new Dictionary<string, VariableInfo>();
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!closure.ContainsKey(kvp.Key))
                {
                    // Store reference to the actual VariableInfo, not a copy
                    closure[kvp.Key] = kvp.Value;
                }
            }
        }
        
        // For arrow functions with expression body, wrap it in a return statement
        List<Statement> body;
        if (arrowFuncExpr.IsExpressionBody && arrowFuncExpr.Body != null)
        {
            // Create implicit return statement
            body = new List<Statement> { new ReturnStatement(arrowFuncExpr.Body) };
        }
        else if (arrowFuncExpr.BlockBody != null)
        {
            // Use the block body as-is
            body = arrowFuncExpr.BlockBody;
        }
        else
        {
            throw new ECEngineException("Arrow function must have either expression or block body", 
                arrowFuncExpr.Token?.Line ?? 0, arrowFuncExpr.Token?.Column ?? 0, _sourceCode, 
                "Arrow function body error");
        }
        
        // Arrow function - no name, just return the function object
        return new Function(null, arrowFuncExpr.Parameters, body, closure);
    }

    private object? EvaluateGeneratorFunctionDeclaration(GeneratorFunctionDeclaration genFuncDecl)
    {
        // Create closure with current scope variables (maintain references, not copies)
        var closure = new Dictionary<string, VariableInfo>();
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!closure.ContainsKey(kvp.Key))
                {
                    // Store reference to the actual VariableInfo, not a copy
                    closure[kvp.Key] = kvp.Value;
                }
            }
        }
        
        // Create generator function and store it in current scope
        var generator = new GeneratorFunction(genFuncDecl.Name, genFuncDecl.Parameters, genFuncDecl.Body, closure);
        if (genFuncDecl.Name != null)
        {
            SetVariable(genFuncDecl.Name, generator);
        }
        
        return generator;
    }

    private object? EvaluateGeneratorFunctionExpression(GeneratorFunctionExpression genFuncExpr)
    {
        // Create closure with current scope variables (maintain references, not copies)
        var closure = new Dictionary<string, VariableInfo>();
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!closure.ContainsKey(kvp.Key))
                {
                    // Store reference to the actual VariableInfo, not a copy
                    closure[kvp.Key] = kvp.Value;
                }
            }
        }
        
        // Anonymous generator function - no name, just return the generator function object
        return new GeneratorFunction(null, genFuncExpr.Parameters, genFuncExpr.Body, closure);
    }

    private object? EvaluateYieldStatement(YieldStatement yieldStmt)
    {
        var value = yieldStmt.Argument != null ? Evaluate(yieldStmt.Argument) : null;
        throw new YieldException(value);
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
            
            // Schedule observer execution on next tick to make it non-blocking (if event loop is available)
            if (_eventLoop != null)
            {
                _eventLoop.NextTick(() =>
                {
                    try
                    {
                        // Call observer with oldValue, newValue, and variableName as arguments
                        var arguments = new List<object?> { oldValue, newValue, variableName };
                        CallUserFunction(currentObserver, arguments, null);
                    }
                    catch (Exception ex)
                    {
                        // Log observer error but don't stop execution
                        Console.WriteLine($"Warning: Observer for variable '{variableName}' threw an error: {ex.Message}");
                    }
                });
            }
            else
            {
                // If no event loop, execute synchronously
                try
                {
                    // Call observer with oldValue, newValue, and variableName as arguments
                    var arguments = new List<object?> { oldValue, newValue, variableName };
                    CallUserFunction(currentObserver, arguments, null);
                }
                catch (Exception ex)
                {
                    // Log observer error but don't stop execution
                    Console.WriteLine($"Warning: Observer for variable '{variableName}' threw an error: {ex.Message}");
                }
            }
        }
    }

    public object? CallUserFunction(Function function, List<object?> arguments, object? thisContext = null)
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
        
        // Push 'this' context for the function call
        _thisStack.Push(thisContext);
        
        try
        {
            // Add closure variables to current scope first
            foreach (var variable in function.Closure)
            {
                // Set the actual VariableInfo reference to maintain closure semantics
                _scopes.Peek()[variable.Key] = variable.Value;
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
            
            // Pop 'this' context
            if (_thisStack.Count > 0)
            {
                _thisStack.Pop();
            }
        }
    }

    public object? CallGeneratorFunction(GeneratorFunction generatorFunction, List<object?> arguments, object? thisContext = null)
    {
        // Create a new generator instance
        var generator = generatorFunction.CreateGenerator();
        
        // Initialize the generator with the interpreter and arguments
        generator.Initialize(this, arguments, thisContext);
        
        // Return the generator object
        return generator;
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
        
        // Sync variables from scopes to maintain backwards compatibility
        SyncVariables();
        
        // Extract the exported name and value
        if (exportStmt.Declaration is VariableDeclaration varDecl)
        {
            var value = _variables.ContainsKey(varDecl.Name) ? _variables[varDecl.Name].Value : null;
            _exports[varDecl.Name] = value;
        }
        else if (exportStmt.Declaration is FunctionDeclaration funcDecl)
        {
            var value = _variables.ContainsKey(funcDecl.Name ?? "") ? _variables[funcDecl.Name ?? ""].Value : null;
            if (funcDecl.Name != null)
            {
                _exports[funcDecl.Name] = value;
            }
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
            
            // Check if module loaded successfully
            if (module == null)
            {
                var token = importStmt.Token;
                throw new ECEngineException($"Failed to load module '{importStmt.ModulePath}'",
                    token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                    $"Module '{importStmt.ModulePath}' could not be loaded or resolved");
            }
            
            // Handle namespace import (import * as name from "module")
            if (importStmt.IsNamespaceImport && importStmt.NamespaceImportName != null)
            {
                // Create a namespace object containing all exports
                var namespaceObject = new Dictionary<string, object?>(module.Exports);
                
                var currentScope = _scopes.Peek();
                currentScope[importStmt.NamespaceImportName] = new VariableInfo("const", namespaceObject);
                _variables[importStmt.NamespaceImportName] = new VariableInfo("const", namespaceObject);
                
                return null;
            }
            
            // Handle default import (import name from "module")
            if (importStmt.DefaultImportName != null)
            {
                // For default imports, look for "default" export or use the entire module if CommonJS
                object? defaultValue = null;
                
                if (module.Exports.ContainsKey("default"))
                {
                    defaultValue = module.Exports["default"];
                }
                else if (module.Exports.Count == 1)
                {
                    // If there's only one export, use it as the default (common in CommonJS)
                    defaultValue = module.Exports.Values.First();
                }
                else
                {
                    // If no default and multiple exports, create an object with all exports
                    defaultValue = module.Exports;
                }
                
                if (defaultValue != null)
                {
                    var currentScope = _scopes.Peek();
                    currentScope[importStmt.DefaultImportName] = new VariableInfo("const", defaultValue);
                    _variables[importStmt.DefaultImportName] = new VariableInfo("const", defaultValue);
                }
            }
            
            // Handle named imports (import { name1, name2 as alias } from "module")
            if (importStmt.ImportedNames.Count > 0)
            {
                foreach (var name in importStmt.ImportedNames)
                {
                    // Skip default if already handled above
                    if (name == "default" && importStmt.DefaultImportName != null)
                        continue;
                        
                    if (module.Exports.ContainsKey(name))
                    {
                        var value = module.Exports[name];
                        
                        // Check if this import has an alias
                        var importName = importStmt.ImportAliases.ContainsKey(name) 
                            ? importStmt.ImportAliases[name] 
                            : name;
                        
                        // Add to current scope
                        var currentScope = _scopes.Peek();
                        currentScope[importName] = new VariableInfo("const", value);
                        // Also add to _variables for backwards compatibility
                        _variables[importName] = new VariableInfo("const", value);
                    }
                    else
                    {
                        var token = importStmt.Token;
                        throw new ECEngineException($"'{name}' is not exported by module '{importStmt.ModulePath}'",
                            token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                            $"Module '{importStmt.ModulePath}' does not export '{name}'");
                    }
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

    private object? EvaluateDynamicImportExpression(DynamicImportExpression dynamicImport)
    {
        if (_moduleSystem == null)
        {
            var token = dynamicImport.Token;
            throw new ECEngineException("Module system not available",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                "Dynamic import requires a module system to be configured");
        }

        try
        {
            // Evaluate the module path expression
            var modulePathValue = Evaluate(dynamicImport.ModulePath, _sourceCode);
            var modulePath = modulePathValue?.ToString() ?? "";

            if (string.IsNullOrEmpty(modulePath))
            {
                var token = dynamicImport.Token;
                throw new ECEngineException("Dynamic import path cannot be empty",
                    token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                    "Dynamic import requires a valid module path string");
            }

            // Load the module
            var module = _moduleSystem.LoadModule(modulePath, this);

            if (module == null)
            {
                var token = dynamicImport.Token;
                throw new ECEngineException($"Failed to load module '{modulePath}'",
                    token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                    $"Module '{modulePath}' could not be loaded or resolved");
            }

            // For dynamic imports, always return a Promise-like object with the module namespace
            // Since ECEngine doesn't have Promises yet, we'll return the module exports directly
            return new Dictionary<string, object?>(module.Exports);
        }
        catch (ECEngineException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var token = dynamicImport.Token;
            throw new ECEngineException($"Failed to dynamically import module: {ex.Message}",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                ex.Message);
        }
    }

    private object? EvaluateDefaultExportStatement(DefaultExportStatement defaultExportStmt)
    {
        // Evaluate the value to be exported
        var result = Evaluate(defaultExportStmt.Value, _sourceCode);
        
        // Store as default export
        _exports["default"] = result;
        
        return result;
    }

    private object? EvaluateNamedExportStatement(NamedExportStatement namedExportStmt)
    {
        // Sync variables from scopes to maintain backwards compatibility
        SyncVariables();
        
        foreach (var export in namedExportStmt.ExportedNames)
        {
            var localName = export.LocalName;
            var exportName = export.ExportName ?? localName;
            
            // Check if the variable exists
            if (!_variables.ContainsKey(localName))
            {
                var token = namedExportStmt.Token;
                throw new ECEngineException($"Cannot export '{localName}': variable not found",
                    token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                    $"Variable '{localName}' must be declared before it can be exported");
            }
            
            // Export the variable with the specified name
            var value = _variables[localName].Value;
            _exports[exportName] = value;
        }
        
        return null;
    }

    private object? EvaluateReExportStatement(ReExportStatement reExportStmt)
    {
        if (_moduleSystem == null)
        {
            var token = reExportStmt.Token;
            throw new ECEngineException("Module system not available",
                token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                "Re-export statements require a module system to be configured");
        }
        
        try
        {
            // Load the module to re-export from
            var module = _moduleSystem.LoadModule(reExportStmt.ModulePath, this);
            
            // Re-export the specified names
            foreach (var export in reExportStmt.ExportedNames)
            {
                var localName = export.LocalName;
                var exportName = export.ExportName ?? localName;
                
                if (module.Exports.ContainsKey(localName))
                {
                    var value = module.Exports[localName];
                    _exports[exportName] = value;
                }
                else
                {
                    var token = reExportStmt.Token;
                    throw new ECEngineException($"'{localName}' is not exported by module '{reExportStmt.ModulePath}'",
                        token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                        $"Module '{reExportStmt.ModulePath}' does not export '{localName}'");
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
            var token = reExportStmt.Token;
            throw new ECEngineException($"Failed to re-export from module '{reExportStmt.ModulePath}': {ex.Message}",
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
    
    private object? EvaluateForInStatement(ForInStatement forInStmt)
    {
        // Create new scope for the for...in loop
        PushScope();
        
        try
        {
            // Evaluate the object to iterate over
            var obj = Evaluate(forInStmt.Object, _sourceCode);
            
            object? result = null;
            
            if (obj == null)
            {
                // Can't iterate over null - just return
                return null;
            }
            
            // Handle different object types
            if (obj is Dictionary<string, object?> dictionary)
            {
                // Iterate over object properties
                foreach (var key in dictionary.Keys)
                {
                    // Set the loop variable to the current key in the current scope
                    var currentScope = _scopes.Peek();
                    currentScope[forInStmt.Variable] = new VariableInfo("let", key);
                    
                    try
                    {
                        // Execute body
                        result = Evaluate(forInStmt.Body, _sourceCode);
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
            }
            else if (obj is List<object?> list)
            {
                // Iterate over array indices
                for (int i = 0; i < list.Count; i++)
                {
                    // Set the loop variable to the current index in the current scope
                    var currentScope = _scopes.Peek();
                    currentScope[forInStmt.Variable] = new VariableInfo("let", i.ToString());
                    
                    try
                    {
                        // Execute body
                        result = Evaluate(forInStmt.Body, _sourceCode);
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
            }
            else if (obj is string str)
            {
                // Iterate over string indices
                for (int i = 0; i < str.Length; i++)
                {
                    // Set the loop variable to the current index in the current scope
                    var currentScope = _scopes.Peek();
                    currentScope[forInStmt.Variable] = new VariableInfo("let", i.ToString());
                    
                    try
                    {
                        // Execute body
                        result = Evaluate(forInStmt.Body, _sourceCode);
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
            }
            else
            {
                // For other types, try to convert to string and get properties
                var token = forInStmt.Token;
                throw new ECEngineException($"Object is not iterable with for...in loop",
                    token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                    $"Cannot iterate over object of type {obj.GetType().Name} using for...in");
            }
            
            return result;
        }
        finally
        {
            // Restore original scope
            PopScope();
        }
    }
    
    private object? EvaluateForOfStatement(ForOfStatement forOfStmt)
    {
        // Create new scope for the for...of loop
        PushScope();
        
        try
        {
            // Evaluate the iterable to iterate over
            var iterable = Evaluate(forOfStmt.Iterable, _sourceCode);
            
            object? result = null;
            
            if (iterable == null)
            {
                // Can't iterate over null - just return
                return null;
            }
            
            // Handle different iterable types
            if (iterable is List<object?> list)
            {
                // Iterate over array values
                foreach (var item in list)
                {
                    // Set the loop variable to the current value in the current scope
                    var currentScope = _scopes.Peek();
                    currentScope[forOfStmt.Variable] = new VariableInfo("let", item);
                    
                    try
                    {
                        // Execute body
                        result = Evaluate(forOfStmt.Body, _sourceCode);
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
            }
            else if (iterable is Generator generator)
            {
                // Iterate over generator values
                while (true)
                {
                    var next = generator.Next();
                    
                    // Check if the generator is done
                    if (next is Dictionary<string, object?> nextResult)
                    {
                        if (nextResult.ContainsKey("done") && 
                            nextResult["done"] is bool isDone && isDone)
                        {
                            break; // Generator is exhausted
                        }
                        
                        // Set the loop variable to the yielded value
                        var value = nextResult.ContainsKey("value") ? nextResult["value"] : null;
                        var currentScope = _scopes.Peek();
                        currentScope[forOfStmt.Variable] = new VariableInfo("let", value);
                        
                        try
                        {
                            // Execute body
                            result = Evaluate(forOfStmt.Body, _sourceCode);
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
                    else
                    {
                        // Unexpected generator result format
                        break;
                    }
                }
            }
            else if (iterable is string str)
            {
                // Iterate over string characters
                foreach (char ch in str)
                {
                    // Set the loop variable to the current character in the current scope
                    var currentScope = _scopes.Peek();
                    currentScope[forOfStmt.Variable] = new VariableInfo("let", ch.ToString());
                    
                    try
                    {
                        // Execute body
                        result = Evaluate(forOfStmt.Body, _sourceCode);
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
            }
            else
            {
                // For other types, not iterable with for...of
                var token = forOfStmt.Token;
                throw new ECEngineException($"Object is not iterable with for...of loop",
                    token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
                    $"Cannot iterate over object of type {iterable.GetType().Name} using for...of");
            }
            
            return result;
        }
        finally
        {
            // Restore original scope
            PopScope();
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
    
    /// <summary>
    /// Format an object for console output in a JavaScript-like way
    /// </summary>
    private string FormatObjectForConsole(object? obj)
    {
        if (obj == null)
            return "";
            
        if (obj is string str)
            return str;
            
        if (obj is Dictionary<string, object?> dict)
        {
            var pairs = dict.Select(kvp => $"{kvp.Key}: {FormatValueForConsole(kvp.Value)}");
            return "{ " + string.Join(", ", pairs) + " }";
        }
        
        if (obj is List<object?> list)
        {
            var items = list.Select(FormatValueForConsole);
            return "[ " + string.Join(", ", items) + " ]";
        }
        
        if (obj is DateObject dateObj)
        {
            return dateObj.DateTime.ToString();
        }
        
        // Handle special numeric values for JavaScript compatibility
        if (obj is double d)
        {
            if (double.IsPositiveInfinity(d))
                return "Infinity";
            if (double.IsNegativeInfinity(d))
                return "-Infinity";
            if (double.IsNaN(d))
                return "NaN";
        }
        
        return obj.ToString() ?? "";
    }
    
    /// <summary>
    /// Format a value for console output (used recursively)
    /// </summary>
    private string FormatValueForConsole(object? value)
    {
        if (value == null)
            return "null";
            
        if (value is string str)
            return $"\"{EscapeStringForConsole(str)}\"";
            
        if (value is bool b)
            return b ? "true" : "false";
            
        if (value is Dictionary<string, object?> dict)
        {
            var pairs = dict.Select(kvp => $"{kvp.Key}: {FormatValueForConsole(kvp.Value)}");
            return "{ " + string.Join(", ", pairs) + " }";
        }
        
        if (value is List<object?> list)
        {
            var items = list.Select(FormatValueForConsole);
            return "[ " + string.Join(", ", items) + " ]";
        }
        
        // Handle special numeric values for JavaScript compatibility
        if (value is double d)
        {
            if (double.IsPositiveInfinity(d))
                return "Infinity";
            if (double.IsNegativeInfinity(d))
                return "-Infinity";
            if (double.IsNaN(d))
                return "NaN";
        }
        
        return value.ToString() ?? "";
    }
    
    /// <summary>
    /// Escape a string for console display (showing escape sequences)
    /// </summary>
    private string EscapeStringForConsole(string str)
    {
        return str
            .Replace("\\", "\\\\")  // Backslash first (to avoid double-escaping)
            .Replace("\n", "\\n")   // Newline
            .Replace("\t", "\\t")   // Tab
            .Replace("\r", "\\r")   // Carriage return
            .Replace("\"", "\\\"")  // Double quote
            .Replace("\b", "\\b")   // Backspace
            .Replace("\f", "\\f")   // Form feed
            .Replace("\v", "\\v")   // Vertical tab
            .Replace("\0", "\\0");  // Null character
    }

    /// <summary>
    /// Check if two values are strictly equal (===)
    /// </summary>
    private bool IsStrictEqual(object? left, object? right)
    {
        // Strict equality: same type and same value
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        if (left.GetType() != right.GetType()) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Check if an operator is a bitwise operator
    /// </summary>
    private bool IsBitwiseOperator(string op)
    {
        return op == "&" || op == "|" || op == "^" || op == "<<" || op == ">>" || op == ">>>";
    }

    /// <summary>
    /// Convert a value to integer for bitwise operations (JavaScript-like conversion)
    /// </summary>
    private int ConvertToInt(object? value)
    {
        return value switch
        {
            double d => (int)d,
            int i => i,
            bool b => b ? 1 : 0,
            null => 0,
            string s => int.TryParse(s, out var result) ? result : 0,
            _ => 0
        };
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
