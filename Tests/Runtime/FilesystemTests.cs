using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;
using ECEngine.Runtime;
using ECEngine.Parser;
using ECEngine.Lexer;
using ECEngine.AST;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Runtime
{
    public class FilesystemTests
    {
        private RuntimeInterpreter _interpreter;
        private string _testDirectory;

        public FilesystemTests()
        {
            _interpreter = new RuntimeInterpreter();
            _testDirectory = Path.Combine(Path.GetTempPath(), $"ec_test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDirectory);
            
            // Set up module system to enable require() functionality
            var moduleSystem = new ModuleSystem(_testDirectory);
            _interpreter.SetModuleSystem(moduleSystem);
        }

        ~FilesystemTests()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Fact]
        public void TestFilesystemModuleImport()
        {
            var code = "const fs = require('fs');";
            var lexer = new ECEngine.Lexer.Lexer(code);
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            
            var exception = Record.Exception(() => _interpreter.Evaluate(ast, code));
            Assert.Null(exception);
        }

        [Fact]
        public void TestWriteFileSync()
        {
            var testFile = Path.Combine(_testDirectory, "test.txt");
            var code = $@"
                const fs = require('fs');
                fs.writeFileSync('{testFile.Replace("\\", "\\\\")}', 'Hello World!');
            ";
            
            ExecuteCode(code);
            
            Assert.True(File.Exists(testFile));
            Assert.Equal("Hello World!", File.ReadAllText(testFile));
        }

        [Fact]
        public void TestReadFileSync()
        {
            var testFile = Path.Combine(_testDirectory, "test.txt");
            File.WriteAllText(testFile, "Test Content");
            
            var code = $@"
                const fs = require('fs');
                const content = fs.readFileSync('{testFile.Replace("\\", "\\\\")}');
            ";
            
            var result = ExecuteCode(code);
            var content = _interpreter.GetVariable("content");
            
            Assert.Equal("Test Content", content);
        }

        [Fact]
        public void TestAppendFileSync()
        {
            var testFile = Path.Combine(_testDirectory, "test.txt");
            File.WriteAllText(testFile, "Initial Content");
            
            var code = $@"
                const fs = require('fs');
                fs.appendFileSync('{testFile.Replace("\\", "\\\\")}', '\nAppended Content');
            ";
            
            ExecuteCode(code);
            
            var content = File.ReadAllText(testFile);
            Assert.Equal("Initial Content\nAppended Content", content);
        }

        [Fact]
        public void TestExistsSync()
        {
            var existingFile = Path.Combine(_testDirectory, "existing.txt");
            var nonExistingFile = Path.Combine(_testDirectory, "nonexisting.txt");
            File.WriteAllText(existingFile, "content");
            
            var code = $@"
                const fs = require('fs');
                const exists = fs.existsSync('{existingFile.Replace("\\", "\\\\")}');
                const notExists = fs.existsSync('{nonExistingFile.Replace("\\", "\\\\")}');
            ";
            
            ExecuteCode(code);
            
            var exists = _interpreter.GetVariable("exists");
            var notExists = _interpreter.GetVariable("notExists");
            
            Assert.True((bool)exists);
            Assert.False((bool)notExists);
        }

        [Fact]
        public void TestMkdirSync()
        {
            var testDir = Path.Combine(_testDirectory, "newdir");
            
            var code = $@"
                const fs = require('fs');
                fs.mkdirSync('{testDir.Replace("\\", "\\\\")}');
            ";
            
            ExecuteCode(code);
            
            Assert.True(Directory.Exists(testDir));
        }

        [Fact]
        public void TestRmdirSync()
        {
            var testDir = Path.Combine(_testDirectory, "testdir");
            Directory.CreateDirectory(testDir);
            
            var code = $@"
                const fs = require('fs');
                fs.rmdirSync('{testDir.Replace("\\", "\\\\")}');
            ";
            
            ExecuteCode(code);
            
            Assert.False(Directory.Exists(testDir));
        }

        [Fact]
        public void TestReaddirSync()
        {
            var file1 = Path.Combine(_testDirectory, "file1.txt");
            var file2 = Path.Combine(_testDirectory, "file2.txt");
            File.WriteAllText(file1, "content1");
            File.WriteAllText(file2, "content2");
            
            var code = $@"
                const fs = require('fs');
                const files = fs.readdirSync('{_testDirectory.Replace("\\", "\\\\")}');
            ";
            
            ExecuteCode(code);
            
            var files = _interpreter.GetVariable("files") as string[];
            Assert.NotNull(files);
            Assert.True(files.Length >= 2);
            Assert.Contains("file1.txt", files);
            Assert.Contains("file2.txt", files);
        }

        [Fact]
        public void TestStatSync()
        {
            var testFile = Path.Combine(_testDirectory, "test.txt");
            File.WriteAllText(testFile, "Hello World!");
            
            var code = $@"
                const fs = require('fs');
                const stats = fs.statSync('{testFile.Replace("\\", "\\\\")}');
                const isFile = stats.isFile();
                const isDirectory = stats.isDirectory();
                const size = stats.size;
            ";
            
            ExecuteCode(code);
            
            var isFile = _interpreter.GetVariable("isFile");
            var isDirectory = _interpreter.GetVariable("isDirectory");
            var size = _interpreter.GetVariable("size");
            
            Assert.True((bool)isFile);
            Assert.False((bool)isDirectory);
            Assert.Equal(12L, size); // "Hello World!" is 12 characters
        }

        [Fact]
        public void TestCopyFileSync()
        {
            var sourceFile = Path.Combine(_testDirectory, "source.txt");
            var destFile = Path.Combine(_testDirectory, "dest.txt");
            File.WriteAllText(sourceFile, "Source Content");
            
            var code = $@"
                const fs = require('fs');
                fs.copyFileSync('{sourceFile.Replace("\\", "\\\\")}', '{destFile.Replace("\\", "\\\\")}');
            ";
            
            ExecuteCode(code);
            
            Assert.True(File.Exists(destFile));
            Assert.Equal("Source Content", File.ReadAllText(destFile));
        }

        [Fact]
        public void TestRenameSync()
        {
            var oldFile = Path.Combine(_testDirectory, "old.txt");
            var newFile = Path.Combine(_testDirectory, "new.txt");
            File.WriteAllText(oldFile, "Content");
            
            var code = $@"
                const fs = require('fs');
                fs.renameSync('{oldFile.Replace("\\", "\\\\")}', '{newFile.Replace("\\", "\\\\")}');
            ";
            
            ExecuteCode(code);
            
            Assert.False(File.Exists(oldFile));
            Assert.True(File.Exists(newFile));
            Assert.Equal("Content", File.ReadAllText(newFile));
        }

        [Fact]
        public void TestUnlinkSync()
        {
            var testFile = Path.Combine(_testDirectory, "test.txt");
            File.WriteAllText(testFile, "Content");
            
            var code = $@"
                const fs = require('fs');
                fs.unlinkSync('{testFile.Replace("\\", "\\\\")}');
            ";
            
            ExecuteCode(code);
            
            Assert.False(File.Exists(testFile));
        }

        [Fact]
        public void TestRealpathSync()
        {
            var testFile = Path.Combine(_testDirectory, "test.txt");
            File.WriteAllText(testFile, "Content");
            
            var code = $@"
                const fs = require('fs');
                const realpath = fs.realpathSync('{testFile.Replace("\\", "\\\\")}');
            ";
            
            ExecuteCode(code);
            
            var realpath = _interpreter.GetVariable("realpath") as string;
            Assert.NotNull(realpath);
            Assert.True(File.Exists(realpath));
        }

        [Fact]
        public void TestFilesystemConstants()
        {
            var code = @"
                const fs = require('fs');
                const F_OK = fs.constants.F_OK;
                const R_OK = fs.constants.R_OK;
                const W_OK = fs.constants.W_OK;
                const X_OK = fs.constants.X_OK;
                const O_RDONLY = fs.constants.O_RDONLY;
                const O_WRONLY = fs.constants.O_WRONLY;
                const O_CREAT = fs.constants.O_CREAT;
                const S_IFREG = fs.constants.S_IFREG;
                const S_IFDIR = fs.constants.S_IFDIR;
            ";
            
            ExecuteCode(code);
            
            Assert.Equal(0.0, _interpreter.GetVariable("F_OK"));
            Assert.Equal(4.0, _interpreter.GetVariable("R_OK"));
            Assert.Equal(2.0, _interpreter.GetVariable("W_OK"));
            Assert.Equal(1.0, _interpreter.GetVariable("X_OK"));
            Assert.Equal(0.0, _interpreter.GetVariable("O_RDONLY"));
            Assert.Equal(1.0, _interpreter.GetVariable("O_WRONLY"));
            Assert.Equal(64.0, _interpreter.GetVariable("O_CREAT"));
            Assert.Equal(32768.0, _interpreter.GetVariable("S_IFREG"));
            Assert.Equal(16384.0, _interpreter.GetVariable("S_IFDIR"));
        }

        [Fact]
        public void TestErrorHandling_NonExistentFile()
        {
            var nonExistentFile = Path.Combine(_testDirectory, "nonexistent.txt");
            
            var code = $@"
                const fs = require('fs');
                let errorCaught = false;
                try {{
                    fs.readFileSync('{nonExistentFile.Replace("\\", "\\\\")}');
                }} catch (error) {{
                    errorCaught = true;
                }}
            ";
            
            ExecuteCode(code);
            
            var errorCaught = _interpreter.GetVariable("errorCaught");
            Assert.True((bool)errorCaught);
        }

        private object? ExecuteCode(string code)
        {
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            return _interpreter.Evaluate(ast, code);
        }
    }
}
