// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters.Tasks;

/// <summary>
/// Provides core helper methods for converting test data collections into arbitrary result types asynchronously,
/// with threshold-based optimization and optional deduplication based on test case identity.
/// </summary>
/// <remarks>
/// <para>
/// This class contains foundational helpers used by specialized converter classes in the
/// <see cref="Converters"/> namespace hierarchy. For row array conversion methods,
/// see <see cref="RowArrays.CollectionConverter"/>.
/// </para>
/// <para>
/// <strong>Async Conversion:</strong> Both <see cref="ToConvertedRowsTask{TTestData, TConvertedRows}"/> and
/// <see cref="ToDistinctConvertedRowsTask{TTestData, TConvertedRows}"/> apply smart threshold-based
/// optimization, choosing between synchronous and asynchronous execution based on collection size
/// (threshold: 100 items).
/// </para>
/// <para>
/// <strong>Deduplication:</strong> <see cref="ToDistinctConvertedRowsTask{TTestData, TConvertedRows}"/>
/// removes duplicate test data based on <see cref="INamedCase.TestCaseName"/> using
/// <see cref="RowArrays.TestData.CollectionConverter.ToDistinctRowArray{TTestData}(IEnumerable{TTestData})"/>
/// before the snapshot is passed to the conversion function. <see cref="ToConvertedRowsTask{TTestData, TConvertedRows}"/>
/// does not deduplicate.
/// </para>
/// </remarks>
public static class CollectionConverter
{
    #region ToConvertedRowsTask

    /// <summary>
    /// Applies smart threshold-based optimization to convert a collection asynchronously into an arbitrary
    /// result type, choosing between synchronous and thread-pool execution based on collection size, without
    /// deduplication.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TConvertedRows">
    /// The type of the conversion result.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="convertRows">
    /// A function that transforms the collection snapshot into the desired result type.
    /// Cannot be null.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing the conversion result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRows"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Smart Threshold Strategy:</strong> This method snapshots the collection and evaluates its size:
    /// <list type="bullet">
    ///   <item><strong>&lt; 100 items:</strong> Executes synchronously via <see cref="Task.FromResult{TResult}"/> (avoids Task.Run overhead)</item>
    ///   <item><strong>&#8805; 100 items:</strong> Offloads to thread pool via <see cref="Task.Run{TResult}(Func{TResult})"/> (parallel execution benefit)</item>
    /// </list>
    /// </para>
    /// <para>
    /// The threshold of 100 items is empirically derived from BenchmarkDotNet measurements showing this as the
    /// break-even point where Task.Run benefits outweigh its overhead.
    /// </para>
    /// <para>
    /// Delegates to the private helper with <c>removeDuplicates: false</c>, which validates and snapshots the
    /// collection via <see cref="NotNullOrEmpty{T}(IEnumerable{T}, string, out int)"/> before applying
    /// <paramref name="convertRows"/>.
    /// </para>
    /// </remarks>
    public static Task<TConvertedRows> ToConvertedRowsTask<TTestData, TConvertedRows>(
        this IEnumerable<TTestData> testDataCollection,
        Func<IEnumerable<TTestData>, TConvertedRows> convertRows)
    where TTestData : notnull, ITestData
    where TConvertedRows : notnull
    => testDataCollection.ToDConvertedRowsTask(convertRows,
        removeDuplicates: false);

    #endregion

    #region ToDistinctConvertedRowsTask

