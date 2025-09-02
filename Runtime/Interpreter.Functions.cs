using ECEngine.AST;
using ECEngine.Lexer;
using System.Reflection;

namespace ECEngine.Runtime;

/// <summary>
/// Function declarations, expressions, calls, and execution for ECEngine interpreter
/// </summary>
public partial class Interpreter
{
    /// <summary>
    /// Evaluate function declarations
    /// </summary>
    private object? EvaluateFunctionDeclaration(FunctionDeclaration funcDecl)
    {
        // Create closure with current scope variables (maintain references, not copies)
        var closure = new Dictionary<string, VariableInfo>();
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!closure.ContainsKey(kvp.Key))
                {
                    // Store reference to the actual VariableInfo, not a copy
                    closure[kvp.Key] = kvp.Value;
                }
            }
        }
        
        var function = new Function(funcDecl.Name, funcDecl.Parameters, funcDecl.Body, closure);
        DeclareVariable("function", funcDecl.Name, function);
        return function;
    }

    /// <summary>
    /// Evaluate function expressions
    /// </summary>
    private object? EvaluateFunctionExpression(FunctionExpression funcExpr)
    {
        // Create closure with current scope variables (maintain references, not copies)
        var closure = new Dictionary<string, VariableInfo>();
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!closure.ContainsKey(kvp.Key))
                {
                    // Store reference to the actual VariableInfo, not a copy
                    closure[kvp.Key] = kvp.Value;
                }
            }
        }
        
        // Anonymous function - no name, just return the function object
        return new Function(null, funcExpr.Parameters, funcExpr.Body, closure);
    }

    /// <summary>
    /// Evaluate arrow function expressions
    /// </summary>
    private object? EvaluateArrowFunctionExpression(ArrowFunctionExpression arrowFuncExpr)
    {
        // Create closure with current scope variables (maintain references, not copies)
        var closure = new Dictionary<string, VariableInfo>();
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!closure.ContainsKey(kvp.Key))
                {
                    // Store reference to the actual VariableInfo, not a copy
                    closure[kvp.Key] = kvp.Value;
                }
            }
        }
        
        // For arrow functions with expression body, wrap it in a return statement
        List<Statement> body;
        if (arrowFuncExpr.IsExpressionBody && arrowFuncExpr.Body != null)
        {
            // Create implicit return statement
            body = new List<Statement> { new ReturnStatement(arrowFuncExpr.Body) };
        }
        else if (arrowFuncExpr.BlockBody != null)
        {
            // Use the block body as-is
            body = arrowFuncExpr.BlockBody;
        }
        else
        {
            throw new ECEngineException("Arrow function must have either expression or block body", 
                arrowFuncExpr.Token?.Line ?? 0, arrowFuncExpr.Token?.Column ?? 0, _sourceCode, 
                "Arrow function body error");
        }
        
        // Arrow function - no name, just return the function object
        return new Function(null, arrowFuncExpr.Parameters, body, closure);
    }

    /// <summary>
    /// Evaluate generator function declarations
    /// </summary>
    private object? EvaluateGeneratorFunctionDeclaration(GeneratorFunctionDeclaration genFuncDecl)
    {
        // Create closure with current scope variables (maintain references, not copies)
        var closure = new Dictionary<string, VariableInfo>();
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!closure.ContainsKey(kvp.Key))
                {
                    // Store reference to the actual VariableInfo, not a copy
                    closure[kvp.Key] = kvp.Value;
                }
            }
        }
        
        // Create generator function and store it in current scope
        var generator = new GeneratorFunction(genFuncDecl.Name, genFuncDecl.Parameters, genFuncDecl.Body, closure);
        if (genFuncDecl.Name != null)
        {
            DeclareVariable("function", genFuncDecl.Name, generator);
        }
        
        return generator;
    }

    /// <summary>
    /// Evaluate generator function expressions
    /// </summary>
    private object? EvaluateGeneratorFunctionExpression(GeneratorFunctionExpression genFuncExpr)
    {
        // Create closure with current scope variables (maintain references, not copies)
        var closure = new Dictionary<string, VariableInfo>();
        foreach (var scope in _scopes.Reverse())
        {
            foreach (var kvp in scope)
            {
                if (!closure.ContainsKey(kvp.Key))
                {
                    // Store reference to the actual VariableInfo, not a copy
                    closure[kvp.Key] = kvp.Value;
                }
            }
        }
        
        // Anonymous generator function - no name, just return the generator function object
        return new GeneratorFunction(null, genFuncExpr.Parameters, genFuncExpr.Body, closure);
    }

    /// <summary>
    /// Public function call optimization method
    /// </summary>
    public object? EvaluateCallExpressionOptimized(CallExpression call)
    {
        // Performance-optimized version for critical path calls
        // Currently just delegates to main implementation
        return EvaluateCallExpression(call);
    }

    /// <summary>
    /// Evaluate function call expressions
    /// </summary>
    private object? EvaluateCallExpression(CallExpression call)
    {
        var function = Evaluate(call.Callee, _sourceCode);
        var arguments = call.Arguments.Select(arg => Evaluate(arg, _sourceCode)).ToList();

        // Detect if this is a method call on an object (callee is MemberExpression)
        object? thisContext = null;
        if (call.Callee is MemberExpression memberExpr)
        {
            thisContext = Evaluate(memberExpr.Object, _sourceCode);
        }

        // Handle built-in functions - delegate to specialized handlers
        return function switch
        {
            // Console functions
            ConsoleLogFunction => HandleConsoleLog(arguments),
            
            // Async/Timer functions
            SetTimeoutFunction setTimeoutFunc => setTimeoutFunc.Call(arguments),
            SetIntervalFunction setIntervalFunc => setIntervalFunc.Call(arguments),
            ClearTimeoutFunction clearTimeoutFunc => clearTimeoutFunc.Call(arguments),
            ClearIntervalFunction clearIntervalFunc => clearIntervalFunc.Call(arguments),
            NextTickFunction nextTickFunc => nextTickFunc.Call(arguments),
            
            // HTTP functions (legacy)
            ServerMethodFunction serverMethodFunc => serverMethodFunc.Call(arguments),
            ResponseMethodFunction responseMethodFunc => responseMethodFunc.Call(arguments),
            ObservableServerMethodFunction observableServerMethodFunc => observableServerMethodFunc.Call(arguments),
            ResponseWrapperFunction responseWrapperFunc => responseWrapperFunc.Call(arguments),
            
            // HTTP functions (new Node.js-compatible)
            HttpResponseMethodFunction httpResponseMethodFunc => httpResponseMethodFunc.Call(arguments),
            
            // Buffer functions
            BufferConstructor bufferConstructor => bufferConstructor.Call(arguments),
            BufferAllocFunction bufferAllocFunc => bufferAllocFunc.Call(arguments),
            BufferAllocUnsafeFunction bufferAllocUnsafeFunc => bufferAllocUnsafeFunc.Call(arguments),
            BufferAllocUnsafeSlowFunction bufferAllocUnsafeSlowFunc => bufferAllocUnsafeSlowFunc.Call(arguments),
            BufferByteLengthFunction bufferByteLengthFunc => bufferByteLengthFunc.Call(arguments),
            BufferCompareFunction bufferCompareFunc => bufferCompareFunc.Call(arguments),
            BufferConcatFunction bufferConcatFunc => bufferConcatFunc.Call(arguments),
            BufferFromFunction bufferFromFunc => bufferFromFunc.Call(arguments),
            BufferIsBufferFunction bufferIsBufferFunc => bufferIsBufferFunc.Call(arguments),
            BufferIsEncodingFunction bufferIsEncodingFunc => bufferIsEncodingFunc.Call(arguments),
            BufferMethodFunction bufferMethodFunc => bufferMethodFunc.Call(arguments),
            
            // Date functions
            DateModule dateModuleAsFunc => dateModuleAsFunc.Constructor.Call(arguments),
            DateConstructorFunction dateConstructorFunc => dateConstructorFunc.Call(arguments),
            DateNowFunction dateNowFunc => dateNowFunc.Call(arguments),
            DateParseFunction dateParseFunc => dateParseFunc.Call(arguments),
            DateUTCFunction dateUTCFunc => dateUTCFunc.Call(arguments),
            DateMethodFunction dateMethodFunc => dateMethodFunc.Call(arguments),
            
            // Math and JSON functions
            MathMethodFunction mathMethodFunc => mathMethodFunc.Call(arguments),
            
            // Math function objects
            MathAbsFunction mathAbsFunc => mathAbsFunc.Call(arguments),
            MathAcosFunction mathAcosFunc => mathAcosFunc.Call(arguments),
            MathAsinFunction mathAsinFunc => mathAsinFunc.Call(arguments),
            MathAtanFunction mathAtanFunc => mathAtanFunc.Call(arguments),
            MathAtan2Function mathAtan2Func => mathAtan2Func.Call(arguments),
            MathCeilFunction mathCeilFunc => mathCeilFunc.Call(arguments),
            MathCosFunction mathCosFunc => mathCosFunc.Call(arguments),
            MathExpFunction mathExpFunc => mathExpFunc.Call(arguments),
            MathFloorFunction mathFloorFunc => mathFloorFunc.Call(arguments),
            MathLogFunction mathLogFunc => mathLogFunc.Call(arguments),
            MathMaxFunction mathMaxFunc => mathMaxFunc.Call(arguments),
            MathMinFunction mathMinFunc => mathMinFunc.Call(arguments),
            MathPowFunction mathPowFunc => mathPowFunc.Call(arguments),
            MathRandomFunction mathRandomFunc => mathRandomFunc.Call(arguments),
            MathRoundFunction mathRoundFunc => mathRoundFunc.Call(arguments),
            MathSinFunction mathSinFunc => mathSinFunc.Call(arguments),
            MathSqrtFunction mathSqrtFunc => mathSqrtFunc.Call(arguments),
            MathTanFunction mathTanFunc => mathTanFunc.Call(arguments),
            MathTruncFunction mathTruncFunc => mathTruncFunc.Call(arguments),
            
            JsonMethodFunction jsonMethodFunc => jsonMethodFunc.Call(arguments),
            QuerystringMethodFunction querystringMethodFunc => querystringMethodFunc.Call(arguments),
            PathMethodFunction pathMethodFunc => pathMethodFunc.Call(arguments),
            ProcessMethodFunction processMethodFunc => processMethodFunc.Call(arguments),
            ObjectMethodFunction objectMethodFunc => objectMethodFunc.Call(arguments),
            
            // EventEmitter functions (legacy)
            EventEmitterCreateFunction eventEmitterCreateFunc => eventEmitterCreateFunc.Call(arguments),
            EventEmitterOnFunction eventEmitterOnFunc => eventEmitterOnFunc.Call(arguments),
            EventEmitterEmitFunction eventEmitterEmitFunc => HandleEventEmitterEmit(eventEmitterEmitFunc, arguments),
            EventEmitterOffFunction eventEmitterOffFunc => eventEmitterOffFunc.Call(arguments),
            EventEmitterRemoveAllFunction eventEmitterRemoveAllFunc => eventEmitterRemoveAllFunc.Call(arguments),
            EventEmitterListenerCountFunction eventEmitterListenerCountFunc => eventEmitterListenerCountFunc.Call(arguments),
            EventEmitterListenersFunction eventEmitterListenersFunc => eventEmitterListenersFunc.Call(arguments),
            EventEmitterEventNamesFunction eventEmitterEventNamesFunc => eventEmitterEventNamesFunc.Call(arguments),
            
            // New EventEmitter functions (Node.js compatible)
            EventEmitterConstructor eventEmitterConstructor => eventEmitterConstructor.Call(arguments),
            NodeEventEmitterOnFunction nodeEventEmitterOnFunc => nodeEventEmitterOnFunc.Call(arguments),
            NodeEventEmitterEmitFunction nodeEventEmitterEmitFunc => HandleNodeEventEmitterEmit(nodeEventEmitterEmitFunc, arguments),
            NodeEventEmitterOffFunction nodeEventEmitterOffFunc => nodeEventEmitterOffFunc.Call(arguments),
            NodeEventEmitterOnceFunction eventEmitterOnceFunc => eventEmitterOnceFunc.Call(arguments),
            NodeEventEmitterPrependListenerFunction eventEmitterPrependFunc => eventEmitterPrependFunc.Call(arguments),
            NodeEventEmitterPrependOnceListenerFunction eventEmitterPrependOnceFunc => eventEmitterPrependOnceFunc.Call(arguments),
            NodeEventEmitterRawListenersFunction eventEmitterRawListenersFunc => eventEmitterRawListenersFunc.Call(arguments),
            NodeEventEmitterListenersFunction nodeEventEmitterListenersFunc => nodeEventEmitterListenersFunc.Call(arguments),
            NodeEventEmitterListenerCountFunction nodeEventEmitterListenerCountFunc => nodeEventEmitterListenerCountFunc.Call(arguments),
            NodeEventEmitterEventNamesFunction nodeEventEmitterEventNamesFunc => nodeEventEmitterEventNamesFunc.Call(arguments),
            NodeEventEmitterSetMaxListenersFunction eventEmitterSetMaxFunc => eventEmitterSetMaxFunc.Call(arguments),
            NodeEventEmitterGetMaxListenersFunction eventEmitterGetMaxFunc => eventEmitterGetMaxFunc.Call(arguments),
            NodeEventEmitterRemoveAllListenersFunction eventEmitterRemoveAllFunc => eventEmitterRemoveAllFunc.Call(arguments),
            
            // Events module static functions
            EventsOnceFunction eventsOnceFunc => eventsOnceFunc.Call(arguments),
            EventsListenerCountFunction eventsListenerCountFunc => eventsListenerCountFunc.Call(arguments),
            EventsGetEventListenersFunction eventsGetEventListenersFunc => eventsGetEventListenersFunc.Call(arguments),
            EventsGetMaxListenersFunction eventsGetMaxFunc => eventsGetMaxFunc.Call(arguments),
            EventsSetMaxListenersFunction eventsSetMaxFunc => eventsSetMaxFunc.Call(arguments),
            EventsAddAbortListenerFunction eventsAddAbortFunc => eventsAddAbortFunc.Call(arguments),
            
            // String functions
            StringModule stringModule => stringModule.Call(arguments),
            StringStaticMethodFunction stringStaticFunc => stringStaticFunc.Call(arguments),
            StringMethodFunction stringMethodFunc => stringMethodFunc.Call(arguments),
            ArrayMethodFunction arrayMethodFunc => arrayMethodFunc.Call(arguments),
            
            // Utility functions
            UtilMethodFunction utilMethodFunc => utilMethodFunc.Call(arguments),
            InspectFunction inspectFunc => inspectFunc.Call(arguments),
            
            // URL functions
            UrlMethodFunction urlMethodFunc => urlMethodFunc.Call(arguments),
            UrlParseFunction urlParseFunc => urlParseFunc.Call(arguments),
            UrlFormatFunction urlFormatFunc => urlFormatFunc.Call(arguments),
            UrlResolveFunction urlResolveFunc => urlResolveFunc.Call(arguments),
            DomainToASCIIFunction domainToASCIIFunc => domainToASCIIFunc.Call(arguments),
            DomainToUnicodeFunction domainToUnicodeFunc => domainToUnicodeFunc.Call(arguments),
            FileURLToPathFunction fileURLToPathFunc => fileURLToPathFunc.Call(arguments),
            PathToFileURLFunction pathToFileURLFunc => pathToFileURLFunc.Call(arguments),
            UrlToHttpOptionsFunction urlToHttpOptionsFunc => urlToHttpOptionsFunc.Call(arguments),
            UrlConstructorFunction urlConstructorFunc => urlConstructorFunc.Call(arguments),
            URLSearchParamsConstructorFunction urlSearchParamsConstructorFunc => urlSearchParamsConstructorFunc.Call(arguments),
            URLSearchParamsMethodFunction urlSearchParamsMethodFunc => urlSearchParamsMethodFunc.Call(arguments),
            
            // Module system
            RequireFunction requireFunc => requireFunc.Call(arguments.ToArray()),
            
            // Filesystem functions
            ReadFileFunction readFileFunc => readFileFunc.Call(arguments.ToArray()),
            ReadFileSyncFunction readFileSyncFunc => readFileSyncFunc.Call(arguments.ToArray()),
            WriteFileFunction writeFileFunc => writeFileFunc.Call(arguments.ToArray()),
            WriteFileSyncFunction writeFileSyncFunc => writeFileSyncFunc.Call(arguments.ToArray()),
            ExistsFunction existsFunc => existsFunc.Call(arguments.ToArray()),
            StatFunction statFunc => statFunc.Call(arguments.ToArray()),
            StatSyncFunction statSyncFunc => statSyncFunc.Call(arguments.ToArray()),
            MkdirFunction mkdirFunc => mkdirFunc.Call(arguments.ToArray()),
            MkdirSyncFunction mkdirSyncFunc => mkdirSyncFunc.Call(arguments.ToArray()),
            RmdirFunction rmdirFunc => rmdirFunc.Call(arguments.ToArray()),
            RmdirSyncFunction rmdirSyncFunc => rmdirSyncFunc.Call(arguments.ToArray()),
            UnlinkFunction unlinkFunc => unlinkFunc.Call(arguments.ToArray()),
            UnlinkSyncFunction unlinkSyncFunc => unlinkSyncFunc.Call(arguments.ToArray()),
            ReaddirFunction readdirFunc => readdirFunc.Call(arguments.ToArray()),
            ReaddirSyncFunction readdirSyncFunc => readdirSyncFunc.Call(arguments.ToArray()),
            AppendFileFunction appendFileFunc => appendFileFunc.Call(arguments.ToArray()),
            AppendFileSyncFunction appendFileSyncFunc => appendFileSyncFunc.Call(arguments.ToArray()),
            ExistsSyncFunction existsSyncFunc => existsSyncFunc.Call(arguments.ToArray()),
            CopyFileFunction copyFileFunc => copyFileFunc.Call(arguments.ToArray()),
            CopyFileSyncFunction copyFileSyncFunc => copyFileSyncFunc.Call(arguments.ToArray()),
            RenameFunction renameFunc => renameFunc.Call(arguments.ToArray()),
            RenameSyncFunction renameSyncFunc => renameSyncFunc.Call(arguments.ToArray()),
            RealpathFunction realpathFunc => realpathFunc.Call(arguments.ToArray()),
            RealpathSyncFunction realpathSyncFunc => realpathSyncFunc.Call(arguments.ToArray()),
            
            // File stats functions
            IsFileFunction isFileFunc => isFileFunc.Call(arguments.ToArray()),
            IsDirectoryFunction isDirFunc => isDirFunc.Call(arguments.ToArray()),
            
            // HTTP functions
            CreateServerFunction createServerFunc => createServerFunc.Call(arguments),
            RequestFunction requestFunc => requestFunc.Call(arguments),
            GetFunction getFunc => getFunc.Call(arguments),
            ServerListenFunction serverListenFunc => serverListenFunc.Call(arguments),
            ServerCloseFunction serverCloseFunc => serverCloseFunc.Call(arguments),
            ServerOnFunction serverOnFunc => serverOnFunc.Call(arguments),
            ServerEmitFunction serverEmitFunc => serverEmitFunc.Call(arguments),
            ClientRequestWriteFunction clientWriteFunc => clientWriteFunc.Call(arguments),
            ClientRequestEndFunction clientEndFunc => clientEndFunc.Call(arguments),
            ClientRequestOnFunction clientOnFunc => clientOnFunc.Call(arguments),
            IncomingMessageOnFunction messageOnFunc => messageOnFunc.Call(arguments),
            IncomingMessageSetEncodingFunction messageSetEncodingFunc => messageSetEncodingFunc.Call(arguments),
            
            // C# delegate support
            Func<object[], object> delegateFunc => delegateFunc(arguments.ToArray()),
            
            // Generator functions
            GeneratorMethodFunction generatorMethodFunc => generatorMethodFunc.Call(arguments),
            GeneratorFunction generatorFunction => CallGeneratorFunction(generatorFunction, arguments, thisContext),
            
            // User-defined functions
            Function userFunction => CallUserFunction(userFunction, arguments, thisContext),
            
            _ => throw new ECEngineException($"Cannot call {function?.GetType().Name}",
                call.Token?.Line ?? 1, call.Token?.Column ?? 1, _sourceCode,
                "Attempted to call a non-function value")
        };
    }

    /// <summary>
    /// Handle console.log calls specifically
    /// </summary>
    private object? HandleConsoleLog(List<object?> arguments)
    {
        foreach (var arg in arguments)
        {
            Console.WriteLine(FormatObjectForConsole(arg));
        }
        return null; // console.log returns undefined
    }

    /// <summary>
    /// Handle EventEmitter emit calls with special listener execution
    /// </summary>
    private object? HandleEventEmitterEmit(EventEmitterEmitFunction eventEmitterEmitFunc, List<object?> arguments)
    {
        if (arguments.Count < 1)
        {
            return eventEmitterEmitFunc.Call(arguments);
        }

        var (eventName, eventArgs, listeners) = eventEmitterEmitFunc.GetEventData(arguments);
        
        if (listeners.Count == 0)
        {
            return false;
        }

        // Call all listeners with the provided arguments
        foreach (var listener in listeners.ToList())
        {
            try
            {
                CallUserFunction(listener, eventArgs);
            }
            catch (Exception ex)
            {
                // In Node.js, EventEmitter errors are handled specially
                Console.WriteLine($"Warning: EventEmitter listener error: {ex.Message}");
            }
        }

        return true;
    }

    /// <summary>
    /// Handle Node.js EventEmitter emit calls with EventListenerInfo support
    /// </summary>
    private object? HandleNodeEventEmitterEmit(NodeEventEmitterEmitFunction nodeEventEmitterEmitFunc, List<object?> arguments)
    {
        if (arguments.Count < 1)
        {
            throw new ECEngineException("EventEmitter.emit() requires at least 1 argument: event name",
                1, 1, "", "Usage: emitter.emit('event', arg1, arg2, ...)");
        }

        var eventName = arguments[0]?.ToString() ?? "";
        var eventArgs = arguments.Skip(1).ToList();

        // Get the event listeners from the emit function
        var (events, emitterInstance, captureRejections) = nodeEventEmitterEmitFunc.GetEventData();

        // Special handling for 'error' events
        if (eventName == "error" && (!events.ContainsKey("error") || events["error"].Count == 0))
        {
            // If no error listeners and it's an error event, throw the error
            var error = eventArgs.Count > 0 ? eventArgs[0] : new ECEngineException("Unhandled 'error' event", 1, 1, "", "");
            throw new ECEngineException($"Unhandled 'error' event: {error}", 1, 1, "", "Add an error event listener to handle this event");
        }

        // Check if there are listeners for this event
        if (!events.ContainsKey(eventName) || events[eventName].Count == 0)
        {
            return false; // No listeners
        }

        var listeners = events[eventName].ToList(); // Copy to avoid modification during iteration
        var hadListeners = listeners.Count > 0;

        // Execute all listeners
        var listenersToRemove = new List<EventListenerInfo>();
        
        foreach (var listenerInfo in listeners)
        {
            try
            {
                // Call the listener function using the interpreter
                CallUserFunction(listenerInfo.Listener, eventArgs);
                
                // Mark once listeners for removal
                if (listenerInfo.Once)
                {
                    listenersToRemove.Add(listenerInfo);
                }
            }
            catch (Exception ex)
            {
                if (captureRejections)
                {
                    // Handle promise rejections (Node.js feature)
                    // For now, just log the error
                    Console.WriteLine($"Warning: EventEmitter promise rejection: {ex.Message}");
                }
                else
                {
                    // Re-throw the error as Node.js would do
                    throw;
                }
            }
        }

        // Remove once listeners after execution
        foreach (var listenerInfo in listenersToRemove)
        {
            events[eventName].Remove(listenerInfo);
        }

        return hadListeners;
    }

    /// <summary>
    /// Call a user-defined function with proper scope and parameter handling
    /// </summary>
    private object? CallUserFunction(Function function, List<object?> arguments, object? thisContext = null)
    {
        return CallUserFunction(function, arguments.ToArray(), thisContext);
    }

    /// <summary>
    /// Call a user-defined function with proper scope and parameter handling
    /// </summary>
    private object? CallUserFunction(Function function, object[] arguments, object? thisContext = null)
    {
        // Save current state
        var originalScopes = new Stack<Dictionary<string, VariableInfo>>(_scopes.Reverse());
        var originalThisStack = new Stack<object?>(_thisStack.Reverse());
        
        try
        {
            // Push 'this' context for the function call
            _thisStack.Push(thisContext);
            
            // Restore closure scope (function's lexical environment)
            _scopes.Clear();
            
            // First, add the closure scopes in the correct order
            var closureScopes = new Stack<Dictionary<string, VariableInfo>>();
            foreach (var kvp in function.Closure)
            {
                // We need to group variables by their original scope level
                // For now, we'll put all closure variables in a single scope
                if (closureScopes.Count == 0)
                {
                    closureScopes.Push(new Dictionary<string, VariableInfo>());
                }
                closureScopes.Peek()[kvp.Key] = kvp.Value;
            }
            
            // Add closure scopes to the interpreter
            foreach (var scope in closureScopes)
            {
                _scopes.Push(scope);
            }
            
            // If no closure scopes, ensure we have at least a global scope
            if (_scopes.Count == 0)
            {
                _scopes.Push(new Dictionary<string, VariableInfo>());
            }
            
            // Push a new scope for function parameters and local variables
            PushScope();
            
            // Bind parameters to arguments
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                var paramName = function.Parameters[i];
                var argValue = i < arguments.Length ? arguments[i] : null;
                DeclareVariable("var", paramName, argValue);
            }
            
            // Execute function body
            object? result = null;
            try
            {
                foreach (var statement in function.Body)
                {
                    Evaluate(statement, _sourceCode);
                }
                // If no return statement was executed, return undefined (null)
                result = null;
            }
            catch (ReturnException returnEx)
            {
                result = returnEx.Value;
            }
            
            return result;
        }
        finally
        {
            // Restore original state
            _scopes = originalScopes;
            _thisStack = originalThisStack;
            SyncVariables();
        }
    }

    /// <summary>
    /// Call a generator function and return a generator instance
    /// </summary>
    private object? CallGeneratorFunction(GeneratorFunction generatorFunction, List<object?> arguments, object? thisContext = null)
    {
        // Create a new generator instance
        var generator = new Generator(generatorFunction.Name, generatorFunction.Parameters, generatorFunction.Body, generatorFunction.Closure);
        generator.Initialize(this, arguments, thisContext);
        return generator;
    }

    /// <summary>
    /// Format an object for console output
    /// </summary>
    private string FormatObjectForConsole(object? obj)
    {
        return obj switch
        {
            null => "null",
            string s => s,
            bool b => b ? "true" : "false",
            double d when double.IsNaN(d) => "NaN",
            double d when double.IsPositiveInfinity(d) => "Infinity",
            double d when double.IsNegativeInfinity(d) => "-Infinity",
            Dictionary<string, object?> dict => FormatDictionary(dict),
            System.Collections.IEnumerable enumerable when !(enumerable is string) => 
                $"[{string.Join(", ", enumerable.Cast<object?>().Select(FormatObjectForConsole))}]",
            _ when IsUndefined(obj) => "undefined",
            _ => obj?.ToString() ?? "undefined"
        };
    }

    /// <summary>
    /// Format a dictionary for console output
    /// </summary>
    private string FormatDictionary(Dictionary<string, object?> dict)
    {
        var pairs = dict.Select(kvp => $"{kvp.Key}: {FormatObjectForConsole(kvp.Value)}");
        return $"{{ {string.Join(", ", pairs)} }}";
    }
}
