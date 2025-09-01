// Advanced Import Features Demo
console.log("🚀 Advanced Import Features Demo");
console.log("=================================\n");

// 1. Namespace import (import * as module)
console.log("📦 Namespace Import:");
import * as MathUtils from "./modules/math-utils.ec";
console.log("MathUtils namespace:", Object.keys(MathUtils));
console.log("MathUtils.add(5, 3):", MathUtils.add(5, 3));
console.log("MathUtils.PI:", MathUtils.PI);
console.log("MathUtils.default(2, 3):", MathUtils.default(2, 3));
console.log();

// 2. Import renaming (import { name as newName })
console.log("🔄 Import Renaming:");
import { add as sum, multiply as product, PI as piValue } from "./modules/math-utils.ec";
console.log("sum(4, 6):", sum(4, 6));
console.log("product(3, 7):", product(3, 7));
console.log("piValue:", piValue);
console.log();

// 3. Mixed imports with renaming
console.log("🎯 Mixed Import with Renaming:");
import calculate, { add as addition, PI as pi } from "./modules/math-utils.ec";
console.log("calculate(2, 4):", calculate(2, 4));
console.log("addition(10, 5):", addition(10, 5));
console.log("pi:", pi);
console.log();

// 4. Multiple named imports with and without aliases
console.log("📋 Multiple Named Imports:");
import { greeting, version, format as formatMessage } from "./modules/utils.ec";
console.log("greeting:", greeting);
console.log("version:", version);
console.log("formatMessage('ECEngine'):", formatMessage('ECEngine'));
console.log();

// 5. Dynamic imports (import("./module"))
console.log("⚡ Dynamic Imports:");

// Dynamic import with string literal
var dynamicMath = import("./modules/math-utils.ec");
console.log("Dynamic import result keys:", Object.keys(dynamicMath));
console.log("Dynamic math add(1, 2):", dynamicMath.add(1, 2));
console.log("Dynamic math default(3, 4):", dynamicMath.default(3, 4));
console.log();

// Dynamic import with variable
var modulePath = "./modules/utils.ec";
var dynamicUtils = import(modulePath);
console.log("Dynamic utils keys:", Object.keys(dynamicUtils));
console.log("Dynamic utils format('World'):", dynamicUtils.format('World'));
console.log();

// 6. Complex example combining multiple features
console.log("🌟 Complex Example:");
import defaultCalc, { multiply as times } from "./modules/math-utils.ec";
import * as Utils from "./modules/utils.ec";

var result1 = defaultCalc(3, 4);
var result2 = times(result1, 2);
var message = Utils.format("Advanced Imports");

console.log("Complex calculation result:", result2);
console.log("Complex message:", message);
console.log();

console.log("✅ Advanced Import Features Demo Complete!");
console.log("All import types working: namespace, renaming, mixed, and dynamic!");
