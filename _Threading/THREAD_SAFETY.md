# Thread-Safety: Construction & Safe Publication

## ?? Overview

This document explains the **safe publication problem** in concurrent programming and how it affects `TestDataExpected<TResult>` instances.

---

## ? What is the Safe Publication Problem?

### The Issue

Even though `TestDataExpected<TResult>` instances are **immutable after construction** (all properties use `init` accessors), the .NET memory model **does not guarantee** that other threads will see the fully-constructed object without proper synchronization.

### Memory Reordering Example

```csharp
// Thread 1: Creates and publishes
var testData = new TestDataReturns<int>("Add(2,3)", 5);
// ?? Constructor executes:
//    1. base(definition) initializes parent
//    2. Expected = expected (5)
//    3. TestCaseName = CreateTestCaseName() ("Add(2,3) => returns 5")

_sharedField = testData; // Publish to other threads
```

**Without proper memory barriers, the CPU/compiler might reorder operations:**

```csharp
// What Thread 2 might see (incorrect reordering):
_sharedField = testData;        // Reference visible first! ?
Expected = 5;                   // Not yet visible
TestCaseName = "Add(2,3) => ..."// Not yet visible

// Thread 2 reads _sharedField and sees:
// - testData != null ?
// - testData.Expected == default(int) ? Should be 5!
// - testData.TestCaseName == null ? Should be "Add(2,3) => returns 5"!
```

---

## ?? When is This a Problem?

### ? UNSAFE Scenarios

1. **Static/Shared Fields Without `volatile`**
   ```csharp
   private static TestDataExpected<int>? _sharedTestData; // ? Not volatile

   // Thread 1
   _sharedTestData = new TestDataReturns<int>("Test", 5);

   // Thread 2 (might see partial object)
   if (_sharedTestData != null)
   {
	   var x = _sharedTestData.Expected; // ? Might be 0 instead of 5
   }
   ```

2. **Publishing During Construction**
   ```csharp
   public class TestDataFactory
   {
	   private static TestDataExpected<int>? _current;

	   public TestDataFactory()
	   {
		   _current = new TestDataReturns<int>("Test", 5); // ? Racing
		   // Other threads can read _current before constructor finishes
	   }
   }
   ```

3. **Collections Without Synchronization**
   ```csharp
   private static List<TestDataExpected<int>> _tests = new(); // ? Not thread-safe

   // Thread 1
   _tests.Add(new TestDataReturns<int>("Test", 5));

   // Thread 2 (concurrent read)
   var test = _tests[0]; // ? Race condition + partial object
   ```

---

## ? When is This NOT a Problem?

### Safe Scenarios (No Special Handling Needed)

1. **Local Variables (Single Thread)**
   ```csharp
   public void RunTest()
   {
	   // ? SAFE: Created and used in same thread
	   var testData = new TestDataReturns<int>("Add(2,3)", 5);
	   Assert.Equal(5, testData.Expected);
   }
   ```

2. **Method Parameters (Caller Responsibility)**
   ```csharp
   public void ProcessTest(TestDataExpected<int> testData)
   {
	   // ? SAFE: Caller ensures safe publication
	   Console.WriteLine(testData.TestCaseName);
   }
   ```

3. **Readonly Fields Initialized in Constructor**
   ```csharp
   public class TestRunner
   {
	   // ? SAFE: Readonly field + constructor initialization
	   private readonly TestDataExpected<int> _testData;

	   public TestRunner()
	   {
		   _testData = new TestDataReturns<int>("Test", 5);
		   // No other thread can see this until constructor completes
	   }
   }
   ```

---

## ??? Solutions

### 1?? Use `volatile` for Static/Shared Fields

**Use When:** Storing a single test data instance in a static/shared field.

```csharp
private static volatile TestDataExpected<int>? _cachedTestData;

// Thread 1: Write
_cachedTestData = new TestDataReturns<int>("Add(2,3)", 5);
// Memory barrier ensures all fields initialized before reference visible

// Thread 2: Read
var testData = _cachedTestData; // ? Guaranteed fully constructed
```

**How it works:**
- `volatile` inserts memory barriers
- **Write barrier:** Constructor completes ? then reference becomes visible
- **Read barrier:** Reference read ? then all fields guaranteed visible

---

### 2?? Use `Lazy<T>` for Lazy Initialization

**Use When:** Expensive test data that should be created on-demand.

```csharp
private static readonly Lazy<TestDataExpected<int>> _lazyTestData =
	new(() => new TestDataReturns<int>("HeavyComputation()", 42));

// ? SAFE: Lazy<T> handles thread-safe initialization
public static TestDataExpected<int> GetTestData()
=> _lazyTestData.Value; // First access initializes, others wait
```

