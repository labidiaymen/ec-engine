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
        if (node is NumberLiteral literal)
            return literal.Value;
        if (node is StringLiteral stringLiteral)
            return stringLiteral.Value;
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
        
        var value = Evaluate(assignment.Right, _sourceCode);
        variableInfo.Value = value;
        return value;
    }

    private object? EvaluateBinaryExpression(BinaryExpression binary)
    {
        var left = Evaluate(binary.Left, _sourceCode);
        var right = Evaluate(binary.Right, _sourceCode);

        if (left is double leftNum && right is double rightNum)
        {
            return binary.Operator switch
            {
                "+" => leftNum + rightNum,
                "-" => leftNum - rightNum,
                "*" => leftNum * rightNum,
                "/" => leftNum / rightNum,
                _ => throw new ECEngineException($"Unknown operator: {binary.Operator}",
                    binary.Token?.Line ?? 1, binary.Token?.Column ?? 1, _sourceCode,
                    $"The operator '{binary.Operator}' is not supported")
            };
        }

        var token = binary.Token;
        throw new ECEngineException($"Cannot perform {binary.Operator} on {left?.GetType().Name} and {right?.GetType().Name}",
            token?.Line ?? 1, token?.Column ?? 1, _sourceCode,
            "Type mismatch in binary operation");
    }

    private object? EvaluateMemberExpression(MemberExpression member)
    {
        var obj = Evaluate(member.Object, _sourceCode);

        if (obj is ConsoleObject && member.Property == "log")
        {
            return new ConsoleLogFunction();
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
}

// Helper classes for runtime objects
public class ConsoleObject { }

public class ConsoleLogFunction { }
