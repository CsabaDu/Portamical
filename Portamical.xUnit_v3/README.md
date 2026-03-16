# Portamical.xUnit_v3

**xUnit v3 adapter for Portamical: Universal, identity-driven test data modeling for .NET.**

Portamical.xUnit_v3 bridges **Portamical.Core** test data to **xUnit v3** (3.2.2+), enabling strongly-typed, reusable test data with automatic deduplication and self-documenting test names.

---

## Install

```bash
dotnet add package Portamical.xUnit_v3
```

> **Prerequisites:**  
> - xunit.v3 3.2.2+  
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

### 2. Consume in xUnit v3 Tests

#### **Option A: Shared Style (Framework-Agnostic)**

```csharp
using Portamical.TestBases.TestDataCollection;

public class CalculatorTests : TestBase
{
    public static IEnumerable<TestData<int, int>> AddArgs
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

#### **Option B: Native Style (xUnit v3-Specific)**

```csharp
using Portamical.xUnit_v3.Attributes;
using Portamical.xUnit_v3.TestBases;

public class CalculatorTests : TestBase
{
    public static TheoryTestData<TestData<int, int>> AddArgs
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

Transform test data collections into xUnit v3's `TheoryTestData<T>`:

```csharp
using Portamical.xUnit_v3.Converters;

var theoryData = testDataCollection.ToTheoryTestData(
    ArgsCode.Instance,
    testMethodName: nameof(MyTest));
```

### Data Providers

**`TheoryTestData<TTestData>`** — xUnit v3's native type-safe wrapper:

```csharp
public class TheoryTestData<TTestData> : TheoryData<TTestData>
where TTestData : notnull, ITestData
{
    // Inherits from xUnit v3's TheoryData<T>
    // Adds automatic deduplication via ITheoryTestDataRow
}
```

Features:
- ✅ **xUnit v3 native** — Extends `TheoryData<T>`
- ✅ **Automatic deduplication** — Uses `ITheoryTestDataRow` identity
- ✅ **Self-documenting names** — Via `TestDisplayName` property
- ✅ **Type safety** — Generic `TTestData` instead of raw `object?[]`

### Theory Data Rows

**`TheoryTestDataRow`** — Implements `ITheoryTestDataRow`:

```csharp
public interface ITheoryTestDataRow : ITheoryDataRow, INamedCase
{
    string? TestDisplayName { get; init; }
}

public class TheoryTestDataRow : ITheoryTestDataRow
{
    public string TestCaseName { get; init; }
    public string? TestDisplayName { get; init; }
    public object?[] GetData() => ...;
}
```

**Key Features:**
- ✅ **Identity-based equality** — Via `TestCaseName`
- ✅ **Custom display names** — Via `TestDisplayName`
- ✅ **xUnit v3 integration** — Implements `ITheoryDataRow`

### Attributes

**`PortamicalDataAttribute`** — xUnit v3-native attribute for test data:

```csharp
[Theory, PortamicalData(nameof(Args))]
public void MyTest(TestData<int> testData) { ... }
```

Automatically:
- ✅ Reads data from `TheoryTestData<T>` or `IEnumerable<ITheoryDataRow>`
- ✅ Sets test case names to `"definition => result"`
- ✅ Deduplicates test cases via `ITheoryTestDataRow` identity
- ✅ Extracts `TestDisplayName` for test runner

### TestBases

Abstract base classes with `Convert()` methods:

```csharp
using Portamical.xUnit_v3.TestBases;

public class MyTests : TestBase
{
    protected static TheoryTestData<TestData<int>> Args
        => Convert(dataSource.GetArgs());
}
```

### Assertions

xUnit v3-specific assertion helpers:

```csharp
using Portamical.xUnit_v3.Assertions;

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
public static TheoryTestData<TestData<int>> Args
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
public static TheoryTestData<TestData<int>> Args
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
public static TheoryTestData<TestDataReturns<int, int, int>> Args
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
public static TheoryTestData<TestDataThrows<ArgumentNullException, string>> Args
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

**Use:** `Portamical.TestBases.TestDataCollection.TestBase`

**Benefits:**
- ✅ Same code works in xUnit v2, xUnit v3, MSTest, NUnit
- ✅ Easy migration between frameworks
- ✅ Uses standard `[MemberData]` attribute
- ✅ Returns `IEnumerable<TTestData>`

**Example:**

```csharp
using Portamical.TestBases.TestDataCollection;

public class MyTests : TestBase
{
    public static IEnumerable<TestData<int>> Args
        => Convert(dataSource.GetArgs());

