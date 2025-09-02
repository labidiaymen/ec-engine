# Process Examples

This directory contains examples demonstrating ECEngine's Node.js-compatible `process` global object implementation.

## Examples

### 📋 `process_demo.ec`
Basic demonstration of core process properties and methods:
- Process information (`pid`, `platform`, `arch`, `version`)
- Environment variables (`process.env`)
- Command line arguments (`process.argv`)
- Working directory operations (`cwd()`, `chdir()`)
- Memory usage and timing

### 🎯 `process_simple_test.ec`
Simple test demonstrating basic process functionality:
- Property access and setting
- Event method availability
- Exit code management

### 🚀 `process_events_demo.ec`
Comprehensive demonstration of the process event system:
- Event listener registration (`process.on()`)
- Event emission (`process.emit()`)
- Event management (`eventNames()`, `listenerCount()`)
- Process lifecycle events (`beforeExit`, `exit`)
- High-resolution timing and memory usage
- Next tick scheduling

### 🔚 `process_exit_test.ec`
Basic process exit behavior testing:
- Normal exit with `beforeExit` event
- Exit event handling

### 📊 `process_exit_scenarios.ec`
Advanced exit scenarios demonstrating different exit behaviors:
- `process.exit()` with no arguments (triggers `beforeExit`)
- `process.exit(0)` with explicit 0 (no `beforeExit`)
- `process.exit(1)` with non-zero code (no `beforeExit`)
- Setting `exitCode` then calling `process.exit()` (no `beforeExit`)

## Running Examples

```bash
# Run any example from the project root
dotnet run -- examples/process/process_events_demo.ec

# Or with specific arguments for exit scenarios
dotnet run -- examples/process/process_exit_scenarios.ec 1
```

## Features Demonstrated

- ✅ Complete Node.js process API compatibility
- ✅ EventEmitter pattern implementation
- ✅ Process lifecycle event handling
- ✅ Cross-platform process information
- ✅ Memory usage and timing utilities
- ✅ Environment variable access
- ✅ Working directory management
- ✅ Exit code handling with proper event semantics
