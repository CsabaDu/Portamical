// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.Models.CustomRow;

namespace Portamical.MSTest.DataProviders.Models;

public sealed class TestDataProvider<TTestData>
: TestDataProvider<TTestData, TTestData>, ITestDataSource
where TTestData : notnull, ITestData
{
    #region Constructors

    public TestDataProvider(ArgsCode argsCode)
    : base(argsCode, testMethodName: null)
    {
    }

    public TestDataProvider(TTestData testData, ArgsCode argsCode)
    : base(testData, argsCode, testMethodName: null)
    {
    }

    public TestDataProvider(IEnumerable<TTestData> testDataCollection, ArgsCode argsCode)
    : base(testDataCollection, argsCode, testMethodName: null)
    {
    }

    #endregion

    #region ITestDataSource implementation

    public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
    {
        foreach (var testData in this)
        {
            yield return ArgsCode == ArgsCode.Instance ?
                convertRow(testData)
                : convertRow(testData.ToArgs(ArgsCode.Properties));
        }

        #region Local function

        object[] convertRow<T>(T row)
        => [new TestDataRow<T>(row)
        {
            DisplayName = GetDisplayName(methodInfo, row),
        }];

        #endregion
    }

    public string? GetDisplayName(MethodInfo methodInfo, params object?[]? data)
    => NamedCase.CreateDisplayName(testMethod: methodInfo, args: data);

    #endregion

    #region TestDataProvider implementation

    public override TTestData ConvertRow(TTestData testData)
    => testData;

    #endregion
}
