// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using static Portamical.Converters.Utilities;

namespace Portamical.Converters.RowArrays;

/// <summary>
/// Provides extension methods for converting test data collections into row arrays,
/// with optional deduplication based on test case identity, optimized for test framework integration.
/// </summary>
/// <remarks>
/// <para>
/// This class contains the primary row array conversion methods: <see cref="ToRowArray{TTestData, TRow}"/>
/// for standard conversion and <see cref="ToDistinctRowArray{TTestData, TRow}"/> for deduplicated conversion.
/// Both methods use custom conversion functions to transform test data into framework-compatible row types.
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong> The <c>ToDistinctRowArray</c> method uses <see cref="NamedCase.Comparer"/>
/// for semantic equality based on test case names (via <see cref="INamedCase.TestCaseName"/>), not reference equality.
/// Test data with identical <c>TestCaseName</c> values are treated as duplicates, with the first occurrence retained.
/// </para>
/// <para>
/// <strong>Return Type:</strong> All methods return arrays for optimal performance with test frameworks.
/// Arrays provide zero-allocation enumeration, direct indexing, and better compatibility with
/// data-driven test attributes (MSTest <c>DynamicData</c>, xUnit <c>MemberData</c>, NUnit <c>TestCaseSource</c>).
/// </para>
/// <para>
/// <strong>Related Classes:</strong> For asynchronous conversion helpers and collection snapshotting utilities,
/// see <see cref="Tasks.CollectionConverter"/>.
/// </para>
/// </remarks>
public static class CollectionConverter
{
    #region ToRowArray

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
    /// An array containing the rows rows, preserving the order from the input collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is the core conversion method. Other <c>ToRowArray</c> overloads typically delegate to this method.
    /// The collection is validated and snapshotted before conversion to ensure stability.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Pre-allocates the result array based on input count for O(n) performance.
    /// </para>
    /// </remarks>
    public static TRow[] ToRowArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        var (snapshot, count) =
            SnapshotWithCount(testDataCollection);
        _ = NotNull(convertRow, nameof(convertRow));
        var rows = new TRow[count];

        for (int i = 0; i < count; i++)
        {
            var testData = snapshot[i];
            rows[i] = convertRow(testData);
        }
        
        return rows;
    }

    #endregion

    #region ToDistinctRowArray

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
    /// An array containing the rows rows for distinct test data items, preserving the order
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
    /// var distinct = testDataCollection.ToDistinctRowArray(td => td);
    /// 
    /// // Convert to argument arrays
    /// var args = testDataCollection.ToDistinctRowArray(td => td.ToArgs(ArgsCode.Instance));
    /// 
    /// // Custom row conversion
    /// var rows = testDataCollection.ToDistinctRowArray(td => new 
    /// { 
    ///     Name = td.TestCaseName, 
    ///     Args = td.ToArgs(ArgsCode.Instance) 
    /// });
    /// </code>
    /// </example>
    public static TRow[] ToDistinctRowArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        var snapshot = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        _ = NotNull(convertRow, nameof(convertRow));
        var rows = new List<TRow>(snapshot.Length);

        AddConvertedRows(
            snapshot: snapshot,
            addConvertedRow: (testData) => rows.Add(convertRow(testData)),
            beDistinct: true,
            skipFirst: false);

        return [.. rows];
    }

    #endregion
}
