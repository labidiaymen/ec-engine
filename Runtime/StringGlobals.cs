using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ECEngine.Runtime
{
    /// <summary>
    /// String method implementation for ECEngine scripts
    /// Provides JavaScript-like string functionality
    /// </summary>
    public class StringMethodFunction
    {
        private readonly string _stringValue;
        private readonly string _methodName;

        public StringMethodFunction(string stringValue, string methodName)
        {
            _stringValue = stringValue;
            _methodName = methodName;
        }

        public object? Call(List<object?> arguments)
        {
            return _methodName switch
            {
                // Character access methods
                "charAt" => CharAt(arguments),
                "charCodeAt" => CharCodeAt(arguments),
                "codePointAt" => CodePointAt(arguments),
                "at" => At(arguments),
                
                // Search methods
                "indexOf" => IndexOf(arguments),
                "lastIndexOf" => LastIndexOf(arguments),
                "search" => Search(arguments),
                "includes" => Includes(arguments),
                "startsWith" => StartsWith(arguments),
                "endsWith" => EndsWith(arguments),
                
                // Extraction methods
                "slice" => Slice(arguments),
                "substring" => Substring(arguments),
                "substr" => Substr(arguments),
                
                // Case methods
                "toLowerCase" => ToLowerCase(),
                "toUpperCase" => ToUpperCase(),
                "toLocaleLowerCase" => ToLocaleLowerCase(arguments),
                "toLocaleUpperCase" => ToLocaleUpperCase(arguments),
                
                // String building methods
                "concat" => Concat(arguments),
                "repeat" => Repeat(arguments),
                "padStart" => PadStart(arguments),
                "padEnd" => PadEnd(arguments),
                
                // Modification methods
                "trim" => Trim(),
                "trimStart" => TrimStart(),
                "trimEnd" => TrimEnd(),
                "replace" => Replace(arguments),
                "replaceAll" => ReplaceAll(arguments),
                
                // Splitting and joining
                "split" => Split(arguments),
                
                // Pattern matching
                "match" => Match(arguments),
                "matchAll" => MatchAll(arguments),
                
                // Unicode methods
                "normalize" => Normalize(arguments),
                "isWellFormed" => IsWellFormed(),
                "toWellFormed" => ToWellFormed(),
                
                // Comparison
                "localeCompare" => LocaleCompare(arguments),
                
                // Object methods
                "toString" => ToString(),
                "valueOf" => ValueOf(),
                
                // HTML wrapper methods (deprecated but included for compatibility)
                "anchor" => Anchor(arguments),
                "big" => Big(),
                "blink" => Blink(),
                "bold" => Bold(),
                "fixed" => Fixed(),
                "fontcolor" => FontColor(arguments),
                "fontsize" => FontSize(arguments),
                "italics" => Italics(),
                "link" => Link(arguments),
                "small" => Small(),
                "strike" => Strike(),
                "sub" => Sub(),
                "sup" => Sup(),
                
                _ => throw new ECEngineException($"String method {_methodName} not implemented",
                    1, 1, "", $"The method '{_methodName}' is not available on strings")
            };
        }

        // Character access methods
        private object? CharAt(List<object?> arguments)
        {
            var index = arguments.Count > 0 ? ConvertToInt(arguments[0]) : 0;
            if (index < 0 || index >= _stringValue.Length)
                return "";
            return _stringValue[index].ToString();
        }

        private object? CharCodeAt(List<object?> arguments)
        {
            var index = arguments.Count > 0 ? ConvertToInt(arguments[0]) : 0;
            if (index < 0 || index >= _stringValue.Length)
                return double.NaN;
            return (double)_stringValue[index];
        }

        private object? CodePointAt(List<object?> arguments)
        {
            var index = arguments.Count > 0 ? ConvertToInt(arguments[0]) : 0;
            if (index < 0 || index >= _stringValue.Length)
                return null;
            
            var codePoint = char.ConvertToUtf32(_stringValue, index);
            return (double)codePoint;
        }

        private object? At(List<object?> arguments)
        {
            var index = arguments.Count > 0 ? ConvertToInt(arguments[0]) : 0;
            
            // Handle negative indices
            if (index < 0)
                index = _stringValue.Length + index;
                
            if (index < 0 || index >= _stringValue.Length)
                return null;
                
            return _stringValue[index].ToString();
        }

        // Search methods
        private object? IndexOf(List<object?> arguments)
        {
            if (arguments.Count == 0) return -1.0;
            
            var searchValue = arguments[0]?.ToString() ?? "";
            var fromIndex = arguments.Count > 1 ? Math.Max(0, ConvertToInt(arguments[1])) : 0;
            
            if (fromIndex >= _stringValue.Length) return -1.0;
            
            var result = _stringValue.IndexOf(searchValue, fromIndex, StringComparison.Ordinal);
            return (double)result;
        }

        private object? LastIndexOf(List<object?> arguments)
        {
            if (arguments.Count == 0) return -1.0;
            
            var searchValue = arguments[0]?.ToString() ?? "";
            var fromIndex = arguments.Count > 1 ? ConvertToInt(arguments[1]) : _stringValue.Length - 1;
            
            if (fromIndex < 0) return -1.0;
            if (fromIndex >= _stringValue.Length) fromIndex = _stringValue.Length - 1;
            
            var result = _stringValue.LastIndexOf(searchValue, fromIndex, StringComparison.Ordinal);
            return (double)result;
        }

        private object? Search(List<object?> arguments)
        {
            if (arguments.Count == 0) return -1.0;
            
            var pattern = arguments[0]?.ToString() ?? "";
            
            try
            {
                var match = Regex.Match(_stringValue, pattern);
                return match.Success ? (double)match.Index : -1.0;
            }
            catch
            {
                // If pattern is not a valid regex, treat as literal string
                return IndexOf(arguments);
            }
        }

        private object? Includes(List<object?> arguments)
        {
            if (arguments.Count == 0) return false;
            
            var searchValue = arguments[0]?.ToString() ?? "";
            var fromIndex = arguments.Count > 1 ? Math.Max(0, ConvertToInt(arguments[1])) : 0;
            
            if (fromIndex >= _stringValue.Length) return false;
            
            return _stringValue.IndexOf(searchValue, fromIndex, StringComparison.Ordinal) >= 0;
        }

        private object? StartsWith(List<object?> arguments)
        {
            if (arguments.Count == 0) return false;
            
            var searchString = arguments[0]?.ToString() ?? "";
            var position = arguments.Count > 1 ? Math.Max(0, ConvertToInt(arguments[1])) : 0;
            
            if (position >= _stringValue.Length) return false;
            
            return _stringValue.Substring(position).StartsWith(searchString, StringComparison.Ordinal);
        }

        private object? EndsWith(List<object?> arguments)
        {
            if (arguments.Count == 0) return false;
            
            var searchString = arguments[0]?.ToString() ?? "";
            var length = arguments.Count > 1 ? ConvertToInt(arguments[1]) : _stringValue.Length;
            
            if (length < 0) length = 0;
            if (length > _stringValue.Length) length = _stringValue.Length;
            
            var substring = _stringValue.Substring(0, length);
            return substring.EndsWith(searchString, StringComparison.Ordinal);
        }

        // Extraction methods
        private object? Slice(List<object?> arguments)
        {
            var start = arguments.Count > 0 ? ConvertToInt(arguments[0]) : 0;
            var end = arguments.Count > 1 ? ConvertToInt(arguments[1]) : _stringValue.Length;
            
            // Handle negative indices
            if (start < 0) start = Math.Max(0, _stringValue.Length + start);
            if (end < 0) end = Math.Max(0, _stringValue.Length + end);
            
            // Ensure start <= end
            if (start > end) return "";
            
            // Clamp to string bounds
            start = Math.Max(0, Math.Min(start, _stringValue.Length));
            end = Math.Max(0, Math.Min(end, _stringValue.Length));
            
            if (start >= end) return "";
            
            return _stringValue.Substring(start, end - start);
        }

        private object? Substring(List<object?> arguments)
        {
            var start = arguments.Count > 0 ? Math.Max(0, ConvertToInt(arguments[0])) : 0;
            var end = arguments.Count > 1 ? Math.Max(0, ConvertToInt(arguments[1])) : _stringValue.Length;
            
            // Swap if start > end
            if (start > end)
            {
                (start, end) = (end, start);
            }
            
            // Clamp to string bounds
            start = Math.Min(start, _stringValue.Length);
            end = Math.Min(end, _stringValue.Length);
            
            return _stringValue.Substring(start, end - start);
        }

        private object? Substr(List<object?> arguments)
        {
            var start = arguments.Count > 0 ? ConvertToInt(arguments[0]) : 0;
            var length = arguments.Count > 1 ? ConvertToInt(arguments[1]) : _stringValue.Length;
            
            // Handle negative start
            if (start < 0) start = Math.Max(0, _stringValue.Length + start);
            
            // Clamp start to string bounds
            start = Math.Min(start, _stringValue.Length);
            
            // Handle negative or zero length
            if (length <= 0) return "";
            
            // Clamp length
            length = Math.Min(length, _stringValue.Length - start);
            
            return _stringValue.Substring(start, length);
        }

        // Case methods
        private object? ToLowerCase()
        {
            return _stringValue.ToLowerInvariant();
        }

        private object? ToUpperCase()
        {
            return _stringValue.ToUpperInvariant();
        }

        private object? ToLocaleLowerCase(List<object?> arguments)
        {
            var locale = arguments.Count > 0 ? arguments[0]?.ToString() : null;
            
            try
            {
                var culture = string.IsNullOrEmpty(locale) ? CultureInfo.CurrentCulture : new CultureInfo(locale);
                return _stringValue.ToLower(culture);
            }
            catch
            {
                return _stringValue.ToLowerInvariant();
            }
        }

        private object? ToLocaleUpperCase(List<object?> arguments)
        {
            var locale = arguments.Count > 0 ? arguments[0]?.ToString() : null;
            
            try
            {
                var culture = string.IsNullOrEmpty(locale) ? CultureInfo.CurrentCulture : new CultureInfo(locale);
                return _stringValue.ToUpper(culture);
            }
            catch
            {
                return _stringValue.ToUpperInvariant();
            }
        }

        // String building methods
        private object? Concat(List<object?> arguments)
        {
            var result = new StringBuilder(_stringValue);
            foreach (var arg in arguments)
            {
                result.Append(arg?.ToString() ?? "");
            }
            return result.ToString();
        }

        private object? Repeat(List<object?> arguments)
        {
            if (arguments.Count == 0) return "";
            
            var count = ConvertToInt(arguments[0]);
            if (count < 0) throw new ECEngineException("Invalid count value", 1, 1, "", "Repeat count must be non-negative");
            if (count == 0) return "";
            
            var result = new StringBuilder(_stringValue.Length * count);
            for (int i = 0; i < count; i++)
            {
                result.Append(_stringValue);
            }
            return result.ToString();
        }

        private object? PadStart(List<object?> arguments)
        {
            var targetLength = arguments.Count > 0 ? ConvertToInt(arguments[0]) : 0;
            var padString = arguments.Count > 1 ? (arguments[1]?.ToString() ?? " ") : " ";
            
            if (targetLength <= _stringValue.Length) return _stringValue;
            if (string.IsNullOrEmpty(padString)) return _stringValue;
            
            var padLength = targetLength - _stringValue.Length;
            var pad = new StringBuilder();
            
            while (pad.Length < padLength)
            {
                pad.Append(padString);
            }
            
            return pad.ToString().Substring(0, padLength) + _stringValue;
        }

        private object? PadEnd(List<object?> arguments)
        {
            var targetLength = arguments.Count > 0 ? ConvertToInt(arguments[0]) : 0;
            var padString = arguments.Count > 1 ? (arguments[1]?.ToString() ?? " ") : " ";
            
            if (targetLength <= _stringValue.Length) return _stringValue;
            if (string.IsNullOrEmpty(padString)) return _stringValue;
            
            var padLength = targetLength - _stringValue.Length;
            var pad = new StringBuilder();
            
            while (pad.Length < padLength)
            {
                pad.Append(padString);
            }
            
            return _stringValue + pad.ToString().Substring(0, padLength);
        }

        // Modification methods
        private object? Trim()
        {
            return _stringValue.Trim();
        }

        private object? TrimStart()
        {
            return _stringValue.TrimStart();
        }

        private object? TrimEnd()
        {
            return _stringValue.TrimEnd();
        }

        private object? Replace(List<object?> arguments)
        {
            if (arguments.Count < 2) return _stringValue;
            
            var searchValue = arguments[0]?.ToString() ?? "";
            var replaceValue = arguments[1]?.ToString() ?? "";
            
            // For now, do simple string replacement (first occurrence only)
            var index = _stringValue.IndexOf(searchValue, StringComparison.Ordinal);
            if (index == -1) return _stringValue;
            
            return _stringValue.Substring(0, index) + replaceValue + _stringValue.Substring(index + searchValue.Length);
        }

        private object? ReplaceAll(List<object?> arguments)
        {
            if (arguments.Count < 2) return _stringValue;
            
            var searchValue = arguments[0]?.ToString() ?? "";
            var replaceValue = arguments[1]?.ToString() ?? "";
            
            if (string.IsNullOrEmpty(searchValue)) return _stringValue;
            
            return _stringValue.Replace(searchValue, replaceValue);
        }

        // Splitting
        private object? Split(List<object?> arguments)
        {
            var separator = arguments.Count > 0 ? arguments[0]?.ToString() : null;
            var limit = arguments.Count > 1 ? ConvertToInt(arguments[1]) : int.MaxValue;
            
            if (separator == null)
            {
                return new List<object?> { _stringValue };
            }
            
            if (string.IsNullOrEmpty(separator))
            {
                // Split into individual characters
                var chars = _stringValue.Take(limit).Select(c => (object?)c.ToString()).ToList();
                return chars;
            }
            
            var parts = _stringValue.Split(new[] { separator }, StringSplitOptions.None);
            if (limit < parts.Length)
            {
                parts = parts.Take(limit).ToArray();
            }
            
            return parts.Select(p => (object?)p).ToList();
        }

        // Pattern matching (simplified)
        private object? Match(List<object?> arguments)
        {
            if (arguments.Count == 0) return null;
            
            var pattern = arguments[0]?.ToString() ?? "";
            
            try
            {
                var match = Regex.Match(_stringValue, pattern);
                if (!match.Success) return null;
                
                var result = new List<object?> { match.Value };
                foreach (Group group in match.Groups.Cast<Group>().Skip(1))
                {
                    result.Add(group.Success ? group.Value : null);
                }
                
                return result;
            }
            catch
            {
                return null;
            }
        }

        private object? MatchAll(List<object?> arguments)
        {
            if (arguments.Count == 0) return new List<object?>();
            
            var pattern = arguments[0]?.ToString() ?? "";
            
            try
            {
                var matches = Regex.Matches(_stringValue, pattern);
                var result = new List<object?>();
                
                foreach (Match match in matches)
                {
                    var matchArray = new List<object?> { match.Value };
                    foreach (Group group in match.Groups.Cast<Group>().Skip(1))
                    {
                        matchArray.Add(group.Success ? group.Value : null);
                    }
                    result.Add(matchArray);
                }
                
                return result;
            }
            catch
            {
                return new List<object?>();
            }
        }

        // Unicode methods
        private object? Normalize(List<object?> arguments)
        {
            var form = arguments.Count > 0 ? arguments[0]?.ToString() : "NFC";
            
            try
            {
                var normalizationForm = form switch
                {
                    "NFC" => NormalizationForm.FormC,
                    "NFD" => NormalizationForm.FormD,
                    "NFKC" => NormalizationForm.FormKC,
                    "NFKD" => NormalizationForm.FormKD,
                    _ => NormalizationForm.FormC
                };
                
                return _stringValue.Normalize(normalizationForm);
            }
            catch
            {
                return _stringValue;
            }
        }

        private object? IsWellFormed()
        {
            // Check for lone surrogates
            for (int i = 0; i < _stringValue.Length; i++)
            {
                char c = _stringValue[i];
                if (char.IsSurrogate(c))
                {
                    if (char.IsHighSurrogate(c))
                    {
                        if (i + 1 >= _stringValue.Length || !char.IsLowSurrogate(_stringValue[i + 1]))
                            return false;
                        i++; // Skip the low surrogate
                    }
                    else
                    {
                        return false; // Lone low surrogate
                    }
                }
            }
            return true;
        }

        private object? ToWellFormed()
        {
            var result = new StringBuilder();
            
            for (int i = 0; i < _stringValue.Length; i++)
            {
                char c = _stringValue[i];
                if (char.IsSurrogate(c))
                {
                    if (char.IsHighSurrogate(c))
                    {
                        if (i + 1 < _stringValue.Length && char.IsLowSurrogate(_stringValue[i + 1]))
                        {
                            result.Append(c);
                            result.Append(_stringValue[i + 1]);
                            i++; // Skip the low surrogate
                        }
                        else
                        {
                            result.Append('\uFFFD'); // Replacement character
                        }
                    }
                    else
                    {
                        result.Append('\uFFFD'); // Replacement character for lone low surrogate
                    }
                }
                else
                {
                    result.Append(c);
                }
            }
            
            return result.ToString();
        }

        // Comparison
        private object? LocaleCompare(List<object?> arguments)
        {
            if (arguments.Count == 0) return 0.0;
            
            var compareString = arguments[0]?.ToString() ?? "";
            
            // Simplified locale comparison
            var result = string.Compare(_stringValue, compareString, StringComparison.CurrentCulture);
            return (double)Math.Sign(result);
        }

        // Object methods
        private new object? ToString()
        {
            return _stringValue;
        }

        private object? ValueOf()
        {
            return _stringValue;
        }

        // HTML wrapper methods (deprecated but included for compatibility)
        private object? Anchor(List<object?> arguments)
        {
            var name = arguments.Count > 0 ? (arguments[0]?.ToString() ?? "").Replace("\"", "&quot;") : "";
            return $"<a name=\"{name}\">{_stringValue}</a>";
        }

        private object? Big()
        {
            return $"<big>{_stringValue}</big>";
        }

        private object? Blink()
        {
            return $"<blink>{_stringValue}</blink>";
        }

        private object? Bold()
        {
            return $"<b>{_stringValue}</b>";
        }

        private object? Fixed()
        {
            return $"<tt>{_stringValue}</tt>";
        }

        private object? FontColor(List<object?> arguments)
        {
            var color = arguments.Count > 0 ? (arguments[0]?.ToString() ?? "").Replace("\"", "&quot;") : "";
            return $"<font color=\"{color}\">{_stringValue}</font>";
        }

        private object? FontSize(List<object?> arguments)
        {
            var size = arguments.Count > 0 ? (arguments[0]?.ToString() ?? "").Replace("\"", "&quot;") : "";
            return $"<font size=\"{size}\">{_stringValue}</font>";
        }

        private object? Italics()
        {
            return $"<i>{_stringValue}</i>";
        }

        private object? Link(List<object?> arguments)
        {
            var url = arguments.Count > 0 ? (arguments[0]?.ToString() ?? "").Replace("\"", "&quot;") : "";
            return $"<a href=\"{url}\">{_stringValue}</a>";
        }

        private object? Small()
        {
            return $"<small>{_stringValue}</small>";
        }

        private object? Strike()
        {
            return $"<strike>{_stringValue}</strike>";
        }

        private object? Sub()
        {
            return $"<sub>{_stringValue}</sub>";
        }

        private object? Sup()
        {
            return $"<sup>{_stringValue}</sup>";
        }

        // Helper method to convert values to integers
        private int ConvertToInt(object? value)
        {
            return value switch
            {
                double d => (int)d,
                int i => i,
                string s when int.TryParse(s, out var result) => result,
                _ => 0
            };
        }
    }

    /// <summary>
    /// String static methods
    /// </summary>
    public class StringStaticModule
    {
        public static object? FromCharCode(List<object?> arguments)
        {
            var result = new StringBuilder();
            foreach (var arg in arguments)
            {
                var code = ConvertToInt(arg);
                if (code >= 0 && code <= 65535)
                {
                    result.Append((char)code);
                }
            }
            return result.ToString();
        }

        public static object? FromCodePoint(List<object?> arguments)
        {
            var result = new StringBuilder();
            foreach (var arg in arguments)
            {
                var code = ConvertToInt(arg);
                if (code >= 0 && code <= 0x10FFFF)
                {
                    try
                    {
                        result.Append(char.ConvertFromUtf32(code));
                    }
                    catch
                    {
                        // Invalid code point, skip
                    }
                }
            }
            return result.ToString();
        }

        public static object? Raw(List<object?> arguments)
        {
            // Simplified implementation - in real JS this works with template literals
            if (arguments.Count == 0) return "";
            return arguments[0]?.ToString() ?? "";
        }

        private static int ConvertToInt(object? value)
        {
            return value switch
            {
                double d => (int)d,
                int i => i,
                string s when int.TryParse(s, out var result) => result,
                _ => 0
            };
        }
    }

    /// <summary>
    /// String module with static methods and constructor
    /// </summary>
    public class StringModule
    {
        public StringStaticMethodFunction fromCharCode { get; }
        public StringStaticMethodFunction fromCodePoint { get; }
        public StringStaticMethodFunction raw { get; }

        public StringModule()
        {
            fromCharCode = new StringStaticMethodFunction(StringStaticModule.FromCharCode, "fromCharCode");
            fromCodePoint = new StringStaticMethodFunction(StringStaticModule.FromCodePoint, "fromCodePoint");
            raw = new StringStaticMethodFunction(StringStaticModule.Raw, "raw");
        }

        /// <summary>
        /// String constructor function
        /// </summary>
        public object? Call(List<object?> arguments)
        {
            if (arguments.Count == 0)
                return "";
            
            var value = arguments[0];
            return value switch
            {
                null => "null",
                string s => s,
                bool b => b ? "true" : "false",
                double d => d.ToString(),
                int i => i.ToString(),
                _ => value.ToString() ?? ""
            };
        }
    }

    /// <summary>
    /// Function wrapper for String static methods
    /// </summary>
    public class StringStaticMethodFunction
    {
        private readonly Func<List<object?>, object?> _method;
        private readonly string _name;

        public StringStaticMethodFunction(Func<List<object?>, object?> method, string name)
        {
            _method = method;
            _name = name;
        }

        public object? Call(List<object?> arguments)
        {
            try
            {
                return _method(arguments);
            }
            catch (Exception ex)
            {
                throw new ECEngineException($"Error in String.{_name}: {ex.Message}",
                    1, 1, "", $"The static method String.{_name} encountered an error");
            }
        }
    }
}
