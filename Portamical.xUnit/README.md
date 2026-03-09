# Portamical.xUnit

**xUnit v2 adapter for Portamical: Universal, identity-driven test data modeling for .NET.**

Portamical.xUnit bridges **Portamical.Core** test data to **xUnit v2**, enabling strongly-typed, reusable test data with automatic deduplication and self-documenting test names.

---

## Install

```bash
dotnet add package Portamical.xUnit
```

> **Prerequisites:**  
> - xunit.core 2.9.3+  
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

### 2. Consume in xUnit Tests

#### **Option A: Shared Style (Framework-Agnostic)**

```csharp
using Portamical.TestBases.ObjectArrayCollection;

public class CalculatorTests : TestBase
{
    public static IEnumerable<object?[]> AddArgs
        => Convert(CalculatorDataSource.AddCases());

    [Theory, MemberData(nameof(AddArgs))]
    public void Add_validInputs_returnsSum(TestData<int, int> testData)
    {
        // Arrange
        var sut = new Calculator();

        // Act
        var actual = sut.Add(testData.Arg1, testData.Arg2);

        // Assert
        Assert.Equal(5, actual); // (example expected)
    }
}
```

#### **Option B: Native Style (xUnit-Specific)**

```csharp
using Portamical.xUnit.Attributes;
using Portamical.xUnit.TestBases;

public class CalculatorTests : TestBase
{
    public static TestDataProvider<TestData<int, int>> AddArgs
        => Convert(CalculatorDataSource.AddCases());

    [Theory, PortamicalData(nameof(AddArgs))]
    public void Add_validInputs_returnsSum(TestData<int, int> testData)
    {
        // Arrange
        var sut = new Calculator();

        // Act
        var actual = sut.Add(testData.Arg1, testData.Arg2);

        // Assert
        Assert.Equal(5, actual);
    }
}
```

---

## What's Included

### Converters

Transform test data collections into xUnit's `TestDataProvider<T>`:

```csharp
using Portamical.xUnit.Converters;

var provider = testDataCollection.ToTestDataProvider(ArgsCode.Instance);
```

### Data Providers

**`TestDataProvider<TTestData>`** — Type-safe wrapper for test data:

```csharp
public class TestDataProvider<TTestData> : DataProviderBase<TTestData>
where TTestData : notnull, ITestData
{
    public void Add(TTestData testData) { ... }
    public override IEnumerator<object?[]> GetEnumerator() { ... }
}
```

Features:
- ✅ **Type safety** — Generic `TTestData` instead of raw `object?[]`
- ✅ **Automatic deduplication** — Uses `HashSet<INamedCase>`
- ✅ **Identity-based equality** — Compares via `TestCaseName`

### Attributes

**`PortamicalDataAttribute`** — xUnit-native attribute for test data:

```csharp
[Theory, PortamicalData(nameof(Args))]
public void MyTest(TestData<int> testData) { ... }
```

Automatically:
- ✅ Reads data from `TestDataProvider<T>`
- ✅ Sets test case names to `"definition => result"`
- ✅ Deduplicates test cases via identity

### TestBases

Abstract base classes with `Convert()` methods:

```csharp
using Portamical.xUnit.TestBases;

public class MyTests : TestBase
{
    protected static TestDataProvider<TestData<int>> Args
        => Convert(dataSource.GetArgs());
}
```

### Assertions

xUnit-specific assertion helpers:

```csharp
using Portamical.xUnit.Assertions;

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
public static TestDataProvider<TestData<int>> Args
    => Convert(dataSource.GetArgs());  // ← ArgsCode.Instance (default)

// Test signature
[Theory, PortamicalData(nameof(Args))]
public void Test(TestData<int> testData)  // ← Receives object
{
    var actual = Sut.Method(testData.Arg1);
    Assert.Equal(expected, actual);
}
```

**Best for:** Tests needing access to `TestCaseName` or full test data object.

