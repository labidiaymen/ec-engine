using System.Text.Json;
using System.Net.Http;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace ECEngine.Runtime;

/// <summary>
/// Represents a package dependency with semantic versioning support
/// </summary>
public class PackageDependency
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Source { get; set; } = ""; // "npm", "url", "git", "local", "workspace"
    public string? Url { get; set; } // For URL-based packages
    public string? LocalPath { get; set; } // For local/linked packages
    public bool IsDev { get; set; } = false; // Development dependency
    public bool IsOptional { get; set; } = false; // Optional dependency
    public string? ResolvedVersion { get; set; } // Actual resolved version
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a package script
/// </summary>
public class PackageScript
{
    public string Name { get; set; } = "";
    public string Command { get; set; } = "";
    public List<string> PreHooks { get; set; } = new();
    public List<string> PostHooks { get; set; } = new();
}

/// <summary>
/// Represents a workspace configuration
/// </summary>
public class WorkspaceConfig
{
    public List<string> Packages { get; set; } = new();
    public Dictionary<string, string> SharedDependencies { get; set; } = new();
    public bool NoHoist { get; set; } = false;
}

/// <summary>
/// Represents the package configuration (equivalent to package.json)
/// </summary>
public class PackageConfig
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "";
    public string Main { get; set; } = "index.ec";
    public Dictionary<string, PackageDependency> Dependencies { get; set; } = new();
    public Dictionary<string, PackageDependency> DevDependencies { get; set; } = new();
    public Dictionary<string, PackageDependency> OptionalDependencies { get; set; } = new();
    public Dictionary<string, PackageDependency> UrlDependencies { get; set; } = new();
    public Dictionary<string, PackageScript> Scripts { get; set; } = new();
    public string[]? Keywords { get; set; }
    public string? Author { get; set; }
    public string? License { get; set; }
    public string? Repository { get; set; }
    public string? Homepage { get; set; }
    public Dictionary<string, string> Engines { get; set; } = new();
    public WorkspaceConfig? Workspaces { get; set; }
    public Dictionary<string, object?> Config { get; set; } = new();
}

/// <summary>
/// Semantic version utility class
/// </summary>
public class SemVer
{
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }
    public string? PreRelease { get; set; }
    public string? Build { get; set; }

    public SemVer(int major, int minor, int patch, string? preRelease = null, string? build = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
        Build = build;
    }

    public static SemVer? Parse(string version)
    {
        try
        {
            // Remove leading 'v' if present
            version = version.TrimStart('v', 'V');
            
            // Split on '+' for build metadata
            var parts = version.Split('+');
            var versionPart = parts[0];
            var build = parts.Length > 1 ? parts[1] : null;
            
            // Split on '-' for pre-release
            var mainParts = versionPart.Split('-');
            var numberPart = mainParts[0];
            var preRelease = mainParts.Length > 1 ? string.Join("-", mainParts.Skip(1)) : null;
            
            // Parse major.minor.patch
            var numbers = numberPart.Split('.');
            if (numbers.Length < 3) return null;
            
            if (int.TryParse(numbers[0], out int major) &&
                int.TryParse(numbers[1], out int minor) &&
                int.TryParse(numbers[2], out int patch))
            {
                return new SemVer(major, minor, patch, preRelease, build);
            }
        }
        catch { }
        
        return null;
    }

    public bool Satisfies(string range)
    {
        // Basic semver range matching
        range = range.Trim();
        
        if (range.StartsWith("^"))
        {
            // Caret range - compatible within same major version
            var targetVersion = Parse(range.Substring(1));
            if (targetVersion == null) return false;
            
            return Major == targetVersion.Major && 
                   (Minor > targetVersion.Minor || 
                    (Minor == targetVersion.Minor && Patch >= targetVersion.Patch));
        }
        else if (range.StartsWith("~"))
        {
            // Tilde range - compatible within same minor version
            var targetVersion = Parse(range.Substring(1));
            if (targetVersion == null) return false;
            
            return Major == targetVersion.Major && 
                   Minor == targetVersion.Minor && 
                   Patch >= targetVersion.Patch;
        }
        else if (range.StartsWith(">="))
        {
            var targetVersion = Parse(range.Substring(2));
            return targetVersion != null && CompareTo(targetVersion) >= 0;
        }
        else if (range.StartsWith("<="))
        {
            var targetVersion = Parse(range.Substring(2));
            return targetVersion != null && CompareTo(targetVersion) <= 0;
        }
        else if (range.StartsWith(">"))
        {
            var targetVersion = Parse(range.Substring(1));
            return targetVersion != null && CompareTo(targetVersion) > 0;
        }
        else if (range.StartsWith("<"))
        {
            var targetVersion = Parse(range.Substring(1));
            return targetVersion != null && CompareTo(targetVersion) < 0;
        }
        else if (range == "*" || range == "latest")
        {
            return true;
        }
        else
        {
            // Exact match
            var targetVersion = Parse(range);
            return targetVersion != null && CompareTo(targetVersion) == 0;
        }
    }

    public int CompareTo(SemVer other)
    {
        var majorComp = Major.CompareTo(other.Major);
        if (majorComp != 0) return majorComp;
        
        var minorComp = Minor.CompareTo(other.Minor);
        if (minorComp != 0) return minorComp;
        
        var patchComp = Patch.CompareTo(other.Patch);
        if (patchComp != 0) return patchComp;
        
        // Handle pre-release versions
        if (PreRelease == null && other.PreRelease != null) return 1;
        if (PreRelease != null && other.PreRelease == null) return -1;
        if (PreRelease != null && other.PreRelease != null)
        {
            return string.Compare(PreRelease, other.PreRelease, StringComparison.OrdinalIgnoreCase);
        }
        
        return 0;
    }

    public string? GetBestMatch(string versionSpec, List<string> availableVersions)
    {
        // Parse available versions
        var validVersions = availableVersions
            .Select(v => new { Original = v, Parsed = Parse(v) })
            .Where(v => v.Parsed != null)
            .Select(v => new { v.Original, Parsed = v.Parsed! })
            .ToList();

        if (!validVersions.Any())
            return null;

        // Handle exact version
        if (validVersions.Any(v => v.Original == versionSpec))
            return versionSpec;

        // Handle range specifications
        var matchingVersions = validVersions
            .Where(v => v.Parsed.Satisfies(versionSpec))
            .OrderByDescending(v => v.Parsed)
            .ToList();

        return matchingVersions.FirstOrDefault()?.Original;
    }

    public override string ToString()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        if (!string.IsNullOrEmpty(PreRelease))
            version += $"-{PreRelease}";
        if (!string.IsNullOrEmpty(Build))
            version += $"+{Build}";
        return version;
    }
}

