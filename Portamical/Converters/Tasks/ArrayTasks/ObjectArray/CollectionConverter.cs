// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.ObjectArray;

namespace Portamical.Converters.Tasks.ArrayTasks.ObjectArray;

/// <summary>
/// Provides extension methods for asynchronously converting test data collections into jagged <c>object?[][]</c>
/// arrays of test method arguments, with optional deduplication based on test case identity.
/// </summary>
/// <remarks>
/// <para>
/// These methods apply the smart threshold-based optimization implemented by
/// <see cref="Tasks.CollectionConverter.ToConvertedRowsTask{TTestData, TConvertedRows}(IEnumerable{TTestData}, Func{IEnumerable{TTestData}, TConvertedRows})"/>,
/// choosing between synchronous and thread-pool execution based on collection size (threshold: 100 items),
/// while delegating the actual row construction to <see cref="RowArrays.ObjectArray.CollectionConverter"/>.
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong> The <c>ToDistinctArrayTask</c> overloads remove duplicates
/// based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>. Test data with
/// identical <c>TestCaseName</c> values are treated as duplicates, with the first occurrence retained.
/// </para>
/// </remarks>
public static class CollectionConverter
{
    #region ToRowArray

    /// <summary>
    /// Asynchronously converts a collection of test data into a jagged array of argument arrays using the
    /// specified argument code.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining how arguments are extracted from each test data item.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing a jagged array where each element is an <c>object?[]</c>
    /// with arguments for one test data item.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Uses smart threshold optimization (see class remarks for details). Delegates to
    /// <see cref="RowArrays.ObjectArray.CollectionConverter.ToRowArray{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>.
    /// </remarks>
    public static Task<object?[][]> ToArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToRowArray(argsCode));

    /// <summary>
    /// Asynchronously converts a collection of test data into a jagged array of argument arrays using the
    /// specified argument and properties codes.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining the primary conversion strategy.
    /// </param>
    /// <param name="propsCode">
    /// The properties code determining which properties to include when flattening.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing a jagged array where each element is an <c>object?[]</c>
    /// with arguments extracted according to the specified codes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Uses smart threshold optimization (see class remarks for details). Delegates to
    /// <see cref="RowArrays.ObjectArray.CollectionConverter.ToRowArray{TTestData}(IEnumerable{TTestData}, ArgsCode, PropsCode)"/>.
    /// </remarks>
    public static Task<object?[][]> ToArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToRowArray(argsCode, propsCode));

    #endregion

    #region ToDistinctRowArray

    /// <summary>
    /// Asynchronously converts a collection of test data into a distinct jagged array of argument arrays
    /// using the specified argument code.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining how arguments are extracted from each test data item.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing a jagged array of unique <c>object?[]</c> argument arrays
    /// produced from distinct test data items, preserving the order of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Deduplication is based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>.
    /// Uses smart threshold optimization (see class remarks for details). Delegates to
    /// <see cref="RowArrays.ObjectArray.CollectionConverter.ToDistinctRowArray{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>.
    /// </remarks>
    public static Task<object?[][]> ToDistinctArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToDistinctRowArray(argsCode));

    /// <summary>
    /// Asynchronously converts a collection of test data into a distinct jagged array of argument arrays
    /// using the specified argument and properties codes.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining the primary conversion strategy.
    /// </param>
    /// <param name="propsCode">
    /// The properties code determining which properties to include when flattening.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing a jagged array of unique <c>object?[]</c> argument arrays
    /// extracted from distinct test data items, preserving the order of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Deduplication is based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>.
    /// Uses smart threshold optimization (see class remarks for details). Delegates to
    /// <see cref="RowArrays.ObjectArray.CollectionConverter.ToDistinctRowArray{TTestData}(IEnumerable{TTestData}, ArgsCode, PropsCode)"/>.
    /// </remarks>
    public static Task<object?[][]> ToDistinctArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToDistinctRowArray(argsCode, propsCode));

    #endregion
}