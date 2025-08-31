namespace ECEngine.Runtime;

// Stub for implementing console.log
public static class ConsoleRuntime
{
    public static void Log(object value)
    {
        // Handle JavaScript-compatible formatting
        var output = FormatForJavaScript(value);
        Console.WriteLine(output);
        Console.Out.Flush(); // Ensure output is flushed immediately
    }
    
    private static string FormatForJavaScript(object? value)
    {
        return value switch
        {
            double d when double.IsPositiveInfinity(d) => "Infinity",
            double d when double.IsNegativeInfinity(d) => "-Infinity",
            double d when double.IsNaN(d) => "NaN",
            float f when float.IsPositiveInfinity(f) => "Infinity",
            float f when float.IsNegativeInfinity(f) => "-Infinity", 
            float f when float.IsNaN(f) => "NaN",
            _ => value?.ToString() ?? "undefined"
        };
    }
}
