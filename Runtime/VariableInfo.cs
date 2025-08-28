namespace ECEngine.Runtime;

/// <summary>
/// Represents information about a variable including its type and value
/// </summary>
public class VariableInfo
{
    public string Type { get; }
    public object? Value { get; set; }
    public bool IsConstant => Type == "const";

    public VariableInfo(string type, object? value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString()
    {
        return $"{Type} = {Value ?? "undefined"}";
    }
}
