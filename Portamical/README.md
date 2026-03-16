# Portamical

**Shared utilities and base classes for cross-framework test data solutions in .NET.**

Portamical provides framework-agnostic converters, assertions, and test base classes that bridge between Portamical.Core and framework-specific adapters.

---

## Install

```bash
dotnet add package Portamical
```

> **Note:** Most users should install a framework adapter instead:
> - `Portamical.xUnit` for xUnit v2
> - `Portamical.xUnit_v3` for xUnit v3
> - `Portamical.MSTest` for MSTest 4
> - `Portamical.NUnit` for NUnit 4

---

## What's New

## Portamical [2.0.0] - 2026-03-16

### Breaking Changes
- **Removed** `TestBase.ResetLogCounter()` - Use `Resolver.ResetLogCounter()` directly
- **Removed** `IDisposable` implementation from `Portamical.TestBases.TestBase`
- **Removed** mutable `ArgsCode` property with setter
- **Changed** `TestBase` to stateless architecture (thread-safe, immutable design)
- **Changed** `ITestDataProvider<TTestData>` to **contravariant** (`ITestDataProvider<in TTestData>`)
- **Changed** `ITestDataConverter<TTestData, TRow>` to **variant** (`ITestDataConverter<in TTestData, out TRow>`)

### Added
- `ConvertAsInstance<TTestData, T>()` helper methods (2 overloads) for centralized delegation
- Comprehensive XML documentation (3,000+ lines)
- `AsInstance`, `AsProperties`, `WithTestCaseName` read-only properties
- Design pattern documentation (Template Method, Strategy, Delegation)
- Framework adapter implementation guidelines
- **Variance support** for generic interfaces enabling flexible type assignments

### Changed
- **TestBase.cs**: Refactored from 38 lines (stateful, disposable) to 195 lines (stateless, documented)
- **ITestDataProvider.cs**: Made contravariant to support base-to-derived type assignments
- **ITestDataConverter.cs**: Made contravariant (input) and covariant (output) for flexible conversions
- All properties now expression-bodied read-only members
- Enhanced documentation with examples, remarks, and exception handling

### Documentation
- Added detailed XML comments for all public APIs
- Documented variance patterns with practical examples
- Documented delegation pattern for framework adapters
- Added usage examples for MSTest, NUnit, xUnit implementations
- Enhanced parameter descriptions and return value documentation
- Added thread safety and stateless design notes

### Variance Examples

#### **ITestDataProvider** (Contravariance)
```csharp
// BEFORE (v1) - Invariant
public interface ITestDataProvider<TTestData> { }

// AFTER (v2) - Contravariant
public interface ITestDataProvider<in TTestData> { }

// Example usage:
ITestDataProvider<ITestData> generalProvider = new GeneralProvider();

// ✅ Now works: Can use general provider for specific type
ITestDataProvider<TestDataReturns<int>> specificProvider = generalProvider;
// Works because TestDataReturns<int> IS AN ITestData (contravariance)
```

### Architecture

```csharp
// BEFORE (v1)
public abstract class TestBase : IDisposable
{
    protected static ArgsCode ArgsCode { get; set; } = AsInstance; // Mutable
    protected static long ResetLogCounter() => Resolver.ResetLogCounter();
    public void Dispose() { ... }
}

public interface ITestDataProvider<TTestData> { }      // Invariant
public interface ITestDataConverter<TTestData, TRow> { } // Invariant

// AFTER (v2)
public abstract class TestBase // Stateless
{
    protected static ArgsCode AsInstance => ArgsCode.Instance;
    protected static ArgsCode AsProperties => ArgsCode.Properties;
    
    // Centralized delegation pattern
    protected static T ConvertAsInstance<TTestData, T>(
        Func<IEnumerable<TTestData>, ArgsCode, string?, T> convert,
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName)
    where TTestData : notnull, ITestData;
}

public interface ITestDataProvider<in TTestData> { }        // Contravariant
public interface ITestDataConverter<in TTestData, out TRow> { } // Variant
```

**Migration:**
// v1 → v2 Migration
- Remove: IDisposable inheritance
- Remove: Dispose() methods
- Remove: ResetLogCounter() calls
- Remove all `ArgsCode` static field assignments
- Default behavior unchanged (uses `ArgsCode.Instance`)
+ Add: using Portamical.Core.Safety;
+ Add: Direct Resolver.ResetLogCounter() calls
+ Update: Use AsInstance/AsProperties instead of ArgsCode property
+ Benefit: Leverage variance for flexible type assignments

---

## What's Included

### Converters
Transform test data collections into framework-consumable formats:

```csharp
using Portamical.Converters;

// Convert to object arrays with automatic deduplication
var args = testDataCollection.ToDistinctReadOnly(ArgsCode.Instance);
var argsFlattened = testDataCollection.ToDistinctReadOnly(ArgsCode.Properties);
```

### Assertions
Framework-agnostic assertion helpers with delegate injection:

```csharp
using static Portamical.Assertions.PortamicalAssert;

// xUnit
ThrowsDetails(
    attempt: () => MethodUnderTest(),
    expected: new ArgumentNullException("paramName"),
    catchException: Record.Exception,
    assertIsType: Assert.IsType,
    assertEquality: Assert.Equal,
    assertFail: Assert.Fail);

// MSTest
ThrowsDetails(
    attempt,
    expected,
    catchException: CatchException,
    assertIsType: (e, a) => Assert.AreEqual(e, a.GetType()),
    assertEquality: (e, a) => Assert.AreEqual(e, a),
    assertFail: Assert.Fail);
```

### Test Bases

Three specialized base classes for different conversion strategies:

