Here's the updated README.md for the POC_TestDataExpected branch, ready for check-in:

```markdown
# Portamical.Core

**Framework-agnostic foundation of Portamical**: Universal, identity-driven test data modeling for .NET 10.

Define test data **once** and consume it across test frameworks using adapter packages - without rewriting the data or sacrificing strong typing.

---

## 🚨 What's New in v3.2.0 (Preview)

**Breaking Architectural Improvements**

This preview introduces significant type hierarchy changes to improve extensibility and type support:

### Breaking Changes

1. **TestDataExpected Architecture Refactoring**
   - **Renamed:** `TestDataExpected<TResult>` → `TestDataExpectedBase<TResult>`
   - **Added:** New intermediate sealed class `TestDataExpected<TResult>`
   - **Impact:** Custom types inheriting from `TestDataExpected<TResult>` must change to `TestDataExpectedBase<TResult>`

2. **TestDataReturns Type Constraint Relaxation**
   - **Changed:** `TestDataReturns<TStruct>` → `TestDataReturns<TResult>` with `notnull` constraint
   - **Now Supports:** Both value types AND reference types (strings, collections, custom classes)
   - **Impact:** Type parameter renamed from `TStruct` to `TResult`

### New Features

1. **Generic TestDataExpected Variants**
   - Added `TestDataExpected<TResult, T1...T9>` supporting 1-9 typed arguments
   - T4-generated for consistent implementation
   - Enables strongly-typed expected outcome tests

2. **Reference Type Support in TestDataReturns**
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

3. **Enhanced Code Generation**
   - Standardized `#nullable enable` across all T4 templates
   - Added missing XML documentation tags
   - Improved consistency in generated code

---

## Install

```bash
# Preview release
dotnet add package Portamical.Core --version 3.2.0-preview

# Stable release (for production)
dotnet add package Portamical.Core --version 2.2.0
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

### Return Value Test Data (v3.2.0 - Reference Type Support)

```csharp
public sealed class UserServiceCases
{
    public static IEnumerable<TestDataReturns<string, int>> GetUserName()
    {
        // ✅ NEW: String return values
        yield return CreateTestDataReturns(
            definition: "valid user ID",
            expected: "John Doe",
            arg1: 123);

        yield return CreateTestDataReturns(
            definition: "admin user",
            expected: "Administrator",
            arg1: 1);
    }

    public static IEnumerable<TestDataReturns<List<User>, int>> GetUserList()
    {
        // ✅ NEW: Collection return values
        yield return CreateTestDataReturns(
            definition: "department ID 5",
            expected: new List<User> { user1, user2 },
            arg1: 5);
    }
}
```

### Exception Test Data

```csharp
public sealed class ValidationCases
{
    public static IEnumerable<TestDataThrows<ArgumentNullException, string>> InvalidArgs()
    {
        yield return CreateTestDataThrows(
            definition: "null name",
            expected: new ArgumentNullException("name"),
            arg1: null);
    }
}
```

---

## Architecture

### Namespace Organization

```
Portamical.Core/
├── Factories/              # Factory methods for test data creation
├── Identity/               # Identity and equality contracts (INamedCase)
├── Safety/                 # Validation utilities (Validator, Resolver)
├── Strategy/               # Strategy enums (ArgsCode, PropsCode)
└── TestDataTypes/          # Core test data domain
    ├── ITestData.cs        # Base test data contract
    ├── Models/             # Concrete implementations
    │   ├── General/        # TestData<T1...T9>
    │   └── Specialized/    # TestDataReturns, TestDataThrows, TestDataExpected
    └── Patterns/           # Domain-specific marker interfaces
        ├── IExpected.cs    # Base for tests with expected outcomes
        ├── IReturns.cs     # Marker for return value tests
        └── IThrows.cs      # Marker for exception tests
```

### Four-Layer Model

| Layer | Role | Example Types |
|-------|------|--------------|
| **Base Interfaces** | Universal access across all test types | `ITestData`, `INamedCase` |
| **Markers** | Intent discovery & pattern matching | `IExpected`, `IReturns`, `IThrows` |
| **Generic Constraints** | Compile-time type safety | `IExpected<TResult>`, `IReturns<TResult>`, `IThrows<TException>` |
| **Concrete Implementations** | Type-safe operations with context | `TestData<T1...T9>`, `TestDataReturns<TResult, T1...T9>`, `TestDataThrows<TException, T1...T9>`, `TestDataExpected<TResult, T1...T9>` |

---

## Test Data Types (v3.2.0)

### Updated Type Hierarchy

```
TestDataBase (abstract)
├── TestData<T1...T9>                           # General test data
└── TestDataExpectedBase<TResult> (abstract)    # ⭐ Renamed (was TestDataExpected)
    ├── TestDataExpected<TResult>               # ⭐ New intermediate sealed class
    │   └── TestDataExpected<TResult, T1...T9>  # ⭐ New generic variants (1-9 args)
    ├── TestDataReturns<TResult>                # ⭐ notnull constraint (was struct)
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

### Step 1: Update Custom TestDataExpected Inheritance

```csharp
// v2.2.0 ❌
public class MyTestData<TResult> : TestDataExpected<TResult>
    where TResult : notnull
{ }

// v3.2.0 ✅
public class MyTestData<TResult> : TestDataExpectedBase<TResult>
    where TResult : notnull
{ }
```

