// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.NUnit.Converters;
using Portamical.NUnit.TestDataTypes;

namespace Portamical.NUnit.TestBases;

public abstract class TestBase_TestCaseTestDatas(ArgsCode argsCode = ArgsCode.Instance)
: TestBase_NUnit(argsCode)
{
    protected IReadOnlyCollection<TestCaseTestData<TTestData>> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTestCaseTestDataCollection(
        ArgsCode,
        testMethodName);
}