// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.Models.CustomRow;
using Portamical.xUnit_v3.TestDataTypes;
using Portamical.xUnit_v3.TestDataTypes.Model;

namespace Portamical.xUnit_v3.DataProviders.Models;

public sealed class TestDataProvider<TTestData>
: TestDataProvider<TTestData, ITheoryTestDataRow>
where TTestData: notnull, ITestData
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

    public override ITheoryTestDataRow ConvertRow(TTestData testData)
    => TheoryTestDataRow.From(testData, ArgsCode, TestMethodName);
}
