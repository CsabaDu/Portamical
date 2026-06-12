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

### **Version 3.1.0 (2026-05-27)**

***Exception Metadata Assertion API***

**Added**
- **`MetadataEquality<TException>(TException, TException, Action<string, string?>)`** - New public synchronous method for exception metadata assertions
  - Framework-agnostic verification of exception messages and parameter names
  - Thread-safe synchronous wrapper delegating to `MetadataEqualityAsync`
  - Supports `ArgumentException`, `ArgumentOutOfRangeException`, `ObjectDisposedException` with intelligent message filtering

**Changed**
- **`MetadataEqualityAsync<TException>(TException, TException, Func<string, string?, ValueTask>)`** - Made public (was private)
  - Primary async implementation for exception metadata verification
  - Intelligently handles framework-generated messages that vary across runtime versions and locales
  - Skips assertion for:
    - `ArgumentException` guard clauses: `"The value cannot be an empty string"`, `"'paramName' ('value'...)"`
    - `ObjectDisposedException` runtime patterns: `"Cannot access a disposed object.\nObject name: 'objectName'"`

**Improved**
- Enhanced XML documentation for both `MetadataEqualityAsync` and `MetadataEquality`
  - Detailed remarks explaining selective assertion logic for framework exceptions
  - Usage examples for TUnit (async) and NUnit (sync) scenarios
  - Performance notes and thread safety guidance

**Example Usage**

Async (TUnit, MSTest):
```csharp
var expected = new ArgumentException("Invalid value", "paramName");
var actual = new ArgumentException("Invalid value", "paramName");

await PortamicalAssert.MetadataEqualityAsync(
    expected,
    actual,
    async (exp, act) => await Assert.Equal(exp, act));
```

Sync (NUnit, xUnit):
```csharp
var expected = new ArgumentOutOfRangeException("count", "Count must be positive");
var actual = new ArgumentOutOfRangeException("count", "Count must be positive");

PortamicalAssert.MetadataEquality(
    expected,
    actual,
    (exp, act) => Assert.AreEqual(exp, act));
```

---

### **Version 3.0.0 (2026-04-27)**

***API Cleanup***

**Breaking Changes**
- **Removed `ThrowsDetailsAsync(Action, Func<Func<Task>, ValueTask<Exception?>>, ...)` overload**
  - Async method accepting sync action with async exception catcher
  - Rationale: Unnecessary wrapper encouraging anti-pattern (async test for sync code)
  - Migration: Use sync `ThrowsDetails(Action, Func<Action, Exception?>, ...)` for testing synchronous code

**Migration Guide**

Before (v2.3.0):
```csharp
await ThrowsDetailsAsync(
    () => mySyncMethod(),
    expected,
    CatchExceptionAsync,  // Async catcher with sync action
    // ...
);
```

After (v3.0.0):
```csharp
// Recommended: sync test for sync code
ThrowsDetails(
    () => mySyncMethod(),
    expected,
    CatchException,  // Sync catcher
    assertIsType,
    assertEquality,
    assertFail);
```

---

### **Version 2.3.0 (2026-04-25)**

***Async-First Architecture Completed***

**Added**
- `CatchExceptionAsync(Func<Task>)` - Async exception catcher with fatal exception filtering
- `DoesNotThrowAsync(Func<Task>, Func<string, ValueTask>)` - Async action overload for assertion scenarios

**Changed**
- `CatchException(Action)` - Refactored as sync wrapper delegating to `CatchExceptionAsync` (backward compatible)
- `DoesNotThrow(Action, Action<string>)` - Refactored as sync wrapper, eliminated `#pragma warning disable CA2012`
- Async assertion methods visibility changed from `protected` to `public`
  - Affected: `DoesNotThrowAsync`, `ThrowsDetailsAsync`, `EqualityAsync`, `IsTypeOfAsync`
  - Enables direct usage in async frameworks (TUnit, MSTest 4)

**Improved**
- Comprehensive XML documentation for `IsNotFatal` and all new/changed methods
- Fatal exception filtering consistent across all sync/async paths
- Zero-allocation success paths preserved in all refactorings

