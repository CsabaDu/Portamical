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

    /// <summary>
    /// Converts a collection of test data into a typed-row data provider instance with <see cref="ArgsCode"/> and test method name configuration.
    /// </summary>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="ITestDataProvider{TTestData, TRow}"/>.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The custom row type produced by the data provider.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least one item.
    /// </param>
    /// <param name="initDataProvider">
    /// A function that initializes a new data provider instance using a test data item, <see cref="ArgsCode"/>, and test method name. Cannot be null.
    /// </param>
    /// <param name="argsCode">
    /// The <see cref="ArgsCode"/> configuration for argument extraction. Cannot be undefined.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method, or <see langword="null"/> if not applicable.
    /// </param>
    /// <returns>
    /// A data provider instance containing test data items from the collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="initDataProvider"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty or <paramref name="argsCode"/> is undefined.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method wraps the <paramref name="initDataProvider"/> function to include <paramref name="argsCode"/> and
    /// <paramref name="testMethodName"/> parameters, then delegates to
    /// <see cref="DataProviders.CollectionConverter.ToDataProvider{TDataProvider, TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, TDataProvider})"/>.
    /// </para>
    /// <para>
    /// This overload does NOT perform deduplication. For deduplication, use <see cref="ToDistinctDataProvider{TDataProvider, TTestData, TRow}"/>.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    /// <summary>
    /// Converts a collection of test data into a distinct typed-row data provider instance with <see cref="ArgsCode"/> and test method name configuration.
    /// Removes duplicates based on <see cref="INamedCase.TestCaseName"/> identity.
    /// </summary>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="ITestDataProvider{TTestData, TRow}"/>.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The custom row type produced by the data provider.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least one item.
    /// </param>
    /// <param name="initDataProvider">
    /// A function that initializes a new data provider instance using a test data item, <see cref="ArgsCode"/>, and test method name. Cannot be null.
    /// </param>
    /// <param name="argsCode">
    /// The <see cref="ArgsCode"/> configuration for argument extraction. Cannot be undefined.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method, or <see langword="null"/> if not applicable.
    /// </param>
    /// <returns>
    /// A data provider instance containing distinct test data items from the collection, with duplicates removed based on <see cref="INamedCase.TestCaseName"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="initDataProvider"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty or <paramref name="argsCode"/> is undefined.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method wraps the <paramref name="initDataProvider"/> function to include <paramref name="argsCode"/> and
    /// <paramref name="testMethodName"/> parameters, then delegates to
    /// <see cref="DataProviders.CollectionConverter.ToDistinctDataProvider{TDataProvider, TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, TDataProvider})"/>.
    /// </para>
    /// <para>
    /// Deduplication uses <see cref="NamedCase.Comparer"/> based on <see cref="INamedCase.TestCaseName"/>.
    /// Test data with identical <c>TestCaseName</c> values are considered duplicates; only the first occurrence is retained.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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