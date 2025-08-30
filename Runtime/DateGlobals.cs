namespace ECEngine.Runtime;

/// <summary>
/// Global Date object and functions for ECEngine scripts
/// </summary>

/// <summary>
/// Date constructor function
/// </summary>
public class DateConstructorFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
        {
            // new Date() - current date/time
            return new DateObject(DateTime.Now);
        }
        else if (arguments.Count == 1)
        {
            var arg = arguments[0];
            
            // new Date(milliseconds)
            if (arg is double || arg is int || arg is long)
            {
                var milliseconds = Convert.ToInt64(arg);
                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(milliseconds);
                return new DateObject(dateTime);
            }
            
            // new Date(dateString)
            if (arg is string dateString)
            {
                if (DateTime.TryParse(dateString, out var parsedDate))
                {
                    return new DateObject(parsedDate);
                }
                else
                {
                    return new DateObject(DateTime.MinValue); // Invalid Date
                }
            }
        }
        else if (arguments.Count >= 3)
        {
            // new Date(year, month, day, ...)
            try
            {
                var year = Convert.ToInt32(arguments[0]);
                var month = Convert.ToInt32(arguments[1]) + 1; // JS months are 0-based
                var day = arguments.Count > 2 ? Convert.ToInt32(arguments[2]) : 1;
                var hour = arguments.Count > 3 ? Convert.ToInt32(arguments[3]) : 0;
                var minute = arguments.Count > 4 ? Convert.ToInt32(arguments[4]) : 0;
                var second = arguments.Count > 5 ? Convert.ToInt32(arguments[5]) : 0;
                var millisecond = arguments.Count > 6 ? Convert.ToInt32(arguments[6]) : 0;
                
                var dateTime = new DateTime(year, month, day, hour, minute, second, millisecond);
                return new DateObject(dateTime);
            }
            catch
            {
                return new DateObject(DateTime.MinValue); // Invalid Date
            }
        }
        
        return new DateObject(DateTime.Now);
    }
}

/// <summary>
/// Date.now() static function
/// </summary>
public class DateNowFunction
{
    public object? Call(List<object?> arguments)
    {
        var now = DateTime.UtcNow;
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var milliseconds = (long)(now - epoch).TotalMilliseconds;
        return (double)milliseconds;
    }
}

/// <summary>
/// Date.parse() static function
/// </summary>
public class DateParseFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count == 0)
            return double.NaN;
            
        var dateString = arguments[0]?.ToString();
        if (string.IsNullOrEmpty(dateString))
            return double.NaN;
            
        if (DateTime.TryParse(dateString, out var parsedDate))
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var milliseconds = (long)(parsedDate.ToUniversalTime() - epoch).TotalMilliseconds;
            return (double)milliseconds;
        }
        
        return double.NaN;
    }
}

/// <summary>
/// Date.UTC() static function
/// </summary>
public class DateUTCFunction
{
    public object? Call(List<object?> arguments)
    {
        if (arguments.Count < 2)
            return double.NaN;
            
        try
        {
            var year = Convert.ToInt32(arguments[0]);
            var month = Convert.ToInt32(arguments[1]) + 1; // JS months are 0-based
            var day = arguments.Count > 2 ? Convert.ToInt32(arguments[2]) : 1;
            var hour = arguments.Count > 3 ? Convert.ToInt32(arguments[3]) : 0;
            var minute = arguments.Count > 4 ? Convert.ToInt32(arguments[4]) : 0;
            var second = arguments.Count > 5 ? Convert.ToInt32(arguments[5]) : 0;
            var millisecond = arguments.Count > 6 ? Convert.ToInt32(arguments[6]) : 0;
            
            var dateTime = new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var milliseconds = (long)(dateTime - epoch).TotalMilliseconds;
            return (double)milliseconds;
        }
        catch
        {
            return double.NaN;
        }
    }
}

/// <summary>
/// Date object that wraps a DateTime and provides JavaScript Date methods
/// </summary>
public class DateObject
{
    private readonly DateTime _dateTime;
    private readonly bool _isValid;

    public DateObject(DateTime dateTime)
    {
        _dateTime = dateTime;
        _isValid = dateTime != DateTime.MinValue;
    }

    public DateTime DateTime => _dateTime;
    public bool IsValid => _isValid;

    // Instance methods
    public object? GetTime(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var milliseconds = (long)(_dateTime.ToUniversalTime() - epoch).TotalMilliseconds;
        return (double)milliseconds;
    }

