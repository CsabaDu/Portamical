# Portamical.Core.Formatting

**Extensible, High-Performance Formatting Infrastructure for Portamical Test Data Framework**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Version](https://img.shields.io/badge/version-2.0.0-orange.svg)](https://github.com/CsabaDu/Portamical/releases)
[![C#](https://img.shields.io/badge/language-C%23-239120.svg)](https://docs.microsoft.com/dotnet/csharp/)

> **Extensible formatters, zero-allocation string building, and thread-safe custom formatter registry for human-readable test case names and diagnostic output.**

`Portamical.Core.Formatting` provides the formatting infrastructure used by [Portamical.Core](https://github.com/CsabaDu/Portamical/tree/master/Portamical.Core) and can be used independently in any .NET application that needs extensible, performant object-to-string conversion.

---

## What's New in v2.0.0

**Architecture Simplification & API Refinement**

**WHAT'S NEW:**  
- **Simplified formatter architecture** - 2 types instead of 3
- **Single inheritance model** via `Formatter<T>` base class
- **Flat namespace structure** for better discoverability
- **Configurable `maxCount` parameter** for Builder join methods
- **Enhanced XML documentation** (60+ fixes and clarifications)

**BREAKING CHANGES:**  
- Removed generic `IFormatter<T>` interface (replaced by `Formatter<T>` base class)
  - All custom formatters must now inherit from abstract `Formatter<T>` base class
  - Eliminates interface segregation - single inheritance model
  - **Migration:** Change `IFormatter<T>` → `Formatter<T>` and inherit instead of implement
- Deleted CustomFormatters namespace and folder structure
  - All formatter types now in root `Portamical.Core.Formatting` namespace

---

## Features

### **Extensible Formatter System**
- **`IFormatter` interface** - Base contract for all formatters
- **`Formatter<T>` abstract base class** - Template Method pattern with type safety for custom formatters
- **Thread-safe formatter registry** - Register/unregister formatters at runtime via `Formatter` static class
- **Priority system** - Custom formatters consulted before built-in patterns

### **High Performance**
- **Zero-allocation string building** - Uses `string.Create()` with `Span<char>`
- **66-75% fewer allocations** - For common formatting scenarios (2-3 item collections)
- **Aggressive inlining** - Hot-path helpers marked with `[MethodImpl(AggressiveInlining)]`
- **Optimized pattern matching** - Method overloading for efficient type dispatch

### **Built-in Formatters**
`DefaultFormatter` provides specialized formatting for 12+ .NET types:

| Type | Format Example |
|------|----------------|
| `string` | `"hello"` (quoted, except literal `null`) |
| `char` | `'a'` (single-quoted) |
| `DateTime`, `DateTimeOffset` | `2026-01-15T10:30:00.0000000Z` (ISO 8601) |
| `Guid` | `12345678-1234-1234-1234-123456789012` |
| `byte[]` | `01-02-03-FF` (hex) |
| `Exception` | `ArgumentException: Value cannot be null` |
| `Type` | `int`, `List<string>`, `int?`, `int[]` (C#-friendly names) |
| `Delegate` | `Func<int, string> (MethodName)` or `Action (anonymous)` |
| `KeyValuePair<K,V>` | `{"key": value}` |
| `Tuple` / `ValueTuple` | `(item1, item2, item3)` |
| Collections (`IEnumerable`) | `[3]: [1, 2, 3]` or `[First 3 of 3+]: [1, 2, 3]` |
| Dictionaries (`IDictionary`) | `[2]: {{"a": 1}, {"b": 2}}` |
| `Stream` | `MemoryStream (Length: 1024, Position: 0)` |

### **Utility Helpers**
- **`Builder.CreateSeparatedString`** - Zero-copy three-part string assembly
- **`Builder.JoinWithSeparator`, `Builder.JoinWithComma`** - Optimized for 0-3 item lists
- **`Builder.CopyAsSpan`** - Efficient character copying for `Span<char>`
- **`Builder.FallbackIfNull`** - Consistent `null` → `"null"` conversion
- **`Builder.FallbackIfNullSeparator`** - Consistent `null` → `", "` conversion

---

## Installation

```bash
dotnet add package Portamical.Core.Formatting
```

Or via NuGet Package Manager:
```powershell
Install-Package Portamical.Core.Formatting
```

---

## Quick Start

### Basic Formatting

```csharp
using Portamical.Core.Formatting;

// Format various types
var result1 = Formatter.Format("hello");           // ""hello""
var result2 = Formatter.Format('a');               // "'a'"
var result3 = Formatter.Format(42);                // "42"
var result4 = Formatter.Format(new[] { 1, 2, 3 }); // "[3]: [1, 2, 3]"

// Format tuples
var tuple = (name: "Alice", age: 30, active: true);
var result5 = Formatter.Format(tuple);  // "("Alice", 30, True)"

// Format DateTime (ISO 8601)
var date = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
var result6 = Formatter.Format(date);  // "2026-01-15T10:30:00.0000000Z"
```

### Custom Formatters

```csharp
using Portamical.Core.Formatting;

// 1. Define a custom type
public record ProductId(int Id);

// 2. Inherit from Formatter<T> base class
public sealed class ProductIdFormatter : Formatter<ProductId>
{
	public override string Format(ProductId value)
	{
		if (value is null)
			return Builder.NullString;

		return $"PROD-{value.Id:D6}";
	}
}

// 3. Register the formatter globally
Formatter.RegisterFormatter<ProductId>(new ProductIdFormatter());

// 4. Use it automatically
var productId = new ProductId(42);
var formatted = Formatter.Format(productId);
// Result: "PROD-000042" ?
```

### Using Formatter<T> Base Class

```csharp
using Portamical.Core.Formatting;

// Inherit from Formatter<T> for type-safe implementation
public sealed class MoneyFormatter : Formatter<Money>
{
	public override string Format(Money value)
	{
		if (value is null)
			return Builder.FallbackIfNull(null);  // Use Builder helper

		return $"{value.Currency} {value.Amount:N2}";
	}
}

// Register and use
Formatter.RegisterFormatter<Money>(new MoneyFormatter());

var price = new Money { Currency = "USD", Amount = 99.99m };
var formatted = Formatter.Format(price);
// Result: "USD 99.99" ?
```

### Checking Formatter Registration

```csharp
// Check if a formatter is registered
if (Formatter.IsFormatterRegistered<ProductId>())
{
	Console.WriteLine("ProductId formatter is active");
}

// Unregister a formatter
bool removed = Formatter.UnregisterFormatter<ProductId>();

// Clear all custom formatters (useful in test cleanup)
Formatter.ClearFormatters();

// Access the registry (read-only view)
var registry = Formatter.Registry;
Console.WriteLine($"Active custom formatters: {registry.Count}");
```

---

## Architecture

### Namespace Organization

All formatting types are in the root `Portamical.Core.Formatting` namespace:

- **`IFormatter.cs`** - Public base contract for all formatters
- **`Formatter.cs`** - Static registry + formatting pipeline class + abstract `Formatter<T>` base class  
- **`Builder.cs`** - String building utilities (FallbackIfNull, JoinWithComma, CreateSeparatedString)
- **`DefaultFormatter.cs`** - Singleton built-in formatter with intelligent type-specific formatting


### Formatter Hierarchy

```
IFormatter (non-generic, public)
	│
	├── DefaultFormatter (built-in, 12+ type patterns)
	│
	└── Formatter<T> (abstract base class)
			│
			└── [Your Custom Formatters]
```

### Formatting Pipeline
![Portamical_Core_Formatting_FormatterSelection](https://raw.githubusercontent.com/CsabaDu/Portamical/refs/heads/dev/_Images/Portamical_Core_Formatting_FormatterSelection.svg)


### Zero-Allocation String Building

```csharp
// Traditional approach (2 allocations)
string result = definition + " => " + formatted;

// Portamical approach (0 allocations)
string result = Builder.CreateSeparatedString(
	baseString: definition,
	separator: " => ",
	appendix: formatted);

// Uses string.Create<T> with Span<char> internally
```

---

## Performance

### Benchmarks (compared to traditional string concatenation)

| Scenario | Traditional | Portamical | Reduction |
|----------|-------------|------------|-----------|
| 2-item tuple | 3 allocations | 1 allocation | **66%** |
| 3-item list | 4 allocations | 1 allocation | **75%** |
| Key-value pair | 5 allocations | 1 allocation | **80%** |
| Quoted string | 2 allocations | 1 allocation | **50%** |

### Why It Matters

- **Reduced GC pressure** - Fewer allocations mean less garbage collection overhead
- **Better throughput** - Hot paths execute faster without intermediate string allocations
- **Scalability** - Performance advantage grows with test suite size (10,000+ test cases)

---

## Thread Safety

All public APIs are thread-safe for concurrent use:

- **`Formatter`** - Uses `ConcurrentDictionary<Type, IFormatter>` for lock-free reads/writes
- **`DefaultFormatter`** - Stateless singleton, safe for parallel test execution
- **Custom formatters** - Should be implemented as stateless or use appropriate synchronization

```csharp
// Safe to call from multiple threads simultaneously
Parallel.For(0, 1000, i =>
{
	var result = Formatter.Format(new MyType { Id = i });
});

// Safe to register formatters from multiple threads
Parallel.Invoke(
	() => Formatter.RegisterFormatter<TypeA>(new FormatterA()),
	() => Formatter.RegisterFormatter<TypeB>(new FormatterB())
);
```

---

## Integration with Portamical.Core

`Portamical.Core` uses this package for test case name generation:

```csharp
// Portamical.Core internally uses Formatter.Format
var testData = CreateTestDataReturns(
	definition: "Calculate tax",
	expected: 19.99m,
	arg1: 99.95m,
	arg2: 0.20m);

// TestCaseName generated: "Calculate tax => returns 19.99"
// Uses Formatter.Format(expected) internally
```

Custom formatters registered in `Formatter` are automatically used by Portamical test case generation.

---

## API Reference

### Formatter Methods

| Method | Type | Description |
|--------|------|-------------|
| `Format<T>(T)` | `string` | Format a value using registered or default formatter |
| `RegisterFormatter<T>(IFormatter)` | `bool`| Register a custom formatter for type `T` |
| `RegisterFormatter(Type, IFormatter)` | `bool`| Register a custom formatter for a type |
| `UnregisterFormatter<T>()` | `bool`| Remove the formatter for type `T` |
| `UnregisterFormatter(Type)` | `bool`| Remove the formatter for a type |
| `IsFormatterRegistered<T>()` | `bool`| Check if a formatter is registered for `T` |
| `IsFormatterRegistered(Type)` | `bool`| Check if a formatter is registered for a type |
| `GetFormatter<T>()` | `IFormatter`| Get the formatter for `T` (custom or default) |
| `GetFormatter(Type)` | `IFormatter`| Get the formatter for a type (custom or default) |
| `ClearFormatters()` | (void) | Remove all custom formatters |

### Builder Methods

| Method | Type | Description |
|--------|------|-------------|
| `CreateSeparatedString(string, string, string)` | `string` | Zero-allocation three-part string assembly |
| `JoinWithComma(IEnumerable<string?>)` | `string` | Join items with `", "` separator (optimized for 0-3 items) |
| `JoinWithSeparator(IEnumerable<string?>, string)` | `string` | Join items with custom separator |
| `CopyAsSpan(string, Span<char>, int)` | (void) | Copy string to span at index (inlined) |
| `FallbackIfNull(string?)` | `string` | Convert `null` to `"null"` (inlined) |
| `FallbackIfNullSeparator(string?)` | `string` | Convert `null` separator to `", "` (inlined) |

### DefaultFormatter Methods

| Method | Type | Description |
|--------|------|-------------|
| `Format(object?)` | `string?` | Formats an object into a predefined human-readable string representation, or returns `null` |

### Static Properties

| Property | Type | Description |
|----------|------|-------------|
| `Formatter.Registry` | `IReadOnlyDictionary<Type, IFormatter>` | The registered custom formatter map |
| `DefaultFormatter.Instance` | `IFormatter`| The singleton instance of `DefaultFormatter` |

### Constants

| Constant | Type | Value | Description |
|----------|------|-------|-------------|
| `Builder.MaxCount` | `int` | `3` | Max items shown in collections |
| `Builder.NullString` | `string` | `"null"` | String representation of `null` |

### Abstract Methods

| Method | Type | Description |
|--------|------|-------------|
| `Formatter<T>.Format(T)` | `string` | Abstract method to implement custom formatting logic for type `T` |

---

## Best Practices

### DO

- **Implement formatters as stateless** - Enables thread-safe concurrent use
- **Use `Formatter<T>` base class** - Provides type-safe implementation with automatic type checking
- **Keep formatted output concise** - Aim for < 50 characters for test case names
- **Handle null values explicitly** - Return `"null"` for null inputs
- **Register formatters at startup** - Avoid runtime registration in hot paths

### DON'T

- **Don't use reflection in formatters** - Impacts performance on hot paths
- **Don't throw exceptions** - Return `null` for unsupported types instead
- **Don't block in formatters** - Avoid I/O, database calls, or network operations
- **Don't maintain mutable state** - Can cause race conditions in parallel tests
- **Don't allocate unnecessarily** - Use span-based helpers when building strings

---

## Examples

### Format Complex Types

```csharp
// Format exceptions
var ex = new ArgumentException("Value cannot be null");
var result = Formatter.Format(ex);
// "ArgumentException: Value cannot be null"

// Format types with C#-friendly names
var result1 = Formatter.Format(typeof(int));          // "int"
var result2 = Formatter.Format(typeof(List<string>)); // "List<string>"
var result3 = Formatter.Format(typeof(int?));         // "int?"
var result4 = Formatter.Format(typeof(int[]));        // "int[]"

// Format delegates
Func<int, string> func = x => x.ToString();
var result5 = Formatter.Format(func);  // "Func<int, string> (anonymous)"

Action<string> action = Console.WriteLine;
var result6 = Formatter.Format(action);  // "Action<string> (WriteLine)"
```

### Format Collections

```csharp
// Arrays
var array = new[] { 1, 2, 3 };
var result1 = Formatter.Format(array);  // "[3]: [1, 2, 3]"

// Large collections (truncated)
var largeArray = new[] { 1, 2, 3, 4, 5, 6 };
var result2 = Formatter.Format(largeArray);  // "[First 3 of 3+]: [1, 2, 3]"

// Dictionaries
var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
var result3 = Formatter.Format(dict);  // "[2]: {{"a": 1}, {"b": 2}}"
```

### Custom Formatter with Base Class Utilities

```csharp
public sealed class RangeFormatter : Formatter<Range>
{
	public override string Format(Range value)
	{
		if (value is null)
			return Builder.FallbackIfNull(null);  // Use Builder helper

		// Use Builder.JoinWithComma for consistent formatting
		var parts = new[] { value.Start.ToString(), value.End.ToString() };
		return $"[{Builder.JoinWithComma(parts)}]";
	}
}
```

---

## Contributing

Contributions are welcome! Please see the main [Portamical repository](https://github.com/CsabaDu/Portamical) for contribution guidelines.

---

## License

This project is licensed under the MIT License - see the [LICENSE.txt](../LICENSE.txt) file for details.

---

## Related Projects

- **[Portamical.Core](https://github.com/CsabaDu/Portamical/tree/master/Portamical.Core)** - Core test data framework (uses this package)
- **[Portamical](https://github.com/CsabaDu/Portamical/tree/master/Portamical)** - Shared utilities and base classes (uses Core)
- **[Portamical.xUnit](https://github.com/CsabaDu/Portamical/tree/master/Portamical.xUnit)** - xUnit v2 adapter (uses shared and test framework modules)
- **[Portamical.xUnit_v3](https://github.com/CsabaDu/Portamical/tree/master/Portamical.xUnit_v3)** - xUnit v3 adapter (uses shared and test framework modules)
- **[Portamical.MSTest](https://github.com/CsabaDu/Portamical/tree/master/Portamical.MSTest)** - MSTest 4 adapter (uses shared and test framework modules)
- **[Portamical.NUnit](https://github.com/CsabaDu/Portamical/tree/master/Portamical.NUnit)** - NUnit 4 adapter (uses shared and test framework modules)
- **[Portamical.TUnit](https://github.com/CsabaDu/Portamical/tree/master/Portamical.TUnit)** ***(Preview)*** - TUnit adapter (uses shared and test framework modules)

---

## Support

- **Documentation**: [GitHub Wiki](https://github.com/CsabaDu/Portamical/wiki)
- **Issues**: [GitHub Issues](https://github.com/CsabaDu/Portamical/issues)
- **Discussions**: [GitHub Discussions](https://github.com/CsabaDu/Portamical/discussions)

---

## Changelog

### **Version 2.0.0 - Current** (2026-06-27)

**Architecture Simplification & API Refinement**

**WHAT'S NEW:**  
- **Simplified formatter architecture** - 2 types instead of 3
- **Single inheritance model** via `Formatter<T>` base class
- **Flat namespace structure** for better discoverability
- **Configurable `maxCount` parameter** for Builder join methods
- **Enhanced XML documentation** (60+ fixes and clarifications)

**BREAKING CHANGES:**  
- Removed generic `IFormatter<T>` interface (replaced by `Formatter<T>` base class)
  - All custom formatters must now inherit from abstract `Formatter<T>` base class
  - Eliminates interface segregation - single inheritance model
  - **Migration:** Change `IFormatter<T>` → `Formatter<T>` and inherit instead of implement
- Deleted CustomFormatters namespace and folder structure
  - All formatter types now in root `Portamical.Core.Formatting` namespace

**ARCHITECTURE IMPROVEMENTS:**  
- Simplified formatter type hierarchy
  - Before: IFormatter (non-generic) + IFormatter<in T> interface + Formatter<T> base class
  - After: IFormatter (non-generic) + Formatter<T> abstract base class
  - Reduced complexity: 3 types → 2 types
- Consolidated namespace structure
  - All formatting types in single `Portamical.Core.Formatting` namespace
  - Removed nested CustomFormatters/Model folder hierarchy
- Enhanced `Formatter<T>` base class
  - Sealed `IFormatter.Format(object?)` implementation ensures consistent type checking
  - Simplified implementation: override single `Format(T)` method

**BUILDER ENHANCEMENTS:**  
- Added **configurable `maxCount` parameter** to join methods
  - `JoinWithComma(items, maxCount)` - default 3, configurable for different use cases
  - `JoinWithSeparator(items, separator, maxCount)` - flexible truncation control
  - Allows callers to control collection/tuple truncation threshold
- Updated `DefaultFormatter.Format(ITuple)` to use `maxCount: 8`
  - Tuples support up to 8 elements natively before nesting
  - Ensures complete tuple formatting without truncation

**DOCUMENTATION OVERHAUL:**  
- Complete XML documentation review and correction (60+ fixes)
- Standardized all inline code tags and cref attributes
- Enhanced API documentation for IFormatter, Formatter<T>, Builder, DefaultFormatter
  - **Builder:** Documented configurable `maxCount` parameter behavior
  - **DefaultFormatter:** Explained `maxCount: 8` rationale for tuple formatting
- Updated all code examples to use new architecture
- Fixed cref attributes (e.g., `JoinWithComma` method signature correction)

**CODE QUALITY:**  
- Removed contravariance complexity
- Comprehensive test coverage for new features
- All tests passing with simplified architecture

**BENEFITS:**  
- ✅ Simpler API surface: Fewer types to learn and understand
- ✅ Single inheritance model: Clear extension point via `Formatter<T>`
- ✅ Flat namespace: Better IDE auto-completion and discoverability
- ✅ Flexible truncation: Configurable `maxCount` for various use cases
- ✅ Better documentation: Corrected and standardized across all files
- ✅ Maintained performance: Zero-allocation formatting still intact

---

### **Version 1.0.0** (2026-06-26)

**Initial Release**

- **Core Features:**  
  - `IFormatter` and `IFormatter<T>` interfaces for custom formatter implementations  
  - `Formatter<T>` abstract base class with Template Method pattern  
  - `DefaultFormatter` with specialized formatting for 12+ .NET types 
    - Strings (quoted), chars (single-quoted), dates (ISO 8601)  
    - Collections, tuples, dictionaries (first 3 items with truncation)  
    - Exceptions, Types (C#-friendly names), Delegates, KeyValuePair  
    - Byte arrays (hex), Guid, Stream metadata  
  - `Formatter` (non-generic, static) for thread-safe custom formatter registration  
    - `ConcurrentDictionary`-based registry for lock-free reads/writes  
    - `RegisterFormatter`, `UnregisterFormatter`, `GetFormatter`, `Format` methods  
    - Custom formatters consulted before built-in pattern matching  
  - Builder utilities for zero-allocation string construction  
    - `CreateSeparatedString`: Span-based three-part string assembly  
    - `JoinWithComma`/`JoinWithSeparator`: Optimized for 0-3 item collections  
    - `CopyAsSpan`: Inline helper for efficient character copying  
    - `FallbackIfNull`: Consistent null-to-"null" conversion  

- **Performance Optimizations:**  
  - Zero-allocation string building using `string.Create()` with `Span<char>`  
    - 66-75% fewer allocations for common formatting scenarios  
    - Direct span writes for quoted strings, tuples, key-value pairs  
    - Allocation-free delegate and type name formatting  
  - Aggressive inlining for hot-path helpers (`CopyAsSpan`, `FallbackIfNull`)  
  - Optimized pattern matching with method overloading for type dispatch  

- **Documentation:**  
  - Comprehensive XML documentation for all public APIs  
    - Detailed type-specific formatting tables  
    - Formatter registration examples and patterns  
    - Performance characteristics and thread-safety notes  
    - Extensive code samples for all scenarios  
  - README with feature overview, quick start, and extensibility guide  

- **Thread Safety:**  
  - All public APIs are thread-safe for concurrent use  
  - `Formatter.Registry` uses `ConcurrentDictionary` for lock-free operations  
  - `DefaultFormatter` is stateless and safe for parallel test execution

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

