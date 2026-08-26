// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters.AsyncEnumerables;

/// <summary>
/// Provides IAsyncEnumerable-based extension methods for converting and deduplicating test data collections.
/// </summary>
/// <remarks>
/// <para>
/// This class offers <see cref="IAsyncEnumerable{T}"/> variants of the synchronous <see cref="Converters.CollectionConverter"/> methods,
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
/// synchronous <see cref="Converters.CollectionConverter.ToDistinctArray{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, TRow})"/>
/// method, but the resulting elements are yielded asynchronously. This provides a bridge between synchronous
/// deduplication logic and asynchronous consumption patterns.
/// </para>
/// <para>
/// <strong>Return Type:</strong> All methods return <see cref="IAsyncEnumerable{T}"/> for streaming scenarios.
/// For Task-based approaches compatible with test frameworks, see <see cref="Tasks.CollectionConverter"/>.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> All methods are stateless and thread-safe. However, the input
/// <paramref name="testDataCollection"/> should not be modified during enumeration.
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
/// await foreach (var testCase in row.ToDistinctAsyncEnumerable())
/// {
///     await ProcessTestCaseAsync(testCase);
/// }
/// </code>
/// </example>
public static class CollectionConverter
{
    #region ToDAsyncEnumerable

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct _rows.
    /// </summary>
    /// <typeparam name="TTestData">The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.</typeparam>
    /// <typeparam name="TRow">The type of the output row elements produced by the conversion function.</typeparam>
    /// <param name="testDataCollection">The source collection of test data to convert. Cannot be null or empty.</param>
    /// <param name="convertRow">A function that transforms each test data item into a row of type <typeparamref name="TRow"/>. Cannot be null.</param>
    /// <returns>An asynchronous sequence that yields each distinct converted row once.</returns>
    public static IAsyncEnumerable<TRow> ToDAsyncEnumerable<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArray(convertRow).ToAsyncEnumerable();

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct elements.
    /// </summary>
    /// <remarks>
    /// This is an identity conversion that yields the test data items themselves after deduplication.
    /// The deduplication is performed synchronously using <see cref="Converters.CollectionConverter.ToDistinctArray{TTestData}(IEnumerable{TTestData})"/>,
    /// but the resulting elements are yielded asynchronously. This method is useful for integrating
    /// synchronous deduplicated data into asynchronous workflows or streaming scenarios.
    /// </remarks>
    /// <typeparam name="TTestData">The type of elements in the test data collection. Must implement <see cref="ITestData"/> and cannot be null.</typeparam>
    /// <param name="testDataCollection">The source collection of test data elements to convert. Cannot be null or empty.</param>
    /// <returns>An asynchronous sequence that yields each distinct element once, preserving the order of first occurrence.</returns>
    /// <example>
    /// <code>
    /// // Convert to async stream for consumption in async context
    /// var row = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// await foreach (var item in row.ToDistinctAsyncEnumerable())
    /// {
    ///     Console.WriteLine(item.TestCaseName);
    /// }
    /// // Output: "Add(2,3)", "Add(5,7)" (duplicate removed)
    /// </code>
    /// </example>
    public static IAsyncEnumerable<TTestData> ToDAsyncEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArray().ToAsyncEnumerable();

    #endregion

    #region ToDistinctAsyncEnumerable

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct _rows.
    /// </summary>
    /// <typeparam name="TTestData">The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.</typeparam>
    /// <typeparam name="TRow">The type of the output row elements produced by the conversion function.</typeparam>
    /// <param name="testDataCollection">The source collection of test data to convert. Cannot be null or empty.</param>
    /// <param name="convertRow">A function that transforms each test data item into a row of type <typeparamref name="TRow"/>. Cannot be null.</param>
    /// <returns>An asynchronous sequence that yields each distinct converted row once.</returns>
    public static IAsyncEnumerable<TRow> ToDistinctAsyncEnumerable<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(convertRow).ToAsyncEnumerable();

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct elements.
    /// </summary>
    /// <remarks>
    /// This is an identity conversion that yields the test data items themselves after deduplication.
    /// The deduplication is performed synchronously using <see cref="Converters.CollectionConverter.ToDistinctArray{TTestData}(IEnumerable{TTestData})"/>,
    /// but the resulting elements are yielded asynchronously. This method is useful for integrating
    /// synchronous deduplicated data into asynchronous workflows or streaming scenarios.
    /// </remarks>
    /// <typeparam name="TTestData">The type of elements in the test data collection. Must implement <see cref="ITestData"/> and cannot be null.</typeparam>
    /// <param name="testDataCollection">The source collection of test data elements to convert. Cannot be null or empty.</param>
    /// <returns>An asynchronous sequence that yields each distinct element once, preserving the order of first occurrence.</returns>
    /// <example>
    /// <code>
    /// // Convert to async stream for consumption in async context
    /// var row = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// await foreach (var item in row.ToDistinctAsyncEnumerable())
    /// {
    ///     Console.WriteLine(item.TestCaseName);
    /// }
    /// // Output: "Add(2,3)", "Add(5,7)" (duplicate removed)
    /// </code>
    /// </example>
    public static IAsyncEnumerable<TTestData> ToDistinctAsyncEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray().ToAsyncEnumerable();

    #endregion

    #region Helper method

    private static async IAsyncEnumerable<TRow> ToAsyncEnumerable<TRow>(this TRow[] converted)
    {
        foreach (var row in converted)
        {
            yield return row;
        }
    }

    #endregion
}