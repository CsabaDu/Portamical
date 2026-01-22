// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.Converters;
using Portamical.xUnit_v3.TestDataTypes;

namespace Portamical.xUnit_v3.TestBases;

public abstract class TestBase_TheoryTestDataRows(ArgsCode argsCode = ArgsCode.Instance)
: TestBase_xUnit_v3(argsCode)
{
    public IEnumerable<ITheoryTestDataRow> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryTestDataRowCollection(
        ArgsCode,
        testMethodName);
}