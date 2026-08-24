// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.TestData;

namespace Portamical.DataProviders.Models.TestData;

public sealed class TestDataProvider<TTestData>
: TestDataProviderBase<TTestData, TTestData>,
ITestDataProvider<TTestData>
where TTestData : notnull, ITestData
{
    public TestDataProvider(TTestData testData)
    : base(testData, convertRow: td => td)
    {
    }

    public TestDataProvider(IEnumerable<TTestData> testDataCollection)
    : base(testDataCollection)
    {
    }

    public override void AddRow(TTestData testData)
    => AddRow(testData,
        convertRow: td => td);
}
