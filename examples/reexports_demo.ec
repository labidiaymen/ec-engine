// ECEngine Re-exports Demo
// This file demonstrates re-exporting from other modules

// ===== RE-EXPORTS WITHOUT RENAMING =====

// Re-export specific items from a math utilities module
export { add, subtract, multiply } from "./math_utils";

// Re-export specific items from a string utilities module  
export { capitalize, trim, split } from "./string_utils";

// ===== RE-EXPORTS WITH RENAMING =====

// Re-export with renaming to avoid conflicts or provide better names
export { 
    defaultConfig as config,
    developmentSettings as devSettings,
    productionSettings as prodSettings 
} from "./app_config";

// Re-export utility functions with descriptive names
export {
    validateEmail as isValidEmail,
    validatePhone as isValidPhone,
    sanitizeInput as cleanInput
} from "./validators";

// ===== MIXED RE-EXPORTS =====

// Some items renamed, some not
export {
    Logger,                    // exported as 'Logger'
    FileHandler as FileWriter, // exported as 'FileWriter'
    ErrorReporter             // exported as 'ErrorReporter'
} from "./logging";

// Re-export constants with prefixes to avoid naming conflicts
export {
    MAX_SIZE as FILE_MAX_SIZE,
    MIN_SIZE as FILE_MIN_SIZE,
    ALLOWED_TYPES as FILE_ALLOWED_TYPES
} from "./file_constants";

// ===== BARREL EXPORTS =====

// This pattern is commonly used to create "barrel" files that
// re-export everything from multiple modules in one place

// Database utilities
export { connect, disconnect, query } from "./db/connection";
export { User, Post, Comment } from "./db/models";
export { createUser as registerUser, findUser } from "./db/user_service";

// API utilities  
export { Router, middleware } from "./api/router";
export { authenticate, authorize } from "./api/auth";
export { 
    validate as validateRequest,
    sanitize as sanitizeRequest 
} from "./api/validation";

// ===== EMPTY RE-EXPORTS =====

// Sometimes you might want to conditionally re-export
// export { } from "./optional_module";

console.log("Re-exports module loaded!");

// Note: The imported modules (math_utils, string_utils, etc.) would need to exist
// for this to actually run. This demonstrates the syntax.
