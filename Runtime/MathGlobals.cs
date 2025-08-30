namespace ECEngine.Runtime;

/// <summary>
/// Global Math object and functions for ECEngine scripts
/// </summary>

/// <summary>
/// Math module that provides mathematical constants and functions
/// </summary>
public class MathModule
{
    // Mathematical constants
    public double E => Math.E;
    public double PI => Math.PI;
    public double LN2 => Math.Log(2);
    public double LN10 => Math.Log(10);
    public double LOG2E => Math.Log2(Math.E);
    public double LOG10E => Math.Log10(Math.E);
    public double SQRT1_2 => Math.Sqrt(0.5);
    public double SQRT2 => Math.Sqrt(2);

    // Math function objects
    public MathAbsFunction Abs { get; } = new();
    public MathAcosFunction Acos { get; } = new();
    public MathAsinFunction Asin { get; } = new();
    public MathAtanFunction Atan { get; } = new();
    public MathAtan2Function Atan2 { get; } = new();
    public MathCeilFunction Ceil { get; } = new();
    public MathCosFunction Cos { get; } = new();
    public MathExpFunction Exp { get; } = new();
    public MathFloorFunction Floor { get; } = new();
    public MathLogFunction Log { get; } = new();
    public MathMaxFunction Max { get; } = new();
    public MathMinFunction Min { get; } = new();
    public MathPowFunction Pow { get; } = new();
    public MathRandomFunction Random { get; } = new();
    public MathRoundFunction Round { get; } = new();
    public MathSinFunction Sin { get; } = new();
    public MathSqrtFunction Sqrt { get; } = new();
    public MathTanFunction Tan { get; } = new();
    public MathTruncFunction Trunc { get; } = new();
}

/// <summary>
/// Math.abs() function
/// </summary>
public class MathAbsFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Abs(d);
        if (value is int i) return Math.Abs(i);
        if (value is long l) return Math.Abs(l);
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Abs(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Math.acos() function
/// </summary>
public class MathAcosFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Acos(d);
        if (value is int i) return Math.Acos(i);
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Acos(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Math.asin() function
/// </summary>
public class MathAsinFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Asin(d);
        if (value is int i) return Math.Asin(i);
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Asin(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Math.atan() function
/// </summary>
public class MathAtanFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Atan(d);
        if (value is int i) return Math.Atan(i);
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Atan(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Math.atan2() function
/// </summary>
public class MathAtan2Function
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2) return double.NaN;
        
        var y = arguments[0];
        var x = arguments[1];
        
        double yVal = 0, xVal = 0;
        
        if (y is double dy) yVal = dy;
        else if (y is int iy) yVal = iy;
        else if (!double.TryParse(y?.ToString(), out yVal)) return double.NaN;
        
        if (x is double dx) xVal = dx;
        else if (x is int ix) xVal = ix;
        else if (!double.TryParse(x?.ToString(), out xVal)) return double.NaN;
        
        return Math.Atan2(yVal, xVal);
    }
}

/// <summary>
/// Math.ceil() function
/// </summary>
public class MathCeilFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Ceiling(d);
        if (value is int i) return (double)i;
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Ceiling(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Math.cos() function
/// </summary>
public class MathCosFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Cos(d);
        if (value is int i) return Math.Cos(i);
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Cos(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Math.exp() function
/// </summary>
public class MathExpFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Exp(d);
        if (value is int i) return Math.Exp(i);
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Exp(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Math.floor() function
/// </summary>
public class MathFloorFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Floor(d);
        if (value is int i) return (double)i;
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Floor(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Math.log() function
/// </summary>
public class MathLogFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Log(d);
        if (value is int i) return Math.Log(i);
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Log(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Math.max() function
/// </summary>
public class MathMaxFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NegativeInfinity;
        
        double max = double.NegativeInfinity;
        
        foreach (var arg in arguments)
        {
            double value;
            if (arg is double d) value = d;
            else if (arg is int i) value = i;
            else if (double.TryParse(arg?.ToString(), out value)) { }
            else return double.NaN;
            
            if (double.IsNaN(value)) return double.NaN;
            if (value > max) max = value;
        }
        
        return max;
    }
}

/// <summary>
/// Math.min() function
/// </summary>
public class MathMinFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.PositiveInfinity;
        
        double min = double.PositiveInfinity;
        
        foreach (var arg in arguments)
        {
            double value;
            if (arg is double d) value = d;
            else if (arg is int i) value = i;
            else if (double.TryParse(arg?.ToString(), out value)) { }
            else return double.NaN;
            
            if (double.IsNaN(value)) return double.NaN;
            if (value < min) min = value;
        }
        
        return min;
    }
}

/// <summary>
/// Math.pow() function
/// </summary>
public class MathPowFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2) return double.NaN;
        
        var baseValue = arguments[0];
        var exponent = arguments[1];
        
        double baseVal = 0, expVal = 0;
        
        if (baseValue is double db) baseVal = db;
        else if (baseValue is int ib) baseVal = ib;
        else if (!double.TryParse(baseValue?.ToString(), out baseVal)) return double.NaN;
        
        if (exponent is double de) expVal = de;
        else if (exponent is int ie) expVal = ie;
        else if (!double.TryParse(exponent?.ToString(), out expVal)) return double.NaN;
        
        return Math.Pow(baseVal, expVal);
    }
}

