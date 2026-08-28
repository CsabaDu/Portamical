// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters;

/// <summary>
/// Provides extension methods for converting and deduplicating test data collections into arrays,
/// optimized for test framework integration.
/// </summary>
/// <remarks>
/// <para>
/// The methods in this class help ensure that test data collections are deduplicated based on test case
/// identity (via <see cref="INamedCase.TestCaseName"/>) and are returned as arrays.
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong> Uses <see cref="NamedCase.Comparer"/> for semantic equality
/// based on test case names, not reference equality. This ensures that test data with identical
/// <c>TestCaseName</c> values are treated as duplicates, with the first occurrence retained.
/// </para>
/// <para>
/// <strong>Return Type:</strong> All methods return arrays for optimal performance with test frameworks.
/// Arrays provide zero-allocation enumeration, direct indexing, and better compatibility with
/// data-driven test attributes (MSTest <c>DynamicData</c>, xUnit <c>MemberData</c>, NUnit <c>TestCaseSource</c>).
/// </para>
/// </remarks>
public static class CollectionConverter
{
    #region ToArrayRows

    /// <summary>
    /// Converts a collection of test data into an array of rows using a custom conversion function.
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
    /// Cannot be null. Called once for each item in the collection.
    /// </param>
    /// <returns>
    /// An array containing the converted rows, preserving the order from the input collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is the core conversion method. Other <c>ToArrayRows</c> overloads typically delegate to this method.
    /// The collection is validated and snapshotted before conversion to ensure stability.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Pre-allocates the result array based on input count for O(n) performance.
    /// </para>
    /// </remarks>
    public static TRow[] ToArrayRows<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        var snapshot = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        _ = NotNull(convertRow, nameof(convertRow));
        var count = snapshot.Length;
        var converted = new TRow[count];

        for (int i = 0; i < count; i++)
        {
            var testData = snapshot[i];
            converted[i] = convertRow(testData);
        }
        
        return converted;
    }

    #endregion

    #region ToDistinctArrayRows

    /// <summary>
    /// Core deduplication method that converts a collection of test data into a distinct array of rows
    /// using a custom conversion function.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Deduplication Strategy:</strong> This method removes duplicates based on 
    /// <see cref="INamedCase.TestCaseName"/> identity using <see cref="NamedCase.Comparer"/>.
    /// Test data items with identical <c>TestCaseName</c> values are considered duplicates, and only
    /// the first occurrence is retained.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Uses <see cref="HashSet{T}"/> with <see cref="NamedCase.Comparer"/>
    /// for O(n) deduplication. The <see cref="HashSet{T}.Add"/> method returns false for duplicates,
    /// which is used as a filter predicate.
    /// </para>
    /// <para>
    /// <strong>Order Preservation:</strong> The order of elements from the original collection is preserved
    /// in the output array. Duplicates are removed based on first-occurrence semantics.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> 
    /// (which inherits <see cref="INamedCase"/>) and cannot be null.
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
    /// An array containing the converted rows for distinct test data items, preserving the order
    /// of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <example>
    /// <code>
    /// // Identity conversion (keeping test data as-is)
    /// var distinct = testDataCollection.ToDistinctArrayRows(td => td);
    /// 
    /// // Convert to argument arrays
    /// var args = testDataCollection.ToDistinctArrayRows(td => td.ToArgs(ArgsCode.Instance));
    /// 
    /// // Custom row conversion
    /// var rows = testDataCollection.ToDistinctArrayRows(td => new 
    /// { 
    ///     Name = td.TestCaseName, 
    ///     Args = td.ToArgs(ArgsCode.Instance) 
    /// });
    /// </code>
    /// </example>
    public static TRow[] ToDistinctArrayRows<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        var snapshot = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        _ = NotNull(convertRow, nameof(convertRow));
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
        var rows = new List<TRow>(snapshot.Length);

        foreach (var testData in snapshot)
        {
            testData.ExecuteIfDistinct(namedCases,
                action: () => rows.Add(convertRow(testData)));
        }

        return [.. rows];
    }

    #endregion


    #region ToTask

    /// <summary>
    /// Core helper method that applies smart threshold-based optimization to convert a collection asynchronously,
    /// choosing between synchronous and thread-pool execution based on collection size.
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
    ///   <item><strong>&lt; 100 items:</strong> Executes synchronously via <see cref="System.Threading.Tasks.Task.FromResult{TResult}"/> (avoids Task.Run overhead)</item>
    ///   <item><strong>≥ 100 items:</strong> Offloads to thread pool via <see cref="System.Threading.Tasks.Task.Run{TResult}(Func{TResult})"/> (parallel execution benefit)</item>
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
    public static Task<TConvertedRows> ToConvertedRowsTask<TTestData, TConvertedRows>(
        this IEnumerable<TTestData> testDataCollection,
        Func<IEnumerable<TTestData>, TConvertedRows> convert)
    where TTestData : notnull, ITestData
    {
        const int smallCollectionCountLimit = 100;

        var (snapshot, count) = SnapshotWithCount(testDataCollection);

        return count < smallCollectionCountLimit ?
            Task.FromResult(result: convert(snapshot))
            : Task.Run(function: () => convert(snapshot));
    }

    #endregion

    #region Helper methods

    /// <summary>
    /// Internal helper that validates and snapshots a collection, returning both the snapshot array and its count.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection to snapshot. Cannot be null or empty.
    /// </param>
    /// <returns>
    /// A tuple containing the snapshot array and its length.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// This method is used internally to avoid recalculating array length in performance-critical paths.
    /// </remarks>
    internal static (TTestData[] snapshot, int count) SnapshotWithCount<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    {
        var snapshot = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        var count = snapshot.Length;

        return (snapshot, count);
    }

    #endregion
}