    [Theory, MemberData(nameof(Args))]
    public void Test(TestData<int> testData) { ... }
}
```

---

### Native Style (xUnit v3-Specific)

**Use:** `Portamical.xUnit_v3.TestBases.TestBase`

**Benefits:**
- ✅ xUnit v3-specific optimizations
- ✅ Type-safe `TheoryTestData<T>` (extends xUnit v3's `TheoryData<T>`)
- ✅ `PortamicalDataAttribute` with automatic deduplication
- ✅ Better integration with xUnit v3 Test Explorer
- ✅ `TestDisplayName` for custom test names

**Example:**

```csharp
using Portamical.xUnit_v3.Attributes;
using Portamical.xUnit_v3.TestBases;

public class MyTests : TestBase
{
    public static TheoryTestData<TestData<int>> Args
        => Convert(dataSource.GetArgs());

    [Theory, PortamicalData(nameof(Args))]
    public void Test(TestData<int> testData) { ... }
}
```

---

## xUnit v3 Integration Details

### How `TheoryTestData<T>` Works

```csharp
public class TheoryTestData<TTestData> : TheoryData<TTestData>
where TTestData : notnull, ITestData
{
    private readonly HashSet<INamedCase> _namedCases = new(NamedCase.Comparer);
    
    public override void Add(TTestData testData)
    {
        if (_namedCases.Add(testData))  // ← Deduplication via identity
        {
            base.Add(testData);  // ← Adds to xUnit v3's TheoryData<T>
        }
    }
}
```

**Key Features:**
1. **xUnit v3 native** — Extends `TheoryData<T>` (not a wrapper)
2. **Automatic deduplication** — `HashSet<INamedCase>` with identity comparer
3. **Type-safe** — Generic `TTestData` instead of `object?[]`
4. **Lazy evaluation** — Inherits from xUnit v3's collection

---

### How `ITheoryTestDataRow` Works

```csharp
public interface ITheoryTestDataRow : ITheoryDataRow, INamedCase
{
    string? TestDisplayName { get; init; }
}

public class TheoryTestDataRow : ITheoryTestDataRow
{
    private readonly ITestData _testData;
    private readonly ArgsCode _argsCode;
    private readonly PropsCode _propsCode;
    
    public string TestCaseName { get; init; }  // ← From INamedCase
    public string? TestDisplayName { get; init; }  // ← Custom display name
    
    public object?[] GetData()  // ← From ITheoryDataRow
    => _testData.ToArgs(_argsCode, _propsCode);
    
    public bool Equals(INamedCase? other)  // ← Identity-based equality
    => NamedCase.Comparer.Equals(this, other);
}
```

**Key Features:**
1. **xUnit v3 contract** — Implements `ITheoryDataRow`
2. **Portamical identity** — Implements `INamedCase`
3. **Custom display names** — `TestDisplayName` property
4. **Lazy serialization** — `GetData()` called on demand

---

### How `PortamicalDataAttribute` Works

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PortamicalDataAttribute : MemberDataAttribute
{
    public PortamicalDataAttribute(string memberName)
        : base(memberName)
    {
    }
    
    public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(
        MethodInfo testMethod,
        DisposalTracker disposalTracker)
    {
        // Get data from member
        var data = base.GetData(testMethod, disposalTracker).Result;
        
        if (data is null) return new(data);
        
        // Deduplicate via ITheoryTestDataRow identity
        HashSet<ITheoryTestDataRow> deduplicatedRows = new(NamedCase.Comparer);
        
        foreach (var row in data)
        {
            if (row is ITheoryTestDataRow ttdr)
            {
                // Set TestDisplayName if not set
                if (string.IsNullOrEmpty(ttdr.TestDisplayName))
                {
                    ttdr = new TheoryTestDataRow(ttdr, testMethod.Name);
                }
                
                deduplicatedRows.Add(ttdr);  // ← Identity-based deduplication
            }
        }
        
        return new(deduplicatedRows.CastOrToReadOnlyCollection());
    }
}
```

**Behavior:**
1. Extends xUnit v3's `MemberDataAttribute`
2. Reads data from static property/method
3. Expects `IEnumerable<ITheoryDataRow>` or `TheoryTestData<T>`
4. Deduplicates via `HashSet<ITheoryTestDataRow>` (identity-based)
5. Sets `TestDisplayName` if not already set
6. Returns deduplicated `IReadOnlyCollection<ITheoryDataRow>`

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

**Self-documenting test names via `TestDisplayName`!**

---

## PropsCode Options

Control which properties are included in test arguments:

| `PropsCode` | Includes | Use Case |
|-------------|----------|----------|
| `TrimTestCaseName` | All properties **except** `TestCaseName` | **Default** — xUnit v3 provides test naming |
| `All` | `TestCaseName` + all properties | Custom scenarios needing explicit names |
| `TrimReturnsExpected` | Also excludes `Expected` (for `IReturns`) | Return-value tests (extract expected separately) |
| `TrimThrowsExpected` | Also excludes `Expected` (for `IThrows`) | Exception tests (extract exception separately) |

**Example:**

```csharp
// Default: Exclude TestCaseName
var theoryData = Convert(dataSource.GetArgs());  // ← TrimTestCaseName

// Include TestCaseName
var theoryData = Convert(dataSource.GetArgs(), AsProperties, WithTestCaseName);

// Exclude Expected for return-value tests
var theoryData = Convert(dataSource.GetReturnsArgs(), AsProperties, TrimReturnsExpected);
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

### Test Class (xUnit v3 Native Style)

```csharp
using Portamical.xUnit_v3.Attributes;
using Portamical.xUnit_v3.Assertions;
using Portamical.xUnit_v3.TestBases;

public class BirthDayTests : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();
    
