using ECEngine.AST;
using ECEngine.Lexer;
using System.Reflection;

namespace ECEngine.Runtime;

/// <summary>
/// Expression evaluation including binary operations, unary operations, assignments, and member access for ECEngine interpreter
/// </summary>
public partial class Interpreter
{
    /// <summary>
    /// Evaluate binary expressions (+, -, *, /, ==, etc.)
    /// </summary>
    private object? EvaluateBinaryExpression(BinaryExpression binaryExpr)
    {
        var left = Evaluate(binaryExpr.Left, _sourceCode);
        var right = Evaluate(binaryExpr.Right, _sourceCode);
        
        return binaryExpr.Operator switch
        {
            "+" => EvaluateAddition(left, right),
            "-" => EvaluateSubtraction(left, right),
            "*" => EvaluateMultiplication(left, right),
            "/" => EvaluateDivision(left, right),
            "%" => EvaluateModulo(left, right),
            "==" => EvaluateLooseEquality(left, right),
            "!=" => !EvaluateLooseEquality(left, right),
            "===" => StrictEquals(left, right),
            "!==" => !StrictEquals(left, right),
            "<" => EvaluateLessThan(left, right),
            ">" => EvaluateGreaterThan(left, right),
            "<=" => EvaluateLessThanOrEqual(left, right),
            ">=" => EvaluateGreaterThanOrEqual(left, right),
            "&&" => EvaluateLogicalAnd(binaryExpr.Left, binaryExpr.Right),
            "||" => EvaluateLogicalOr(binaryExpr.Left, binaryExpr.Right),
            "&" => EvaluateBitwiseAnd(left, right),
            "|" => EvaluateBitwiseOr(left, right),
            "^" => EvaluateBitwiseXor(left, right),
            "<<" => EvaluateLeftShift(left, right),
            ">>" => EvaluateRightShift(left, right),
            ">>>" => EvaluateUnsignedRightShift(left, right),
            "instanceof" => IsInstanceOf(left, right),
            _ => throw new ECEngineException($"Unknown binary operator: {binaryExpr.Operator}",
                binaryExpr.Token?.Line ?? 1, binaryExpr.Token?.Column ?? 1, _sourceCode,
                $"Unsupported binary operator '{binaryExpr.Operator}'")
        };
    }
    
    /// <summary>
    /// Evaluate unary expressions (!, -, +, typeof, ++, --)
    /// </summary>
    private object? EvaluateUnaryExpression(UnaryExpression unaryExpr)
    {
        return unaryExpr.Operator switch
        {
            "!" => !IsTruthy(Evaluate(unaryExpr.Operand, _sourceCode)),
            "-" => -(ToNumber(Evaluate(unaryExpr.Operand, _sourceCode))),
            "unary-" => -(ToNumber(Evaluate(unaryExpr.Operand, _sourceCode))), // Alternative unary minus format
            "+" => ToNumber(Evaluate(unaryExpr.Operand, _sourceCode)),
            "unary+" => ToNumber(Evaluate(unaryExpr.Operand, _sourceCode)), // Alternative unary plus format
            "~" => (double)(~ToInt32(Evaluate(unaryExpr.Operand, _sourceCode))),
            "typeof" => EvaluateTypeOf(unaryExpr),
            "++" => EvaluateIncrementDecrement(unaryExpr, true),  // Increment
            "--" => EvaluateIncrementDecrement(unaryExpr, false), // Decrement
            _ => throw new ECEngineException($"Unknown unary operator: {unaryExpr.Operator}",
                unaryExpr.Token?.Line ?? 1, unaryExpr.Token?.Column ?? 1, _sourceCode,
                $"Unsupported unary operator '{unaryExpr.Operator}'")
        };
    }
    
    /// <summary>
    /// Evaluate increment (++) and decrement (--) operators
    /// </summary>
    private object? EvaluateIncrementDecrement(UnaryExpression unaryExpr, bool isIncrement)
    {
        // The operand must be an identifier (variable)
        if (unaryExpr.Operand is not Identifier identifier)
        {
            throw new ECEngineException($"Invalid operand for {(isIncrement ? "increment" : "decrement")} operator",
                unaryExpr.Token?.Line ?? 1, unaryExpr.Token?.Column ?? 1, _sourceCode,
                $"The {(isIncrement ? "++" : "--")} operator can only be applied to variables");
        }
        
