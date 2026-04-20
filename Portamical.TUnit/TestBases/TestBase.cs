// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.TUnit.Converters;

namespace Portamical.TUnit.TestBases;

/// <summary>
/// Provides TUnit-specific helpers for converting Portamical test data into synchronous or asynchronous sources.
/// </summary>
public abstract class TestBase : Portamical.TestBases.TestBase
{
    /// <summary>
    /// Converts a test data collection to an asynchronous sequence of distinct test data items.
    /// </summary>
    /// <typeparam name="TTestData">The test data type contained in the collection.</typeparam>
    /// <param name="testDataCollection">The source collection of test data.</param>
    /// <returns>An asynchronous sequence that yields each distinct test data item once.</returns>
    protected static IAsyncEnumerable<TTestData> ConvertAsync<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToAsyncDistinct();

    /// <summary>
    /// Converts a test data collection to a task that yields the distinct test data items as an array.
    /// </summary>
    /// <typeparam name="TTestData">The test data type contained in the collection.</typeparam>
    /// <param name="testDataCollection">The source collection of test data.</param>
    /// <returns>A completed task containing the distinct test data items.</returns>
    protected static Task<TTestData[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArrayTask();
}