    /// <summary>
    /// Applies smart threshold-based optimization to convert a collection asynchronously into an arbitrary
    /// result type, choosing between synchronous and thread-pool execution based on collection size, after
    /// removing duplicate test data based on test case identity.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TConvertedRows">
    /// The type of the conversion result.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="convertRows">
    /// A function that transforms the deduplicated collection snapshot into the desired result type.
    /// Cannot be null.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing the conversion result, computed from the distinct test data items.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRows"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Follows the same threshold strategy as <see cref="ToConvertedRowsTask{TTestData, TConvertedRows}"/>
    /// (synchronous below 100 items, <see cref="Task.Run{TResult}(Func{TResult})"/> at or above), but the
    /// item count used for the threshold check is measured after deduplication.
    /// </para>
    /// <para>
    /// Delegates to the private helper with <c>removeDuplicates: true</c>, which deduplicates the collection
    /// via <see cref="RowArrays.TestData.CollectionConverter.ToDistinctRowArray{TTestData}(IEnumerable{TTestData})"/>
    /// (using <see cref="NamedCase.Comparer"/> on <see cref="INamedCase.TestCaseName"/>) before applying
    /// <paramref name="convertRows"/>.
    /// </para>
    /// </remarks>
    public static Task<TConvertedRows> ToDistinctConvertedRowsTask<TTestData, TConvertedRows>(
        this IEnumerable<TTestData> testDataCollection,
        Func<IEnumerable<TTestData>, TConvertedRows> convertRows)
    where TTestData : notnull, ITestData
    where TConvertedRows : notnull
    => testDataCollection.ToDConvertedRowsTask(convertRows,
        removeDuplicates: true);

    #endregion

    #region Private Base ToDConvertedRowsTask

    /// <summary>
    /// Shared implementation that validates, optionally deduplicates, and snapshots
    /// <paramref name="testDataCollection"/>, then converts it asynchronously using a smart
    /// threshold-based strategy.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TConvertedRows">
    /// The type of the conversion result.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="convertRows">
    /// A function that transforms the collection snapshot into the desired result type. Cannot be null.
    /// </param>
    /// <param name="removeDuplicates">
    /// If <see langword="true"/>, snapshots the collection via
    /// <see cref="RowArrays.TestData.CollectionConverter.ToDistinctRowArray{TTestData}(IEnumerable{TTestData})"/>,
    /// removing items whose <see cref="INamedCase.TestCaseName"/> duplicates an earlier item. If
    /// <see langword="false"/>, snapshots via <see cref="NotNullOrEmpty{T}(IEnumerable{T}, string, out int)"/>
    /// without deduplication.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing the conversion result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRows"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Smart Threshold Strategy:</strong> The item count of the (optionally deduplicated) snapshot
    /// determines execution mode:
    /// <list type="bullet">
    ///   <item><strong>&lt; 100 items:</strong> Executes synchronously via <see cref="Task.FromResult{TResult}"/> (avoids Task.Run overhead)</item>
    ///   <item><strong>&#8805; 100 items:</strong> Offloads to thread pool via <see cref="Task.Run{TResult}(Func{TResult})"/> (parallel execution benefit)</item>
    /// </list>
    /// The threshold of 100 items is empirically derived from BenchmarkDotNet measurements showing this as the
    /// break-even point where Task.Run benefits outweigh its overhead.
    /// </para>
    /// </remarks>
    private static Task<TConvertedRows> ToDConvertedRowsTask<TTestData, TConvertedRows>(
        this IEnumerable<TTestData> testDataCollection,
        Func<IEnumerable<TTestData>, TConvertedRows> convertRows,
        bool removeDuplicates)
    where TTestData : notnull, ITestData
    where TConvertedRows : notnull
    {
        const int smallSnapshotCountLimit = 100;

        var snapshot = toSnapshot(out var count);
        _ = NotNull(convertRows, nameof(convertRows));

        return count < smallSnapshotCountLimit ?
            Task.FromResult(result: convertSnapshot())
            : Task.Run(function: convertSnapshot);

        #region Local functions

        TTestData[] toSnapshot(out int count)
        {
            if (!removeDuplicates)
            {
                return NotNullOrEmpty(
                    testDataCollection,
                    nameof(testDataCollection),
                    out count);
            }

            var distinctSnapshot =
                RowArrays.TestData.CollectionConverter.ToDistinctRowArray(
                    testDataCollection);
            count = distinctSnapshot.Length;

            return distinctSnapshot;
        }

        TConvertedRows convertSnapshot()
        => convertRows(snapshot);

        #endregion
    }

    #endregion
}