// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;
using Portamical.DataProviders;

namespace Portamical.Converters;

/// <summary>
/// Provides extension methods for converting collections of test data to distinct, read-only collections and for
/// transforming test data collections into data provider formats suitable for parameterized testing.
/// </summary>
/// <remarks>The methods in this class help ensure that test data collections are deduplicated based on their
/// identity and are returned in immutable forms. These utilities are intended for use in test scenarios where unique,
/// read-only collections or data provider objects are required. All methods are static and are designed to be used as
/// extension methods on <see cref="IEnumerable{TTestData}" />.</remarks>
public static class CollectionConverter
{
    /// <summary>
    /// Creates a read-only collection containing distinct elements from the specified test data collection.
    /// </summary>
    /// <typeparam name="TTestData">The type of elements in the test data collection. Must implement ITestData and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data elements from which to create a distinct, read-only collection. Cannot be null.</param>
    /// <returns>A read-only collection containing the distinct elements from the input collection. The order of elements is
    /// preserved from the original collection.</returns>
    public static TTestData[] ToDistinctArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        testData => testData);

    /// <summary>
    /// Returns a read-only collection of distinct argument arrays generated from the specified test data collection
    /// using the provided argument code.
    /// </summary>
    /// <remarks>Each element in the returned collection corresponds to the arguments produced by calling
    /// ToArgs on each test data item with the specified argument code. Duplicate argument arrays are removed based on
    /// their contents.</remarks>
    /// <typeparam name="TTestData">The type of the test data elements. Must implement the ITestData interface and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data items from which to generate argument arrays. Cannot be null.</param>
    /// <param name="argsCode">The argument code that determines how arguments are extracted from each test data item.</param>
    /// <returns>A read-only collection containing unique arrays of arguments produced from the test data. The collection is
    /// empty if the input collection contains no items.</returns>
    public static IReadOnlyCollection<object?[]> ToDistinctReadOnly<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        testData => testData.ToArgs(argsCode));

    /// <summary>
    /// Creates a read-only collection of distinct argument arrays from the specified test data collection, using the
    /// provided argument and property codes to extract values.
    /// </summary>
    /// <remarks>The returned collection contains only distinct arrays, where uniqueness is determined by the
    /// values of the extracted arguments and properties. The order of elements in the collection is not
    /// guaranteed.</remarks>
    /// <typeparam name="TTestData">The type of the test data elements. Must implement the ITestData interface and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data items from which to generate argument arrays. Cannot be null.</param>
    /// <param name="argsCode">The code specifying which arguments to extract from each test data item.</param>
    /// <param name="propsCode">The code specifying which properties to extract from each test data item.</param>
    /// <returns>A read-only collection containing unique arrays of arguments and properties extracted from the test data. The
    /// collection is empty if no items are found.</returns>
    public static IReadOnlyCollection<object?[]> ToDistinctReadOnly<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        testData => testData.ToArgs(argsCode, propsCode));

    /// <summary>
    /// Converts a collection of test data items to a distinct, read-only collection of rows using the specified
    /// conversion function.
    /// </summary>
    /// <remarks>The resulting collection contains only unique rows as determined by the conversion function.
    /// The order of the output collection is not guaranteed.</remarks>
    /// <typeparam name="TTestData">The type of the input test data items. Must implement the ITestData interface and cannot be null.</typeparam>
    /// <typeparam name="TRow">The type of the output row elements produced by the conversion function.</typeparam>
    /// <param name="testDataCollection">The collection of test data items to convert. Cannot be null.</param>
    /// <param name="convertRow">A function that converts each test data item, along with the provided ArgsCode and optional test method name, to
    /// a row of type TRow. Cannot be null.</param>
    /// <param name="argsCode">The ArgsCode instance to pass to the conversion function. Cannot be null.</param>
    /// <param name="testMethodName">An optional name of the test method to provide to the conversion function. May be null.</param>
    /// <returns>A read-only collection containing the distinct rows produced by applying the conversion function to each test
    /// data item.</returns>
    public static IReadOnlyCollection<TRow> ToDistinctReadOnly<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        testData => convertRow(
            testData,
            argsCode.Defined(nameof(argsCode)),
        testMethodName));

    /// <summary>
    /// Converts a collection of test data into a data provider instance, initializing it with the specified arguments
    /// and test method name.
    /// </summary>
    /// <remarks>The first item in the collection is used to initialize the data provider. Additional items
    /// are added using the data provider's AddRow method. The method requires that the collection is not
    /// empty.</remarks>
    /// <typeparam name="TDataProvider">The type of the data provider to create. Must implement ITestDataProvider<TTestData>.</typeparam>
    /// <typeparam name="TTestData">The type of test data contained in the collection. Must implement ITestData and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data items to be provided to the data provider. Cannot be null and must contain at least
    /// one item.</param>
    /// <param name="initDataProvider">A function that initializes a new data provider instance using a test data item, argument code, and optional
    /// test method name. Cannot be null.</param>
    /// <param name="argsCode">The argument code to pass to the data provider initializer.</param>
    /// <param name="testMethodName">The name of the test method to associate with the data provider, or null if not applicable.</param>
    /// <returns>A data provider instance containing all test data items from the collection.</returns>
    public static TDataProvider ToDataProvider<TDataProvider, TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TDataProvider> initDataProvider,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    where TDataProvider : ITestDataProvider<TTestData>
    {
        var testDatas = testDataCollection.ToDistinctArray();
        var dataProvider = NotNull(
            initDataProvider, nameof(initDataProvider))(
                testDatas[0],
                argsCode,
                testMethodName);
        var count = testDatas.Length;

        if (count > 1)
        {
            for (int i = 1; i < count; i++)
            {
                dataProvider.AddRow(testDatas[i]);
            }
        }

        return dataProvider;
    }

    private static TRow[] ToDistinctArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        _ = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        _ = NotNull(convertRow, nameof(convertRow));

        // Deduplicate based on 'NamedCase' identity/equality semantics
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
        var rowList = new List<TRow>();

        foreach (var testData in testDataCollection)
        {
            if (namedCases.Add(testData))
            {
                var row = convertRow(testData);
                rowList.Add(row);
            }
        }

        return [.. rowList];
    }
}