using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;

namespace ECEngine.Runtime
{
    public static class UrlHelpers
    {
        public static bool IsSlashProtocol(string protocol)
        {
            var slashProtocols = new[] { "http:", "https:", "ftp:", "file:", "ws:", "wss:" };
            return slashProtocols.Contains(protocol?.ToLower());
        }
    }

    // WHATWG URL Class Implementation
    public class UrlClass
    {
        private string _protocol = "";
        private string _username = "";
        private string _password = "";
        private string _hostname = "";
        private string _port = "";
        private string _pathname = "";
        private string _search = "";
        private string _hash = "";
        private URLSearchParams _searchParams;

        public UrlClass(string input, string? baseUrl = null)
        {
            ParseUrl(input, baseUrl);
            _searchParams = new URLSearchParams(_search);
        }

        // Properties
        public string Protocol
        {
            get => _protocol;
            set
            {
                var normalized = NormalizeProtocol(value);
                if (IsValidProtocol(normalized))
                {
                    _protocol = normalized;
                }
            }
        }

        public string Username
        {
            get => _username;
            set => _username = PercentEncode(value ?? "", UserInfoEncodeSet);
        }

        public string Password
        {
            get => _password;
            set => _password = PercentEncode(value ?? "", UserInfoEncodeSet);
        }

