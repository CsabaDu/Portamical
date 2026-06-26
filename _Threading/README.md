# Thread-Safety Documentation Index

## ?? Overview

This directory contains comprehensive thread-safety documentation for the Portamical test data framework.

---

## ?? Documents

### 1. **THREAD_SAFETY.md** - Safe Publication Guide
**Purpose:** Explains the safe publication problem and how to share test data across threads.

**Key Topics:**
- What is the safe publication problem?
- When is it a problem? (unsafe scenarios)
- When is it NOT a problem? (safe scenarios)
- 5 solution patterns with code examples:
  1. `volatile` fields
  2. `Lazy<T>`
  3. `ImmutableList<T>`
  4. `ConcurrentDictionary<TKey, TValue>`
  5. `Interlocked` operations

**Audience:** All developers sharing test data instances across threads.

**Read this if:**
- You're storing test data in static/shared fields
- You're caching test data in collections
- You're passing test data between threads
- You're seeing inconsistent values in concurrent tests

---

### 2. **BASE_CLASSES_THREAD_SAFETY.md** - Base Class Analysis
**Purpose:** Detailed thread-safety analysis of core test data base classes.

**Classes Analyzed:**
- `TestDataBase` ? Thread-safe
- `TestData` ? Thread-safe
- `NamedCase` ? Thread-safe
- `TestDataExpected<TResult>` ? Thread-safe (see original context)
- Supporting utilities (`Resolver`, `Formatter`, `Registration`) ? Thread-safe

**Key Topics:**
- Field-by-field immutability analysis
- Method-by-method safety verification
- Dependency verification (helpers, utilities)
- Best practices for derived classes
- Safe vs. unsafe patterns for inheritance

**Audience:** Framework developers and advanced users creating custom test data types.

**Read this if:**
- You're creating custom test data classes
- You're debugging thread-safety issues in derived types
- You want to understand the internal safety guarantees
- You're reviewing/auditing the codebase

---

### 3. **SafePublicationExamples.cs** - Reference Implementations
**Purpose:** Production-ready code examples for safe multi-threaded usage.

**Classes Provided:**
- `VolatileCache<TResult>` - Simple volatile field pattern
- `LazyInitializer<TResult>` - Thread-safe lazy initialization
- `ImmutableRepository<TResult>` - Lock-free collection storage
- `ConcurrentCache<TResult>` - High-performance concurrent dictionary

**Key Features:**
- Fully documented with XML comments
- Working code you can copy/paste
- Examples in XML doc comments
- Thread-safety guarantees explained

**Audience:** Developers implementing multi-threaded test scenarios.

**Read this if:**
- You need working code for thread-safe caching
- You want to see best practices in action
- You're implementing a test data repository
- You need a quick reference implementation

---

## ?? Quick Decision Tree

```
Do you need to share test data across threads?
?
?? YES
?  ?
?  ?? Single instance in a static field?
?  ?  ?? Use: VolatileCache<T> or volatile field
?  ?     See: THREAD_SAFETY.md § Solution 1
?  ?
?  ?? Lazy initialization?
?  ?  ?? Use: LazyInitializer<T> or Lazy<T>
?  ?     See: THREAD_SAFETY.md § Solution 2
?  ?
?  ?? Collection of instances?
?  ?  ?? Use: ImmutableRepository<T> or ImmutableList<T>
?  ?     See: THREAD_SAFETY.md § Solution 3
?  ?
?  ?? High-performance cache with keys?
?  ?  ?? Use: ConcurrentCache<T> or ConcurrentDictionary<K,V>
?  ?     See: THREAD_SAFETY.md § Solution 4
?  ?
?  ?? Custom scenario?
?     ?? Use: Interlocked operations
?        See: THREAD_SAFETY.md § Solution 5
?
?? NO (local/method scope only)
   ?? No special handling needed! ?
	  Test data is already thread-safe after construction.
```

---

## ?? Common Scenarios

### Scenario 1: Static Test Data Field

**Problem:**
```csharp
private static TestDataExpected<int>? _sharedTestData; // ? Not thread-safe
```

**Solution:**
```csharp
private static volatile TestDataExpected<int>? _sharedTestData; // ? Thread-safe
```

**Reference:** THREAD_SAFETY.md § Solution 1, SafePublicationExamples.cs `VolatileCache<T>`

