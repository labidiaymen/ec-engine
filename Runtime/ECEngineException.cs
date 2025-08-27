namespace ECEngine.Runtime;

// Custom exception class that preserves source location information
public class ECEngineException : Exception
{
    public int Line { get; }
    public int Column { get; }
    public string SourceCode { get; }
    public string? ContextInfo { get; }

    public ECEngineException(string message, int line, int column, string sourceCode, string? contextInfo = null) 
        : base(message)
    {
        Line = line;
        Column = column;
        SourceCode = sourceCode;
        ContextInfo = contextInfo;
    }

    public ECEngineException(string message, int line, int column, string sourceCode, Exception innerException, string? contextInfo = null) 
        : base(message, innerException)
    {
        Line = line;
        Column = column;
        SourceCode = sourceCode;
        ContextInfo = contextInfo;
    }

    public string GetFormattedLocation()
    {
        return $"Line {Line}, Column {Column}";
    }

    public string GetSourceCodeWithHighlight()
    {
        var lines = SourceCode.Split('\n');
        if (Line <= 0 || Line > lines.Length)
            return SourceCode;

        var result = new System.Text.StringBuilder();
        
        // Show a few lines of context
        var startLine = Math.Max(1, Line - 2);
        var endLine = Math.Min(lines.Length, Line + 2);

        for (int i = startLine; i <= endLine; i++)
        {
            var lineContent = lines[i - 1]; // Arrays are 0-indexed
            var lineNumber = i.ToString().PadLeft(3);
            
            if (i == Line)
            {
                // Highlight the error line
                result.AppendLine($">>> {lineNumber}: {lineContent}");
                
                // Add a pointer to the exact column
                var pointer = new string(' ', 7 + Column - 1) + "^";
                result.AppendLine($"    {pointer}");
            }
            else
            {
                result.AppendLine($"    {lineNumber}: {lineContent}");
            }
        }

        return result.ToString();
    }
}