/// <summary>
/// Math.random() function
/// </summary>
public class MathRandomFunction
{
    private static readonly Random _random = new();
    
    public object? Call(List<object?> arguments)
    {
        return _random.NextDouble();
    }
}

/// <summary>
/// Math.round() function
/// </summary>
public class MathRoundFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Round(d);
        if (value is int i) return (double)i;
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Round(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Math.sin() function
/// </summary>
public class MathSinFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Sin(d);
        if (value is int i) return Math.Sin(i);
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Sin(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Math.sqrt() function
/// </summary>
public class MathSqrtFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Sqrt(d);
        if (value is int i) return Math.Sqrt(i);
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Sqrt(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Math.tan() function
/// </summary>
public class MathTanFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Tan(d);
        if (value is int i) return Math.Tan(i);
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Tan(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Math.trunc() function
/// </summary>
public class MathTruncFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0) return double.NaN;
        
        var value = arguments[0];
        if (value is double d) return Math.Truncate(d);
        if (value is int i) return (double)i;
        
        if (double.TryParse(value?.ToString(), out var parsed))
            return Math.Truncate(parsed);
            
        return double.NaN;
    }
}

/// <summary>
/// Wrapper function for calling Math methods through member expressions
/// </summary>
public class MathMethodFunction
{
    private readonly object _mathFunction;
    private readonly string _methodName;

    public MathMethodFunction(object mathFunction, string methodName)
    {
        _mathFunction = mathFunction;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        return _mathFunction switch
        {
            MathAbsFunction abs => abs.Call(arguments),
            MathAcosFunction acos => acos.Call(arguments),
            MathAsinFunction asin => asin.Call(arguments),
            MathAtanFunction atan => atan.Call(arguments),
            MathAtan2Function atan2 => atan2.Call(arguments),
            MathCeilFunction ceil => ceil.Call(arguments),
            MathCosFunction cos => cos.Call(arguments),
            MathExpFunction exp => exp.Call(arguments),
            MathFloorFunction floor => floor.Call(arguments),
            MathLogFunction log => log.Call(arguments),
            MathMaxFunction max => max.Call(arguments),
            MathMinFunction min => min.Call(arguments),
            MathPowFunction pow => pow.Call(arguments),
            MathRandomFunction random => random.Call(arguments),
            MathRoundFunction round => round.Call(arguments),
            MathSinFunction sin => sin.Call(arguments),
            MathSqrtFunction sqrt => sqrt.Call(arguments),
            MathTanFunction tan => tan.Call(arguments),
            MathTruncFunction trunc => trunc.Call(arguments),
            _ => throw new ECEngineException($"Unknown Math method: {_methodName}",
                1, 1, "", $"The Math method '{_methodName}' is not implemented")
        };
    }
}
