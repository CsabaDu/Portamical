// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.Converters;
using Portamical.xUnit_v3.DataProviders.Model;

namespace Portamical.xUnit_v3.TestBases;

public abstract class TestBase : Portamical.TestBases.TestBase
{
    protected static TheoryTestData<TTestData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryTestData(
        argsCode,
        testMethodName);
}