// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.Models;

public abstract class TestDataProvider<TTestData, TRow>
: DistinctDataProvider<TTestData, TRow>
where TTestData : notnull, ITestData
{
    protected TestDataProvider()
    {
    }

    protected TestDataProvider(TTestData testData)
    {
        AddRow(testData);
    }

    protected TestDataProvider(IEnumerable<TTestData> testDataCollection)
    {
        AddRange(testDataCollection);
    }
}