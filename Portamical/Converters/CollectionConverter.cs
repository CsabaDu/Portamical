// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters;

/// <remarks>
/// <para>
/// The methods in this class help ensure that test data collections are deduplicated based on test case
/// identity (via <see cref="INamedCase.TestCaseName"/>) and are returned as arrays.
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong> Uses <see cref="NamedCase.Comparer"/> for semantic equality
/// based on test case names, not reference equality. This ensures that test data with identical
/// <c>TestCaseName</c> values are treated as duplicates, with the first occurrence retained.
/// </para>
/// <para>
/// <strong>Return Type:</strong> All methods return arrays for optimal performance with test frameworks.
/// Arrays provide zero-allocation enumeration, direct indexing, and better compatibility with
/// data-driven test attributes (MSTest <c>DynamicData</c>, xUnit <c>MemberData</c>, NUnit <c>TestCaseSource</c>).
/// </para>
/// </remarks>
public static class CollectionConverter
{
    #region ToArray

    #region TRow[] ToArray base method
    public static TRow[] ToArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        var snapshot = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        _ = NotNull(convertRow, nameof(convertRow));
        var count = snapshot.Length;
        var converted = new TRow[count];

        for (int i = 0; i < count; i++)
        {
            var testData = snapshot[i];
            converted[i] = convertRow(testData);
        }
        
