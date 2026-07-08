# AggressiveInlining Analysis for PortamicalAssert.cs

## Current State

Currently, `PortamicalAssert.cs` has `[MethodImpl(MethodImplOptions.AggressiveInlining)]` on:
- ? `IsNotFatal(Exception)` - Line 1203-1209 (simple pattern match, 1 line)
- ? `ThreadSafeSync(ValueTask)` - Line 1225-1227 (simple delegation, 1 line)
- ? `ThreadSafeSync<T>(ValueTask<T>)` - Line 1245-1247 (simple delegation, 1 line)
- ? `ThreadSafeSync(Func<Task>)` - Line 1305-1307 (simple delegation, 1 line)
- ? `AreApproximatelyEqual(float, float, double?)` - Line 1046-1075 (small predicate, ~30 lines)
- ? `AreApproximatelyEqual(double, double, double?)` - Line 1080-1111 (small predicate, ~30 lines)

## Recommended Methods for AggressiveInlining

### ? SHOULD Add AggressiveInlining

#### 1. `GetTypeFullName(object?)` - Lines 1011-1012
**Reason:** 1-line expression-body wrapper
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
protected static string? GetTypeFullName(object? obj)
=> GetFullName(obj?.GetType());
```
- **Size:** 1 line (expression body)
- **Complexity:** O(1) - simple null-conditional + method call
- **Call frequency:** Medium (used in error message generation)
- **Benefit:** Eliminates call overhead, allows JIT to optimize through to `GetFullName`
- **Similar pattern:** Like `CollectionConverter` thin wrappers

#### 2. `GetFullName(Type?)` - Lines 1017-1018
**Reason:** 1-line expression-body with null-coalescing
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
protected static string GetFullName(Type? obj)
=> obj?.FullName ?? "null";
```
- **Size:** 1 line (expression body)
- **Complexity:** O(1) - null-conditional + coalesce
- **Call frequency:** Medium (called by `GetTypeFullName` and directly)
- **Benefit:** Tiny method, perfect for inlining
- **Similar pattern:** Like `Validator.NotNull` (simple validation/conversion)

#### 3. `GetAssertionFailedException(string)` - Lines 1023-1024
**Reason:** 1-line expression-body constructor wrapper
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
protected static InvalidOperationException GetAssertionFailedException(string message)
=> new($"Assertion failed: {message}");
```
- **Size:** 1 line (expression body)
- **Complexity:** O(1) - string interpolation + constructor
- **Call frequency:** Low (error path only)
- **Benefit:** Reduces call overhead in error paths
- **Rationale:** Even though it's an error path, inlining is still beneficial for consistency

#### 4. `GetNotExpectedValueMessage(object, object?)` - Lines 1037-1038
**Reason:** 1-line expression-body message formatter
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
protected static string GetNotExpectedValueMessage(object expected, object? actual)
=> $"Expected '{expected}' but got '{actual ?? "null"}'.";
```
- **Size:** 1 line (expression body)
- **Complexity:** O(1) - string interpolation with null-coalesce
- **Call frequency:** Medium (called in equality assertions)
- **Benefit:** Small formatting helper, perfect candidate
- **Similar pattern:** Like other 1-line helper methods

### ? SHOULD NOT Add AggressiveInlining

#### 5. `DoesNotThrow(Action, Action<string>)` - Lines 533-548
**Reason:** Multi-line sync wrapper with closures
```csharp
// NO AggressiveInlining
public static void DoesNotThrow(Action attempt, Action<string> assertFail)
{
	_ = NotNull(attempt, nameof(attempt));
	_ = NotNull(assertFail, nameof(assertFail));

	ThreadSafeSync(DoesNotThrowAsync(() =>  // Lambda closure
	{
		attempt();
		return Task.CompletedTask;
	},
	msg =>  // Another lambda closure
	{
		assertFail(msg);
		return new ValueTask();
	}));
}
```
- **Size:** ~15 lines with validation + lambdas
- **Complexity:** Contains 2 lambda closures, validation calls
- **Reason to avoid:** Too large, contains complex lambda creation
- **Pattern:** All sync wrapper methods follow this pattern (not inlining candidates)