    // Constructor - Valid Args
    public static TheoryTestData<TestData<DateOnly>> ConstructorValidArgs
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
    public static TheoryTestData<TestDataThrows<ArgumentException, string>> ConstructorInvalidArgs
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
    public static TheoryTestData<TestDataReturns<int, DateOnly, BirthDay>> CompareToArgs
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

**With Portamical xUnit v3:** `TheoryTestData<T>` deduplicates automatically:

```csharp
public override void Add(TTestData testData)
{
    if (_namedCases.Add(testData))  // ← HashSet.Add returns false for duplicate
    {
        base.Add(testData);  // ← Only adds if new identity
    }
}
```

**Result:** Only **one** test case runs, avoiding duplicate execution.

---

## Alternative: TheoryDataRow Collection

xUnit v3 supports `IEnumerable<ITheoryDataRow>` directly:

```csharp
using Portamical.xUnit_v3.Converters;

public static IEnumerable<ITheoryDataRow> Args
    => testDataCollection.ToTheoryDataRowCollection(
        ArgsCode.Instance,
        testMethodName: nameof(MyTest));

[Theory, MemberData(nameof(Args))]
public void MyTest(TestData<int> testData) { ... }
```

**Benefits:**
- ✅ Direct xUnit v3 integration
- ✅ Custom `ITheoryDataRow` implementations
- ✅ More control over serialization

**Note:** Use `TheoryTestData<T>` for simplicity, `ITheoryDataRow` for advanced scenarios.

---

## xUnit v3 Contract/Model Separation

Portamical.xUnit_v3 has the **most complex architecture** due to xUnit v3's contract/model separation:

```
xUnit v3 Architecture:
├── Contracts (Interfaces)
│   ├── ITheoryDataRow          ← xUnit v3 contract
│   └── ITheoryTestDataRow      ← Portamical extension
├── Models (Implementations)
│   ├── TheoryDataRow           ← xUnit v3 model
│   └── TheoryTestDataRow       ← Portamical model
└── Collections
    ├── TheoryData<T>           ← xUnit v3 native
    └── TheoryTestData<T>       ← Portamical extension
```

**Why the complexity?**

xUnit v3 separates contracts (interfaces) from models (implementations) for:
- ✅ **Extensibility** — Custom `ITheoryDataRow` implementations
- ✅ **Testability** — Mock interfaces in tests
- ✅ **Flexibility** — Multiple implementations of same contract

**Result:** 9 namespaces (most complex adapter).

---

## Use Cases

Install **Portamical.xUnit_v3** if you are:

- ✅ Using xUnit v3 (3.2.2+) for testing
- ✅ Want strongly-typed, reusable test data
- ✅ Need cross-framework test data portability
- ✅ Want self-documenting test names (`"definition => result"`)
- ✅ Need automatic deduplication of test cases
- ✅ Prefer xUnit v3's native `TheoryData<T>` over xUnit v2's wrappers
- ✅ Want `ITheoryTestDataRow` identity-based equality

---

## Architecture

```
Your xUnit v3 Tests
    ↓ depends on
Portamical.xUnit_v3 (Adapter)
    ├── Converters              → ToTheoryTestData(), ToTheoryDataRowCollection()
    ├── DataProviders/
    │   ├── Contracts           → ITheoryTestDataRow
    │   └── Model               → TheoryTestDataRow, TheoryTestData<T>
    ├── Attributes              → PortamicalDataAttribute
    ├── TestBases               → TestBase with Convert() methods
    └── Assertions              → PortamicalAssert
    ↓ depends on
Portamical (Shared Layer)
    ├── Converters              → ToDistinctArray()
    ├── Assertions              → PortamicalAssert base
    └── TestBases               → Framework-agnostic base classes
    ↓ depends on
Portamical.Core (Domain)
    ├── ITestData               → Core abstraction
    ├── TestData<T>             → Test data types
    └── TestDataFactory         → Factory methods
```

**Key Principle:** `Portamical.Core` has **zero dependencies** on xUnit v3.

---

## Comparison: xUnit v3 vs. Other Frameworks

| Feature | xUnit v3 | xUnit v2 | MSTest | NUnit |
|---------|----------|----------|--------|-------|
| **Attribute** | `[MemberData]` | `[MemberData]` | `[DynamicData]` | `[TestCaseSource]` |
| **Display Names** | `ITheoryDataRow.TestDisplayName` | Manual | `DynamicDataDisplayName` | `TestName` |
| **Return Type** | `TheoryData<T>` | `IEnumerable<object?[]>` | `IEnumerable<object?[]>` | `IEnumerable<TestCaseData>` |
| **Native Type** | `TheoryTestData<T>` | `TestDataProvider<T>` | `object?[]` | `TestCaseData` |
| **Deduplication** | ✅ `TheoryTestData<T>` | ✅ `TestDataProvider<T>` | Manual | Manual |
| **Type Safety** | ✅ Generic `<T>` | ✅ Generic `<T>` | ❌ Raw `object?[]` | ⚠️ Wrapper |
| **Contract/Model** | ✅ Separated | ❌ Mixed | ❌ N/A | ❌ N/A |
| **Complexity** | ⭐⭐⭐ High | ⭐⭐ Moderate | ⭐ Simple | ⭐⭐ Moderate |

---

## Migration from xUnit v2

If you're migrating from xUnit v2:

**xUnit v2 (Portamical.xUnit):**
```csharp
using Portamical.xUnit.TestBases;

public static TestDataProvider<TestData<int>> Args
    => Convert(dataSource.GetArgs());

[Theory, PortamicalData(nameof(Args))]
public void Test(TestData<int> testData) { ... }
```

**xUnit v3 (Portamical.xUnit_v3):**
```csharp
using Portamical.xUnit_v3.TestBases;

public static TheoryTestData<TestData<int>> Args
    => Convert(dataSource.GetArgs());

[Theory, PortamicalData(nameof(Args))]
public void Test(TestData<int> testData) { ... }
```

**Differences:**
- Namespace: `Portamical.xUnit` → `Portamical.xUnit_v3`
- Type: `TestDataProvider<T>` → `TheoryTestData<T>`
- Test method signature: **Same**
- Attribute: **Same** (`[PortamicalData]`)

**Migration effort:** ~5 minutes per test class (change namespace + data source type).

---

## Links

- **GitHub:** https://github.com/CsabaDu/Portamical
- **Documentation:** https://github.com/CsabaDu/Portamical/blob/master/README.md
- **Issues:** https://github.com/CsabaDu/Portamical/issues
- **Portamical.Core:** https://github.com/CsabaDu/Portamical/tree/master/Portamical.Core
- **xUnit v2 Adapter:** https://github.com/CsabaDu/Portamical/tree/master/Portamical.xUnit

---

## License

MIT

---

## Changelog

### **Version 2.0.0 (2026-03-13)**

**Note:** This version does not introduce breaking changes in Portamical.Core itself. The major version bump to 2.0.0 aligns with the Portamical extension packages, where new versions may introduce rare breaking changes. The version number synchronization ensures consistency across the Portamical ecosystem.

---

### **Version 1.0.0 (2026-03-06)**

- **Initial release**
- xUnit v3 (3.2.2+) integration
- `TheoryTestData<T>` extending xUnit v3's `TheoryData<T>`
- `ITheoryTestDataRow` with identity-based equality
- `PortamicalDataAttribute` with automatic deduplication
- `TestBase` with `Convert()` methods
- xUnit v3-specific `PortamicalAssert`
- Contract/model separation (9 namespaces)

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

---