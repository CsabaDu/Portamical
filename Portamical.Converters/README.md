# Portamical.Converters

**Test Data Collection Conversion & Deduplication Infrastructure for Cross-Framework Testing**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Version](https://img.shields.io/badge/version-5.0.0-orange.svg)](https://www.nuget.org/packages/Portamical.Converter)
[![C#](https://img.shields.io/badge/language-C%23-239120.svg)](https://docs.microsoft.com/dotnet/csharp/)

> **Identity-driven deduplication, async/sync conversion patterns, and framework-agnostic data provider interfaces for test data collections.**

`Portamical.Converters` provides the conversion layer between [Portamical.Core](https://github.com/CsabaDu/Portamical/tree/master/Portamical.Core) test data types and framework-specific test adapters, with automatic deduplication based on test case identity.

---

## Features

### **Strategy-Based Row Conversion**
- **Concrete rows** - Direct materialization into strongly-typed arrays (`TTestData[]`) or object-array structures (`object[][]`)
- **Abstract rows** - Custom conversion logic via `Func<TTestData, TRow>` delegates
- **Built-in strategies** - `ArgsCode.Instance` (wrap testData), `ArgsCode.Properties` (flatten properties)
- **Extensible** - Implement custom conversion strategies for framework-specific requirements

### **Identity-Based Deduplication**
- **Automatic duplicate removal** - Uses `INamedCase.TestCaseName` for semantic equality
- **O(n) performance** - `HashSet<INamedCase>` with `NamedCase.Comparer`
- **First-occurrence wins** - Preserves original collection order
- **Thread-safe** - Stateless static methods

### **Multiple Conversion Patterns**
- **Synchronous**: Arrays for immediate use (`TRow[]`)
- **Task-based**: `Task<TRow[]>` for async workflows with smart threshold optimization
- **Streaming**: `IAsyncEnumerable<TRow>` for memory-efficient async iteration

### **Performance Optimizations**
- **Zero-allocation array returns** - Direct array construction for test frameworks
- **Smart threshold strategy** - Sync for small collections (&lt;10 items), async for larger
- **Aggressive inlining** - Hot-path methods marked with `[MethodImpl(AggressiveInlining)]`

### **Framework-Agnostic Interfaces**
- **`ITestDataProvider<in TTestData>`** - Builder pattern for test data collection management
- **`ITestDataConverter<in TTestData, out TRow>`** - Conversion contract with variance support
- **Contravariant/covariant** - Flexible type assignments for base/derived type scenarios

---

## Install

```bash
dotnet add package Portamical.Converters
```

Or via NuGet Package Manager:
```powershell
Install-Package Portamical.Converters
```

**Dependencies:**
- `Portamical.Core` >= 4.2.0
- `.NET 10.0`

> **Note:** Most users should install a framework adapter instead:
> - `Portamical.xUnit` for xUnit v2
> - `Portamical.xUnit_v3` for xUnit v3
> - `Portamical.MSTest` for MSTest 4
> - `Portamical.NUnit` for NUnit 4
> - `Portamical.TUnit` (***Preview***) for TUnit
>
> Framework adapters include `Portamical.Converters` automatically.

---

## Quick Start

### Basic Deduplication

```csharp
using Portamical.Converters;

var testData = new[]
{
    CreateTestDataReturns("Add(2,3)", expected: 5, arg1: 2, arg2: 3),
    CreateTestDataReturns("Add(2,3)", expected: 5, arg1: 2, arg2: 3),  // Duplicate
    CreateTestDataReturns("Add(5,7)", expected: 12, arg1: 5, arg2: 7)
};

// Simple deduplication (identity conversion)
var distinct = testData.ToDistinctArray();
// Result: 2 elements (duplicate removed based on TestCaseName)
```

### Convert to Argument Arrays

```csharp
using Portamical.Converters;

// Convert to object[][] for test frameworks
var args = testData.ToDistinctArray(ArgsCode.Instance);
// Result: [[testData1], [testData2]]

// Or flatten properties
var flatArgs = testData.ToDistinctArray(ArgsCode.Properties);
// Result: [[2, 3, 5], [5, 7, 12]]
```

### Data Provider Pattern

```csharp
using Portamical.Converters.DataProviders;

// Combined provider + converter implementation
public class TestDataProvider<TTestData> 
    : ITestDataProvider<TTestData>,
      ITestDataConverter<TTestData, object[]>,
      IEnumerable<object[]>
where TTestData : notnull, ITestData
{
    private readonly List<TTestData> _rows = [];
    
    public string? TestMethodName { get; init; }
    public ArgsCode ArgsCode { get; init; } = ArgsCode.Instance;
    
    public void AddRow(TTestData testData) => _rows.Add(testData);
    
    public object[] ConvertRow(TTestData testData, string? testMethodName)
    {
        return testData.ToArgs(ArgsCode);
    }
    
    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var row in _rows)
        {
            yield return ConvertRow(row, TestMethodName);
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// Usage in tests
public static TestDataProvider<TestDataReturns<int>> TestCases { get; } = new()
{
    TestMethodName = "AddTest",
    ArgsCode = ArgsCode.Properties
};

static MyTests()
{
    TestCases.AddRow(CreateTestDataReturns("Add(2,3)", 5, 2, 3));
    TestCases.AddRow(CreateTestDataReturns("Add(5,7)", 12, 5, 7));
}

[Theory, MemberData(nameof(TestCases))]
public void AddTest(int arg1, int arg2, int expected)
{
    Assert.Equal(expected, Calculator.Add(arg1, arg2));
}
```

---

## Core Components

### Namespace Organization

```
Portamical.Converters/
├─── CollectionConverter.cs          # Root namespace - Synchronous conversion (TRow[])
├─── AsyncEnumerables/
│   └─── CollectionConverter.cs      # IAsyncEnumerable<TRow> streaming variants
├─── Tasks/
│   └─── CollectionConverter.cs      # Task<TRow[]> async variants with threshold optimization
└─── DataProviders/
    ├─── ITestDataProvider.cs        # Collection management contract
    ├─── ITestDataConverter.cs       # Row conversion contract
    └─── CollectionConverter.cs      # Provider-based conversion methods
```

---

### 1. **CollectionConverter** (Synchronous)

Located in the root namespace: `Portamical.Converters`

#### Primary Deduplication Method

```csharp
public static TRow[] ToDistinctArray<TTestData, TRow>(
    this IEnumerable<TTestData> testDataCollection,
    Func<TTestData, TRow> convertRow)
where TTestData : notnull, ITestData
```

**Purpose**: Core deduplication that converts a collection of test data into a distinct array using a custom conversion function.

**Algorithm**:
1. Converts collection to array snapshot and validates non-empty
2. Uses `HashSet<INamedCase>` with `NamedCase.Comparer` for O(n) deduplication
3. Only items with unique `TestCaseName` values are retained (first occurrence wins)
4. Order of elements is preserved from original collection

**Deduplication Strategy**:
- **Comparer**: `NamedCase.Comparer` compares `INamedCase.TestCaseName`
- **Semantics**: First occurrence wins
- **Performance**: O(n) using `HashSet<INamedCase>`
- **Order**: Original collection order preserved

**Example**:

```csharp
var testData = new[]
{
    CreateTestDataReturns("Add(2,3)", 5, 2, 3),
    CreateTestDataReturns("Add(2,3)", 5, 2, 3),  // Duplicate - removed
    CreateTestDataReturns("Add(5,7)", 12, 5, 7)
};

// Identity conversion
var distinct = testData.ToDistinctArray(td => td);
// Result: 2 elements

// Convert to argument arrays
var args = testData.ToDistinctArray(td => td.ToArgs(ArgsCode.Instance));
// Result: [[testData1], [testData2]]

// Custom row conversion
var rows = testData.ToDistinctArray(td => new 
{ 
    Name = td.TestCaseName, 
    Args = td.ToArgs(ArgsCode.Instance) 
});
```

#### Wrapper Methods

```csharp
// Simple deduplication (identity conversion)
TTestData[] ToDistinctArray<TTestData>(
    this IEnumerable<TTestData> testDataCollection)

// Convert to argument arrays
object?[][] ToDistinctArray<TTestData>(
    this IEnumerable<TTestData> testDataCollection,
    ArgsCode argsCode)

// Convert with args and props codes
object?[][] ToDistinctArray<TTestData>(
    this IEnumerable<TTestData> testDataCollection,
    ArgsCode argsCode,
    PropsCode propsCode)

// Convert with ArgsCode, test method name
TRow[] ToDistinctArray<TTestData, TRow>(
    this IEnumerable<TTestData> testDataCollection,
    Func<TTestData, string?, TRow> convertRow,
    string? testMethodName)
```

---

### 2. **CollectionConverter (Tasks)** 

Located in: `Portamical.Converters.Tasks`

Task-based async variants with **smart performance optimization**.

#### Performance Strategy

```csharp
// Small collections (< 10 items): Execute synchronously
Task<TRow[]> ToDistinctArrayTask<TTestData, TRow>(...)

// Performance:
// - < 10 items: Uses Task.FromResult (avoids Task.Run overhead ~5-20 µs)
// - ≥ 10 items: Uses Task.Run (offloads to thread pool)
```

**Benefits**:
- **5-20x better performance** for small collections (common in unit tests)
- **Non-blocking** for larger collections
- **Compatible** with async test frameworks

**Example**:

```csharp
using Portamical.Converters.Tasks;

// Task-based approach for async test frameworks
public static async Task<IEnumerable<object[]>> GetTestDataAsync()
{
    var testData = new[]
    {
        CreateTestDataReturns("Add(2,3)", 5, 2, 3),
        CreateTestDataReturns("Add(5,7)", 12, 5, 7)
    };
    
    // Smart threshold: sync for small, async for large
    var distinctArray = await testData.ToDistinctArrayTask();
    return distinctArray;
}
```

**Performance Characteristics**:

| Operation | Time Complexity | Space Complexity | Allocations |
|-----------|----------------|------------------|-------------|
| Deduplication | O(n) | O(n) | Minimal |
| Task threshold check | O(1) | O(1) | 0 bytes |
| Small collection (&lt;10) | ~1-3 µs | O(n) | 0 bytes (Task.FromResult) |
| Large collection (≥10) | ~6-22 µs | O(n) | Task.Run overhead |

---

### 3. **CollectionConverter (AsyncEnumerables)**

Located in: `Portamical.Converters.AsyncEnumerables`

Streaming variants for memory-efficient async iteration.

```csharp
// Stream distinct test data asynchronously
IAsyncEnumerable<TRow> ToDistinctAsyncEnumerable<TTestData, TRow>(
    this IEnumerable<TTestData> testDataCollection,
    Func<TTestData, TRow> convertRow)
```

**Use Case**: Streaming scenarios where test data is consumed incrementally.

**Example**:

```csharp
using Portamical.Converters.AsyncEnumerables;

var testData = GetLargeTestDataCollection();

await foreach (var testCase in testData.ToDistinctAsyncEnumerable())
{
    await ProcessTestCaseAsync(testCase);
}
```

**Note**: Deduplication is performed synchronously using the underlying synchronous converter, but results are yielded asynchronously.

---

### 4. **CollectionConverter.DataProviders** (DataProviders Namespace)

Located in: `Portamical.Converters.DataProviders`

Data provider creation with automatic deduplication based on test case identity.

#### Primary Method - With Initializer Function

```csharp
public static TDataProvider ToDataProvider<TDataProvider, TTestData>(
    this IEnumerable<TTestData> testDataCollection,
    Func<TTestData, TDataProvider> initDataProvider)
where TTestData : notnull, ITestData
where TDataProvider : ITestDataProvider<TTestData>
```

**Purpose**: Convert a collection of test data into a populated data provider instance using a custom initializer function.

**Algorithm**:
1. Converts collection to array snapshot and validates non-empty
2. Initializes data provider with first test data item via `initDataProvider` function
3. For remaining items, adds only those with unique `TestCaseName` values via `AddRow()`
4. Returns the populated data provider

**Key Features**:
- **First-item initialization**: Uses the first test data item for provider initialization
- **Custom configuration**: Initializer function can pass parameters to provider constructor
- **Automatic deduplication**: `HashSet<INamedCase>` with `NamedCase.Comparer` (O(n) performance)
- **First-occurrence wins**: Duplicates (same `TestCaseName`) are silently skipped

**Example**:

```csharp
using Portamical.Converters.DataProviders;

var testData = new[]
{
    CreateTestDataReturns("Add(2,3)", 5, 2, 3),
    CreateTestDataReturns("Add(2,3)", 5, 2, 3),  // Duplicate - will be filtered
    CreateTestDataReturns("Add(5,7)", 12, 5, 7)
};

// Initialize provider with custom configuration
var provider = testData.ToDataProvider(
    firstItem => new MyDataProvider(firstItem, ArgsCode.Properties, "TestAdd"));

// Result: provider.Count == 2 (duplicate removed, first item used for initialization)
```

#### Overload - With Default Constructor

```csharp
public static TDataProvider ToDataProvider<TDataProvider, TTestData>(
    this IEnumerable<TTestData> testDataCollection)
where TTestData : notnull, ITestData
where TDataProvider : ITestDataProvider<TTestData>, new()
```

**Purpose**: Convert a collection using the provider's default (parameterless) constructor.

**Algorithm**:
1. Converts collection to array snapshot and validates non-empty
2. Creates data provider instance using `new()` constraint
3. Iterates through all items, adding only those with unique `TestCaseName` values
4. Returns the populated data provider

**Key Features**:
- **Default construction**: Uses parameterless constructor (`new()` constraint)
- **All items via AddRow**: No special first-item initialization
- **Automatic deduplication**: Same `HashSet<INamedCase>` strategy as primary method
- **Performance**: Uses `foreach` instead of LINQ for better HashSet-based deduplication

**Example**:

```csharp
using Portamical.Converters.DataProviders;

var testData = new[]
{
    CreateTestDataReturns("Add(2,3)", 5, 2, 3),
    CreateTestDataReturns("Add(2,3)", 5, 2, 3),  // Duplicate
    CreateTestDataReturns("Add(5,7)", 12, 5, 7)
};

// Use default constructor (provider must have parameterless ctor)
var provider = testData.ToDataProvider<MyDataProvider, TestDataReturns<int>>();

// Result: provider.Count == 2 (duplicate removed)
```

**Comparison**:

| Aspect | Initializer Function | Default Constructor |
|--------|---------------------|-------------------|
| **Signature** | `ToDataProvider(Func<TTestData, TDataProvider>)` | `ToDataProvider<TDataProvider, TTestData>()` |
| **Constraint** | None | Requires `new()` constraint |
| **First Item** | Used for initialization via function | Added via `AddRow()` like others |
| **Configuration** | Can pass parameters to constructor | Must use parameterless constructor |
| **Use Case** | When provider needs initial configuration | When provider has sensible defaults |

---

### 5. **ITestDataProvider&lt;TTestData&gt;**

Located in: `Portamical.Converters.DataProviders`

Defines a contract for managing collections of test data rows with test method metadata.

```csharp
public interface ITestDataProvider<in TTestData> : IEnumerable
where TTestData : notnull, ITestData
{
    string? TestMethodName { get; init; }
    void AddRow(TTestData testData);
}
```

**Features**:
- **Contravariant** (`in TTestData`) - Provider accepting base types can be assigned to variables expecting derived types
- **Builder Pattern** - Add rows incrementally via `AddRow()`
- **Framework Integration** - Implements `IEnumerable` for test framework discovery

**Contravariance Example**:

```csharp
// Provider accepts base type ITestData
ITestDataProvider<ITestData> baseProvider = new TestDataProvider<ITestData>();

// Can be assigned to variable expecting derived type
ITestDataProvider<TestDataReturns<int>> derivedProvider = baseProvider; // ✅

// This works because TestDataReturns<int> : ITestData
// The provider can accept any ITestData, including derived types
```

**Usage**:

```csharp
var provider = new TestDataProvider<TestDataReturns<int>>
{
    TestMethodName = "AddTest"
};

provider.AddRow(CreateTestDataReturns("Add(2,3)", 5, 2, 3));
provider.AddRow(CreateTestDataReturns("Add(5,7)", 12, 5, 7));

// Use with xUnit, NUnit, MSTest attributes
[Theory, MemberData(nameof(TestCases))]
public void AddTest(int arg1, int arg2, int expected)
{
    Assert.Equal(expected, Calculator.Add(arg1, arg2));
}
```

---

### 6. **ITestDataConverter&lt;TTestData, TRow&gt;**

Located in: `Portamical.Converters.DataProviders`

Defines conversion logic for transforming test data into framework-specific row formats.

```csharp
public interface ITestDataConverter<in TTestData, out TRow>
where TTestData : notnull, ITestData
{
    ArgsCode ArgsCode { get; init; }
    TRow ConvertRow(TTestData testData, string? testMethodName);
}
```

**Variance**:
- **Contravariant** `in TTestData` - Converter accepting base types works with derived types
- **Covariant** `out TRow` - Converter returning specific types works where general types expected

**Conversion Strategies**:

| ArgsCode | Conversion | Test Signature |
|----------|------------|----------------|
| `ArgsCode.Instance` | `[testDataObject]` | `void Test(TTestData testData)` |
| `ArgsCode.Properties` | `[arg1, arg2, expected]` | `void Test(int arg1, int arg2, int expected)` |

**Variance Example**:

```csharp
// Contravariance: Base type converter → Derived type variable
ITestDataConverter<ITestData, object[]> generalConverter = new TestDataProvider<ITestData>();
ITestDataConverter<TestDataReturns<int>, object[]> specificConverter = generalConverter; // ✅

// Covariance: Specific return type → General return type
ITestDataConverter<ITestData, object[]> arrayConverter = specificConverter;
ITestDataConverter<ITestData, object> objectConverter = arrayConverter; // ✅
```

**Combined Implementation Pattern**:

The recommended pattern combines both interfaces for complete test data management:

```csharp
public class TestDataProvider<TTestData> 
    : ITestDataProvider<TTestData>,               // Manages collection
      ITestDataConverter<TTestData, object[]>,    // Transforms rows
      IEnumerable<object[]>
where TTestData : notnull, ITestData
{
    private readonly List<TTestData> _rows = [];
    private readonly HashSet<INamedCase> _namedCases = new(NamedCase.Comparer);
    
    public string? TestMethodName { get; init; }
    public ArgsCode ArgsCode { get; init; } = ArgsCode.Instance;
    
    public void AddRow(TTestData testData)
    {
        // Deduplicate: only add if TestCaseName is unique
        if (_namedCases.Add(testData))
        {
            _rows.Add(testData);
        }
    }
    
    public object[] ConvertRow(TTestData testData, string? testMethodName)
        => testData.ToArgs(ArgsCode);
    
    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var row in _rows)
        {
            yield return ConvertRow(row, TestMethodName);
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

**Benefits of Combined Implementation**:

- **Unified Configuration**: `ArgsCode` and `TestMethodName` set once during construction
- **Framework Integration**: Implements `IEnumerable` for iteration with internal `ConvertRow` usage
- **Stateful Conversion**: Provider maintains state while converter provides transformation logic
- **Separation of Concerns**: `ITestDataProvider` manages collection, `ITestDataConverter` handles conversion

---

## Usage Examples

### Example 1: Simple Deduplication

```csharp
using Portamical.Converters;

var testData = new[]
{
    CreateTestDataReturns("Add(2,3)", 5, 2, 3),
    CreateTestDataReturns("Add(2,3)", 5, 2, 3),  // Duplicate
    CreateTestDataReturns("Add(5,7)", 12, 5, 7)
};

// Get distinct array
var distinct = testData.ToDistinctArray();
// Result: 2 elements (duplicate removed based on TestCaseName)
```

### Example 2: Convert to Argument Arrays

```csharp
using Portamical.Converters;

// Convert to object[][] for test frameworks
var args = testData.ToDistinctArray(ArgsCode.Instance);
// Result: [[testData1], [testData2]]

// Or flatten properties
var flatArgs = testData.ToDistinctArray(ArgsCode.Properties);
// Result: [[2, 3, 5], [5, 7, 12]]
```

### Example 3: Task-Based Async

```csharp
using Portamical.Converters.Tasks;

public static async Task<IEnumerable<object[]>> GetTestDataAsync()
{
    var testData = GetTestDataCollection();
    
    // Smart threshold optimization
    var distinct = await testData.ToDistinctArrayTask(ArgsCode.Instance);
    return distinct;
}
```

### Example 4: Streaming with AsyncEnumerable

```csharp
using Portamical.Converters.AsyncEnumerables;

var testData = GetLargeTestDataCollection();

await foreach (var testCase in testData.ToDistinctAsyncEnumerable())
{
    await ProcessTestCaseAsync(testCase);
}
```

### Example 5: Create Data Provider from Collection

**Two Overloads for Data Provider Creation:**

```csharp
using Portamical.Converters.DataProviders;

// Approach 1: Using custom initializer function
var testDataCollection = new[]
{
    CreateTestDataReturns("Add(2,3)", 5, 2, 3),
    CreateTestDataReturns("Add(2,3)", 5, 2, 3),  // Duplicate
    CreateTestDataReturns("Add(5,7)", 12, 5, 7)
};

// Initialize provider with first item via custom function
var provider1 = testDataCollection.ToDataProvider(
    firstItem => new MyDataProvider(firstItem, ArgsCode.Properties, "TestAdd"));

// Result: provider1.Count == 2 (duplicate removed, first item used for initialization)

// Approach 2: Using default constructor (new() constraint)
var provider2 = testDataCollection.ToDataProvider<MyDataProvider, TestDataReturns<int>>();

// Result: provider2.Count == 2 (duplicate removed, parameterless constructor used)

// Custom provider class example:
public class MyDataProvider : ITestDataProvider<TestDataReturns<int>>
{
    private readonly List<TestDataReturns<int>> _rows = [];
    
    // Constructor for Approach 1
    public MyDataProvider(TestDataReturns<int> firstItem, ArgsCode argsCode, string? testMethodName)
    {
        ArgsCode = argsCode;
        TestMethodName = testMethodName;
        AddRow(firstItem);
    }
    
    // Parameterless constructor for Approach 2
    public MyDataProvider()
    {
        ArgsCode = ArgsCode.Instance;
        TestMethodName = null;
    }
    
    public ArgsCode ArgsCode { get; init; }
    public string? TestMethodName { get; init; }
    
    public void AddRow(TestDataReturns<int> testData) => _rows.Add(testData);
}
```

**Key Differences:**

| Aspect | Initializer Function | Default Constructor |
|--------|---------------------|-------------------|
| **Signature** | `ToDataProvider(Func<TTestData, TDataProvider>)` | `ToDataProvider<TDataProvider, TTestData>()` |
| **Constraint** | None | Requires `new()` constraint |
| **First Item** | Used for initialization via function | Added via `AddRow()` |
| **Configuration** | Can pass parameters to constructor | Must use parameterless constructor |
| **Use Case** | When provider needs initial configuration | When provider has sensible defaults |

### Example 6: Framework-Specific Provider (xUnit v3 Pattern)

**Recommended Pattern:** For production use, implement framework-specific providers that extend both the conversion infrastructure and the framework's base classes. See `Portamical.xUnit_v3.DataProviders.Model.TheoryTestData<TTestData>` for the complete implementation.

```csharp
using Portamical.Converters.DataProviders;
using Portamical.xUnit_v3.TestDataTypes;
using Portamical.xUnit_v3.TestDataTypes.Model;
using Xunit.v3;  // xUnit v3
using static Portamical.Core.Formatting.Formatter;
using static Portamical.Core.Safety.Validator;

/// <summary>
/// xUnit v3 theory test data provider that combines Portamical's ITestDataProvider/ITestDataConverter
/// with xUnit v3's TheoryDataBase for seamless integration.
/// </summary>
/// <typeparam name="TTestData">The type of test data implementing ITestData.</typeparam>
/// <remarks>
/// <para><strong>Design Patterns:</strong></para>
/// <list type="bullet">
///   <item><strong>Builder Pattern:</strong> Incremental construction via AddRow</item>
///   <item><strong>Template Method:</strong> Abstract Convert() uses instance config (ArgsCode, TestMethodName)</item>
///   <item><strong>Automatic Deduplication:</strong> HashSet with NamedCase.Comparer (O(1) checks)</item>
///   <item><strong>Pattern Matching Type Safety:</strong> Efficient runtime type validation</item>
/// </list>
/// <para><strong>Inheritance Hierarchy:</strong></para>
/// <code>
/// xUnit.v3.TheoryDataBase&lt;ITheoryDataRow, TTestData&gt; (xUnit v3 base)
///   ↓ inherits
/// TheoryTestData&lt;TTestData&gt; (this class)
///   ↓ implements
/// ITheoryTestData&lt;TTestData&gt; (Portamical)
///   ↓ extends
/// ITestDataProvider&lt;TTestData&gt; + ITestDataConverter&lt;TTestData, ITheoryDataRow&gt;
/// </code>
/// </remarks>
public sealed class TheoryTestData<TTestData>
    : TheoryDataBase<ITheoryDataRow, TTestData>,        // xUnit v3 base
      ITheoryTestData<TTestData>                        // Portamical (includes provider + converter)
where TTestData : notnull, ITestData
{
    private readonly HashSet<INamedCase> _namedCases = new(NamedCase.Comparer);
    
    public string? TestMethodName { get; init; }
    public ArgsCode ArgsCode { get; init; }
    
    /// <summary>
    /// Constructor (internal): Encourages use of static factory methods.
    /// </summary>
    internal TheoryTestData(TTestData testData, ArgsCode argsCode, string? testMethodName)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));  // Validates enum value
        TestMethodName = testMethodName;
        AddRow(testData);  // Add first item immediately (ensures non-empty collection)
    }
    
    /// <summary>
    /// Template Method: Uses instance configuration (ArgsCode, TestMethodName) for conversion.
    /// Called internally by xUnit v3's TheoryDataBase when iterating rows.
    /// </summary>
    protected override ITheoryTestDataRow Convert(TTestData row)
        => ConvertRow(testData: row, TestMethodName);
    
    /// <summary>
    /// Override xUnit v3's Add: Adds pattern-based type validation and automatic deduplication.
    /// </summary>
    /// <remarks>
    /// <para><strong>Type Validation (Pattern Matching):</strong></para>
    /// Uses C# pattern matching for efficient type checking:
    /// <code>
    /// if (NotNull(row, nameof(row)) is not TheoryTestDataRow&lt;TTestData&gt;)
    /// {
    ///     throw new ArgumentException(...);
    /// }
    /// </code>
    /// This single pattern match validates both null-safety and type compatibility
    /// in one efficient operation.
    /// 
    /// <para><strong>Deduplication Logic:</strong></para>
    /// <code>
    /// if (_namedCases.Add(row))
    /// {
    ///     // _namedCases.Add returns:
    ///     // - true: row.TestCaseName is unique → add to collection
    ///     // - false: row.TestCaseName is duplicate → skip silently
    ///     base.Add(row);
    /// }
    /// </code>
    /// </remarks>
    public override void Add(ITheoryTestDataRow row)
    {
        // Pattern matching: validates type in one efficient operation
        if (NotNull(row, nameof(row)) is not TheoryTestDataRow<TTestData>)
        {
            throw new ArgumentException(
                $"The provided test data row has an incompatible type. " +
                $"Expected: {Format(typeof(TheoryTestDataRow<TTestData>))}, " +
                $"Actual: {Format(row.GetType())}",
                nameof(row));
        }
        
        // Deduplication: only add if TestCaseName is unique
        if (_namedCases.Add(row))
        {
            base.Add(row);
        }
    }
    
    /// <summary>
    /// Builder Pattern: Add test data with automatic deduplication.
    /// </summary>
    public void AddRow(TTestData testData)
        => Add(Convert(testData));
    
    /// <summary>
    /// Converter: Transform test data to xUnit v3 row format.
    /// </summary>
    /// <remarks>
    /// <para><strong>Simplified Signature (v5.0.0):</strong></para>
    /// <list type="bullet">
    ///   <item><strong>Parameters:</strong> Only testData and testMethodName</item>
    ///   <item><strong>ArgsCode:</strong> Uses instance property (this.ArgsCode), not a parameter</item>
    ///   <item><strong>Benefits:</strong> Single configuration point reduces parameter passing</item>
    /// </list>
    /// </remarks>
    public ITheoryTestDataRow ConvertRow(TTestData testData, string? testMethodName)
        => new TheoryTestDataRow<TTestData>(testData, ArgsCode, testMethodName);
}

// Usage in test class:
public class CalculatorTests
{
    public static IEnumerable<ITheoryDataRow> GetAddTestData()
    {
        // Create with first test data
        // Note: Constructor is internal - typically used via static factory methods
        var data = new TheoryTestData<TestDataReturns<int>>(
            testData: CreateTestDataReturns("Add(2,3)", 5, 2, 3),
            argsCode: ArgsCode.Properties,
            testMethodName: "TestAdd");
        
        // Builder pattern - incremental construction with automatic deduplication
        data.AddRow(CreateTestDataReturns("Add(2,3)", 5, 2, 3));  // Duplicate - silently ignored
        data.AddRow(CreateTestDataReturns("Add(5,7)", 12, 5, 7));
        data.AddRow(CreateTestDataReturns("Add(-1,1)", 0, -1, 1));
        
        return data;  // xUnit v3 consumes IEnumerable<ITheoryDataRow>
        // Result: 3 rows (duplicate "Add(2,3)" not added due to same TestCaseName)
    }
    
    [Theory]
    [MemberData(nameof(GetAddTestData))]
    public void TestAdd(int x, int y, int expected)
    {
        int result = Calculator.Add(x, y);
        Assert.Equal(expected, result);
    }
}
    
    [Theory, MemberData(nameof(GetAddTestData))]
    public void TestAdd(int arg1, int arg2, int expected)
    {
        Assert.Equal(expected, Calculator.Add(arg1, arg2));
    }
}

