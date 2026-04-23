# ?? **Concise README for NuGet Package**

---

```markdown
# Portamical.NUnit

NUnit 4 adapter for Portamical: Universal, identity-driven test data modeling for .NET.

## Install

```bash
dotnet add package Portamical.NUnit
```

**Requirements:** NUnit 4.4.0+ | .NET 10.0

---

## Quick Start

```csharp
using Portamical.NUnit.TestBases;
using static Portamical.Core.Factories.TestDataFactory;

[TestFixture]
public class CalculatorTests : TestBase
{
    // Define test data
    private static readonly TestDataReturns<int, int, int>[] AddCases =
    [
        CreateTestDataReturns("2 + 3", expected: 5, arg1: 2, arg2: 3),
        CreateTestDataReturns("0 + 0", expected: 0, arg1: 0, arg2: 0)
    ];

    // Convert to NUnit test cases
    private static IReadOnlyCollection<TestCaseData> AddArgs
        => Convert(AddCases, AsProperties);

    // Run parameterized tests
    [Test, PortamicalData(nameof(AddArgs))]
    public void Add_ValidInputs_ReturnsExpected(int arg1, int arg2, int expected)
    {
        var result = new Calculator().Add(arg1, arg2);
        Assert.That(result, Is.EqualTo(expected));
    }
}
```

---

## What's New in 2.1.0

**Enhanced Assertions**
- `ThrowsDetails<TException>()` now uses `CatchException` instead of `Assert.Catch` wrapper (eliminates double-lambda indirection, adds fatal exception filtering)
- `Equality<T>()` - Generic equality with NUnit constraints
- `Equality(object, object?, double?)` - Floating-point tolerance support
- `CollectionEquality<T>()` - Element-wise collection comparison
- Support for 22+ types: primitives, DateTime, Guid, BigInteger, collections
- Special value handling: NaN, ±?, ±0.0

**Examples**

```csharp
using Portamical.NUnit.Assertions;

// Floating-point with tolerance
PortamicalAssert.Equality(0.3, 0.1 + 0.2);  // ? PASSES

// Custom tolerance
PortamicalAssert.Equality(3.14159, Math.PI, floatingPointTolerance: 0.001);

// Collection comparison
PortamicalAssert.CollectionEquality(
    expected: new[] { 1, 2, 3 },
    actual: service.GetNumbers());

// Special values
PortamicalAssert.Equality(float.NaN, float.NaN);  // ? PASSES
```

### Exception Validation

Comprehensive exception testing with type and metadata validation:

```csharp
[Test]
public void Test_ExceptionDetails()
{
    var expected = new ArgumentException("Value must be positive", "amount");
    
    // Validates exception type, message, AND parameter name
    var actual = PortamicalAssert.ThrowsDetails(
        attempt: () => BankAccount.Withdraw(-100),
        expected: expected);
    
    // Returns actual exception for further assertions
    Assert.That(actual.ParamName, Is.EqualTo("amount"));
}
```

---

## Features

### Test Data Management
- ? **Identity-driven:** Unique test cases via `TestCaseName`
- ? **Automatic deduplication:** Remove duplicate test cases
- ? **Self-documenting:** Test names = `"definition => result"`
- ? **Type-safe:** Strongly-typed test data

### NUnit Integration
- ? **`[PortamicalData]` attribute:** Drop-in replacement for `[TestCaseSource]`
- ? **`TestBase` classes:** Convenient `Convert()` methods
- ? **Enhanced assertions:** Floating-point, collections, exceptions
- ? **Multiple assertion modes:** `AssertMultiple()`, `AssertMultipleAsync()`

### Cross-Framework
- ? **Framework-agnostic core:** Reuse test data across NUnit, xUnit, MSTest
- ? **Shared/Native styles:** Choose portability or NUnit-specific features
- ? **Zero coupling:** `Portamical.Core` has no testing framework dependencies

---

## Data Strategies

### Instance Mode (Shared Style - Default)

```csharp
// Pass entire test data object
private static IReadOnlyCollection<TestCaseData> Args
    => Convert(dataSource.GetCases());  // ArgsCode.Instance

