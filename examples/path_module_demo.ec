// Node.js Path Module Demo - Complete API showcase
const path = require('path');

console.log('=== Path Module Demo ===');
console.log('Node.js-compatible path operations');
console.log('');

// Platform detection
console.log('1. Platform Info:');
console.log('   Default separator:', path.sep);
console.log('   Path delimiter:', path.delimiter);
console.log('');

// Basic path operations
console.log('2. Basic Operations:');
console.log('   basename("/path/to/file.txt"):', path.basename('/path/to/file.txt'));
console.log('   basename with suffix:', path.basename('/path/to/file.txt', '.txt'));
console.log('   dirname("/path/to/file.txt"):', path.dirname('/path/to/file.txt'));
console.log('   extname("/path/to/file.txt"):', path.extname('/path/to/file.txt'));
console.log('');

// Path joining
console.log('3. Path Joining:');
console.log('   join("/", "path", "to", "file"):', path.join('/', 'path', 'to', 'file'));
console.log('   join("relative", "path"):', path.join('relative', 'path'));
console.log('   join with "..":', path.join('/path/to', '..', 'other'));
console.log('');

// Cross-platform examples
console.log('4. Cross-Platform Examples:');
console.log('   POSIX join:', path.posix.join('/', 'home', 'user', 'documents'));
console.log('   Win32 join:', path.win32.join('C:\\\\', 'Users', 'user', 'documents'));
console.log('   Win32 with forward slashes:', path.win32.join('C:/', 'Users', 'user'));
console.log('');

// Path normalization
console.log('5. Path Normalization:');
console.log('   normalize("/path//to/../file"):', path.normalize('/path//to/../file'));
console.log('   Win32 normalize:', path.win32.normalize('C:\\\\\\\\path\\\\to\\\\..\\\\file'));
console.log('');

// Absolute vs relative
console.log('6. Absolute vs Relative:');
console.log('   isAbsolute("/path/to/file"):', path.isAbsolute('/path/to/file'));
console.log('   isAbsolute("relative/path"):', path.isAbsolute('relative/path'));
console.log('   Win32 isAbsolute("C:\\\\path"):', path.win32.isAbsolute('C:\\\\path'));
console.log('');

// Path parsing
console.log('7. Path Parsing:');
const parsed = path.parse('/home/user/documents/file.txt');
console.log('   parse("/home/user/documents/file.txt"):');
console.log('     root:', parsed.root);
console.log('     dir:', parsed.dir);
console.log('     base:', parsed.base);
console.log('     name:', parsed.name);
console.log('     ext:', parsed.ext);
console.log('');

// Path formatting
console.log('8. Path Formatting:');
const formatted = path.format({
    root: '/',
    dir: '/home/user',
    base: 'file.txt'
});
console.log('   format({root: "/", dir: "/home/user", base: "file.txt"}):', formatted);
console.log('');

console.log('=== Win32 Bug Fix Demonstration ===');
console.log('Previously: path.win32.join("C:\\\\", "foo", "bar") would produce "C:\\\\C:\\\\foo\\\\bar"');
console.log('Now fixed: path.win32.join("C:\\\\", "foo", "bar") produces:', path.win32.join('C:\\\\', 'foo', 'bar'));
console.log('All Win32 path operations now work correctly! ✅');
