// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters.RowArrays.CustomRow;

/// <summary>
/// Provides extension methods for converting test data collections into arrays of custom row types,
/// using conversion functions that also receive contextual metadata such as <see cref="ArgsCode"/> and
/// the test method name, with optional deduplication based on test case identity.
/// </summary>
/// <remarks>
/// <para>
/// Unlike the simpler <c>ToRowArray(Func{TTestData, TRow})</c> overloads in
/// <see cref="RowArrays.CollectionConverter"/>, the methods in this class accept richer conversion
/// delegates that also take an <see cref="ArgsCode"/> and/or a test method name, letting callers produce
/// custom row types whose construction depends on this additional context.
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong> The <c>ToDistinctRowArray</c> overloads remove duplicates
/// based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>. Test data with
/// identical <c>TestCaseName</c> values are treated as duplicates, with the first occurrence retained.
/// </para>
/// </remarks>
public static class CollectionConverter
{
    #region ToRowArray

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
    public static TRow[] ToRowArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToRowArray(
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
    public static TRow[] ToRowArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToRowArray(
        convertRow: testData => NotNull(convertRow, nameof(convertRow))(
            testData,
            testMethodName));

    #endregion

    #region ToDistinctRowArray

    /// <summary>
    /// Converts a collection of test data into a distinct array of rows using a custom conversion function
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
    /// Called only for non-duplicate items.
    /// </param>
    /// <param name="argsCode">
    /// The argument code to pass to the conversion function. Cannot be undefined.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method, or <see langword="null"/> if not applicable.
    /// </param>
    /// <returns>
    /// An array containing the converted rows for distinct test data items, preserving the order
    /// of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty or <paramref name="argsCode"/> is undefined.
    /// </exception>
    /// <remarks>
    /// Deduplication is based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>.
    /// The order of elements from the original collection is preserved (first occurrence wins).
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TRow[] ToDistinctRowArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(
        convertRow: testData => NotNull(convertRow, nameof(convertRow))(
            testData,
            argsCode.Defined(nameof(argsCode)),
            testMethodName));

    /// <summary>
    /// Converts a collection of test data into a distinct array of rows using a custom conversion function
    /// with test method name parameter.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of the resulting row elements.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="convertRow">
    /// A function that converts each test data item and <paramref name="testMethodName"/> to a row of type
    /// <typeparamref name="TRow"/>. Cannot be null. Called only for non-duplicate items.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method, or <see langword="null"/> if not applicable.
    /// </param>
    /// <returns>
    /// An array containing the converted rows for distinct test data items, preserving the order
    /// of first occurrence.
    /// </returns>
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
    public static TRow[] ToDistinctRowArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(
        convertRow: testData => NotNull(convertRow, nameof(convertRow))(
            testData,
            testMethodName));

    #endregion
}
