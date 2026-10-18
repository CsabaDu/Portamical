// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.MSTest.DataProviders.Models.ObjectArray;

public sealed class TestDataProvider<TTestData>
: TestDataProvider<TTestData, object?[]>, ITestDataProvider<TTestData, object?[]>

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

    public override object?[] ConvertRow(TTestData testData)
    => testData.ToArgs(ArgsCode.Properties);

    #endregion
}
