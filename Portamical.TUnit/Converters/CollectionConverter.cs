// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using static Portamical.Converters.CollectionConverter;

namespace Portamical.TUnit.Converters;

public static class CollectionConverter
{
    public static IReadOnlyCollection<object?[]> ToDistinctReadOnly<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(Core.Strategy.ArgsCode.Instance);

    public static Task<TTestData[]> ToDistinctTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => Task.FromResult(testDataCollection.ToDistinctArray());

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
