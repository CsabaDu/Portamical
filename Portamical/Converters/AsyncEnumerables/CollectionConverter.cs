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
/// var testData = new[]
/// {
///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate
///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
/// };
/// 
/// await foreach (var testCase in testData.ToDistinctAsyncEnumerable())
/// {
///     await ProcessTestCaseAsync(testCase);
/// }
/// </code>
/// </example>
public static class CollectionConverter
{
    #region IAsyncEnumerable<TRow> ToDistinctAsyncEnumerable base method

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct rows.
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
    {
        var distinctRowArray = testDataCollection.ToDistinctArray(convertRow);

        return toAsyncEnumerable(distinctRowArray);

        #region Local function

        static async IAsyncEnumerable<TRow> toAsyncEnumerable(TRow[] rows)
        {
            foreach (var row in rows)
            {
                yield return row;
            }
        }

        #endregion
    }

    #endregion

    #region Wrapper methods

    #region IAsyncEnumerable<TTestData>

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
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// await foreach (var item in testData.ToDistinctAsyncEnumerable())
    /// {
    ///     Console.WriteLine(item.TestCaseName);
    /// }
    /// // Output: "Add(2,3)", "Add(5,7)" (duplicate removed)
    /// </code>
    /// </example>
    public static IAsyncEnumerable<TTestData> ToDistinctAsyncEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctAsyncEnumerable(
        convertRow: testData => testData);

    #endregion

    #region IAsyncEnumerable<TRow>

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct rows using the specified conversion function and test method name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This overload allows passing a test method name to the conversion function, enabling context-aware row generation.
    /// The deduplication is performed synchronously using the base conversion method, but the resulting elements
    /// are yielded asynchronously.
    /// </para>
    /// <para>
    /// This method is useful for integrating synchronous deduplicated data into asynchronous workflows or streaming
    /// scenarios where the test method context is needed.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.</typeparam>
    /// <typeparam name="TRow">The type of the resulting row elements.</typeparam>
    /// <param name="testDataCollection">The collection of test data to convert. Cannot be null or empty.</param>
    /// <param name="convertRow">The function to convert each test data item and test method name to a row. Cannot be null.</param>
    /// <param name="testMethodName">The name of the test method, or <c>null</c>.</param>
    /// <returns>An asynchronous sequence that yields each distinct converted row once, preserving the order of first occurrence.</returns>
    public static IAsyncEnumerable<TRow> ToDistinctAsyncEnumerable<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctAsyncEnumerable(
        convertRow: testData => NotNull(convertRow, nameof(convertRow))(
            testData,
            testMethodName));

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct rows using the specified
    /// conversion function with argument code and test method name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This overload provides the most flexibility by passing both an <see cref="ArgsCode"/> and a test method name
    /// to the conversion function, enabling rich context-aware row generation in asynchronous streaming scenarios.
    /// </para>
    /// <para>
    /// The deduplication is performed synchronously, but the resulting elements are yielded asynchronously.
    /// This method is useful for integrating synchronous deduplicated data into asynchronous workflows with
    /// full context information.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of the input test data items. Must implement <see cref="ITestData"/> and cannot be null.</typeparam>
    /// <typeparam name="TRow">The type of the output row elements produced by the conversion function.</typeparam>
    /// <param name="testDataCollection">The collection of test data items to convert. Cannot be null or empty.</param>
    /// <param name="convertRow">A function that converts each test data item, along with the provided <see cref="ArgsCode"/> and optional test method name, to
    /// a row of type <typeparamref name="TRow"/>. Cannot be null.</param>
    /// <param name="argsCode">The <see cref="ArgsCode"/> instance to pass to the conversion function. Cannot be null.</param>
    /// <param name="testMethodName">An optional name of the test method to provide to the conversion function. May be null.</param>
    /// <returns>An asynchronous sequence that yields each distinct converted row once, preserving the order of first occurrence.</returns>
    public static IAsyncEnumerable<TRow> ToDistinctAsyncEnumerable<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctAsyncEnumerable(
        convertRow: testData => NotNull(convertRow, nameof(convertRow))(
            testData,
            argsCode,
            testMethodName));

    #endregion

    #region IAsyncEnumerable<object?[]>

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct argument arrays
    /// using the provided argument code.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each yielded element corresponds to the arguments produced by calling
    /// <see cref="ITestData.ToArgs(ArgsCode)"/> on each test data item with the specified argument code.
    /// Duplicates are removed based on test case name identity using <see cref="NamedCase.Comparer"/>.
    /// </para>
    /// <para>
    /// The deduplication is performed synchronously, but the resulting argument arrays are yielded asynchronously.
    /// This method is useful for streaming test data arguments in asynchronous workflows.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of the test data elements. Must implement <see cref="ITestData"/> and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data items from which to generate argument arrays. Cannot be null or empty.</param>
    /// <param name="argsCode">The argument code that determines how arguments are extracted from each test data item. Cannot be null.</param>
    /// <returns>An asynchronous sequence that yields unique argument arrays produced from distinct test data items, preserving the order of first occurrence.</returns>
    public static IAsyncEnumerable<object?[]> ToDistinctAsyncEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctAsyncEnumerable(
        convertRow: testData => testData.ToArgs(argsCode));

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct argument arrays,
    /// using the provided argument and property codes to extract values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each yielded element is produced by calling <see cref="ITestData.ToArgs(ArgsCode, PropsCode)"/> on each
    /// test data item with the specified codes. Only distinct argument arrays are yielded, where uniqueness is
    /// determined by test case name identity using <see cref="NamedCase.Comparer"/>.
    /// </para>
    /// <para>
    /// The order of elements from the original collection is preserved (first occurrence wins).
    /// The deduplication is performed synchronously, but the resulting argument arrays are yielded asynchronously.
    /// </para>
    /// <para>
    /// This method is useful for streaming test data arguments with properties in asynchronous workflows.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of the test data elements. Must implement <see cref="ITestData"/> and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data items from which to generate argument arrays. Cannot be null or empty.</param>
    /// <param name="argsCode">The code specifying which arguments to extract from each test data item. Cannot be null.</param>
    /// <param name="propsCode">The code specifying which properties to extract from each test data item. Cannot be null.</param>
    /// <returns>An asynchronous sequence that yields unique argument arrays extracted from distinct test data items, preserving the order of first occurrence.</returns>
    public static IAsyncEnumerable<object?[]> ToDistinctAsyncEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctAsyncEnumerable(
        convertRow: testData => testData.ToArgs(argsCode, propsCode));

    #endregion

    #endregion
}