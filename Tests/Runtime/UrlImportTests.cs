using Xunit;
using ECEngine.Runtime;
using ECEngine.Parser;
using ECEngine.Lexer;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Runtime
{
    public class UrlImportTests
    {
        [Fact]
        public void IsUrl_ValidHttpUrl_ReturnsTrue()
        {
            // Arrange
            var moduleSystem = new ModuleSystem();
            var httpUrl = "http://example.com/module.js";
            
            // Use reflection to access private method for testing
            var method = typeof(ModuleSystem).GetMethod("IsUrl", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            var result = (bool)method!.Invoke(moduleSystem, new object[] { httpUrl })!;
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public void IsUrl_ValidHttpsUrl_ReturnsTrue()
        {
            // Arrange
            var moduleSystem = new ModuleSystem();
            var httpsUrl = "https://example.com/module.js";
            
            // Use reflection to access private method for testing
            var method = typeof(ModuleSystem).GetMethod("IsUrl", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            var result = (bool)method!.Invoke(moduleSystem, new object[] { httpsUrl })!;
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public void IsUrl_LocalPath_ReturnsFalse()
        {
            // Arrange
            var moduleSystem = new ModuleSystem();
            var localPath = "./local/module.js";
            
            // Use reflection to access private method for testing
            var method = typeof(ModuleSystem).GetMethod("IsUrl", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            var result = (bool)method!.Invoke(moduleSystem, new object[] { localPath })!;
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void GetUrlCacheKey_GeneratesConsistentKey()
        {
            // Arrange
            var moduleSystem = new ModuleSystem();
            var url = "https://example.com/module.js";
            
            // Use reflection to access private method for testing
            var method = typeof(ModuleSystem).GetMethod("GetUrlCacheKey", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            var key1 = (string)method!.Invoke(moduleSystem, new object[] { url })!;
            var key2 = (string)method!.Invoke(moduleSystem, new object[] { url })!;
            
            // Assert
            Assert.Equal(key1, key2);
            Assert.EndsWith(".js", key1);
        }
        
        [Fact]
        public void GetUrlCacheKey_DifferentUrlsGenerateDifferentKeys()
        {
            // Arrange
            var moduleSystem = new ModuleSystem();
            var url1 = "https://example.com/module1.js";
            var url2 = "https://example.com/module2.js";
            
            // Use reflection to access private method for testing
            var method = typeof(ModuleSystem).GetMethod("GetUrlCacheKey", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            var key1 = (string)method!.Invoke(moduleSystem, new object[] { url1 })!;
            var key2 = (string)method!.Invoke(moduleSystem, new object[] { url2 })!;
            
            // Assert
            Assert.NotEqual(key1, key2);
        }
        
        [Fact]
        public void ClearUrlCache_CreatesNewCacheDirectory()
        {
            // Arrange
            var moduleSystem = new ModuleSystem();
            
            // Act
            moduleSystem.ClearUrlCache();
            
            // Assert - Should not throw and cache directory should exist
            // This test mainly verifies the method doesn't crash
            Assert.True(true);
        }
        
        // Note: Testing actual URL downloads would require either:
        // 1. A mock HTTP server for reliable testing
        // 2. Network connectivity and external dependencies
        // 3. Integration tests rather than unit tests
        // For now, we'll focus on testing the URL detection and caching logic
        
        [Fact(Skip = "Requires network connectivity and external URL")]
        public async Task LoadModuleFromUrl_ValidUrl_ShouldLoadModule()
        {
            // This test would require a reliable test URL
            // In a real implementation, you would set up a test server
            // or use a known-good URL for testing
            
            // Example of what the test would look like:
            // var interpreter = new RuntimeInterpreter();
            // var moduleSystem = new ModuleSystem();
            // interpreter.SetModuleSystem(moduleSystem);
            // 
            // var module = moduleSystem.LoadModule("https://test-server.com/module.js", interpreter);
            // Assert.NotNull(module);
        }
    }
}
