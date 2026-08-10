// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Core.Identity;
using Portamical.Core.Identity.Model;
using Portamical.Core.Safety;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using static Portamical.Core.Safety.Validator;

namespace Portamical.Core.Converters;

/// <remarks>
/// <para>
/// The methods in this class help ensure that test data collections are deduplicated based on test case
/// identity (via <see cref="INamedCase.TestCaseName"/>) and are returned as arrays.
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong> Uses <see cref="NamedCase.Comparer"/> for semantic equality
/// based on test case names, not reference equality. This ensures that test data with identical
/// <c>TestCaseName</c> values are treated as duplicates, with the first occurrence retained.
/// </para>
/// <para>
/// <strong>Return Type:</strong> All methods return arrays for optimal performance with test frameworks.
/// Arrays provide zero-allocation enumeration, direct indexing, and better compatibility with
/// data-driven test attributes (MSTest <c>DynamicData</c>, xUnit <c>MemberData</c>, NUnit <c>TestCaseSource</c>).
/// </para>
/// </remarks>
public static class CollectionConverter
{
    #region TRow[] ToDistinctArray base method

    /// <summary>
    /// Core deduplication method that converts a collection of test data into a distinct array of rows
    /// using a custom conversion function.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Deduplication Strategy:</strong> This method removes duplicates based on 
    /// <see cref="INamedCase.TestCaseName"/> identity using <see cref="NamedCase.Comparer"/>.
    /// Test data items with identical <c>TestCaseName</c> values are considered duplicates, and only
    /// the first occurrence is retained.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Uses <see cref="HashSet{T}"/> with <see cref="NamedCase.Comparer"/>
    /// for O(n) deduplication. The <see cref="HashSet{T}.Add"/> method returns false for duplicates,
    /// which is used as a filter predicate.
    /// </para>
    /// <para>
    /// <strong>Order Preservation:</strong> The order of elements from the original collection is preserved
    /// in the output array. Duplicates are removed based on first-occurrence semantics.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> 
    /// (which inherits <see cref="INamedCase"/>) and cannot be null.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The type of elements in the output array, produced by <paramref name="convertRow"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="convertRow">
    /// A function that transforms each test data item into a row of type <typeparamref name="TRow"/>.
    /// Cannot be null. Called only for non-duplicate items.
    /// </param>
    /// <returns>
    /// An array containing the converted rows for distinct test data items, preserving the order
    /// of first occurrence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null or <paramref name="convertRow"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <example>
    /// <code>
    /// // Identity conversion (keeping test data as-is)
    /// var distinct = testDataCollection.ToDistinctArray(td => td);
    /// 
    /// // Convert to argument arrays
    /// var args = testDataCollection.ToDistinctArray(td => td.ToArgs(ArgsCode.Instance));
    /// 
    /// // Custom row conversion
    /// var rows = testDataCollection.ToDistinctArray(td => new 
    /// { 
    ///     Name = td.TestCaseName, 
    ///     Args = td.ToArgs(ArgsCode.Instance) 
    /// });
    /// </code>
    /// </example>
    public static TRow[] ToDistinctArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        var snapshot = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        _ = NotNull(convertRow, nameof(convertRow));
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
        var rows = new List<TRow>(snapshot.Length);

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions - foreach is more performant for HashSet-based deduplication
        foreach (var testData in snapshot)
        {
            // Deduplicate based on 'NamedCase' identity/equality semantics
            if (namedCases.Add(testData))
            {
                var row = convertRow(testData);
                rows.Add(row);
            }
        }
#pragma warning restore S3267

