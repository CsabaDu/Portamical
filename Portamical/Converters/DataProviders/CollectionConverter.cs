// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.DataProviders;

namespace Portamical.Converters.DataProviders;

/// <summary>
/// Provides extension methods for converting test data collections into data provider instances that
/// implement <see cref="IDataProvider{TTestData, TRow}"/>.
/// </summary>
/// <remarks>
/// <para>
/// This class exposes two families of conversion methods: <c>ToDataProvider</c>, which adds every item
/// from the source collection, and <c>ToDistinctDataProvider</c>, which additionally removes duplicate
/// test data based on test case identity (via <see cref="INamedCase.TestCaseName"/>). Each family provides
/// an overload accepting a custom initializer function and an overload relying on a parameterless
/// constructor (<c>new()</c>).
/// </para>
/// <para>
/// <strong>Deduplication Strategy (distinct overloads only):</strong> Uses <see cref="NamedCase.Comparer"/>
/// for semantic equality based on test case names, not reference equality. This ensures that test data with
/// identical <c>TestCaseName</c> values are treated as duplicates, with the first occurrence retained.
/// </para>
/// </remarks>
public static class CollectionConverter
{
    #region ToDataProvider<TDataProvider, TTestData>

    /// <summary>
    /// Converts a collection of test data into a data provider instance, initializing it from the first
    /// test data item (primary implementation).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is the PRIMARY implementation.</strong> The parameterless-constructor overload
    /// delegates to this method by wrapping <see cref="ITestDataRegistry{TTestData}.AddRow(TTestData)"/>
    /// as the row-adding action.
    /// </para>
    /// <para>
    /// <strong>No Deduplication:</strong> Unlike <see cref="ToDistinctDataProvider{TDataProvider, TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, TDataProvider})"/>,
    /// this method adds every item from the source collection, including duplicates by
    /// <see cref="INamedCase.TestCaseName"/>.
    /// </para>
    /// <para>
    /// <strong>Algorithm:</strong>
    /// </para>
    /// <list type="number">
    ///   <item>Converts the collection to an array snapshot and validates it is not empty</item>
    ///   <item>Initializes the data provider with the first test data item via <paramref name="initDataProvider"/></item>
    ///   <item>Adds each remaining item to the data provider via <see cref="ITestDataRegistry{TTestData}.AddRow(TTestData)"/></item>
    ///   <item>Returns the populated data provider</item>
    /// </list>
    /// </remarks>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="IDataProvider{TTestData, TRow}"/>.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The row type exposed by the data provider.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least
    /// one item.
    /// </param>
    /// <param name="initDataProvider">
    /// A function that initializes a new data provider instance using the first test data item. Cannot be null.
    /// </param>
    /// <returns>
    /// A data provider instance containing all test data items from the collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="initDataProvider"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <example>
    /// <code>
    /// var td = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// var dataProvider = td.ToDataProvider&lt;MyDataProvider, TestDataReturns&lt;int&gt;, object[]&gt;(
    ///     td => new MyDataProvider(td));
    /// // Result: dataProvider contains all 2 items
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDataProvider ToDataProvider<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TDataProvider> initDataProvider)
    where TDataProvider : notnull, IDataProvider<TTestData, TRow>
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRows(
        initDataProvider,
        addConvertedRow: AddConvertedRow<TTestData, TDataProvider, TRow>);

    /// <summary>
    /// Converts a collection of test data into a data provider instance, creating it via its parameterless
    /// constructor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This overload</strong> uses the <c>new()</c> constraint to instantiate the data provider
    /// directly, without requiring an initializer function. All test data items are added via
    /// <see cref="ITestDataRegistry{TTestData}.AddRow(TTestData)"/>.
    /// </para>
    /// <para>
    /// <strong>No Deduplication:</strong> Unlike <see cref="ToDistinctDataProvider{TDataProvider, TTestData, TRow}(IEnumerable{TTestData})"/>,
    /// this method adds every item from the source collection, including duplicates by
    /// <see cref="INamedCase.TestCaseName"/>.
    /// </para>
    /// <para>
    /// <strong>Algorithm:</strong>
    /// </para>
    /// <list type="number">
    ///   <item>Converts the collection to an array snapshot and validates it is not empty</item>
    ///   <item>Creates a new data provider instance using the default constructor</item>
    ///   <item>Adds every item to the data provider via <see cref="ITestDataRegistry{TTestData}.AddRow(TTestData)"/></item>
    ///   <item>Returns the populated data provider</item>
    /// </list>
    /// </remarks>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="IDataProvider{TTestData, TRow}"/>
    /// and have a parameterless constructor.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The row type exposed by the data provider.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least
    /// one item.
    /// </param>
    /// <returns>
    /// A data provider instance containing all test data items from the collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <example>
    /// <code>
    /// var td = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// var dataProvider = td.ToDataProvider&lt;MyDataProvider, TestDataReturns&lt;int&gt;, object[]&gt;();
    /// // Result: dataProvider contains all 2 items
    /// </code>
    /// </example>
    public static TDataProvider ToDataProvider<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection)
    where TDataProvider : IDataProvider<TTestData, TRow>, new()
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRows<TTestData, TDataProvider>(
        addConvertedRow: AddConvertedRow<TTestData, TDataProvider, TRow>);

    #endregion

    #region ToDistinctDataProvider<TDataProvider, TTestData>

    /// <summary>
    /// Converts a collection of test data into a data provider instance, initializing it from the first
    /// test data item and removing duplicate items by test case name (primary implementation).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is the PRIMARY implementation.</strong> The parameterless-constructor overload
    /// delegates to this method by wrapping <see cref="ITestDataRegistry{TTestData}.AddRow(TTestData)"/>
    /// as the row-adding action.
    /// </para>
    /// <para>
    /// <strong>Deduplication:</strong> Uses <see cref="NamedCase.Comparer"/> to remove duplicate
    /// test data based on <see cref="INamedCase.TestCaseName"/>. Only the first occurrence of each
    /// unique test case name is retained.
    /// </para>
    /// <para>
    /// <strong>Algorithm:</strong>
    /// </para>
    /// <list type="number">
    ///   <item>Converts the collection to an array snapshot and validates it is not empty</item>
    ///   <item>Initializes the data provider with the first test data item</item>
    ///   <item>For remaining items, adds only those with unique <c>TestCaseName</c> values</item>
    ///   <item>Returns the populated data provider</item>
    /// </list>
    /// <para>
    /// <strong>Performance:</strong> Uses <see cref="HashSet{T}"/> with <see cref="NamedCase.Comparer"/>
    /// for O(n) deduplication.
    /// </para>
    /// </remarks>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="IDataProvider{TTestData, TRow}"/>.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The row type exposed by the data provider.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least
    /// one item.
    /// </param>
    /// <param name="initDataProvider">
    /// A function that initializes a new data provider instance using the first test data item. Cannot be null.
    /// </param>
    /// <returns>
    /// A data provider instance containing all distinct test data items from the collection, with duplicates
    /// removed based on <see cref="INamedCase.TestCaseName"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="initDataProvider"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <example>
    /// <code>
    /// var td = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate - will be filtered
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// var dataProvider = td.ToDistinctDataProvider&lt;MyDataProvider, TestDataReturns&lt;int&gt;, object[]&gt;(
    ///     td => new MyDataProvider(td));
    /// // Result: dataProvider contains 2 items (duplicate removed)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDataProvider ToDistinctDataProvider<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TDataProvider> initDataProvider)
    where TDataProvider : notnull, IDataProvider<TTestData, TRow>
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctConvertedRows(
        initDataProvider,
        addConvertedRow: AddConvertedRow<TTestData, TDataProvider, TRow>);

    /// <summary>
    /// Converts a collection of test data into a data provider instance, creating it via its parameterless
    /// constructor and removing duplicate items by test case name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This overload</strong> uses the <c>new()</c> constraint to instantiate the data provider
    /// directly, without requiring an initializer function. All non-duplicate test data items are added via
    /// <see cref="ITestDataRegistry{TTestData}.AddRow(TTestData)"/>.
    /// </para>
    /// <para>
    /// <strong>Deduplication:</strong> Uses <see cref="NamedCase.Comparer"/> to remove duplicate
    /// test data based on <see cref="INamedCase.TestCaseName"/>. Only the first occurrence of each
    /// unique test case name is retained.
    /// </para>
    /// <para>
    /// <strong>Algorithm:</strong>
    /// </para>
    /// <list type="number">
    ///   <item>Converts the collection to an array snapshot and validates it is not empty</item>
    ///   <item>Creates a new data provider instance using the default constructor</item>
    ///   <item>Iterates through all items, adding only those with unique <c>TestCaseName</c> values</item>
    ///   <item>Returns the populated data provider</item>
    /// </list>
    /// <para>
    /// <strong>Performance:</strong> Uses <see cref="HashSet{T}"/> with <see cref="NamedCase.Comparer"/>
    /// for O(n) deduplication.
    /// </para>
    /// </remarks>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="IDataProvider{TTestData, TRow}"/>
    /// and have a parameterless constructor.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The row type exposed by the data provider.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least
    /// one item.
    /// </param>
    /// <returns>
    /// A data provider instance containing all distinct test data items from the collection, with duplicates
    /// removed based on <see cref="INamedCase.TestCaseName"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <example>
    /// <code>
    /// var td = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate - will be filtered
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// var dataProvider = td.ToDistinctDataProvider&lt;MyDataProvider, TestDataReturns&lt;int&gt;, object[]&gt;();
    /// // Result: dataProvider contains 2 items (duplicate removed)
    /// </code>
    /// </example>
    public static TDataProvider ToDistinctDataProvider<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection)
    where TDataProvider : IDataProvider<TTestData, TRow>, new()
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctConvertedRows<TTestData, TDataProvider>(
        addConvertedRow: AddConvertedRow<TTestData, TDataProvider, TRow>);

    #endregion

    #region Helper methods

    /// <summary>
    /// Adds a single test data item to a data provider's row registry.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider. Must implement <see cref="IDataProvider{TTestData, TRow}"/>.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The row type exposed by the data provider.
    /// </typeparam>
    /// <param name="dataProvider">The data provider to add the row to.</param>
    /// <param name="testData">The test data item to add.</param>
    /// <remarks>
    /// Used as the <c>addConvertedRow</c> delegate passed to <see cref="CollectionConverter.ToConvertedRows{TTestData, TConvertedRows}(IEnumerable{TTestData}, Func{TTestData, TConvertedRows}, Action{TConvertedRows, TTestData})"/>
    /// and its distinct counterpart, bridging the generic converter infrastructure to
    /// <see cref="ITestDataRegistry{TTestData}.AddRow(TTestData)"/>.
    /// </remarks>
    private static void AddConvertedRow<TTestData, TDataProvider, TRow>(
        TDataProvider dataProvider,
        TTestData testData)
    where TDataProvider : IDataProvider<TTestData, TRow>
    where TTestData : notnull, ITestData
    => dataProvider.AddRow(testData);

    #endregion
}