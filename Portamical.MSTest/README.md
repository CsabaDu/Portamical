# Portamical.MSTest

**MSTest 4 adapter for Portamical: Universal, identity-driven test data modeling for .NET.**

Portamical.MSTest bridges **Portamical.Core** test data to **MSTest 4**, enabling strongly-typed, reusable test data with automatic deduplication and self-documenting test names.

---

## Portamical.MSTest [2.0.0] - 2026-03-19

### ⚠️ BREAKING CHANGES

**Portamical.TestBases.TestBase** (inherited from Portamical 2.0.0)

**Removed IDisposable Implementation**
```csharp
// BEFORE (v1):
public abstract class TestBase : IDisposable
{
    protected static ArgsCode ArgsCode { get; set; } = AsInstance;
    
    protected static long ResetLogCounter()
        => Resolver.ResetLogCounter();
    
    public void Dispose() { ... }
    protected virtual void Dispose(bool disposing) { ... }
}

// AFTER (v2):
public abstract class TestBase  // ❌ No IDisposable
{
    // ❌ Removed: ArgsCode property with setter
    // ❌ Removed: ResetLogCounter() method
    // ❌ Removed: Dispose() methods
    
    protected static ArgsCode AsInstance => ArgsCode.Instance;
    protected static ArgsCode AsProperties => ArgsCode.Properties;
    
    // ✅ Added: ConvertAsInstance() helpers
}
```

**Removed Members:**

    ❌ `IDisposable` interface implementation
    ❌ `public void Dispose()`
    ❌ `protected virtual void Dispose(bool disposing)`
    ❌ `protected static long ResetLogCounter()`
    ❌ `protected static ArgsCode ArgsCode { get; set; }`

**Migration Required:**

```csharp
// v1 Code:
public class MyTests : TestBase
{
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ArgsCode = AsInstance;      // ❌ Property removed
            ResetLogCounter();          // ❌ Method removed
        }
        base.Dispose(disposing);        // ❌ Base class no longer IDisposable
    }
}
```

---

## Install

```bash
dotnet add package Portamical.MSTest
```

> **Prerequisites:**  
> - MSTest.TestFramework 4.0.2+  
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

### 2. Consume in MSTest Tests

#### **Option A: Shared Style (Framework-Agnostic)**

```csharp
using Portamical.TestBases.ObjectArrayCollection;

[TestClass]
public class CalculatorTests : TestBase
{
    private static IEnumerable<object?[]> AddArgs
        => Convert(CalculatorDataSource.AddCases());

    [TestMethod, DynamicData(nameof(AddArgs))]
    public void Add_validInputs_returnsSum(TestData<int, int> testData)
    {
        // Arrange
        var sut = new Calculator();

        // Act
        var actual = sut.Add(testData.Arg1, testData.Arg2);

        // Assert
        Assert.AreEqual(5, actual); // (example expected)
    }
}
```

#### **Option B: Native Style (MSTest-Specific with Display Names)**

```csharp
using Portamical.MSTest.Attributes;
using Portamical.MSTest.TestBases;

[TestClass]
public class CalculatorTests : TestBase
{
    private static IEnumerable<object?[]> AddArgs
        => Convert(CalculatorDataSource.AddCases());

    [TestMethod, PortamicalData(nameof(AddArgs))]
    public void Add_validInputs_returnsSum(TestData<int, int> testData)
    {
        // Arrange
        var sut = new Calculator();

        // Act
        var actual = sut.Add(testData.Arg1, testData.Arg2);

        // Assert
        Assert.AreEqual(5, actual);
    }
}
```

---

## What's Included

### Converters

Transform test data collections into MSTest's `object?[]` format:

```csharp
using Portamical.MSTest.Converters;

// Include TestCaseName (for DynamicDataDisplayName)
var args = testDataCollection.ToArgsWithTestCaseName(ArgsCode.Instance);

// Exclude TestCaseName (default)
var args = testDataCollection.ToObjectArrayCollection(ArgsCode.Instance);
```

### Attributes

**`PortamicalDataAttribute`** — MSTest-native attribute with `DynamicDataDisplayName`:

```csharp
[TestMethod, PortamicalData(nameof(Args))]
public void MyTest(TestData<int> testData) { ... }
```

