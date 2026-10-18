// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.MSTest.DataProviders.Models.TestData;

public sealed class TestDataProvider<TTestData>
: TestDataProvider<TTestData, TTestData>, ITestDataProvider<TTestData, TTestData>

where TTestData : notnull, ITestData
{
    #region Constructors

    public TestDataProvider(params string[] testCategories)
    : base(testCategories)
    {
    }

    public TestDataProvider(TTestData testData, params string[] testCategories)
    : base(testData, testCategories)
    {
    }

    public TestDataProvider(IEnumerable<TTestData> testDataCollection, params string[] testCategories)
    : base(testDataCollection, testCategories)
    {
    }

    #endregion

    #region TestDataProvider implementation

    public override TTestData ConvertRow(TTestData testData)
    => testData;

    #endregion
}