**How it works:**
- Only one thread executes the factory delegate
- Other threads block until initialization completes
- All threads see the same fully-constructed instance

---

### 3?? Use `ImmutableList<T>` for Collections

**Use When:** Storing multiple test data instances in a collection.

```csharp
using System.Collections.Immutable;

private static ImmutableList<TestDataExpected<int>> _testCases =
	ImmutableList<TestDataExpected<int>>.Empty;

// Thread 1: Add
ImmutableInterlocked.Update(ref _testCases,
	list => list.Add(new TestDataReturns<int>("Test1", 5)));

// Thread 2: Add (concurrent)
ImmutableInterlocked.Update(ref _testCases,
	list => list.Add(new TestDataReturns<int>("Test2", 10)));

// Thread 3: Read (safe snapshot)
var allTests = _testCases; // ? Consistent view
```

**How it works:**
- `ImmutableInterlocked.Update` uses `Interlocked.CompareExchange` for atomic updates
- Updates create new list instances (copy-on-write)
- Readers see consistent snapshots

---

### 4?? Use `ConcurrentDictionary<TKey, TValue>` for Caching

**Use When:** High-performance concurrent access with key-based lookup.

```csharp
using System.Collections.Concurrent;

private static readonly ConcurrentDictionary<string, TestDataExpected<int>> _cache = new();

// Thread 1: Add
_cache.TryAdd("Add_2_3", new TestDataReturns<int>("Add(2,3)", 5));

// Thread 2: Get-or-add (atomic)
var testData = _cache.GetOrAdd("Add_2_3",
	key => new TestDataReturns<int>("Add(2,3)", 5));

// Thread 3: Try get
if (_cache.TryGetValue("Add_2_3", out var cached))
{
	Console.WriteLine(cached.Expected); // ? Always 5
}
```

**How it works:**
- Lock-free reads and writes (internal synchronization)
- `GetOrAdd` is atomic (only one thread creates the value)
- Safe publication guaranteed by internal memory barriers

---

### 5?? Use `Interlocked.CompareExchange` for Conditional Initialization

**Use When:** Double-checked locking pattern (first-time initialization).

```csharp
private static TestDataExpected<int>? _instance;

public static TestDataExpected<int> GetOrCreate()
{
	// First read (no lock, fast path)
	var instance = Volatile.Read(ref _instance);
	if (instance != null)
		return instance;

	// Slow path: Create new instance
	var newInstance = new TestDataReturns<int>("Compute()", 100);

	// Atomic compare-and-swap (winner takes all)
	Interlocked.CompareExchange(ref _instance, newInstance, null);

	// Return the winner (might be our instance or another thread's)
	return _instance!; // ? Guaranteed fully constructed
}
```

**How it works:**
- `Volatile.Read` ensures memory barrier on read
- `Interlocked.CompareExchange` provides atomic swap + memory barriers
- Only one thread's instance "wins" and becomes visible

---

## ?? Reference Implementations

See **`SafePublicationExamples.cs`** for complete, production-ready examples:

- `VolatileCache<TResult>` - Simple volatile field pattern
- `LazyInitializer<TResult>` - Lazy<T> pattern
- `ImmutableRepository<TResult>` - ImmutableList pattern
- `ConcurrentCache<TResult>` - ConcurrentDictionary pattern

---

## ?? Recommendations

| Scenario | Recommended Solution | Complexity |
|----------|---------------------|------------|
| Single shared instance | `volatile` field | ? Simple |
| Lazy initialization | `Lazy<T>` | ? Simple |
| Collection storage | `ImmutableList<T>` | ?? Moderate |
| High-concurrency cache | `ConcurrentDictionary<TKey,TValue>` | ?? Moderate |
| Local/method scope | No special handling | ? Simple |

---

## ?? Further Reading

- [ECMA-335 CLI Specification - Memory Model](https://www.ecma-international.org/publications-and-standards/standards/ecma-335/)
- [.NET Memory Model](https://learn.microsoft.com/en-us/dotnet/framework/performance/understanding-garbage-collection)
- [Volatile Keyword](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/volatile)
- [Interlocked Class](https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked)
- [Concurrent Collections](https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)

---

## ? Summary

**Key Takeaways:**

1. ? **`TestDataExpected<TResult>` is immutable** after construction (`init` accessors)
2. ?? **Sharing across threads requires safe publication** (memory barriers)
3. ??? **Use:** `volatile`, `Lazy<T>`, `ImmutableList<T>`, or `ConcurrentDictionary<TKey,TValue>`
4. ?? **Local instances need no special handling** (single-threaded usage is safe)

**When in doubt:** Use the patterns in `SafePublicationExamples.cs`! ??