Automatically:
- ✅ Sets test case names to `"definition => result"`
- ✅ Uses `DynamicDataDisplayName` for readable test explorer output
- ✅ Extends `DynamicDataAttribute`

### TestBases

Abstract base classes with `Convert()` methods:

```csharp
using Portamical.MSTest.TestBases;

[TestClass]
public class MyTests : TestBase
{
    protected static IEnumerable<object?[]> Args
        => Convert(dataSource.GetArgs());
}
```

### Assertions

MSTest-specific assertion helpers:

```csharp
using Portamical.MSTest.Assertions;

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
private static IEnumerable<object?[]> Args
    => Convert(dataSource.GetArgs());  // ← ArgsCode.Instance (default)

// Test signature
[TestMethod, PortamicalData(nameof(Args))]
public void Test(TestData<int> testData)  // ← Receives object
{
    var actual = Sut.Method(testData.Arg1);
    Assert.AreEqual(expected, actual);
}
```

**Best for:** Tests needing access to `TestCaseName` or full test data object.

---

### Strategy 2: Properties Mode (Flatten Parameters)

```csharp
// Data source
private static IEnumerable<object?[]> Args
    => Convert(dataSource.GetArgs(), AsProperties);  // ← ArgsCode.Properties

// Test signature
[TestMethod, PortamicalData(nameof(Args))]
public void Test(int arg1)  // ← Receives flattened parameter
{
    var actual = Sut.Method(arg1);
    Assert.AreEqual(expected, actual);
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
private static IEnumerable<object?[]> Args
    => Convert(DataSource.AddCases(), AsProperties);

[TestMethod, PortamicalData(nameof(Args))]
public void Add_validInputs_returnsExpected(int arg1, int arg2, int expected)
{
    var actual = new Calculator().Add(arg1, arg2);
    Assert.AreEqual(expected, actual);
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
private static IEnumerable<object?[]> Args
    => Convert(DataSource.NullArgCases());

[TestMethod, PortamicalData(nameof(Args))]
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
- ✅ Uses standard `[DynamicData]` attribute

**Example:**

```csharp
using Portamical.TestBases.ObjectArrayCollection;

[TestClass]
public class MyTests : TestBase
{
    private static IEnumerable<object?[]> Args
        => Convert(dataSource.GetArgs());

    [TestMethod, DynamicData(nameof(Args))]
    public void Test(TestData<int> testData) { ... }
}
```

---

### Native Style (MSTest-Specific)

**Use:** `Portamical.MSTest.TestBases.TestBase`

**Benefits:**
- ✅ MSTest-specific optimizations
- ✅ `PortamicalDataAttribute` with `DynamicDataDisplayName`
- ✅ Better integration with MSTest Test Explorer

**Example:**

```csharp
using Portamical.MSTest.Attributes;
using Portamical.MSTest.TestBases;

[TestClass]
public class MyTests : TestBase
{
    private static IEnumerable<object?[]> Args
        => Convert(dataSource.GetArgs());

    [TestMethod, PortamicalData(nameof(Args))]
    public void Test(TestData<int> testData) { ... }
}
```

---

## MSTest Integration Details

### How `PortamicalDataAttribute` Works

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PortamicalDataAttribute : DynamicDataAttribute
{
    public PortamicalDataAttribute(string dynamicDataSourceName)
        : base(
            dynamicDataSourceName,
            DynamicDataSourceType.Property,
            DynamicDataDisplayName.Method,
            DynamicDataDisplayNameDeclaringType.MethodDeclaringType,
            nameof(GetDisplayName))
    {
    }
    
    public static string GetDisplayName(
        MethodInfo methodInfo,
        object?[]? data)
    {
        if (data is null || data.Length == 0) return methodInfo.Name;
        
        // Extract TestCaseName from data
        return data[0] is INamedCase namedCase
            ? $"{methodInfo.Name}(testData: {namedCase.TestCaseName})"
            : methodInfo.Name;
    }
}
```

**Behavior:**
1. Extends MSTest's `DynamicDataAttribute`
2. Reads data from static property/method
3. Expects `IEnumerable<object?[]>`
4. Each `object?[]` contains test arguments
5. Uses `DynamicDataDisplayName.Method` to call `GetDisplayName()`
6. Displays as: `"MethodName(testData: definition => result)"`

