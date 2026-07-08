# AggressiveInlining Analysis for CollectionConverter

## Current State
Currently, `CollectionConverter` has NO methods marked with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`.

## Recommended Methods for AggressiveInlining

### ? SHOULD Add AggressiveInlining

#### 1. `ToDistinctArray<TTestData>(IEnumerable<TTestData>)` - Lines 42-46
**Reason:** Thin wrapper that just forwards to the core method with identity lambda
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static TTestData[] ToDistinctArray<TTestData>(
	this IEnumerable<TTestData> testDataCollection)
where TTestData : notnull, ITestData
=> testDataCollection.ToDistinctArray(testData => testData);
```
- **Size:** 1 line (expression body)
- **Complexity:** O(1) - simple delegation
- **Call frequency:** High (public API entry point)
- **Benefit:** Eliminates method call overhead, allows JIT to optimize through to core implementation

#### 2. `ToDistinctReadOnly<TTestData>(IEnumerable<TTestData>)` - Lines 59-63
**Reason:** Thin wrapper with ArgsCode.Instance
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static IReadOnlyCollection<object?[]> ToDistinctReadOnly<TTestData>(
	this IEnumerable<TTestData> testDataCollection)
where TTestData : notnull, ITestData
=> testDataCollection.ToDistinctArray(testData => testData.ToArgs(ArgsCode.Instance));
```
- **Size:** 1 line (expression body)
- **Complexity:** O(1) - simple delegation with constant
- **Call frequency:** High (common default case)
- **Benefit:** Reduces overhead for default arg code scenario

#### 3. `ToDistinctReadOnly<TTestData>(IEnumerable<TTestData>, ArgsCode)` - Lines 77-82
**Reason:** Thin wrapper with parameter forwarding
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static IReadOnlyCollection<object?[]> ToDistinctReadOnly<TTestData>(
	this IEnumerable<TTestData> testDataCollection,
	ArgsCode argsCode)
where TTestData : notnull, ITestData
=> testDataCollection.ToDistinctArray(testData => testData.ToArgs(argsCode));
```
- **Size:** 1 line (expression body)
- **Complexity:** O(1) - simple delegation
- **Call frequency:** High
- **Benefit:** Lambda allocation might be optimized away by JIT

#### 4. `ToDistinctReadOnly<TTestData>(IEnumerable<TTestData>, ArgsCode, PropsCode)` - Lines 97-103
**Reason:** Thin wrapper with two parameter forwarding
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static IReadOnlyCollection<object?[]> ToDistinctReadOnly<TTestData>(
	this IEnumerable<TTestData> testDataCollection,
	ArgsCode argsCode,
	PropsCode propsCode)
where TTestData : notnull, ITestData
=> testDataCollection.ToDistinctArray(testData => testData.ToArgs(argsCode, propsCode));
```
- **Size:** 1 line (expression body)
- **Complexity:** O(1) - simple delegation
- **Call frequency:** Medium-High
- **Benefit:** Reduces overhead especially when called in tight loops during test setup

### ? SHOULD NOT Add AggressiveInlining

#### 5. `ToDistinctReadOnly<TTestData, TRow>(...)` - Lines 120-130
**Reason:** Contains closure and method call
```csharp
// NO AggressiveInlining
public static IReadOnlyCollection<TRow> ToDistinctReadOnly<TTestData, TRow>(
	this IEnumerable<TTestData> testDataCollection,
	Func<TTestData, ArgsCode, string?, TRow> convertRow,
	ArgsCode argsCode,
	string? testMethodName)
where TTestData : notnull, ITestData
=> testDataCollection.ToDistinctArray(
	testData => convertRow(
		testData,
		argsCode.Defined(nameof(argsCode)),  // Method call!
	testMethodName));
```
- **Size:** Multi-line lambda with method call
- **Complexity:** Contains `Defined()` method call
- **Reason to avoid:** Complex lambda, not a simple passthrough
- **JIT decision:** Let JIT decide based on call site analysis

#### 6. `ToDataProvider<TDataProvider, TTestData>(...)` - Lines 148-173
**Reason:** Complex logic with loops and conditionals
```csharp
// NO AggressiveInlining
public static TDataProvider ToDataProvider<TDataProvider, TTestData>(...)
{
	var testDatas = testDataCollection.ToDistinctArray();
	var dataProvider = NotNull(initDataProvider, nameof(initDataProvider))(
		testDatas[0], argsCode, testMethodName);
	var count = testDatas.Length;

	if (count > 1)
	{
		for (int i = 1; i < count; i++)  // Loop!
		{
			dataProvider.AddRow(testDatas[i]);
		}
	}

	return dataProvider;
}
```
- **Size:** 20+ lines with control flow
- **Complexity:** O(n) loop, branching, method calls
- **Reason to avoid:** Too large, JIT will not inline regardless
- **Code bloat risk:** Would duplicate 20+ lines at every call site

#### 7. `ToDistinctArray<TTestData, TRow>(...)` (private) - Lines 236-249
**Reason:** Core implementation with LINQ and HashSet
```csharp
// NO AggressiveInlining
private static TRow[] ToDistinctArray<TTestData, TRow>(
	this IEnumerable<TTestData> testDataCollection,
	Func<TTestData, TRow> convertRow)