---

### Strategy 2: Properties Mode (Flatten Parameters)

```csharp
// Data source
public static TestDataProvider<TestData<int>> Args
    => Convert(dataSource.GetArgs(), AsProperties);  // ← ArgsCode.Properties

// Test signature
[Theory, PortamicalData(nameof(Args))]
public void Test(int arg1)  // ← Receives flattened parameter
{
    var actual = Sut.Method(arg1);
    Assert.Equal(expected, actual);
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
public static TestDataProvider<TestDataReturns<int, int, int>> Args
    => Convert(DataSource.AddCases(), AsProperties);

[Theory, PortamicalData(nameof(Args))]
public void Add_validInputs_returnsExpected(int arg1, int arg2, int expected)
{
    var actual = new Calculator().Add(arg1, arg2);
    Assert.Equal(expected, actual);
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
public static TestDataProvider<TestDataThrows<ArgumentNullException, string>> Args
    => Convert(DataSource.NullArgCases());

[Theory, PortamicalData(nameof(Args))]
public void Constructor_nullName_throwsArgumentNullException(
    TestDataThrows<ArgumentNullException, string> testData)
{
    // Arrange
    var expected = testData.Expected;
    var name = testData.Arg1;

    // Act & Assert
    PortamicalAssert.ThrowsDetails(
        attempt: () => new SomeClass(name!),
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
- ✅ Uses standard `[MemberData]` attribute

**Example:**

```csharp
using Portamical.TestBases.ObjectArrayCollection;

public class MyTests : TestBase
{
    public static IEnumerable<object?[]> Args
        => Convert(dataSource.GetArgs());

    [Theory, MemberData(nameof(Args))]
    public void Test(TestData<int> testData) { ... }
}
```

---

### Native Style (xUnit-Specific)

**Use:** `Portamical.xUnit.TestBases.TestBase`

**Benefits:**
- ✅ Type-safe `TestDataProvider<T>` (not raw `object?[]`)
- ✅ `PortamicalDataAttribute` with automatic deduplication
- ✅ Better IntelliSense support
- ✅ Compile-time type checking

**Example:**

```csharp
using Portamical.xUnit.Attributes;
using Portamical.xUnit.TestBases;

public class MyTests : TestBase
{
    public static TestDataProvider<TestData<int>> Args
        => Convert(dataSource.GetArgs());

    [Theory, PortamicalData(nameof(Args))]
    public void Test(TestData<int> testData) { ... }
}
```

---

## xUnit Integration Details

### How `TestDataProvider<T>` Works

```csharp
public class TestDataProvider<TTestData> : DataProviderBase<TTestData>
where TTestData : notnull, ITestData
{
    private readonly HashSet<INamedCase> _namedCases = new(NamedCase.Comparer);
    private readonly List<TTestData> _testData = [];
    
    public void Add(TTestData testData)
    {
        if (_namedCases.Add(testData))  // ← Deduplication via identity
        {
            _testData.Add(testData);
        }
    }
    
    public override IEnumerator<object?[]> GetEnumerator()
    {
        foreach (var testData in _testData)
        {
            yield return testData.ToArgs(ArgsCode, PropsCode);
        }
    }
}
```

**Key Features:**
1. **Type-safe** — Generic `TTestData` instead of `object?[]`
2. **Automatic deduplication** — `HashSet<INamedCase>` with identity comparer
3. **Lazy evaluation** — Yields test data on demand
4. **Configurable serialization** — Uses `ArgsCode` and `PropsCode`

---

### How `PortamicalDataAttribute` Works

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PortamicalDataAttribute : MemberDataAttributeBase
{
    public PortamicalDataAttribute(string memberName)
        : base(memberName)
    {
    }
    
    protected override object?[]? ConvertDataItem(
        MethodInfo testMethod,
        object? item)
    {
        // Extract TestCaseName for display
        if (item is ITestData testData)
        {
            var displayName = testData.GetDisplayName(testMethod.Name);
            // Store display name for test runner
        }
        
        return base.ConvertDataItem(testMethod, item);
    }
}
```

