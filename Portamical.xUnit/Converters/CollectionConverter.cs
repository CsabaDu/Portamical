// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit.DataProviders;

namespace Portamical.xUnit.Converters;

/// <summary>
/// Provides extension methods for transforming collections of <see cref="ITestData"/> 
/// into framework-ready representations. Acts as a dual-strategy converter supporting 
/// both parameter arrays and custom row formats.
/// </summary>
public static class CollectionConverter
{
    public static TheoryData<TTestData> ToTheoryData<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
#pragma warning disable CA1825
=> [.. testDataCollection.ToDistinctArray()];
#pragma warning restore CA1825

    public static TestDataProvider<TTestData> ToTestDataProvider<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDataProvider(
        initDataProvider: TestDataConverter.InitTestDataProvider,
        argsCode: argsCode,
        testMethodName: null);
}
