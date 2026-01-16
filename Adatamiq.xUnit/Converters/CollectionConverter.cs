// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.Converters;
using Adatamiq.Strategy;
using Adatamiq.TestDataTypes;
using Xunit;

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
        ArgsCode.Instance => testDataCollection.Convert(
            (testData, argsCode, testMethodName) => testData,
            (testData) => new TheoryData<TTestData>(testData),
            (theoryData, testData) => theoryData.Add(testData),
            argsCode,
            null),
        ArgsCode.Properties => testDataCollection.Convert(
            (testData, argsCode, testMethodName) => testData.ToArgs(argsCode),
            (properties) => new TheoryData<object?[]>(properties),
            (theoryData, properties) => theoryData.Add(properties),
            argsCode,
            null),
        _ => throw argsCode.GetInvalidEnumArgumentException(nameof(argsCode)),
    };
}