**Behavior:**
1. Extends xUnit's `MemberDataAttributeBase`
2. Reads data from static property/method
3. Expects `TestDataProvider<TTestData>`
4. Extracts `TestCaseName` for display names
5. Supports both `TestData` mode and `Properties` mode

---

### Test Explorer Output

**Without `PortamicalDataAttribute`:**

```
✓ Add_validInputs_returnsSum (testData: TestData<Int32, Int32>)
✓ Add_validInputs_returnsSum (testData: TestData<Int32, Int32>)
```

**With `PortamicalDataAttribute`:**

```
✓ Add_validInputs_returnsSum(testData: adding two positive numbers => returns their sum)
✓ Add_validInputs_returnsSum(testData: adding with zero => returns the other number)
```

**Self-documenting test names!**

---

## PropsCode Options

Control which properties are included in test arguments:

| `PropsCode` | Includes | Use Case |
|-------------|----------|----------|
| `TrimTestCaseName` | All properties **except** `TestCaseName` | **Default** — xUnit provides test naming |
| `All` | `TestCaseName` + all properties | Custom scenarios needing explicit names |
| `TrimReturnsExpected` | Also excludes `Expected` (for `IReturns`) | Return-value tests (extract expected separately) |
| `TrimThrowsExpected` | Also excludes `Expected` (for `IThrows`) | Exception tests (extract exception separately) |

**Example:**

```csharp
// Default: Exclude TestCaseName
var provider = Convert(dataSource.GetArgs());  // ← TrimTestCaseName

// Include TestCaseName
var provider = Convert(dataSource.GetArgs(), AsProperties, WithTestCaseName);

// Exclude Expected for return-value tests
var provider = Convert(dataSource.GetReturnsArgs(), AsProperties, TrimReturnsExpected);
```

---

## Complete Example: BirthDay Class Tests

### Data Source (Framework-Agnostic)

```csharp
using static Portamical.Core.Factories.TestDataFactory;

public class BirthDayDataSource
{
    public static IEnumerable<TestData<DateOnly>> GetConstructorValidArgs()
    {
        const string result = "creates BirthDay instance";
        
        string definition = "Valid name and dateOfBirth is today";
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now);
        yield return createTestData();
        
        definition = "Valid name and dateOfBirth is in the past";
        dateOfBirth = dateOfBirth.AddDays(-1);
        yield return createTestData();
        
        #region Local Methods
        TestData<DateOnly> createTestData()
        => CreateTestData(definition, result, dateOfBirth);
        #endregion
    }
    
    public static IEnumerable<TestDataThrows<ArgumentException, string>> GetConstructorInvalidArgs()
    {
        yield return CreateTestDataThrows(
            definition: "name is null",
            expected: new ArgumentNullException("name"),
            arg1: (string?)null);
        
        yield return CreateTestDataThrows(
            definition: "name is empty",
            expected: new ArgumentException("Value cannot be empty.", "name"),
            arg1: string.Empty);
    }
    
    public static IEnumerable<TestDataReturns<int, DateOnly, BirthDay>> GetCompareToArgs()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        
        yield return CreateTestDataReturns(
            definition: "other is null",
            expected: -1,
            arg1: today,
            arg2: (BirthDay?)null);
        
        yield return CreateTestDataReturns(
            definition: "other has earlier dateOfBirth",
            expected: 1,
            arg1: today,
            arg2: new BirthDay("John", today.AddDays(-1)));
    }
}
```

### Test Class (xUnit Native Style)

