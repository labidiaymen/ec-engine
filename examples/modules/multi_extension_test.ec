// ECEngine test file for multi-extension support
import { jsValue, jsFunction, number } from "./test.js";
import mjsDefault, { mjsValue, mjsFunction } from "./test.mjs";

console.log("JS Value:", jsValue);
console.log("JS Function:", jsFunction());
console.log("JS Number:", number);

console.log("MJS Value:", mjsValue);
console.log("MJS Function:", mjsFunction("ECEngine"));
console.log("MJS Default:", mjsDefault());