        // Get current value
        var currentValue = GetVariable(identifier.Name);
        var numericValue = ToNumber(currentValue);
        
        // Calculate new value
        var newValue = isIncrement ? numericValue + 1 : numericValue - 1;
        
        // Update the variable
        SetVariable(identifier.Name, newValue);
        
        // For prefix operators, return the new value
        // For postfix operators, return the old value
        // Currently implementing as prefix (++i, --i)
        return unaryExpr.IsPrefix ? newValue : numericValue;
    }
    
    /// <summary>
    /// Evaluate assignment expressions (=, +=, -=, etc.)
    /// </summary>
    private object? EvaluateAssignmentExpression(AssignmentExpression assignExpr)
    {
        if (assignExpr.Left is Identifier identifier)
        {
            var value = Evaluate(assignExpr.Right, _sourceCode);
            // Simple assignment (only = operator for AssignmentExpression)
            SetVariable(identifier.Name, value);
            return value;
        }
        
        throw new ECEngineException("Invalid assignment target",
            assignExpr.Token?.Line ?? 1, assignExpr.Token?.Column ?? 1, _sourceCode,
            "Assignment target must be an identifier");
    }
    
    /// <summary>
    /// Evaluate a compound assignment expression (+=, -=, etc.)
    /// </summary>
    private object? EvaluateCompoundAssignmentExpression(CompoundAssignmentExpression assignExpr)
    {
        if (assignExpr.Left is Identifier identifier)
        {
            var rightValue = Evaluate(assignExpr.Right, _sourceCode);
            var currentValue = GetVariable(identifier.Name);
            var newValue = assignExpr.Operator switch
            {
                "+=" => EvaluateAddition(currentValue, rightValue),
                "-=" => EvaluateSubtraction(currentValue, rightValue),
                "*=" => EvaluateMultiplication(currentValue, rightValue),
                "/=" => EvaluateDivision(currentValue, rightValue),
                "%=" => EvaluateModulo(currentValue, rightValue),
                _ => throw new ECEngineException($"Unknown assignment operator: {assignExpr.Operator}",
                    assignExpr.Token?.Line ?? 1, assignExpr.Token?.Column ?? 1, _sourceCode,
                    $"Unsupported assignment operator '{assignExpr.Operator}'")
            };
            SetVariable(identifier.Name, newValue);
            return newValue;
        }
        
        throw new ECEngineException("Invalid assignment target",
            assignExpr.Token?.Line ?? 1, assignExpr.Token?.Column ?? 1, _sourceCode,
            "Assignment target must be an identifier");
    }
    
    /// <summary>
    /// Evaluate 'this' expressions
    /// </summary>
    private object? EvaluateThisExpression(ThisExpression thisExpr)
    {
        if (_thisStack.Count > 0)
        {
            return _thisStack.Peek();
        }
        
        // In global context, 'this' typically refers to global object or null in strict mode
        // For now, return null to indicate global context
        return null;
    }
    
    /// <summary>
    /// Evaluate member assignment expressions (obj.prop = value, obj[prop] = value)
    /// </summary>
    private object? EvaluateMemberAssignmentExpression(MemberAssignmentExpression memberAssignExpr)
    {
        return EvaluateMemberAssignment(memberAssignExpr.Left, memberAssignExpr.Right, "=");
    }
    
    /// <summary>
    /// Evaluate member expressions (obj.prop, obj[prop])
    /// </summary>
    private object? EvaluateMemberExpression(MemberExpression memberExpr)
    {
        var obj = Evaluate(memberExpr.Object, _sourceCode);
        
        if (obj == null)
        {
            throw new ECEngineException("Cannot read properties of null",
                memberExpr.Token?.Line ?? 1, memberExpr.Token?.Column ?? 1, _sourceCode,
                "Attempted to access property of null value");
        }
        
