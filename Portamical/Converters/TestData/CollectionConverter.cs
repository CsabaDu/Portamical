// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters.TestData;

public static class CollectionConverter
{
    #region ToRowArray

    /// <summary>
    /// Converts a collection of test data into an array, preserving all elements in their original order.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert to an array. Cannot be null or empty.
    /// </param>
    /// <returns>
    /// An array containing all elements from the input collection, in their original order.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// This method provides a simple identity conversion to array format. Use <see cref="ToDistinctRowArray{TTestData}(IEnumerable{TTestData})"/>
    /// if deduplication is needed.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTestData[] ToRowArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => NotNullOrEmpty(testDataCollection, nameof(testDataCollection));

    #endregion

    #region ToDistinctRowArray

    /// <summary>
    /// Creates an array containing distinct elements from the specified test data collection.
    /// </summary>
    /// <typeparam name="TTestData">The type of elements in the test data collection. Must implement ITestData and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data elements from which to create a distinct array. Cannot be null.</param>
    /// <returns>An array containing the distinct elements from the input collection. The order of elements is
    /// preserved from the original collection (first occurrence wins).</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Deduplication is based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>.
    /// This is useful for removing duplicate test cases from a collection.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic deduplication
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", 5),  // Duplicate
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", 12)
    /// };
    /// 
    /// var distinct = testData.ToDistinctRowArray();
    /// // Result: 2 elements (duplicate removed based on TestCaseName)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTestData[] ToDistinctRowArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(
        convertRow: testData => testData);

    #endregion
}