---

### Scenario 2: Expensive Test Data Creation

**Problem:**
```csharp
private static TestDataExpected<int>? _cachedData;

public static TestDataExpected<int> GetTestData()
{
	if (_cachedData == null) // ? Race condition
	{
		_cachedData = CreateExpensiveTestData();
	}
	return _cachedData;
}
```

**Solution:**
```csharp
private static readonly Lazy<TestDataExpected<int>> _lazyData =
	new(() => CreateExpensiveTestData(), isThreadSafe: true); // ? Thread-safe

public static TestDataExpected<int> GetTestData()
	=> _lazyData.Value;
```

**Reference:** THREAD_SAFETY.md § Solution 2, SafePublicationExamples.cs `LazyInitializer<T>`

---

### Scenario 3: Test Data Collection

**Problem:**
```csharp
private static List<TestDataExpected<int>> _testCases = new(); // ? Not thread-safe

// Thread 1
_testCases.Add(test1);

// Thread 2
var all = _testCases.ToArray(); // ? Race condition
```

**Solution:**
```csharp
private static ImmutableList<TestDataExpected<int>> _testCases =
	ImmutableList<TestDataExpected<int>>.Empty; // ? Thread-safe

// Thread 1
ImmutableInterlocked.Update(ref _testCases, list => list.Add(test1));

// Thread 2
var all = _testCases; // ? Safe snapshot
```

**Reference:** THREAD_SAFETY.md § Solution 3, SafePublicationExamples.cs `ImmutableRepository<T>`

---

### Scenario 4: Test Data Cache by Key

**Problem:**
```csharp
private static Dictionary<string, TestDataExpected<int>> _cache = new(); // ? Not thread-safe
```

**Solution:**
```csharp
private static readonly ConcurrentDictionary<string, TestDataExpected<int>> _cache = new(); // ? Thread-safe

// Thread-safe get-or-add
var testData = _cache.GetOrAdd("key", k => new TestDataReturns<int>("Test", 5));
```

**Reference:** THREAD_SAFETY.md § Solution 4, SafePublicationExamples.cs `ConcurrentCache<T>`

---

### Scenario 5: Custom Derived Class

**Problem:**
```csharp
public class MyTestData : TestDataExpected<int>
{
	private string _counter = "0"; // ? Mutable field

	public void Increment()
	{
		_counter = (int.Parse(_counter) + 1).ToString(); // ? Race condition
	}
}
```

**Solution:**
```csharp
public class MyTestData : TestDataExpected<int>
{
	private readonly string _customField; // ? Readonly field

	public MyTestData(string definition, int expected, string custom)
		: base(definition, expected)
	{
		_customField = custom; // Set once in constructor
	}

	public string CustomField => _customField; // ? Readonly property
}
```

**Reference:** BASE_CLASSES_THREAD_SAFETY.md § Inheritance Safety

---

## ? Verification Checklist

Use this checklist to verify your code is thread-safe:

### For Base Class Users

- [ ] Test data instances are immutable (no settable properties)
- [ ] Static/shared fields use proper publication (volatile, Lazy<T>, etc.)
- [ ] Collections use thread-safe types (ImmutableList, ConcurrentDictionary)
- [ ] No lazy initialization without locks or Lazy<T>
- [ ] No mutation after construction

### For Custom Derived Classes

