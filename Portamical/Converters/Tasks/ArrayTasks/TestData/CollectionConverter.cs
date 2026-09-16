// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.TestData;

namespace Portamical.Converters.Tasks.ArrayTasks.TestData;

public static class CollectionConverter
{
    #region ToRowArray

    /// <summary>
    /// Asynchronously converts a collection of test data into an array, preserving the test data items as-is (identity conversion).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing an array of <typeparamref name="TTestData"/> items.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is an identity conversion that returns test data items without transformation.
    /// Uses smart threshold optimization (see class remarks for details).
    /// </para>
    /// <para>
    /// Delegates to <see cref="CollectionConverter.ToRowArray{TTestData}(IEnumerable{TTestData})"/>.
    /// </para>
    /// </remarks>
    public static Task<TTestData[]> ToArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToRowArray());

    #endregion

    #region ToDistinctRowArray

    /// <summary>
    /// Asynchronously converts a collection of test data into a distinct array, removing duplicates based on
    /// <see cref="INamedCase.TestCaseName"/> identity (identity conversion).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing an array of distinct <typeparamref name="TTestData"/> items,
    /// preserving the order of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is an identity conversion that deduplicates test data items without transformation.
    /// Deduplication uses <see cref="NamedCase.Comparer"/> based on <see cref="INamedCase.TestCaseName"/>.
    /// </para>
    /// <para>
    /// Uses smart threshold optimization (see class remarks for details).
    /// Delegates to <see cref="CollectionConverter.ToDistinctRowArray{TTestData}(IEnumerable{TTestData})"/>.
    /// </para>
    /// </remarks>
    public static Task<TTestData[]> ToDistinctArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToDistinctRowArray());

    #endregion
}