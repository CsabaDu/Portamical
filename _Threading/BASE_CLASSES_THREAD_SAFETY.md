# Thread-Safety Analysis: TestData Base Classes

## ?? Overview

This document analyzes the thread-safety of the core test data base classes:
- **TestDataBase.cs**
- **TestData.cs**
- **NamedCase.cs**

All supporting utility classes (`Resolver`, `Formatter`, `Registration`) are also reviewed.

---

## ? Summary: All Classes Are Thread-Safe

| Class | Thread-Safe? | Notes |
|-------|-------------|-------|
| **TestDataBase** | ? Yes | Immutable after construction |
| **TestData** | ? Yes | Immutable after construction |
| **NamedCase** | ? Yes | Stateless + immutable instances |
| **Resolver** | ? Yes | Atomic operations for shared state |
| **Formatter** | ? Yes | Stateless static methods |
| **Registration** | ? Yes | ConcurrentDictionary for registry |

**Conclusion:** All base classes are thread-safe. Safe publication guidance (from previous document) still applies.

---

## ?? Detailed Analysis

### 1?? **TestDataBase.cs** - ? Thread-Safe

#### Immutable State
```csharp
public abstract class TestDataBase(string definition) : NamedCase, ITestData
{
	// Primary constructor parameter - captured as readonly field
	private readonly string definition; // ? Immutable

	// Constants - inherently thread-safe
	private const string DefinitionString = "definition";      // ?
	private const string Separator = " => ";                   // ?
}
```

#### Thread-Safe Operations
- **`GetDefinition()`** - Reads immutable `definition` field ?
- **`CreateTestCaseName()`** - Creates new strings using `string.Create()` ?
  - Uses local variables only
  - No shared mutable state
- **`ToArgs()` / `ToObjectArray()`** - Create new arrays ?
  - No mutation of existing state
  - Virtual dispatch is thread-safe (CLR guarantees)
- **`Extend()` / `Trim()`** - Static helper methods ?
  - Stateless
  - Create new arrays

#### Dependencies
- ? `FallbackIfNullOrWhiteSpace` from `Resolver` - **Thread-safe** (atomic counter)
- ? `CreateSeparatedString` from `Formatter` - **Thread-safe** (stateless)

**Verdict:** TestDataBase is fully thread-safe. Instances are immutable after construction.

---

### 2?? **TestData.cs** - ? Thread-Safe

#### Immutable State
```csharp
public abstract class TestData : TestDataBase
{
	private readonly string _result; // ? Readonly field

	public override sealed string TestCaseName { get; init; } // ? Init-only

	private protected TestData(string definition, string result)
		: base(definition)
	{
		_result = result;                    // Set once
		TestCaseName = CreateTestCaseName(); // Set once
	}
}
```

#### Thread-Safe Operations
- **`GetResult()`** - Reads readonly `_result` field ?
  - Calls `FallbackIfNullOrWhiteSpace` (thread-safe)
- **`ToArgs()`** - Calls base method ?
  - No additional mutable state

**Verdict:** TestData is fully thread-safe. All fields are readonly or init-only.

---

### 3?? **NamedCase.cs** - ? Thread-Safe

#### Immutable State
```csharp
public abstract class NamedCase : INamedCase
{
	// Abstract property - implemented by derived classes with init accessor
	public abstract string TestCaseName { get; init; } // ?

	// Static readonly comparer - initialized once
	public static IEqualityComparer<INamedCase> Comparer { get; } =
		new NamedCaseEqualityComparer(); // ?
}
```

#### Thread-Safe Components

**Nested Comparer Class:**
```csharp
private sealed class NamedCaseEqualityComparer : IEqualityComparer<INamedCase>
{
	public bool Equals(INamedCase? x, INamedCase? y)
	{
		if (ReferenceEquals(x, y)) return true;  // ? Safe
		if (x is null || y is null) return false; // ? Safe

		// StringComparer.Ordinal is thread-safe (stateless)
		return StringComparer.Ordinal.Equals(
			x.TestCaseName,
			y.TestCaseName); // ? Safe
	}

	public int GetHashCode(INamedCase obj)
	{
		var testCaseName = NotNull(obj, nameof(obj)).TestCaseName ?? string.Empty;

		// StringComparer.Ordinal is thread-safe (stateless)
		return StringComparer.Ordinal.GetHashCode(testCaseName); // ? Safe
	}
}
```

**Static Methods:**
- All static methods are **stateless** ?
- `Contains()` creates local snapshots (`INamedCase[]`) ?
- `CreateDisplayName()` overloads create new strings ?

