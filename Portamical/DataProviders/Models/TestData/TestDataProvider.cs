// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.TestData;

namespace Portamical.DataProviders.Models.TestData;

public sealed class TestDataProvider<TTestData>
: DistinctDataProvider<TTestData, TTestData>,
ITestDataProvider<TTestData>
where TTestData : notnull, ITestData
{
    public TestDataProvider()
    {
    }
        
    public TestDataProvider(TTestData testData)
    : base(testData)
    {
    }

    public TestDataProvider(IEnumerable<TTestData> testDataCollection)
    : base(testDataCollection)
    {
    }

    public override TTestData ConvertRow(TTestData testData)
    => testData;
}
