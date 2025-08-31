// Test single module import
console.log("Testing single module import...");

import { greet, version } from "simple-utils";
console.log("Imported greet and version from simple-utils");
console.log("greet('Test'):", greet("Test"));
console.log("version:", version);
