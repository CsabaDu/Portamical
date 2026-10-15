// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.TUnit.Converters;

namespace Portamical.TUnit.DataProviders.Models;

public abstract class TestDataProvider<TTestData, TRow>
: Portamical.DataProviders.Models.TestDataProvider<TTestData, TRow>,
ITestDataProvider
where TTestData : notnull, ITestData
{
    #region Constructors

    private protected TestDataProvider(string[]? categories)
    : base()
    {
        Categories = categories;
    }

    private protected TestDataProvider(TTestData testData, string[]? categories)
    : base(testData)
    {
        Categories = categories;
    }

    private protected TestDataProvider(IEnumerable<TTestData> testDataCollection, string[]? categories)
    : base(testDataCollection)
    {
        Categories = categories;
    }

    #endregion

    #region Properties

    public string[]? Categories { get; init; }

    #endregion

    public abstract IEnumerable<Func<object?[]>> GenerateDataFactories(DataSourceContext context);
}

public sealed class TestDataProvider<TTestData>(IEnumerable<TTestData> testDataCollection, params string[] categories)
: TestDataProvider<TTestData, TTestData>(testDataCollection, categories)
where TTestData : notnull, ITestData
{
    #region TestDataProvider implementation

    public override TTestData ConvertRow(TTestData testData)
    => testData;

    #endregion

    #region IDataSource implementation

    public override IEnumerable<Func<object?[]>> GenerateDataFactories(DataSourceContext context)
    {
        var testMethodName = context.TargetMember?.Name;

        foreach (var namedCase in GetNamedCases())
        {
            var testData = (TTestData)namedCase;

            yield return () => [testData.ToTestDataRow(testMethodName, Categories)];
        }
    }

    #endregion
}
