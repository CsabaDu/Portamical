// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.Converters.RowArrays;
using Portamical.Converters.DataProviders.CustomRow;
using Portamical.Core.Identity;
using Portamical.Core.Identity.Model;
using Portamical.DataProviders.Models.CustomRow;
using Portamical.Processing;
using System.Diagnostics;

namespace Portamical.TUnit.Attributes;

/// <summary>
/// Provides a reusable TUnit data-source attribute that materializes Portamical test data from a synchronous factory.
/// </summary>
/// <typeparam name="TTestData">The test data type produced by the factory.</typeparam>
/// <param name="dataFactory">The factory that returns the test data sequence used by TUnit.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
public abstract class PortamicalBaseDataAttribute<TTestData>(Func<IEnumerable<TTestData>> dataFactory)
: DataSourceGeneratorAttribute<TTestData>
where TTestData : notnull, ITestData
{
    EnvironmentVariableTarget 
    private readonly Func<IEnumerable<TTestData>> _dataFactory = dataFactory;
    private readonly HashSet<INamedCase> _namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
    private readonly TestDataProcessor processor = new();
    private IEnumerable<TTestData>? _cached;

    /// <summary>
    /// Generates deferred TUnit data sources from the cached Portamical test data sequence.
    /// </summary>
    /// <param name="dataGeneratorMetadata">Metadata supplied by TUnit for the current data generation request.</param>
    /// <returns>A sequence of delegates that each return one test data item.</returns>
    protected override IEnumerable<Func<TTestData>> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
    {
        _cached ??= _dataFactory();

        //return _cached.ToDistinctConvertedRows(
        //    initConvertedRows: testData => new List<Func<TTestData>>() { () => testData },
        //    addConvertedRow: (testDataList, testData) => testDataList.Add(() => testData));

        foreach (var testData in _cached)
        {
            if (_namedCases.Add(testData))
            {
                yield return () => testData;
            }
        }
    }
}

/// <summary>
/// Exposes Portamical test data to TUnit through a synchronous data-source attribute.
/// </summary>
/// <typeparam name="TTestData">The test data type produced by the factory.</typeparam>
/// <param name="dataFactory">The factory that returns the test data sequence used by TUnit.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class PortamicalDataAttribute<TTestData>(Func<IEnumerable<TTestData>> dataFactory)
: PortamicalBaseDataAttribute<TTestData>(dataFactory)
where TTestData : notnull, ITestData
{
}
