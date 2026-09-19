// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays;
using Portamical.Converters.RowArrays.TestData;

namespace Portamical.Converters.AsyncEnumerables;

/// <summary>
/// Provides IAsyncEnumerable-based extension methods for converting and deduplicating test data collections.
/// </summary>
/// <remarks>
/// <para>
/// This class offers <see cref="IAsyncEnumerable{T}"/> variants of the synchronous <see cref="RowArrays.CollectionConverter"/> methods,
/// enabling integration with streaming scenarios and async iteration patterns (<c>await foreach</c>).
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong> Uses <see cref="NamedCase.Comparer"/> for semantic equality
/// based on test case names (via <see cref="INamedCase.TestCaseName"/>), not reference equality.
/// This ensures that test data with identical <c>TestCaseName</c> values are treated as duplicates,
/// with the first occurrence retained.
/// </para>
/// <para>
/// <strong>Implementation Note:</strong> The deduplication is performed synchronously using the underlying
/// synchronous <see cref="RowArrays.CollectionConverter.ToDistinctRowArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, TRow})"/>
/// method, but the resulting elements are yielded asynchronously. This provides a bridge between synchronous
/// deduplication logic and asynchronous consumption patterns.
/// </para>
/// <para>
/// <strong>Return Type:</strong> All methods return <see cref="IAsyncEnumerable{T}"/> for streaming scenarios.
/// For Task-based approaches compatible with test frameworks, see <see cref="Task.CollectionConverter"/>.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> All methods are stateless and thread-safe. However, input
/// collections should not be modified during enumeration.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Async enumerable for streaming scenarios
/// var row = new[]
/// {
///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate
///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
/// };
/// 
/// await foreach (var testCase in row.ToDistinctAsyncRowEnumerable())
/// {
///     await ProcessTestCaseAsync(testCase);
/// }
/// </code>
/// </example>
public static class CollectionConverter
{
    #region ToAsyncEnumerable

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of rows.
    /// </summary>
    /// <typeparam name="TTestData">The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.</typeparam>
    /// <typeparam name="TRow">The type of the output row elements produced by the conversion function.</typeparam>
    /// <param name="testDataCollection">The source collection of test data to convert. Cannot be null or empty.</param>
    /// <param name="convertRow">A function that transforms each test data item into a row of type <typeparamref name="TRow"/>. Cannot be null.</param>
    /// <returns>An asynchronous sequence that yields each convertedRows row.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Delegates to <see cref="RowArrays.CollectionConverter.ToRowArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, TRow})"/>
    /// for conversion, then wraps the result in an async enumerable for streaming consumption.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<TRow> ToAsyncRowEnumerable<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    => testDataCollection.ToRowArray(convertRow).ToAsyncRowEnumerable();

    #endregion

    #region ToDistinctAsyncEnumerable

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct rows.
    /// Removes duplicates based on <see cref="INamedCase.TestCaseName"/> identity.
    /// </summary>
    /// <typeparam name="TTestData">The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.</typeparam>
    /// <typeparam name="TRow">The type of the output row elements produced by the conversion function.</typeparam>
    /// <param name="testDataCollection">The source collection of test data to convert. Cannot be null or empty.</param>
    /// <param name="convertRow">A function that transforms each test data item into a row of type <typeparamref name="TRow"/>. Cannot be null. Called only for non-duplicate items.</param>
    /// <returns>An asynchronous sequence that yields each distinct convertedRows row once.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Deduplication uses <see cref="NamedCase.Comparer"/> based on <see cref="INamedCase.TestCaseName"/>.
    /// Test data with identical <c>TestCaseName</c> values are considered duplicates; only the first occurrence is retained.
    /// </para>
    /// <para>
    /// Delegates to <see cref="RowArrays.CollectionConverter.ToDistinctRowArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, TRow})"/>
    /// for deduplication, then wraps the result in an async enumerable.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<TRow> ToDistinctAsyncRowEnumerable<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(convertRow).ToAsyncRowEnumerable();

    #endregion

    #region Helper method

    /// <summary>
    /// Internal helper that wraps an array in an <see cref="IAsyncEnumerable{T}"/> for asynchronous iteration.
    /// </summary>
    /// <typeparam name="TRow">
    /// The type of elements in the array.
    /// </typeparam>
    /// <param name="convertedRows">
    /// The source array to wrap. Cannot be null.
    /// </param>
    /// <returns>
    /// An async enumerable that yields each element from the array.
    /// </returns>
    /// <remarks>
    /// This method enables <c>await foreach</c> consumption of synchronously-processed arrays.
    /// The <c>async</c> modifier is required for the iterator pattern, even though no actual
    /// asynchronous I/O occurs.
    /// </remarks>
    internal static async IAsyncEnumerable<TRow> ToAsyncRowEnumerable<TRow>(this TRow[] convertedRows)
    {
        foreach (var row in convertedRows)
        {
            yield return row;
        }
    }

    #endregion
}