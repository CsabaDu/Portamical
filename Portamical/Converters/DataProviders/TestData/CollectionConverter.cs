// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.TestData;

namespace Portamical.Converters.DataProviders.TestData;

/// <summary>
/// Provides extension methods for converting test data collections into identity data provider instances.
/// </summary>
/// <remarks>
/// <para>
/// This class provides specialized converters for identity-conversion data providers where the test data items
/// are returned as-is (TRow = TTestData). These methods delegate to the base
/// <see cref="DataProviders.CollectionConverter"/> with the row type set to the test data type.
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong> Uses <see cref="NamedCase.Comparer"/> for semantic equality
/// based on test case names (via <see cref="INamedCase.TestCaseName"/>), not reference equality.
/// This ensures that test data with identical <c>TestCaseName</c> values are treated as duplicates,
/// with the first occurrence retained.
/// </para>
/// </remarks>
public static class CollectionConverter
{
    #region ToDataProvider

    /// <summary>
    /// Converts a collection of test data into an identity data provider instance (TRow = TTestData).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// This type is also used as the row type (identity conversion).
    /// </typeparam>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="ITestDataProvider{TTestData}"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least one item.
    /// </param>
    /// <param name="initDataProvider">
    /// A function that initializes a new data provider instance using a test data item. Cannot be null.
    /// </param>
    /// <returns>
    /// A data provider instance containing test data items from the collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="initDataProvider"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is an identity-conversion specialization where test data items are returned as-is (row type = test data type).
    /// Delegates to <see cref="DataProviders.CollectionConverter.ToDataProvider{TDataProvider, TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, TDataProvider})"/>
    /// with TRow set to TTestData.
    /// </para>
    /// <para>
    /// This overload does NOT perform deduplication. For deduplication, use <see cref="ToDistinctDataProvider{TTestData, TDataProvider}(IEnumerable{TTestData}, Func{TTestData, TDataProvider})"/>.
    /// </para>
    /// </remarks>
    public static TDataProvider ToDataProvider<TTestData, TDataProvider>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TDataProvider> initDataProvider)
    where TDataProvider : notnull, ITestDataProvider<TTestData>
    where TTestData : notnull, ITestData
    => testDataCollection.ToDataProvider<TDataProvider, TTestData, TTestData>(
        initDataProvider);

    /// <summary>
    /// Converts a collection of test data into an identity data provider instance using the default constructor (TRow = TTestData).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// This type is also used as the row type (identity conversion).
    /// </typeparam>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="ITestDataProvider{TTestData}"/> and have a parameterless constructor.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least one item.
    /// </param>
    /// <returns>
    /// A data provider instance containing test data items from the collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is an identity-conversion specialization where test data items are returned as-is (row type = test data type).
    /// Uses the <c>new()</c> constraint to instantiate the data provider directly. Delegates to
    /// <see cref="DataProviders.CollectionConverter.ToDataProvider{TDataProvider, TTestData, TRow}(IEnumerable{TTestData})"/>
    /// with TRow set to TTestData.
    /// </para>
    /// <para>
    /// This overload does NOT perform deduplication. For deduplication, use <see cref="ToDistinctDataProvider{TTestData, TDataProvider}(IEnumerable{TTestData})"/>.
    /// </para>
    /// </remarks>
    public static TDataProvider ToDataProvider<TTestData, TDataProvider>(
        this IEnumerable<TTestData> testDataCollection)
    where TDataProvider : notnull, ITestDataProvider<TTestData>, new()
    where TTestData : notnull, ITestData
    => testDataCollection.ToDataProvider<TDataProvider, TTestData, TTestData>();

    #endregion

    #region ToDistinctDataProvider

    /// <summary>
    /// Converts a collection of test data into a distinct identity data provider instance (TRow = TTestData).
    /// Removes duplicates based on <see cref="INamedCase.TestCaseName"/> identity.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// This type is also used as the row type (identity conversion).
    /// </typeparam>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="ITestDataProvider{TTestData}"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least one item.
    /// </param>
    /// <param name="initDataProvider">
    /// A function that initializes a new data provider instance using a test data item. Cannot be null.
    /// </param>
    /// <returns>
    /// A data provider instance containing distinct test data items from the collection, with duplicates removed based on <see cref="INamedCase.TestCaseName"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="initDataProvider"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is an identity-conversion specialization where test data items are returned as-is (row type = test data type).
    /// Delegates to <see cref="DataProviders.CollectionConverter.ToDistinctDataProvider{TDataProvider, TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, TDataProvider})"/>
    /// with TRow set to TTestData.
    /// </para>
    /// <para>
    /// Deduplication uses <see cref="NamedCase.Comparer"/> based on <see cref="INamedCase.TestCaseName"/>.
    /// Test data with identical <c>TestCaseName</c> values are considered duplicates; only the first occurrence is retained.
    /// </para>
    /// </remarks>
    public static TDataProvider ToDistinctDataProvider<TTestData, TDataProvider>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TDataProvider> initDataProvider)
    where TDataProvider : notnull, ITestDataProvider<TTestData>
    where TTestData : notnull, ITestData
    => testDataCollection .ToDistinctDataProvider<TDataProvider, TTestData, TTestData>(
        initDataProvider);

    /// <summary>
    /// Converts a collection of test data into a distinct identity data provider instance using the default constructor (TRow = TTestData).
    /// Removes duplicates based on <see cref="INamedCase.TestCaseName"/> identity.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// This type is also used as the row type (identity conversion).
    /// </typeparam>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="ITestDataProvider{TTestData}"/> and have a parameterless constructor.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least one item.
    /// </param>
    /// <returns>
    /// A data provider instance containing distinct test data items from the collection, with duplicates removed based on <see cref="INamedCase.TestCaseName"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is an identity-conversion specialization where test data items are returned as-is (row type = test data type).
    /// Uses the <c>new()</c> constraint to instantiate the data provider directly. Delegates to
    /// <see cref="DataProviders.CollectionConverter.ToDistinctDataProvider{TDataProvider, TTestData, TRow}(IEnumerable{TTestData})"/>
    /// with TRow set to TTestData.
    /// </para>
    /// <para>
    /// Deduplication uses <see cref="NamedCase.Comparer"/> based on <see cref="INamedCase.TestCaseName"/>.
    /// Test data with identical <c>TestCaseName</c> values are considered duplicates; only the first occurrence is retained.
    /// </para>
    /// </remarks>
    public static TDataProvider ToDistinctDataProvider<TTestData, TDataProvider>(
        this IEnumerable<TTestData> testDataCollection)
    where TDataProvider : notnull, ITestDataProvider<TTestData>, new()
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctDataProvider<TDataProvider, TTestData, TTestData>();

    #endregion
}