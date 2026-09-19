// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.CustomRow;

namespace Portamical.Converters.Tasks.ArrayTasks.CustomRow;

/// <summary>
/// Provides extension methods for asynchronously converting test data collections into arrays of custom row
/// types, using conversion functions that also receive contextual metadata such as <see cref="ArgsCode"/> and
/// the test method name, with optional deduplication based on test case identity.
/// </summary>
/// <remarks>
/// <para>
/// These methods apply the smart threshold-based optimization implemented by
/// <see cref="Tasks.CollectionConverter.ToConvertedRowsTask{TTestData, TConvertedRows}(IEnumerable{TTestData}, Func{IEnumerable{TTestData}, TConvertedRows})"/>,
/// choosing between synchronous and thread-pool execution based on collection size (threshold: 100 items),
/// while delegating the actual row construction to <see cref="RowArrays.CustomRow.CollectionConverter"/>.
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
    /// Asynchronously converts a collection of test data into an array of rows using a custom conversion
    /// function with test method name parameter.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of elements in the output array.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="convertRow">
    /// A function that converts each test data item and <paramref name="testMethodName"/> to a row of type
    /// <typeparamref name="TRow"/>. Cannot be null.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method, or <see langword="null"/> if not applicable.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing an array of the converted rows.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Uses smart threshold optimization (see class remarks for details). Delegates to
    /// <see cref="RowArrays.CustomRow.CollectionConverter.ToRowArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, string?, TRow}, string?)"/>.
    /// </remarks>
    public static Task<TRow[]> ToArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToRowArray(convertRow, testMethodName));

    /// <summary>
    /// Asynchronously converts a collection of test data into an array of rows using a custom conversion
    /// function with argument code and test method name parameters.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
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
    /// A <see cref="Task{TResult}"/> containing an array of the converted rows.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty or <paramref name="argsCode"/> is undefined.
    /// </exception>
    /// <remarks>
    /// Uses smart threshold optimization (see class remarks for details). Delegates to
    /// <see cref="RowArrays.CustomRow.CollectionConverter.ToRowArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, ArgsCode, string?, TRow}, ArgsCode, string?)"/>.
    /// </remarks>
    public static Task<TRow[]> ToArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToRowArray(convertRow, argsCode, testMethodName));

    #endregion

    #region ToDistinctRowArray

    /// <summary>
    /// Asynchronously converts a collection of test data into a distinct array of rows using a custom
    /// conversion function with test method name parameter.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of elements in the output array.
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
    /// A <see cref="Task{TResult}"/> containing an array of the converted rows for distinct test data items,
    /// preserving the order of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Deduplication is based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>.
    /// Uses smart threshold optimization (see class remarks for details). Delegates to
    /// <see cref="RowArrays.CustomRow.CollectionConverter.ToDistinctRowArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, string?, TRow}, string?)"/>.
    /// </remarks>
    public static Task<TRow[]> ToDistinctArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToDistinctRowArray(convertRow, testMethodName));

    /// <summary>
    /// Asynchronously converts a collection of test data into a distinct array of rows using a custom
    /// conversion function with argument code and test method name parameters.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
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
    /// A <see cref="Task{TResult}"/> containing an array of the converted rows for distinct test data items,
    /// preserving the order of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty or <paramref name="argsCode"/> is undefined.
    /// </exception>
    /// <remarks>
    /// Deduplication is based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>.
    /// Uses smart threshold optimization (see class remarks for details). Delegates to
    /// <see cref="RowArrays.CustomRow.CollectionConverter.ToDistinctRowArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, ArgsCode, string?, TRow}, ArgsCode, string?)"/>.
    /// </remarks>
    public static Task<TRow[]> ToDistinctArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToDistinctRowArray(convertRow, argsCode, testMethodName));

    #endregion
}
