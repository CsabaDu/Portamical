// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.TestData;

namespace Portamical.Converters.AsyncEnumerables.TestData;

public static class CollectionConverter
{
    #region ToRowArray

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of elements (identity conversion).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is an identity conversion that yields the test data items themselves without transformation.
    /// The collection is convertedRows to an array using <see cref="RowArrays.CollectionConverter.ToRowArray{TTestData}(IEnumerable{TTestData})"/>,
    /// then the resulting elements are yielded asynchronously. This method is useful for integrating
    /// synchronous data into asynchronous workflows or streaming scenarios.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of elements in the test data collection. Must implement <see cref="ITestData"/> and cannot be null.</typeparam>
    /// <param name="testDataCollection">The source collection of test data elements to convert. Cannot be null or empty.</param>
    /// <returns>An asynchronous sequence that yields each element, preserving the order from the input collection.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <example>
    /// <code>
    /// // Convert to async stream for consumption in async context
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// await foreach (var item in testData.ToAsyncRowEnumerable())
    /// {
    ///     Console.WriteLine(item.TestCaseName);
    /// }
    /// // Output: "Add(2,3)", "Add(2,3)", "Add(5,7)" (no deduplication)
    /// </code>
    /// </example>
    public static IAsyncEnumerable<TTestData> ToAsyncRowEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => AsyncEnumerables.CollectionConverter.ToAsyncRowEnumerable(testDataCollection.ToRowArray());

    #endregion

    #region ToDistinctRowArray

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct elements (identity conversion).
    /// Removes duplicates based on <see cref="INamedCase.TestCaseName"/> identity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is an identity conversion that yields the test data items themselves after deduplication.
    /// The deduplication is performed synchronously using <see cref="RowArrays.CollectionConverter.ToDistinctRowArray{TTestData}(IEnumerable{TTestData})"/>,
    /// but the resulting elements are yielded asynchronously. This method is useful for integrating
    /// synchronous deduplicated data into asynchronous workflows or streaming scenarios.
    /// </para>
    /// <para>
    /// Deduplication uses <see cref="NamedCase.Comparer"/> based on <see cref="INamedCase.TestCaseName"/>.
    /// Test data with identical <c>TestCaseName</c> values are considered duplicates; only the first occurrence is retained.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">The type of elements in the test data collection. Must implement <see cref="ITestData"/> and cannot be null.</typeparam>
    /// <param name="testDataCollection">The source collection of test data elements to convert. Cannot be null or empty.</param>
    /// <returns>An asynchronous sequence that yields each distinct element once, preserving the order of first occurrence.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
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
    /// await foreach (var item in testData.ToDistinctAsyncRowEnumerable())
    /// {
    ///     Console.WriteLine(item.TestCaseName);
    /// }
    /// // Output: "Add(2,3)", "Add(5,7)" (duplicate removed)
    /// </code>
    /// </example>
    public static IAsyncEnumerable<TTestData> ToDistinctAsyncRowEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => AsyncEnumerables.CollectionConverter.ToAsyncRowEnumerable(testDataCollection.ToDistinctRowArray());

    #endregion
}