        string propertyName;
        if (memberExpr.Computed)
        {
            // obj[expr] - computed property access
            var propertyValue = Evaluate(memberExpr.ComputedProperty!, _sourceCode);
            propertyName = propertyValue?.ToString() ?? "undefined";
        }
        else
        {
            // obj.prop - direct property access
            propertyName = memberExpr.Property;
        }
        
        return GetObjectProperty(obj, propertyName);
    }
    
    /// <summary>
    /// Addition operation with proper type coercion
    /// </summary>
    private object? EvaluateAddition(object? left, object? right)
    {
        var (coercedLeft, coercedRight) = CoerceForAddition(left, right);
        
        if (coercedLeft is string leftStr && coercedRight is string rightStr)
        {
            return leftStr + rightStr;
        }
        
        return ToNumber(coercedLeft) + ToNumber(coercedRight);
    }
    
    /// <summary>
    /// Subtraction operation
    /// </summary>
    private object? EvaluateSubtraction(object? left, object? right)
    {
        return ToNumber(left) - ToNumber(right);
    }
    
    /// <summary>
    /// Multiplication operation
    /// </summary>
    private object? EvaluateMultiplication(object? left, object? right)
    {
        return ToNumber(left) * ToNumber(right);
    }
    
    /// <summary>
    /// Division operation
    /// </summary>
    private object? EvaluateDivision(object? left, object? right)
    {
        var rightNum = ToNumber(right);
        if (rightNum == 0.0)
        {
            var leftNum = ToNumber(left);
            if (leftNum == 0.0)
                return double.NaN; // 0/0 = NaN
            return leftNum > 0 ? double.PositiveInfinity : double.NegativeInfinity;
        }
        return ToNumber(left) / rightNum;
    }
    
    /// <summary>
    /// Modulo operation
    /// </summary>
    private object? EvaluateModulo(object? left, object? right)
    {
        var leftNum = ToNumber(left);
        var rightNum = ToNumber(right);
        
        if (double.IsNaN(leftNum) || double.IsNaN(rightNum) || double.IsInfinity(leftNum) || rightNum == 0.0)
        {
            return double.NaN;
        }
        
        return leftNum % rightNum;
    }
    
    /// <summary>
    /// Loose equality comparison (==)
    /// </summary>
    private bool EvaluateLooseEquality(object? left, object? right)
    {
        var (coercedLeft, coercedRight) = CoerceForEquality(left, right);
        return Equals(coercedLeft, coercedRight);
    }
    
    /// <summary>
    /// Less than comparison
    /// </summary>
    private bool EvaluateLessThan(object? left, object? right)
    {
        var (coercedLeft, coercedRight) = CoerceForComparison(left, right);
        
        if (coercedLeft is string leftStr && coercedRight is string rightStr)
        {
            return string.Compare(leftStr, rightStr, StringComparison.Ordinal) < 0;
        }
        
        var leftNum = ToNumber(coercedLeft);
        var rightNum = ToNumber(coercedRight);
        
        if (double.IsNaN(leftNum) || double.IsNaN(rightNum))
            return false;
            
        return leftNum < rightNum;
    }
    
    /// <summary>
    /// Greater than comparison
    /// </summary>
    private bool EvaluateGreaterThan(object? left, object? right)
    {
        var (coercedLeft, coercedRight) = CoerceForComparison(left, right);
        
        if (coercedLeft is string leftStr && coercedRight is string rightStr)
        {
            return string.Compare(leftStr, rightStr, StringComparison.Ordinal) > 0;
        }
        
        var leftNum = ToNumber(coercedLeft);
        var rightNum = ToNumber(coercedRight);
        
        if (double.IsNaN(leftNum) || double.IsNaN(rightNum))
            return false;
            
        return leftNum > rightNum;
    }
    
    /// <summary>
    /// Less than or equal comparison
    /// </summary>
    private bool EvaluateLessThanOrEqual(object? left, object? right)
    {
        return EvaluateLessThan(left, right) || EvaluateLooseEquality(left, right);
    }
    
    /// <summary>
    /// Greater than or equal comparison
    /// </summary>
    private bool EvaluateGreaterThanOrEqual(object? left, object? right)
    {
        return EvaluateGreaterThan(left, right) || EvaluateLooseEquality(left, right);
    }
    
