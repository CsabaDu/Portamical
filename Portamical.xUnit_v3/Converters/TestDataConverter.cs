// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.TestDataTypes.Model;

namespace Portamical.xUnit_v3.Converters;

public static class TestDataConverter
{
    internal static TheoryTestDataRow<TTestData> ToTheoryTestDataRow<TTestData>(
        this TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => new(testData, argsCode, testMethodName);

    internal static TheoryTestDataRow ToTheoryTestDataRow(
        this ITestData testData,
        ArgsCode argsCode)
    => testData.ToTheoryTestDataRow(argsCode, null);
}
