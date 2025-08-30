# ECEngine Feature Implementation Planning Guide

This comprehensive prompt will guide you through systematically planning and implementing new features in the ECEngine JavaScript interpreter. Follow this structured approach to ensure consistent, well-tested, and well-documented feature additions.

## 🎯 Feature Implementation Planning Process

### Phase 1: Analysis & Planning

#### 1.1 Codebase Analysis
**Prompt**: "Analyze the ECEngine codebase to understand the current implementation of [FEATURE_TYPE]. Examine these key areas:"

- **Lexer Analysis**: 
  - Check `Lexer/Lexer.cs` for existing token types
  - Review `Lexer/Token.cs` for token enum definitions
  - Identify any similar tokens already implemented

- **Parser Analysis**:
  - Examine relevant Parser files (`Parser/*.cs`)
  - Look for similar AST node implementations in `AST/ASTNode.cs`
  - Understand existing parsing patterns and precedence rules

- **Runtime Analysis**:
  - Review `Runtime/Interpreter.cs` for evaluation patterns
  - Check existing global object implementations (`Runtime/*Globals.cs`)
  - Understand current runtime error handling

- **Testing Analysis**:
  - Review existing test patterns in `Tests/` directory
  - Understand test organization and naming conventions
  - Check for similar feature test implementations

**Output**: Detailed analysis report identifying:
- Existing similar implementations to use as reference
- Required modifications to each component
- Potential conflicts or dependencies
- Complexity assessment and implementation strategy

#### 1.2 Feature Specification
**Prompt**: "Create a detailed specification for implementing [FEATURE_NAME] in ECEngine based on the codebase analysis:"

- **ECMAScript Compatibility**: Reference standard JavaScript behavior
- **Syntax Definition**: Exact syntax and grammar rules
- **Semantic Behavior**: Expected runtime behavior and edge cases
- **Error Handling**: Error conditions and appropriate error messages
- **Integration Points**: How it integrates with existing features

**Output**: Complete feature specification document with examples

### Phase 2: Implementation Planning

#### 2.1 Implementation Roadmap
**Prompt**: "Create a step-by-step implementation roadmap for [FEATURE_NAME] following ECEngine's architecture:"

**Required Steps:**
1. **Lexer Updates**
   - Token definitions to add
   - Tokenization logic modifications
   - Character recognition patterns

2. **AST Definitions**
   - New AST node classes needed
   - Properties and structure
   - Visitor pattern support

3. **Parser Updates**
   - Grammar rules and precedence
   - Parsing methods to add/modify
   - Error recovery strategies

4. **Runtime Implementation**
   - Interpreter evaluation logic
   - Runtime object/function implementations
   - Global object integration (if applicable)

5. **Testing Strategy**
   - Unit test categories needed
   - Integration test scenarios
   - Edge case coverage

6. **Documentation Updates**
   - README.md updates
   - FEATURES_CHECKLIST.md updates
   - Example creation

**Output**: Ordered checklist with file-specific implementation tasks

#### 2.2 Dependency Analysis
**Prompt**: "Analyze dependencies and prerequisites for implementing [FEATURE_NAME]:"

- **Required Dependencies**: Features that must exist first
- **Optional Dependencies**: Features that enhance functionality
- **Breaking Changes**: Potential impacts on existing code
- **Migration Strategy**: How to handle backward compatibility

**Output**: Dependency matrix and implementation order

### Phase 3: Implementation Steps

#### 3.1 Lexer Implementation
**Prompt**: "Implement lexer support for [FEATURE_NAME] following ECEngine patterns:"

**Tasks:**
- Update `Lexer/Token.cs` with new token types
- Modify `Lexer/Lexer.cs` tokenization logic
- Add character recognition and token creation
- Handle edge cases and invalid syntax
- Follow existing naming conventions

**Validation**: Test tokenization with simple examples

#### 3.2 AST Implementation
**Prompt**: "Create AST node definitions for [FEATURE_NAME] in ECEngine:"

