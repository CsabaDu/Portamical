// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.Models.CustomRow;
using Portamical.NUnit.TestDataTypes.Model;

namespace Portamical.NUnit.DataProviders.Model;

public sealed class TestDataProvider<TTestData>
: TestDataProvider<TTestData, TestCaseTestData<TTestData>>
where TTestData : notnull, ITestData
{
    public TestDataProvider(ArgsCode argsCode, string? testMethodName)
    : base(argsCode, testMethodName)
    {
    }

    public TestDataProvider(TTestData testData, ArgsCode argsCode, string? testMethodName)
    : base(testData, argsCode, testMethodName)
    {
    }

    public TestDataProvider(IEnumerable<TTestData> testDataCollection, ArgsCode argsCode, string? testMethodName)
    : base(testDataCollection, argsCode, testMethodName)
    {
    }

    public override TestCaseTestData<TTestData> ConvertRow(TTestData testData)
    => TestCaseTestData<TTestData>.From(testData, ArgsCode, TestMethodName);
}
