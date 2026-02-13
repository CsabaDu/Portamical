// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

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
    public static IReadOnlyCollection<TTestData> ToDistinctReadOnly<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
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
    => testDataCollection.ToDistinctReadOnly(
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
    => testDataCollection.ToDistinctReadOnly(
        testData => testData.ToArgs(argsCode, propsCode));

    /// <summary>
    /// Adds each distinct test data item from the specified collection to the given data provider and returns the data
    /// provider instance.
    /// </summary>
    /// <remarks>This method is intended for use with data providers that support both storing and converting
    /// test data. The original data provider instance is returned to enable method chaining or further
    /// configuration.</remarks>
    /// <typeparam name="TDataProvider">The type of the data provider that stores and converts test data rows.</typeparam>
    /// <typeparam name="TTestData">The type of the test data items to be added to the data provider. Must implement <see cref="ITestData"/> and
    /// cannot be null.</typeparam>
    /// <typeparam name="TRow">The type of the row representation used by the data provider.</typeparam>
    /// <param name="testDataCollection">The collection of test data items to add to the data provider. Only distinct items are added. Cannot be null.</param>
    /// <param name="dataProvider">The data provider instance to which the test data items are added. Cannot be null.</param>
    /// <returns>The data provider instance after the test data items have been added.</returns>
    public static TDataProvider Convert<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TDataProvider> initDataProvider,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    where TDataProvider : IDataProvider<TTestData, TRow>, ITestDataConverter<TTestData, TRow>
    {
        var testDataArray = ToDistinctReadOnly(
            testDataCollection,
            testData => testData);
        var dataProvider = NotNull(
            initDataProvider, nameof(initDataProvider))(
                testDataArray[0],
                argsCode,
                testMethodName);

        if (testDataArray.Length > 1)
        {
            for (int i = 1; i < testDataArray.Length; i++)
            {
                dataProvider.AddRow(testDataArray[i]);
            }
        }

        return dataProvider;
    }

    /// <summary>
    /// Converts a collection of test data into a data provider by transforming each test data item into a row and
    /// aggregating the rows using the specified initialization and addition functions.
    /// </summary>
    /// <remarks>The first row in the collection is used to initialize the data provider. Subsequent rows are
    /// added using the provided addRow function. The method requires that the test data collection contains at least
    /// one item.</remarks>
    /// <typeparam name="TDataProvider">The type of the data provider to be created and returned.</typeparam>
    /// <typeparam name="TTestData">The type of the input test data items. Must implement ITestData and cannot be null.</typeparam>
    /// <typeparam name="TRow">The type representing a single row derived from the test data.</typeparam>
    /// <param name="testDataCollection">The collection of test data items to convert. Cannot be null.</param>
    /// <param name="initDataProvider">A function that initializes the data provider using the first row.</param>
    /// <param name="convertRow">A function that converts a test data item, along with the specified arguments code and optional test method
    /// name, into a row.</param>
    /// <param name="addRow">A function that adds a row to the data provider.</param>
    /// <param name="argsCode">The arguments code to use when converting test data items to rows.</param>
    /// <param name="testMethodName">The name of the test method, or null if not specified. Used during row conversion.</param>
    /// <returns>A data provider instance containing all rows converted from the input test data collection.</returns>
    public static TDataProvider Convert<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TRow, TDataProvider> initDataProvider,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        Action<TDataProvider, TRow> addRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    {
        var rowCollection = testDataCollection.Convert(
            convertRow,
            argsCode,
            testMethodName);

        if (rowCollection is not TRow[] rows)
        {
            rows = [.. rowCollection];
        }

        var dataProvider = initDataProvider(rows[0]);

        if (rows.Length > 0)
        {
            for (int i = 1; i < rows.Length; i++)
            {
                addRow(dataProvider, rows[i]);
            }
        }

        return dataProvider;
    }

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
    public static IReadOnlyCollection<TRow> Convert<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        testData => convertRow(
            testData,
            argsCode.Defined(nameof(argsCode)),
        testMethodName));

    private static TRow[] ToDistinctReadOnly<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        _ = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        _ = NotNull(convertRow, nameof(convertRow));

        // Deduplicate based on 'NamedCase' identity/equality semantics
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
        var rows = new List<TRow>();

        foreach (var testData in testDataCollection)
        {
            if (namedCases.Add(testData))
            {
                rows.Add(convertRow(testData));
            }
        }

        return [.. rows];
    }
}