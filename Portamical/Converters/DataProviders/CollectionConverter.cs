// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.DataProviders;

namespace Portamical.Converters.DataProviders;

/// <summary>
/// Provides extension methods for converting test data collections into <see cref="ITestDataRegistry{TTestData}"/> instances.
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
    #region ToDataProvider<TDataProvider, TTestData>

    /// <summary>
    /// Converts a collection of test data into a data provider instance (primary implementation).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is the PRIMARY implementation.</strong> The overload with <c>testMethodName</c>
    /// delegates to this method by wrapping the initializer function.
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
    /// for O(n) deduplication. Does not use aggressive inlining due to loop and complex logic.
    /// </para>
    /// </remarks>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="ITestDataRegistry{TTestData}"/>.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least
    /// one item.
    /// </param>
    /// <param name="initDataProvider">
    /// A function that initializes a new data provider instance using a test data item. Cannot be null.
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
    /// var provider = td.ToConvertedRows(
    ///     td => new MyDataProvider(td));
    /// // Result: provider contains 2 items (duplicate removed)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDataProvider ToDataProvider<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TDataProvider> initDataProvider)
    where TDataProvider : notnull, IDataProvider<TTestData, TRow>
    where TTestData : notnull, ITestData
    => testDataCollection.ToDataProvider<TDataProvider, TTestData, TRow>(
        initDataProvider,
        isDistinct: false);

    /// <summary>
    /// Converts a collection of test data into a data provider instance using the default constructor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This overload</strong> uses the <c>new()</c> constraint to instantiate the data provider
    /// directly, without requiring an initializer function. All test data items are added via
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
    /// for O(n) deduplication. Uses <c>foreach</c> instead of LINQ for better performance with HashSet-based
    /// deduplication (see suppression of S3267).
    /// </para>
    /// </remarks>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="ITestDataRegistry{TTestData}"/>
    /// and have a parameterless constructor.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
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
    /// var provider = td.ToConvertedRows&lt;MyDataProvider, TestDataReturns&lt;int&gt;&gt;();
    /// // Result: provider contains 2 items (duplicate removed)
    /// </code>
    /// </example>
    public static TDataProvider ToDataProvider<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    where TDataProvider : notnull, IDataProvider<TTestData, TRow>, new()
    {
        var snapshot = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        var dataProvider = new TDataProvider();
        dataProvider.AddRange(snapshot);

        return dataProvider;
    }

    #endregion

    #region ToDistinctDataProvider<TDataProvider, TTestData>

    /// <summary>
    /// Converts a collection of test data into a data provider instance (primary implementation).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is the PRIMARY implementation.</strong> The overload with <c>testMethodName</c>
    /// delegates to this method by wrapping the initializer function.
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
    /// for O(n) deduplication. Does not use aggressive inlining due to loop and complex logic.
    /// </para>
    /// </remarks>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="ITestDataRegistry{TTestData}"/>.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least
    /// one item.
    /// </param>
    /// <param name="initDataProvider">
    /// A function that initializes a new data provider instance using a test data item. Cannot be null.
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
    /// var provider = td.ToDistinctDataProvider(
    ///     td => new MyDataProvider(td));
    /// // Result: provider contains 2 items (duplicate removed)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDataProvider ToDistinctDataProvider<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TDataProvider> initDataProvider)
    where TDataProvider : notnull, IDataProvider<TTestData, TRow>
    where TTestData : notnull, ITestData
    => testDataCollection.ToDataProvider<TDataProvider, TTestData, TRow>(
        initDataProvider,
        isDistinct: true);

    /// <summary>
    /// Converts a collection of test data into a data provider instance using the default constructor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This overload</strong> uses the <c>new()</c> constraint to instantiate the data provider
    /// directly, without requiring an initializer function. All test data items are added via
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
    /// for O(n) deduplication. Uses <c>foreach</c> instead of LINQ for better performance with HashSet-based
    /// deduplication (see suppression of S3267).
    /// </para>
    /// </remarks>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="ITestDataRegistry{TTestData}"/>
    /// and have a parameterless constructor.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
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
    /// var provider = td.ToDistinctDataProvider&lt;MyDataProvider, TestDataReturns&lt;int&gt;&gt;();
    /// // Result: provider contains 2 items (duplicate removed)
    /// </code>
    /// </example>
    public static TDataProvider ToDistinctDataProvider<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    where TDataProvider : notnull, IDataProvider<TTestData, TRow>, new()
    {
        var snapshot = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        var dataProvider = new TDataProvider();
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);

        foreach (var testData in snapshot)
        {
            testData.ExecuteIfDistinct(namedCases,
                action: () => dataProvider.AddRow(testData));
        }

        return dataProvider;
    }

    #endregion

    #region Helper methods

    /// <summary>
    /// Core helper method that converts a test data collection into a data provider instance,
    /// with optional deduplication based on test case names.
    /// </summary>
    /// <typeparam name="TDataProvider">
    /// The type of the data provider to create. Must implement <see cref="IDataProvider{TTestData, TRow}"/>.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of test data contained in the collection. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The row type for the test framework produced by the data provider.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data items to be provided to the data provider. Cannot be null and must contain at least
    /// one item.
    /// </param>
    /// <param name="initDataProvider">
    /// A function that initializes a new data provider instance using the first test data item. Cannot be null.
    /// </param>
    /// <param name="isDistinct">
    /// If <see langword="true"/>, removes duplicate test data based on <see cref="INamedCase.TestCaseName"/> using
    /// <see cref="NamedCase.Comparer"/>; if <see langword="false"/>, adds all items without deduplication.
    /// </param>
    /// <returns>
    /// A data provider instance containing test data items from the collection. If <paramref name="isDistinct"/>
    /// is <see langword="true"/>, duplicates are removed based on <see cref="INamedCase.TestCaseName"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="initDataProvider"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Algorithm:</strong>
    /// </para>
    /// <list type="number">
    ///   <item>Snapshots and validates the collection</item>
    ///   <item>Initializes the data provider using the first test data item via <paramref name="initDataProvider"/></item>
    ///   <item>If collection has only one item, returns immediately</item>
    ///   <item>If <paramref name="isDistinct"/> is <see langword="true"/>, uses <see cref="HashSet{T}"/> with <see cref="NamedCase.Comparer"/> for O(n) deduplication</item>
    ///   <item>Iterates through remaining items, adding them according to the deduplication strategy</item>
    /// </list>
    /// <para>
    /// Uses a local <c>addRows</c> method to iterate efficiently through remaining items starting from index 1.
    /// </para>
    /// </remarks>
    private static TDataProvider ToDataProvider<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TDataProvider> initDataProvider,
        bool isDistinct)
    where TDataProvider : notnull, IDataProvider<TTestData, TRow>
    where TTestData : notnull, ITestData
    {
        var (snapshot, count) = Converters.CollectionConverter.SnapshotWithCount(testDataCollection);
        var testData = snapshot[0];
        var dataProvider = NotNull(initDataProvider, nameof(initDataProvider))(
            testData);

        if (count == 1)
        {
            return dataProvider;
        }

        if (isDistinct)
        {
            var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
            _ = namedCases.Add(testData);

            addRows(td => td.ExecuteIfDistinct(namedCases,
                action: () => dataProvider.AddRow(td)));
        }
        else
        {
            addRows(dataProvider.AddRow);
        }

        return dataProvider;

        #region Local methods

        void addRows(Action<TTestData> addRow)
        {
            for (int i = 1; i < count; i++)
            {
                testData = snapshot[i];
                addRow(testData);
            }
        }

        #endregion
    }

    #endregion
}