- [ ] All new fields are `readonly`
- [ ] All new properties use `init` accessor
- [ ] No settable properties (`set` accessor)
- [ ] No lazy initialization (or use `Lazy<T>` with `isThreadSafe: true`)
- [ ] Override methods create new objects (don't mutate existing)
- [ ] No mutable static fields (or use atomic operations)
- [ ] Thread-safety documented in XML comments

---

## ?? API Reference

### Thread-Safe Types

| Type | Purpose | Thread-Safety |
|------|---------|---------------|
| `TestDataBase` | Base for all test data | ? Immutable after construction |
| `TestData` | General-purpose test data | ? Immutable after construction |
| `TestDataExpected<T>` | Test data with expected result | ? Immutable after construction |
| `NamedCase` | Identity object base | ? Stateless + immutable instances |
| `Resolver` | Fallback utilities | ? Atomic counter operations |
| `Formatter` | String formatting | ? Stateless static methods |
| `Registration` | Formatter registry | ? ConcurrentDictionary-based |

### Thread-Safe Helper Classes

| Class | Purpose | Use When |
|-------|---------|----------|
| `VolatileCache<T>` | Simple static field | Single shared instance |
| `LazyInitializer<T>` | Lazy creation | Expensive initialization |
| `ImmutableRepository<T>` | Collection storage | Multiple instances |
| `ConcurrentCache<T>` | Key-based cache | High-concurrency lookups |

---

## ?? Getting Started

1. **Read THREAD_SAFETY.md** to understand safe publication
2. **Browse SafePublicationExamples.cs** for working code
3. **Read BASE_CLASSES_THREAD_SAFETY.md** if creating custom classes
4. **Use the decision tree above** to pick the right pattern
5. **Verify with the checklist** before deploying

---

## ?? Troubleshooting

### Issue: "Sometimes test data properties are null/default"

**Cause:** Safe publication problem - other threads see partially-constructed objects.

**Solution:** Use volatile field, Lazy<T>, or immutable collections. See THREAD_SAFETY.md § Safe Publication.

---

### Issue: "ConcurrentModificationException in test data collection"

**Cause:** Using non-thread-safe collection type (List<T>, Dictionary<K,V>).

**Solution:** Use ImmutableList<T> or ConcurrentDictionary<K,V>. See THREAD_SAFETY.md § Solution 3 or 4.

---

### Issue: "Lazy initialization creating multiple instances"

**Cause:** Race condition in double-check locking without proper synchronization.

**Solution:** Use Lazy<T> with `isThreadSafe: true`. See THREAD_SAFETY.md § Solution 2.

---

### Issue: "Custom derived class not thread-safe"

**Cause:** Introduced mutable fields or settable properties.

**Solution:** Follow immutability guidelines in BASE_CLASSES_THREAD_SAFETY.md § Inheritance Safety.

---

## ?? Additional Resources

- [C# Memory Model](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/memory-model)
- [Thread-Safe Collections](https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)
- [Interlocked Class](https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked)
- [Volatile Keyword](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/volatile)
- [Lazy<T> Class](https://learn.microsoft.com/en-us/dotnet/api/system.lazy-1)

---

## ?? Document Change History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-01-XX | Initial documentation |
|     |            | - THREAD_SAFETY.md created |
|     |            | - BASE_CLASSES_THREAD_SAFETY.md created |
|     |            | - SafePublicationExamples.cs created |
|     |            | - README.md created |

---

**All base classes are thread-safe! No code changes required.** ??

For questions or issues, see the troubleshooting section above or consult the detailed documents.

---

## Safe Publication Examples

```csharp
// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.TestDataTypes.Models.Specialized;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Portamical.Core.TestDataTypes.Threading;

/// <summary>
/// Demonstrates safe publication patterns for sharing <see cref="TestDataExpected{TResult}"/> instances across threads.
/// </summary>
/// <remarks>
/// <para>
/// While <see cref="TestDataExpected{TResult}"/> instances are immutable after construction,
/// sharing them across threads requires proper publication to ensure all threads see the fully-initialized object.
/// This class provides reference implementations of common thread-safe patterns.
/// </para>
/// <para>
/// <strong>Choose the right pattern:</strong>
/// <list type="bullet">
///   <item><see cref="VolatileCache{TResult}"/> - Simple static/shared field storage</item>
///   <item><see cref="LazyInitializer{TResult}"/> - Thread-safe lazy initialization</item>
///   <item><see cref="ImmutableRepository{TResult}"/> - Concurrent collection storage</item>
///   <item><see cref="ConcurrentCache{TResult}"/> - High-performance concurrent dictionary</item>
/// </list>
/// </para>
/// </remarks>
public static class SafePublicationExamples
{
    /// <summary>
    /// Demonstrates safe publication using <c>volatile</c> fields.
    /// </summary>
    /// <typeparam name="TResult">The type of the expected result.</typeparam>
    /// <remarks>
    /// <para>
    /// <strong>Use When:</strong> Storing a single test data instance in a static/shared field.
    /// </para>
    /// <para>
    /// <strong>Memory Barrier:</strong> The <c>volatile</c> keyword ensures:
    /// <list type="number">
    ///   <item>Constructor completes (including all field initializations)</item>
    ///   <item>Memory barrier inserted</item>
    ///   <item>Reference becomes visible to other threads</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Thread-Safety:</strong> ? Safe for concurrent reads and writes.
    /// Multiple writes may race, but the winner is always fully constructed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Thread 1: Writes
    /// VolatileCache&lt;int&gt;.Set(new TestDataReturns&lt;int&gt;("Add(2,3)", 5));
    /// 
    /// // Thread 2: Reads (safe - guaranteed fully constructed)
    /// var testData = VolatileCache&lt;int&gt;.Get();
    /// if (testData != null)
    /// {
    ///     var expected = testData.Expected; // ? Always initialized
    /// }
    /// </code>
    /// </example>
    public static class VolatileCache<TResult>
        where TResult : notnull
    {
        // ? SAFE: volatile ensures all threads see fully-constructed instance
        private static volatile TestDataExpected<TResult>? _cachedInstance;

        /// <summary>
        /// Sets the cached instance. Thread-safe.
        /// </summary>
        public static void Set(TestDataExpected<TResult> instance)
        {
            ArgumentNullException.ThrowIfNull(instance);
            _cachedInstance = instance; // Memory barrier after constructor
        }

        /// <summary>
        /// Gets the cached instance. Thread-safe. May return null if not set.
        /// </summary>
        public static TestDataExpected<TResult>? Get()
        => _cachedInstance; // Memory barrier before read
    }

    /// <summary>
    /// Demonstrates safe publication using <see cref="Lazy{T}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the expected result.</typeparam>
    /// <remarks>
    /// <para>
    /// <strong>Use When:</strong> Lazy initialization of expensive test data (e.g., loading from database, file, or computation).
    /// </para>
    /// <para>
    /// <strong>Thread-Safety:</strong> ? <see cref="Lazy{T}"/> guarantees:
    /// <list type="bullet">
    ///   <item>Only one thread executes the factory delegate</item>
    ///   <item>Other threads block until initialization completes</item>
    ///   <item>All threads see the same fully-constructed instance</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> First access incurs initialization cost; subsequent accesses are fast (no locks).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Initialization
    /// LazyInitializer&lt;int&gt;.Initialize(() => new TestDataReturns&lt;int&gt;("HeavyComputation()", 42));
    /// 
    /// // Thread-safe access (blocks until initialized)
    /// var testData = LazyInitializer&lt;int&gt;.Get();
    /// var expected = testData.Expected; // ? Always 42
    /// </code>
    /// </example>
    public static class LazyInitializer<TResult>
        where TResult : notnull
    {
        private static Lazy<TestDataExpected<TResult>>? _lazyInstance;

        /// <summary>
        /// Initializes the lazy instance with a factory delegate. Not thread-safe - call once during startup.
        /// </summary>
        public static void Initialize(Func<TestDataExpected<TResult>> factory)
        {
            ArgumentNullException.ThrowIfNull(factory);
            _lazyInstance = new Lazy<TestDataExpected<TResult>>(factory, isThreadSafe: true);
        }

        /// <summary>
        /// Gets the lazily-initialized instance. Thread-safe. Blocks until initialization completes.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Initialize"/> was not called.</exception>
        public static TestDataExpected<TResult> Get()
        {
            if (_lazyInstance == null)
            {
                throw new InvalidOperationException(
                    $"Call {nameof(Initialize)} before {nameof(Get)}.");
            }

            return _lazyInstance.Value; // ? Thread-safe initialization
        }
    }

    /// <summary>
    /// Demonstrates safe publication using <see cref="ImmutableList{T}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the expected result.</typeparam>
    /// <remarks>
    /// <para>
    /// <strong>Use When:</strong> Storing multiple test data instances in a collection.
    /// </para>
    /// <para>
    /// <strong>Thread-Safety:</strong> ? <see cref="ImmutableList{T}"/> guarantees:
    /// <list type="bullet">
    ///   <item>Updates create new list instances (copy-on-write)</item>
    ///   <item><see cref="ImmutableInterlocked"/> ensures atomic updates</item>
    ///   <item>Readers see consistent snapshots</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> O(log n) updates due to structural sharing. Reads are O(1) for snapshot access, O(n) for enumeration.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Thread 1: Adds test case
    /// ImmutableRepository&lt;int&gt;.Add(new TestDataReturns&lt;int&gt;("Add(2,3)", 5));
    /// 
    /// // Thread 2: Adds test case (concurrent with Thread 1)
    /// ImmutableRepository&lt;int&gt;.Add(new TestDataReturns&lt;int&gt;("Multiply(2,3)", 6));
    /// 
    /// // Thread 3: Reads all (safe snapshot)
    /// var allTests = ImmutableRepository&lt;int&gt;.GetAll();
    /// foreach (var test in allTests)
    /// {
    ///     Console.WriteLine(test.TestCaseName); // ? Consistent view
    /// }
    /// </code>
    /// </example>
    public static class ImmutableRepository<TResult>
        where TResult : notnull
    {
        // ? SAFE: ImmutableList with ImmutableInterlocked updates
        private static ImmutableList<TestDataExpected<TResult>> _testCases = [];
            //ImmutableList<TestDataExpected<TResult>>.Empty;

        /// <summary>
        /// Adds a test case to the repository. Thread-safe.
        /// </summary>
        public static void Add(TestDataExpected<TResult> testData)
        {
            ArgumentNullException.ThrowIfNull(testData);
            // Atomic update: retries if another thread modified _testCases
            ImmutableInterlocked.Update(ref _testCases, list => list.Add(testData));
        }

        /// <summary>
        /// Gets all test cases as an immutable snapshot. Thread-safe.
        /// </summary>
        public static ImmutableList<TestDataExpected<TResult>> GetAll()
        => _testCases; // Returns current snapshot
    }

    /// <summary>
    /// Demonstrates safe publication using <see cref="ConcurrentDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the expected result.</typeparam>
    /// <remarks>
    /// <para>
    /// <strong>Use When:</strong> Caching test data by key with high-performance concurrent access.
    /// </para>
    /// <para>
    /// <strong>Thread-Safety:</strong> ? <see cref="ConcurrentDictionary{TKey,TValue}"/> provides:
    /// <list type="bullet">
    ///   <item>Lock-free reads and writes</item>
    ///   <item>Atomic get-or-add operations</item>
    ///   <item>Safe publication of stored instances</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> O(1) reads and writes with minimal contention. Best choice for high-concurrency scenarios.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Thread 1: Adds test case
    /// var test1 = new TestDataReturns&lt;int&gt;("Add(2,3)", 5);
    /// ConcurrentCache&lt;int&gt;.Add("Add_2_3", test1);
    /// 
    /// // Thread 2: Gets or creates (if Thread 1 already added, returns same instance)
    /// var test2 = ConcurrentCache&lt;int&gt;.GetOrAdd(
    ///     "Add_2_3",
    ///     key => new TestDataReturns&lt;int&gt;("Add(2,3)", 5));
    /// 
    /// // Thread 3: Tries to get
    /// if (ConcurrentCache&lt;int&gt;.TryGet("Add_2_3", out var cached))
    /// {
    ///     Console.WriteLine(cached.Expected); // ? Always 5
    /// }
    /// </code>
    /// </example>
    public static class ConcurrentCache<TResult>
        where TResult : notnull
    {
        // ? SAFE: ConcurrentDictionary handles safe publication internally
        private static readonly ConcurrentDictionary<string, TestDataExpected<TResult>> _cache = new();

        /// <summary>
        /// Adds a test case with the specified key. Thread-safe.
        /// </summary>
        /// <returns><see langword="true"/> if added; <see langword="false"/> if key already exists.</returns>
        public static bool Add(string key, TestDataExpected<TResult> testData)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(testData);
            return _cache.TryAdd(key, testData);
        }

        /// <summary>
        /// Gets the test case for the specified key, or adds it using the factory if not present. Thread-safe and atomic.
        /// </summary>
        public static TestDataExpected<TResult> GetOrAdd(
            string key,
            Func<string, TestDataExpected<TResult>> factory)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(factory);
            return _cache.GetOrAdd(key, factory); // ? Atomic get-or-add
        }

        /// <summary>
        /// Tries to get the test case for the specified key. Thread-safe.
        /// </summary>
        public static bool TryGet(string key, out TestDataExpected<TResult>? testData)
        {
            ArgumentNullException.ThrowIfNull(key);
            return _cache.TryGetValue(key, out testData);
        }

        /// <summary>
        /// Clears all cached test cases. Thread-safe.
        /// </summary>
        public static void Clear()
        => _cache.Clear();
    }
}

```