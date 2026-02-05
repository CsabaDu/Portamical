// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.xUnit_v3.DataProviders;
using Portamical.xUnit_v3.DataProviders.Model;
using Portamical.xUnit_v3.TestDataTypes;

namespace Portamical.xUnit_v3.Converters;

public static class CollectionConverter
{
    public static IReadOnlyCollection<ITheoryTestDataRow> ToTheoryTestDataRowCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        convertRow: TestDataConverter.ToTheoryTestDataRow,
        argsCode,
        testMethodName);

    public static TheoryTestData<TTestData> ToTheoryTestData<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        initDataProvider: ttdr => TestDataConverter.ToTheoryTestData(ttdr, argsCode, testMethodName),
        convertRow: TestDataConverter.ToTheoryTestDataRow,
        addRow: TheoryTestData<TTestData>.AddRow,
        argsCode,
        testMethodName);

    public static IReadOnlyCollection<ITheoryDataRow> ToTheoryDataRowCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        convertRow: TestDataConverter.ToTheoryDataRow,
        argsCode,
        testMethodName);
}
