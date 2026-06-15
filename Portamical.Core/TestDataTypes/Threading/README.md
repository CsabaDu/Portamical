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
