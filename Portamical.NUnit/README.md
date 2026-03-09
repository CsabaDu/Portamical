# Portamical.NUnit

**NUnit 4 adapter for Portamical: Universal, identity-driven test data modeling for .NET.**

Portamical.NUnit bridges **Portamical.Core** test data to **NUnit 4**, enabling strongly-typed, reusable test data with automatic deduplication and self-documenting test names.

---

## Install

```bash
dotnet add package Portamical.NUnit
```

> **Prerequisites:**  
> - NUnit 4.4.0+  
> - .NET 10.0

---

## Example

### 1. Create Test Data (Framework-Agnostic)

```csharp
using static Portamical.Core.Factories.TestDataFactory;

public class CalculatorDataSource
{
    public static IEnumerable<TestData<int, int>> AddCases()
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

### 2. Consume in NUnit Tests

#### **Option A: Shared Style (Framework-Agnostic)**

```csharp
using Portamical.TestBases.ObjectArrayCollection;

public class CalculatorTests : TestBase
{
    private static IEnumerable<object?[]> AddArgs
        => Convert(CalculatorDataSource.AddCases());

    [Test, TestCaseSource(nameof(AddArgs))]
    public void Add_validInputs_returnsSum(TestData<int, int> testData)
    {
        // Arrange
        var sut = new Calculator();

        // Act
        var actual = sut.Add(testData.Arg1, testData.Arg2);

        // Assert
        Assert.That(actual, Is.EqualTo(5)); // (example expected)
    }
}
```

#### **Option B: Native Style (NUnit-Specific)**

```csharp
using Portamical.NUnit.Attributes;
using Portamical.NUnit.TestBases;

public class CalculatorTests : TestBase
{
    private static IReadOnlyCollection<TestCaseData> AddArgs
        => Convert(CalculatorDataSource.AddCases());

    [Test, PortamicalData(nameof(AddArgs))]
    public void Add_validInputs_returnsSum(TestData<int, int> testData)
    {
        // Arrange
        var sut = new Calculator();

        // Act
        var actual = sut.Add(testData.Arg1, testData.Arg2);

        // Assert
        Assert.That(actual, Is.EqualTo(5));
    }
}
```

---

## What's Included

### Converters

Transform test data collections into NUnit's `TestCaseData`:

```csharp
using Portamical.NUnit.Converters;

var testCases = testDataCollection.ToTestCaseTestDataCollection(
    ArgsCode.Instance,
    testMethodName: nameof(MyTest));
```

### Attributes

**`PortamicalDataAttribute`** — NUnit-native attribute for test data:

```csharp
[Test, PortamicalData(nameof(Args))]
public void MyTest(TestData<int> testData) { ... }
```

Automatically:
- ✅ Sets test case names to `"definition => result"`
- ✅ Deduplicates test cases via identity
- ✅ Wraps test data in NUnit's `TestCaseData`

### TestBases

Abstract base classes with `Convert()` methods:

```csharp
using Portamical.NUnit.TestBases;

public class MyTests : TestBase
{
    protected static IReadOnlyCollection<TestCaseData> Args
        => Convert(dataSource.GetArgs());
}
```

### Assertions

NUnit-specific assertion helpers:

```csharp
using Portamical.NUnit.Assertions;

PortamicalAssert.ThrowsDetails(
    attempt: () => Sut.Method(null),
    expected: new ArgumentNullException("paramName"));
```

Validates:
- ✅ Exception type
- ✅ Exception message
- ✅ Parameter name (for `ArgumentException`)

---

## Data Strategies

### Strategy 1: TestData Mode (Pass Entire Object)

```csharp
// Data source
private static IReadOnlyCollection<TestCaseData> Args
    => Convert(dataSource.GetArgs());  // ← ArgsCode.Instance (default)

// Test signature
[Test, PortamicalData(nameof(Args))]
public void Test(TestData<int> testData)  // ← Receives object
{
    var actual = Sut.Method(testData.Arg1);
    Assert.That(actual, Is.EqualTo(expected));
}
```

**Best for:** Tests needing access to `TestCaseName` or full test data object.

---

### Strategy 2: Properties Mode (Flatten Parameters)

```csharp
// Data source
private static IReadOnlyCollection<TestCaseData> Args
    => Convert(dataSource.GetArgs(), AsProperties);  // ← ArgsCode.Properties

// Test signature
[Test, PortamicalData(nameof(Args))]
public void Test(int arg1)  // ← Receives flattened parameter
{
    var actual = Sut.Method(arg1);
    Assert.That(actual, Is.EqualTo(expected));
}
```

**Best for:** Tests preferring traditional parameter signatures.

---

### Strategy 3: Return-Value Tests with `TestDataReturns`

```csharp
using static Portamical.Core.Factories.TestDataFactory;

public class DataSource
{
    public static IEnumerable<TestDataReturns<int, int, int>> AddCases()
    {
        yield return CreateTestDataReturns(
            definition: "adding 2 and 3",
            expected: 5,
            arg1: 2,
            arg2: 3);
    }
}

// Test
private static IReadOnlyCollection<TestCaseData> Args
    => Convert(DataSource.AddCases(), AsProperties);

[Test, PortamicalData(nameof(Args))]
public void Add_validInputs_returnsExpected(int arg1, int arg2, int expected)
{
    var actual = new Calculator().Add(arg1, arg2);
    Assert.That(actual, Is.EqualTo(expected));
}
```

---

### Strategy 4: Exception Tests with `TestDataThrows`

```csharp
using static Portamical.Core.Factories.TestDataFactory;

public class DataSource
{
    public static IEnumerable<TestDataThrows<ArgumentNullException, string>> NullArgCases()
    {
        yield return CreateTestDataThrows(
            definition: "name is null",
            expected: new ArgumentNullException("name"),
            arg1: (string?)null);
    }
}

