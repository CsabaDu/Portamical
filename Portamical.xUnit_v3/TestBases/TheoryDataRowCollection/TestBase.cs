// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.Converters;

namespace Portamical.xUnit_v3.TestBases.TheoryDataRowCollection;

public abstract class TestBase : Portamical.TestBases.TestDataCollection.TestBase
{
    protected static IEnumerable<ITheoryDataRow> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryDataRowCollection(
        argsCode,
        testMethodName);

    protected static IEnumerable<ITheoryDataRow> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => Convert(testDataCollection, ArgsCode, testMethodName);
}