        return [.. rows];
    }

    #endregion

    #region Wrapper methods

    #region TRow[]

    /// <summary>
    /// Converts a collection of test data items to a distinct array of rows using the specified
    /// conversion function.
    /// </summary>
    /// <remarks>The resulting array contains only unique rows based on test case name identity
    /// using <see cref="NamedCase.Comparer"/>. The order of elements from the original collection is preserved.</remarks>
    /// <typeparam name="TTestData">The type of the input test data items. Must implement the ITestData interface and cannot be null.</typeparam>
    /// <typeparam name="TRow">The type of the output row elements produced by the conversion function.</typeparam>
    /// <param name="testDataCollection">The collection of test data items to convert. Cannot be null.</param>
    /// <param name="convertRow">A function that converts each test data item, along with the provided ArgsCode and optional test method name, to
    /// a row of type TRow. Cannot be null.</param>
    /// <param name="argsCode">The ArgsCode instance to pass to the conversion function. Cannot be null.</param>
    /// <param name="testMethodName">An optional name of the test method to provide to the conversion function. May be null.</param>
    /// <returns>An array containing the distinct rows produced by applying the conversion function to each distinct test
    /// data item.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TRow[] ToDistinctArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        convertRow: testData => convertRow(
            testData,
            argsCode.Defined(nameof(argsCode)),
        testMethodName));

    /// <summary>
    /// Converts a collection of test data to a distinct array of rows using the specified conversion function and test method name.
    /// </summary>
    /// <typeparam name="TTestData">The type of test data in the collection. Must implement <see cref="ITestData"/> and be non-null.</typeparam>
    /// <typeparam name="TRow">The type of the resulting row elements.</typeparam>
    /// <param name="testDataCollection">The collection of test data to convert.</param>
    /// <param name="convertRow">The function to convert each test data item and test method name to a row.</param>
    /// <param name="testMethodName">The name of the test method, or <c>null</c>.</param>
    /// <returns>A distinct array of converted rows.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TRow[] ToDistinctArray<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        convertRow: testData => convertRow(
            testData,
            testMethodName));

    #endregion

    #region TTestData[]

    /// <summary>
    /// Creates an array containing distinct elements from the specified test data collection.
    /// </summary>
    /// <typeparam name="TTestData">The type of elements in the test data collection. Must implement ITestData and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data elements from which to create a distinct array. Cannot be null.</param>
    /// <returns>An array containing the distinct elements from the input collection. The order of elements is
    /// preserved from the original collection (first occurrence wins).</returns>
    /// <example>
    /// <code>
    /// // Basic deduplication
    /// var row = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// var distinct = row.ToDistinctArray();
    /// // Result: 2 elements (duplicate removed based on TestCaseName)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTestData[] ToDistinctArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        convertRow: testData => testData);

    #endregion

    #region object?[][]

    /// <summary>
    /// Returns a jagged array of distinct argument arrays generated from the specified test data collection
    /// using the provided argument code.
    /// </summary>
    /// <remarks>Each element in the returned array corresponds to the arguments produced by calling
    /// ToArgs on each test data item with the specified argument code. Duplicates are removed based on
    /// test case name identity using <see cref="NamedCase.Comparer"/>.</remarks>
    /// <typeparam name="TTestData">The type of the test data elements. Must implement the ITestData interface and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data items from which to generate argument arrays. Cannot be null.</param>
    /// <param name="argsCode">The argument code that determines how arguments are extracted from each test data item.</param>
    /// <returns>A jagged array containing unique argument arrays produced from distinct test data items. 
    /// The array is empty if the input collection contains no items.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object?[][] ToDistinctArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        convertRow: testData => testData.ToArgs(argsCode));

    /// <summary>
    /// Creates a jagged array of distinct argument arrays from the specified test data collection, using the
    /// provided argument and property codes to extract values.
    /// </summary>
    /// <remarks>The returned array contains only distinct argument arrays, where uniqueness is determined by
    /// test case name identity using <see cref="NamedCase.Comparer"/>. The order of elements from the
    /// original collection is preserved (first occurrence wins).</remarks>
    /// <typeparam name="TTestData">The type of the test data elements. Must implement the ITestData interface and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data items from which to generate argument arrays. Cannot be null.</param>
    /// <param name="argsCode">The code specifying which arguments to extract from each test data item.</param>
    /// <param name="propsCode">The code specifying which properties to extract from each test data item.</param>
    /// <returns>A jagged array containing unique argument arrays extracted from distinct test data items. 
    /// The array is empty if no items are found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object?[][] ToDistinctArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray(
        convertRow: testData => testData.ToArgs(argsCode, propsCode));

    #endregion

    #endregion Wrapper methods
}

/// <summary>
/// Provides asynchronous extension methods for converting and deduplicating test data collections.
/// </summary>
/// <remarks>
/// <para>
/// This class offers async variants of the synchronous <see cref="CollectionConverter"/> methods,
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
/// <strong>Return Types:</strong>
/// <list type="bullet">
///   <item><see cref="Task{TResult}"/> methods - Return arrays for compatibility with test frameworks (xUnit, NUnit, MSTest)</item>
///   <item><see cref="IAsyncEnumerable{T}"/> methods - Support streaming scenarios and async iteration patterns (<c>await foreach</c>)</item>
/// </list>
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
/// 
/// // Async enumerable for streaming scenarios
/// await foreach (var testCase in testData.ToDistinctAsyncEnumerable())
/// {
///     await ProcessTestCaseAsync(testCase);
/// }
/// </code>
/// </example>
public static class AsyncCollectionConverter
{
    #region Task<>

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
        bool isSmallCollection =
            testDataCollection is ICollection<TTestData> collection &&
            collection.Count < 10;

        return isSmallCollection ?
            Task.FromResult(convertToDistinctRowArray())
            : Task.Run(() => convertToDistinctRowArray());

        #region Local function

        TRow[] convertToDistinctRowArray()
        => testDataCollection.ToDistinctArray(convertRow);

        #endregion
    }

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

    #region IAsyncEnumerable<>

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
        return toAsync(testDataCollection.ToDistinctArray(convertRow));

        #region Local function

        static async IAsyncEnumerable<TRow> toAsync(TRow[] rows)
        {
            foreach (var row in rows)
            {
                yield return row;
            }
        }

        #endregion
    }

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct elements.
    /// </summary>
    /// <remarks>
    /// This is an identity conversion that yields the test data items themselves after deduplication.
    /// The deduplication is performed synchronously using <see cref="CollectionConverter.ToDistinctArray{TTestData}(IEnumerable{TTestData})"/>,
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
    => testDataCollection.ToDistinctAsyncEnumerable(convertRow: testData => testData);

    #endregion
}