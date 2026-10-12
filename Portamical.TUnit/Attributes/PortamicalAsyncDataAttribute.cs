// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.AsyncEnumerables;

namespace Portamical.TUnit.Attributes;

/// <summary>
/// Provides a reusable TUnit async data-source attribute that wraps a synchronous Portamical data factory.
/// </summary>
/// <typeparam name="TTestData">The test data type produced by the factory.</typeparam>
/// <param name="dataFactory">The factory that returns the test data sequence used by TUnit.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
public abstract class PortamicalAsyncBaseDataAttribute<TTestData>(Func<IEnumerable<TTestData>> dataFactory)
: AsyncDataSourceGeneratorAttribute<TTestData>
where TTestData : notnull, ITestData
{
    private readonly Func<IEnumerable<TTestData>> _dataFactory = dataFactory;
    private IEnumerable<TTestData>? _cached;

    /// <summary>
    /// Generates asynchronous TUnit data sources from the Portamical test data sequence.
    /// </summary>
    /// <param name="dataGeneratorMetadata">Metadata supplied by TUnit for the current data generation request.</param>
    /// <returns>An asynchronous sequence of delegates that each asynchronously return one test data item.</returns>
    protected override IAsyncEnumerable<Func<Task<TTestData>>> GenerateDataSourcesAsync(DataGeneratorMetadata dataGeneratorMetadata)
    {
        _cached ??= _dataFactory();

        return _cached.ToDistinctAsyncRowEnumerable<TTestData, Func<Task<TTestData>>>(
            convertRow: testData => () => Task.FromResult(testData));
    }
}

/// <summary>
/// Exposes Portamical test data to TUnit through an asynchronous data-source attribute.
/// </summary>
/// <typeparam name="TTestData">The test data type produced by the factory.</typeparam>
/// <param name="dataFactory">The factory that returns the test data sequence used by TUnit.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
public sealed class PortamicalAsyncDataAttribute<TTestData>(Func<IEnumerable<TTestData>> dataFactory)
: PortamicalAsyncBaseDataAttribute<TTestData>(dataFactory)
where TTestData : notnull, ITestData
{
}
