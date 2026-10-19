// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.MSTest.TestDataTypes;

namespace Portamical.MSTest.DataProviders.Models;

public abstract class TestDataProvider<TTestData, TRow>
: Portamical.DataProviders.Models.TestDataProvider<TTestData, TRow>,
ITestDataProvider<TTestData, TRow>
where TTestData : notnull, ITestData
{
    #region Constructors

    protected TestDataProvider(params string[] testCategories)
    : base()
    {
        TestCategories = testCategories;
    }

    protected TestDataProvider(TTestData testData, params string[] testCategories    )
    : base(testData)
    {
        TestCategories = testCategories;
    }

    protected TestDataProvider(IEnumerable<TTestData> testDataCollection, params string[] testCategories)
    : base(testDataCollection)
    {
        TestCategories = testCategories;
    }

    #endregion

    #region Properties

    public string[]? TestCategories { get; init; }

    #endregion

    #region ITestDataSource implementation

    public IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        foreach (var namedCase in GetNamedCases())
        {
            var testData = (TTestData)namedCase;

            yield return [TestDataRow.From(
                testData,
                convertRow: ConvertRow,
                testMethodName: methodInfo.Name,
                testCategories: TestCategories ?? [])];
        }
    }

    public string? GetDisplayName(MethodInfo methodInfo, params object?[]? data)
    => NamedCase.CreateDisplayName(testMethod: methodInfo, args: data);

    #endregion
}
