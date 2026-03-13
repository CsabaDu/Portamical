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

**Thread Safety Enhancement: Removed Static `TestBase.ArgsCode` Property**

In v1.x, the static `TestBase.ArgsCode` property created race conditions during parallel test execution:

```csharp
// v1.x ? RACE CONDITION EXAMPLE
// Thread 1: Set ArgsCode to Properties
TestBase.ArgsCode = AsProperties;

// Thread 2: Simultaneously sets ArgsCode to Instance (OVERWRITES Thread 1)
TestBase.ArgsCode = AsInstance;

// Thread 1: Convert reads ArgsCode.Instance (WRONG VALUE)
var args = Convert(dataSource.GetArgs());  // Uses Instance instead of Properties
```

**v2.0 Solution:** New `ConvertAsInstance` method  

In v2.0, `ConvertAsInstance` is a convenience helper for **instance-mode** conversion that avoids the v1.x static `ArgsCode` state and keeps conversions **thread-safe**.

```csharp
// v2.0
var args = ConvertAsInstance(convert, testDataCollection, testMethodName);
```

**Equivalent to:** invoking the adapter-supplied conversion delegate with `ArgsCode.Instance`.  

**Migration:**
- Remove all `ArgsCode` static field assignments
- Pass `ArgsCode` as parameter to `Convert()` methods
- Default behavior unchanged (uses `ArgsCode.Instance`)

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

### **Version 2.0.0 (2026-03-13)**

#### Breaking Changes

- **Removed:** `TestBase.ArgsCode` static property (thread safety issue)

#### Major Enhancements

**Comprehensive XML Documentation**
- Added extensive XML documentation comments across the entire codebase (65 files updated)
- Documented design patterns (Template Method, Strategy, Facade, Decorator)
- Added detailed usage examples with code samples for all public APIs
- Enhanced documentation for three distinct `TestBase` strategies

**Core Infrastructure Improvements**
- **`ConvertAsInstance` Helper Methods:**
  - Added protected helper methods in base `TestBase` class
  - Enables framework adapters to default to `ArgsCode.Instance` strategy
  - Two overloads: with/without `testMethodName` parameter
  - Thread-safe design (stateless implementation)

- **`TestBases.TestBase` (Base Class):**
  - Added `AsInstance`, `AsProperties`, `WithTestCaseName` constants
  - Added `ConvertAsInstance<TTestData, T>` protected helper methods
  - Serves as foundation for framework-agnostic conversion utilities

- **`TestBases.TestDataCollection.TestBase`:**
  - Provides `Convert<TTestData>` returning `IReadOnlyCollection<TTestData>`
  - Automatic deduplication via `ToDistinctArray()` extension
  - Designed for type-safe test data collection handling

- **`TestBases.ObjectArrayCollection.TestBase`:**
  - Provides `Convert<TTestData>` with optional `ArgsCode` parameter
  - Two-parameter: `Convert(data, argsCode)` for explicit strategy
  - One-parameter: `Convert(data)` defaults to `ArgsCode.Instance`
  - Returns `IReadOnlyCollection<object?[]>` for framework compatibility

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