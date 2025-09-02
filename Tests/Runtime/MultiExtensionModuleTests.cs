using Xunit;
using ECEngine.Runtime;
using ECEngine.Parser;
using ECEngine.Lexer;
using System.IO;
using System.Linq;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Runtime
{
    public class MultiExtensionModuleTests
    {
        private RuntimeInterpreter CreateInterpreterWithModuleSupport()
        {
            var interpreter = new RuntimeInterpreter();
            // Find the project root by looking for ECEngine.csproj
            var currentDirectory = Directory.GetCurrentDirectory();
            var projectRoot = FindProjectRoot(currentDirectory);
            var moduleBasePath = Path.Combine(projectRoot, "examples", "modules");
            var moduleSystem = new ModuleSystem(moduleBasePath);
            interpreter.SetModuleSystem(moduleSystem);
            return interpreter;
        }

        private string FindProjectRoot(string startPath)
        {
            var directory = new DirectoryInfo(startPath);
            while (directory != null)
            {
                if (directory.GetFiles("ECEngine.csproj").Any())
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Could not find project root containing ECEngine.csproj");
        }

        [Fact]
        public void LoadJSFile_ShouldLoadSuccessfully()
        {
            // Arrange
            var interpreter = CreateInterpreterWithModuleSupport();
            var code = @"
                import { jsValue } from './test.js';
                export const result = jsValue;
            ";

            // Act
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            interpreter.Evaluate(ast, code);

            // Assert
            var exports = interpreter.GetExports();
            Assert.True(exports.ContainsKey("result"));
            Assert.Equal("Hello from JavaScript!", exports["result"]);
        }

        [Fact]
        public void LoadMJSFile_ShouldLoadSuccessfully()
        {
            // Arrange
            var interpreter = CreateInterpreterWithModuleSupport();
            var code = @"
                import { mjsValue } from './test.mjs';
                export const result = mjsValue;
            ";

            // Act
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            interpreter.Evaluate(ast, code);

            // Assert
            var exports = interpreter.GetExports();
            Assert.True(exports.ContainsKey("result"));
            Assert.Equal("Hello from MJS!", exports["result"]);
        }

        [Fact]
        public void LoadWithoutExtension_ShouldTryMultipleExtensions()
        {
            // Arrange
            var interpreter = CreateInterpreterWithModuleSupport();
            var code = @"
                import { jsValue } from './test';
                export const result = jsValue;
            ";

            // Act
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            interpreter.Evaluate(ast, code);

            // Assert
            var exports = interpreter.GetExports();
            Assert.True(exports.ContainsKey("result"));
            // Should find test.ec first (if it exists), then test.js, then test.mjs
            Assert.Equal("Hello from ECEngine!", exports["result"]);
        }

        [Fact]
        public void LoadMixedExtensions_ShouldWorkCorrectly()
        {
            // Arrange
            var interpreter = CreateInterpreterWithModuleSupport();
            var code = @"
                import { jsValue } from './test.js';
                import { mjsValue } from './test.mjs';
                export const jsResult = jsValue;
                export const mjsResult = mjsValue;
            ";

            // Act
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            interpreter.Evaluate(ast, code);

            // Assert
            var exports = interpreter.GetExports();
            Assert.True(exports.ContainsKey("jsResult"));
            Assert.True(exports.ContainsKey("mjsResult"));
            Assert.Equal("Hello from JavaScript!", exports["jsResult"]);
            Assert.Equal("Hello from MJS!", exports["mjsResult"]);
        }
    }
}