**Instance Methods:**
- `ContainedBy()` - Delegates to static `Contains()` ?
- `GetDisplayName()` - Delegates to static `CreateDisplayName()` ?
- `Equals()` - Uses thread-safe `Comparer` ?
- `GetHashCode()` - Uses thread-safe `Comparer` ?
- `ToString()` - Returns immutable `TestCaseName` ?

**Verdict:** NamedCase is fully thread-safe. All operations are stateless or use immutable state.

---

## ??? Supporting Utilities Thread-Safety

### **Resolver.cs** - ? Thread-Safe

#### Shared Mutable State
```csharp
public static class Resolver
{
	// Shared mutable state - requires atomic operations
	private static long LogCounter; // ?? Mutable
}
```

#### Thread-Safe Operations

**FallbackIfNullOrWhiteSpace:**
```csharp
public static string FallbackIfNullOrWhiteSpace(
	this string fallbackLabel,
	string? preferredValue,
	string methodName)
{
	if (string.IsNullOrWhiteSpace(preferredValue))
	{
		// ? Thread-safe atomic increment
		var logIndex = IncrementLogIndex(out string logPrefix);

		var indexedFallback = $"{fallbackLabel} ({logIndex})";

		// ? Trace.TraceWarning is thread-safe
		Trace.TraceWarning(...);

		return indexedFallback;
	}

	return preferredValue;
}
```

**IncrementLogIndex:**
```csharp
private static long IncrementLogIndex(out string logPrefix)
{
	// ? Interlocked.Increment provides atomic increment
	// Multiple threads get unique sequential indices
	var logIndex = Interlocked.Increment(ref LogCounter);
	logPrefix = $"Portamical log {logIndex}: ";
	return logIndex;
}
```

**ResetLogCounter:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static long ResetLogCounter()
	// ? Interlocked.Exchange provides atomic reset
	=> Interlocked.Exchange(ref LogCounter, 0L);
```

**Thread-Safety Guarantees:**
- `Interlocked.Increment` - Lock-free atomic increment ?
- `Interlocked.Exchange` - Lock-free atomic swap ?
- `Trace.TraceWarning` - Thread-safe (uses thread-safe listeners) ?

**Verdict:** Resolver is fully thread-safe. All mutable state uses atomic operations.

---

### **Formatter.cs** - ? Thread-Safe

#### All Static Methods
```csharp
public static class Formatter
{
	// Constant - inherently thread-safe
	public const string NullString = "null"; // ?
	public const int MaxCount = 3;           // ?

	// All methods are static and stateless
	public static string CreateSeparatedString(...) // ?
	public static string JoinWithComma(...)         // ?
	public static void CopyAsSpan(...)              // ?
	// ...etc
}
```

**CreateSeparatedString:**
```csharp
public static string CreateSeparatedString(
	int totalLength,
	string baseString,
	string separator,
	string appendix)
	=> string.Create(
		totalLength,
		(baseString, separator, appendix), // Tuple captures state
		static (span, state) =>
		{
			var (b, sep, app) = state;

			// ? Local variables only
			var i = 0;
			CopyAsSpan(b, span, i);

			i = b.Length;
			CopyAsSpan(sep, span, i);

			i += sep.Length;
			CopyAsSpan(app, span, i);
		});