**Tasks:**
- Add new AST node classes to `AST/ASTNode.cs`
- Implement required properties and constructors
- Follow existing AST patterns and naming
- Ensure proper inheritance hierarchy
- Add ToString() methods for debugging

**Validation**: Verify AST construction without evaluation

#### 3.3 Parser Implementation
**Prompt**: "Implement parser support for [FEATURE_NAME] following ECEngine's parsing patterns:"

**Tasks:**
- Add parsing methods to appropriate `Parser/*.cs` files
- Implement grammar rules and operator precedence
- Add error handling and recovery
- Update parsing dispatch logic
- Follow existing method naming conventions

**Validation**: Test parsing with valid and invalid syntax

#### 3.4 Runtime Implementation
**Prompt**: "Implement runtime evaluation for [FEATURE_NAME] in ECEngine:"

**Tasks:**
- Add evaluation logic to `Runtime/Interpreter.cs`
- Implement any required global objects/functions
- Add runtime error handling
- Follow existing evaluation patterns
- Ensure proper type conversion and coercion

**For Global Objects** (if applicable):
- Create `Runtime/[Feature]Globals.cs`
- Follow the pattern established in `MathGlobals.cs` and `DateGlobals.cs`
- Implement module and function classes
- Register in interpreter's global scope

**Validation**: Test runtime execution with comprehensive examples

### Phase 4: Testing & Validation

#### 4.1 Test Implementation
**Prompt**: "Create comprehensive tests for [FEATURE_NAME] following ECEngine's testing patterns:"

**Test Categories:**
- **Lexer Tests**: `Tests/Lexer/` - Token recognition
- **Parser Tests**: `Tests/Parser/` - AST construction
- **Interpreter Tests**: `Tests/Interpreter/` - Runtime behavior
- **Integration Tests**: `Tests/Integration/` - End-to-end scenarios
- **Error Handling Tests**: `Tests/ErrorHandling/` - Edge cases

**Test Structure:**
```csharp
[Collection("Console Tests")]
public class [Feature]Tests
{
    [Fact]
    public void Test_[Feature]_[Scenario]()
    {
        // Arrange
        // Act  
        // Assert
    }
}
```

**Validation**: Achieve high test coverage and pass all scenarios

#### 4.2 Example Creation
**Prompt**: "Create practical examples demonstrating [FEATURE_NAME] usage:"

**Example Categories:**
- **Basic Usage**: `examples/[feature]/basic_[feature].ec`
- **Advanced Usage**: `examples/[feature]/advanced_[feature].ec`
- **Integration**: `examples/[feature]/[feature]_with_other_features.ec`
- **Performance**: `examples/performance/[feature]_performance.ec`

**Example Structure:**
```javascript
// [Feature] Example - [Description]
// This example demonstrates [specific use case]

[code examples with comments]
```

### Phase 5: Documentation & Integration

#### 5.1 Documentation Updates
**Prompt**: "Update ECEngine documentation for [FEATURE_NAME]:"

**Required Updates:**
1. **README.md**:
   - Add feature to "Implemented Features" section
   - Update examples and capabilities
   - Update feature count if applicable

2. **FEATURES_CHECKLIST.md**:
   - Mark feature as ✅ implemented
   - Update progress summary counts
   - Add any sub-features or related items

3. **Feature-specific README** (if applicable):
   - Create `examples/[feature]/README.md`
   - Document usage patterns and best practices

#### 5.2 VS Code Extension Updates (if needed)
**Prompt**: "Update VS Code extension for [FEATURE_NAME] syntax highlighting:"

**Files to Update:**
- `vscode-extension/syntaxes/ecengine.tmLanguage.json`
- `vscode-extension/language-configuration.json`
- Test syntax highlighting with examples

### Phase 6: Quality Assurance

#### 6.1 Code Review Checklist
**Prompt**: "Perform code review for [FEATURE_NAME] implementation:"