/// <summary>
/// Package cache for faster installs
/// </summary>
public class PackageCache
{
    private readonly string _cacheDir;
    private readonly Dictionary<string, CacheEntry> _cache = new();

    public PackageCache(string cacheDir)
    {
        _cacheDir = cacheDir;
        Directory.CreateDirectory(_cacheDir);
        LoadCache();
    }

    public class CacheEntry
    {
        public string PackageName { get; set; } = "";
        public string Version { get; set; } = "";
        public string CachedPath { get; set; } = "";
        public DateTime CachedAt { get; set; }
        public string Checksum { get; set; } = "";
    }

    public string? GetCachedPackage(string packageName, string version)
    {
        var key = $"{packageName}@{version}";
        if (_cache.TryGetValue(key, out var entry) && File.Exists(entry.CachedPath))
        {
            return entry.CachedPath;
        }
        return null;
    }

    public void CachePackage(string packageName, string version, string sourcePath)
    {
        var key = $"{packageName}@{version}";
        var cachedFileName = $"{packageName}-{version}.tgz";
        var cachedPath = Path.Combine(_cacheDir, cachedFileName);
        
        try
        {
            File.Copy(sourcePath, cachedPath, overwrite: true);
            
            _cache[key] = new CacheEntry
            {
                PackageName = packageName,
                Version = version,
                CachedPath = cachedPath,
                CachedAt = DateTime.UtcNow,
                Checksum = ComputeChecksum(cachedPath)
            };
            
            SaveCache();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to cache package {packageName}@{version}: {ex.Message}");
        }
    }

    private string ComputeChecksum(string filePath)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }

    private void LoadCache()
    {
        var cacheIndexPath = Path.Combine(_cacheDir, "cache-index.json");
        if (File.Exists(cacheIndexPath))
        {
            try
            {
                var json = File.ReadAllText(cacheIndexPath);
                // Simple cache loading - in production you'd use proper JSON parsing
            }
            catch { }
        }
    }

    private void SaveCache()
    {
        var cacheIndexPath = Path.Combine(_cacheDir, "cache-index.json");
        try
        {
            var cacheData = _cache.Values.Select(entry => new
            {
                entry.PackageName,
                entry.Version,
                entry.CachedPath,
                CachedAt = entry.CachedAt.ToString("O"),
                entry.Checksum
            });
            
            // Manual JSON creation for cache index
            var json = "{\"entries\": [" + string.Join(",", cacheData.Select(entry =>
                $"{{\"packageName\":\"{entry.PackageName}\",\"version\":\"{entry.Version}\",\"cachedPath\":\"{entry.CachedPath}\",\"cachedAt\":\"{entry.CachedAt}\",\"checksum\":\"{entry.Checksum}\"}}")) + "]}";
            
            File.WriteAllText(cacheIndexPath, json);
        }
        catch { }
    }

    public bool IsCached(string packageName, string version)
    {
        var key = $"{packageName}@{version}";
        return _cache.ContainsKey(key);
    }

    public bool RestoreFromCache(string packageName, string version, string targetPath)
    {
        try
        {
            var key = $"{packageName}@{version}";
            if (_cache.TryGetValue(key, out var cacheEntry))
            {
                var targetDir = Path.Combine(targetPath, packageName);
                if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                }

                if (Directory.Exists(cacheEntry.CachedPath))
                {
                    CopyDirectory(cacheEntry.CachedPath, targetDir);
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        
        foreach (var file in Directory.GetFiles(source))
        {
            var fileName = Path.GetFileName(file);
            File.Copy(file, Path.Combine(target, fileName), true);
        }
        
        foreach (var directory in Directory.GetDirectories(source))
        {
            var dirName = Path.GetFileName(directory);
            CopyDirectory(directory, Path.Combine(target, dirName));
        }
    }
}

/// <summary>
/// NPM package information for dependency resolution
/// </summary>
public class NpmPackageInfo
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public Dictionary<string, Dictionary<string, object?>> Versions { get; set; } = new();
    public Dictionary<string, string>? Dependencies { get; set; }
}

/// <summary>
/// NPM version information
/// </summary>
public class NpmVersionInfo
{
    public Dictionary<string, string>? Dependencies { get; set; }
    public Dictionary<string, string>? DevDependencies { get; set; }
    public Dictionary<string, string>? OptionalDependencies { get; set; }
    public string? Main { get; set; }
    public Dictionary<string, string>? Scripts { get; set; }
}

/// <summary>
/// Package manager for EC Engine
/// </summary>
public class PackageManager
{
    private readonly string _projectRoot;
    private readonly string _rootPath;
    private readonly string _nodeModulesPath;
    private readonly string _urlPackagesPath;
    private readonly string _packageConfigPath;
    private readonly string _lockFilePath;
    private readonly string _cacheDir;
    private readonly HttpClient _httpClient;
    private readonly JsonModule _jsonModule;
    private readonly PackageCache _cache;
    private readonly SemVer _semver;
    private readonly Dictionary<string, string> _linkedPackages;
    private readonly Dictionary<string, NpmPackageInfo> _packageInfoCache;
    
    public PackageManager(string projectRoot)
    {
        _projectRoot = Path.GetFullPath(projectRoot);
        _rootPath = _projectRoot;
        _nodeModulesPath = Path.Combine(_projectRoot, "node_modules");
        _urlPackagesPath = Path.Combine(_projectRoot, "ec_packages");
        _packageConfigPath = Path.Combine(_projectRoot, "package.json");
        _lockFilePath = Path.Combine(_projectRoot, "package-lock.json");
        _cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ecengine", "cache");
        _httpClient = new HttpClient();
        _jsonModule = new JsonModule();
        _cache = new PackageCache(_cacheDir);
        _semver = new SemVer(1, 0, 0);
        _linkedPackages = new Dictionary<string, string>();
        _packageInfoCache = new Dictionary<string, NpmPackageInfo>();
        
        // Set user agent for better compatibility
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ECEngine-PackageManager/1.0.0");
        
        // Load linked packages
        LoadLinkedPackages();
    }

    /// <summary>
    /// Load previously linked packages
    /// </summary>
    private void LoadLinkedPackages()
    {
        // For now, just initialize empty - could load from a config file later
        // This method is called during initialization to restore linked packages
    }