---

### DynamicDataDisplayName Feature

MSTest 4 introduced `DynamicDataDisplayName` for custom test names:

```csharp
[TestMethod]
[DynamicData(
    nameof(Args),
    DynamicDataSourceType.Property,
    DynamicDataDisplayName.Method,  // ← Custom display name
    DynamicDataDisplayNameDeclaringType.MethodDeclaringType,
    nameof(GetDisplayName))]
public void Test(TestData<int> testData) { ... }
```

**`PortamicalDataAttribute` does this automatically!**

---

### Test Explorer Output

**Without `PortamicalDataAttribute`:**

```
✓ Add_validInputs_returnsSum (TestData<Int32, Int32>)
✓ Add_validInputs_returnsSum (TestData<Int32, Int32>)
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
| `All` | `TestCaseName` + all properties | **MSTest default** — needed for `DynamicDataDisplayName` |
| `TrimTestCaseName` | All properties **except** `TestCaseName` | Shared style (no custom display names) |
| `TrimReturnsExpected` | Also excludes `Expected` (for `IReturns`) | Return-value tests (extract expected separately) |
| `TrimThrowsExpected` | Also excludes `Expected` (for `IThrows`) | Exception tests (extract exception separately) |

**Example:**

```csharp
// MSTest Native: Include TestCaseName (for display names)
var args = Convert(dataSource.GetArgs());  // ← PropsCode.All (default)

// MSTest Shared: Exclude TestCaseName
var args = Convert(dataSource.GetArgs(), AsInstance);  // ← TrimTestCaseName

// Properties mode with TestCaseName
var args = Convert(dataSource.GetArgs(), AsProperties, WithTestCaseName);
```

---

## ToArgsWithTestCaseName Converter

MSTest-specific converter that **always includes `TestCaseName`**:

```csharp
using Portamical.MSTest.Converters;

var args = testDataCollection.ToArgsWithTestCaseName(ArgsCode.Instance);
// Equivalent to: testDataCollection.ToObjectArrayCollection(ArgsCode.Instance, PropsCode.All)
```

**Why?** MSTest's `DynamicDataDisplayName` extracts `TestCaseName` from the first argument.

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
    
    public static IEnumerable<TestDataThrows<ArgumentNullException, string>> GetConstructorInvalidArgs()
    {
        yield return CreateTestDataThrows(
            definition: "name is null",
            expected: new ArgumentNullException("name"),
            arg1: (string?)null);
    }
    
    public static IEnumerable<TestDataReturns<int, DateOnly, BirthDay>> GetCompareToArgs()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        
        yield return CreateTestDataReturns(
            definition: "other is null",
            expected: -1,
            arg1: today,
            arg2: (BirthDay?)null);
    }
}
```

### Test Class (MSTest Native Style)

```csharp
using Portamical.MSTest.Attributes;
using Portamical.MSTest.Assertions;
using Portamical.MSTest.TestBases;

[TestClass]
public class BirthDayTests : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();
    
    // Constructor - Valid Args
    private static IEnumerable<object?[]> ConstructorValidArgs
        => Convert(_dataSource.GetConstructorValidArgs());
    
    [TestMethod, PortamicalData(nameof(ConstructorValidArgs))]
    public void Constructor_validArgs_createsInstance(TestData<DateOnly> testData)
    {
        // Arrange
        const string name = "John Doe";
        var dateOfBirth = testData.Arg1;
        
        // Act
        var actual = new BirthDay(name, dateOfBirth);
        
        // Assert
        Assert.IsNotNull(actual);
        Assert.AreEqual(name, actual.Name);
        Assert.AreEqual(dateOfBirth, actual.DateOfBirth);
    }
    
    // Constructor - Invalid Args
    private static IEnumerable<object?[]> ConstructorInvalidArgs
        => Convert(_dataSource.GetConstructorInvalidArgs());
    
    [TestMethod, PortamicalData(nameof(ConstructorInvalidArgs))]
    public void Constructor_invalidArgs_throwsArgumentNullException(
        TestDataThrows<ArgumentNullException, string> testData)
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
    private static IEnumerable<object?[]> CompareToArgs
        => Convert(_dataSource.GetCompareToArgs());
    
    [TestMethod, PortamicalData(nameof(CompareToArgs))]
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
        Assert.AreEqual(expected, actual);
    }
}
```

