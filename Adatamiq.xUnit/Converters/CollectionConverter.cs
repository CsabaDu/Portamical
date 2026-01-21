// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.Converters;
using Adatamiq.Strategy;
using Adatamiq.TestDataTypes;

namespace Adatamiq.xUnit.Converters;

/// <summary>
/// Provides extension methods for transforming collections of <see cref="ITestData"/> 
/// into framework-ready representations. Acts as a dual-strategy converter supporting 
/// both parameter arrays and custom row formats.
/// </summary>
public static class CollectionConverter
{
    public static TheoryData ToTheoryData<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => argsCode switch
    {
        ArgsCode.Instance => testDataCollection.InstanceToTheoryData(),
        ArgsCode.Properties => testDataCollection.PropertiesToTheoryData(),
        _ => throw argsCode.GetInvalidEnumArgumentException(nameof(argsCode)),
    };

    public static TheoryData<TTestData> InstanceToTheoryData<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        convertRow: (testData, _, _) => testData,
        initDataProvider: testData => new TheoryData<TTestData>(testData),
        addRow: (theoryData, testData) => theoryData.Add(testData),
        ArgsCode.Instance,
        testMethodName: null);

    public static TheoryData<object?[]> PropertiesToTheoryData<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        convertRow: (testData, argsCode, _) => testData.ToArgs(argsCode),
        initDataProvider: row => new TheoryData<object?[]>(row),
        addRow: (theoryData, row) => theoryData.Add(row),
        ArgsCode.Properties,
        testMethodName: null);
}