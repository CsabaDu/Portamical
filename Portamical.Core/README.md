# Portamical.Core

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Version](https://img.shields.io/badge/version-4.0.0-orange.svg)](https://github.com/CsabaDu/Portamical/releases)

**Framework-agnostic foundation of Portamical**: Universal, identity-driven test data modeling for .NET 10.

Define test data **once** and consume it across test frameworks using adapter packages - without rewriting the data or sacrificing strong typing.

---

## What's New in v4.0.0

**Major Architectural Evolution: Standalone Formatting Library & API Refinement**

This major release extracts the formatting system into a standalone library (`Portamical.Core.Formatting`), refines the formatter API for better extensibility, and streamlines the core architecture.

### Breaking Changes

1. **Formatting Library Extracted**
   - **New Package:** `Portamical.Core.Formatting` - Standalone formatting library
   - **Impact:** Formatting utilities are now in a separate NuGet package (automatically included as dependency)
   - **Migration:** Update to `Portamical.Core` v4.0.0 - `Portamical.Core.Formatting` is included automatically

2. **Formatter API Refinement**
   - **Renamed:** `ValueFormatter` → `DefaultFormatter` (better intent clarity)
   - **Moved:** `IFormatter` and `IFormatter<T>` to `Portamical.Core.Formatting.CustomFormatters` namespace
   - **Changed:** `IFormatter<T>.Format(T)` return type: `string?` → `string` (non-nullable)
   - **New:** `Formatter<T>` abstract base class for easier custom formatter implementation
   - **Impact:** Code using `using static Portamical.Core.Formatting.ValueFormatter;` needs updating
   - **Migration:** 
     ```csharp
     // Before (v3.3.0)
     using static Portamical.Core.Formatting.ValueFormatter;
     using Portamical.Core.Formatting;
     
     // After (v4.0.0)
     using static Portamical.Core.Formatting.Builder;
     using Portamical.Core.Formatting.CustomFormatters;
     ```

3. **Formatter Registry API Changes**
   - **Removed:** `ValueFormatter.Registry`, `RegisterFormatter`, `UnregisterFormatter`, `IsFormatterRegistered`, `ClearFormatters`
   - **New:** `Formatter` static class with simplified registration API
   - **Migration:**
     ```csharp
     // Before (v3.3.0)
     ValueFormatter.Registry[typeof(ProductId)] = new ProductIdFormatter();
     ValueFormatter.RegisterFormatter<ProductId>(new ProductIdFormatter());
     ValueFormatter.UnregisterFormatter<ProductId>();
     
     // After (v4.0.0) - via Portamical.Core.Formatting package
     Formatter.RegisterFormatter<ProductId>(new ProductIdFormatter());
     Formatter.UnregisterFormatter<ProductId>();
     ```

### New Features

1. **Standalone Formatting Library (`Portamical.Core.Formatting`)**
   - **Separate Package:** Independently versioned and maintained (automatically referenced by Portamical.Core)
   - **Builder Utilities:** `Builder` class with span-based string building helpers
     - `FallbackIfNull(string?)` - Null-to-"null" conversion
     - `JoinWithComma(IEnumerable<string?>)` - Optimized joining
     - `JoinWithSeparator(IEnumerable<string?>)` - Flexible separator insertion
     - `CreateSeparatedString(string, string, string)` - Zero-allocation concatenation
     - `CopyAsSpan(string, Span<char>, int)` - Efficient span copying
   - **Singleton Pattern:** `DefaultFormatter.Instance` for reusable formatter access

2. **Improved Formatter Base Class**
   - **New:** `Formatter<T>` abstract base class simplifies custom formatter implementation
   - **Pattern:** Sealed `IFormatter.Format(object?)` with abstract `Format(T)` override
   - **Example:**
     ```csharp
     public sealed class ProductIdFormatter : Formatter<ProductId>
     {
         public override string Format(ProductId value)
         {
             return value is null ? NullString : $"PROD-{value.Id:D6}";
         }
     }
     ```

3. **Enhanced Documentation**
   - Comprehensive XML docs for all formatter infrastructure
   - Performance notes and design pattern explanations
   - Thread-safety guarantees documented

### Architecture Changes

```
Portamical.Core/                 (Core test data types)
├── Identity/
├── TestDataTypes/
├── Factories/
├── Safety/
└── Strategy/

Portamical.Core.Formatting/     (NEW - Standalone formatting library)
├── Formatter.cs                (Custom formatter registry, formatting pipeline)
├── Builder.cs                  (String building utilities)
├── DefaultFormatter.cs         (Singleton built-in formatter, 12+ type patterns)
└── CustomFormatters/
    ├── IFormatter.cs           (Formatter contracts)
    └── Model/
        └── Formatter.cs        (Abstract generic base class)
```

### Performance Improvements

- **Optimized Character Formatting:** Pre-cached ASCII printable characters (32-126)
- **Span-Based Operations:** Zero-allocation string building throughout
- **Singleton Pattern:** `DefaultFormatter.Instance` eliminates repeated instantiation

### Migration Guide (v3.3.0 → v4.0.0)

#### Step 1: Update Package Version

```bash
# Update to Portamical.Core v4.0.0 (automatically includes Portamical.Core.Formatting)
dotnet add package Portamical.Core --version 4.0.0
```

#### Step 2: Update Namespaces

```csharp
// Before (v3.3.0)
using static Portamical.Core.Formatting.ValueFormatter;
using Portamical.Core.Formatting;

// After (v4.0.0)
using static Portamical.Core.Formatting.Builder;  // For helper methods
using Portamical.Core.Formatting.CustomFormatters;  // For IFormatter
```

#### Step 3: Update Formatter Registration

```csharp
// Before (v3.3.0)
ValueFormatter.RegisterFormatter<ProductId>(new ProductIdFormatter());
var formatted = ValueFormatter.Format(productId);

// After (v4.0.0)
Formatter.RegisterFormatter<ProductId>(new ProductIdFormatter());
var formatted = DefaultFormatter.Format(productId);
// or
var formatted = DefaultFormatter.Instance.Format(productId);
```

#### Step 4: Update Custom Formatters

```csharp
// Before (v3.3.0)
public class ProductIdFormatter : IFormatter<ProductId>
{
    public string? Format(ProductId value) => ...;
    string? IFormatter.Format(object value) => value is ProductId p ? Format(p) : null;
}

// After (v4.0.0) - Easier with base class
public sealed class ProductIdFormatter : Formatter<ProductId>
{
    public override string Format(ProductId value) => ...;
    // IFormatter.Format(object) is handled by base class
}
```

#### Step 5: Verify Breaking Changes

- ✅ `Portamical.Core` v4.0.0 package installed (includes `Portamical.Core.Formatting` dependency)
- ✅ Namespace imports updated
- ✅ `ValueFormatter` → `DefaultFormatter`
- ✅ Custom formatter implementations updated
- ✅ Formatter registration calls updated
- ✅ Tests rebuild and pass

---

## Install

```bash
# Core test data library (automatically includes Portamical.Core.Formatting)
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

### Return Value Test Data (with Intelligent Formatting)

```csharp
public sealed class UserServiceCases
{
    public static IEnumerable<TestDataReturns<string, int>> GetUserName()
    {
        // ✅ String return values with automatic quote formatting
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
        // ✅ Collection return values with count and item display
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

### Custom Formatters (v4.0.0)

```csharp
using Portamical.Core.Formatting.CustomFormatters;
using Portamical.Core.Formatting.CustomFormatters.Model;

// Define a custom formatter using base class
public sealed class ProductIdFormatter : Formatter<ProductId>
{
    public override string Format(ProductId value)
    {
        if (value is null) return NullString;
        return $"PROD-{value.Id:D6}";
    }
}

// Register globally
Formatter.RegisterFormatter<ProductId>(new ProductIdFormatter());

// All test cases now use custom formatting
var test = CreateTestDataReturns(
    definition: "Get product by ID",
    expected: new ProductId(42),
    arg1: request);
// TestCaseName: "Get product by ID => returns PROD-000042" ✅
```

---

## Architecture

### Namespace Organization (v4.0.0)

```
Portamical.Core/
├── Factories/              # Factory methods for test data creation
├── Identity/               # Test case identity and equality contracts
│   ├── INamedCase.cs       # Core interface for test case naming and equality
│   └── Model/              # Abstract base with equality comparer and display name creation (NamedCase)
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

Portamical.Core.Formatting/ (NEW - Standalone package)
├── Formatter.cs            # Formatting utilities (custom formatter registry, formatting pipeline)
├── Builder.cs              # String building utilities (FallbackIfNull, JoinWithComma, CreateSeparatedString)
├── DefaultFormatter.cs     # Singleton built-in formatter with intelligent type-specific formatting
└── CustomFormatters/
    ├── IFormatter.cs       # Extensibility contracts for custom formatters
    └── Model/
        └── Formatter.cs    # Abstract generic base class + registry for custom formatters
```

### Four-Layer Test Data Model

|  Layer |  Role  | Example Types |
|------------|------------|-------|
| **Identity** | Test case naming, equality & deduplication | `ITestData` |
| **Core Abstraction** | Universal access across all test types | `INamedCase`, `NamedCase` |
| **Pattern Markers** | Intent discovery, pattern matching & compile-time type safety | Marker interfaces: `IExpected`, `IReturns`, `IThrows`; Generic constraints: `IExpected<TResult>`, `IReturns<TResult>`, `IThrows<TException>` |
| **Specializations** | Type-safe operations with context | `TestData<T1...T9>`, `TestDataReturns<TResult, T1...T9>`, `TestDataThrows<TException, T1...T9>` |

---

## Test Data Types

### Type Hierarchy

```
TestDataBase (abstract)
├── TestData<T1...T9>                           # General test data
└── TestDataExpected<TResult> (abstract)        # Base for expected outcomes
    ├── TestDataReturns<TResult>                # notnull constraint
    │   └── TestDataReturns<TResult, T1...T9>   # Supports value AND reference types
    └── TestDataThrows<TException>
        └── TestDataThrows<TException, T1...T9> # Exception constraint
```

### Factory Methods

All test data types are created via T4-generated factory methods:

```csharp
// General test data
CreateTestData<T1, T2>(definition, result, arg1, arg2)

// Return value test data (supports reference types)
CreateTestDataReturns<TResult, T1>(definition, expected, arg1)

// Exception test data
CreateTestDataThrows<TException, T1>(definition, expected, arg1)
```

---

## Core Concepts

### Identity System

Every test case has a deterministic `TestCaseName`:

```
"{scenario description} => {expected outcome}"
```

**Examples (with automatic formatting):**
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

### Why Standalone Formatting Library?

| Aspect | Embedded (v3.3.0) | Standalone (v4.0.0) |
|--------|-------------------|---------------------|
| **Reusability** | ❌ Tightly coupled to test data | ✅ Usable in any .NET project |
| **Separation of Concerns** | ❌ Mixed with core domain | ✅ Clean architectural boundary |
| **Extensibility** | ⚠️ Limited to test scenarios | ✅ Extensible formatter infrastructure |
| **Maintenance** | ❌ Harder to version independently | ✅ Independent versioning and releases |

> **Note:** While `Portamical.Core.Formatting` is architecturally separate, it remains specialized for Portamical's test data formatting needs (particularly `TestDataExpected` types). It is automatically included as a dependency when you install `Portamical.Core`.

### Why Formatter<T> Base Class?

**Before (v3.3.0 - Manual Implementation):**  
```csharp
public class ProductIdFormatter : IFormatter<ProductId>
{
    public string? Format(ProductId value) => $"PROD-{value.Id:D6}";
    
    // Boilerplate for IFormatter.Format(object)
    string? IFormatter.Format(object value)
    {
        return value is ProductId id ? Format(id) : null;
    }
}
```

**After (v4.0.0 - Base Class Simplification):**
```csharp
public sealed class ProductIdFormatter : Formatter<ProductId>
{
    public override string Format(ProductId value)
    {
        return value is null ? NullString : $"PROD-{value.Id:D6}";
    }
    // IFormatter.Format(object) automatically implemented by base class
}
```

**Key Benefits:**
- ✅ **Less Boilerplate** - No manual type checking
- ✅ **Type Safety** - Base class handles casting safely
- ✅ **Consistent Pattern** - All custom formatters follow same structure
- ✅ **Built-in Utilities** - Access to `NullString`, `FallbackIfNull`, etc.

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

## Performance

| Operation | Before | After | Speedup |
|-----------|--------|-------|---------|
| NotNull validation | ~10 cycles | ~2 cycles | 5x |
| NotNullOrEmpty | ~15 cycles | ~4 cycles | 3.75x |
| Enum validation | ~12 cycles | ~3 cycles | 4x |
| ToString() | ~10 cycles | ~2 cycles | 5x |
| Constructor (3 validations) | ~30 cycles | ~6 cycles | 5x |
| Character formatting | ~5 cycles | ~1 cycle | 5x (cached ASCII) |

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
| Type Support | Value types only | ✅ Value + reference types (v3.2.0+) |
| Formatting | ToString() only | ✅ Intelligent DefaultFormatter (v3.2.0+) |
| Extensibility | ❌ Monolithic | ✅ Standalone formatting library (v4.0.0) |
| Performance | Baseline | 5x faster (v2.2.0+) |
| Documentation | Minimal | Comprehensive XML docs |

---

## Changelog

### **Version 4.0.0 - Current** (2026-06-26)

**Architectural Evolution: Standalone Formatting Library**

This major release extracts the formatting system into an independent library, refines the formatter API, and simplifies extensibility patterns.

**Breaking Changes**

1. **Formatting Library Extracted**
   - Created standalone `Portamical.Core.Formatting` NuGet package
   - Formatting utilities automatically included as dependency of `Portamical.Core` v4.0.0
   - **Migration:** Update to `Portamical.Core` v4.0.0

2. **Formatter API Refinement**
   - **Renamed:** `ValueFormatter` → `DefaultFormatter` for clarity
   - **Moved:** `IFormatter` to `Portamical.Core.Formatting.CustomFormatters` namespace
   - **Changed:** `IFormatter<T>.Format(T)` return type: `string?` → `string` (non-nullable)
   - **New:** `Formatter<T>` abstract base class for custom formatters
   - **Removed:** `ValueFormatter.Registry` property (use `Formatter` static class instead)

3. **Builder Class Reorganization**
   - Helper methods moved from `Model.Formatter` to `Builder` static class
   - **Migration:** Change `using static Portamical.Core.Formatting.Model.Formatter;` to `using static Portamical.Core.Formatting.Builder;`

**Added**

- **Standalone Formatting Package:** `Portamical.Core.Formatting` independently versioned and maintained
- **Formatter<T> Base Class:** Simplified custom formatter implementation with sealed base method pattern
- **Builder Utilities:** Centralized string building helpers (`FallbackIfNull`, `JoinWithComma`, `CreateSeparatedString`, `CopyAsSpan`)
- **Singleton Pattern:** `DefaultFormatter.Instance` for reusable formatter access
- **ASCII Character Caching:** Pre-cached formats for printable ASCII characters (32-126) with 5x performance improvement

**Improved**

- Simplified custom formatter API surface
- Comprehensive XML documentation across formatter infrastructure
- Thread-safety guarantees documented for all formatters
- Performance optimizations in character formatting hot paths

**Documentation**

- Complete migration guide from v3.3.0
- Updated architecture diagrams showing standalone library
- Enhanced custom formatter examples using `Formatter<T>` base class
- Design rationale for standalone library approach

**Migration from v3.3.0**

```csharp
// 1. Update package version
dotnet add package Portamical.Core --version 4.0.0

// 2. Update using statements
// Before
using static Portamical.Core.Formatting.ValueFormatter;
using Portamical.Core.Formatting;

// After
using static Portamical.Core.Formatting.Builder;
using Portamical.Core.Formatting.CustomFormatters;

// 3. Update formatter usage
// Before
ValueFormatter.Format(value);
ValueFormatter.RegisterFormatter<T>(formatter);

// After
DefaultFormatter.Format(value);
Formatter.RegisterFormatter<T>(formatter);

// 4. Update custom formatters
// Before
public class MyFormatter : IFormatter<T>
{
    public string? Format(T value) => ...;
    string? IFormatter.Format(object value) => ...;
}

// After - Easier with base class
public sealed class MyFormatter : Formatter<T>
{
    public override string Format(T value) => ...;
}
```

---

### **Version 3.3.0** (2026-06-13)

**Extensibility, Performance & Documentation Release**

See master branch README for full v3.3.0 details.

**Key Features:**
- Custom formatter registry (`ValueFormatter.Registry`)
- `IFormatter` interface for extensibility
- Delegate formatting
- Span-based string building optimizations
- 66-75% fewer allocations in hot paths

---

### **Version 3.2.0** (2026-06-05)

**Breaking Architectural Improvements + Intelligent Formatting**

**Breaking Changes:**
- Changed `TestDataReturns<TStruct>` → `TestDataReturns<TResult>` with `notnull` constraint

**New Features:**
- Reference type support in `TestDataReturns`
- `ValueFormatter` class for intelligent formatting
- Automatic type-specific formatting for 12+ types

---

### **Version 2.2.0** (2026-04-23)

**Performance Optimizations**

- Aggressive inlining for hot path methods (5x performance improvement)
- Zero-allocation success paths maintained

---

### **Version 2.0.0** (2026-03-13)

**Comprehensive XML Documentation**

- Extensive documentation across entire codebase
- Design pattern documentation
- Performance and thread-safety notes

---

### **Version 1.0.0** (2026-03-04)

**Initial Release**

- Core test data types
- Factory methods
- Identity system
- Strategy pattern support

</details>

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

---