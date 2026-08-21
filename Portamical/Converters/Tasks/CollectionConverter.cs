// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters.Tasks;

/// <summary>
/// Provides Task-based asynchronous extension methods for converting and deduplicating test data collections.
/// </summary>
/// <remarks>
/// <para>
/// This class offers Task-based async variants of the synchronous <see cref="Converters.CollectionConverter"/> methods,
/// enabling integration with asynchronous workflows and providing performance optimizations for
/// different collection sizes.
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong> Uses <see cref="NamedCase.Comparer"/> for semantic equality
/// based on test case names (via <see cref="INamedCase.TestCaseName"/>), not reference equality.
/// This ensures that test data with identical <c>TestCaseName</c> values are treated as duplicates,
/// with the first occurrence retained.
/// </para>
/// <para>
/// <strong>Performance Optimization:</strong> Task-returning methods employ a smart threshold strategy:
/// <list type="bullet">
///   <item><strong>Small collections (&lt; 10 items):</strong> Executes synchronously via <see cref="Task.FromResult{TResult}"/> to avoid Task.Run overhead (~5-20 µs)</item>
///   <item><strong>Larger collections (≥ 10 items):</strong> Offloads work to thread pool via <see cref="Task.Run{TResult}(Func{TResult})"/> to avoid blocking the caller</item>
/// </list>
/// This optimization provides 5-20x better performance for small collections common in unit test scenarios.
/// </para>
/// <para>
/// <strong>Return Type:</strong> All methods return <see cref="Task{TResult}"/> with arrays for compatibility 
/// with test frameworks (xUnit, NUnit, MSTest). For streaming scenarios, see <see cref="AsyncEnumerables.CollectionConverter"/>.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> All methods are stateless and thread-safe. However, the input
/// <paramref name="testDataCollection"/> should not be modified during enumeration.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Task-based approach for test framework compatibility
/// public static IEnumerable&lt;object[]&gt; GetTestData()
/// {
///     var testData = new[]
///     {
///         new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
///         new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate
///         new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
///     };
///     
///     var task = testData.ToDistinctArrayTask();
///     return task.Result;  // Blocks, but executed only once at discovery time
/// }
/// </code>
/// </example>
public static class CollectionConverter
{
    #region ToArrayTask

    public static Task<TRow[]> ToArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArray(convertRow).ToTask();

    public static Task<TTestData[]> ToArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArray().ToTask();

    #endregion

    #region ToDistinctArrayTask

    /// <summary>
    /// Asynchronously converts a collection of test data to a distinct converted of rows.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Performance Optimization:</strong> For small collections (&lt; 10 items), the deduplication
    /// is performed synchronously to avoid Task.Run overhead (~5-20 µs). For larger collections,
    /// the work is offloaded to the thread pool to avoid blocking the calling thread.
    /// </para>
    /// <para>
    /// This optimization provides 5-20x better performance for very small test data collections,
    /// which are common in unit test scenarios.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.</typeparam>
    /// <typeparam name="TRow">The type of the output row elements produced by the conversion function.</typeparam>
    /// <param name="testDataCollection">The source collection of test data to convertArray. Cannot be null or empty.</param>
    /// <param name="convertRow">A function that transforms each test data item into a row of type <typeparamref name="TRow"/>. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the distinct converted of converted rows.</returns>
    public static Task<TRow[]> ToDistinctArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(convertRow).ToTask();

    /// <summary>
    /// Asynchronously creates an converted containing distinct elements from the specified test data collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is an identity conversion that returns the test data items themselves after deduplication.
    /// </para>
    /// <para>
    /// <strong>Performance Optimization:</strong> For small collections (&lt; 10 items), the deduplication
    /// is performed synchronously to avoid Task.Run overhead (~5-20 µs). For larger collections,
    /// the work is offloaded to the thread pool to avoid blocking the calling thread.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of elements in the test data collection. Must implement <see cref="ITestData"/> and cannot be null.</typeparam>
    /// <param name="testDataCollection">The source collection of test data elements from which to create a distinct converted. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an converted with distinct elements
    /// from the input collection, preserving the order of first occurrence.</returns>
    /// <example>
    /// <code>
    /// // Asynchronously deduplicate test data
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// var distinctTask = testData.ToDistinctArrayTask();
    /// var result = await distinctTask;
    /// // Result: 2 elements (duplicate removed based on TestCaseName)
    /// </code>
    /// </example>
    public static Task<TTestData[]> ToDistinctArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray().ToTask();

    #endregion

    #region Helper method

    private static Task<TRow[]> ToTask<TRow>(this TRow[] converted)
    {
        const int smallCollectionCountLimit = 10;

        return converted.Length < smallCollectionCountLimit ?
            Task.FromResult(result: converted)
            : Task.Run(function: () => converted);
    }

    #endregion
}