```

**Thread-Safety:**
- No shared mutable state ?
- All operations on local variables or parameters ?
- `string.Create()` provides isolated span for each call ?

**Verdict:** Formatter is fully thread-safe. All methods are stateless.

---

### **Registration.cs** - ? Thread-Safe

Already analyzed in detail in the previous document.

**Summary:**
- Uses `ConcurrentDictionary<Type, IFormatter>` ?
- All operations use lock-free atomic methods ?
- `TryAdd`, `TryRemove`, `TryGetValue`, `Clear` are thread-safe ?

---

## ?? Inheritance Safety

### Derived Class Requirements

For derived classes to maintain thread-safety:

#### ? **Safe Patterns**

1. **Readonly Fields**
   ```csharp
   public class MyTestData : TestData
   {
	   private readonly string _customField; // ? Readonly

	   public MyTestData(string definition, string result, string custom)
		   : base(definition, result)
	   {
		   _customField = custom; // Set once in constructor
	   }
   }
   ```

2. **Init-Only Properties**
   ```csharp
   public class MyTestData : TestData
   {
	   public string CustomProperty { get; init; } // ? Init-only

	   public MyTestData(string definition, string result, string custom)
		   : base(definition, result)
	   {
		   CustomProperty = custom; // Set once in constructor
	   }
   }
   ```

3. **Computed Properties (No State)**
   ```csharp
   public class MyTestData : TestData
   {
	   public int Length => TestCaseName.Length; // ? Computed
   }
   ```

#### ? **Unsafe Patterns**

1. **Mutable Fields**
   ```csharp
   public class UnsafeTestData : TestData
   {
	   private string _counter = "0"; // ? Mutable field

	   public void Increment()
	   {
		   // ? Race condition!
		   _counter = (int.Parse(_counter) + 1).ToString();
	   }
   }
   ```

2. **Set Accessors**
   ```csharp
   public class UnsafeTestData : TestData
   {
	   public string CustomProperty { get; set; } // ? Settable

	   // Multiple threads can modify this!
   }
   ```

3. **Lazy Initialization Without Locking**
   ```csharp
   public class UnsafeTestData : TestData
   {
	   private string? _cachedValue; // ? Lazy field without lock

	   public string GetCachedValue()
	   {
		   if (_cachedValue == null) // ? Race condition!
		   {
			   _cachedValue = ExpensiveComputation();
		   }
		   return _cachedValue;
	   }
   }
   ```

   **Fix:** Use `Lazy<T>`
   ```csharp
   public class SafeTestData : TestData
   {
	   private readonly Lazy<string> _cachedValue; // ? Thread-safe lazy

	   public SafeTestData(string definition, string result)
		   : base(definition, result)
	   {
		   _cachedValue = new Lazy<string>(
			   ExpensiveComputation,
			   isThreadSafe: true); // ? Thread-safe mode
	   }

	   public string GetCachedValue() => _cachedValue.Value; // ? Safe
   }
   ```

---

## ?? Best Practices for Derived Classes

### ? Recommended Guidelines

1. **Immutability First**
   - Use `readonly` fields for all instance state
   - Use `init` accessors for properties
   - Set all values in constructor only

2. **No Lazy Initialization (Unless Using `Lazy<T>`)**
   - Avoid conditional field initialization
   - If needed, use `Lazy<T>` with `isThreadSafe: true`

3. **Override Virtual Methods Safely**
   - Don't introduce mutable state in overrides
   - Create new objects rather than mutating existing ones
   ```csharp
   protected override object?[] ToObjectArray(ArgsCode argsCode)
   {
	   // ? Creates new array, doesn't mutate
	   return Extend(base.ToObjectArray, argsCode, CustomProperty);
   }
   ```

4. **Document Thread-Safety in XML Comments**
   ```csharp
   /// <remarks>
   /// <para>
   /// <strong>Thread-Safety:</strong> This class is immutable after construction
   /// and safe to share across threads. Ensure proper safe publication when
   /// storing in static/shared fields (see THREAD_SAFETY.md).
   /// </para>
   /// </remarks>
   public class MyTestData : TestData
   {
	   // ...
   }
   ```

5. **Test for Race Conditions**
   ```csharp
   [TestMethod]
   public void TestData_ConcurrentAccess_IsThreadSafe()
   {
	   var testData = new MyTestData("Test", "result", "custom");

	   // Run 100 threads concurrently
	   Parallel.For(0, 100, i =>
	   {
		   // ? All reads should be safe
		   var name = testData.TestCaseName;
		   var args = testData.ToArgs(ArgsCode.Properties);
		   var expected = testData.Expected;

		   // Verify consistency
		   Assert.IsNotNull(name);
		   Assert.IsNotNull(args);
		   Assert.IsNotNull(expected);
	   });
   }
   ```

---

## ? Conclusion

**All core test data base classes are thread-safe:**

| Class | Immutability | Shared State | Verdict |
|-------|-------------|--------------|---------|
| TestDataBase | ? Primary constructor parameter (readonly) | None | ? Safe |
| TestData | ? Readonly field + init properties | None | ? Safe |
| NamedCase | ? Abstract init property | Static readonly comparer | ? Safe |
| Resolver | N/A (static) | Atomic counter (`Interlocked`) | ? Safe |
| Formatter | N/A (static) | None (stateless) | ? Safe |
| Registration | N/A (static) | `ConcurrentDictionary` | ? Safe |

**Key Takeaways:**

1. ? **Base classes are immutable** - Safe to use concurrently after construction
2. ? **Utilities use atomic operations** - `Interlocked` for counters, `ConcurrentDictionary` for registry
3. ? **Static methods are stateless** - No shared mutable state
4. ?? **Safe publication still required** - See `THREAD_SAFETY.md` for guidance
5. ? **Derived classes can maintain safety** - Follow immutability guidelines

**No changes needed** - All classes are already thread-safe! ??

---

## ?? Further Reading

- **THREAD_SAFETY.md** - Safe publication patterns
- **SafePublicationExamples.cs** - Reference implementations
- [C# Memory Model](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/memory-model)
- [Thread-Safe Collections](https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)
- [Interlocked Class](https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked)
