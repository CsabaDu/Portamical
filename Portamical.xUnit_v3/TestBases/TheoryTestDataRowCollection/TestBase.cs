// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.Converters;
using Portamical.xUnit_v3.TestDataTypes;

namespace Portamical.xUnit_v3.TestBases.TheoryTestDataRowCollection;

public abstract class TestBase(ArgsCode argsCode = ArgsCode.Instance)
: Portamical.TestBases.TestBase(argsCode)
{
    public IReadOnlyCollection<ITheoryTestDataRow> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryTestDataRowCollection(
        ArgsCode,
        testMethodName);
}