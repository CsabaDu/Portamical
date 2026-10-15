// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.TestDataTypes.Models.General;
using Portamical.DataProviders.Models.CustomRow;
using Portamical.MSTest.Converters;

namespace Portamical.MSTest.DataProviders.Models;

public sealed class TestDataProvider<TTestData>
: TestDataProvider<TTestData, object[]>, ITestDataProvider
where TTestData : notnull, ITestData
{
    #region Constructors

    public TestDataProvider(ArgsCode argsCode, string? testMethodName, params string[] testCategories)
    : base(argsCode, testMethodName)
    {
        TestCategories = testCategories;
    }

    public TestDataProvider(TTestData testData, ArgsCode argsCode, string? testMethodName, params string[] testCategories)
    : base(testData, argsCode, testMethodName)
    {
        TestCategories = testCategories;
    }

    public TestDataProvider(IEnumerable<TTestData> testDataCollection, ArgsCode argsCode, string? testMethodName, params string[] testCategories)
    : base(testDataCollection, argsCode, testMethodName)
    {
        TestCategories = testCategories;
    }

    #endregion

    #region Properties

    public string[] TestCategories { get; }

    #endregion

    #region ITestDataSource implementation

    public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
    {
        foreach (var namedCase in GetNamedCases())
        {
            var testData = (TTestData)namedCase;

            yield return testData.ToSingleTestDataRowArray(ArgsCode, methodInfo.Name, TestCategories);
        }
    }

    public string? GetDisplayName(MethodInfo methodInfo, params object?[]? data)
    => NamedCase.CreateDisplayName(testMethod: methodInfo, args: data);

    #endregion

    #region TestDataProvider implementation

    public override object[] ConvertRow(TTestData testData)
    => testData.ToSingleTestDataRowArray(ArgsCode, TestMethodName, TestCategories);

    #endregion
}
