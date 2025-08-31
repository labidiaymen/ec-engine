# Node.js Module Resolution Test

This directory demonstrates ECEngine's Node.js-style module resolution capabilities.

## Directory Structure

```
nodejs-modules-test/
├── main.ec                           # Main test file
├── subproject/
│   └── test.ec                       # Tests upward traversal
└── node_modules/
    ├── simple-utils/                 # Package with package.json main field
    │   ├── package.json              # Points to lib/index.js
    │   └── lib/
    │       └── index.js              # Actual module code
    ├── math-helpers/                 # Package with .ec main file
    │   ├── package.json              # Points to index.ec
    │   └── index.ec                  # ECEngine module
    ├── direct-module/                # Directory without package.json
    │   └── index.js                  # Uses index.js fallback
    └── color-utils.mjs               # Direct file module with .mjs extension
```

## Features Demonstrated

1. **Package.json main field resolution**
   - `simple-utils` uses package.json to point to `lib/index.js`
   - `math-helpers` uses package.json to point to `index.ec`

2. **Index file fallback**
   - `direct-module` has no package.json, so ECEngine looks for `index.js`

3. **Multi-extension support**
   - `.js`, `.ec`, and `.mjs` files are all supported
   - ECEngine automatically resolves the correct extension

4. **Direct file modules**
   - `color-utils.mjs` is imported directly as a file

5. **Upward node_modules traversal**
   - `subproject/test.ec` can import modules from parent's `node_modules`

## Running the Tests

From the ECEngine root directory:

```bash
# Run main test
dotnet run examples/nodejs-modules-test/main.ec

# Run subproject test (demonstrates upward traversal)
cd examples/nodejs-modules-test/subproject
dotnet run test.ec
```

## Expected Output

The tests should successfully import and use functions from all modules, demonstrating that ECEngine's Node.js-style module resolution is working correctly.
