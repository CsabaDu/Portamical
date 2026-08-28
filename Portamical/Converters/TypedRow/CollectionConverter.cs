// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters.TypedRow;

public static class CollectionConverter
{
    #region ToArrayRow

    /// <summary>
    /// Converts a collection of test data into an array of rows using a custom conversion function
    /// with argument code and test method name parameters.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of elements in the output array.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="convertRow">
    /// A function that converts each test data item, along with <paramref name="argsCode"/> and
    /// <paramref name="testMethodName"/>, to a row of type <typeparamref name="TRow"/>. Cannot be null.
    /// </param>
    /// <param name="argsCode">
    /// The argument code to pass to the conversion function. Cannot be undefined.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method, or <see langword="null"/> if not applicable.
    /// </param>
    /// <returns>
    /// An array containing the converted rows.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty or <paramref name="argsCode"/> is undefined.
    /// </exception>
    /// <remarks>
    /// This overload is useful when the conversion function requires both configuration (<paramref name="argsCode"/>)
    /// and metadata (<paramref name="testMethodName"/>).
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TRow[] ToArrayRow<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArrayRow(
        convertRow: testData => NotNull(convertRow, nameof(convertRow))(
            testData,
            argsCode.Defined(nameof(argsCode)),
            testMethodName));

    /// <summary>
    /// Converts a collection of test data into an array of rows using a custom conversion function
    /// with test method name parameter.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of elements in the output array.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="convertRow">
    /// A function that converts each test data item and <paramref name="testMethodName"/> to a row
    /// of type <typeparamref name="TRow"/>. Cannot be null.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method, or <see langword="null"/> if not applicable.
    /// </param>
    /// <returns>
    /// An array containing the converted rows.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// This overload is useful when the conversion function needs test method metadata but not argument code configuration.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TRow[] ToArrayRow<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArrayRow(
        convertRow: testData => NotNull(convertRow, nameof(convertRow))(
            testData,
            testMethodName));

    #endregion

    #region ToDistinctArrayRow

    /// <summary>
    /// Converts a collection of test data items to a distinct array of rows using the specified
    /// conversion function.
    /// </summary>
    /// <remarks>The resulting array contains only unique rows based on test case name identity
    /// using <see cref="NamedCase.Comparer"/>. The order of elements from the original collection is preserved.</remarks>
    /// <typeparam name="TTestData">The type of the input test data items. Must implement the ITestData interface and cannot be null.</typeparam>
    /// <typeparam name="TRow">The type of the output row elements produced by the conversion function.</typeparam>
    /// <param name="testDataCollection">The collection of test data items to convert. Cannot be null.</param>
    /// <param name="convertRow">A function that converts each test data item, along with the provided ArgsCode and optional test method name, to
    /// a row of type TRow. Cannot be null.</param>
    /// <param name="argsCode">The ArgsCode instance to pass to the conversion function. Cannot be undefined.</param>
    /// <param name="testMethodName">An optional name of the test method to provide to the conversion function. May be null.</param>
    /// <returns>An array containing the distinct rows produced by applying the conversion function to each distinct test
    /// data item.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty or <paramref name="argsCode"/> is undefined.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TRow[] ToDistinctArrayRow<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArrayRow(
        convertRow: testData => NotNull(convertRow, nameof(convertRow))(
            testData,
            argsCode.Defined(nameof(argsCode)),
            testMethodName));

    /// <summary>
    /// Converts a collection of test data to a distinct array of rows using the specified conversion function and test method name.
    /// </summary>
    /// <typeparam name="TTestData">The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.</typeparam>
    /// <typeparam name="TRow">The type of the resulting row elements.</typeparam>
    /// <param name="testDataCollection">The collection of test data to convert. Cannot be null.</param>
    /// <param name="convertRow">The function to convert each test data item and test method name to a row. Cannot be null.</param>
    /// <param name="testMethodName">The name of the test method, or <see langword="null"/>.</param>
    /// <returns>A distinct array of converted rows.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Deduplication is based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>.
    /// The order of elements from the original collection is preserved (first occurrence wins).
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TRow[] ToDistinctArrayRow<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArrayRow(
        convertRow: testData => NotNull(convertRow, nameof(convertRow))(
            testData,
            testMethodName));

    #endregion
}
