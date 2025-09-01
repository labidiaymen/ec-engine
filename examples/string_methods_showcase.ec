// ECEngine String Methods Showcase
// Demonstrates comprehensive JavaScript string API compatibility

console.log("🔤 ECEngine String Methods Showcase");
console.log("===================================\n");

// Basic String Operations
console.log("📝 Basic Operations:");
var text = "Hello ECEngine World";
console.log("Original:", text);
console.log("Length:", text.length);
console.log("charAt(6):", text.charAt(6));
console.log("charCodeAt(0):", text.charCodeAt(0));
console.log();

// Search & Test Methods
console.log("🔍 Search & Test:");
console.log("indexOf('Engine'):", text.indexOf('Engine'));
console.log("includes('ECEngine'):", text.includes('ECEngine'));
console.log("startsWith('Hello'):", text.startsWith('Hello'));
console.log("endsWith('World'):", text.endsWith('World'));
console.log();

// Extraction Methods
console.log("✂️ Extraction:");
console.log("slice(6, 14):", text.slice(6, 14));
console.log("substring(0, 5):", text.substring(0, 5));
console.log("at(-5):", text.at(-5)); // Negative indexing
console.log();

// Case Transformation
console.log("🔄 Case Transformation:");
console.log("toLowerCase():", text.toLowerCase());
console.log("toUpperCase():", text.toUpperCase());
console.log();

// String Building & Padding
console.log("🔨 Building & Padding:");
var version = "1.0";
console.log("Version:", version);
console.log("padStart(6, '0'):", version.padStart(6, '0'));
console.log("repeat(3):", "⭐".repeat(3));
console.log("concat():", "EC".concat("Engine", " v", version));
console.log();

// Modification & Cleanup
console.log("🧹 Modification & Cleanup:");
var messy = "  Hello World  ";
console.log("Original:", JSON.stringify(messy));
console.log("trim():", JSON.stringify(messy.trim()));
console.log("replace():", text.replace('World', 'Universe'));
console.log("replaceAll():", "abc abc abc".replaceAll('abc', 'xyz'));
console.log();

// Splitting & Arrays
console.log("📊 Splitting:");
console.log("split(' '):", text.split(' '));
var chars = text.split('');
console.log("split('') first 5:", chars.slice(0, 5));
console.log();

// Unicode & Advanced Features
console.log("🌍 Unicode & Advanced:");
var emoji = "Hello 🌟 World";
console.log("Unicode string:", emoji);
console.log("Length:", emoji.length);
console.log("codePointAt(6):", emoji.codePointAt(6));
console.log("isWellFormed():", emoji.isWellFormed());
console.log();

// Pattern Matching
console.log("🎯 Pattern Matching:");
var data = "Price: $99.99, Year: 2024";
console.log("Data:", data);
console.log("search digits:", data.search("\\d+"));
var matches = data.match("\\d+");
console.log("match digits:", matches);
console.log();

// Static Methods
console.log("⚡ Static Methods:");
console.log("fromCharCode(72,101,108,108,111):", String.fromCharCode(72, 101, 108, 108, 111));
console.log("fromCodePoint(128640):", String.fromCodePoint(128640)); // 🚀
console.log();

// String Constructor
console.log("🏗️ Constructor:");
console.log("String(42):", String(42));
console.log("String(true):", String(true));
console.log("String(null):", String(null));
console.log();

// HTML Methods (Legacy but Available)
console.log("📄 HTML Methods:");
var important = "IMPORTANT";
console.log("bold():", important.bold());
console.log("italics():", important.italics());
console.log("fontcolor('red'):", important.fontcolor('red'));
console.log();

console.log("✅ String Methods Showcase Complete!");
console.log("ECEngine supports 70+ JavaScript string methods");
