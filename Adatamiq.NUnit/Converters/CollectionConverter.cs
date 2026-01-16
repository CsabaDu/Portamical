// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.Converters;
using Adatamiq.NUnit.TestDataTypes;

namespace Adatamiq.NUnit.Converters;

public static class CollectionConverter
{
    public static IEnumerable<TestCaseData> ToTestCaseDataCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        TestDataConverter.ToTestCaseData,
        argsCode,
        testMethodName);

    public static IEnumerable<TestCaseTestData<TTestData>> ToTestCaseTestDataCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        TestDataConverter.ToTestCaseTestData,
        argsCode,
        testMethodName);
}