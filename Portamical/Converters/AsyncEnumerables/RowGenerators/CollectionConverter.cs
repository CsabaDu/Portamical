// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.TestData;

namespace Portamical.Converters.AsyncEnumerables.RowGenerators;

public static class CollectionConverter
{
    #region ToAsyncRowGeneratorEnumerable

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async IAsyncEnumerable<Func<Task<TRow>>> ToAsyncRowGeneratorEnumerable<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        var snapshot = testDataCollection.ToRowArray();

        foreach (var testData in snapshot)
        {
            yield return () =>Task.FromResult(convertRow(testData));
        }

        await Task.CompletedTask; // Ensures the method is recognized as async for IAsyncEnumerable
    }

    #endregion

    #region ToDistinctAsyncRowGeneratorEnumerable

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async IAsyncEnumerable<Func<Task<TRow>>> ToDistinctAsyncRowGeneratorEnumerable<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        var snapshot = testDataCollection.ToDistinctRowArray();

        foreach (var testData in snapshot)
        {
            yield return () => Task.FromResult(convertRow(testData));
        }

        await Task.CompletedTask; // Ensures the method is recognized as async for IAsyncEnumerable
    }

    #endregion
}