        public string Hostname
        {
            get => _hostname;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _hostname = value.ToLower();
                }
            }
        }

        public string Port
        {
            get => _port;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _port = "";
                    return;
                }

                if (int.TryParse(value, out int portNum) && portNum >= 0 && portNum <= 65535)
                {
                    // Check if it's a default port
                    if (IsDefaultPort(_protocol, portNum))
                    {
                        _port = "";
                    }
                    else
                    {
                        _port = portNum.ToString();
                    }
                }
            }
        }

        public string Host
        {
            get => string.IsNullOrEmpty(_port) ? _hostname : $"{_hostname}:{_port}";
            set
            {
                if (string.IsNullOrEmpty(value)) return;

                var parts = value.Split(':');
                _hostname = parts[0].ToLower();
                _port = parts.Length > 1 ? parts[1] : "";
            }
        }

        public string Pathname
        {
            get => _pathname;
            set => _pathname = PercentEncode(value ?? "/", PathEncodeSet);
        }

        public string Search
        {
            get => _search;
            set
            {
                _search = string.IsNullOrEmpty(value) ? "" : 
                         value.StartsWith("?") ? value : $"?{value}";
                _searchParams = new URLSearchParams(_search);
            }
        }

        public URLSearchParams SearchParams => _searchParams;

        public string Hash
        {
            get => _hash;
            set => _hash = string.IsNullOrEmpty(value) ? "" :
                          value.StartsWith("#") ? value : $"#{value}";
        }

        public string Origin
        {
            get
            {
                if (string.IsNullOrEmpty(_protocol) || _protocol == "file:")
                    return "null";
                
                var port = string.IsNullOrEmpty(_port) ? "" : $":{_port}";
                return $"{_protocol}//{_hostname}{port}";
            }
        }

        public string Href
        {
            get => ToString();
            set => ParseUrl(value);
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            
            result.Append(_protocol);
            if (RequiresSlashes(_protocol))
            {
                result.Append("//");
                
                if (!string.IsNullOrEmpty(_username) || !string.IsNullOrEmpty(_password))
                {
                    result.Append(_username);
                    if (!string.IsNullOrEmpty(_password))
                    {
                        result.Append($":{_password}");
                    }
                    result.Append("@");
                }
                
                result.Append(_hostname);
                if (!string.IsNullOrEmpty(_port))
                {
                    result.Append($":{_port}");
                }
            }
            
            result.Append(_pathname);
            result.Append(_search);
            result.Append(_hash);
            
            return result.ToString();
        }

        public string ToJSON() => ToString();

        // Static methods
        public static bool CanParse(string input, string? baseUrl = null)
        {
            try
            {
                new UrlClass(input, baseUrl);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static UrlClass? Parse(string input, string? baseUrl = null)
        {
            try
            {
                return new UrlClass(input, baseUrl);
            }
            catch
            {
                return null;
            }
        }

        // Helper methods
        private void ParseUrl(string input, string? baseUrl = null)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Invalid URL");

            input = input.Trim();
            
            // Handle relative URLs
            if (!IsAbsoluteUrl(input) && !string.IsNullOrEmpty(baseUrl))
            {
                var baseUri = new UrlClass(baseUrl);
                input = ResolveRelativeUrl(baseUri, input);
            }

            // Parse the URL
            var urlPattern = @"^(?:([^:/?#]+):)?(?://(?:([^/?#@]*)@)?([^/?#:]*|\[[^\]]*\])(?::(\d+))?)?([^?#]*)(?:\?([^#]*))?(?:#(.*))?$";
            var match = Regex.Match(input, urlPattern);

            if (!match.Success)
                throw new ArgumentException("Invalid URL");

            _protocol = NormalizeProtocol(match.Groups[1].Value);
            
            var userInfo = match.Groups[2].Value;
            if (!string.IsNullOrEmpty(userInfo))
            {
                var userParts = userInfo.Split(':');
                _username = PercentDecode(userParts[0]);
                _password = userParts.Length > 1 ? PercentDecode(userParts[1]) : "";
            }

            _hostname = match.Groups[3].Value.ToLower();
            _port = match.Groups[4].Value;
            _pathname = string.IsNullOrEmpty(match.Groups[5].Value) ? "/" : PercentDecode(match.Groups[5].Value);
            _search = string.IsNullOrEmpty(match.Groups[6].Value) ? "" : $"?{match.Groups[6].Value}";
            _hash = string.IsNullOrEmpty(match.Groups[7].Value) ? "" : $"#{match.Groups[7].Value}";

            // Validate and normalize
            if (!IsValidProtocol(_protocol))
                throw new ArgumentException("Invalid protocol");

            // Handle default ports
            if (!string.IsNullOrEmpty(_port) && int.TryParse(_port, out int portNum))
            {
                if (IsDefaultPort(_protocol, portNum))
                    _port = "";
            }
        }

        private string ResolveRelativeUrl(UrlClass baseUri, string relativeUrl)
        {
            if (relativeUrl.StartsWith("//"))
                return $"{baseUri._protocol}{relativeUrl}";
            
            if (relativeUrl.StartsWith("/"))
                return $"{baseUri._protocol}//{baseUri.Host}{relativeUrl}";
            
            if (relativeUrl.StartsWith("?"))
                return $"{baseUri._protocol}//{baseUri.Host}{baseUri._pathname}{relativeUrl}";
            
            if (relativeUrl.StartsWith("#"))
                return $"{baseUri._protocol}//{baseUri.Host}{baseUri._pathname}{baseUri._search}{relativeUrl}";

            // Resolve relative path
            var basePath = baseUri._pathname.EndsWith("/") ? baseUri._pathname : Path.GetDirectoryName(baseUri._pathname)?.Replace('\\', '/') + "/";
            var resolvedPath = Path.Combine(basePath ?? "/", relativeUrl).Replace('\\', '/');
            
            return $"{baseUri._protocol}//{baseUri.Host}{resolvedPath}";
        }

        private bool IsAbsoluteUrl(string url) => url.Contains("://") || url.StartsWith("//");

        private string NormalizeProtocol(string protocol)
        {
            if (string.IsNullOrEmpty(protocol)) return "";
            return protocol.ToLower() + (protocol.EndsWith(":") ? "" : ":");
        }

        private bool IsValidProtocol(string protocol)
        {
            if (string.IsNullOrEmpty(protocol)) return false;
            var schemes = new[] { "http:", "https:", "ftp:", "file:", "ws:", "wss:", "data:", "blob:", "about:" };
            return schemes.Contains(protocol) || Regex.IsMatch(protocol, @"^[a-z][a-z0-9+.-]*:$");
        }

        private bool RequiresSlashes(string protocol)
        {
            var specialSchemes = new[] { "http:", "https:", "ftp:", "file:", "ws:", "wss:" };
            return specialSchemes.Contains(protocol);
        }

        private bool IsDefaultPort(string protocol, int port)
        {
            var defaultPorts = new Dictionary<string, int>
            {
                { "http:", 80 },
                { "https:", 443 },
                { "ftp:", 21 },
                { "ws:", 80 },
                { "wss:", 443 }
            };
            
            return defaultPorts.TryGetValue(protocol, out int defaultPort) && defaultPort == port;
        }

        // Percent encoding sets
        private static readonly HashSet<char> UserInfoEncodeSet = new HashSet<char>
        {
            ' ', '"', '#', '<', '>', '?', '`', '{', '}', '/', ':', ';', '=', '@', '[', ']', '^', '|'
        };

        private static readonly HashSet<char> PathEncodeSet = new HashSet<char>
        {
            ' ', '"', '#', '<', '>', '?', '`', '{', '}'
        };

        private string PercentEncode(string input, HashSet<char> encodeSet)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            var result = new StringBuilder();
            foreach (char c in input)
            {
                if (encodeSet.Contains(c) || c < 0x20 || c > 0x7E)
                {
                    result.Append($"%{((int)c):X2}");
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        private string PercentDecode(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return Uri.UnescapeDataString(input);
        }
    }

    // URLSearchParams Implementation
    public class URLSearchParams
    {
        private List<KeyValuePair<string, string>> _params = new List<KeyValuePair<string, string>>();

        public URLSearchParams() { }

        public URLSearchParams(string queryString)
        {
            if (string.IsNullOrEmpty(queryString)) return;
            
            var query = queryString.StartsWith("?") ? queryString.Substring(1) : queryString;
            var pairs = query.Split('&');
            
            foreach (var pair in pairs)
            {
                if (string.IsNullOrEmpty(pair)) continue;
                
                var parts = pair.Split('=');
                var name = Uri.UnescapeDataString(parts[0]);
                var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
                _params.Add(new KeyValuePair<string, string>(name, value));
            }
        }

        public URLSearchParams(Dictionary<string, object> obj)
        {
            foreach (var kvp in obj)
            {
                _params.Add(new KeyValuePair<string, string>(kvp.Key, kvp.Value?.ToString() ?? ""));
            }
        }

        public void Append(string name, string value)
        {
            _params.Add(new KeyValuePair<string, string>(name, value));
        }

        public void Delete(string name, string? value = null)
        {
            if (value == null)
            {
                _params.RemoveAll(p => p.Key == name);
            }
            else
            {
                _params.RemoveAll(p => p.Key == name && p.Value == value);
            }
        }

        public string? Get(string name)
        {
            var param = _params.FirstOrDefault(p => p.Key == name);
            return param.Key == name ? param.Value : null;
        }

        public List<string> GetAll(string name)
        {
            return _params.Where(p => p.Key == name).Select(p => p.Value).ToList();
        }

        public bool Has(string name, string? value = null)
        {
            if (value == null)
            {
                return _params.Any(p => p.Key == name);
            }
            return _params.Any(p => p.Key == name && p.Value == value);
        }

        public List<string> Keys()
        {
            return _params.Select(p => p.Key).ToList();
        }

        public List<string> Values()
        {
            return _params.Select(p => p.Value).ToList();
        }

        public List<KeyValuePair<string, string>> Entries()
        {
            return new List<KeyValuePair<string, string>>(_params);
        }

        public void Set(string name, string value)
        {
            // Remove all existing entries with this name
            _params.RemoveAll(p => p.Key == name);
            // Add the new entry
            _params.Add(new KeyValuePair<string, string>(name, value));
        }

        public int Size => _params.Count;

        public void Sort()
        {
            _params = _params.OrderBy(p => p.Key).ToList();
        }

        public override string ToString()
        {
            return string.Join("&", _params.Select(p => 
                $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        }

        public void ForEach(Action<string, string, URLSearchParams> callback)
        {
            foreach (var param in _params)
            {
                callback(param.Value, param.Key, this);
            }
        }
    }

    // URL Module Implementation
    public class UrlModule
    {
        public UrlClass URL { get; } = new UrlClass("http://example.com"); // Placeholder
        public URLSearchParams URLSearchParams { get; } = new URLSearchParams();
        
        // Static utility functions
        public UrlFormatFunction Format { get; } = new UrlFormatFunction();
        public DomainToASCIIFunction DomainToASCII { get; } = new DomainToASCIIFunction();
        public DomainToUnicodeFunction DomainToUnicode { get; } = new DomainToUnicodeFunction();
        public FileURLToPathFunction FileURLToPath { get; } = new FileURLToPathFunction();
        public PathToFileURLFunction PathToFileURL { get; } = new PathToFileURLFunction();
        public UrlToHttpOptionsFunction UrlToHttpOptions { get; } = new UrlToHttpOptionsFunction();
        
        // Legacy API (deprecated but still supported)
        public UrlParseFunction Parse { get; } = new UrlParseFunction();
        public UrlResolveFunction Resolve { get; } = new UrlResolveFunction();
        public LegacyUrlFormatFunction LegacyFormat { get; } = new LegacyUrlFormatFunction();
    }

    // URL Constructor Function for ECEngine
    public class UrlConstructorFunction
    {
        public object? Call(List<object?> arguments)
        {
            if (arguments.Count == 0)
                throw new ECEngineException("URL constructor requires at least one argument", 0, 0, "", "TypeError");

            var input = arguments[0]?.ToString() ?? "";
            var baseUrl = arguments.Count > 1 ? arguments[1]?.ToString() : null;

            try
            {
                return new UrlClass(input, baseUrl);
            }
            catch (Exception ex)
            {
                throw new ECEngineException($"Invalid URL: {ex.Message}", 0, 0, "", "TypeError");
            }
        }
    }

    // URLSearchParams Constructor Function
    public class URLSearchParamsConstructorFunction
    {
        public object? Call(List<object?> arguments)
        {
            if (arguments.Count == 0)
                return new URLSearchParams();

            var arg = arguments[0];
            
            if (arg is string str)
                return new URLSearchParams(str);
            
            if (arg is Dictionary<string, object> dict)
                return new URLSearchParams(dict);

            // Handle array of arrays
            if (arg is List<object?> list)
            {
                var urlParams = new URLSearchParams();
                foreach (var item in list)
                {
                    if (item is List<object?> pair && pair.Count >= 2)
                    {
                        urlParams.Append(pair[0]?.ToString() ?? "", pair[1]?.ToString() ?? "");
                    }
                }
                return urlParams;
            }

            return new URLSearchParams();
        }
    }

    // Static URL methods as function classes
    public class UrlFormatFunction
    {
        public object? Call(List<object?> arguments)
        {
            if (arguments.Count == 0)
                return "";

            var url = arguments[0];
            var options = arguments.Count > 1 ? arguments[1] as Dictionary<string, object> : new Dictionary<string, object>();

            if (url is UrlClass urlObj)
            {
                var auth = GetOption(options, "auth", true);
                var fragment = GetOption(options, "fragment", true);
                var search = GetOption(options, "search", true);
                var unicode = GetOption(options, "unicode", false);

                var result = new StringBuilder();
                result.Append(urlObj.Protocol);
                result.Append("//");

                if (auth && (!string.IsNullOrEmpty(urlObj.Username) || !string.IsNullOrEmpty(urlObj.Password)))
                {
                    result.Append(urlObj.Username);
                    if (!string.IsNullOrEmpty(urlObj.Password))
                        result.Append($":{urlObj.Password}");
                    result.Append("@");
                }

                result.Append(urlObj.Hostname);
                if (!string.IsNullOrEmpty(urlObj.Port))
                    result.Append($":{urlObj.Port}");

                result.Append(urlObj.Pathname);

                if (search)
                    result.Append(urlObj.Search);

                if (fragment)
                    result.Append(urlObj.Hash);

                return result.ToString();
            }

            // Handle dictionary/object format
            if (url is Dictionary<string, object> urlDict)
            {
                var result = new StringBuilder();
                
                // Add protocol
                if (urlDict.TryGetValue("protocol", out var protocol))
                    result.Append(protocol?.ToString() ?? "");
                    
                // Add slashes if protocol contains ://
                var protocolStr = protocol?.ToString() ?? "";
                if (protocolStr.Contains(":") && !protocolStr.Contains("://"))
                    result.Append("//");
                
                // Add auth if present
                if (urlDict.TryGetValue("auth", out var auth) && !string.IsNullOrEmpty(auth?.ToString()))
                {
                    result.Append(auth);
                    result.Append("@");
                }
                
                // Add hostname
                if (urlDict.TryGetValue("hostname", out var hostname))
                    result.Append(hostname?.ToString() ?? "");
                else if (urlDict.TryGetValue("host", out var host))
                    result.Append(host?.ToString() ?? "");
                
                // Add port
                if (urlDict.TryGetValue("port", out var port) && !string.IsNullOrEmpty(port?.ToString()))
                    result.Append($":{port}");
                
                // Add pathname
                if (urlDict.TryGetValue("pathname", out var pathname))
                    result.Append(pathname?.ToString() ?? "");
                else if (urlDict.TryGetValue("path", out var path))
                    result.Append(path?.ToString() ?? "");
                
                // Add search
                if (urlDict.TryGetValue("search", out var search) && !string.IsNullOrEmpty(search?.ToString()))
                {
                    var searchStr = search.ToString();
                    if (!searchStr!.StartsWith("?"))
                        result.Append("?");
                    result.Append(searchStr);
                }
                else if (urlDict.TryGetValue("query", out var query) && !string.IsNullOrEmpty(query?.ToString()))
                {
                    result.Append("?");
                    result.Append(query);
                }
                
                // Add hash
                if (urlDict.TryGetValue("hash", out var hash) && !string.IsNullOrEmpty(hash?.ToString()))
                {
                    var hashStr = hash.ToString();
                    if (!hashStr!.StartsWith("#"))
                        result.Append("#");
                    result.Append(hashStr);
                }
                
                return result.ToString();
            }

            return url?.ToString() ?? "";
        }

        private bool GetOption(Dictionary<string, object> options, string key, bool defaultValue)
        {
            return options.TryGetValue(key, out var value) && value is bool b ? b : defaultValue;
        }
    }

    public class DomainToASCIIFunction
    {
        public object? Call(List<object?> arguments)
        {
            if (arguments.Count == 0)
                return "";

            var domain = arguments[0]?.ToString() ?? "";
            
            try
            {
                // Simple ASCII conversion - in a full implementation, this would use Punycode
                return domain.ToLower();
            }
            catch
            {
                return "";
            }
        }
    }

    public class DomainToUnicodeFunction
    {
        public object? Call(List<object?> arguments)
        {
            if (arguments.Count == 0)
                return "";

            var domain = arguments[0]?.ToString() ?? "";
            
            try
            {
                // Simple Unicode conversion - in a full implementation, this would decode Punycode
                return domain;
            }
            catch
            {
                return "";
            }
        }
    }

    public class FileURLToPathFunction
    {
        public object? Call(List<object?> arguments)
        {
            if (arguments.Count == 0)
                return "";

            var url = arguments[0]?.ToString() ?? "";
            
            if (!url.StartsWith("file://"))
                throw new ECEngineException("URL must be a file URL", 0, 0, "", "TypeError");

            var path = url.Substring(7); // Remove "file://"
            
            // Handle Windows paths
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (path.StartsWith("/") && path.Length > 1 && path[2] == ':')
                    path = path.Substring(1); // Remove leading slash for Windows drive paths
                path = path.Replace('/', '\\');
            }

            return Uri.UnescapeDataString(path);
        }
    }

    public class PathToFileURLFunction
    {
        public object? Call(List<object?> arguments)
        {
            if (arguments.Count == 0)
                return "";

            var path = arguments[0]?.ToString() ?? "";
            
            // Convert to forward slashes
            path = path.Replace('\\', '/');
            
            // Handle Windows absolute paths
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && path.Length >= 2 && path[1] == ':')
            {
                path = "/" + path;
            }

            // Ensure absolute path
            if (!path.StartsWith("/"))
                path = "/" + path;

            return $"file://{Uri.EscapeDataString(path).Replace("%2F", "/")}";
        }
    }

    public class UrlToHttpOptionsFunction
    {
        public object? Call(List<object?> arguments)
        {
            if (arguments.Count == 0 || !(arguments[0] is UrlClass url))
                return new Dictionary<string, object>();

            var options = new Dictionary<string, object>
            {
                { "protocol", url.Protocol },
                { "hostname", url.Hostname },
                { "hash", url.Hash },
                { "search", url.Search },
                { "pathname", url.Pathname },
                { "path", url.Pathname + url.Search },
                { "href", url.Href }
            };

            if (!string.IsNullOrEmpty(url.Port) && int.TryParse(url.Port, out int port))
                options["port"] = port;

            if (!string.IsNullOrEmpty(url.Username) || !string.IsNullOrEmpty(url.Password))
                options["auth"] = $"{url.Username}:{url.Password}";

            return options;
        }
    }

    // Legacy URL API Functions
    public class UrlParseFunction
    {
        public object? Call(List<object?> arguments)
        {
            if (arguments.Count == 0)
                return null;

            var urlString = arguments[0]?.ToString() ?? "";
            var parseQueryString = arguments.Count > 1 && arguments[1] is bool b && b;
            var slashesDenoteHost = arguments.Count > 2 && arguments[2] is bool b2 && b2;

            try
            {
                var url = new UrlClass(urlString);
                var result = new Dictionary<string, object>
                {
                    { "protocol", url.Protocol },
                    { "hostname", url.Hostname },
                    { "port", url.Port },
                    { "pathname", url.Pathname },
                    { "search", url.Search },
                    { "hash", url.Hash },
                    { "host", url.Host },
                    { "href", url.Href },
                    { "origin", url.Origin },
                    { "slashes", UrlHelpers.IsSlashProtocol(url.Protocol) }
                };

                if (!string.IsNullOrEmpty(url.Username) || !string.IsNullOrEmpty(url.Password))
                    result["auth"] = $"{url.Username}:{url.Password}";

                result["path"] = url.Pathname + url.Search;

                if (parseQueryString && !string.IsNullOrEmpty(url.Search))
                {
                    result["query"] = new URLSearchParams(url.Search);
                }
                else
                {
                    result["query"] = url.Search.StartsWith("?") ? url.Search.Substring(1) : url.Search;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new ECEngineException($"URL parse error: {ex.Message}", 0, 0, "", "TypeError");
            }
        }
    }

    public class UrlResolveFunction
    {
        public object? Call(List<object?> arguments)
        {
            if (arguments.Count < 2)
                return "";

            var from = arguments[0]?.ToString() ?? "";
            var to = arguments[1]?.ToString() ?? "";

            try
            {
                var resolved = new UrlClass(to, from);
                return resolved.ToString();
            }
            catch
            {
                return to; // Return target URL if resolution fails
            }
        }
    }

    public class LegacyUrlFormatFunction
    {
        public object? Call(List<object?> arguments)
        {
            if (arguments.Count == 0)
                return "";

            var urlObject = arguments[0];
            
            if (urlObject is string str)
            {
                // If it's already a string, parse it first
                try
                {
                    var url = new UrlClass(str);
                    return url.ToString();
                }
                catch
                {
                    return str;
                }
            }

            if (urlObject is Dictionary<string, object> dict)
            {
                var result = new StringBuilder();
                
                if (dict.TryGetValue("protocol", out var protocol))
                    result.Append(protocol.ToString());
                
                if (dict.TryGetValue("slashes", out var slashes) && slashes is bool && (bool)slashes)
                    result.Append("//");
                
                if (dict.TryGetValue("auth", out var auth))
                    result.Append($"{auth}@");
                
                if (dict.TryGetValue("hostname", out var hostname))
                    result.Append(hostname.ToString());
                
                if (dict.TryGetValue("port", out var port))
                    result.Append($":{port}");
                
                if (dict.TryGetValue("pathname", out var pathname))
                    result.Append(pathname.ToString());
                
                if (dict.TryGetValue("search", out var search))
                    result.Append(search.ToString());
                
                if (dict.TryGetValue("hash", out var hash))
                    result.Append(hash.ToString());
                
                return result.ToString();
            }

            return urlObject?.ToString() ?? "";
        }
    }

    // URL Method Function for member access
    public class UrlMethodFunction
    {
        private readonly object _urlObject;
        private readonly string _methodName;

        public UrlMethodFunction(object urlObject, string methodName)
        {
            _urlObject = urlObject;
            _methodName = methodName;
        }

        public object? Call(List<object?> arguments)
        {
            if (_urlObject is UrlClass url)
            {
                switch (_methodName)
                {
                    case "toString":
                        return url.ToString();
                    case "toJSON":
                        return url.ToJSON();
                    default:
                        throw new ECEngineException($"Unknown URL method: {_methodName}", 0, 0, "", "TypeError");
                }
            }

            if (_urlObject is URLSearchParams urlParams)
            {
                switch (_methodName)
                {
                    case "append":
                        if (arguments.Count >= 2)
                            urlParams.Append(arguments[0]?.ToString() ?? "", arguments[1]?.ToString() ?? "");
                        return null;
                    case "delete":
                        if (arguments.Count >= 1)
                            urlParams.Delete(arguments[0]?.ToString() ?? "", arguments.Count > 1 ? arguments[1]?.ToString() : null);
                        return null;
                    case "get":
                        return arguments.Count >= 1 ? urlParams.Get(arguments[0]?.ToString() ?? "") : null;
                    case "getAll":
                        return arguments.Count >= 1 ? urlParams.GetAll(arguments[0]?.ToString() ?? "") : new List<string>();
                    case "has":
                        return arguments.Count >= 1 ? urlParams.Has(arguments[0]?.ToString() ?? "", arguments.Count > 1 ? arguments[1]?.ToString() : null) : false;
                    case "keys":
                        return urlParams.Keys();
                    case "values":
                        return urlParams.Values();
                    case "entries":
                        return urlParams.Entries();
                    case "set":
                        if (arguments.Count >= 2)
                            urlParams.Set(arguments[0]?.ToString() ?? "", arguments[1]?.ToString() ?? "");
                        return null;
                    case "sort":
                        urlParams.Sort();
                        return null;
                    case "toString":
                        return urlParams.ToString();
                    case "forEach":
                        if (arguments.Count >= 1 && arguments[0] is Function callback)
                        {
                            urlParams.ForEach((value, name, searchParams) =>
                            {
                                // Call the callback function
                                // This would need interpreter context to call properly
                            });
                        }
                        return null;
                    default:
                        throw new ECEngineException($"Unknown URLSearchParams method: {_methodName}", 0, 0, "", "TypeError");
                }
            }

            throw new ECEngineException($"Method {_methodName} not found", 0, 0, "", "TypeError");
        }
    }

    public class URLSearchParamsMethodFunction
    {
        private URLSearchParams _searchParams;
        private string _methodName;

        public URLSearchParamsMethodFunction(URLSearchParams searchParams, string methodName)
        {
            _searchParams = searchParams;
            _methodName = methodName;
        }

        public object? Call(List<object?> arguments)
        {
            switch (_methodName)
            {
                case "append":
                    if (arguments.Count >= 2)
                        _searchParams.Append(arguments[0]?.ToString() ?? "", arguments[1]?.ToString() ?? "");
                    return null;
                case "delete":
                    if (arguments.Count >= 1)
                        _searchParams.Delete(arguments[0]?.ToString() ?? "", arguments.Count > 1 ? arguments[1]?.ToString() : null);
                    return null;
                case "get":
                    return arguments.Count >= 1 ? _searchParams.Get(arguments[0]?.ToString() ?? "") : null;
                case "getAll":
                    return arguments.Count >= 1 ? _searchParams.GetAll(arguments[0]?.ToString() ?? "") : new List<string>();
                case "has":
                    return arguments.Count >= 1 ? _searchParams.Has(arguments[0]?.ToString() ?? "", arguments.Count > 1 ? arguments[1]?.ToString() : null) : false;
                case "keys":
                    return _searchParams.Keys();
                case "values":
                    return _searchParams.Values();
                case "entries":
                    return _searchParams.Entries();
                case "set":
                    if (arguments.Count >= 2)
                        _searchParams.Set(arguments[0]?.ToString() ?? "", arguments[1]?.ToString() ?? "");
                    return null;
                case "sort":
                    _searchParams.Sort();
                    return null;
                case "toString":
                    return _searchParams.ToString();
                case "forEach":
                    if (arguments.Count >= 1 && arguments[0] is Function callback)
                    {
                        _searchParams.ForEach((value, name, searchParams) =>
                        {
                            // Call the callback function
                            // This would need interpreter context to call properly
                        });
                    }
                    return null;
                default:
                    throw new ECEngineException($"Unknown URLSearchParams method: {_methodName}", 0, 0, "", "TypeError");
            }
        }
    }
}
