// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters.DataProviders;

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
    /// The type of the data provider to create. Must implement <see cref="ITestDataProvider{TTestData}"/>.
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
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate - will be filtered
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// var provider = testData.ToDataProvider(
    ///     td => new MyDataProvider(td));
    /// // Result: provider contains 2 items (duplicate removed)
    /// </code>
    /// </example>
    public static TDataProvider ToDataProvider<TDataProvider, TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TDataProvider> initDataProvider)
    where TTestData : notnull, ITestData
    where TDataProvider : ITestDataProvider<TTestData>
    {
        var snapshot =
            NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        var testData = snapshot[0];
        var dataProvider =
            NotNull(initDataProvider, nameof(initDataProvider))(
                testData);
        var count = snapshot.Length;

        if (count > 1)
        {
            var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
            _ = namedCases.Add(testData);

            for (int i = 1; i < count; i++)
            {
                testData = snapshot[i];

                // Deduplicate based on 'NamedCase' identity/equality semantics
                if (namedCases.Add(testData))
                {
                    dataProvider.AddRow(testData);
                }
            }
        }

        return dataProvider;
    }

    /// <summary>
    /// Converts a collection of test data into a data provider instance using the default constructor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This overload</strong> uses the <c>new()</c> constraint to instantiate the data provider
    /// directly, without requiring an initializer function. All test data items are added via
    /// <see cref="ITestDataProvider{TTestData}.AddRow(TTestData)"/>.
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
    /// The type of the data provider to create. Must implement <see cref="ITestDataProvider{TTestData}"/>
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
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate - will be filtered
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// var provider = testData.ToDataProvider&lt;MyDataProvider, TestDataReturns&lt;int&gt;&gt;();
    /// // Result: provider contains 2 items (duplicate removed)
    /// </code>
    /// </example>
    public static TDataProvider ToDataProvider<TDataProvider, TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    where TDataProvider : ITestDataProvider<TTestData>, new()
    {
        var snapshot =
            NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        var dataProvider = new TDataProvider();
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions - foreach is more performant for HashSet-based deduplication
        foreach (var testData in snapshot)
        {
            // Deduplicate based on 'NamedCase' identity/equality semantics
            if (namedCases.Add(testData))
            {
                dataProvider.AddRow(testData);
            }
        }
#pragma warning restore S3267

        return dataProvider;
    }
}