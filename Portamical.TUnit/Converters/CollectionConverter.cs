// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays;
using Portamical.TUnit.DataProviders.Models;
using Portamical.TUnit.TestDataTypes;

namespace Portamical.TUnit.Converters;

public static class CollectionConverter
{
    public static TestDataProvider<TTestData> ToDataProvider<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        params string[] categories)
    where TTestData : notnull, ITestData
    => new(testDataCollection, categories);

    public static TestDataRow<TTestData>[] ToDistinctRowArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        string? testMethodName,
        params string[] categories)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(
        convertRow: testData => TestDataRow.From(
            testData,
            testMethodName,
            categories));
}