---

## Use Cases

Install **Portamical.MSTest** if you are:

- ✅ Using MSTest 4.0.2+ for testing
- ✅ Want strongly-typed, reusable test data
- ✅ Need cross-framework test data portability
- ✅ Want self-documenting test names (`"definition => result"`)
- ✅ Need MSTest's `DynamicDataDisplayName` feature

---

## Architecture

```
Your MSTest Tests
    ↓ depends on
Portamical.MSTest (Adapter)
    ├── Converters    → ToArgsWithTestCaseName()
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

**Key Principle:** `Portamical.Core` has **zero dependencies** on MSTest.

---

## Comparison: MSTest vs. Other Frameworks

| Feature | MSTest | xUnit v2 | NUnit | xUnit v3 |
|---------|--------|----------|-------|----------|
| **Attribute** | `[DynamicData]` | `[MemberData]` | `[TestCaseSource]` | `[MemberData]` |
| **Display Names** | `DynamicDataDisplayName` | Manual `DisplayName` | `TestName` | `ITheoryDataRow.TestDisplayName` |
| **Return Type** | `IEnumerable<object?[]>` | `IEnumerable<object?[]>` | `IEnumerable<TestCaseData>` | `TheoryData<T>` |
| **Native Attribute** | `PortamicalDataAttribute` | `PortamicalDataAttribute` | `PortamicalDataAttribute` | `PortamicalDataAttribute` |
| **TestCaseName** | Included (PropsCode.All) | Excluded | Excluded | Excluded |

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

### **Version 2.0.0 (2026-03-20)**

#### **Breaking Changes (from Portamical 2.0.0 base)**

- **Removed from TestBase:**
  - ❌ `IDisposable` interface implementation
  - ❌ `Dispose()` and `Dispose(bool)` methods
  - ❌ `ResetLogCounter()` method ? use `Resolver.ResetLogCounter()`
  - ❌ `ArgsCode` property with setter

#### **Non-Breaking Changes**

- **`PortamicalDataAttribute`**
  - **`PortamicalDataAttributeBase`** → `PortamicalBaseDataAttribute`
  - **Added:** `ITestDataSourceIgnoreCapability` interface (discovery control)
  - **Factory method:** `Create()` maps 6 MSTest constructor patterns
  - **Constructor Overloads Expanded**
    - v1: 2 constructors, v2: 6 constructors (all MSTest patterns covered)
    - Supports: sourceName, sourceType, sourceArgs, declaringType, combinations
  - **`GetDisplayName()`** Enhanced

- **`TestBase`**
    `Convert<TTestData>(IEnumerable<TTestData>)` method refactored to use `ConvertAsInstance` helper method instead of the removed static `ArgsCode` proerty.

- **Documentation Overhaul**
  - **+2,300 lines** of comprehensive XML docs

---

##### **Version 2.0.1 (2026-03-20)**

- **Dependency updates** 
  - `Microsoft.Extensions.Configuration.UserSecrets`: Version 10.0.3 to 10.0.5
  - `Portamical` Version: 2.0.0-beta to 2.0.0

---

##### **Version 2.1.0 (2026-04-02)**

**Changed**
 - PortamicalBaseDataAttribute.GetDisplayName() � Simplified display-name resolution: replaced inline data[0] type guard (string or INamedCase) with a DynamicDataDisplayName is null check, delegating data-type validation to NamedCase.CreateDisplayName() in Portamical.Core 2.0.1

**Fixed**
- GetDisplayName() now correctly respects user-specified DynamicDataDisplayName � when explicitly set, the Portamical enhanced display-name logic is bypassed and MSTest's built-in formatting is used

**Dependencies**
- Portamical: 2.0.0 ? 2.0.2

---

### **Version 1.0.0 (2026-03-06)**

- **Initial release**
- MSTest 4.0.2+ integration
- `PortamicalDataAttribute` with `DynamicDataDisplayName`
- `TestBase` with `Convert()` methods
- MSTest-specific `PortamicalAssert`
- `ToArgsWithTestCaseName()` converter

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

---