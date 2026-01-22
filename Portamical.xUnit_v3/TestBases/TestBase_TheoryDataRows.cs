// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.Converters;

namespace Portamical.xUnit_v3.TestBases;

public abstract class TestBase_TheoryDataRows(ArgsCode argsCode = ArgsCode.Instance)
: TestBase_xUnit_v3(argsCode)
{
    public IEnumerable<ITheoryDataRow> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryDataRowCollection(
        ArgsCode,
        testMethodName);
}