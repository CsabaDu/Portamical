// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.xUnit_v3.DataProviders.Model;
using Portamical.xUnit_v3.TestDataTypes;

namespace Portamical.xUnit_v3.Converters;

public static class CollectionConverter
{
    internal static IReadOnlyCollection<ITheoryTestDataRow> ToTheoryTestDataRowCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        TestDataConverter.ToTheoryTestDataRow,
        argsCode,
        testMethodName);

    internal static TheoryTestData<TTestData> ToTheoryTestData<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        ttdr => new TheoryTestData<TTestData>(ttdr, argsCode, testMethodName),
        TestDataConverter.ToTheoryTestDataRow,
        TheoryTestData<TTestData>.AddRow,
        argsCode,
        testMethodName);

    internal static IReadOnlyCollection<ITheoryDataRow> ToTheoryDataRowCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        TestDataConverter.ToTheoryDataRow,
        argsCode,
        testMethodName);
}
