// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays;
using Portamical.TUnit.DataProviders;
using Portamical.TUnit.DataProviders.Models;

namespace Portamical.TUnit.Converters;

public static class CollectionConverter
{
    public static ITestDataProvider<TTestData> ToDataProvider<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        string? testMethodName,
        params string[]? categories)
    where TTestData : notnull, ITestData
    => new TestDataProvider<TTestData>(
        testDataCollection,
        testMethodName,
        categories);

    public static TestDataRow<TTestData>[] ToDistinctRowArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        string? testMethodName,
        params string[]? categories)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(
        convertRow: testData => new TestDataRow<TTestData>(
            Data: testData,
            DisplayName: testData.GetDisplayName(testMethodName),
            Categories: categories));
}
