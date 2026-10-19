// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.TUnit.DataProviders.Models;
using Portamical.TUnit.TestDataTypes;
using static Portamical.Core.Safety.Validator;

namespace Portamical.TUnit.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class PortamicalDataProviderAttribute<TTestData>
: TypedDataSourceAttribute<TestDataRow<TTestData>>
where TTestData : notnull, ITestData
{
    public PortamicalDataProviderAttribute(TTestData[] snapshot, params string[] categories)
    {
        _dataProvider = new TestDataProvider<TTestData>(
            testDataCollection: NotNullOrEmpty(snapshot, nameof(snapshot)),
            categories);
    }

    public PortamicalDataProviderAttribute(TestDataProvider<TTestData> dataProvider)
    {
        _dataProvider = NotNull(dataProvider, nameof(dataProvider));
    }

    private  readonly TestDataProvider<TTestData> _dataProvider;

    public override async IAsyncEnumerable<Func<Task<TestDataRow<TTestData>>>> GetTypedDataRowsAsync(DataGeneratorMetadata dataGeneratorMetadata)
    {
        var testMethodName = dataGeneratorMetadata.TestInformation?.Name;

        foreach (var namedCase in _dataProvider.GetNamedCases())
        {
            var testData = (TTestData)namedCase;

            yield return () => Task.FromResult(
                TestDataRow.From(
                    testData,
                    testMethodName,
                    categories: _dataProvider.Categories));
        }
    }
}
