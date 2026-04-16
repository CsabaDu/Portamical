// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.TUnit.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
public class PortamicalAsyncDataAttribute<TTestData>(Func<IEnumerable<TTestData>> dataFactory)
    : AsyncDataSourceGeneratorAttribute<TTestData>
    where TTestData : notnull, ITestData
{
    private readonly Func<IEnumerable<TTestData>> _dataFactory = dataFactory;

    protected override IAsyncEnumerable<Func<Task<TTestData>>> GenerateDataSourcesAsync(
        DataGeneratorMetadata dataGeneratorMetadata)
    {
        return generateAsync(_dataFactory());

        static async IAsyncEnumerable<Func<Task<TTestData>>> generateAsync(IEnumerable<TTestData> items)
        {
            foreach (var item in items)
            {
                yield return () => Task.FromResult(item);
            }

            await Task.CompletedTask; // opcionális, de nem árt
        }
    }
}