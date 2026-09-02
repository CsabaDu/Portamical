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

    /// <summary>
    /// Converts a collection of test data into an object-array data provider instance with <see cref="ArgsCode"/> and <see cref="PropsCode"/> configuration.
    /// </summary>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="ITestDataProvider{TTestData}"/> with <c>object?[]</c> as the row type.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least one item.
    /// </param>
    /// <param name="initDataProvider">
    /// A function that initializes a new data provider instance using a test data item, <see cref="ArgsCode"/>, and <see cref="PropsCode"/>. Cannot be null.
    /// </param>
    /// <param name="argsCode">
    /// The <see cref="ArgsCode"/> configuration for argument extraction. Cannot be undefined.
    /// </param>
    /// <param name="propsCode">
    /// The <see cref="PropsCode"/> configuration for property extraction. Cannot be undefined.
    /// </param>
    /// <returns>
    /// A data provider instance containing test data items from the collection, configured to convert them to <c>object?[]</c> rows.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="initDataProvider"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty, or <paramref name="argsCode"/> or <paramref name="propsCode"/> is undefined.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method wraps the <paramref name="initDataProvider"/> function to include <paramref name="argsCode"/> and
    /// <paramref name="propsCode"/> parameters, then delegates to
    /// <see cref="DataProviders.CollectionConverter.ToDataProvider{TDataProvider, TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, TDataProvider})"/>
    /// with TConvertedRows set to <c>object?[]</c>.
    /// </para>
    /// <para>
    /// This overload does NOT perform deduplication. For deduplication, use <see cref="ToDistinctDataProvider{TDataProvider, TTestData}"/>.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDataProvider ToDataProvider<TDataProvider, TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, PropsCode, TDataProvider> initDataProvider,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    where TDataProvider : notnull, ITestDataProvider<TTestData>
    => testDataCollection.ToDataProvider<TDataProvider, TTestData, object?[]>(
        testData => NotNull(initDataProvider, nameof(initDataProvider))(
            testData,
            argsCode,
            propsCode));

    #endregion

    #region ToDistinctDataProvider

    /// <summary>
    /// Converts a collection of test data into a distinct object-array data provider instance with <see cref="ArgsCode"/> and <see cref="PropsCode"/> configuration.
    /// Removes duplicates based on <see cref="INamedCase.TestCaseName"/> identity.
    /// </summary>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="ITestDataProvider{TTestData}"/> with <c>object?[]</c> as the row type.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least one item.
    /// </param>
    /// <param name="initDataProvider">
    /// A function that initializes a new data provider instance using a test data item, <see cref="ArgsCode"/>, and <see cref="PropsCode"/>. Cannot be null.
    /// </param>
    /// <param name="argsCode">
    /// The <see cref="ArgsCode"/> configuration for argument extraction. Cannot be undefined.
    /// </param>
    /// <param name="propsCode">
    /// The <see cref="PropsCode"/> configuration for property extraction. Cannot be undefined.
    /// </param>
    /// <returns>
    /// A data provider instance containing distinct test data items from the collection, with duplicates removed based on <see cref="INamedCase.TestCaseName"/>.
    /// Configured to convert test data to <c>object?[]</c> rows.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="initDataProvider"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty, or <paramref name="argsCode"/> or <paramref name="propsCode"/> is undefined.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method wraps the <paramref name="initDataProvider"/> function to include <paramref name="argsCode"/> and
    /// <paramref name="propsCode"/> parameters, then delegates to
    /// <see cref="DataProviders.CollectionConverter.ToDistinctDataProvider{TDataProvider, TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, TDataProvider})"/>
    /// with TConvertedRows set to <c>object?[]</c>.
    /// </para>
    /// <para>
    /// Deduplication uses <see cref="NamedCase.Comparer"/> based on <see cref="INamedCase.TestCaseName"/>.
    /// Test data with identical <c>TestCaseName</c> values are considered duplicates; only the first occurrence is retained.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDataProvider ToDistinctDataProvider<TDataProvider, TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, PropsCode, TDataProvider> initDataProvider,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    where TDataProvider : notnull, ITestDataProvider<TTestData>
    => testDataCollection.ToDistinctDataProvider<TDataProvider, TTestData, object?[]>(
        testData => NotNull(initDataProvider, nameof(initDataProvider))(
            testData,
            argsCode,
            propsCode));

    #endregion
}