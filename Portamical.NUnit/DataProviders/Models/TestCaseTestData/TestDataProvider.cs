// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.Models.CustomRow;
using Portamical.NUnit.TestDataTypes.Models;

namespace Portamical.NUnit.DataProviders.Models.TestCaseTestData;

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
    => new(testData, ArgsCode, TestMethodName);
}
