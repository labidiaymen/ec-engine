using System.Text.RegularExpressions;

namespace ECEngine.Runtime
{
    /// <summary>
    /// JavaScript-like regex test function
    /// </summary>
    public class RegexTestFunction
    {
        private readonly Regex _regex;
        
        public RegexTestFunction(string pattern, string flags)
        {
            var options = RegexOptions.None;
            if (flags.Contains('i')) options |= RegexOptions.IgnoreCase;
            if (flags.Contains('m')) options |= RegexOptions.Multiline;
            
            _regex = new Regex(pattern, options);
        }
        
        public object Call(List<object?> arguments)
        {
            if (arguments.Count == 0) return false;
            
            var input = arguments[0]?.ToString() ?? "";
            return _regex.IsMatch(input);
        }
    }
    
    /// <summary>
    /// JavaScript-like regex exec function
    /// </summary>
    public class RegexExecFunction
    {
        private readonly Regex _regex;
        
        public RegexExecFunction(string pattern, string flags)
        {
            var options = RegexOptions.None;
            if (flags.Contains('i')) options |= RegexOptions.IgnoreCase;
            if (flags.Contains('m')) options |= RegexOptions.Multiline;
            
            _regex = new Regex(pattern, options);
        }
        
        public object? Call(List<object?> arguments)
        {
            if (arguments.Count == 0) return null;
            
            var input = arguments[0]?.ToString() ?? "";
            var match = _regex.Match(input);
            
            if (!match.Success) return null;
            
            // Return array-like object with match results
            var result = new Dictionary<string, object?>();
            var groups = new List<object?>();
            
            // Add full match
            groups.Add(match.Value);
            
            // Add captured groups
            for (int i = 1; i < match.Groups.Count; i++)
            {
                groups.Add(match.Groups[i].Value);
            }
            
            result["0"] = match.Value;
            result["length"] = groups.Count;
            result["index"] = match.Index;
            result["input"] = input;
            
            // Add indexed properties
            for (int i = 0; i < groups.Count; i++)
            {
                result[i.ToString()] = groups[i];
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// JavaScript-like regex toString function
    /// </summary>
    public class RegexToStringFunction
    {
        private readonly string _pattern;
        private readonly string _flags;
        
        public RegexToStringFunction(string pattern, string flags)
        {
            _pattern = pattern;
            _flags = flags;
        }
        
        public object Call(List<object?> arguments)
        {
            return $"/{_pattern}/{_flags}";
        }
    }
}
