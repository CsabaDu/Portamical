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
> - `Portamical.TUnit` for TUnit

---

## What's New

### **Version 2.2.0 (2026-04-22)**

**Added**
- Async-first assertion architecture using `ValueTask` for zero-allocation performance
- `DoesNotThrowAsync(Action, Func<string, ValueTask>)` - Async version of `DoesNotThrow`
- `ThrowsDetailsAsync<TException>(...)` - Async exception validation with metadata
- `EqualityAsync<T>(...)` - Async generic equality with custom comparison delegate
- `EqualityAsync(object, object?, ...)` - Async built-in type equality with floating-point tolerance
- `IsTypeOfAsync(Type, object?, ...)` - Async runtime type verification

**Changed**
- Internal refactoring: sync assertion methods now delegate to async base implementations (no API changes)
- Performance optimizations with zero-allocation success paths using `default(ValueTask)`
- Enhanced XML documentation with async-first architecture guide

**Performance**
- Zero heap allocations on success paths for async assertions
- Optimized hot paths with `MethodImpl(AggressiveInlining)`
- Sync wrappers have ~5ns overhead (negligible)

**Migration**
- **Fully backward compatible** with 2.1.x - no code changes required
- Existing sync methods work unchanged (delegate internally to async base)
- Optional: upgrade to async methods in async-first frameworks (TUnit, MSTest v2+)

---

### **Version 2.1.0 (2026-04-20)**

**Added**
- `Equality<T>(T?, T?, Func<T?, T?, bool>, Action<string?>, string?)` - Generic equality with custom comparison
- `Equality(object, object?, Action, double?)` - Optimized equality for 22+ built-in types with floating-point tolerance
- Collection equality support using `SequenceEqual` with recursive comparison
- Floating-point tolerance: configurable epsilon for `float` (1e-6f) and `double` (1e-10)
- Special value handling: NaN, +∞, -∞ with bitwise comparison
- `BigInteger` equality support
- `ToDistinctReadOnly<TTestData>(IEnumerable<TTestData>)` - Parameterless converter using default `ArgsCode.Instance`

**Changed**
- `IsTypeOf(Type, object?, Action<Type, Type?>)` - Now accepts nullable `actual` parameter
- Refactored floating-point comparison with hybrid absolute/relative tolerance
- Improved null handling in generic equality methods

**Fixed**
- Floating-point precision issues (0.1 + 0.2 == 0.3 now works correctly)
- Collection comparison now respects element equality rules

---

## What's Included

### Converters
Transform test data collections into framework-consumable formats:

```csharp
using Portamical.Converters;

// Convert to object arrays with automatic deduplication
var args = testDataCollection.ToDistinctReadOnly(ArgsCode.Instance);
var argsFlattened = testDataCollection.ToDistinctReadOnly(ArgsCode.Properties);

// Parameterless overload (new in 2.1.0)
var args = testDataCollection.ToDistinctReadOnly();  // Uses ArgsCode.Instance
```

### Assertions
Framework-agnostic assertion helpers with delegate injection:

```csharp
using static Portamical.Assertions.PortamicalAssert;

// Value equality with floating-point tolerance
Equality(
    expected: 0.3,
    actual: 0.1 + 0.2,  // Handles precision correctly
    assertFail: () => Assert.Fail(),
    floatingPointTolerance: 1e-10);

// Collection equality
Equality(
    expected: new[] { 1, 2, 3 },
    actual: service.GetNumbers(),
    assertFail: () => Assert.Fail());

// Exception validation
ThrowsDetails(
    attempt: () => MethodUnderTest(),
    expected: new ArgumentNullException("paramName"),
    catchException: Record.Exception,
    assertIsType: Assert.IsType,
    assertEquality: Assert.Equal,
    assertFail: Assert.Fail);

// Async assertions (new in 2.2.0)
await DoesNotThrowAsync(
    attempt: async () => await service.ProcessAsync(),
    assertFailAsync: msg => throw new AssertionException(msg));
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

## Equality Method Features

### Supported Types (Pattern Matching)

**Integer Types (10):** `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `nint`, `nuint`

**Floating-Point (2):** `float`, `double` (with tolerance)

**Other Primitives (4):** `bool`, `char`, `string`, `decimal`

**Framework Types (6):** `Guid`, `DateTime`, `DateOnly`, `TimeOnly`, `TimeSpan`, `DateTimeOffset`

**Numerics (1):** `BigInteger`

**Collections:** Any `IEnumerable` (recursive comparison)

### Floating-Point Handling

```csharp
// Default tolerance
Equality(0.3, 0.1 + 0.2, Assert.Fail);  // ✅ PASSES

// Custom tolerance
Equality(3.14159, Math.PI, Assert.Fail, floatingPointTolerance: 0.001);

// Special values
Equality(float.NaN, float.NaN, Assert.Fail);  // ✅ PASSES
Equality(double.PositiveInfinity, double.PositiveInfinity, Assert.Fail);  // ✅ PASSES
```

**Special Value Behavior:**
- **NaN:** All NaN representations are equal (bitwise-independent)
- **Infinity:** Must match exactly (+∞ == +∞, -∞ == -∞)
- **Zero:** +0.0 and -0.0 are equal (mathematical equality)

