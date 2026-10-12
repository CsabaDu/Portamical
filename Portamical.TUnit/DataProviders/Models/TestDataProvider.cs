// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.Models;

namespace Portamical.TUnit.DataProviders.Models;

public sealed class TestDataProvider<TTestData>
: TestDataProvider<TTestData, TestDataRow<TTestData>>,
ITestDataProvider<TTestData>
where TTestData : notnull, ITestData
{
    #region Constructors

    private TestDataProvider()
    {
    }

    public TestDataProvider(string? testMethodName, params string[]? categories)
    : this()
    {
        TestMethodName = testMethodName;
        Categories = categories;
    }

    public TestDataProvider(TTestData testData, string? testMethodName, params string[]? categories)
    : base(testData)
    {
        TestMethodName = testMethodName;
        Categories = categories;
    }

    public TestDataProvider(IEnumerable<TTestData> testDataCollection, string? testMethodName, params string[]? categories)
    : base(testDataCollection)
    {
        TestMethodName = testMethodName;
        Categories = categories;
    }

    #endregion

    #region Properties

    public string? TestMethodName { get; init; }
    public string[]? Categories { get; init; }
    public bool SkipIfEmpty { get; set; } = default;

    #endregion

    #region TestDataProvider implementation

    public override TestDataRow<TTestData> ConvertRow(TTestData testData)
    => new(Data: testData,
        DisplayName: testData.GetDisplayName(TestMethodName),
        Categories: Categories);

    #endregion

    #region IDataSourceAttribute implementation

    public async IAsyncEnumerable<Func<Task<object?[]?>>> GetDataRowsAsync(DataGeneratorMetadata dataGeneratorMetadata)
    {
        foreach (var testDataRow in this)
        {
            yield return () => Task.FromResult<object?[]?>([testDataRow]);
        }
    }

    #endregion
}
