// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

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
        initDataProvider);

    public static TDataProvider ToDataProvider<TTestData, TDataProvider>(
        this IEnumerable<TTestData> testDataCollection)
    where TDataProvider : notnull, ITestDataProvider<TTestData>, new()
    where TTestData : notnull, ITestData
    => testDataCollection.ToDataProvider<TDataProvider, TTestData, TTestData>();

    #endregion

    #region ToDistinctDataProvider

    public static TDataProvider ToDistinctDataProvider<TTestData, TDataProvider>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TDataProvider> initDataProvider)
    where TDataProvider : notnull, ITestDataProvider<TTestData>
    where TTestData : notnull, ITestData
    => testDataCollection .ToDistinctDataProvider<TDataProvider, TTestData, TTestData>(
        initDataProvider);

    public static TDataProvider ToDistinctDataProvider<TTestData, TDataProvider>(
        this IEnumerable<TTestData> testDataCollection)
    where TDataProvider : notnull, ITestDataProvider<TTestData>, new()
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctDataProvider<TDataProvider, TTestData, TTestData>();

    #endregion
}