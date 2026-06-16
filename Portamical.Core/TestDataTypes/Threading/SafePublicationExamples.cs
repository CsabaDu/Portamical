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
