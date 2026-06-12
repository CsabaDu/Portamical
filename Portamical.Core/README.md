# Portamical.Core

**Framework-agnostic foundation of Portamical**: Universal, identity-driven test data modeling for .NET 10.

Define test data **once** and consume it across test frameworks using adapter packages - without rewriting the data or sacrificing strong typing.

---

## What's New in v3.3.0

**Extensibility, Performance & Documentation Release**

This version introduces a powerful extensibility model through custom formatters, significant performance improvements via span-based optimizations, and comprehensive documentation enhancements across the test data type hierarchy.

### Breaking Changes

1. **Formatter Architecture Refactoring**
   - **Extracted:** Shared formatting logic from `ValueFormatter` into new `Formatter` base class
   - **Moved:** Helper methods (`FallbackIfNull`, `JoinWithComma`) to `Formatter` base class
   - **Impact:** Code using `using static Portamical.Core.Formatting.ValueFormatter;` may need updating
   - **Migration:** Change to `using static Portamical.Core.Formatting.Model.Formatter;` for helper access

### New Features

1. **Custom Formatter Registry**
   ```csharp
   // Register custom formatter for your domain types
   public class ProductIdFormatter : IFormatter<ProductId>
   {
       public string Format(ProductId value) => $"PROD-{value.Id:D6}";
   }

   // Register it globally
   ValueFormatter.Registry[typeof(ProductId)] = new ProductIdFormatter();

   // Now all test cases format ProductId automatically
   var test = CreateTestDataReturns(
       definition: "Get product by ID",
       expected: productResult,
       arg1: new ProductId(42));
   // TestCaseName: "Get product by ID => returns PROD-000042" ?
   ```

2. **IFormatter Interface & Formatter Base Class**
   - **`IFormatter<T>`**: Type-safe extensibility contract for custom formatters
   - **`Formatter`**: Shared utilities for all formatters
     - `FallbackIfNull(string?)`: Null-to-"null" conversion
     - `JoinWithComma(IEnumerable<string?>)`: Optimized comma-separated list building
     - `CreateSeparatedString(string, string, string)`: Zero-allocation concatenation
     - Span-based helpers for high-performance string operations

3. **Delegate Formatting**
   ```csharp
   // Distinguishes named methods from anonymous lambdas
   Func<int, string> lambda = x => x.ToString();
   // Formats as: "Func<int, string> (anonymous)"

   Action<string> method = Console.WriteLine;
   // Formats as: "Action<string> (WriteLine)"
   ```

### Performance Improvements

- **Span-Based String Building**
  - `Formatter.JoinWithComma()`: 66-75% fewer allocations for 2-3 item collections
  - `ValueFormatter.Format()`: Zero-copy operations throughout hot paths
  - `string.Create<TState>()`: Eliminates intermediate allocations
  - Particularly beneficial in test case name generation (tuples, type arguments, small collections)

### Enhanced Documentation

- **ValueFormatter**: Comprehensive XML documentation with formatting table for 12+ types
- **TestDataExpected, TestDataReturns, TestDataThrows**: Enhanced XML comments with detailed examples
- **IFormatter & Formatter**: Complete API documentation
- **T4 Templates**: All templates now include `#nullable enable` directives
  - `TestDataExpected.tt` ? **NEW**
  - `TestDataReturns.tt`
  - `TestDataThrows.tt`

### Migration from v3.2.x

```csharp
// Before (v3.2.x)
using static Portamical.Core.Formatting.ValueFormatter;

// After (v3.3.0)
using static Portamical.Core.Formatting.Model.Formatter;
```

All existing `ValueFormatter.Format()` calls work unchanged. Performance improvements apply automatically.

---

<details>
<summary><strong>What's New in v3.2.0</strong> (Click to expand)</summary>

## What's New in v3.2.0

🚨 **Breaking Architectural Improvements + Intelligent Formatting**

This version introduces significant enhancements to the test data type hierarchy and adds intelligent value formatting for readable test case names.

### Breaking Changes