// Test
private static IReadOnlyCollection<TestCaseData> Args
    => Convert(DataSource.NullArgCases());

[Test, PortamicalData(nameof(Args))]
public void Constructor_nullName_throwsArgumentNullException(
    TestDataThrows<ArgumentNullException, string> testData)
{
    // Arrange
    var expected = testData.Expected;
    var name = testData.Arg1;

    // Act & Assert
    PortamicalAssert.ThrowsDetails(
        attempt: () => new SomeClass(name),
        expected: expected);
}
```

---

## Shared vs. Native Styles

### Shared Style (Framework-Agnostic)

**Use:** `Portamical.TestBases.ObjectArrayCollection.TestBase`

**Benefits:**
- ✅ Same code works in xUnit, MSTest, NUnit
- ✅ Easy migration between frameworks
- ✅ Uses standard `[TestCaseSource]` attribute

**Example:**

```csharp
using Portamical.TestBases.ObjectArrayCollection;

public class MyTests : TestBase
{
    private static IEnumerable<object?[]> Args
        => Convert(dataSource.GetArgs());

    [Test, TestCaseSource(nameof(Args))]
    public void Test(TestData<int> testData) { ... }
}
```

---

### Native Style (NUnit-Specific)

**Use:** `Portamical.NUnit.TestBases.TestBase`

**Benefits:**
- ✅ NUnit-specific optimizations
- ✅ `PortamicalDataAttribute` with automatic deduplication
- ✅ Better integration with NUnit Test Explorer

**Example:**

```csharp
using Portamical.NUnit.Attributes;
using Portamical.NUnit.TestBases;

public class MyTests : TestBase
{
    private static IReadOnlyCollection<TestCaseData> Args
        => Convert(dataSource.GetArgs());

    [Test, PortamicalData(nameof(Args))]
    public void Test(TestData<int> testData) { ... }
}
```

---

## NUnit Integration Details

### How `PortamicalDataAttribute` Works

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PortamicalDataAttribute : TestCaseSourceAttribute
{
    public PortamicalDataAttribute(string sourceName)
        : base(sourceName)
    {
    }
}
```

**Behavior:**
1. Extends NUnit's `TestCaseSourceAttribute`
2. Reads data from static property/method
3. Expects `IReadOnlyCollection<TestCaseData>`
4. Each `TestCaseData` contains:
   - Test arguments (`object?[]`)
   - Test name (`"definition => result"`)
   - Categories, properties (optional)

---

### TestCaseData Structure

```csharp
var testData = CreateTestData(
    definition: "Valid input",
    result: "creates instance",
    arg1: 42);

var testCaseData = new TestCaseData(testData)
    .SetName("Valid input => creates instance");  // ← Identity-based name
```

**Result in NUnit Test Explorer:**

```
✓ Constructor_validInput_createsInstance(testData: Valid input => creates instance)
```

---

## PropsCode Options

Control which properties are included in test arguments:

| `PropsCode` | Includes | Use Case |
|-------------|----------|----------|
| `TrimTestCaseName` | All properties **except** `TestCaseName` | **Default** — NUnit provides test naming |
| `All` | `TestCaseName` + all properties | Custom scenarios needing explicit names |
| `TrimReturnsExpected` | Also excludes `Expected` (for `IReturns`) | Return-value tests (extract expected separately) |
| `TrimThrowsExpected` | Also excludes `Expected` (for `IThrows`) | Exception tests (extract exception separately) |

**Example:**

```csharp
// Default: Exclude TestCaseName
var args = Convert(dataSource.GetArgs());  // ← TrimTestCaseName

// Include TestCaseName
var args = Convert(dataSource.GetArgs(), AsProperties, WithTestCaseName);

// Exclude Expected for return-value tests
var args = Convert(dataSource.GetReturnsArgs(), AsProperties, TrimReturnsExpected);
```

---

## Use Cases

Install **Portamical.NUnit** if you are:

- ✅ Using NUnit 4.4.0+ for testing
- ✅ Want strongly-typed, reusable test data
- ✅ Need cross-framework test data portability
- ✅ Want self-documenting test names (`"definition => result"`)
- ✅ Need automatic deduplication of test cases

---

## Architecture

```
Your NUnit Tests
    ↓ depends on
Portamical.NUnit (Adapter)
    ├── Converters    → ToTestCaseTestDataCollection()
    ├── Attributes    → PortamicalDataAttribute
    ├── TestBases     → TestBase with Convert() methods
    └── Assertions    → PortamicalAssert
    ↓ depends on
Portamical (Shared Layer)
    ├── Converters    → ToObjectArrayCollection()
    ├── Assertions    → PortamicalAssert base
    └── TestBases     → Framework-agnostic base classes
    ↓ depends on
Portamical.Core (Domain)
    ├── ITestData     → Core abstraction
    ├── TestData<T>   → Test data types
    └── TestDataFactory → Factory methods
```

**Key Principle:** `Portamical.Core` has **zero dependencies** on NUnit.

---

## Links

- **GitHub:** https://github.com/CsabaDu/Portamical
- **Documentation:** https://github.com/CsabaDu/Portamical/blob/master/README.md
- **Issues:** https://github.com/CsabaDu/Portamical/issues
- **Portamical.Core:** https://github.com/CsabaDu/Portamical/tree/master/Portamical.Core

---

## License

MIT

---

## Changelog

### **Version 1.0.0 (2026-03-06)**

- **Initial release**
- NUnit 4.4.0+ integration
- `PortamicalDataAttribute` for test data
- `TestBase` with `Convert()` methods
- NUnit-specific `PortamicalAssert`

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

---