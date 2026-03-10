// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.MSTest.Converters;

namespace Portamical.MSTest.TestBases;

public abstract class TestBase : Portamical.TestBases.TestBase
{
    protected static IReadOnlyCollection<object?[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArgsWithTestCaseName(argsCode);

    protected static IReadOnlyCollection<object?[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => Convert(testDataCollection, ArgsCode);
}
