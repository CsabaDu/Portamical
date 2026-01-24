// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.Converters;

namespace Portamical.xUnit_v3.TestBases.TheoryDataRowCollection;

public abstract class TestBase(ArgsCode argsCode = ArgsCode.Instance)
: Portamical.TestBases.TestBase(argsCode)
{
    public IReadOnlyCollection<ITheoryDataRow> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryDataRowCollection(
        ArgsCode,
        testMethodName);
}