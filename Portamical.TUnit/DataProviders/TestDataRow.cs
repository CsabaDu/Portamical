// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.TUnit.DataProviders;

public static class TestDataRow
{
    public static TestDataRow<TRow> From<TTestData, TRow>(
        TTestData testData,
        Func<TTestData, TRow> convertRow,
        string? testMethodName,
        params string[]? categories)
    where TTestData : notnull, ITestData
    => new(
        Data: convertRow(testData),
        DisplayName: testData.GetDisplayName(testMethodName),
        Categories: categories);

    public static TestDataRow<TTestData> From<TTestData>(
        TTestData testData,
        string? testMethodName,
        params string[]? categories)
    where TTestData : notnull, ITestData
    => From(
        testData,
        convertRow: td => td,
        testMethodName,
        categories);


}