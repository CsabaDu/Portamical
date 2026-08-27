// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using static Portamical.Converters.CollectionConverter;

namespace Portamical.Converters.Tasks;

/// <summary>
/// Provides Task-based asynchronous extension methods for converting and deduplicating test data collections.
/// </summary>
/// <remarks>
/// <para>
/// This class offers Task-based async variants of the synchronous <see cref="Converters.CollectionConverter"/> methods,
/// enabling integration with asynchronous workflows and providing performance optimizations for
/// different testDataCollection sizes.
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
///   <item><strong>Small collections (&lt; 100 items):</strong> Executes synchronously via <see cref="Task.FromResult{TResult}"/> to avoid Task.Run overhead</item>
///   <item><strong>Larger collections (≥ 100 items):</strong> Offloads work to thread pool via <see cref="Task.Run{TResult}(Func{TResult})"/> for parallel execution</item>
/// </list>
/// The threshold of 100 items is based on BenchmarkDotNet measurements showing this as the empirical break-even point
/// where Task.Run benefits outweigh its overhead (~5.8µs synchronous vs ~5.8µs async at 100 items).
/// </para>
/// <para>
/// <strong>Return Type:</strong> All methods return <see cref="Task{TResult}"/> with arrays for compatibility 
/// with test frameworks (xUnit, NUnit, MSTest). For streaming scenarios, see <see cref="AsyncEnumerables.CollectionConverter"/>.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> All methods are stateless and thread-safe. However, input
/// collections should not be modified during enumeration.
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

    /// <summary>
    /// Asynchronously converts a collection of test data into an array of rows using a custom conversion function.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of elements in the output array, produced by <paramref name="convertRow"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="convertRow">
    /// A function that transforms each test data item into a row of type <typeparamref name="TRow"/>.
    /// Cannot be null.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing an array of converted rows, preserving the order from the input collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method uses smart threshold optimization: collections with fewer than 100 items execute synchronously
    /// via <see cref="Task.FromResult{TResult}"/>, while larger collections offload to the thread pool via <see cref="Task.Run{TResult}(Func{TResult})"/>.
    /// </para>
    /// <para>
    /// Delegates to <see cref="CollectionConverter.ToArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, TRow})"/>.
    /// </para>
    /// </remarks>
    public static Task<TRow[]> ToArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedTask(
        tdc => tdc.ToArray(convertRow));

    /// <summary>
    /// Asynchronously converts a collection of test data into an array, preserving the test data items as-is (identity conversion).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing an array of <typeparamref name="TTestData"/> items.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is an identity conversion that returns test data items without transformation.
    /// Uses smart threshold optimization (see class remarks for details).
    /// </para>
    /// <para>
    /// Delegates to <see cref="CollectionConverter.ToArray{TTestData}(IEnumerable{TTestData})"/>.
    /// </para>
    /// </remarks>
    public static Task<TTestData[]> ToArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedTask(
        tdc => tdc.ToArray());

    #endregion

    #region ToDistinctArrayTask

    /// <summary>
    /// Asynchronously converts a collection of test data into a distinct array of rows using a custom conversion function.
    /// Removes duplicates based on <see cref="INamedCase.TestCaseName"/> identity.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of elements in the output array, produced by <paramref name="convertRow"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="convertRow">
    /// A function that transforms each test data item into a row of type <typeparamref name="TRow"/>.
    /// Cannot be null. Called only for non-duplicate items.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing an array of converted rows for distinct test data items,
    /// preserving the order of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Deduplication uses <see cref="NamedCase.Comparer"/> based on <see cref="INamedCase.TestCaseName"/>.
    /// Test data with identical <c>TestCaseName</c> values are considered duplicates; only the first occurrence is retained.
    /// </para>
    /// <para>
    /// Uses smart threshold optimization (see class remarks for details).
    /// Delegates to <see cref="CollectionConverter.ToDistinctArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, TRow})"/>.
    /// </para>
    /// </remarks>
    public static Task<TRow[]> ToDistinctArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedTask(
        tdc => tdc.ToDistinctArray(convertRow));

    /// <summary>
    /// Asynchronously converts a collection of test data into a distinct array, removing duplicates based on
    /// <see cref="INamedCase.TestCaseName"/> identity (identity conversion).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing an array of distinct <typeparamref name="TTestData"/> items,
    /// preserving the order of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is an identity conversion that deduplicates test data items without transformation.
    /// Deduplication uses <see cref="NamedCase.Comparer"/> based on <see cref="INamedCase.TestCaseName"/>.
    /// </para>
    /// <para>
    /// Uses smart threshold optimization (see class remarks for details).
    /// Delegates to <see cref="CollectionConverter.ToDistinctArray{TTestData}(IEnumerable{TTestData})"/>.
    /// </para>
    /// </remarks>
    public static Task<TTestData[]> ToDistinctArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedTask(
        tdc => tdc.ToDistinctArray());

    #endregion

    #region ToTask

    /// <summary>
    /// Core helper method that applies smart threshold-based optimization to convert a collection asynchronously,
    /// choosing between synchronous and thread-pool execution based on collection size.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TResult">
    /// The type of the conversion result.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="convert">
    /// A function that transforms the collection snapshot into the desired result type.
    /// Cannot be null.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing the conversion result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convert"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Smart Threshold Strategy:</strong> This method snapshots the collection and evaluates its size:
    /// <list type="bullet">
    ///   <item><strong>&lt; 100 items:</strong> Executes synchronously via <see cref="Task.FromResult{TResult}"/> (avoids Task.Run overhead)</item>
    ///   <item><strong>≥ 100 items:</strong> Offloads to thread pool via <see cref="Task.Run{TResult}(Func{TResult})"/> (parallel execution benefit)</item>
    /// </list>
    /// </para>
    /// <para>
    /// The threshold of 100 items is empirically derived from BenchmarkDotNet measurements showing this as the
    /// break-even point where Task.Run benefits outweigh its overhead.
    /// </para>
    /// <para>
    /// Uses <see cref="SnapshotWithCount{TTestData}(IEnumerable{TTestData})"/> to validate and snapshot the collection
    /// before applying the conversion function.
    /// </para>
    /// </remarks>
    public static Task<TResult> ToConvertedTask<TTestData, TResult>(
        this IEnumerable<TTestData> testDataCollection,
        Func<IEnumerable<TTestData>, TResult> convert)
    where TTestData : notnull, ITestData
    {
        const int smallCollectionCountLimit = 100;

        var (snapshot, count) = SnapshotWithCount(testDataCollection);

        return count < smallCollectionCountLimit ?
            Task.FromResult(result: convert(snapshot))
            : Task.Run(function: () => convert(snapshot));
    }

    #endregion
}