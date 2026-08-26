// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.DataProviders;
using Portamical.DataProviders.TestData;

namespace Portamical.Converters.DataProviders.TestData;

public static class CollectionConverter
{
    #region ToDataProvider

    public static TDataProvider ToDataProvider<TTestData, TDataProvider>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TDataProvider> initDataProvider)
    where TDataProvider : notnull, ITestDataProvider<TTestData>
    where TTestData : notnull, ITestData
    => testDataCollection.ToDataProvider<TDataProvider, TTestData, TTestData>(
        initDataProvider,
        DataProviderWithSnapshotAndCount);

    #endregion

    #region ToDistinctDataProvider

    public static TDataProvider ToDistinctDataProvider<TTestData, TDataProvider>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TDataProvider> initDataProvider)
    where TDataProvider : notnull, ITestDataProvider<TTestData>
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctDataProvider<TDataProvider, TTestData, TTestData>(
        initDataProvider,
        DataProviderWithSnapshotAndCount);

    #endregion

    #region Helper methods

    private static (TDataProvider DataProvider, TTestData[] Snapshot, int Count) DataProviderWithSnapshotAndCount<TDataProvider, TTestData>(
        IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TDataProvider> initDataProvider)
    where TDataProvider : notnull, IDataProvider<TTestData, TTestData>
    where TTestData : notnull, ITestData
    {
        var (snapshot, count) = testDataCollection.SnapshotAndCount();
        var dataProvider =
            NotNull(initDataProvider, nameof(initDataProvider))(
                snapshot[0]);

        return (dataProvider, snapshot, count);
    }

    #endregion
}