// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.MSTest.TestDataTypes;

public static class TestDataRow
{
    public static TestDataRow<TRow> From<TTestData, TRow>(
        TTestData testData,
        Func<TTestData, TRow> convertRow,
        string? testMethodName,
        params string[] testCategories)
    where TTestData : notnull, ITestData
    => new(convertRow(testData))
    {
        DisplayName = testData.GetDisplayName(testMethodName),
        TestCategories = testCategories,
    };

    public static TestDataRow<TTestData> From<TTestData>(
        TTestData testData,
        string? testMethodName,
        params string[] testCategories)
    where TTestData : notnull, ITestData
    => From(
        testData,
        convertRow: td => td,
        testMethodName,
        testCategories);

    public static TestDataRow<object?[]> From<TTestData>(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName,
        params string[] testCategories)
    where TTestData : notnull, ITestData
    => From(
        testData,
        convertRow: td => td.ToArgs(argsCode),
        testMethodName,
        testCategories);
}