**Breaking Changes:** None - fully backward compatible with 2.2.x

---

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
- Fully backward compatible with 2.1.x - no code changes required
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
    catchException: CatchException,
    assertIsType: Assert.IsType,
    assertEquality: Assert.Equal,
    assertFail: Assert.Fail);

// Async exception catching (new in 2.3.0)
var exception = await CatchExceptionAsync(async () => 
    await myService.ProcessAsync());

// Async assertion (new in 2.3.0)
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
Equality(0.3, 0.1 + 0.2, Assert.Fail);  // Passes

// Custom tolerance
Equality(3.14159, Math.PI, Assert.Fail, floatingPointTolerance: 0.001);

// Special values
Equality(float.NaN, float.NaN, Assert.Fail);  // Passes
Equality(double.PositiveInfinity, double.PositiveInfinity, Assert.Fail);  // Passes
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
    assertFail: Assert.Fail);  // Passes

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

## Async-First Architecture (v2.2.0+)

### Design Principle

Core assertion logic is implemented in async methods using `ValueTask`. Sync methods are thin wrappers that delegate to async implementations:

```csharp
// Primary implementation (async)
public static async ValueTask DoesNotThrowAsync(
    Func<Task> attempt,
    Func<string, ValueTask> assertFailAsync);

// Sync wrapper (delegates to async)
public static void DoesNotThrow(Action attempt, Action<string> assertFail)
{
    ThreadSafeSyncAssertion(DoesNotThrowAsync(() =>
    {
        attempt();
        return Task.CompletedTask;
    },
    msg =>
    {
        assertFail(msg);
        return new ValueTask();
    }));
}
```

### Exception Handling Architecture (v2.3.0)

**Sync/Async Parity:**

| Operation | Sync Method | Async Method | Implementation |
|-----------|-------------|--------------|----------------|
| **Catch Exception** | `CatchException(Action)` | `CatchExceptionAsync(Func<Task>)` | Async primary, sync wrapper |
| **Assert No Throw** | `DoesNotThrow(Action, ...)` | `DoesNotThrowAsync(Func<Task>, ...)` | Async primary, sync wrapper |
| **Fatal Exception Filtering** | `IsNotFatal(Exception)` | Same (shared) | Used in both sync/async paths |

**Fatal Exception Safety:**
All exception catching methods filter fatal exceptions using `IsNotFatal`:
- `OutOfMemoryException`
- `AccessViolationException`
- `StackOverflowException`
- `ThreadAbortException`

These exceptions always propagate immediately to terminate the process safely.

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

### **Version 3.0.0 (2026-04-25)**

***API Cleanup***

**Breaking Changes**
- **Removed `ThrowsDetailsAsync(Action, Func<Func<Task>, ValueTask<Exception?>>, ...)` overload**
  - Async method with sync action and async exception catcher parameter
  - Rationale: Unnecessary wrapper that encouraged anti-pattern (async test for sync code)
  - Migration: Use sync `ThrowsDetails(Action, Func<Action, Exception?>, ...)` for testing synchronous code
  - Impact: Users calling this specific overload signature will need to update to sync wrapper

**Retained**
- `ThrowsDetailsAsync(Action, Func<Action, Exception?>, ...)` - Async version with sync exception catcher
  - Primary async implementation for exception detail testing
  - Accepts sync action and sync exception catcher
  - Used internally by sync `ThrowsDetails` wrapper

**Technical Details**
- Simplified API surface by removing redundant async/sync hybrid overload
- No impact on common usage patterns (framework adapters unaffected)
- Version bump to 3.0.0 due to public API removal

**Migration Guide**

Before (v2.3.0):
```csharp
// Anti-pattern: async catcher with sync action
await ThrowsDetailsAsync(
    () => mySyncMethod(),
    expected,
    CatchExceptionAsync,  // Async catcher (mismatch!)
    // ...
);
```

After (v3.0.0):
```csharp
// Recommended: sync test for sync code
ThrowsDetails(
    () => mySyncMethod(),
    expected,
    CatchException,  // Sync catcher (matches!)
    assertIsType,
    assertEquality,
    assertFail);

// Or: async test with sync catcher
await ThrowsDetailsAsync(
    () => mySyncMethod(),
    expected,
    CatchException,
    assertIsTypeAsync,
    assertEqualityAsync,
    assertFailAsync);
```

