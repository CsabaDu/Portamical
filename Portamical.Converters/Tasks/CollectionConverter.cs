// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters.Tasks;

/// <summary>
/// Provides Task-based asynchronous extension methods for converting and deduplicating test data collections.
/// </summary>
/// <remarks>
/// <para>
/// This class offers Task-based async variants of the synchronous <see cref="Converters.CollectionConverter"/> methods,
/// enabling integration with asynchronous workflows and providing performance optimizations for
/// different collection sizes.
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong> Uses <see cref="NamedCase.Comparer"/> for semantic equality
/// based on test case names (via <see cref="INamedCase.TestCaseName"/>), not reference equality.
/// This ensures that test data with identical <c>TestCaseName</c> values are treated as duplicates,
/// with the first occurrence retained.
/// </para>
/// <para>
/// <strong>Performance Optimization:</strong> Task-returning methods employ a smart threshold strategy:
/// <list type="bullet">
///   <item><strong>Small collections (&lt; 10 items):</strong> Executes synchronously via <see cref="Task.FromResult{TResult}"/> to avoid Task.Run overhead (~5-20 µs)</item>
///   <item><strong>Larger collections (≥ 10 items):</strong> Offloads work to thread pool via <see cref="Task.Run{TResult}(Func{TResult})"/> to avoid blocking the caller</item>
/// </list>
/// This optimization provides 5-20x better performance for small collections common in unit test scenarios.
/// </para>
/// <para>
/// <strong>Return Type:</strong> All methods return <see cref="Task{TResult}"/> with arrays for compatibility 
/// with test frameworks (xUnit, NUnit, MSTest). For streaming scenarios, see <see cref="AsyncEnumerables.CollectionConverter"/>.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> All methods are stateless and thread-safe. However, the input
/// <paramref name="testDataCollection"/> should not be modified during enumeration.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Task-based approach for test framework compatibility
/// public static IEnumerable&lt;object[]&gt; GetTestData()
/// {
///     var testData = new[]
///     {
///         new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
///         new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate
///         new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
///     };
///     
///     var task = testData.ToDistinctArrayTask();
///     return task.Result;  // Blocks, but executed only once at discovery time
/// }
/// </code>
/// </example>
public static class CollectionConverter
{
    #region Task<TRow[]> ToDistinctArrayTask base method

    /// <summary>
    /// Asynchronously converts a collection of test data to a distinct array of rows.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Performance Optimization:</strong> For small collections (&lt; 10 items), the deduplication
    /// is performed synchronously to avoid Task.Run overhead (~5-20 µs). For larger collections,
    /// the work is offloaded to the thread pool to avoid blocking the calling thread.
    /// </para>
    /// <para>
    /// This optimization provides 5-20x better performance for very small test data collections,
    /// which are common in unit test scenarios.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.</typeparam>
    /// <typeparam name="TRow">The type of the output row elements produced by the conversion function.</typeparam>
    /// <param name="testDataCollection">The source collection of test data to convert. Cannot be null or empty.</param>
    /// <param name="convertRow">A function that transforms each test data item into a row of type <typeparamref name="TRow"/>. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the distinct array of converted rows.</returns>
    public static Task<TRow[]> ToDistinctArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        const int smallCollectionCountLimit = 10;

        var snapshot = NotNullOrEmpty(
            testDataCollection,
            nameof(testDataCollection));

        return snapshot.Length < smallCollectionCountLimit ?
            Task.FromResult(result: toDistinctRowArray())
            : Task.Run(function: toDistinctRowArray);

        #region Local function

        TRow[] toDistinctRowArray()
        => snapshot.ToDistinctArray(convertRow);

