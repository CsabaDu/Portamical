// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.TUnit.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
public abstract class PortamicalBaseDataAttribute<TTestData>(Func<IEnumerable<TTestData>> dataFactory)
: DataSourceGeneratorAttribute<TTestData>
where TTestData : notnull, ITestData
{
    private readonly Func<IEnumerable<TTestData>> _dataFactory = dataFactory;

    private IEnumerable<TTestData>? _cached;

    protected override IEnumerable<Func<TTestData>> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
    {
        _cached ??= _dataFactory();

        foreach (var testData in _cached)
        {
            yield return () => testData;
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class PortamicalDataAttribute<TTestData>(Func<IEnumerable<TTestData>> dataFactory)
: PortamicalBaseDataAttribute<TTestData>(dataFactory)
where TTestData : notnull, ITestData
{
}