**Dependencies**
- Portamical.Core: 2.2.0 (unchanged)

---

##### **Version 3.0.1 (2026-05-01)**

**Added**
- Null parameter validation to async assertion methods (`DoesNotThrowAsync(Func<Task>`, `Func<string, ValueTask>)`, `ThrowsDetailsAsync<TException>(Func<Task>`, `TException, Func<Func<Task>, ValueTask<Exception?>>`, `Func<Type, object, ValueTask>`, `Func<string, string?, ValueTask>`, `Func<string, ValueTask>), CatchExceptionAsync(Func<Task>)`)
- 6 unit tests for `CatchExceptionAsync(Func<Task>)` method coverage
- 5 unit tests for exception metadata edge cases (`ArgumentOutOfRangeException`, `ObjectDisposedException`)
- 5 unit tests for collection equality edge cases (empty collections, different lengths)

**Fixed**
- `DoesNotThrowAsync(Func<Task>, Func<string, ValueTask>)`: Null check now occurs before `CatchExceptionAsync(Func<Task>)` call
- `ThrowsDetailsAsync<TException>(Func<Task>, TException, Func<Func<Task>, ValueTask<Exception?>>, Func<Type, object, ValueTask>, Func<string, string?, ValueTask>, Func<string, ValueTask>)`: Added validation for all 5 delegate parameters
- `CatchExceptionAsync(Func<Task>)`: Added missing attempt parameter validation

**Changed**
- Test count increased from 119 to 135 (all passing)
- Async methods now throw `ArgumentNullException` instead of `NullReferenceException` for null parameters

---

#### **Version 3.1.0 (2026-05-27)**

**Added**
- **`MetadataEquality<TException>(TException, TException, Action<string, string?>)`** - New public synchronous wrapper for exception metadata assertions
  - Framework-agnostic verification of exception messages and parameter names
  - Thread-safe synchronous wrapper delegating to `MetadataEqualityAsync` via `ThreadSafeSync`
  - Supports `ArgumentException`, `ArgumentOutOfRangeException`, `ObjectDisposedException` with intelligent message filtering
- Comprehensive XML documentation for both `MetadataEqualityAsync` and `MetadataEquality` methods
  - Detailed remarks explaining selective assertion logic for framework exceptions
  - Usage examples for TUnit (async) and NUnit (sync) scenarios
  - Performance notes and thread safety guidance

**Changed**
- **`MetadataEqualityAsync<TException>(TException, TException, Func<string, string?, ValueTask>)`** - Visibility changed from `private` to `public`
  - Now accessible for direct use in async test frameworks (TUnit, MSTest 4)
  - Primary async implementation for exception metadata verification
  - Intelligently handles framework-generated messages that vary across runtime versions and locales
  - Skips assertion for:
    - `ArgumentException` guard clauses: `"The value cannot be an empty string"`, `"'paramName' ('value'...)"`
    - `ObjectDisposedException` runtime patterns: `"Cannot access a disposed object.\nObject name: 'objectName'"`

**Example Usage**

Async (TUnit, MSTest):
```csharp
var expected = new ArgumentException("Invalid value", "paramName");
var actual = new ArgumentException("Invalid value", "paramName");

await PortamicalAssert.MetadataEqualityAsync(
    expected,
    actual,
    async (exp, act) => await Assert.Equal(exp, act));
```

Sync (NUnit, xUnit):
```csharp
var expected = new ArgumentOutOfRangeException("count", "Count must be positive");
var actual = new ArgumentOutOfRangeException("count", "Count must be positive");

PortamicalAssert.MetadataEquality(
    expected,
    actual,
    (exp, act) => Assert.AreEqual(exp, act));
```

**Dependencies**
- Portamical.Core: 2.2.0 (unchanged)

---

#### **Version 3.2.0 (2025-06-05)**

**Updated**
- Portamical.Core dependency to v3.2.0
  - Maintains compatibility with latest Portamical.Core features and improvements
  - No breaking changes or API modifications in this release

