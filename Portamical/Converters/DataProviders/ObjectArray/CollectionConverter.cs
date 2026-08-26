// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.ObjectArray;

namespace Portamical.Converters.DataProviders.ObjectArray;

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

    public static TDataProvider ToDataProvider<TDataProvider, TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, PropsCode, TDataProvider> initDataProvider,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    where TDataProvider : notnull, ITestDataProvider<TTestData>
    => testDataCollection.ToDataProvider<TDataProvider, TTestData, object?[]>(
        td => initDataProvider(td, argsCode, propsCode),
        (tdc, idp) => tdc.DataProviderWithSnapshotAndCount(
            (td, argsCode, propsCode) => idp(td),
            argsCode,
            propsCode));

    #endregion

    #region ToDistinctDataProvider

    public static TDataProvider ToDistinctDataProvider<TDataProvider, TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, PropsCode, TDataProvider> initDataProvider,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    where TDataProvider : notnull, ITestDataProvider<TTestData>
    => testDataCollection.ToDistinctDataProvider<TDataProvider, TTestData, object?[]>(
        td => initDataProvider(td, argsCode, propsCode),
        (tdc, idp) => tdc.DataProviderWithSnapshotAndCount(
            (td, argsCode, propsCode) => idp(td),
            argsCode,
            propsCode));

    #endregion

    #region Helper methods

    private static (TDataProvider DataProvider, TTestData[] Snapshot, int Count) DataProviderWithSnapshotAndCount<TDataProvider, TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, PropsCode, TDataProvider> initDataProvider,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    where TDataProvider : notnull, ITestDataProvider<TTestData>
    {
        var (snapshot, count) = testDataCollection.SnapshotAndCount();
        var dataProvider =
            NotNull(initDataProvider, nameof(initDataProvider))(
                snapshot[0], argsCode, propsCode);

        return (dataProvider, snapshot, count);
    }

    #endregion
}