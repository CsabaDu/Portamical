// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters;

/// <summary>
/// Provides internal utility methods for collection conversion operations, including iteration,
/// snapshotting, and deduplication logic used by converter classes.
/// </summary>
/// <remarks>
/// <para>
/// This class contains low-level helper methods that are shared across multiple converter implementations
/// in the <see cref="Portamical.Converters"/> namespace. All methods are marked <c>internal</c> and are
/// not part of the public API.
/// </para>
/// <para>
/// <strong>Key CollectionConverter:</strong>
/// </para>
/// <list type="bullet">
///   <item><see cref="AddConvertedRows{TTestData}"/> - Efficient iteration with optional deduplication and skip-first capability</item>
///   <item><see cref="SnapshotWithCount{TTestData}"/> - Collection validation and snapshotting with count</item>
/// </list>
/// </remarks>
internal static class CollectionConverter
{
    #region ToConvertedRows methods

    public static TConvertedRows ToConvertedRows<TTestData, TConvertedRows>(
    this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TConvertedRows> initConvertedRows,
        Action<TConvertedRows, TTestData> addConvertedRow)
    where TTestData : notnull, ITestData
    where TConvertedRows : notnull
    => testDataCollection.ToConvertedRows(
        initConvertedRows,
        addConvertedRow,
        removeDuplicates: false);

    public static TConvertedRows ToConvertedRows<TTestData, TConvertedRows>(
    this IEnumerable<TTestData> testDataCollection,
        Action<TConvertedRows, TTestData> addConvertedRow)
    where TTestData : notnull, ITestData
    where TConvertedRows : new()
    => testDataCollection.ToConvertedRows(
        addConvertedRow,
        removeDuplicates: false);

    #endregion

    #region ToDistinctConvertedRows methods

    public static TConvertedRows ToDistinctConvertedRows<TTestData, TConvertedRows>(
    this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TConvertedRows> initConvertedRows,
        Action<TConvertedRows, TTestData> addConvertedRow)
    where TTestData : notnull, ITestData
    where TConvertedRows : notnull
    => testDataCollection.ToConvertedRows(
        initConvertedRows,
        addConvertedRow,
        removeDuplicates: true);

    public static TConvertedRows ToDistinctConvertedRows<TTestData, TConvertedRows>(
    this IEnumerable<TTestData> testDataCollection,
        Action<TConvertedRows, TTestData> addConvertedRow)
    where TTestData : notnull, ITestData
    where TConvertedRows : new()
    => testDataCollection.ToConvertedRows(
        addConvertedRow,
        removeDuplicates: true);

    #endregion

    #region Helper methods

    private static TConvertedRows ToConvertedRows<TTestData, TConvertedRows>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TConvertedRows> initConvertedRows,
        Action<TConvertedRows, TTestData> addConvertedRow,
        bool removeDuplicates)
    where TTestData : notnull, ITestData
    where TConvertedRows : notnull
    {
        var snapshot = NotNullOrEmpty(
            testDataCollection,
            nameof(testDataCollection),
            out var count);
        var convertedRows = initConvertedRows(snapshot[0]);

        if (count == 1)
        {
            return convertedRows;
        }

        return snapshot.ToConvertedRows(
            convertedRows,
            addConvertedRow,
            removeDuplicates,
            skipFirst: true);
    }

    private static TConvertedRows ToConvertedRows<TTestData, TConvertedRows>(
        this IEnumerable<TTestData> testDataCollection,
        Action<TConvertedRows, TTestData> addConvertedRow,
        bool removeDuplicates)
    where TTestData : notnull, ITestData
    where TConvertedRows : new()
    {
        var snapshot = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        var convertedRows = new TConvertedRows();

        return snapshot.ToConvertedRows(
            convertedRows,
            addConvertedRow,
            removeDuplicates,
            skipFirst: false);
    }

    private static TConvertedRows ToConvertedRows<TTestData, TConvertedRows>(
        this TTestData[] snapshot,
        TConvertedRows convertedRows,
        Action<TConvertedRows, TTestData> addConvertedRow,
        bool removeDuplicates,
        bool skipFirst)
    where TTestData : notnull, ITestData
    {
        if (removeDuplicates)
        {
            var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);

            if (skipFirst)
            {
                _ = namedCases.Add(snapshot[0]);
            }

            addRange(testData =>
            {
                if (namedCases.Add(testData))
                {
                    addConvertedRow(convertedRows, testData);
                }
            });
        }
        else
        {
            addRange((testData) => addConvertedRow(convertedRows, testData));
        }

        #region Local function

        void addRange(Action<TTestData> addConverted)
        {
            var startIndex = skipFirst ? 1 : 0;

            for (int i = startIndex; i < snapshot.Length; i++)
            {
                var testData = snapshot[i];
                addConverted(testData);
            }
        }

        return convertedRows;

        #endregion
    }

    #endregion
}
