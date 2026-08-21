// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Core.Identity;
using Portamical.Core.Identity.Model;
using Portamical.DataProviders;
using Portamical.Shared.DataProviders;
using Portamical.Shared.DataProviders.Model;

namespace Portamical.Shared.Converters;

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
    where TDataProvider : ITestDataProvider<TTestData, TRow>
    {
        var (snapshot, testData, count) = SnapshotWithFirstAndCount(
            testDataCollection,
            initDataProvider,
            argsCode,
            testMethodName,
            out var dataProvider);

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

    public static TestDataProvider<TTestData> ToDataProvider<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDataProvider<TestDataProvider<TTestData>, TTestData, object?[]>(
        initDataProvider: (testData, argsCode, _) => new DataProviders.Model.TestDataProvider<TTestData>(testData, argsCode),
        argsCode: argsCode,
        testMethodName: null);

    #endregion

    #region ToDistinctDataProvider

    public static TDataProvider ToDistinctDataProvider<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TDataProvider> initDataProvider,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    where TDataProvider : ITestDataProvider<TTestData, TRow>
    {
        var (snapshot, testData, count) = SnapshotWithFirstAndCount(
            testDataCollection,
            initDataProvider,
            argsCode,
            testMethodName,
            out var dataProvider);

        if (count > 1)
        {
            var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
            _ = namedCases.Add(testData);

            for (int i = 1; i < count; i++)
            {
                testData = snapshot[i];

                if (namedCases.Add(testData))
                {
                    dataProvider.AddRow(testData);
                }
            }
        }

        return dataProvider;
    }

    public static TestDataProvider<TTestData> ToDistinctDataProvider<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctDataProvider<TestDataProvider<TTestData>, TTestData, object?[]>(
        initDataProvider: (testData, argsCode, _) => new TestDataProvider<TTestData>(testData, argsCode),
        argsCode: argsCode,
        testMethodName: null);

    #endregion

    #region Helper methods

    private static (TTestData[] Snapshot, TTestData TestData, int Count) SnapshotWithFirstAndCount<TTestData, TDataProvider>(
        IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TDataProvider> initDataProvider,
        ArgsCode argsCode,
        string? testMethodName,
        out TDataProvider dataProvider)
    where TTestData : notnull, ITestData
    where TDataProvider : ITestDataRegistry<TTestData>
    {
        var snapshotWithFirstAndCount = Portamical.Converters.DataProviders.CollectionConverter.SnapshotWithFirstAndCount(
            testDataCollection);
        dataProvider = NotNull(initDataProvider, nameof(initDataProvider))(
            snapshotWithFirstAndCount.TestData, argsCode, testMethodName);

        return snapshotWithFirstAndCount;
    }

    #endregion
}