---

##### **Version 3.2.1 (2026-06-06)**

**Updated**
- Portamical.Core dependency updated to v3.2.1
  - Maintains compatibility with latest Portamical.Core features and improvements
  - No breaking changes or API modifications in this release

---

##### **Version 3.2.2 (Current: 2026-06-08)**

**Updated**
- Portamical.Core dependency updated to v3.2.2
  - Maintains compatibility with latest Portamical.Core features and improvements
  - No breaking changes or API modifications in this release

---

##### **Version 3.3.0 (Current: 2026-06-12)**

**Updated**
- Portamical.Core dependency updated to v3.3.0
  - Maintains compatibility with latest Portamical.Core features and improvements
  - No breaking changes or API modifications in this release

---

### **Version 2.0.0 (2026-03-16)**

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

##### **Version 2.0.1 (2026-03-20)**

**Documentation Update**
- Breaking changes description corrected

---

##### **Version 2.0.2 (2026-04-02)**

**Changed**
- Updated Portamical.Core dependency: 2.0.0 → 2.0.1

---

#### **Version 2.1.0 (2026-04-20)**

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

##### **Version 2.1.1 (2026-04-21)**

**Added**
- `GetNotExpectedValueMessage()` protected helper method

---

#### **Version 2.2.0 (2026-04-22)**

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

##### **Version 2.2.1 (2026-04-23)**

**Changed**
- Portamical.Core dependency 2.0.1 → 2.2.0

---

#### **Version 2.3.0 (2026-04-25)**

***Async-First Architecture Completed***

**Added**
- `CatchExceptionAsync(Func<Task>)` - Async exception catcher with fatal exception filtering
  - Zero-allocation success path using `ValueTask<Exception?>`
  - Uses `ConfigureAwait(false)` for thread safety
  - Comprehensive XML documentation with fatal exception handling examples
- `DoesNotThrowAsync(Func<Task>, Func<string, ValueTask>)` - Async action overload
  - Supports async operations in assertion scenarios
  - Delegates to `CatchExceptionAsync` for exception handling
  - Completes synchronously with zero allocation when no exception occurs

**Changed**
- `CatchException(Action)` - Refactored as sync wrapper delegating to `CatchExceptionAsync`
  - Backward compatible - identical signature and behavior
  - Consistent architecture - follows async-first pattern
  - DRY principle - exception catching logic centralized
- `DoesNotThrow(Action, Action<string>)` - Refactored as sync wrapper
  - Backward compatible - identical signature and behavior
  - Eliminated `#pragma warning disable CA2012`
  - Uses `ThreadSafeSyncAssertion` pattern consistently
- Async assertion methods visibility - Changed from `protected` to `public`
  - Affected: `DoesNotThrowAsync`, `ThrowsDetailsAsync`, `EqualityAsync`, `IsTypeOfAsync`
  - Non-breaking change (widening access)
  - Enables direct usage in async frameworks (TUnit, MSTest 4)
  - Framework adapters can still provide simplified convenience APIs

**Improved**
- Documentation - Comprehensive XML docs added for:
  - `IsNotFatal` - 98 lines explaining fatal exception classification, usage patterns, and examples
  - `CatchExceptionAsync` - Complete async exception handling documentation
  - `DoesNotThrowAsync` - Async testing patterns with examples
  - All new and refactored methods include detailed remarks and examples
- Architecture - Completed async-first refactoring pattern across all assertion methods
- Thread Safety - All sync wrappers use `ConfigureAwait(false)` via `ThreadSafeSyncAssertion`
- Fatal Exception Filtering - Consistent application of `IsNotFatal` across sync and async paths
  - Fatal exceptions (`OutOfMemoryException`, `AccessViolationException`, `StackOverflowException`, `ThreadAbortException`) propagate immediately

**Technical Details**
- Zero-allocation success paths preserved in all refactored methods
- Performance characteristics unchanged (aggressive inlining maintained)
- Sync/async parity achieved for exception handling operations

**Dependencies**
- Portamical.Core: 2.2.0 (unchanged)

**Breaking Changes**
- None - fully backward compatible with 2.2.x

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

---