// xUnit v3 Test Explorer displays:
// ✓ TestAdd - Add(2,3)   ← Custom test name with method prefix
// ✓ TestAdd - Add(5,7)   ← Custom test name with method prefix
```

**Key Benefits of This Pattern:**

1. **Pattern Matching Type Safety** - C# pattern matching (`is not TheoryTestDataRow<TTestData>`) validates type in one efficient operation
2. **Human-Readable Error Messages** - Uses `Formatter.Format()` for clear type names with full generic parameter information (e.g., `TheoryTestDataRow<TestDataReturns<Int32>>`)
3. **Automatic Deduplication** - `HashSet<INamedCase>` with `NamedCase.Comparer` for O(1) duplicate detection
4. **Template Method** - `Convert()` uses instance `ArgsCode` and `TestMethodName` automatically
5. **Builder Pattern** - `AddRow()` for incremental test data construction
6. **xUnit v3 Integration** - Extends `TheoryDataBase<ITheoryDataRow, TTestData>` for native framework support
7. **Custom Display Names** - xUnit v3's `TestDisplayName` generated via `TheoryTestDataRow<TTestData>` constructor
8. **Single Configuration Point** - `ArgsCode` and `TestMethodName` set once (init-only properties)
9. **Internal Constructor** - Encourages use of static factory methods for cleaner API

> **Note:** For complete implementation including factory methods and additional overloads, see `Portamical.xUnit_v3.DataProviders.Model.TheoryTestData<TTestData>` in the xUnit v3 adapter package.

---

## Framework Support

Works with all major .NET testing frameworks:

| Framework | Version | Support |
|-----------|---------|---------|
| **xUnit** | v2 | ✅ `IEnumerable<object[]>` |
| **xUnit** | v3 | ✅ `TheoryDataRow<T...>` support |
| **MSTest** | v4 | ✅ `DynamicData` attribute |
| **NUnit** | v4 | ✅ `TestCaseSource` attribute |
| **TUnit** | Latest | ✅ Full support |

---

## Design Patterns

### Builder Pattern (ITestDataProvider)

```csharp
var provider = new TestDataProvider<TestDataReturns<int>>();
provider.AddRow(testData1);
provider.AddRow(testData2);
```

### Provider + Converter Pattern

Combine both interfaces for complete test data management:

```csharp
public class TestDataProvider<TTestData> 
    : ITestDataProvider<TTestData>,      // Manages collection
      ITestDataConverter<TTestData, object[]>  // Transforms rows