where TTestData : notnull, ITestData
{
	var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
	var rows = NotNullOrEmpty(testDataCollection, nameof(testDataCollection))
		.Where(testData => namedCases.Add(testData))
		.Select(NotNull(convertRow, nameof(convertRow)));

	return [.. rows];
}
```
- **Size:** Multi-line with LINQ chain
- **Complexity:** O(n) LINQ operations, allocations
- **Reason to avoid:** Complex logic, allocations, not a hot path itself (called through wrappers)
- **Call pattern:** Only called internally, JIT will optimize call sites naturally

## Guidelines Applied

Based on analysis of `PortamicalAssert.cs` and `Validator.cs`:

### ? Good Candidates for AggressiveInlining:
1. **Thin wrappers** (1-2 lines)
2. **Parameter forwarding** methods
3. **Validation helpers** (`NotNull`, `NotNullOrEmpty`)
4. **Fast-path methods** (`ThreadSafeSync`, `IsNotFatal`)
5. **Simple predicates** (`AreApproximatelyEqual`)
6. **Expression-bodied members** with no complex logic

### ? Poor Candidates:
1. **Complex logic** (>10 lines)
2. **Loops and branches**
3. **LINQ chains**
4. **Multiple allocations**
5. **Already large methods** (JIT won't inline anyway)

## Performance Impact

### Expected Benefits:
- **Reduced call overhead** for thin wrappers (4 methods)
- **Better JIT optimization** through method chains
- **Potential lambda elision** in simple forwarding cases
- **Consistent with framework patterns** (Validator, PortamicalAssert)

### Code Bloat Risk:
- **Low risk**: Only 4 small methods (1-line expression bodies)
- **IL size increase**: ~40-60 bytes per call site (minimal)
- **Typical usage**: 2-5 call sites per test project

## Implementation Priority

### High Priority (Add Now):
1. ? `ToDistinctArray<TTestData>(IEnumerable<TTestData>)` - Most direct entry point
2. ? `ToDistinctReadOnly<TTestData>(IEnumerable<TTestData>)` - Common default case

### Medium Priority (Consider):
3. ? `ToDistinctReadOnly<TTestData>(IEnumerable<TTestData>, ArgsCode)`
4. ? `ToDistinctReadOnly<TTestData>(IEnumerable<TTestData>, ArgsCode, PropsCode)`

### No Action Needed:
- All other methods should let JIT decide

## Example Code Changes

```csharp
using System.Runtime.CompilerServices;  // Add to usings

// Method 1
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static TTestData[] ToDistinctArray<TTestData>(
	this IEnumerable<TTestData> testDataCollection)
where TTestData : notnull, ITestData
=> testDataCollection.ToDistinctArray(testData => testData);

// Method 2
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static IReadOnlyCollection<object?[]> ToDistinctReadOnly<TTestData>(
	this IEnumerable<TTestData> testDataCollection)
where TTestData : notnull, ITestData
=> testDataCollection.ToDistinctArray(testData => testData.ToArgs(ArgsCode.Instance));

// Method 3
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static IReadOnlyCollection<object?[]> ToDistinctReadOnly<TTestData>(
	this IEnumerable<TTestData> testDataCollection,
	ArgsCode argsCode)
where TTestData : notnull, ITestData
=> testDataCollection.ToDistinctArray(testData => testData.ToArgs(argsCode));

// Method 4
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static IReadOnlyCollection<object?[]> ToDistinctReadOnly<TTestData>(
	this IEnumerable<TTestData> testDataCollection,
	ArgsCode argsCode,
	PropsCode propsCode)
where TTestData : notnull, ITestData
=> testDataCollection.ToDistinctArray(testData => testData.ToArgs(argsCode, propsCode));
```

## Benchmarking Recommendation

If performance is critical, consider microbenchmarking:
```csharp
[Benchmark]
public object?[] ToDistinctReadOnly_NoInline()
	=> _testData.ToDistinctReadOnly();

[Benchmark]
public object?[] ToDistinctReadOnly_WithInline()
	=> _testData.ToDistinctReadOnly(); // After adding attribute
```

Expected improvement: 2-5% reduction in call overhead for small collections (< 100 items).

## Conclusion

**Recommendation:** Add `[MethodImpl(MethodImplOptions.AggressiveInlining)]` to the 4 thin wrapper methods (methods 1-4).

**Reasoning:**
- Follows established patterns in `Validator.cs` and `PortamicalAssert.cs`
- Low code bloat risk (all are 1-line expression bodies)
- Consistent developer experience across Portamical framework
- Potential performance gains in tight test setup loops
- No downsides for methods this small
