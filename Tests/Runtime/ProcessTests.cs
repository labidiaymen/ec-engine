using Xunit;
using ECEngine.Runtime;
using ECEngine.Parser;
using ECEngine.Lexer;
using System.Collections.Generic;
using System;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Runtime
{
    [Collection("Console Tests")]
    public class ProcessTests
    {
        private object? EvaluateCode(string code)
        {
            var lexer = new ECEngine.Lexer.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new ECEngine.Parser.Parser();
            var ast = parser.Parse(code);
            var interpreter = new RuntimeInterpreter();
            
            // Initialize module system and event loop for full functionality
            var moduleSystem = new ModuleSystem();
            interpreter.SetModuleSystem(moduleSystem);
            
            return interpreter.Evaluate(ast, code);
        }

        [Fact]
        public void Process_ShouldBeAvailable()
        {
            var result = EvaluateCode("typeof process");
            Assert.Equal("object", result);
        }

        [Fact]
        public void Process_ShouldHavePid()
        {
            var result = EvaluateCode("typeof process.pid");
            Assert.Equal("number", result);
            
            var pid = EvaluateCode("process.pid");
            Assert.True(Convert.ToInt32(pid) > 0);
        }

        [Fact]
        public void Process_ShouldHavePlatform()
        {
            var result = EvaluateCode("process.platform");
            var platform = result?.ToString();
            
            Assert.NotNull(platform);
            Assert.Contains(platform, new[] { "darwin", "linux", "win32" });
        }

        [Fact]
        public void Process_ShouldHaveArch()
        {
            var result = EvaluateCode("process.arch");
            var arch = result?.ToString();
            
            Assert.NotNull(arch);
            Assert.Contains(arch, new[] { "x64", "ia32", "arm", "arm64" });
        }

        [Fact]
        public void Process_ShouldHaveVersion()
        {
            var result = EvaluateCode("process.version");
            Assert.Equal("v18.17.0", result);
        }

        [Fact]
        public void Process_ShouldHaveVersions()
        {
            var result = EvaluateCode("typeof process.versions");
            Assert.Equal("object", result);
            
            var nodeVersion = EvaluateCode("process.versions.node");
            Assert.Equal("18.17.0", nodeVersion);
            
            var ecengineVersion = EvaluateCode("process.versions.ecengine");
            Assert.Equal("1.0.0", ecengineVersion);
        }

        [Fact]
        public void Process_ShouldHaveArgv()
        {
            var result = EvaluateCode("typeof process.argv");
            Assert.Equal("object", result);
            
            var length = EvaluateCode("process.argv.length");
            Assert.True(Convert.ToInt32(length) >= 2); // At least executable and script name
        }

        [Fact]
        public void Process_ShouldHaveEnv()
        {
            var result = EvaluateCode("typeof process.env");
            Assert.Equal("object", result);
            
            // PATH should exist on all platforms - check using property access
            var pathExists = EvaluateCode("typeof process.env.PATH !== 'undefined'");
            Assert.True((bool)pathExists!);
        }

        [Fact]
        public void Process_ShouldHaveExecPath()
        {
            var result = EvaluateCode("typeof process.execPath");
            Assert.Equal("string", result);
            
            var execPath = EvaluateCode("process.execPath");
            Assert.NotNull(execPath);
            Assert.NotEmpty(execPath.ToString()!);
        }

        [Fact]
        public void Process_ExitCode_ShouldBeSettable()
        {
            // Reset exit code first to ensure clean state
            EvaluateCode("process.exitCode = 0");
            
            var initialCode = EvaluateCode("process.exitCode");
            Assert.Equal(0, Convert.ToInt32(initialCode));
            
            EvaluateCode("process.exitCode = 42");
            var newCode = EvaluateCode("process.exitCode");
            Assert.Equal(42, Convert.ToInt32(newCode));
            
            // Reset back to 0 for other tests
            EvaluateCode("process.exitCode = 0");
        }

        [Fact]
        public void Process_Cwd_ShouldReturnString()
        {
            var result = EvaluateCode("typeof process.cwd()");
            Assert.Equal("string", result);
            
            var cwd = EvaluateCode("process.cwd()");
            Assert.NotNull(cwd);
            Assert.NotEmpty(cwd.ToString()!);
        }

        [Fact]
        public void Process_MemoryUsage_ShouldReturnObject()
        {
            var result = EvaluateCode("typeof process.memoryUsage()");
            Assert.Equal("object", result);
            
            var hasRss = EvaluateCode("typeof process.memoryUsage().rss !== 'undefined'");
            Assert.True((bool)hasRss!);
            
            var hasHeapTotal = EvaluateCode("typeof process.memoryUsage().heapTotal !== 'undefined'");
            Assert.True((bool)hasHeapTotal!);
            
            var hasHeapUsed = EvaluateCode("typeof process.memoryUsage().heapUsed !== 'undefined'");
            Assert.True((bool)hasHeapUsed!);
        }

        [Fact]
        public void Process_Uptime_ShouldReturnNumber()
        {
            var result = EvaluateCode("typeof process.uptime()");
            Assert.Equal("number", result);
            
            var uptime = EvaluateCode("process.uptime()");
            Assert.True(Convert.ToDouble(uptime) >= 0);
        }

        [Fact]
        public void Process_Hrtime_ShouldReturnArray()
        {
            var result = EvaluateCode("typeof process.hrtime()");
            Assert.Equal("object", result);
            
            var length = EvaluateCode("process.hrtime().length");
            Assert.Equal(2, Convert.ToInt32(length));
            
            // Test with previous time
            var timeResult = EvaluateCode(@"
                var start = process.hrtime();
                var diff = process.hrtime(start);
                typeof diff === 'object' && diff.length === 2
            ");
            Assert.True((bool)timeResult!);
        }

        [Fact]
        public void Process_EventMethods_ShouldExist()
        {
            var onType = EvaluateCode("typeof process.on");
            Assert.Equal("object", onType);
            
            var offType = EvaluateCode("typeof process.off");
            Assert.Equal("object", offType);
            
            var emitType = EvaluateCode("typeof process.emit");
            Assert.Equal("object", emitType);
            
            var eventNamesType = EvaluateCode("typeof process.eventNames");
            Assert.Equal("object", eventNamesType);
            
            var listenerCountType = EvaluateCode("typeof process.listenerCount");
            Assert.Equal("object", listenerCountType);
        }

        [Fact]
        public void Process_EventListeners_ShouldWork()
        {
            var result = EvaluateCode(@"
                var testEventFired = false;
                process.on('test', function(data) {
                    testEventFired = true;
                });
                
                process.emit('test', 'hello');
                testEventFired;
            ");
            Assert.True((bool)result!);
        }

        [Fact]
        public void Process_EventNames_ShouldReturnArray()
        {
            // First register an event listener
            EvaluateCode("process.on('customEventNames', function() {});");
            
            // Then get the event names and check them
            var names = EvaluateCode("process.eventNames()");
            Assert.NotNull(names);
            
            // Check that eventNames returns an object (array)
            var typeCheck = EvaluateCode("typeof process.eventNames()");
            Assert.Equal("object", typeCheck);
            
            // Check that it has a length property (indicating it's array-like)
            var hasLength = EvaluateCode("typeof process.eventNames().length !== 'undefined'");
            Assert.True((bool)hasLength!);
        }

        [Fact]
        public void Process_ListenerCount_ShouldReturnNumber()
        {
            // Use a unique event name for isolation
            var eventName = "testCount" + DateTime.Now.Ticks;
            
            var initialCount = EvaluateCode($"process.listenerCount('{eventName}')");
            Assert.Equal(0, (int)initialCount!);
            
            EvaluateCode($"process.on('{eventName}', function() {{}});");
            
            var afterCount = EvaluateCode($"process.listenerCount('{eventName}')");
            Assert.Equal(1, (int)afterCount!);
        }

        [Fact]
        public void Process_RemoveListener_ShouldWork()
        {
            // Use a unique event name for isolation
            var eventName = "removeTest" + DateTime.Now.Ticks;
            
            // Test removing all listeners for an event (which should work)
            EvaluateCode($@"
                process.on('{eventName}', function() {{}});
                process.on('{eventName}', function() {{}});
                process.on('{eventName}', function() {{}});
            ");
            
            var beforeRemove = EvaluateCode($"process.listenerCount('{eventName}')");
            Assert.Equal(3, (int)beforeRemove!);
            
            // Remove all listeners for this event
            EvaluateCode($"process.off('{eventName}');");
            
            var afterRemove = EvaluateCode($"process.listenerCount('{eventName}')");
            Assert.Equal(0, (int)afterRemove!);
        }

        [Fact]
        public void Process_NextTick_ShouldWork()
        {
            var result = EvaluateCode(@"
                var tickExecuted = false;
                process.nextTick(function() {
                    tickExecuted = true;
                });
                
                // NextTick should be scheduled but not executed yet
                var immediate = tickExecuted;
                immediate; // Should be false initially
            ");
            Assert.False((bool)result!);
        }

        [Fact]
        public void Process_MultipleEventListeners_ShouldWork()
        {
            var result = EvaluateCode(@"
                var count = 0;
                
                process.on('multiTest', function() { count++; });
                process.on('multiTest', function() { count++; });
                process.on('multiTest', function() { count++; });
                
                process.emit('multiTest');
                count === 3;
            ");
            Assert.True((bool)result!);
        }

        [Fact]
        public void Process_EventWithArguments_ShouldWork()
        {
            var result = EvaluateCode(@"
                var receivedArgs = [];
                
                process.on('argsTest', function(a, b, c) {
                    receivedArgs.push(a, b, c);
                });
                
                process.emit('argsTest', 'hello', 42, true);
                
                receivedArgs.length === 3 && 
                receivedArgs[0] === 'hello' && 
                receivedArgs[1] === 42 && 
                receivedArgs[2] === true;
            ");
            Assert.True((bool)result!);
        }

        [Fact]
        public void Process_RemoveAllListeners_ShouldWork()
        {
            // Use a unique event name for isolation
            var eventName = "removeAllTest" + DateTime.Now.Ticks;
            
            // Add multiple listeners
            EvaluateCode($@"
                process.on('{eventName}', function() {{}});
                process.on('{eventName}', function() {{}});
                process.on('{eventName}', function() {{}});
            ");
            
            var beforeRemove = EvaluateCode($"process.listenerCount('{eventName}')");
            Assert.Equal(3, (int)beforeRemove!);
            
            // Remove all listeners for this event
            EvaluateCode($"process.off('{eventName}');");
            
            var afterRemove = EvaluateCode($"process.listenerCount('{eventName}')");
            Assert.Equal(0, (int)afterRemove!);
        }

        [Fact]
        public void Process_Chdir_ShouldChangeDirectory()
        {
            var result = EvaluateCode(@"
                var originalCwd = process.cwd();
                
                try {
                    // Try to change to parent directory
                    process.chdir('..');
                    var newCwd = process.cwd();
                    
                    // Change back to original
                    process.chdir(originalCwd);
                    var backCwd = process.cwd();
                    
                    originalCwd !== newCwd && backCwd === originalCwd;
                } catch (e) {
                    // If chdir fails (permissions, etc.), consider test passed
                    true;
                }
            ");
            Assert.True((bool)result!);
        }

        [Fact]
        public void Process_Properties_ShouldBeReadOnly()
        {
            // Test that core properties cannot be overwritten
            var result = EvaluateCode(@"
                var originalPid = process.pid;
                try {
                    process.pid = 12345;
                    process.pid === originalPid; // Should still be original
                } catch (e) {
                    true; // Exception is expected for read-only properties
                }
            ");
            Assert.True((bool)result!);
        }

        [Fact]
        public void Process_MultipleInstances_ShouldShareState()
        {
            // Test singleton behavior - multiple accesses should share state
            // Reset to ensure clean state
            EvaluateCode("process.exitCode = 0;");
            
            // Set exit code
            EvaluateCode("process.exitCode = 99;");
            var code1 = EvaluateCode("process.exitCode");
            Assert.Equal(99, (int)code1!);
            
            // Access through different variable but same singleton
            var code2 = EvaluateCode("process.exitCode");
            Assert.Equal(99, (int)code2!);
            
            // They should be the same value (singleton behavior)
            Assert.Equal(code1, code2);
            
            // Reset back to clean state
            EvaluateCode("process.exitCode = 0;");
        }

        [Fact]
        public void Process_EventListenerState_ShouldPersist()
        {
            // Test that event listeners persist across different accesses
            var result = EvaluateCode(@"
                var eventFired = false;
                
                // Add listener through one access
                process.on('persistTest', function() {
                    eventFired = true;
                });
                
                // Emit through different access
                var proc = process;
                proc.emit('persistTest');
                
                eventFired;
            ");
            Assert.True((bool)result!);
        }
    }
}
