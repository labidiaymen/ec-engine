using ECEngine.AST;

namespace ECEngine.Runtime;

/// <summary>
/// Number module with static methods and constructor
/// </summary>
public class NumberModule
{
    /// <summary>
    /// Number constructor function - converts value to number
    /// </summary>
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            return 0.0;
        
        var value = arguments[0];
        return value switch
        {
            null => 0.0,
            double d => d,
            int i => (double)i,
            float f => (double)f,
            bool b => b ? 1.0 : 0.0,
            string s => ParseStringToNumber(s),
            _ => double.NaN
        };
    }
    
    /// <summary>
    /// Parse string to number following JavaScript rules
    /// </summary>
    private double ParseStringToNumber(string str)
    {
        if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
            return 0.0;
        
        str = str.Trim();
        
        if (str == "")
            return 0.0;
        
        if (str == "Infinity")
            return double.PositiveInfinity;
        
        if (str == "-Infinity")
            return double.NegativeInfinity;
        
        if (str == "NaN")
            return double.NaN;
        
        // Try parsing as double
        if (double.TryParse(str, out double result))
            return result;
        
        // Handle hexadecimal numbers (0x prefix)
        if (str.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(str[2..], System.Globalization.NumberStyles.HexNumber, null, out int hexResult))
                return hexResult;
        }
        
        // Handle binary numbers (0b prefix)
        if (str.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return Convert.ToInt32(str[2..], 2);
            }
            catch
            {
                return double.NaN;
            }
        }
        
        // Handle octal numbers (0o prefix)
        if (str.StartsWith("0o", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return Convert.ToInt32(str[2..], 8);
            }
            catch
            {
                return double.NaN;
            }
        }
        
        // If all else fails, return NaN
        return double.NaN;
    }
}