```csharp
using Portamical.xUnit.Attributes;
using Portamical.xUnit.Assertions;
using Portamical.xUnit.TestBases;

public class BirthDayTests : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();
    
    // Constructor - Valid Args
    public static TestDataProvider<TestData<DateOnly>> ConstructorValidArgs
        => Convert(_dataSource.GetConstructorValidArgs());
    
    [Theory, PortamicalData(nameof(ConstructorValidArgs))]
    public void Constructor_validArgs_createsInstance(TestData<DateOnly> testData)
    {
        // Arrange
        const string name = "John Doe";
        var dateOfBirth = testData.Arg1;
        
        // Act
        var actual = new BirthDay(name, dateOfBirth);
        
        // Assert
        Assert.NotNull(actual);
        Assert.Equal(name, actual.Name);
        Assert.Equal(dateOfBirth, actual.DateOfBirth);
    }
    
    // Constructor - Invalid Args
    public static TestDataProvider<TestDataThrows<ArgumentException, string>> ConstructorInvalidArgs
        => Convert(_dataSource.GetConstructorInvalidArgs());
    
    [Theory, PortamicalData(nameof(ConstructorInvalidArgs))]
    public void Constructor_invalidArgs_throwsArgumentException(
        TestDataThrows<ArgumentException, string> testData)
    {
        // Arrange
        var name = testData.Arg1;
        var dateOfBirth = DateOnly.FromDateTime(DateTime.Now);
        var expected = testData.Expected;
        
        // Act & Assert
        PortamicalAssert.ThrowsDetails(
            attempt: () => new BirthDay(name!, dateOfBirth),
            expected: expected);
    }
    
    // CompareTo
    public static TestDataProvider<TestDataReturns<int, DateOnly, BirthDay>> CompareToArgs
        => Convert(_dataSource.GetCompareToArgs());
    
    [Theory, PortamicalData(nameof(CompareToArgs))]
    public void CompareTo_validArgs_returnsExpected(
        TestDataReturns<int, DateOnly, BirthDay> testData)
    {
        // Arrange
        const string name = "John Doe";
        var dateOfBirth = testData.Arg1;
        var other = testData.Arg2;
        var expected = testData.Expected;
        var sut = new BirthDay(name, dateOfBirth);
        
        // Act
        var actual = sut.CompareTo(other);
        
        // Assert
        Assert.Equal(expected, actual);
    }
}
```

---

## Deduplication in Action

### Problem: Duplicate Test Cases

```csharp
public static IEnumerable<TestData<int>> GetArgs()
{
    // Accidentally added twice (same definition + result)
    yield return CreateTestData("Valid input", "succeeds", 42);
    yield return CreateTestData("Valid input", "succeeds", 99);
    //                           ↑ Same identity!
}
```

**Without Portamical:** Both test cases run (duplicate waste).

**With Portamical:** `TestDataProvider<T>` deduplicates automatically:

```csharp
public void Add(TTestData testData)
{
    if (_namedCases.Add(testData))  // ← HashSet.Add returns false for duplicate
    {
        _testData.Add(testData);  // ← Only adds if new identity
    }
}
```

**Result:** Only **one** test case runs, avoiding duplicate execution.

---

## Alternative: Using `TheoryData<T>`

xUnit v2 supports `TheoryData<T>` for type-safe test data:

```csharp
using Portamical.xUnit.Converters;

public static TheoryData<TestData<int>> Args
{
    get
    {
        var theoryData = new TheoryData<TestData<int>>();
        foreach (var testData in dataSource.GetArgs())
        {
            theoryData.Add(testData);
        }
        return theoryData;
    }
}

[Theory, MemberData(nameof(Args))]
public void Test(TestData<int> testData) { ... }
```

**Note:** `TheoryData<T>` does **not** provide automatic deduplication. Use `TestDataProvider<T>` for that.

---

## Use Cases

Install **Portamical.xUnit** if you are:

- ✅ Using xUnit v2 (2.9.3+) for testing
- ✅ Want strongly-typed, reusable test data
- ✅ Need cross-framework test data portability
- ✅ Want self-documenting test names (`"definition => result"`)
- ✅ Need automatic deduplication of test cases
- ✅ Prefer type-safe `TestDataProvider<T>` over `object?[]`

