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
    // 'testMethodName' exists only to satisfy the signature required by the
    // 'ToDataProvider' method of the Portamical CollectionConverter class
    // as the 'initDataProvider' delegate parameter.
    string? testMethodName = null)
#pragma warning restore IDE0060
    where TTestData : notnull, ITestData
    => new(testData, argsCode);
}