1. **TestDataReturns Type Constraint Relaxation**
   - **Changed:** `TestDataReturns<TStruct>` → `TestDataReturns<TResult>` with `notnull` constraint
   - **Now Supports:** Both value types AND reference types (strings, collections, custom classes)
   - **Impact:** Type parameter renamed from `TStruct` to `TResult`

### New Features

1. **Reference Type Support in TestDataReturns**
   ```csharp
   // ✅ NEW in v3.2.0 - Reference types now supported
   CreateTestDataReturns(
       definition: "Get user name",
       expected: "John Doe",  // String
       arg1: 123);

   CreateTestDataReturns(
       definition: "Get items",
       expected: new List<int> { 1, 2, 3 },  // Collection
       arg1: userId);
   
   // ✅ Still supported - Value types
   CreateTestDataReturns(
       definition: "Add numbers",
       expected: 5,  // int
       arg1: 2,
       arg2: 3);
   ```

2. **Intelligent Value Formatting (ValueFormatter)**
   - **NEW:** `Portamical.Core.Formatting.ValueFormatter` class for type-specific formatting
   - **Automatic formatting** for char, string, DateTime, Guid, collections, exceptions, tuples, and more
   - **Readable test case names** without manual string conversion  

   **Supported Formats:**  

   | Type | Format Example |
   |------|----------------|
   | `char` | `'a'` (single-quoted) |
   | `string` | `"hello"` (double-quoted) |
   | `DateTime` | `2026-01-15T10:30:00.0000000Z` (ISO 8601) |
   | `Guid` | `12345678-1234-1234-1234-123456789012` (hyphenated) |
   | `byte[]` | `01-02-03-FF` (hex string) |
   | `Exception` | `ArgumentException: Value cannot be null` |
   | `Type` | `int`, `List<string>`, `int?`, `int[]` (C# aliases) |
   | `IEnumerable` | `[3]: [1, 2, 3]` (first 3 items with count) |
   | `IDictionary` | `[2]: {{"a": 1}, {"b": 2}}` (key-value pairs) |
   | `KeyValuePair` | `{"key": "value"}` |
   | `Tuple`/`ValueTuple` | `(1, "test", true)` (parenthesized) |
   | `Stream` | `MemoryStream (Length: 1024, Position: 0)` |

3. **Enhanced Code Generation**
   - All T4-generated files now include `#nullable enable` directives
   - Improved XML documentation completeness
   - Added missing `<returns>` tags in factory methods
   - **NEW:** `TestDataExpected.tt` template for generating `TestDataExpected<TResult, T1...T9>` classes
     - Completes the T4 template trio alongside `TestDataReturns.tt` and `TestDataThrows.tt`
     - Generates 9 generic variants (1-9 type parameters) for flexible test data composition
     - Integrated into verification scripts (`verify-generated.ps1` and `verify-generated.sh`)

</details>

---

## Install

```bash
dotnet add package Portamical.Core
```

> You will typically also install a Portamical adapter package for your test framework (xUnit / MSTest / NUnit).

---

## Quick Example

### General Test Data

```csharp
using static Portamical.Core.Factories.TestDataFactory;

public sealed class CalculatorCases
{
    public static IEnumerable<TestData<int, int>> Add()
    {
        yield return CreateTestData(
            definition: "adding two positive numbers",
            result: "returns their sum",
            arg1: 2,
            arg2: 3);

        yield return CreateTestData(
            definition: "adding with zero",
            result: "returns the other number",
            arg1: 0,
            arg2: 5);
    }
}
```

### Return Value Test Data (v3.2.0 - Reference Type Support + Intelligent Formatting)

```csharp
public sealed class UserServiceCases
{
    public static IEnumerable<TestDataReturns<string, int>> GetUserName()
    {
        // ✅ NEW: String return values with automatic quote formatting
        yield return CreateTestDataReturns(
            definition: "valid user ID",
            expected: "John Doe",
            arg1: 123);
        // TestCaseName: "valid user ID => returns \"John Doe\"" ✅

        yield return CreateTestDataReturns(
            definition: "admin user",
            expected: "Administrator",
            arg1: 1);
        // TestCaseName: "admin user => returns \"Administrator\"" ✅
    }

    public static IEnumerable<TestDataReturns<List<int>, int>> GetNumbers()
    {
        // ✅ NEW: Collection return values with count and item display
        yield return CreateTestDataReturns(
            definition: "department numbers",
            expected: new List<int> { 1, 2, 3 },
            arg1: 5);
        // TestCaseName: "department numbers => returns [3]: [1, 2, 3]" ✅
    }

    public static IEnumerable<TestDataReturns<DateTime, string>> GetTimestamp()
    {
        // ✅ DateTime formatted as ISO 8601
        yield return CreateTestDataReturns(
            definition: "current timestamp",
            expected: new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            arg1: "now");
        // TestCaseName: "current timestamp => returns 2026-01-15T10:30:00.0000000Z" ✅
    }
}
```

### Exception Test Data (v3.2.0 - Enhanced Exception Formatting)

```csharp
public sealed class ValidationCases
{
    public static IEnumerable<TestDataThrows<ArgumentNullException, string>> InvalidArgs()
    {
        yield return CreateTestDataThrows(
            definition: "null name",
            expected: new ArgumentNullException("name", "Value cannot be null"),
            arg1: null);
        // TestCaseName: "null name => throws ArgumentNullException: Value cannot be null (Parameter 'name')" ✅
    }

    public static IEnumerable<TestDataThrows<InvalidOperationException>> ClosedState()
    {
        yield return CreateTestDataThrows(
            definition: "operation when closed",
            expected: new InvalidOperationException("Cannot perform operation on closed object"),
            arg1: null);
        // TestCaseName: "operation when closed => throws InvalidOperationException: Cannot perform operation on closed object" ✅
    }
}
```

---

## Architecture

### Namespace Organization

```
Portamical.Core/
├── Factories/              # Factory methods for test data creation
├── Formatting/             # ⭐ NEW: Value formatting utilities
│   ├── IFormatter.cs       # Extensibility contract for custom formatters
│   ├── Model/Formatter.cs  # Abstract base with shared formatting utilities
│   └── ValueFormatter.cs   # Intelligent type-specific formatting
├── Identity/               # Test case identity and equality contracts
│   ├── INamedCase.cs       # Core interface for test case naming and equality
│   └── Model/NamedCase     # Abstract base with equality comparer and display name creation (NamedCase)
├── Safety/                 # Validation utilities (Validator, Resolver)
├── Strategy/               # Strategy enums (ArgsCode, PropsCode)
└── TestDataTypes/          # Core test data domain
    ├── ITestData.cs        # Base test data contract
    ├── Models/             # Concrete implementations
    │   ├── General/        # TestData
    │   └── Specialized/    # TestDataReturns, TestDataThrows, TestDataExpected
    └── Patterns/           # Domain-specific marker interfaces
        ├── IExpected.cs    # Base for tests with expected outcomes
        ├── IReturns.cs     # Marker for return value tests
        └── IThrows.cs      # Marker for exception tests
```

### Four-Layer Model

| Layer | Role | Example Types |
|-------|------|--------------|
| **Identity** | Test case naming, equality & deduplication | `ITestData` |
| **Core Abstraction** | Universal access across all test types | `INamedCase`, `NamedCase` |
| **Pattern Markers** | Intent discovery, pattern matching & compile-time type safety | Marker interfaces: `IExpected`, `IReturns`, `IThrows`; Generic constraints: `IExpected<TResult>`, `IReturns<TResult>`, `IThrows<TException>` |
| **Specializations** | Type-safe operations with context | `TestData<T1...T9>`, `TestDataReturns<TResult, T1...T9>`, `TestDataThrows<TException, T1...T9>` |

---

## Test Data Types (v3.2.0)

### Updated Type Hierarchy

```
TestDataBase (abstract)
├── TestData<T1...T9>                    # General test data
└── TestDataExpected<TResult> (abstract) # Base for expected outcomes
    ├── TestDataReturns<TResult>         # ⭐ notnull constraint (was struct)
    │   └── TestDataReturns<TResult, T1...T9>   # Supports value AND reference types
    └── TestDataThrows<TException>
        └── TestDataThrows<TException, T1...T9>
```

### Factory Methods

All test data types are created via T4-generated factory methods:

```csharp
// General test data
CreateTestData<T1, T2>(definition, result, arg1, arg2)

// Return value test data (⭐ now supports reference types)
CreateTestDataReturns<TResult, T1>(definition, expected, arg1)

// Exception test data
CreateTestDataThrows<TException, T1>(definition, expected, arg1)
```

---

## Migration Guide (v2.2.0 → v3.2.0)

### Step 1: Update TestDataReturns Type Parameters (Optional)

```csharp
// v2.2.0 (Old naming)
TestDataReturns<TStruct, int>  // Still compiles, but TStruct is legacy

// v3.2.0 (Recommended naming)
TestDataReturns<TResult, int>  // Clearer intent
```

### Step 2: Leverage Reference Type Support

```csharp
// v2.2.0 ❌ Compile error
// TestDataReturns<string, int>  // Error: string is not a struct

// v3.2.0 ✅ Now works!
var testData = CreateTestDataReturns(
    definition: "Get username",
    expected: "Alice",
    arg1: 42);
// TestCaseName: "Get username => returns \"Alice\"" ✅ Automatically formatted!
```

### Step 3: Benefit from Automatic Formatting

```csharp
// ✅ No ToString() override needed for common types
CreateTestDataReturns(
    definition: "Get timestamp",
    expected: DateTime.UtcNow,  // Automatic ISO 8601 formatting
    arg1: userId);

CreateTestDataReturns(
    definition: "Get items",
    expected: new List<int> { 1, 2, 3 },  // Automatic "[3]: [1, 2, 3]" formatting
    arg1: query);

// ⚠️ For custom types, override ToString() for best results
public class User
{
    public string Name { get; init; }
    public override string ToString() => Name;
}

var userData = CreateTestDataReturns(
    definition: "Get user",
    expected: new User { Name = "Bob" },
    arg1: userId);
// TestCaseName: "Get user => returns Bob" ✅
```

### Step 4: Verify Breaking Changes

- ✅ Type parameter updated: `TStruct` → `TResult` (if referencing explicitly)
- ✅ Reference types now supported in `TestDataReturns`
- ✅ Automatic formatting applies to all expected values
- ✅ Tests rebuild and pass

---

## Core Concepts

### Identity System

Every test case has a deterministic `TestCaseName`:

```
"{scenario description} => {expected outcome}"
```

**Examples (v3.2.0 with automatic formatting):**
- `"Add(2,3) => returns 5"`
- `"Validate(null) => throws ArgumentException: Value cannot be null"`
- `"Get user name => returns \"John Doe\""` ← Reference type with quotes
- `"Get timestamp => returns 2026-01-15T10:30:00.0000000Z"` ← ISO 8601 formatting
- `"Get numbers => returns [3]: [1, 2, 3]"` ← Collection with count

### Strategy Pattern

Control test data serialization:

- **`ArgsCode.Instance`** - Pass entire test data object
- **`ArgsCode.Properties`** - Pass individual property values

Combined with **`PropsCode`**:
- `PropsCode.All` - Include all properties
- `PropsCode.TrimTestCaseName` - Exclude `TestCaseName` (default)
- `PropsCode.TrimReturnsExpected` - Also exclude `Expected` (for `IReturns`)
- `PropsCode.TrimThrowsExpected` - Also exclude `Expected` (for `IThrows`)

---

## Design Rationale

### Why `notnull` Instead of `struct`?

| Aspect | `struct` (v2.2.0) | `notnull` (v3.2.0) |
|--------|-------------------|-------------------|
| **Supported Types** | Value types only | Value + reference types |
| **Real-world APIs** | ⚠️Limited | ✅ Matches actual method signatures |
| **Type Safety** | ✅ Non-null guaranteed | ✅ Non-null guaranteed |
| **API Complexity** | ❌ Multiple specialized types needed | ✅ Unified API |
| **Formatting** | ✅ Always meaningful (ToString) | ✅ Automatic intelligent formatting (ValueFormatter) |

### Why ValueFormatter?

**Before (v2.2.0):**
```csharp
// Relied on ToString() - inconsistent across types
Expected = "John Doe"  // TestCase name: "... => returns John Doe" ❌ No quotes
Expected = DateTime.Now  // TestCase name: "... => returns 1/15/2026 10:30:00 AM" ❌ Ambiguous
Expected = new List<int> { 1, 2 }  // TestCase name: "... => returns System.Collections.Generic.List`1[System.Int32]" ❌ Unreadable
```

**After (v3.2.0):**
```csharp
// Intelligent type-specific formatting
Expected = "John Doe"  // TestCase name: "... => returns \"John Doe\"" ✅ Quoted
Expected = DateTime.Now  // TestCase name: "... => returns 2026-01-15T10:30:00.0000000Z" ✅ ISO 8601
Expected = new List<int> { 1, 2 }  // TestCase name: "... => returns [2]: [1, 2]" ✅ Readable
```

**Key Benefits:**
- ✅ **Consistency** - Same formatting rules across all test data
- ✅ **Readability** - Human-friendly output for test runners
- ✅ **C# alignment** - Matches C# literal syntax (char `'a'`, string `"text"`, etc.)
- ✅ **Extensibility** - Pattern-matching dispatch enables new formatters

---

## T4 Code Generation

All generic variants are T4-generated from a single source:

```
Portamical.Core/
├── T4/SharedHelpers.ttinclude     # MaxArity = 9
├── Factories/
│   ├── TestDataFactory.tt
│   └── TestDataFactory.generated.cs
└── TestDataTypes/Models/
    ├── General/
    │   ├── TestData.tt
    │   └── TestData.generated.cs
    └── Specialized/
        ├── TestDataReturns.tt
        ├── TestDataReturns.generated.cs
        ├── TestDataThrows.tt
        └── TestDataThrows.generated.cs
```

### Regenerate T4 Templates

```bash
# 1. Edit MaxArity in SharedHelpers.ttinclude
# 2. In Visual Studio: Right-click .tt files → Run Custom Tool
# 3. Build
dotnet build
```

---

## Performance (v2.2.0 Baseline)

| Operation | Before | After | Speedup |
|-----------|--------|-------|---------|
| NotNull validation | ~10 cycles | ~2 cycles | 5x |
| NotNullOrEmpty | ~15 cycles | ~4 cycles | 3.75x |
| Enum validation | ~12 cycles | ~3 cycles | 4x |
| ToString() | ~10 cycles | ~2 cycles | 5x |
| Constructor (3 validations) | ~30 cycles | ~6 cycles | 5x |

**Real-world impact:** Creating 1000 test data objects - reduced from ~35-50 μs to ~7-10 μs.

---

## Links

- **GitHub:** https://github.com/CsabaDu/Portamical
- **Documentation:** https://github.com/CsabaDu/Portamical/blob/master/README.md
- **Issues:** https://github.com/CsabaDu/Portamical/issues

---

## License and Project Lineage

This project is licensed under the [MIT License](https://github.com/CsabaDu/Portamical/blob/master/LICENSE.txt).

**Portamical.Core** is the successor to **CsabaDu.DynamicTestData.Core** (legacy, no longer supported).

### Key Improvements Over Legacy

| Aspect | CsabaDu.DynamicTestData.Core | Portamical.Core |
|--------|------------------------------|-----------------|
| Data Model | Record-based | Immutable classes |
| Identity | Basic equality | Value Object pattern |
| Type Support | Value types only | ✅ Value + reference types (v3.2.0) |
| Formatting | ToString() only | ✅ Intelligent ValueFormatter (v3.2.0) |
| Performance | Baseline | 5x faster (v2.2.0) |
| Documentation | Minimal | Comprehensive XML docs |

---

## Changelog

### **Version 3.2.0** (2026-06-05)

**Breaking Changes:**
- Changed `TestDataReturns<TStruct>` → `TestDataReturns<TResult>` with `notnull` constraint
- Type parameter rename across generated code

**New Features:**
- Reference type support in `TestDataReturns` (strings, collections, custom classes)
- **NEW:** `Portamical.Core.Formatting.ValueFormatter` class for intelligent formatting
- Automatic type-specific formatting for 12+ common types (char, string, DateTime, Guid, collections, exceptions, tuples, etc.)
- All T4-generated files now include `#nullable enable` directives

**Improvements:**
- Enhanced XML documentation across TestDataExpected, TestDataReturns, TestDataThrows
- Improved test case name readability with automatic formatting
- Expanded test coverage for formatting scenarios

**Migration:**
- Type parameters: Update `TStruct` → `TResult` if referencing explicitly
- See [Migration Guide](#migration-guide-v220--v320)

---

##### **Version 3.2.1** (2026-06-06)

**Documentation:**
- Enhanced XML documentation for `ValueFormatter` class with comprehensive method-level docs
- Added performance notes explaining `AggressiveInlining` decisions for hot-path methods (Format(char), Format(string), FallbackIfNull, JoinWithComma)
- Documented design rationale for complex formatting methods (Type, Stream, Dictionary, ITuple, Exception)
- Updated `TestDataExpected.GetExpected()` docs with performance characteristics
- Added usage context and caller information across all formatter methods

**No breaking changes** - documentation-only improvements for better API understanding and maintainability.

---

##### **Version 3.2.2** (2026-06-08)

**Bug Fixes**
- **ValueFormatter.JoinWithComma**: Fixed incorrect return value for empty collections
- Empty collections now correctly return empty string (`""`) instead of `"null"`
- Distinguishes between empty collection and collection with single null item
- Aligns behavior with `string.Join(", ", items)` semantics

**No Breaking Changes**
This is a patch release that fixes a bug in internal formatting behavior. Existing code continues to work as expected, with improved correctness for edge cases.

**Impact**
- Tuple formatting: `Format(ValueTuple.Create())` now produces `"()"` instead of `"(null)"`
- Empty array formatting: `Format(Array.Empty<int>())` now produces `"[0]: []"` instead of `"[0]: [null]"`
- Generic type formatting with no type parameters (edge case) improved

---

#### **Version 3.3.0 - Current** (2026-06-13)

**Extensibility, Performance & Documentation Release**

This version introduces a powerful extensibility model, significant performance improvements through span-based optimizations, and comprehensive documentation enhancements across the test data type hierarchy.

**Breaking Changes**
- **Formatter Architecture Refactoring**
  - Extracted shared formatting logic from `ValueFormatter` into a new base class `Formatter`
  - Helper methods (`FallbackIfNull`, `JoinWithComma`) moved to `Formatter` base class
  - **Impact:** Code using `using static Portamical.Core.Formatting.ValueFormatter;` may need updating
  - **Migration:** Change to `using static Portamical.Core.Formatting.Model.Formatter;` for helper access

**Added**
- **Custom Formatter Registry**
  - `ValueFormatter.Registry`: Register custom `IFormatter` implementations for specific types
  - Registry-based lookup executes **before** built-in pattern matching
  - Enables domain-specific formatting without modifying core library

- **IFormatter Interface**
  - New extensibility contract for custom formatter implementations
  - Type-safe formatting abstraction
  - Supports both generic (`IFormatter<T>`) and non-generic (`IFormatter`) patterns

- **Formatter Base Class**
  - Shared utilities: `NullString`, `MaxCount`, `Separator` constants
  - `FallbackIfNull(string?)`: Null-to-"null" conversion
  - `JoinWithComma(IEnumerable<string?>)`: Optimized comma-separated list building
  - `CreateSeparatedString(string, string, string)`: Zero-allocation concatenation helper
  - `CopyAsSpan(string)`: Span-based string copying utility
  - Other span-based helpers for zero-allocation string operations

- **Delegate Formatting**
  - Formats `Func<>`, `Action<>`, `Predicate<>`, and custom delegate types
  - Distinguishes named methods from anonymous lambdas
  - Examples:
    - `Func<int, string> (anonymous)` for lambda expressions
    - `Action<string> (WriteLine)` for method references

**Performance**
- **Span-Based String Building**
  - `Formatter.JoinWithComma()`: 66-75% fewer allocations for 2-3 item collections using `string.Create<TState>()`
  - `ValueFormatter.Format(string)`: Direct span write for quoted strings
  - `ValueFormatter.Format(object?, object?)`: Zero-copy key-value pair formatting
  - `ValueFormatter.Format(Delegate)`: Allocation-free delegate name construction
  - `ValueFormatter.Format(Type)`: Span-based array/nullable/generic formatting
  - `Span<char>`-based construction eliminates intermediate allocations
  - Zero-allocation success paths preserved throughout hot paths
  - Particularly beneficial in test case name generation (tuples, type arguments, small collections)
  - `CreateSeparatedString`: Shared by ValueFormatter and TestDataBase for zero-copy concatenation

**Improved**
- Optimized pattern matching in ValueFormatter (removed redundant string check)
- Extracted `GetKvpPropValues` helper for KeyValuePair property access
- Simplified `TestDataBase.CreateTestCaseName()` using CreateSeparatedString helper
- Enhanced null-handling consistency across all formatters
- Reduced GC pressure in hot paths

**Documentation**
- **ValueFormatter**: Comprehensive XML documentation
  - Detailed type-specific formatting table for 12+ types
  - Formatter registration API fully documented
  - Performance characteristics and thread-safety notes
  - Extensive examples for all formatting scenarios
- **TestDataExpected**: Enhanced XML documentation
  - Comprehensive Format() method documentation with type table
  - GetResult() documentation with fallback strategy details
  - Null handling strategy explanation
  - Integration with Resolver.FallbackIfNullOrWhiteSpace documented
- **TestDataReturns**: Updated XML comments
  - Clarified base class formatting (not ToString)
  - Added references to Format() method
  - Improved examples with actual formatted output
  - Enhanced GetResultPrefix() and ToArgs() documentation
- **TestDataThrows**: Updated XML comments for consistency
  - Exception formatting via base class clarified
  - Fixed incorrect type reference (TStruct ? TResult)
  - Enhanced examples showing exception type and message
  - Improved GetResultPrefix() and ToArgs() documentation
- **IFormatter and Formatter**: Added comprehensive XML docs
- Created PERFORMANCE_STRING_CREATE_OPTIMIZATION.md with detailed benchmarks
- Updated all method docs to reflect "expectedType" terminology

**T4 Template Fixes**
- All T4 templates now include `#nullable enable` directives
  - `TestDataExpected.tt`, `TestDataReturns.tt`, `TestDataThrows.tt`
  - Ensures CS8669 compiler warnings are suppressed
  - Regenerated all `.generated.cs` files with proper nullable context

**Testing**
- Added 433 lines of tests for Formatter base class
- Added 217 lines of tests for custom formatter registry
- Added test parallelization control and cleanup for registry tests

#### Migration from v3.2.x
- **Using statements**: Change `using static Portamical.Core.Formatting.ValueFormatter;` to `using static Portamical.Core.Formatting.Model.Formatter;` (for FallbackIfNull, JoinWithComma access)
- **Custom formatters**: Implement `IFormatter<T>` and register in `ValueFormatter.Registry` (optional)
- **Existing code**: All existing `ValueFormatter.Format()` calls work unchanged
- **Performance improvements**: Apply automatically without code changes

---

### **Version 2.0.0** (2026-03-13)

**Note:** This version does not introduce breaking changes in `Portamical.Core` itself. The major version bump to 2.0.0 aligns with the Portamical extension packages, where new versions may introduce breaking changes.

**Added** - Comprehensive XML Documentation
- Extensive XML documentation comments across entire codebase (65 files updated)
- Documented design patterns (Template Method, Strategy, Adapter, Decorator, Factory Method)
- Detailed usage examples with code samples for all public APIs
- Architecture diagrams and inheritance chains in documentation

**Changed** - Core Infrastructure Improvements
- **NamedCase**:
  - Added `[SuppressMessage]` attributes with justifications for SonarLint rules
  - Improved `CreateTestCaseName()` performance using `string.Create` for zero-allocation concatenation
  - Enhanced `Equals` and `GetHashCode` implementations with detailed explanations
- **EnumValidator**:
  - Renamed `GetResultPrefix` ? `GetValidResultPrefix` for clarity
- **Resolver**:
  - Improved `FallbackIfNullOrWhiteSpace` with `string.Create` and `CultureInfo.InvariantCulture`
  - Changed `Trace.WriteLine` ? `Trace.TraceWarning` for better diagnostic categorization
  - Added thread-safety documentation for `ResetLogCounter` atomic operations
- **TestDataBase**:
  - Optimized `CreateTestCaseName()` using `string.Create` (zero-copy concatenation)

**Changed** - T4 Template Improvements
- **SharedHelpers.ttinclude**: Fixed encoding issue (BOM removed)
- Maintained single source of truth for `MaxArity = 9`

**Changed** - Build Configuration
- Updated version to 2.0.0
- Cleared `PackageReleaseNotes` (prepared for final release notes)
- Added `PackageOutputPath` configuration

**Fixed** - Code Quality Improvements
- Fixed typo: "Cmbines" ? "Combines" in `INamedCase`
- Removed unused "TrimTestCaseName" placeholders
- Enhanced null-safety compliance throughout
- Added `ArgumentOutOfRangeException` documentation where applicable
- Improved parameter name consistency (`paramName` validation)

**Documentation Themes**
- Design Patterns: Template Method, Strategy, Adapter, Decorator, Identity Object
- Performance: O(n) complexity notes, zero-allocation techniques, atomic operations
- Thread Safety: Immutability patterns, concurrent access documentation
- Framework Integration: xUnit v2/v3, NUnit 3/4, MSTest examples
- Migration Support: Comparisons with previous API versions

---

##### **Version 2.0.1** (2026-04-02)

**Changed**
- `NamedCase.CreateDisplayName(MethodInfo?, params object?[]?)` now validates that `args[0]` is `string` or `INamedCase` before delegating to the `string`-based overload; returns `null` early for non-matching types
- Moved type-check from call sites into core method for consistency
- Minor formatting fixes in `NamedCase` class declaration

---

#### **Version 2.2.0** (2026-04-23)

**Added**
- Performance optimizations via `MethodImpl(AggressiveInlining)` in 7 hot path methods:
  - `Validator.NotNull<T>` - 5x faster null validation (eliminates method call overhead)
  - `Validator.NotNullOrEmpty<T>` - 3-5x faster collection validation
  - `EnumValidator.Defined<TEnum>` - 4x faster enum validation
  - `Resolver.ResetLogCounter` - 5x faster atomic counter reset
  - `NamedCase.ToString()` - 3-5x faster string conversion (eliminates virtual dispatch)
  - `NamedCase` implicit string operator - 5x faster implicit conversions
  - `TestDataBase.ToArgs(ArgsCode)` - 2x faster wrapper method

**Changed**
- Internal performance improvements: critical methods now use aggressive inlining
- Enhanced XML documentation with performance notes for optimized methods

**Performance**
- Test data instantiation: ~5x faster (reduced overhead from ~35-50 ?s to ~7-10 ?s per 1000 objects)
- Constructor validation: Eliminates guard clause overhead (NotNull/NotNullOrEmpty inlined)
- String operations: Removes virtual dispatch and method call overhead
- Zero-allocation success paths maintained
- Negligible assembly size increase (<1 KB)

#### Migration
- Fully backward compatible - no code changes required
- Existing code automatically benefits from performance improvements upon upgrade
- No API changes, only performance enhancements

---

### **Version 1.0.0** (2026-03-04)

**Added**
- Initial release of Portamical.Core

---

##### **Version 1.0.1** (2026-03-06)

**Changed**
- `Portamical.Core.Safety.EnumValidator.Defined`: `string paramName` changed to non-nullable type
- README.md replaced with comprehensive documentation

---

##### **Version 1.0.2** (2026-03-07)

**Added**
- Link to migration guide (MIGRATION.md) in README.md

---

##### **Version 1.0.3** (2026-03-08)

**Changed**
- T4 Generated Code: All generated files (`TestDataFactory.generated.cs`, `TestData.generated.cs`, `TestDataReturns.generated.cs`, `TestDataThrows.generated.cs`) include explicit `#nullable enable` directives

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

---