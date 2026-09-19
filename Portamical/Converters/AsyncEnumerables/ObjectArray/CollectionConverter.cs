// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.ObjectArray;

namespace Portamical.Converters.AsyncEnumerables.ObjectArray;

/// <summary>
/// Provides extension methods for converting synchronous test data collections into
/// <see cref="IAsyncEnumerable{T}"/> sequences of <c>object?[]</c> argument arrays, with optional
/// deduplication based on test case identity.
/// </summary>
/// <remarks>
/// <para>
/// Conversion and deduplication are performed synchronously via
/// <see cref="RowArrays.ObjectArray.CollectionConverter"/>, and the resulting array is then wrapped
/// in an async enumerable using <see cref="AsyncEnumerables.CollectionConverter.ToAsyncRowEnumerable{TRow}(TRow[])"/>.
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
    /// Converts a synchronous test data collection to an asynchronous sequence of argument arrays
    /// using the specified argument code.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining how arguments are extracted from each test data item.
    /// </param>
    /// <returns>
    /// An asynchronous sequence that yields an <c>object?[]</c> for each test data item, preserving the
    /// order from the input collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Delegates to <see cref="RowArrays.ObjectArray.CollectionConverter.ToRowArray{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
    /// for conversion, then wraps the result in an async enumerable for streaming consumption.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<object?[]> ToAsyncRowEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToRowArray(argsCode).ToAsyncRowEnumerable();

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of argument arrays
    /// using the specified argument and properties codes.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining the primary conversion strategy.
    /// </param>
    /// <param name="propsCode">
    /// The properties code determining which properties to include when flattening.
    /// </param>
    /// <returns>
    /// An asynchronous sequence that yields an <c>object?[]</c> for each test data item, preserving the
    /// order from the input collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Delegates to <see cref="RowArrays.ObjectArray.CollectionConverter.ToRowArray{TTestData}(IEnumerable{TTestData}, ArgsCode, PropsCode)"/>
    /// for conversion, then wraps the result in an async enumerable for streaming consumption.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<object?[]> ToAsyncRowEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToRowArray(argsCode, propsCode).ToAsyncRowEnumerable();

    #endregion

    #region ToDistinctRowArray

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct argument arrays
    /// using the specified argument code.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining how arguments are extracted from each test data item.
    /// </param>
    /// <returns>
    /// An asynchronous sequence that yields an <c>object?[]</c> for each distinct test data item,
    /// preserving the order of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Deduplication is based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>.
    /// Delegates to <see cref="RowArrays.ObjectArray.CollectionConverter.ToDistinctRowArray{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
    /// for deduplication, then wraps the result in an async enumerable.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<object?[]> ToDistinctAsyncRowEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(argsCode).ToAsyncRowEnumerable();

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct argument arrays
    /// using the specified argument and properties codes.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining the primary conversion strategy.
    /// </param>
    /// <param name="propsCode">
    /// The properties code determining which properties to include when flattening.
    /// </param>
    /// <returns>
    /// An asynchronous sequence that yields an <c>object?[]</c> for each distinct test data item,
    /// preserving the order of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Deduplication is based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>.
    /// Delegates to <see cref="RowArrays.ObjectArray.CollectionConverter.ToDistinctRowArray{TTestData}(IEnumerable{TTestData}, ArgsCode, PropsCode)"/>
    /// for deduplication, then wraps the result in an async enumerable.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<object?[]> ToDistinctAsyncRowEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(argsCode, propsCode).ToAsyncRowEnumerable();

    #endregion
}
