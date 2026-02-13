// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.DataProviders.Model;
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

    internal static ITheoryDataRow ToTheoryDataRow<TTestData>(
        this TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    {
        object row = argsCode == ArgsCode.Properties ?
            testData.ToArgs(argsCode)
            : testData;

        // Create the xUnit TheoryDataRow with a unified display name.
        return new TheoryDataRow(row)
        {
            TestDisplayName =
                testData.GetDisplayName(testMethodName)
        };
    }
}