**Review Areas:**
- **Architecture Consistency**: Follows ECEngine patterns
- **Error Handling**: Comprehensive and user-friendly
- **Performance**: Efficient implementation
- **Memory Safety**: No leaks or excessive allocations
- **Code Quality**: Clean, readable, and well-commented
- **Test Coverage**: Comprehensive and meaningful tests

#### 6.2 Integration Testing
**Prompt**: "Perform integration testing for [FEATURE_NAME]:"

**Test Scenarios:**
- Feature works with existing features
- No regressions in existing functionality
- Performance impact assessment
- Memory usage validation
- Error handling in complex scenarios

## 🛠️ Feature-Specific Templates

### For Language Constructs (loops, conditionals, etc.)
```
Feature: [CONSTRUCT_NAME]
Type: Language Construct
Tokens: [list of tokens needed]
AST Nodes: [list of AST nodes needed]
Parser Methods: [parsing methods to implement]
Runtime Logic: [evaluation logic description]
```

### For Global Objects (Math, Date, etc.)
```
Feature: [OBJECT_NAME] Global Object
Type: Global Object
Module Class: [ObjectName]Module
Function Classes: [list of function classes]
Methods: [list of methods to implement]
Properties: [list of properties to implement]
```

### For Operators
```
Feature: [OPERATOR_NAME] Operator
Type: Operator
Symbol: [operator symbol]
Precedence: [precedence level]
Associativity: [left/right/none]
Operand Types: [supported operand types]
```

## 📋 Implementation Checklist Template

For each feature implementation, use this checklist:

- [ ] **Analysis Complete**
  - [ ] Codebase analysis done
  - [ ] Feature specification written
  - [ ] Implementation plan created

- [ ] **Lexer Implementation**
  - [ ] Token types defined
  - [ ] Tokenization logic implemented
  - [ ] Edge cases handled

- [ ] **AST Implementation**
  - [ ] AST node classes created
  - [ ] Properties and methods implemented
  - [ ] ToString() methods added

- [ ] **Parser Implementation**
  - [ ] Grammar rules implemented
  - [ ] Precedence handled correctly
  - [ ] Error recovery added

- [ ] **Runtime Implementation**
  - [ ] Evaluation logic implemented
  - [ ] Global objects created (if applicable)
  - [ ] Error handling added

- [ ] **Testing**
  - [ ] Unit tests written
  - [ ] Integration tests written
  - [ ] Edge case tests written
  - [ ] All tests passing

- [ ] **Examples**
  - [ ] Basic examples created
  - [ ] Advanced examples created
  - [ ] Example README written

- [ ] **Documentation**
  - [ ] README.md updated
  - [ ] FEATURES_CHECKLIST.md updated
  - [ ] Code documented

- [ ] **Quality Assurance**
  - [ ] Code review completed
  - [ ] Integration testing done
  - [ ] Performance validated

## 🎯 Usage Instructions

1. **Copy this template** for each new feature
2. **Replace [FEATURE_NAME]** with your specific feature
3. **Follow the phases sequentially** - don't skip steps
4. **Use the provided prompts** to guide implementation
5. **Check off items** as you complete them
6. **Validate each phase** before moving to the next

This systematic approach ensures:
- ✅ Consistent code quality
- ✅ Comprehensive testing
- ✅ Proper documentation
- ✅ Smooth integration
- ✅ Maintainable codebase

## 🔍 Example Usage

To implement array literals (`[1, 2, 3]`):

1. **Analysis**: "Analyze ECEngine for array literal implementation requirements"
2. **Planning**: "Plan array literal implementation following ECEngine patterns"
3. **Implementation**: Follow lexer → AST → parser → runtime sequence
4. **Testing**: Create comprehensive test suite
5. **Documentation**: Update all relevant documentation
6. **Quality Assurance**: Review and validate implementation

Remember: **Quality over speed** - take time to implement features correctly rather than rushing through the process.
