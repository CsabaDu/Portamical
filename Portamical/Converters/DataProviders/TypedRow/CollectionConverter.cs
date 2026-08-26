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
    => testDataCollection.ToDataProvider<TDataProvider, TTestData, TRow>(
        td => initDataProvider(td, argsCode, testMethodName));

    #endregion

    #region ToDistinctDataProvider

    public static TDataProvider ToDistinctDataProvider<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TDataProvider> initDataProvider,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    where TDataProvider : notnull, ITestDataProvider<TTestData, TRow>
    => testDataCollection.ToDistinctDataProvider<TDataProvider, TTestData, TRow>(
        td => initDataProvider(td, argsCode, testMethodName));

    #endregion
}