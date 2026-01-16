// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.Converters;
using Adatamiq.Strategy;
using Adatamiq.TestDataTypes;
using Adatamiq.xUnit_v3.DataProviders.Model;
using Adatamiq.xUnit_v3.TestDataTypes;

namespace Adatamiq.xUnit_v3.Converters;

public static class CollectionConverter
{
    public static IEnumerable<ITheoryTestDataRow> ToTheoryTestDataRowCollection(
        this IEnumerable<ITestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    => testDataCollection.Convert(
        TestDataConverter.ToTheoryTestDataRow,
        argsCode,
        testMethodName);

    public static TheoryTestData<TTestData> ToTheoryTestData<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        TestDataConverter.ToTheoryTestDataRow,
        ttdr => new TheoryTestData<TTestData>(ttdr, argsCode, testMethodName),
        TheoryTestData<TTestData>.AddRow,
        argsCode,
        testMethodName);
}
