// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using static Portamical.Converters.CollectionConverter;

namespace Portamical.TUnit.Converters;

/// <summary>
/// Provides TUnit-oriented conversions for Portamical test data collections.
/// </summary>
public static class CollectionConverter
{
    /// <summary>
    /// Materializes a distinct test data array and returns it as a completed task.
    /// </summary>
    /// <typeparam name="TTestData">The test data type contained in the collection.</typeparam>
    /// <param name="testDataCollection">The source collection of test data.</param>
    /// <returns>A completed task containing the distinct test data array.</returns>
    public static Task<TTestData[]> ToDistinctArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => Task.FromResult(testDataCollection.ToDistinctArray());

    /// <summary>
    /// Converts a synchronous test data collection to an asynchronous sequence of distinct items.
    /// </summary>
    /// <typeparam name="TTestData">The test data type contained in the collection.</typeparam>
    /// <param name="testDataCollection">The source collection of test data.</param>
    /// <returns>An asynchronous sequence that yields each distinct test data item once.</returns>
    public static IAsyncEnumerable<TTestData> ToAsyncDistinct<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    {
        return toAsync(testDataCollection.ToDistinctArray());

        static async IAsyncEnumerable<TTestData> toAsync(TTestData[] array)
        {
            foreach (var testData in array)
            {
                yield return testData;
            }
        }
    }
}
