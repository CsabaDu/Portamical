// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit.DataProviders;

namespace Portamical.xUnit.Converters;

public static class TestDataConverter
{
    internal static TestDataProvider<TTestData> InitTestDataProvider<TTestData>(
        TTestData testData,
        ArgsCode argsCode,
#pragma warning disable IDE0060
// testMethodName is used here only for having the same signature
// as the one in 'TestDataProviderFactory'.
        string? testMethodName = null)
#pragma warning restore IDE0060
    where TTestData : notnull, ITestData
    => new(testData, argsCode);
}