#### 6. `ThrowsDetails<TException>(...)` - Lines 584-658
**Reason:** Large sync wrapper (~75 lines)
```csharp
// NO AggressiveInlining - too large
```
- **Size:** ~75 lines with multiple lambda conversions
- **Complexity:** Multiple validation calls, lambda closures
- **Reason to avoid:** Way too large for inlining (JIT won't inline anyway)

#### 7. All other sync wrapper methods
- `Equality<T>(...)` - Lines 670-689
- `Equality(object, object?, ...)` - Lines 700-717
- `IsTypeOf(...)` - Lines 728-743
- `MetadataEquality<TException>(...)` - Lines 799-815
- `CatchException(Action)` - Lines 993-1002

**Reason:** All contain lambda closures and multiple lines of logic
- **Size:** 10-20 lines each
- **Pattern:** NotNull validation + ThreadSafeSync with lambda wrappers
- **Reason to avoid:** Consistent pattern - these are convenience wrappers, not hot-path helpers

#### 8. `GetNotExpectedTypeExceptionThrownMessage(Type, Type?)` - Lines 1029-1032
**Reason:** Delegates to another method (but 4 lines)
```csharp
// NO AggressiveInlining - delegates to complex method
protected static string GetNotExpectedTypeExceptionThrownMessage(Type expectedType, Type? actualType)
=> GetExpectedExceptionOfTypeMessage(
	expectedType,
	GetNotExpectedExceptionOfTypeWasThrownMessageInsert(actualType));
```
- **Size:** 4 lines (multi-line expression body)
- **Complexity:** Calls two other methods
- **Reason to avoid:** Let JIT decide; not as simple as 1-line helpers
- **Note:** If this were 1 line, it would be a candidate

#### 9. Complex async methods
- `DoesNotThrowAsync(Func<Task>, Func<string, ValueTask>)` - Lines 112-196
- `ThrowsDetailsAsync<TException>(...)` - Lines 236-288
- `EqualityAsync<T>(...)` - Lines 300-328
- `EqualityAsync(object, object?, ...)` - Lines 343-422
- `IsTypeOfAsync(...)` - Lines 437-460
- `MetadataEqualityAsync<TException>(...)` - Lines 475-489
- `CatchExceptionAsync(Func<Task>)` - Lines 823-923

**Reason:** PRIMARY implementations with complex async logic
- **Size:** 30-80+ lines each
- **Complexity:** Async state machines, multiple await points, complex logic
- **Reason to avoid:** JIT will never inline async methods with state machines
- **Note:** These are appropriately NOT marked for inlining

#### 10. `AreEqual(object?, object?, double?)` - Lines 1313-1395
**Reason:** Large switch-based comparison method (~80 lines)
```csharp
// NO AggressiveInlining - too large
private static bool AreEqual(object? expected, object? actual, double? tolerance)
{
	if (ReferenceEquals(expected, actual)) return true;
	if (expected is null || actual is null) return false;

	return (expected, actual) switch
	{
		// 30+ case arms for different types
		...
	};
}
```
- **Size:** ~80 lines with massive switch expression
- **Complexity:** Complex type pattern matching
- **Reason to avoid:** Way too large (JIT won't inline)
- **Note:** Already has internal optimizations (reference equality fast path)

## Guidelines Applied

### ? Good Candidates for AggressiveInlining:
1. **1-line expression bodies** (GetTypeFullName, GetFullName, GetAssertionFailedException, GetNotExpectedValueMessage)
2. **Simple getters/formatters** (string interpolation, property access)
3. **Thin wrappers** (already applied: ThreadSafeSync variants, IsNotFatal)
4. **Small predicates** (already applied: AreApproximatelyEqual variants)

### ? Poor Candidates:
1. **Multi-line methods** (>10 lines)
2. **Lambda closures** (all sync wrapper methods)
3. **Async state machines** (all async methods)
4. **Complex switch expressions** (AreEqual)
5. **Methods with validation + complex logic** (sync wrappers)

## Performance Impact

### Expected Benefits:
- **Reduced call overhead** for 4 tiny helper methods
- **Better JIT optimization** through call chains (GetTypeFullName ? GetFullName)
- **Consistent pattern** with existing inlined helpers
- **Negligible code bloat** (4 methods × 1-2 lines each)

### Code Bloat Risk:
- **Minimal risk**: Only 4 tiny methods (1 line each)
- **IL size increase**: ~20-40 bytes per call site (negligible)
- **Typical usage**: 5-10 call sites per helper

## Implementation Priority

### High Priority (Add Now):
1. ? `GetTypeFullName(object?)` - Called by error message generators
2. ? `GetFullName(Type?)` - Called by GetTypeFullName and directly

### Medium Priority (Consider):
3. ? `GetAssertionFailedException(string)` - Error path, but consistent pattern
4. ? `GetNotExpectedValueMessage(object, object?)` - Small formatter

### No Action Needed:
- All sync wrapper methods (intentionally NOT inlined due to lambda complexity)
- All async methods (cannot inline state machines)
- Large helper methods (AreEqual, GetNotExpectedTypeExceptionThrownMessage)

## Example Code Changes

```csharp
// Already has: using System.Runtime.CompilerServices;

// Method 1
[MethodImpl(MethodImplOptions.AggressiveInlining)]
protected static string? GetTypeFullName(object? obj)
=> GetFullName(obj?.GetType());

// Method 2
[MethodImpl(MethodImplOptions.AggressiveInlining)]
protected static string GetFullName(Type? obj)
=> obj?.FullName ?? "null";

// Method 3
[MethodImpl(MethodImplOptions.AggressiveInlining)]
protected static InvalidOperationException GetAssertionFailedException(string message)
=> new($"Assertion failed: {message}");

// Method 4
[MethodImpl(MethodImplOptions.AggressiveInlining)]
protected static string GetNotExpectedValueMessage(object expected, object? actual)
=> $"Expected '{expected}' but got '{actual ?? "null"}'.";
```

## Rationale for NOT Inlining Sync Wrappers

The sync wrapper methods like `DoesNotThrow`, `ThrowsDetails`, `Equality`, etc. are explicitly **NOT** candidates because:

1. **Lambda overhead dominates**: Each wrapper creates 1-3 lambda closures
2. **Validation calls**: Each has NotNull validation calls
3. **Size**: 10-20 lines each (too large for inlining benefit)
4. **Intent**: These are PUBLIC API convenience methods, not hot-path helpers
5. **JIT decision**: Let JIT decide based on call-site analysis

The pattern is:
```
Public Sync Wrapper (NOT inlined, has lambdas)
	? calls ThreadSafeSync (INLINED, 1 line)
		? calls Async Primary (NOT inlined, complex async)
```

This design is intentional: only the thin `ThreadSafeSync` bridge is inlined, not the wrapper or primary implementation.

## Conclusion

**Recommendation:** Add `[MethodImpl(MethodImplOptions.AggressiveInlining)]` to the 4 tiny helper methods (GetTypeFullName, GetFullName, GetAssertionFailedException, GetNotExpectedValueMessage).

**Reasoning:**
- Follows established pattern (existing inlined helpers are similar size/complexity)
- Minimal code bloat risk (all are 1-line expression bodies)
- Consistent developer experience
- Small potential performance gains in error message generation paths
- No downsides for methods this tiny

**NOT Recommended:**
- Sync wrapper methods (intentionally use lambdas, too large)
- Async methods (cannot inline state machines)
- Large helper methods (JIT won't inline anyway)
