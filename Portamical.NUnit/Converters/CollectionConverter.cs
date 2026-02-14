// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.NUnit.TestDataTypes;

namespace Portamical.NUnit.Converters;

public static class CollectionConverter
{
    public static IReadOnlyCollection<TestCaseData> ToTestCaseDataCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        TestDataConverter.ToTestCaseData,
        argsCode,
        testMethodName);

    public static IReadOnlyCollection<TestCaseTestData<TTestData>> ToTestCaseTestDataCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        TestDataConverter.ToTestCaseTestData,
        argsCode,
        testMethodName);
}