[Test, PortamicalData(nameof(Args))]
public void Test(TestData<int> testData)  // Receives object
{
    var result = Sut.Method(testData.Arg1);
    Assert.That(result, Is.EqualTo(expected));
}
```

### Properties Mode (Native Style)

```csharp
// Flatten to individual parameters
private static IReadOnlyCollection<TestCaseData> Args
    => Convert(dataSource.GetCases(), AsProperties);  // ArgsCode.Properties

[Test, PortamicalData(nameof(Args))]
public void Test(int arg1, int arg2)  // Receives flattened parameters
{
    var result = Sut.Method(arg1, arg2);
    Assert.That(result, Is.EqualTo(expected));
}
```

---

## Exception Testing

```csharp
using static Portamical.Core.Factories.TestDataFactory;

// Define exception test data
private static readonly TestDataThrows<ArgumentNullException, string>[] NullCases =
[
    CreateTestDataThrows(
        definition: "null input",
        expected: new ArgumentNullException("name"),
        arg1: (string?)null)
];

private static IReadOnlyCollection<TestCaseData> Args => Convert(NullCases);

[Test, PortamicalData(nameof(Args))]
public void Constructor_NullInput_ThrowsArgumentNullException(
    TestDataThrows<ArgumentNullException, string> testData)
{
    PortamicalAssert.ThrowsDetails(
        attempt: () => new SomeClass(testData.Arg1),
        expected: testData.Expected);
}
```

---

## Supported Types (22+)

| Category | Types |
|----------|-------|
| **Integers** | byte, sbyte, short, ushort, int, uint, long, ulong, nint, nuint |
| **Floating-point** | float, double (with tolerance) |
| **Primitives** | bool, char, string, decimal |
| **Framework** | Guid, DateTime, DateOnly, TimeOnly, TimeSpan, DateTimeOffset |
| **Numerics** | BigInteger |
| **Collections** | Any IEnumerable (recursive) |

---

## Architecture

```
Your Tests
    ?
Portamical.NUnit (NUnit 4 Adapter)
    ??? Assertions: PortamicalAssert
    ??? Attributes: PortamicalDataAttribute
    ??? Converters: ToTestCaseDataCollection()
    ??? TestBases: TestBase with Convert()
    ?
Portamical (Shared Layer)
    ??? Base assertions
    ??? Converters
    ??? Framework-agnostic TestBase
    ?
Portamical.Core (Domain - Zero Dependencies)
    ??? ITestData abstraction
    ??? TestData<T> types
    ??? TestDataFactory
```

---

## Links

- **GitHub:** https://github.com/CsabaDu/Portamical
- **Documentation:** https://github.com/CsabaDu/Portamical/blob/master/README.md
- **Issues:** https://github.com/CsabaDu/Portamical/issues

---

## License

MIT License - See [LICENSE.txt](https://github.com/CsabaDu/Portamical/blob/master/LICENSE.txt)

**Project Lineage:** Successor to `CsabaDu.DynamicTestData.NUnit` (legacy)

---

## Changelog

### [2.0.0] - 2026-03-20

- **Breaking:** Removed `IDisposable` from `TestBase`
- **Breaking:** Removed `ArgsCode` property setter
- **Added:** `ConvertAsInstance()` helper methods
- **Added:** 690+ lines of XML documentation

---

##### [2.0.1] - 2026-04-03

- **Changed:** Portamical dependency 2.0.0 ? 2.0.2

---

#### [2.1.0] - 2026-04-21

- **Added:**
  - Enhanced equality methods with floating-point tolerance
  - Collection equality with recursive comparison
  - Support for 22+ built-in types
- **Fixed**
  - Floating-point precision issues (0.1 + 0.2 now equals 0.3)
  - Collection comparison with nested structures
  - Exception message comparison for framework-generated messages
- **Exception capture in `ThrowsDetails<TException>()`**
  - Changed from `Assert.Catch` lambda wrapper to direct `CatchException` method
  - Fixes double-lambda indirection that prevented proper exception capture
  - Adds fatal exception filtering (OutOfMemoryException, StackOverflowException, etc.)
  - Aligns with MSTest/xUnit adapter implementations
- **Changed:**
  - Portamical dependency 2.0.2 ? 2.1.1

---

### [1.0.0] - 2026-03-06
- Initial release

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*
```

---