    /// <summary>
    /// Logical AND with short-circuit evaluation
    /// </summary>
    private object? EvaluateLogicalAnd(ASTNode left, ASTNode right)
    {
        var leftValue = Evaluate(left, _sourceCode);
        if (!IsTruthy(leftValue))
        {
            return leftValue; // Return falsy value
        }
        return Evaluate(right, _sourceCode); // Return right value
    }
    
    /// <summary>
    /// Logical OR with short-circuit evaluation
    /// </summary>
    private object? EvaluateLogicalOr(ASTNode left, ASTNode right)
    {
        var leftValue = Evaluate(left, _sourceCode);
        if (IsTruthy(leftValue))
        {
            return leftValue; // Return truthy value
        }
        return Evaluate(right, _sourceCode); // Return right value
    }
    
    /// <summary>
    /// Evaluate member assignment (obj.prop = value, obj[prop] = value)
    /// </summary>
    private object? EvaluateMemberAssignment(MemberExpression memberExpr, ASTNode valueNode, string operatorType)
    {
        var obj = Evaluate(memberExpr.Object, _sourceCode);
        var value = Evaluate(valueNode, _sourceCode);
        
        if (obj == null)
        {
            throw new ECEngineException("Cannot set properties of null",
                memberExpr.Token?.Line ?? 1, memberExpr.Token?.Column ?? 1, _sourceCode,
                "Attempted to set property on null value");
        }
        
        string propertyName;
        if (memberExpr.Computed)
        {
            var propertyValue = Evaluate(memberExpr.ComputedProperty!, _sourceCode);
            propertyName = propertyValue?.ToString() ?? "undefined";
        }
        else
        {
            propertyName = memberExpr.Property;
        }
        
        if (operatorType != "=")
        {
            var currentValue = GetObjectProperty(obj, propertyName);
            value = operatorType switch
            {
                "+=" => EvaluateAddition(currentValue, value),
                "-=" => EvaluateSubtraction(currentValue, value),
                "*=" => EvaluateMultiplication(currentValue, value),
                "/=" => EvaluateDivision(currentValue, value),
                "%=" => EvaluateModulo(currentValue, value),
                _ => throw new ECEngineException($"Unknown assignment operator: {operatorType}",
                    memberExpr.Token?.Line ?? 1, memberExpr.Token?.Column ?? 1, _sourceCode,
                    $"Unsupported assignment operator '{operatorType}'")
            };
        }
        
        SetObjectProperty(obj, propertyName, value);
        return value;
    }
    
    /// <summary>
    /// Evaluate bitwise AND operation
    /// </summary>
    private object EvaluateBitwiseAnd(object? left, object? right)
    {
        return (double)(ToInt32(left) & ToInt32(right));
    }
    
    /// <summary>
    /// Evaluate bitwise OR operation
    /// </summary>
    private object EvaluateBitwiseOr(object? left, object? right)
    {
        return (double)(ToInt32(left) | ToInt32(right));
    }
    
    /// <summary>
    /// Evaluate bitwise XOR operation
    /// </summary>
    private object EvaluateBitwiseXor(object? left, object? right)
    {
        return (double)(ToInt32(left) ^ ToInt32(right));
    }
    
    /// <summary>
    /// Evaluate left shift operation
    /// </summary>
    private object EvaluateLeftShift(object? left, object? right)
    {
        return (double)(ToInt32(left) << (ToInt32(right) & 0x1F));
    }
    
    /// <summary>
    /// Evaluate right shift operation (sign-extending)
    /// </summary>
    private object EvaluateRightShift(object? left, object? right)
    {
        return (double)(ToInt32(left) >> (ToInt32(right) & 0x1F));
    }
    
    /// <summary>
    /// Evaluate unsigned right shift operation
    /// </summary>
    private object EvaluateUnsignedRightShift(object? left, object? right)
    {
        var leftValue = ToInt32(left);
        var shiftAmount = ToInt32(right) & 0x1F;
        return (double)((uint)leftValue >> shiftAmount);
    }
}
