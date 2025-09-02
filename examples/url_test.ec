// URL Module Test
console.log("=== URL Module Test ===");

// Test URL constructor
let url1 = new URL("https://example.com:8080/path?query=value#hash");
console.log("URL created:", url1.toString());
console.log("Protocol:", url1.protocol);
console.log("Hostname:", url1.hostname);
console.log("Port:", url1.port);
console.log("Pathname:", url1.pathname);
console.log("Search:", url1.search);
console.log("Hash:", url1.hash);

// Test URL with base
let url2 = new URL("/api/data", "https://api.example.com");
console.log("URL with base:", url2.toString());

// Test URL modification
url1.pathname = "/newpath";
url1.search = "?newquery=newvalue";
console.log("Modified URL:", url1.toString());

// Test URLSearchParams
let params = new URLSearchParams("foo=bar&baz=qux");
console.log("URLSearchParams created:", params.toString());

params.append("new", "value");
console.log("After append:", params.toString());

console.log("Get 'foo':", params.get("foo"));
console.log("Has 'baz':", params.has("baz"));

params.set("foo", "newbar");
console.log("After set:", params.toString());

params.delete("baz");
console.log("After delete:", params.toString());

// Test URL module functions
const url = require("node:url");

console.log("URL module loaded successfully");

// Test url.parse (legacy API)
let parsed = url.parse("https://example.com:8080/path?query=value#hash");
console.log("Parsed URL:", parsed);

// Test url.format
let formatted = url.format({
    protocol: "https:",
    hostname: "example.com",
    pathname: "/test",
    search: "?q=test"
});
console.log("Formatted URL:", formatted);

// Test url.resolve
let resolved = url.resolve("https://example.com/", "api/data");
console.log("Resolved URL:", resolved);

// Test utility functions
console.log("domainToASCII test:", url.domainToASCII("bücher.de"));
console.log("domainToUnicode test:", url.domainToUnicode("xn--bcher-kva.de"));

console.log("=== URL Module Test Complete ===");
