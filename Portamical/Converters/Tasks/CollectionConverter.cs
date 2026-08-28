// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters.Tasks;

/// <summary>
/// Provides core helper methods for converting test data collections, including asynchronous conversion
/// with threshold-based optimization and collection snapshotting utilities.
/// </summary>
/// <remarks>
/// <para>
/// This class contains foundational helpers used by specialized converter classes in the
/// <see cref="Converters"/> namespace hierarchy. For row array conversion methods,
/// see <see cref="RowArrays.CollectionConverter"/>.
/// </para>
/// <para>
/// <strong>Async Conversion:</strong> The <see cref="ToConvertedRowsTask{TTestData, TConvertedRows}"/>
/// method applies smart threshold-based optimization, choosing between synchronous and asynchronous
/// execution based on collection size (threshold: 100 items).
/// </para>
/// <para>
/// <strong>Snapshotting:</strong> The <see cref="SnapshotWithCount{TTestData}"/> helper validates
/// and snapshots collections while providing efficient count access, used throughout the converter infrastructure.
/// </para>
/// </remarks>
public static class CollectionConverter
{
    #region ToConvertedRowsTask

    /// <summary>
    /// Core helper method that applies smart threshold-based optimization to convert a collection asynchronously,
    /// choosing between synchronous and thread-pool execution based on collection size.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TConvertedRows">
    /// The type of the conversion result.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="convertRows">
    /// A function that transforms the collection snapshot into the desired result type.
    /// Cannot be null.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing the conversion result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRows"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Smart Threshold Strategy:</strong> This method snapshots the collection and evaluates its size:
    /// <list type="bullet">
    ///   <item><strong>&lt; 100 items:</strong> Executes synchronously via <see cref="Task.FromResult{TResult}"/> (avoids Task.Run overhead)</item>
    ///   <item><strong>≥ 100 items:</strong> Offloads to thread pool via <see cref="Task.Run{TResult}(Func{TResult})"/> (parallel execution benefit)</item>
    /// </list>
    /// </para>
    /// <para>
    /// The threshold of 100 items is empirically derived from BenchmarkDotNet measurements showing this as the
    /// break-even point where Task.Run benefits outweigh its overhead.
    /// </para>
    /// <para>
    /// Uses <see cref="SnapshotWithCount{TTestData}(IEnumerable{TTestData})"/> to validate and snapshot the collection
    /// before applying the conversion function.
    /// </para>
    /// </remarks>
    public static Task<TConvertedRows> ToConvertedRowsTask<TTestData, TConvertedRows>(
        this IEnumerable<TTestData> testDataCollection,
        Func<IEnumerable<TTestData>, TConvertedRows> convertRows)
    where TTestData : notnull, ITestData
    {
        const int smallCollectionCountLimit = 100;

        var (snapshot, count) =
            Converters.CollectionConverter.SnapshotWithCount(testDataCollection);

        return count < smallCollectionCountLimit ?
            Task.FromResult(result: convertRows(snapshot))
            : Task.Run(function: () => convertRows(snapshot));
    }

    #endregion
}