---

## Architecture

```
Your xUnit Tests
    ↓ depends on
Portamical.xUnit (Adapter)
    ├── Converters         → ToTestDataProvider()
    ├── DataProviders      → TestDataProvider<T>
    ├── Attributes         → PortamicalDataAttribute
    ├── TestBases          → TestBase with Convert() methods
    └── Assertions         → PortamicalAssert
    ↓ depends on
Portamical (Shared Layer)
    ├── Converters         → ToObjectArrayCollection()
    ├── Assertions         → PortamicalAssert base
    └── TestBases          → Framework-agnostic base classes
    ↓ depends on
Portamical.Core (Domain)
    ├── ITestData          → Core abstraction
    ├── TestData<T>        → Test data types
    └── TestDataFactory    → Factory methods
```

**Key Principle:** `Portamical.Core` has **zero dependencies** on xUnit.

---

## Comparison: xUnit v2 vs. Other Frameworks

| Feature | xUnit v2 | xUnit v3 | MSTest | NUnit |
|---------|----------|----------|--------|-------|
| **Attribute** | `[MemberData]` | `[MemberData]` | `[DynamicData]` | `[TestCaseSource]` |
| **Display Names** | Manual `DisplayName` | `ITheoryDataRow.TestDisplayName` | `DynamicDataDisplayName` | `TestName` |
| **Return Type** | `IEnumerable<object?[]>` | `TheoryData<T>` | `IEnumerable<object?[]>` | `IEnumerable<TestCaseData>` |
| **Native Type** | `TestDataProvider<T>` | `TheoryTestData<T>` | `object?[]` | `TestCaseData` |
| **Deduplication** | ✅ `TestDataProvider<T>` | ✅ `TheoryTestData<T>` | Manual | Manual |
| **Type Safety** | ✅ Generic `<T>` | ✅ Generic `<T>` | ❌ Raw `object?[]` | ⚠️ `TestCaseData` |

---

## Migration to xUnit v3

If you're considering migrating to xUnit v3:

**xUnit v2 (current):**
```csharp
public static TestDataProvider<TestData<int>> Args
    => Convert(dataSource.GetArgs());

[Theory, PortamicalData(nameof(Args))]
public void Test(TestData<int> testData) { ... }
```

**xUnit v3 (Portamical.xUnit_v3):**
```csharp
public static TheoryTestData<TestData<int>> Args
    => Convert(dataSource.GetArgs());

[Theory, PortamicalData(nameof(Args))]
public void Test(TestData<int> testData) { ... }
```

**Differences:**
- `TestDataProvider<T>` → `TheoryTestData<T>`
- Same test method signature
- Same `[PortamicalData]` attribute

**Migration effort:** ~5 minutes per test class (just change data source return type).

---

## Links

- **GitHub:** https://github.com/CsabaDu/Portamical
- **Documentation:** https://github.com/CsabaDu/Portamical/blob/master/README.md
- **Issues:** https://github.com/CsabaDu/Portamical/issues
- **Portamical.Core:** https://github.com/CsabaDu/Portamical/tree/master/Portamical.Core
- **xUnit v3 Adapter:** https://github.com/CsabaDu/Portamical/tree/master/Portamical.xUnit_v3

---

## License

MIT

---

## Changelog

### **Version 1.0.0 (2026-03-06)**

- **Initial release**
- xUnit v2 (2.9.3+) integration
- `TestDataProvider<T>` with automatic deduplication
- `PortamicalDataAttribute` for test data
- `TestBase` with `Convert()` methods
- xUnit-specific `PortamicalAssert`

### **Version 1.0.1 (2026-03-07)**

- **xunit.runner.json relocation**
  - Moved from `Portamical` package to `Portamical.xUnit`
  - Improves package separation and reduces unnecessary files

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

---