### Step 2: Update TestDataReturns Type Parameters (Optional)

```csharp
// v2.2.0 (Old naming)
TestDataReturns<TStruct, int>  // Still compiles, but TStruct is legacy

// v3.2.0 (Recommended naming)
TestDataReturns<TResult, int>  // Clearer intent
```

### Step 3: Leverage Reference Type Support

```csharp
// v2.2.0 ❌ Compile error
// TestDataReturns<string, int>  // Error: string is not a struct

// v3.2.0 ✅ Now works!
var testData = CreateTestDataReturns(
    definition: "Get username",
    expected: "Alice",
    arg1: 42);

// For best results, override ToString() in custom types
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

- ✅ Custom types inherit from `TestDataExpectedBase<TResult>`
- ✅ Type parameter updated: `TStruct` → `TResult` (if referencing explicitly)
- ✅ Reference types now supported in `TestDataReturns`
- ✅ Tests rebuild and pass

---

## Core Concepts

### Identity System

Every test case has a deterministic `TestCaseName`:

```
"{scenario description} => {expected outcome}"
```

**Examples:**
- `"Add(2,3) => returns 5"`
- `"Validate(null) => throws ArgumentException"`
- `"Get user name => returns John Doe"` ← NEW: reference type

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
| **Real-world APIs** | Limited | ✅ Matches actual method signatures |
| **Type Safety** | ✅ Non-null guaranteed | ✅ Non-null guaranteed |
| **API Complexity** | ❌ Multiple specialized types needed | ✅ Unified API |
| **ToString()** | ✅ Always meaningful | ⚠️ Override recommended for custom types |

### Why Two-Tier TestDataExpected Hierarchy?

```
TestDataExpectedBase<TResult>  ← For user inheritance
    ↓
TestDataExpected<TResult>      ← Sealed framework intermediate
    ↓
TestDataReturns / TestDataThrows  ← Specialized concrete types
```

**Benefits:**
- ✅ **Extensibility** - Users inherit from `TestDataExpectedBase`
- ✅ **Safety** - Sealed middle tier prevents unintended inheritance
- ✅ **Clarity** - Clear separation of concerns

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
        ├── TestDataThrows.generated.cs
        ├── TestDataExpected.tt        # ⭐ NEW
        └── TestDataExpected.generated.cs
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
- **v3.2.0 Preview Branch:** https://github.com/CsabaDu/Portamical/tree/POC_TestDataExpected
- **Documentation:** https://github.com/CsabaDu/Portamical/blob/master/README.md
- **Migration Guide:** https://github.com/CsabaDu/Portamical/blob/POC_TestDataExpected/Portamical.Core/MIGRATION.md
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
| Performance | Baseline | 5x faster (v2.2.0) |
| Documentation | Minimal | Comprehensive XML docs |

---

## Changelog

### **v3.2.0-preview** (Current)

**Breaking Changes:**
- 🚨 Renamed `TestDataExpected<TResult>` → `TestDataExpectedBase<TResult>`
- 🚨 Changed `TestDataReturns<TStruct>` → `TestDataReturns<TResult>` with `notnull` constraint
- 🚨 Type parameter rename across generated code

**New Features:**
- ✨ Added `TestDataExpected<TResult, T1...T9>` generic variants
- ✨ Reference type support in `TestDataReturns` (strings, collections, custom classes)
- ✨ Standardized `#nullable enable` in T4 templates
- ✨ Enhanced XML documentation

**Improvements:**
- 📖 Updated docs to reflect `notnull` constraint semantics
- 🔧 Improved fallback logic in `GetExpectedResult()`
- 🧪 Expanded test coverage for new types

**Migration:**
- Custom types: Change `TestDataExpected<TResult>` → `TestDataExpectedBase<TResult>`
- Type parameters: Update `TStruct` → `TResult` if referencing explicitly
- See [Migration Guide](#migration-guide-v220--v320)

---

### **v2.2.0** (2026-04-23) - Stable

**Performance Optimizations**
- 5x faster null validation (`NotNull`, `NotNullOrEmpty`)
- 4x faster enum validation (`Defined`)
- 3-5x faster string conversions
- Zero-allocation success paths maintained

**Technical:**
- Added `MethodImpl(AggressiveInlining)` to 7 hot path methods
- Fully backward compatible

---

### **v2.0.0** (2026-03-13)

**Comprehensive XML Documentation**
- 65 files updated with detailed docs
- Design patterns documented
- Usage examples added

**Core Improvements:**
- `NamedCase`: Zero-allocation string concatenation
- `Resolver`: Improved fallback logic
- `EnumValidator`: Renamed `GetResultPrefix` → `GetValidResultPrefix`

---

### **v1.0.0** (2026-03-04)

- Initial release of Portamical.Core

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical.Core: Foundation for universal, identity-driven test data modeling.*

```

This README is ready for check-in to the POC_TestDataExpected branch. Key changes:
- ✅ Removed "Why Patterns stays under TestDataTypes" section
- ✅ Focused on v3.2.0 breaking changes and new features
- ✅ Clear migration guide
- ✅ Concise architecture explanation
- ✅ Updated examples showing reference type support
- ✅ Professional, deployment-ready format