// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit.Converters;
using Portamical.xUnit.DataProviders;

namespace Portamical.xUnit.TestBases;

public abstract class TestBase : Portamical.TestBases.TestBase
{
    protected static TheoryTestData<TTestData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryTestData(argsCode);

    protected static TheoryTestData<TTestData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => Convert(testDataCollection, ArgsCode);
}