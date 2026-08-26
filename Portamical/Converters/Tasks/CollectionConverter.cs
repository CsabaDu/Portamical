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
    => testDataCollection.ToConvertedTask(
        tdc => tdc.ToArray(convertRow));

    public static Task<TTestData[]> ToArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedTask(
        tdc => tdc.ToArray());

    #endregion

    #region ToDistinctArrayTask

    public static Task<TRow[]> ToDistinctArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedTask(
        tdc => tdc.ToDistinctArray(convertRow));

    public static Task<TTestData[]> ToDistinctArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedTask(
        tdc => tdc.ToDistinctArray());

    #endregion

    #region ToTask

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