    public object? GetFullYear(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)_dateTime.Year;
    }

    public object? GetMonth(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)(_dateTime.Month - 1); // JS months are 0-based
    }

    public object? GetDate(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)_dateTime.Day;
    }

    public object? GetDay(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)(int)_dateTime.DayOfWeek; // Sunday = 0
    }

    public object? GetHours(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)_dateTime.Hour;
    }

    public object? GetMinutes(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)_dateTime.Minute;
    }

    public object? GetSeconds(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)_dateTime.Second;
    }

    public object? GetMilliseconds(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)_dateTime.Millisecond;
    }

    // UTC versions
    public object? GetUTCFullYear(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)_dateTime.ToUniversalTime().Year;
    }

    public object? GetUTCMonth(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)(_dateTime.ToUniversalTime().Month - 1);
    }

    public object? GetUTCDate(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)_dateTime.ToUniversalTime().Day;
    }

    public object? GetUTCDay(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)(int)_dateTime.ToUniversalTime().DayOfWeek;
    }

    public object? GetUTCHours(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)_dateTime.ToUniversalTime().Hour;
    }

    public object? GetUTCMinutes(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)_dateTime.ToUniversalTime().Minute;
    }

    public object? GetUTCSeconds(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)_dateTime.ToUniversalTime().Second;
    }

    public object? GetUTCMilliseconds(List<object?> arguments)
    {
        if (!_isValid) return double.NaN;
        return (double)_dateTime.ToUniversalTime().Millisecond;
    }

    // String conversion methods
    public object? ToString(List<object?> arguments)
    {
        if (!_isValid) return "Invalid Date";
        return _dateTime.ToString();
    }

    public object? ToDateString(List<object?> arguments)
    {
        if (!_isValid) return "Invalid Date";
        return _dateTime.ToString("ddd MMM dd yyyy");
    }

    public object? ToTimeString(List<object?> arguments)
    {
        if (!_isValid) return "Invalid Date";
        return _dateTime.ToString("HH:mm:ss");
    }

    public object? ToISOString(List<object?> arguments)
    {
        if (!_isValid) return "Invalid Date";
        return _dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    public object? ToUTCString(List<object?> arguments)
    {
        if (!_isValid) return "Invalid Date";
        return _dateTime.ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT";
    }

    public object? ToLocaleDateString(List<object?> arguments)
    {
        if (!_isValid) return "Invalid Date";
        return _dateTime.ToString("MM/dd/yyyy");
    }

    public object? ToLocaleTimeString(List<object?> arguments)
    {
        if (!_isValid) return "Invalid Date";
        return _dateTime.ToString("h:mm:ss tt");
    }

    public object? ToLocaleString(List<object?> arguments)
    {
        if (!_isValid) return "Invalid Date";
        return _dateTime.ToString("MM/dd/yyyy h:mm:ss tt");
    }

    public object? ValueOf(List<object?> arguments)
    {
        return GetTime(arguments);
    }
}

/// <summary>
/// Date global object that provides static methods and constructor
/// </summary>
public class DateModule
{
    private readonly DateConstructorFunction _constructor;
    private readonly DateNowFunction _nowFunction;
    private readonly DateParseFunction _parseFunction;
    private readonly DateUTCFunction _utcFunction;

    public DateModule()
    {
        _constructor = new DateConstructorFunction();
        _nowFunction = new DateNowFunction();
        _parseFunction = new DateParseFunction();
        _utcFunction = new DateUTCFunction();
    }

    public DateConstructorFunction Constructor => _constructor;
    public DateNowFunction Now => _nowFunction;
    public DateParseFunction Parse => _parseFunction;
    public DateUTCFunction UTC => _utcFunction;
}

/// <summary>
/// Helper class for Date method function calls
/// </summary>
public class DateMethodFunction
{
    private readonly DateObject _dateObj;
    private readonly string _methodName;

    public DateMethodFunction(DateObject dateObj, string methodName)
    {
        _dateObj = dateObj;
        _methodName = methodName;
    }

    public object? Call(List<object?> arguments)
    {
        return _methodName switch
        {
            "getTime" => _dateObj.GetTime(arguments),
            "getFullYear" => _dateObj.GetFullYear(arguments),
            "getMonth" => _dateObj.GetMonth(arguments),
            "getDate" => _dateObj.GetDate(arguments),
            "getDay" => _dateObj.GetDay(arguments),
            "getHours" => _dateObj.GetHours(arguments),
            "getMinutes" => _dateObj.GetMinutes(arguments),
            "getSeconds" => _dateObj.GetSeconds(arguments),
            "getMilliseconds" => _dateObj.GetMilliseconds(arguments),
            "getUTCFullYear" => _dateObj.GetUTCFullYear(arguments),
            "getUTCMonth" => _dateObj.GetUTCMonth(arguments),
            "getUTCDate" => _dateObj.GetUTCDate(arguments),
            "getUTCDay" => _dateObj.GetUTCDay(arguments),
            "getUTCHours" => _dateObj.GetUTCHours(arguments),
            "getUTCMinutes" => _dateObj.GetUTCMinutes(arguments),
            "getUTCSeconds" => _dateObj.GetUTCSeconds(arguments),
            "getUTCMilliseconds" => _dateObj.GetUTCMilliseconds(arguments),
            "toString" => _dateObj.ToString(arguments),
            "toDateString" => _dateObj.ToDateString(arguments),
            "toTimeString" => _dateObj.ToTimeString(arguments),
            "toISOString" => _dateObj.ToISOString(arguments),
            "toUTCString" => _dateObj.ToUTCString(arguments),
            "toLocaleDateString" => _dateObj.ToLocaleDateString(arguments),
            "toLocaleTimeString" => _dateObj.ToLocaleTimeString(arguments),
            "toLocaleString" => _dateObj.ToLocaleString(arguments),
            "valueOf" => _dateObj.ValueOf(arguments),
            _ => throw new ECEngineException($"Unknown Date method: {_methodName}", 0, 0, "", "Runtime error")
        };
    }
}