        return converted;
    }

    #endregion

    #region Wrapper methods

    #region TTestData[]

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTestData[] ToArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => NotNullOrEmpty(testDataCollection, nameof(testDataCollection));

    #endregion

    #region object?[][]

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object?[][] ToArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArray(
        convertRow: testData => testData.ToArgs(argsCode));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object?[][] ToArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArray(
        convertRow: testData => testData.ToArgs(argsCode, propsCode));

    #endregion

    #region TRow[]

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TRow[] ToArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArray(
        convertRow: testData => NotNull(convertRow, nameof(convertRow))(
            testData,
            argsCode.Defined(nameof(argsCode)),
            testMethodName));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TRow[] ToArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArray(
        convertRow: testData => NotNull(convertRow, nameof(convertRow))(
            testData,
            testMethodName));

    #endregion

    #endregion

    #endregion ToArray  

    #region ToDistinctArray

    #region TRow[] ToDistinctArray base method

    /// <summary>
    /// Core deduplication method that converts a testDataCollection of test data into a distinct array of _rows
    /// using a custom conversion function.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Deduplication Strategy:</strong> This method removes duplicates based on 
    /// <see cref="INamedCase.TestCaseName"/> identity using <see cref="NamedCase.Comparer"/>.
    /// Test data items with identical <c>TestCaseName</c> values are considered duplicates, and only
    /// the first occurrence is retained.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Uses <see cref="HashSet{T}"/> with <see cref="NamedCase.Comparer"/>
    /// for O(n) deduplication. The <see cref="HashSet{T}.Add"/> method returns false for duplicates,
    /// which is used as a filter predicate.
    /// </para>
    /// <para>
    /// <strong>Order Preservation:</strong> The order of elements from the original testDataCollection is preserved
    /// in the output array. Duplicates are removed based on first-occurrence semantics.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">
    /// The type of test data in the input testDataCollection. Must implement <see cref="ITestData"/> 
    /// (which inherits <see cref="INamedCase"/>) and cannot be null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of elements in the output array, produced by <paramref name="convertRow"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The testDataCollection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="convertRow">
    /// A function that transforms each test data item into a row of type <typeparamref name="TRow"/>.
    /// Cannot be null. Called only for non-duplicate items.
    /// </param>
    /// <returns>
    /// An array containing the converted _rows for distinct test data items, preserving the order
    /// of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <example>
    /// <code>
    /// // Identity conversion (keeping test data as-is)
    /// var distinct = testDataCollection.ToDistinctArray(td => td);
    /// 
    /// // Convert to argument arrays
    /// var args = testDataCollection.ToDistinctArray(td => td.ToArgs(ArgsCode.Instance));
    /// 
    /// // Custom row conversion
    /// var _rows = testDataCollection.ToDistinctArray(td => new 
    /// { 
    ///     Name = td.TestCaseName, 
    ///     Args = td.ToArgs(ArgsCode.Instance) 
    /// });
    /// </code>
    /// </example>
    public static TRow[] ToDistinctArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        var snapshot = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        _ = NotNull(convertRow, nameof(convertRow));
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
        var rows = new List<TRow>(snapshot.Length);

        foreach (var testData in snapshot)
        {
            testData.ExecuteIfDistinct(namedCases,
                action: () => rows.Add(convertRow(testData)));
        }

        return [.. rows];
    }

    #endregion

    #region Wrapper methods

    #region TTestData[]

    /// <summary>
    /// Creates an array containing distinct elements from the specified test data testDataCollection.
    /// </summary>
    /// <typeparam name="TTestData">The type of elements in the test data testDataCollection. Must implement ITestData and cannot be null.</typeparam>
    /// <param name="testDataCollection">The testDataCollection of test data elements from which to create a distinct array. Cannot be null.</param>
    /// <returns>An array containing the distinct elements from the input testDataCollection. The order of elements is
    /// preserved from the original testDataCollection (first occurrence wins).</returns>
    /// <example>
    /// <code>
    /// // Basic deduplication
    /// var row = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// var distinct = row.ToDistinctArray();
    /// // Result: 2 elements (duplicate removed based on TestCaseName)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTestData[] ToDistinctArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        convertRow: testData => testData);

    #endregion

    #region object?[][]

    /// <summary>
    /// Returns a jagged array of distinct argument arrays generated from the specified test data testDataCollection
    /// using the provided argument code.
    /// </summary>
    /// <remarks>Each element in the returned array corresponds to the arguments produced by calling
    /// ToArgs on each test data item with the specified argument code. Duplicates are removed based on
    /// test case name identity using <see cref="NamedCase.Comparer"/>.</remarks>
    /// <typeparam name="TTestData">The type of the test data elements. Must implement the ITestData interface and cannot be null.</typeparam>
    /// <param name="testDataCollection">The testDataCollection of test data items from which to generate argument arrays. Cannot be null.</param>
    /// <param name="argsCode">The argument code that determines how arguments are extracted from each test data item.</param>
    /// <returns>A jagged array containing unique argument arrays produced from distinct test data items. 
    /// The array is empty if the input testDataCollection contains no items.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object?[][] ToDistinctArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        convertRow: testData => testData.ToArgs(argsCode));

    /// <summary>
    /// Creates a jagged array of distinct argument arrays from the specified test data testDataCollection, using the
    /// provided argument and property codes to extract values.
    /// </summary>
    /// <remarks>The returned array contains only distinct argument arrays, where uniqueness is determined by
    /// test case name identity using <see cref="NamedCase.Comparer"/>. The order of elements from the
    /// original testDataCollection is preserved (first occurrence wins).</remarks>
    /// <typeparam name="TTestData">The type of the test data elements. Must implement the ITestData interface and cannot be null.</typeparam>
    /// <param name="testDataCollection">The testDataCollection of test data items from which to generate argument arrays. Cannot be null.</param>
    /// <param name="argsCode">The code specifying which arguments to extract from each test data item.</param>
    /// <param name="propsCode">The code specifying which properties to extract from each test data item.</param>
    /// <returns>A jagged array containing unique argument arrays extracted from distinct test data items. 
    /// The array is empty if no items are found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object?[][] ToDistinctArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        convertRow: testData => testData.ToArgs(argsCode, propsCode));

    #endregion

    #region TRow[]

    /// <summary>
    /// Converts a testDataCollection of test data items to a distinct array of _rows using the specified
    /// conversion function.
    /// </summary>
    /// <remarks>The resulting array contains only unique _rows based on test case name identity
    /// using <see cref="NamedCase.Comparer"/>. The order of elements from the original testDataCollection is preserved.</remarks>
    /// <typeparam name="TTestData">The type of the input test data items. Must implement the ITestData interface and cannot be null.</typeparam>
    /// <typeparam name="TRow">The type of the output row elements produced by the conversion function.</typeparam>
    /// <param name="testDataCollection">The testDataCollection of test data items to convert. Cannot be null.</param>
    /// <param name="convertRow">A function that converts each test data item, along with the provided ArgsCode and optional test method name, to
    /// a row of type TRow. Cannot be null.</param>
    /// <param name="argsCode">The ArgsCode instance to pass to the conversion function. Cannot be null.</param>
    /// <param name="testMethodName">An optional name of the test method to provide to the conversion function. May be null.</param>
    /// <returns>An array containing the distinct _rows produced by applying the conversion function to each distinct test
    /// data item.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TRow[] ToDistinctArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        convertRow: testData => NotNull(convertRow, nameof(convertRow))(
            testData,
            argsCode.Defined(nameof(argsCode)),
            testMethodName));

    /// <summary>
    /// Converts a testDataCollection of test data to a distinct array of _rows using the specified conversion function and test method name.
    /// </summary>
    /// <typeparam name="TTestData">The type of test data in the testDataCollection. Must implement <see cref="ITestData"/> and be non-null.</typeparam>
    /// <typeparam name="TRow">The type of the resulting row elements.</typeparam>
    /// <param name="testDataCollection">The testDataCollection of test data to convert.</param>
    /// <param name="convertRow">The function to convert each test data item and test method name to a row.</param>
    /// <param name="testMethodName">The name of the test method, or <c>null</c>.</param>
    /// <returns>A distinct array of converted _rows.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TRow[] ToDistinctArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        convertRow: testData => NotNull(convertRow, nameof(convertRow))(
            testData,
            testMethodName));

    #endregion

    #endregion

    #endregion ToDistinctArray

    internal static (TTestData[] snapshot, int count) SnapshotWithCount<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    {
        var snapshot = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        var count = snapshot.Length;

        return (snapshot, count);
    }
}
