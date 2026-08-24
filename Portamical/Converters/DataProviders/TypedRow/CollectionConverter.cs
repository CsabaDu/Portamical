// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.TypedRow;

namespace Portamical.Converters.DataProviders.TypedRow;

/// <summary>
/// Provides extension methods for converting test data collections into <see cref="ITestDataAdder{TTestData}"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// The methods in this class help ensure that test data collections are deduplicated based on test case
/// identity (via <see cref="INamedCase.TestCaseName"/>) and are returned in immutable forms.
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong> Uses <see cref="NamedCase.Comparer"/> for semantic equality
/// based on test case names, not reference equality. This ensures that test data with identical
/// <c>TestCaseName</c> values are treated as duplicates, with the first occurrence retained.
/// </para>
/// </remarks>
public static class CollectionConverter
{
    #region ToDataProvider

    public static TDataProvider ToDataProvider<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TDataProvider> initDataProvider,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    where TDataProvider : notnull, ITestDataProvider<TTestData, TRow>
    {
        var (snapshot, testData, count, dataProvider) = InitializeDataProviderWithSnapshot<TDataProvider, TTestData, TRow>(
            testDataCollection,
            initDataProvider,
            argsCode,
            testMethodName);

        if (count > 1)
        {
            for (int i = 1; i < count; i++)
            {
                testData = snapshot[i];
                dataProvider.AddRow(testData);
            }
        }

        return dataProvider;
    }

    #endregion

    #region ToDistinctDataProvider

    public static TDataProvider ToDistinctDataProvider<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TDataProvider> initDataProvider,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    where TDataProvider : notnull, ITestDataProvider<TTestData, TRow>
    {
        var (snapshot, testData, count, dataProvider) = InitializeDataProviderWithSnapshot<TDataProvider, TTestData, TRow>(
            testDataCollection,
            initDataProvider,
            argsCode,
            testMethodName);
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
        _ = namedCases.Add(testData);

        if (count > 1)
        {
            for (int i = 1; i < count; i++)
            {
                testData = snapshot[i];
                testData.ExecuteIfDistinct(namedCases,
                    action: () => dataProvider.AddRow(testData));
            }
        }

        return dataProvider;
    }

    #endregion

    #region Helper methods

    private static (TTestData[] Snapshot, TTestData TestData, int Count, TDataProvider DataProvider) InitializeDataProviderWithSnapshot<TDataProvider, TTestData, TRow>(
        IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TDataProvider> initDataProvider,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    where TDataProvider : notnull, ITestDataProvider<TTestData, TRow>
    {
        var snapshot =
            NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        var testData = snapshot[0];
        var count = snapshot.Length;
        var dataProvider = NotNull(initDataProvider, nameof(initDataProvider))(
            testData, argsCode, testMethodName);

        return (snapshot, testData, count, dataProvider);
    }

    #endregion
}