        #endregion
    }

    #endregion

    #region Wrapper methods

    #region Task<TTestData[]>

    /// <summary>
    /// Asynchronously creates an array containing distinct elements from the specified test data collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is an identity conversion that returns the test data items themselves after deduplication.
    /// </para>
    /// <para>
    /// <strong>Performance Optimization:</strong> For small collections (&lt; 10 items), the deduplication
    /// is performed synchronously to avoid Task.Run overhead (~5-20 µs). For larger collections,
    /// the work is offloaded to the thread pool to avoid blocking the calling thread.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of elements in the test data collection. Must implement <see cref="ITestData"/> and cannot be null.</typeparam>
    /// <param name="testDataCollection">The source collection of test data elements from which to create a distinct array. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array with distinct elements
    /// from the input collection, preserving the order of first occurrence.</returns>
    /// <example>
    /// <code>
    /// // Asynchronously deduplicate test data
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// var distinctTask = testData.ToDistinctArrayTask();
    /// var result = await distinctTask;
    /// // Result: 2 elements (duplicate removed based on TestCaseName)
    /// </code>
    /// </example>
    public static Task<TTestData[]> ToDistinctArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArrayTask(convertRow: testData => testData);

    #endregion

    #region Task<TRow[]>

    /// <summary>
    /// Asynchronously converts a collection of test data to a distinct array of rows using the specified conversion function and test method name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This overload allows passing a test method name to the conversion function, which can be useful for
    /// generating context-aware row data or including method information in test case output.
    /// </para>
    /// <para>
    /// <strong>Performance Optimization:</strong> For small collections (&lt; 10 items), the deduplication
    /// is performed synchronously to avoid Task.Run overhead. For larger collections, the work is offloaded
    /// to the thread pool.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.</typeparam>
    /// <typeparam name="TRow">The type of the resulting row elements.</typeparam>
    /// <param name="testDataCollection">The collection of test data to convert. Cannot be null or empty.</param>
    /// <param name="convertRow">The function to convert each test data item and test method name to a row. Cannot be null.</param>
    /// <param name="testMethodName">The name of the test method, or <c>null</c>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a distinct array of converted rows.</returns>
    public static Task<TRow[]> ToDistinctArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArrayTask(
        testData => convertRow(
            testData,
            testMethodName));

    /// <summary>
    /// Asynchronously converts a collection of test data items to a distinct array of rows using the specified
    /// conversion function with argument code and test method name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This overload provides the most flexibility by passing both an <see cref="ArgsCode"/> and a test method name
    /// to the conversion function, enabling rich context-aware row generation.
    /// </para>
    /// <para>
    /// The <paramref name="argsCode"/> parameter is validated (cannot be undefined) before being passed to the
    /// conversion function.
    /// </para>
    /// <para>
    /// <strong>Performance Optimization:</strong> For small collections (&lt; 10 items), the deduplication
    /// is performed synchronously to avoid Task.Run overhead. For larger collections, the work is offloaded
    /// to the thread pool.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of the input test data items. Must implement <see cref="ITestData"/> and cannot be null.</typeparam>
    /// <typeparam name="TRow">The type of the output row elements produced by the conversion function.</typeparam>
    /// <param name="testDataCollection">The collection of test data items to convert. Cannot be null or empty.</param>
    /// <param name="convertRow">A function that converts each test data item, along with the provided <see cref="ArgsCode"/> and optional test method name, to
    /// a row of type <typeparamref name="TRow"/>. Cannot be null.</param>
    /// <param name="argsCode">The <see cref="ArgsCode"/> instance to pass to the conversion function. Cannot be null or undefined.</param>
    /// <param name="testMethodName">An optional name of the test method to provide to the conversion function. May be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the distinct array of rows produced by applying the conversion function to each distinct test data item.</returns>
    public static Task<TRow[]> ToDistinctArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArrayTask(
        convertRow: testData => convertRow(
            testData,
            argsCode.Defined(nameof(argsCode)),
            testMethodName));

    #endregion

    #region Task<object?[][]>

    /// <summary>
    /// Asynchronously returns a jagged array of distinct argument arrays generated from the specified test data collection
    /// using the provided argument code.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each element in the returned array corresponds to the arguments produced by calling
    /// <see cref="ITestData.ToArgs(ArgsCode)"/> on each test data item with the specified argument code.
    /// Duplicates are removed based on test case name identity using <see cref="NamedCase.Comparer"/>.
    /// </para>
    /// <para>
    /// <strong>Performance Optimization:</strong> For small collections (&lt; 10 items), the deduplication
    /// is performed synchronously to avoid Task.Run overhead. For larger collections, the work is offloaded
    /// to the thread pool.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of the test data elements. Must implement <see cref="ITestData"/> and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data items from which to generate argument arrays. Cannot be null or empty.</param>
    /// <param name="argsCode">The argument code that determines how arguments are extracted from each test data item. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a jagged array of unique argument arrays produced from distinct test data items.</returns>
    public static Task<object?[][]> ToDistinctArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArrayTask(
        convertRow: testData =>  testData.ToArgs(argsCode));

    /// <summary>
    /// Asynchronously creates a jagged array of distinct argument arrays from the specified test data collection, using the
    /// provided argument and property codes to extract values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The returned array contains only distinct argument arrays, where uniqueness is determined by
    /// test case name identity using <see cref="NamedCase.Comparer"/>. The order of elements from the
    /// original collection is preserved (first occurrence wins).
    /// </para>
    /// <para>
    /// Each element is produced by calling <see cref="ITestData.ToArgs(ArgsCode, PropsCode)"/> on each
    /// test data item with the specified codes.
    /// </para>
    /// <para>
    /// <strong>Performance Optimization:</strong> For small collections (&lt; 10 items), the deduplication
    /// is performed synchronously to avoid Task.Run overhead. For larger collections, the work is offloaded
    /// to the thread pool.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of the test data elements. Must implement <see cref="ITestData"/> and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data items from which to generate argument arrays. Cannot be null or empty.</param>
    /// <param name="argsCode">The code specifying which arguments to extract from each test data item. Cannot be null.</param>
    /// <param name="propsCode">The code specifying which properties to extract from each test data item. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a jagged array of unique argument arrays extracted from distinct test data items.</returns>
    public static Task<object?[][]> ToDistinctArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArrayTask(
        convertRow: testData => testData.ToArgs(argsCode, propsCode));

    #endregion

    #endregion
}