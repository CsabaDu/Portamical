// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit.DataProviders;

namespace Portamical.xUnit.Converters;

public static class TestDataConverter
{
    internal static TestDataProvider<TTestData> InitTestDataProvider<TTestData>(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => new(testData, argsCode);
}