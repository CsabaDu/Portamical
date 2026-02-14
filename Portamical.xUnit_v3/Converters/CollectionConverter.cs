// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.DataProviders.Model;
using Portamical.xUnit_v3.TestDataTypes;

namespace Portamical.xUnit_v3.Converters;

public static class CollectionConverter
{
    public static TheoryTestData<TTestData> ToTheoryTestData<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDataProvider(
        initDataProvider: TestDataConverter.ToTheoryTestData,
        argsCode,
        testMethodName);

    public static IReadOnlyCollection<ITheoryTestDataRow> ToTheoryTestDataRowCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        convertRow: TestDataConverter.ToTheoryTestDataRow,
        argsCode,
        testMethodName);

    public static IReadOnlyCollection<ITheoryDataRow> ToTheoryDataRowCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        convertRow: TestDataConverter.ToTheoryDataRow,
        argsCode,
        testMethodName);
}
