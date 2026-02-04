// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.NUnit.Converters;

namespace Portamical.NUnit.TestBases;

public abstract class TestBase : Portamical.TestBases.TestData.TestBase
{
    protected static IReadOnlyCollection<TestCaseData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTestCaseTestDataCollection(argsCode, testMethodName);
}