// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters;

/// <summary>
/// Provides core helper methods for converting test data collections into various row formats,
/// with support for both direct and deduplicated conversion strategies.
/// </summary>
/// <remarks>
/// <para>
/// This class contains foundational conversion infrastructure used throughout the
/// <see cref="Portamical.Converters"/> namespace hierarchy. It provides:
/// </para>
/// <list type="bullet">
///   <item><see cref="ToConvertedRows{TConvertedRows, TTestData, TRow}"/> - Core generic conversion with optional deduplication</item>
///   <item><see cref="SnapshotWithCount{TTestData}"/> - Collection validation and snapshotting utility</item>
/// </list>
/// <para>
/// <strong>Deduplication Strategy:</strong> When deduplication is enabled, uses <see cref="NamedCase.Comparer"/>
/// for semantic equality based on test case names (via <see cref="INamedCase.TestCaseName"/>), not reference equality.
/// Test data with identical <c>TestCaseName</c> values are treated as duplicates, with the first occurrence retained.
/// </para>
/// <para>
/// <strong>Related Classes:</strong>
/// </para>
/// <list type="bullet">
///   <item><see cref="Portamical.Converters.RowArrays.CollectionConverter"/> - Specialized row array conversion methods</item>
///   <item><see cref="Portamical.Converters.DataProviders.CollectionConverter"/> - Data provider creation methods</item>
/// </list>
/// </remarks>
public static class CollectionConverter
{
    #region ToConvertedRows

    /// <summary>
    /// Core generic conversion method that transforms a test data collection into a custom result type,
    /// with optional deduplication based on test case identity.
    /// </summary>
    /// <typeparam name="TConvertedRows">
    /// The type of the result collection. Must be non-null (e.g., <c>List&lt;TRow&gt;</c>, custom collection types).
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of rows produced by the <paramref name="convertRow"/> function and added to the result collection.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="initConvertedRows">
    /// A factory function that creates and initializes the result collection. Cannot be null.
    /// Called once at the start of conversion.
    /// </param>
    /// <param name="convertRow">
    /// A function that transforms each test data item into a row of type <typeparamref name="TRow"/>.
    /// Cannot be null. Called once for each item (or once for each distinct item if deduplication is enabled).
    /// </param>
    /// <param name="add">
    /// An action that adds a converted row to the result collection. Cannot be null.
    /// Typically a delegate to the result collection's <c>Add</c> method.
    /// </param>
    /// <param name="isDistinct">
    /// If <see langword="true"/>, removes duplicate test data based on <see cref="INamedCase.TestCaseName"/>
    /// using <see cref="NamedCase.Comparer"/>; if <see langword="false"/>, converts all items without deduplication.
    /// </param>
    /// <returns>
    /// The populated result collection of type <typeparamref name="TConvertedRows"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/>, <paramref name="initConvertedRows"/>,
    /// <paramref name="convertRow"/>, or <paramref name="add"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is the most flexible conversion method, allowing complete control over the result type and
    /// how rows are added. It's typically used as a building block for more specialized conversion methods.
    /// </para>
    /// <para>
    /// <strong>Deduplication:</strong> When <paramref name="isDistinct"/> is <see langword="true"/>,
    /// uses <see cref="HashSet{T}"/> with <see cref="NamedCase.Comparer"/> for O(n) deduplication.
    /// Only the first occurrence of each unique test case name is processed.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> The collection is validated and snapshotted before conversion to ensure
    /// stability. Uses efficient iteration patterns with minimal allocation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert to List&lt;object?[]&gt; with deduplication
    /// var result = testDataCollection.ToConvertedRows(
    ///     initConvertedRows: () => new List&lt;object?[]&gt;(),
    ///     convertRow: td => td.ToArgs(ArgsCode.Properties, PropsCode.All),
    ///     add: row => list.Add(row),
    ///     isDistinct: true);
    /// 
    /// // Convert to custom collection without deduplication
    /// var custom = testDataCollection.ToConvertedRows(
    ///     initConvertedRows: () => new CustomCollection(),
    ///     convertRow: td => new CustomRow(td),
    ///     add: row => collection.AddRow(row),
    ///     isDistinct: false);
    /// </code>
    /// </example>
    public static TConvertedRows ToConvertedRows<TConvertedRows, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TConvertedRows> initConvertedRows,
        Func<TTestData, TRow> convertRow,
        Action<TRow> add,
        bool isDistinct)
    where TConvertedRows : notnull
    where TTestData : notnull, ITestData
    {
        var snapshot = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        var convertedRows = NotNull(initConvertedRows, nameof(initConvertedRows))();
        _ = NotNull(convertRow, nameof(convertRow));
        _ = NotNull(add, nameof(add));

        if (isDistinct)
        {
            var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);

            addRows(td => td.ExecuteIfDistinct(namedCases,
                action: () => addRows(
                    addConvertedRow: testData => add(convertRow(testData)))));
        }
        else
        {
            addRows(addConvertedRow: testData => add(convertRow(testData)));
        }

        return convertedRows;

        #region Local methods

        void addRows(Action<TTestData> addConvertedRow)
        {
            foreach (var testData in snapshot)
            {
                addConvertedRow(testData);
            }
        }

        #endregion
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
