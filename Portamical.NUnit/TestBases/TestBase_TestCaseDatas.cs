// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.NUnit.Converters;

namespace Portamical.NUnit.TestBases;

public abstract class TestBase_TestCaseData(ArgsCode argsCode = ArgsCode.Instance)
: TestBase_NUnit(argsCode)
{
    protected IEnumerable<TestCaseData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTestCaseDataCollection(
        ArgsCode,
        testMethodName);
}
