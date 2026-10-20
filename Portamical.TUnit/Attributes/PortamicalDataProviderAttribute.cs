// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.TUnit.DataProviders.Models;
using Portamical.TUnit.TestDataTypes;
using static Portamical.Core.Safety.Validator;

namespace Portamical.TUnit.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
public abstract class PortamicalDataProviderAttribute<TTestData, TRow>(TestDataProvider<TTestData, TRow> dataProvider)
: TypedDataSourceAttribute<TestDataRow<TRow>>
where TTestData : notnull, ITestData
{
    private readonly TestDataProvider<TTestData, TRow> _dataProvider = NotNull(dataProvider, nameof(dataProvider));

    public override async IAsyncEnumerable<Func<Task<TestDataRow<TRow>>>> GetTypedDataRowsAsync(DataGeneratorMetadata dataGeneratorMetadata)
    {
        var testMethodName = dataGeneratorMetadata.TestInformation?.Name;

        foreach (var namedCase in _dataProvider.GetNamedCases())
        {
            yield return () => Task.FromResult(TestDataRow.From(
                testData: (TTestData)namedCase,
                convertRow: _dataProvider.ConvertRow,
                testMethodName: testMethodName,
                categories: _dataProvider.Categories));
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class PortamicalDataProviderAttribute<TTestData>(TestDataProvider<TTestData> dataProvider)
: PortamicalDataProviderAttribute<TTestData, TTestData>(dataProvider)
where TTestData : notnull, ITestData
{
    public PortamicalDataProviderAttribute(TTestData[] snapshot, params string[] categories)
    : this(new TestDataProvider<TTestData>(NotNullOrEmpty(snapshot, nameof(snapshot)), categories))
    {
    }
}