```

### Variance Support

```csharp
// Contravariance: Base type provider → Derived type variable
ITestDataProvider<ITestData> baseProvider = ...;
ITestDataProvider<TestDataReturns<int>> derivedProvider = baseProvider; // ✅

// Covariance: Specific return type → General return type
ITestDataConverter<ITestData, object[]> specificConverter = ...;
ITestDataConverter<ITestData, object> baseConverter = specificConverter; // ✅
```

---

## Thread Safety

- **All static methods**: Thread-safe (stateless)
- **Instance methods**: Not thread-safe by design
  - Providers built during test discovery (single-threaded)
  - Used read-only during test execution (safe)
- **External synchronization**: Required if providers are modified concurrently

---

## Performance Characteristics

| Operation | Time Complexity | Space Complexity | Allocations |
|-----------|----------------|------------------|-------------|
| Deduplication | O(n) | O(n) | Minimal |
| Array conversion | O(n) | O(n) | Array only |
| Task threshold check | O(1) | O(1) | 0 bytes |
| Async enumeration | O(n) | O(1) streaming | Per-item |

**Benchmarks** (small collection < 10 items):
- **Synchronous**: ~1-2 µs
- **Task-based (sync path)**: ~1-3 µs (Task.FromResult)
- **Task-based (async path)**: ~6-22 µs (Task.Run overhead)

---

## ArgsCode Strategy Pattern

Control how test data is serialized to test method parameters:

| Strategy | Produces | Test Method Signature | Use Case |
|----------|----------|----------------------|----------|
| `ArgsCode.Instance` | `[testDataObject]` | `void Test(TTestData data)` | Object-oriented, full test data access |
| `ArgsCode.Properties` | `[arg1, arg2, ...]` | `void Test(T arg1, T arg2, ...)` | Functional style, explicit parameters |

**Combined with PropsCode**:
- `PropsCode.All` - Include all properties
- `PropsCode.TrimTestCaseName` - Exclude `TestCaseName` (default)
- `PropsCode.TrimReturnsExpected` - Also exclude `Expected` (for `IReturns`)
- `PropsCode.TrimThrowsExpected` - Also exclude `Expected` (for `IThrows`)

---

## Links

- **GitHub**: https://github.com/CsabaDu/Portamical
- **Documentation**: https://github.com/CsabaDu/Portamical/blob/master/README.md
- **Issues**: https://github.com/CsabaDu/Portamical/issues
- **Related Packages**:
  - [Portamical.Core](https://github.com/CsabaDu/Portamical/tree/master/Portamical.Core) - Core test data types
  - [Portamical.Core.Formatting](https://github.com/CsabaDu/Portamical/tree/master/Portamical.Core.Formatting) - Formatting infrastructure
  - [Portamical](https://github.com/CsabaDu/Portamical/tree/master/Portamical) - Shared utilities and assertions

---

## License

This project is licensed under the [MIT License](https://github.com/CsabaDu/Portamical/blob/master/LICENSE.txt).

```
SPDX-License-Identifier: MIT
Copyright (c) 2025-2026 Csaba Dudas (CsabaDu)
```

`Portamical` is the continuation and successor of `CsabaDu.DynamicTestData.Light` and `CsabaDu.DynamicTestData` (also MIT-licensed).

---

## Changelog

### **Version 5.0.0** - Initial Release (2026-08-12)

**Extraction from Portamical (Shared Module) - Major Architectural Refactoring**

This release extracts data provider interfaces and collection converters from the `Portamical` (shared) module into a dedicated package. Part of the Portamical v5.0.0 ecosystem-wide release that restructures the framework for better modularity.

**Migrated Components**:

1. **Data Provider Interfaces**
   - `ITestDataProvider<in TTestData>` - Manages test data collections with test method metadata
   - `ITestDataConverter<in TTestData, out TRow>` - Converts test data to framework-specific row formats
   - Contravariant/covariant support for flexible type assignments

2. **Collection Converters**
   - `CollectionConverter` (root namespace) - Synchronous deduplication and array conversion
     - `ToDistinctArray<TTestData, TRow>()` - Core deduplication with custom conversion
     - Wrapper methods for common scenarios (identity, ArgsCode, PropsCode)
   - `CollectionConverter` (AsyncEnumerables) - Streaming variants with `IAsyncEnumerable<T>`
     - `ToDistinctAsyncEnumerable<TTestData, TRow>()` - Memory-efficient async iteration
   - `CollectionConverter` (Tasks) - Task-based async variants with smart threshold optimization
     - `ToDistinctArrayTask<TTestData, TRow>()` - Async with &lt;10 items sync, ≥10 items async
   - `CollectionConverter` (DataProviders) - Provider-based conversion methods
     - `ToDataProvider<TDataProvider, TTestData>()` - Convert collection to data provider instance

**Architecture Benefits**:
- **Reusability**: Pure conversion infrastructure, decoupled from assertions and test base classes
- **Separation of Concerns**: Framework-agnostic foundation for data transformation
- **Dependency Graph**: Clean layered architecture without circular dependencies
  - `Portamical.Core` (foundation) → `Portamical.Converters` (conversion) → `Portamical` (shared utilities) → Framework adapters
- **Maintenance**: Independent versioning and releases

**Features**:
- **Identity-based deduplication** using `INamedCase.TestCaseName` with `NamedCase.Comparer`
- **O(n) performance** with `HashSet<INamedCase>` for deduplication
- **First-occurrence wins** semantics preserving original collection order
- **Multiple conversion patterns**: synchronous, Task-based, and streaming
- **Smart threshold optimization** for Task-based conversions (&lt;10 items sync, ≥10 items async)
- **Zero-allocation array returns** for test frameworks (direct array construction)
- **Thread-safe stateless static methods** for concurrent test execution
- **Variance support** for flexible type assignments (contravariant/covariant interfaces)

**Performance Characteristics**:
- **Deduplication**: O(n) time, O(n) space, minimal allocations
- **Synchronous**: ~1-2 µs for small collections (&lt;10 items)
- **Task-based (sync path)**: ~1-3 µs with `Task.FromResult`
- **Task-based (async path)**: ~6-22 µs with `Task.Run` overhead
- **Async enumeration**: O(n) time, O(1) space (streaming)

**Breaking Changes from Portamical v4.x**:
- Data provider interfaces moved from `Portamical.DataProviders` to `Portamical.Converters.DataProviders`
- Collection converters moved from `Portamical.Converters` to `Portamical.Converters` (namespace unchanged, but now in separate package)
- Migration: Update package references from `Portamical` to `Portamical.Converters` for data provider usage

**Migration Guide**:

```csharp
// Before (Portamical v4.x):
using Portamical.DataProviders;
using Portamical.Converters;

// After (Portamical.Converters v5.0.0):
using Portamical.Converters.DataProviders;  // ITestDataProvider, ITestDataConverter
using Portamical.Converters;                // CollectionConverter extensions
using Portamical.Converters.Tasks;          // ToDistinctArrayTask
using Portamical.Converters.AsyncEnumerables;  // ToDistinctAsyncEnumerable
```

**Dependencies**:
- Portamical.Core >= 4.2.0
- .NET 10.0

**Compatibility**:
- Framework adapters (xUnit, MSTest, NUnit, TUnit) updated to v5.0.0 for compatibility
- API surface remains identical except for namespace changes
- No functional changes to deduplication or conversion logic

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

---
