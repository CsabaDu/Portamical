// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.TUnit.Converters;

public static class TestDataConverter
{
    public static TestDataRow<TTestData> ToTestDataRow<TTestData>(
        this TTestData testData,
        string? testMethodName,
        params string[]? categories)
    where TTestData : notnull, ITestData
    => new(
        Data: testData,
        DisplayName: testData.GetDisplayName(testMethodName),
        Categories: categories);
}