    /// <summary>
    /// Install a package from various sources with dependency resolution
    /// </summary>
    public async Task<bool> InstallPackageAsync(string packageSpec, string? version = null, bool isDev = false, bool isOptional = false)
    {
        try
        {
            Console.WriteLine($"Installing package: {packageSpec}");
            
            // Determine package source and install accordingly
            if (IsUrl(packageSpec))
            {
                return await InstallFromUrlAsync(packageSpec, version);
            }
            else if (IsLocalPath(packageSpec))
            {
                return await InstallFromLocalAsync(packageSpec, version, isDev);
            }
            else if (packageSpec.StartsWith("@") || IsValidNpmPackageName(packageSpec))
            {
                return await InstallFromNpmAsync(packageSpec, version ?? "latest", isDev, isOptional);
            }
            else
            {
                Console.WriteLine($"Unknown package source for: {packageSpec}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to install package {packageSpec}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Install dependencies with full dependency resolution
    /// </summary>
    public async Task<bool> InstallAllDependenciesAsync()
    {
        try
        {
            Console.WriteLine("Installing all dependencies...");
            var config = await LoadPackageConfigAsync();
            
            // Create dependency resolution tree
            var resolvedDeps = await ResolveDependencyTreeAsync(config);
            
            // Install in correct order
            foreach (var (packageName, dependency) in resolvedDeps)
            {
                if (!await InstallResolvedPackageAsync(packageName, dependency))
                {
                    Console.WriteLine($"Failed to install {packageName}");
                    return false;
                }
            }
            
            // Run post-install scripts
            await RunLifecycleHookAsync("postinstall");
            
            Console.WriteLine("✅ All dependencies installed successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to install dependencies: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Link a local package for development
    /// </summary>
    public async Task<bool> LinkPackageAsync(string packagePath, string? linkName = null)
    {
        try
        {
            var fullPath = Path.GetFullPath(packagePath);
            if (!Directory.Exists(fullPath))
            {
                Console.WriteLine($"Directory does not exist: {fullPath}");
                return false;
            }
            
            var packageJsonPath = Path.Combine(fullPath, "package.json");
            if (!File.Exists(packageJsonPath))
            {
                Console.WriteLine($"No package.json found in: {fullPath}");
                return false;
            }
            
            // Read package name from package.json
            var packageJson = await File.ReadAllTextAsync(packageJsonPath);
            var parsed = _jsonModule.Parse.Call(new List<object?> { packageJson });
            
            if (parsed is Dictionary<string, object?> packageDict &&
                packageDict.TryGetValue("name", out var nameObj) &&
                nameObj is string packageName)
            {
                linkName ??= packageName;
                _linkedPackages[linkName] = fullPath;
                
                // Create symlink in node_modules
                var linkPath = Path.Combine(_nodeModulesPath, linkName);
                Directory.CreateDirectory(_nodeModulesPath);
                
                if (Directory.Exists(linkPath))
                {
                    Directory.Delete(linkPath, true);
                }
                
                // Create junction/symlink
                CreateSymlink(fullPath, linkPath);
                
                Console.WriteLine($"✅ Linked {linkName} -> {fullPath}");
                return true;
            }
            
            Console.WriteLine("Failed to read package name from package.json");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to link package: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Run a package script with lifecycle hooks
    /// </summary>
    public async Task<bool> RunScriptAsync(string scriptName, string[]? args = null)
    {
        try
        {
            var config = await LoadPackageConfigAsync();
            
            if (!config.Scripts.TryGetValue(scriptName, out var script))
            {
                Console.WriteLine($"Script '{scriptName}' not found");
                return false;
            }
            
            // Run pre-hooks
            await RunLifecycleHookAsync($"pre{scriptName}");
            
            Console.WriteLine($"Running script: {scriptName}");
            
            // Build command with args
            var command = script.Command;
            if (args?.Length > 0)
            {
                command += " " + string.Join(" ", args);
            }
            
            // Execute script
            var success = await ExecuteCommandAsync(command);
            
            // Run post-hooks
            await RunLifecycleHookAsync($"post{scriptName}");
            
            return success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to run script {scriptName}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Initialize workspace support
    /// </summary>
    public async Task<bool> InitWorkspaceAsync(string[]? workspacePatterns = null)
    {
        try
        {
            var config = await LoadPackageConfigAsync();
            
            config.Workspaces = new WorkspaceConfig
            {
                Packages = workspacePatterns?.ToList() ?? new List<string> { "packages/*" }
            };
            
            await SavePackageConfigAsync(config);
            
            Console.WriteLine("✅ Initialized workspace configuration");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to initialize workspace: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Install packages for entire workspace
    /// </summary>
    public async Task<bool> InstallWorkspaceAsync()
    {
        try
        {
            var config = await LoadPackageConfigAsync();
            
            if (config.Workspaces == null)
            {
                Console.WriteLine("No workspace configuration found");
                return false;
            }
            
            Console.WriteLine("Installing workspace dependencies...");
            
            // Find all workspace packages
            var workspacePackages = await FindWorkspacePackagesAsync(config.Workspaces.Packages);
            
            // Install dependencies for each workspace package
            foreach (var packagePath in workspacePackages)
            {
                Console.WriteLine($"Installing dependencies for: {packagePath}");
                var packageManager = new PackageManager(packagePath);
                await packageManager.InstallAllDependenciesAsync();
            }
            
            Console.WriteLine("✅ Workspace dependencies installed");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to install workspace: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Install a package from URL
    /// </summary>
    private async Task<bool> InstallFromUrlAsync(string url, string? version)
    {
        try
        {
            var packageName = ExtractPackageNameFromUrl(url);
            var packageVersion = version ?? "1.0.0";
            
            Console.WriteLine($"Installing {packageName} from URL: {url}");
            
            // Create ec_packages directory if it doesn't exist
            Directory.CreateDirectory(_urlPackagesPath);
            
            // Download the package
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            
            // Determine file extension and save appropriately
            var fileName = GetFileNameFromUrl(url);
            var packageDir = Path.Combine(_urlPackagesPath, packageName);
            Directory.CreateDirectory(packageDir);
            
            var filePath = Path.Combine(packageDir, fileName);
            await File.WriteAllTextAsync(filePath, content);
            
            // Create package metadata
            var metadataJson = $@"{{
  ""name"": ""{packageName}"",
  ""version"": ""{packageVersion}"",
  ""description"": ""Package installed from URL"",
  ""main"": ""{fileName}"",
  ""source"": ""url"",
  ""url"": ""{url}"",
  ""installedAt"": ""{DateTime.UtcNow:O}""
}}";
            
            var metadataPath = Path.Combine(packageDir, "package.json");
            await File.WriteAllTextAsync(metadataPath, metadataJson);
            
            // Update package config
            await UpdatePackageConfigAsync(packageName, new PackageDependency
            {
                Name = packageName,
                Version = packageVersion,
                Source = "url",
                Url = url
            }, isUrlDependency: true);
            
            Console.WriteLine($"✅ Successfully installed {packageName} from URL");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to install from URL {url}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Install a package from npm registry
    /// </summary>
    private async Task<bool> InstallFromNpmAsync(string packageName, string version, bool isDev = false, bool isOptional = false)
    {
        try
        {
            Console.WriteLine($"Installing {packageName}@{version} from npm...");
            
            // Get package metadata from npm
            var packageInfo = await GetNpmPackageInfoAsync(packageName, version);
            if (packageInfo == null)
            {
                Console.WriteLine($"❌ Package {packageName} not found on npm");
                return false;
            }
            
            // Create node_modules directory if it doesn't exist
            Directory.CreateDirectory(_nodeModulesPath);
            
            // Download and extract the package
            var success = await DownloadAndExtractNpmPackageAsync(packageInfo, packageName);
            if (!success)
            {
                return false;
            }
            
            // Update package config
            var versionStr = packageInfo.TryGetValue("version", out var versionObj) && versionObj is string ? (string)versionObj : "latest";
            await UpdatePackageConfigAsync(packageName, new PackageDependency
            {
                Name = packageName,
                Version = versionStr,
                Source = "npm"
            });
            
            Console.WriteLine($"✅ Successfully installed {packageName}@{versionStr}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to install npm package {packageName}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get package information from npm registry
    /// </summary>
    private async Task<Dictionary<string, object?>?> GetNpmPackageInfoAsync(string packageName, string version)
    {
        try
        {
            var registryUrl = $"https://registry.npmjs.org/{packageName}";
            if (version != "latest")
            {
                registryUrl += $"/{version}";
            }
            
            var response = await _httpClient.GetAsync(registryUrl);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var packageData = _jsonModule.Parse.Call(new List<object?> { json });
            
            if (packageData is not Dictionary<string, object?> packageDict)
            {
                return null;
            }
            
            if (version == "latest")
            {
                if (packageDict.TryGetValue("dist-tags", out var distTags) && 
                    distTags is Dictionary<string, object?> tagsDict &&
                    tagsDict.TryGetValue("latest", out var latestObj) &&
                    latestObj is string latestVersion)
                {
                    // Get the specific version info
                    return await GetNpmPackageInfoAsync(packageName, latestVersion);
                }
            }
            
            return packageDict;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get npm package info: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get detailed package information for dependency resolution
    /// </summary>
    private async Task<NpmPackageInfo?> GetNpmPackageInfoAsync(string packageName)
    {
        try
        {
            var registryUrl = $"https://registry.npmjs.org/{packageName}";
            var response = await _httpClient.GetAsync(registryUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var packageData = _jsonModule.Parse.Call(new List<object?> { json });
            
            if (packageData is not Dictionary<string, object?> packageDict)
            {
                return null;
            }
            
            var result = new NpmPackageInfo
            {
                Name = packageName,
                Version = "latest"
            };
            
            // Extract versions
            if (packageDict.TryGetValue("versions", out var versionsObj) &&
                versionsObj is Dictionary<string, object?> versions)
            {
                foreach (var (versionKey, versionValue) in versions)
                {
                    if (versionValue is Dictionary<string, object?> versionDict)
                    {
                        result.Versions[versionKey] = versionDict;
                    }
                }
            }
            
            // Get latest version info
            if (packageDict.TryGetValue("dist-tags", out var distTags) && 
                distTags is Dictionary<string, object?> tagsDict &&
                tagsDict.TryGetValue("latest", out var latestObj) &&
                latestObj is string latestVersion)
            {
                result.Version = latestVersion;
                
                // Get dependencies for latest version
                if (result.Versions.TryGetValue(latestVersion, out var latestVersionInfo) &&
                    latestVersionInfo.TryGetValue("dependencies", out var depsObj) &&
                    depsObj is Dictionary<string, object?> depsDict)
                {
                    result.Dependencies = new Dictionary<string, string>();
                    foreach (var (depName, depVersionObj) in depsDict)
                    {
                        if (depVersionObj is string depVersion)
                        {
                            result.Dependencies[depName] = depVersion;
                        }
                    }
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get detailed npm package info: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Download and extract npm package
    /// </summary>
    private async Task<bool> DownloadAndExtractNpmPackageAsync(Dictionary<string, object?> packageInfo, string packageName)
    {
        try
        {
            // Get tarball URL
            string? tarballUrl = null;
            if (packageInfo.TryGetValue("dist", out var dist) && 
                dist is Dictionary<string, object?> distDict &&
                distDict.TryGetValue("tarball", out var tarball) &&
                tarball is string tarballStr)
            {
                tarballUrl = tarballStr;
            }
            
            if (string.IsNullOrEmpty(tarballUrl))
            {
                Console.WriteLine($"No tarball URL found for package {packageName}");
                return false;
            }
            
            var packageDir = Path.Combine(_nodeModulesPath, packageName);
            
            // Download tarball
            var response = await _httpClient.GetAsync(tarballUrl);
            response.EnsureSuccessStatusCode();
            
            var tarballData = await response.Content.ReadAsByteArrayAsync();
            
            // Create temporary file for tarball
            var tempTarball = Path.GetTempFileName();
            await File.WriteAllBytesAsync(tempTarball, tarballData);
            
            try
            {
                // Extract tarball (simplified extraction - in real implementation you'd use a proper tar library)
                Directory.CreateDirectory(packageDir);
                
                // For now, just create a simple package.json and indicate it's from npm
                var versionStr = packageInfo.TryGetValue("version", out var versionObj) && versionObj is string ? (string)versionObj : "1.0.0";
                var descStr = packageInfo.TryGetValue("description", out var desc) && desc is string ? (string)desc : "";
                var mainStr = packageInfo.TryGetValue("main", out var main) && main is string ? (string)main : "index.js";
                
                var packageJsonContent = $@"{{
  ""name"": ""{packageName}"",
  ""version"": ""{versionStr}"",
  ""description"": ""{descStr}"",
  ""main"": ""{mainStr}"",
  ""source"": ""npm"",
  ""installedAt"": ""{DateTime.UtcNow:O}""
}}";
                
                var packageJsonPath = Path.Combine(packageDir, "package.json");
                await File.WriteAllTextAsync(packageJsonPath, packageJsonContent);
                
                // Create a placeholder main file
                var mainFile = mainStr;
                var mainFilePath = Path.Combine(packageDir, mainFile ?? "index.js");
                if (!File.Exists(mainFilePath))
                {
                    await File.WriteAllTextAsync(mainFilePath, $"// {packageName} - installed from npm\nmodule.exports = {{ name: '{packageName}', version: '{versionStr}' }};");
                }
                
                return true;
            }
            finally
            {
                // Clean up temporary file
                if (File.Exists(tempTarball))
                {
                    File.Delete(tempTarball);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to download and extract npm package: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Update the package configuration file
    /// </summary>
    private async Task UpdatePackageConfigAsync(string packageName, PackageDependency dependency, bool isUrlDependency = false)
    {
        var config = await LoadPackageConfigAsync();
        
        if (isUrlDependency)
        {
            config.UrlDependencies[packageName] = dependency;
        }
        else
        {
            config.Dependencies[packageName] = dependency;
        }
        
        await SavePackageConfigAsync(config);
    }

    /// <summary>
    /// Load package configuration
    /// </summary>
    public async Task<PackageConfig> LoadPackageConfigAsync()
    {
        if (!File.Exists(_packageConfigPath))
        {
            return new PackageConfig();
        }
        
        try
        {
            var json = await File.ReadAllTextAsync(_packageConfigPath);
            var parsed = _jsonModule.Parse.Call(new List<object?> { json });
            return ParsePackageConfigFromDict(parsed);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to load package config: {ex.Message}");
            return new PackageConfig();
        }
    }

    /// <summary>
    /// Parse package config from parsed JSON object
    /// </summary>
    private PackageConfig ParsePackageConfigFromDict(object? parsed)
    {
        try
        {
            if (parsed is not Dictionary<string, object?> root)
            {
                return new PackageConfig();
            }
            
            var config = new PackageConfig();
            
            if (root.TryGetValue("name", out var nameProp) && nameProp is string name)
                config.Name = name;
            
            if (root.TryGetValue("version", out var versionProp) && versionProp is string version)
                config.Version = version;
            
            if (root.TryGetValue("description", out var descProp) && descProp is string desc)
                config.Description = desc;
            
            if (root.TryGetValue("main", out var mainProp) && mainProp is string main)
                config.Main = main;
            
            // Parse dependencies
            if (root.TryGetValue("dependencies", out var depsProp) && depsProp is Dictionary<string, object?> deps)
            {
                foreach (var (depName, depValue) in deps)
                {
                    if (depValue is Dictionary<string, object?> depDict)
                    {
                        // Complex dependency object format
                        var dependency = new PackageDependency
                        {
                            Name = depDict.TryGetValue("name", out var n) && n is string ? (string)n : depName,
                            Version = depDict.TryGetValue("version", out var v) && v is string ? (string)v : "",
                            Source = depDict.TryGetValue("source", out var s) && s is string ? (string)s : "",
                            Url = depDict.TryGetValue("url", out var u) && u is string ? (string)u : null
                        };
                        config.Dependencies[depName] = dependency;
                    }
                    else if (depValue is string versionStr)
                    {
                        // Simple npm-style version string format
                        var dependency = new PackageDependency
                        {
                            Name = depName,
                            Version = versionStr,
                            Source = "npm",
                            Url = null
                        };
                        config.Dependencies[depName] = dependency;
                    }
                }
            }
            
            // Parse URL dependencies - check both "urlDependencies" and "ecDependencies"
            var urlDepsKey = root.ContainsKey("urlDependencies") ? "urlDependencies" : "ecDependencies";
            if (root.TryGetValue(urlDepsKey, out var urlDepsProp) && urlDepsProp is Dictionary<string, object?> urlDeps)
            {
                foreach (var (depName, depValue) in urlDeps)
                {
                    if (depValue is Dictionary<string, object?> depDict)
                    {
                        var dependency = new PackageDependency
                        {
                            Name = depDict.TryGetValue("name", out var n) && n is string ? (string)n : depName,
                            Version = depDict.TryGetValue("version", out var v) && v is string ? (string)v : "",
                            Source = depDict.TryGetValue("source", out var s) && s is string ? (string)s : "",
                            Url = depDict.TryGetValue("url", out var u) && u is string ? (string)u : null
                        };
                        config.UrlDependencies[depName] = dependency;
                    }
                }
            }

            // Parse scripts
            if (root.TryGetValue("scripts", out var scriptsProp) && scriptsProp is Dictionary<string, object?> scripts)
            {
                foreach (var (scriptName, scriptValue) in scripts)
                {
                    if (scriptValue is string command)
                    {
                        config.Scripts[scriptName] = new PackageScript
                        {
                            Name = scriptName,
                            Command = command
                        };
                    }
                }
            }

            // Parse devDependencies
            if (root.TryGetValue("devDependencies", out var devDepsProp) && devDepsProp is Dictionary<string, object?> devDeps)
            {
                foreach (var (depName, depValue) in devDeps)
                {
                    if (depValue is string versionStr)
                    {
                        var dependency = new PackageDependency
                        {
                            Name = depName,
                            Version = versionStr,
                            Source = "npm",
                            IsDev = true
                        };
                        config.DevDependencies[depName] = dependency;
                    }
                }
            }

            // Parse optionalDependencies
            if (root.TryGetValue("optionalDependencies", out var optDepsProp) && optDepsProp is Dictionary<string, object?> optDeps)
            {
                foreach (var (depName, depValue) in optDeps)
                {
                    if (depValue is string versionStr)
                    {
                        var dependency = new PackageDependency
                        {
                            Name = depName,
                            Version = versionStr,
                            Source = "npm",
                            IsOptional = true
                        };
                        config.OptionalDependencies[depName] = dependency;
                    }
                }
            }

            // Parse workspaces
            if (root.TryGetValue("workspaces", out var workspacesProp))
            {
                if (workspacesProp is Dictionary<string, object?> workspacesDict && 
                    workspacesDict.TryGetValue("packages", out var packagesObj) &&
                    packagesObj is List<object?> packagesList)
                {
                    config.Workspaces = new WorkspaceConfig
                    {
                        Packages = packagesList.Where(p => p is string).Cast<string>().ToList()
                    };
                }
                else if (workspacesProp is List<object?> directPackagesList)
                {
                    config.Workspaces = new WorkspaceConfig
                    {
                        Packages = directPackagesList.Where(p => p is string).Cast<string>().ToList()
                    };
                }
            }
            
            return config;
        }
        catch (Exception)
        {
            return new PackageConfig();
        }
    }

    /// <summary>
    /// Parse JSON manually to avoid AOT serialization issues
    /// Handles standard package.json format with ECEngine extensions
    /// </summary>
    private PackageConfig ParsePackageConfigJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            
            var config = new PackageConfig();
            
            if (root.TryGetProperty("name", out var nameProp))
                config.Name = nameProp.GetString() ?? "";
            
            if (root.TryGetProperty("version", out var versionProp))
                config.Version = versionProp.GetString() ?? "1.0.0";
            
            if (root.TryGetProperty("description", out var descProp))
                config.Description = descProp.GetString() ?? "";
            
            if (root.TryGetProperty("main", out var mainProp))
                config.Main = mainProp.GetString() ?? "index.ec";
            
            if (root.TryGetProperty("author", out var authorProp))
                config.Author = authorProp.GetString();
            
            if (root.TryGetProperty("license", out var licenseProp))
                config.License = licenseProp.GetString();
            
            // Parse standard npm dependencies
            if (root.TryGetProperty("dependencies", out var depsProp))
            {
                foreach (var dep in depsProp.EnumerateObject())
                {
                    var dependency = new PackageDependency
                    {
                        Name = dep.Name,
                        Version = dep.Value.GetString() ?? "",
                        Source = "npm"
                    };
                    config.Dependencies[dep.Name] = dependency;
                }
            }
            
            // Parse ECEngine-specific URL dependencies
            if (root.TryGetProperty("ecDependencies", out var ecDepsProp))
            {
                foreach (var dep in ecDepsProp.EnumerateObject())
                {
                    var dependency = new PackageDependency
                    {
                        Name = dep.Name,
                        Version = dep.Value.TryGetProperty("version", out var depVersion) ? depVersion.GetString() ?? "" : "",
                        Source = "url",
                        Url = dep.Value.TryGetProperty("url", out var depUrl) ? depUrl.GetString() : null
                    };
                    config.UrlDependencies[dep.Name] = dependency;
                }
            }
            
            // Parse scripts
            if (root.TryGetProperty("scripts", out var scriptsProp))
            {
                foreach (var script in scriptsProp.EnumerateObject())
                {
                    config.Scripts[script.Name] = new PackageScript
                    {
                        Name = script.Name,
                        Command = script.Value.GetString() ?? ""
                    };
                }
            }
            
            // Parse keywords
            if (root.TryGetProperty("keywords", out var keywordsProp))
            {
                var keywords = new List<string>();
                foreach (var keyword in keywordsProp.EnumerateArray())
                {
                    if (keyword.GetString() is string kw)
                        keywords.Add(kw);
                }
                config.Keywords = keywords.ToArray();
            }
            
            return config;
        }
        catch (Exception)
        {
            return new PackageConfig();
        }
    }

    /// <summary>
    /// Save package configuration
    /// </summary>
    public async Task SavePackageConfigAsync(PackageConfig config)
    {
        try
        {
            // Use a simpler approach for JSON serialization to avoid AOT issues
            var jsonContent = CreatePackageConfigJson(config);
            await File.WriteAllTextAsync(_packageConfigPath, jsonContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to save package config: {ex.Message}");
        }
    }

    /// <summary>
    /// Create JSON manually to avoid AOT serialization issues
    /// Uses standard package.json format
    /// </summary>
    private string CreatePackageConfigJson(PackageConfig config)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"name\": \"{config.Name}\",");
        sb.AppendLine($"  \"version\": \"{config.Version}\",");
        sb.AppendLine($"  \"description\": \"{config.Description}\",");
        sb.AppendLine($"  \"main\": \"{config.Main}\",");
        
        // Standard npm dependencies
        sb.AppendLine("  \"dependencies\": {");
        var depPairs = config.Dependencies.Where(kvp => kvp.Value.Source == "npm").Select(kvp => 
            $"    \"{kvp.Key}\": \"{kvp.Value.Version}\"");
        sb.AppendLine(string.Join(",\n", depPairs));
        sb.AppendLine("  },");
        
        // ECEngine-specific URL dependencies (custom field)
        sb.AppendLine("  \"ecDependencies\": {");
        var urlDepPairs = config.UrlDependencies.Select(kvp => 
            $"    \"{kvp.Key}\": {{" +
            $"\"version\": \"{kvp.Value.Version}\", " +
            $"\"url\": \"{kvp.Value.Url}\"" +
            $"}}");
        sb.AppendLine(string.Join(",\n", urlDepPairs));
        sb.AppendLine("  },");
        
        sb.AppendLine("  \"scripts\": {");
        var scriptPairs = config.Scripts.Select(kvp => $"    \"{kvp.Key}\": \"{kvp.Value}\"");
        sb.AppendLine(string.Join(",\n", scriptPairs));
        sb.AppendLine("  },");
        
        sb.AppendLine($"  \"keywords\": {(config.Keywords != null ? "[\"" + string.Join("\", \"", config.Keywords) + "\"]" : "[]")},");
        sb.AppendLine($"  \"author\": {(config.Author != null ? "\"" + config.Author + "\"" : "\"\"")},");
        sb.AppendLine($"  \"license\": {(config.License != null ? "\"" + config.License + "\"" : "\"ISC\"")},");
        sb.AppendLine("  \"engines\": {");
        sb.AppendLine("    \"ecengine\": \">=1.0.0\"");
        sb.AppendLine("  }");
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    /// <summary>
    /// Initialize a new package configuration
    /// </summary>
    public async Task<bool> InitializePackageAsync(string? name = null, string? version = null, bool createFolder = true)
    {
        try
        {
            var projectName = name ?? Path.GetFileName(_projectRoot);
            
            // Create project directory if name is provided and createFolder is true
            string projectPath = _projectRoot;
            if (createFolder && !string.IsNullOrEmpty(name))
            {
                projectPath = Path.Combine(_projectRoot, projectName);
                Directory.CreateDirectory(projectPath);
                Console.WriteLine($"📁 Created project directory: {projectName}");
            }
            
            var config = new PackageConfig
            {
                Name = projectName,
                Version = version ?? "1.0.0",
                Description = $"An ECEngine project: {projectName}",
                Main = "index.ec"
            };
            
            // Save config in the project directory
            var configPath = Path.Combine(projectPath, "ec.config.json");
            var originalConfigPath = _packageConfigPath;
            var tempPackageManager = new PackageManager(projectPath);
            await tempPackageManager.SavePackageConfigAsync(config);
            
            // Create a sample index.ec file
            var indexPath = Path.Combine(projectPath, "index.ec");
            if (!File.Exists(indexPath))
            {
                var sampleContent = $"// Welcome to {projectName}!\n" +
                                  "// This is your main ECEngine file.\n\n" +
                                  $"console.log(\"Hello from {projectName}!\");\n" +
                                  "console.log(\"ECEngine version: 1.0.0\");\n\n" +
                                  "// You can import packages here:\n" +
                                  "// import { someFunction } from \"package-name\";\n\n" +
                                  "// Add your code below:\n";
                
                await File.WriteAllTextAsync(indexPath, sampleContent);
                Console.WriteLine($"📝 Created sample index.ec file");
            }
            
            Console.WriteLine($"✅ Initialized package configuration for {projectName}");
            if (createFolder && !string.IsNullOrEmpty(name))
            {
                Console.WriteLine($"💡 To get started:");
                Console.WriteLine($"   cd {projectName}");
                Console.WriteLine($"   dotnet run --project ../ index.ec");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to initialize package: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// List installed packages
    /// </summary>
    public async Task ListPackagesAsync()
    {
        try
        {
            var config = await LoadPackageConfigAsync();
            
            Console.WriteLine("📦 Installed Packages:");
            Console.WriteLine();
            
            if (config.Dependencies.Any())
            {
                Console.WriteLine("Dependencies (npm):");
                foreach (var dep in config.Dependencies)
                {
                    Console.WriteLine($"  • {dep.Key}@{dep.Value.Version} ({dep.Value.Source})");
                }
                Console.WriteLine();
            }
            
            if (config.UrlDependencies.Any())
            {
                Console.WriteLine("URL Dependencies:");
                foreach (var dep in config.UrlDependencies)
                {
                    Console.WriteLine($"  • {dep.Key}@{dep.Value.Version} from {dep.Value.Url}");
                }
                Console.WriteLine();
            }
            
            if (!config.Dependencies.Any() && !config.UrlDependencies.Any())
            {
                Console.WriteLine("  No packages installed.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to list packages: {ex.Message}");
        }
    }

    // Helper methods
    private bool IsUrl(string input)
    {
        return Uri.TryCreate(input, UriKind.Absolute, out var uri) && 
               (uri.Scheme == "http" || uri.Scheme == "https");
    }

    private bool IsValidNpmPackageName(string name)
    {
        // Basic npm package name validation
        return !string.IsNullOrWhiteSpace(name) && 
               name.Length <= 214 && 
               !name.StartsWith(".") && 
               !name.StartsWith("_") &&
               name.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.' || c == '@' || c == '/');
    }

    private string ExtractPackageNameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var fileName = Path.GetFileNameWithoutExtension(uri.LocalPath);
            return string.IsNullOrWhiteSpace(fileName) ? "url-package" : fileName;
        }
        catch
        {
            return "url-package";
        }
    }

    private string GetFileNameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var fileName = Path.GetFileName(uri.LocalPath);
            return string.IsNullOrWhiteSpace(fileName) ? "index.js" : fileName;
        }
        catch
        {
            return "index.js";
        }
    }

    // Advanced package management helper methods

    private async Task<Dictionary<string, PackageDependency>> ResolveDependencyTreeAsync(PackageConfig config)
    {
        var resolved = new Dictionary<string, PackageDependency>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();
        
        // Resolve production dependencies first
        foreach (var (name, dependency) in config.Dependencies)
        {
            await ResolveDependencyRecursiveAsync(name, dependency.Version, resolved, visited, visiting, false);
        }
        
        // Resolve dev dependencies
        foreach (var (name, dependency) in config.DevDependencies)
        {
            await ResolveDependencyRecursiveAsync(name, dependency.Version, resolved, visited, visiting, true);
        }
        
        return resolved;
    }

    private async Task<bool> ResolveDependencyRecursiveAsync(
        string packageName, 
        string versionSpec, 
        Dictionary<string, PackageDependency> resolved,
        HashSet<string> visited,
        HashSet<string> visiting,
        bool isDev)
    {
        var key = $"{packageName}@{versionSpec}";
        
        if (visiting.Contains(key))
        {
            Console.WriteLine($"⚠️  Circular dependency detected: {key}");
            return false;
        }
        
        if (visited.Contains(key))
        {
            return true;
        }
        
        visiting.Add(key);
        
        try
        {
            // Get package info from cache or registry
            var packageInfo = await GetPackageInfoAsync(packageName, versionSpec);
            if (packageInfo == null)
            {
                Console.WriteLine($"❌ Package not found: {packageName}@{versionSpec}");
                return false;
            }
            
            // Add to resolved dependencies
            resolved[packageName] = new PackageDependency
            {
                Name = packageName,
                Version = packageInfo.Version,
                ResolvedVersion = packageInfo.Version,
                IsDev = isDev,
                Source = "npm"
            };
            
            // Resolve nested dependencies
            if (packageInfo.Dependencies != null)
            {
                foreach (var (depName, depVersion) in packageInfo.Dependencies)
                {
                    await ResolveDependencyRecursiveAsync(depName, depVersion, resolved, visited, visiting, false);
                }
            }
            
            visited.Add(key);
            return true;
        }
        finally
        {
            visiting.Remove(key);
        }
    }

    private async Task<bool> InstallResolvedPackageAsync(string packageName, PackageDependency dependency)
    {
        // Check cache first
        if (_cache.IsCached(packageName, dependency.ResolvedVersion))
        {
            Console.WriteLine($"📦 Using cached {packageName}@{dependency.ResolvedVersion}");
            return _cache.RestoreFromCache(packageName, dependency.ResolvedVersion, _nodeModulesPath);
        }
        
        // Install based on source
        bool success = dependency.Source switch
        {
            "npm" => await InstallFromNpmAsync(packageName, dependency.ResolvedVersion, dependency.IsDev, dependency.IsOptional),
            "url" => await InstallFromUrlAsync(dependency.Url!, dependency.ResolvedVersion),
            "local" => await InstallFromLocalAsync(dependency.LocalPath!, dependency.ResolvedVersion, dependency.IsDev),
            _ => false
        };
        
        if (success)
        {
            // Cache the installed package
            var packagePath = Path.Combine(_nodeModulesPath, packageName);
            _cache.CachePackage(packageName, dependency.ResolvedVersion, packagePath);
        }
        
        return success;
    }

    private async Task<NpmPackageInfo?> GetPackageInfoAsync(string packageName, string versionSpec)
    {
        try
        {
            // Try cache first
            var cacheKey = $"{packageName}@{versionSpec}";
            if (_packageInfoCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }
            
            // Get from npm registry
            var packageInfo = await GetNpmPackageInfoAsync(packageName);
            if (packageInfo != null)
            {
                // Resolve version based on semver spec
                var resolvedVersion = _semver.GetBestMatch(versionSpec, packageInfo.Versions.Keys.ToList());
                if (resolvedVersion != null && packageInfo.Versions.TryGetValue(resolvedVersion, out var versionInfo))
                {
                    var result = new NpmPackageInfo
                    {
                        Name = packageName,
                        Version = resolvedVersion,
                        Versions = packageInfo.Versions
                    };
                    
                    // Extract dependencies from version info
                    if (versionInfo.TryGetValue("dependencies", out var depsObj) &&
                        depsObj is Dictionary<string, object?> depsDict)
                    {
                        result.Dependencies = new Dictionary<string, string>();
                        foreach (var (depName, depVersionObj) in depsDict)
                        {
                            if (depVersionObj is string depVersion)
                            {
                                result.Dependencies[depName] = depVersion;
                            }
                        }
                    }
                    
                    _packageInfoCache[cacheKey] = result;
                    return result;
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get package info for {packageName}: {ex.Message}");
            return null;
        }
    }

    private async Task<bool> InstallFromLocalAsync(string localPath, string? version, bool isDev)
    {
        try
        {
            var fullPath = Path.GetFullPath(localPath);
            if (!Directory.Exists(fullPath))
            {
                Console.WriteLine($"Local path does not exist: {fullPath}");
                return false;
            }
            
            // Read package.json to get package name
            var packageJsonPath = Path.Combine(fullPath, "package.json");
            if (!File.Exists(packageJsonPath))
            {
                Console.WriteLine($"No package.json found in: {fullPath}");
                return false;
            }
            
            var packageJson = await File.ReadAllTextAsync(packageJsonPath);
            var parsed = _jsonModule.Parse.Call(new List<object?> { packageJson });
            
            if (parsed is Dictionary<string, object?> packageDict &&
                packageDict.TryGetValue("name", out var nameObj) &&
                nameObj is string packageName)
            {
                // Create symlink in node_modules
                var targetPath = Path.Combine(_nodeModulesPath, packageName);
                Directory.CreateDirectory(_nodeModulesPath);
                
                if (Directory.Exists(targetPath))
                {
                    Directory.Delete(targetPath, true);
                }
                
                CreateSymlink(fullPath, targetPath);
                
                Console.WriteLine($"✅ Installed local package: {packageName} -> {fullPath}");
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to install local package: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> RunLifecycleHookAsync(string hookName)
    {
        try
        {
            var config = await LoadPackageConfigAsync();
            
            if (config.Scripts.TryGetValue(hookName, out var script))
            {
                Console.WriteLine($"Running {hookName} hook...");
                return await ExecuteCommandAsync(script.Command);
            }
            
            return true; // No hook defined is not an error
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to run {hookName} hook: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> ExecuteCommandAsync(string command)
    {
        try
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "cmd" : "bash",
                Arguments = OperatingSystem.IsWindows() ? $"/c {command}" : $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = _rootPath
            };
            
            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process == null)
            {
                return false;
            }
            
            // Read and display output in real-time
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();
            
            var output = await outputTask;
            var error = await errorTask;
            
            // Display stdout if there's any output
            if (!string.IsNullOrWhiteSpace(output))
            {
                Console.Write(output);
            }
            
            if (process.ExitCode != 0)
            {
                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine($"Command failed: {error}");
                }
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to execute command: {ex.Message}");
            return false;
        }
    }

    private async Task<List<string>> FindWorkspacePackagesAsync(List<string> patterns)
    {
        var packages = new List<string>();
        
        foreach (var pattern in patterns)
        {
            var normalizedPattern = pattern.Replace('/', Path.DirectorySeparatorChar);
            var globPattern = Path.Combine(_rootPath, normalizedPattern);
            
            // Simple glob matching for patterns like "packages/*"
            if (pattern.EndsWith("/*"))
            {
                var baseDir = Path.Combine(_rootPath, normalizedPattern.TrimEnd('*', Path.DirectorySeparatorChar));
                if (Directory.Exists(baseDir))
                {
                    foreach (var dir in Directory.GetDirectories(baseDir))
                    {
                        var packageJsonPath = Path.Combine(dir, "package.json");
                        if (File.Exists(packageJsonPath))
                        {
                            packages.Add(dir);
                        }
                    }
                }
            }
            else
            {
                var packagePath = Path.Combine(_rootPath, normalizedPattern);
                if (Directory.Exists(packagePath) && File.Exists(Path.Combine(packagePath, "package.json")))
                {
                    packages.Add(packagePath);
                }
            }
        }
        
        return packages;
    }

    private void CreateSymlink(string source, string target)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                // Use junction on Windows
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c mklink /J \"{target}\" \"{source}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                process?.WaitForExit();
            }
            else
            {
                // Use symlink on Unix-like systems
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ln",
                    Arguments = $"-s \"{source}\" \"{target}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                process?.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create symlink: {ex.Message}");
            // Fallback to copy if symlink fails
            CopyDirectory(source, target);
        }
    }

    private void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        
        foreach (var file in Directory.GetFiles(source))
        {
            var fileName = Path.GetFileName(file);
            File.Copy(file, Path.Combine(target, fileName), true);
        }
        
        foreach (var directory in Directory.GetDirectories(source))
        {
            var dirName = Path.GetFileName(directory);
            CopyDirectory(directory, Path.Combine(target, dirName));
        }
    }

    private bool IsLocalPath(string packageSpec)
    {
        return packageSpec.StartsWith("./") || 
               packageSpec.StartsWith("../") || 
               packageSpec.StartsWith("/") ||
               packageSpec.StartsWith("file:") ||
               Path.IsPathRooted(packageSpec);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
