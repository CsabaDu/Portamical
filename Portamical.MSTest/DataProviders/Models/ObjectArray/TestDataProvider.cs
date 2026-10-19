// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;

namespace Portamical.MSTest.DataProviders.Models.ObjectArray;

public sealed class TestDataProvider<TTestData>
: TestDataProvider<TTestData, object?[]>
where TTestData : notnull, ITestData
{
    #region Constructors

    public TestDataProvider(ArgsCode argsCode, params string[] testCategories)
    : base(testCategories)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
    }

    public TestDataProvider(TTestData testData, ArgsCode argsCode, params string[] testCategories)
    : base(testData, testCategories)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
    }

    public TestDataProvider(IEnumerable<TTestData> testDataCollection, ArgsCode argsCode, params string[] testCategories)
    : base(testDataCollection, testCategories)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
    }

    public ArgsCode ArgsCode { get; init; }

    #endregion

    #region TestDataProvider implementation

    public override object?[] ConvertRow(TTestData testData)
    => testData.ToArgs(ArgsCode);

    #endregion
}
