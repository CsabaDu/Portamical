// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.CustomRow;

namespace Portamical.Converters.AsyncEnumerables.CustomRow;

/// <summary>
/// Provides extension methods for converting synchronous test data collections into
/// <see cref="IAsyncEnumerable{T}"/> sequences of custom row types, using conversion functions that also
/// receive contextual metadata such as <see cref="ArgsCode"/> and the test method name, with optional
/// deduplication based on test case identity.
/// </summary>
/// <remarks>
/// <para>
/// Conversion and deduplication are performed synchronously via <see cref="RowArrays.CustomRow.CollectionConverter"/>,
/// and the resulting array is then wrapped in an async enumerable using
/// <see cref="AsyncEnumerables.CollectionConverter.ToAsyncRowEnumerable{TRow}(TRow[])"/>.
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong> The <c>ToDistinctAsyncRowEnumerable</c> overloads remove
/// duplicates based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>.
/// Test data with identical <c>TestCaseName</c> values are treated as duplicates, with the first
/// occurrence retained.
/// </para>
/// </remarks>
public static class CollectionConverter
{
    #region ToRowArray

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of rows using a custom
    /// conversion function with argument code and test method name parameters.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of elements in the output sequence.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data to convert. Cannot be null or empty.
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
    /// An asynchronous sequence that yields each converted row, preserving the order from the input collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty or <paramref name="argsCode"/> is undefined.
    /// </exception>
    /// <remarks>
    /// Delegates to <see cref="RowArrays.CustomRow.CollectionConverter.ToRowArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, ArgsCode, string?, TRow}, ArgsCode, string?)"/>
    /// for conversion, then wraps the result in an async enumerable for streaming consumption.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<TRow> ToAsyncRowEnumerable<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToRowArray(convertRow, argsCode, testMethodName).ToAsyncRowEnumerable();

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of rows using a custom
    /// conversion function with test method name parameter.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of elements in the output sequence.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="convertRow">
    /// A function that converts each test data item and <paramref name="testMethodName"/> to a row of type
    /// <typeparamref name="TRow"/>. Cannot be null.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method, or <see langword="null"/> if not applicable.
    /// </param>
    /// <returns>
    /// An asynchronous sequence that yields each converted row, preserving the order from the input collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Delegates to <see cref="RowArrays.CustomRow.CollectionConverter.ToRowArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, string?, TRow}, string?)"/>
    /// for conversion, then wraps the result in an async enumerable for streaming consumption.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<TRow> ToAsyncRowEnumerable<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToRowArray(convertRow, testMethodName).ToAsyncRowEnumerable();

    #endregion

    #region ToDistinctRowArray

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct rows using a
    /// custom conversion function with argument code and test method name parameters.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of elements in the output sequence.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data to convert. Cannot be null or empty.
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
    /// An asynchronous sequence that yields each converted row for distinct test data items, preserving
    /// the order of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty or <paramref name="argsCode"/> is undefined.
    /// </exception>
    /// <remarks>
    /// Deduplication is based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>.
    /// Delegates to <see cref="RowArrays.CustomRow.CollectionConverter.ToDistinctRowArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, ArgsCode, string?, TRow}, ArgsCode, string?)"/>
    /// for deduplication, then wraps the result in an async enumerable.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<TRow> ToDistinctAsyncRowEnumerable<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(convertRow, argsCode, testMethodName).ToAsyncRowEnumerable();

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct rows using a
    /// custom conversion function with test method name parameter.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of elements in the output sequence.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="convertRow">
    /// A function that converts each test data item and <paramref name="testMethodName"/> to a row of type
    /// <typeparamref name="TRow"/>. Cannot be null. Called only for non-duplicate items.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method, or <see langword="null"/> if not applicable.
    /// </param>
    /// <returns>
    /// An asynchronous sequence that yields each converted row for distinct test data items, preserving
    /// the order of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Deduplication is based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>.
    /// Delegates to <see cref="RowArrays.CustomRow.CollectionConverter.ToDistinctRowArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, string?, TRow}, string?)"/>
    /// for deduplication, then wraps the result in an async enumerable.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<TRow> ToDistinctAsyncRowEnumerable<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(convertRow, testMethodName).ToAsyncRowEnumerable();

    #endregion
}
