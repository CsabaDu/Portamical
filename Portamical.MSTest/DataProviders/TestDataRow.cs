// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.MSTest.DataProviders;

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
}