#### Strategy 1: TestData Collection (Type-Safe)

```csharp
using Portamical.TestBases.TestDataCollection;

public class MyTests : TestBase
{
    protected static IReadOnlyCollection<TestData<DateOnly>> Args
        => Convert(dataSource.GetArgs());
}
```

Returns: `IReadOnlyCollection<TestData<T>>` with automatic deduplication.

#### Strategy 2: Instance Array (Object Wrapper)

```csharp
using Portamical.TestBases.ObjectArrayCollection;

public class MyTests : TestBase
{
    private static IReadOnlyCollection<object?[]> Args
        => Convert(dataSource.GetArgs());  // ArgsCode.Instance default

    [TestMethod, DynamicData(nameof(Args))]
    public void Test(TestData<DateOnly> testData) { ... }
}
```

Returns: `IReadOnlyCollection<object?[]>` where each array contains `[testData]`.

#### Strategy 3: Flattened Properties Array

```csharp
using Portamical.TestBases.ObjectArrayCollection;

public class MyTests : TestBase
{
    public static IReadOnlyCollection<object?[]> Args
        => Convert(dataSource.GetArgs(), AsProperties);

    [Theory, MemberData(nameof(Args))]
    public void Test(DateOnly arg1, BirthDay arg2) { ... }
}
```

Returns: `IReadOnlyCollection<object?[]>` where each array contains `[arg1, arg2, ...]`.

---

## ArgsCode Strategy Pattern

| Strategy | Produces | Test Method Signature | Base Class |
|----------|----------|----------------------|------------|
| No ArgsCode | `IReadOnlyCollection<TTestData>` | `void Test(TestData<T> data)` | `TestDataCollection.TestBase` |
| `AsInstance` (default) | `IReadOnlyCollection<object?[]>` with `[testData]` | `void Test(TestData<T> data)` | `ObjectArrayCollection.TestBase` |
| `AsProperties` | `IReadOnlyCollection<object?[]>` with `[arg1, arg2, ...]` | `void Test(T arg1, T arg2, ...)` | `ObjectArrayCollection.TestBase` |

---

## Links

- GitHub: https://github.com/CsabaDu/Portamical
- Documentation: https://github.com/CsabaDu/Portamical/blob/master/README.md
- Issues: https://github.com/CsabaDu/Portamical/issues

---

## License and Project Lineage

This project is licensed under the [MIT License](https://github.com/CsabaDu/Portamical/blob/master/LICENSE.txt).

`Portamical` is the continuation and successor of `CsabaDu.DynamicTestData.Light` and `CsabaDu.DynamicTestData` (also MIT-licensed).  
`CsabaDu.DynamicTestData.Light` and `CsabaDu.DynamicTestData` are considered legacy and are no longer supported; new development happens in Portamical.

---

## Changelog

### **Version 2.0.0 (2026-03-16)**

**Breaking**
- Removed `TestBase.ResetLogCounter()` → use `Resolver.ResetLogCounter()`
- Removed `IDisposable` from `TestBase` (now stateless)
- Removed mutable `ArgsCode` property
- **Made `ITestDataProvider<TTestData>` contravariant** (`<in TTestData>`)
- **Made `ITestDataConverter<TTestData, TRow>` variant** (`<in TTestData, out TRow>`)

**Added**
- `ConvertAsInstance<TTestData, T>()` delegation helpers (2 overloads)
- 3,000+ lines XML documentation
- Read-only properties: `AsInstance`, `AsProperties`, `WithTestCaseName`
- **Variance support** for flexible type assignments

**Changed**
- `TestBase`: 38 → 195 lines (stateful → stateless)
- `ITestDataProvider`: invariant → contravariant (enables base-to-derived assignment)
- `ITestDataConverter`: invariant → variant (contravariant input + covariant output)
- All properties now expression-bodied read-only

**Variance Benefits**
```csharp
// Contravariance example
ITestDataProvider<ITestData> general = new GeneralProvider();
ITestDataProvider<TestDataReturns<int>> specific = general; // ✅ Now works

// Variance example
ITestDataConverter<ITestData, object[]> converter = new GeneralConverter();
ITestDataConverter<TestDataReturns<int>, object[]> typed = converter; // ✅ Works
```

**Migration**
```diff
- public class MyTests : TestBase { }  // v1: IDisposable
+ public class MyTests : TestBase { }  // v2: Stateless

- ArgsCode = AsProperties;             // v1: Mutable
+ var code = AsProperties;             // v2: Read-only

- public interface ITestDataProvider<TTestData>              // v1: Invariant
+ public interface ITestDataProvider<in TTestData>           // v2: Contravariant

- public interface ITestDataConverter<TTestData, TRow>       // v1: Invariant
+ public interface ITestDataConverter<in TTestData, out TRow> // v2: Variant

- ResetLogCounter();                   // v1: Instance method
+ Resolver.ResetLogCounter();          // v2: Static call
```

**Documentation & Code Quality**
- Enhanced code comments explaining the three-strategy approach
- Improved method naming consistency across base classes
- Added comprehensive examples for each strategy
- Clarified inheritance relationships between `TestBase` classes

---

### **Version 1.0.0 (2026-03-06)**

- Initial release
- Framework-agnostic converters
- `PortamicalAssert` with delegate injection
- Abstract `TestBase` classes
- Strategy Pattern support with `ArgsCode` enum

---

##### **Version 1.0.1 (2026-03-07)**

- Moved xunit.runner.json from Portamical to xUnit adapter packages
- Improved GlobalUsings.cs organization

---

##### **Version 1.0.2 (2026-03-08)**

- Implemented standard `IDisposable` pattern in `Portamical.TestBases.TestBase`

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*