### Collection Equality

```csharp
// Arrays
Equality(
    expected: new[] { 1, 2, 3 },
    actual: new[] { 1, 2, 3 },
    assertFail: Assert.Fail);  // ✅ PASSES

// Nested collections
Equality(
    expected: new[] { new[] { 1, 2 }, new[] { 3, 4 } },
    actual: service.GetMatrix(),
    assertFail: Assert.Fail);  // Recursive comparison

// Mixed types
Equality(
    expected: new object[] { 1, "hello", 3.14, true },
    actual: parser.GetValues(),
    assertFail: Assert.Fail);
```

---

## Async-First Architecture (v2.2.0)

### Design Principle

Core assertion logic is implemented in async methods using `ValueTask`. Sync methods are thin wrappers that delegate to async implementations:

```csharp
// Primary implementation (async)
protected static ValueTask DoesNotThrowAsync(
    Action attempt,
    Func<string, ValueTask> assertFailAsync);

// Sync wrapper (delegates to async)
public static void DoesNotThrow(Action attempt, Action<string> assertFail)
{
    DoesNotThrowAsync(attempt, msg =>
    {
        assertFail(msg);
        return default;
    }).ConfigureAwait(false).GetAwaiter().GetResult();
}
```

### Performance Characteristics

| Operation | Allocations | Overhead |
|-----------|-------------|----------|
| Async assertions (success) | 0 bytes | ~0 ns |
| Sync wrappers | 0 bytes | ~5 ns |

**Zero allocation** on success paths enables high-performance async assertions without garbage collection pressure.

---

## Links

- GitHub: https://github.com/CsabaDu/Portamical
- Documentation: https://github.com/CsabaDu/Portamical/blob/master/README.md
- Issues: https://github.com/CsabaDu/Portamical/issues

---

## License

This project is licensed under the [MIT License](https://github.com/CsabaDu/Portamical/blob/master/LICENSE.txt).

`Portamical` is the continuation and successor of `CsabaDu.DynamicTestData.Light` and `CsabaDu.DynamicTestData` (also MIT-licensed).

---

## Changelog

### **[2.0.0] - 2026-03-16**

**Breaking**
- Removed `TestBase.ResetLogCounter()` → use `Resolver.ResetLogCounter()`
- Removed `IDisposable` from `TestBase` (now stateless)
- Removed mutable `ArgsCode` property
- Made `ITestDataProvider<TTestData>` contravariant (`<in TTestData>`)
- Made `ITestDataConverter<TTestData, TRow>` variant (`<in TTestData, out TRow>`)

**Added**
- `ConvertAsInstance<TTestData, T>()` delegation helpers (2 overloads)
- 3,000+ lines XML documentation
- Read-only properties: `AsInstance`, `AsProperties`, `WithTestCaseName`
- Variance support for flexible type assignments

**Changed**
- `TestBase`: 38 → 195 lines (stateful → stateless)
- All properties now expression-bodied read-only

---

##### **[2.0.1] - 2026-03-20**

**Documentation Update**
- Breaking changes description corrected

---

##### **[2.0.2] - 2026-04-02**

**Changed**
- Updated Portamical.Core dependency: 2.0.0 → 2.0.1

---

#### **[2.1.0] - 2026-04-20**

**Added**
- Generic `Equality<T>()` method with custom comparison delegate
- Built-in `Equality()` method supporting 22+ types with pattern matching
- Floating-point tolerance support (configurable epsilon)
- Collection equality with recursive element comparison
- Special value handling for NaN, infinities, and signed zeros
- `BigInteger` equality support
- `ToDistinctReadOnly()` parameterless overload

**Changed**
- `IsTypeOf()` now accepts nullable `actual` parameter (breaking change)
- Improved floating-point comparison with bitwise equality checks
- Enhanced null handling in generic methods

**Fixed**
- Floating-point precision issues (0.1 + 0.2 now equals 0.3)
- Collection comparison with nested structures

---

##### **[2.1.1] - 2026-04-21**

**Added**
- `GetNotExpectedValueMessage()` protected helper method

---

### **[2.2.0] - 2026-04-22**

**Added**
- Async-first assertion architecture using `ValueTask`
- `DoesNotThrowAsync`, `ThrowsDetailsAsync`, `EqualityAsync`, `IsTypeOfAsync`
- Zero-allocation async methods for optimal performance
- Comprehensive XML documentation for async patterns

**Changed**
- Internal refactoring: sync methods delegate to async base (no API changes)
- Performance optimizations with `ConfigureAwait(false)` and `MethodImpl(AggressiveInlining)`

**Migration**
- Fully backward compatible with 2.1.x
- No code changes required for existing tests

---

### **[1.0.0] - 2026-03-06**

- Initial release
- Framework-agnostic converters
- `PortamicalAssert` with delegate injection
- Abstract `TestBase` classes
- Strategy Pattern support with `ArgsCode` enum

---

##### **[1.0.1] - 2026-03-07**

- Moved xunit.runner.json from Portamical to xUnit adapter packages
- Improved GlobalUsings.cs organization

---

##### **[1.0.2] - 2026-03-08**

- Implemented standard `IDisposable` pattern in `Portamical.TestBases.TestBase`

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

---
