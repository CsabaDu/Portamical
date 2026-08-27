// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.TypedRow;

namespace Portamical.DataProviders.Models.TypedRow;

public abstract class TestDataProvider<TTestData, TRow>
: DistinctDataProviderBase<TTestData, TRow>,
ITestDataProvider<TTestData, TRow> 
where TTestData : notnull, ITestData
{
    private TestDataProvider()
    {
    }

    protected TestDataProvider(ArgsCode argsCode, string? testMethodName)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        TestMethodName = testMethodName;
    }

    protected TestDataProvider(TTestData testData, ArgsCode argsCode, string? testMethodName)
    : base(testData)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        TestMethodName = testMethodName;
    }

    protected TestDataProvider(IEnumerable<TTestData> testDataCollection, ArgsCode argsCode, string? testMethodName)
    : base(testDataCollection)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        TestMethodName = testMethodName;
    }

    public ArgsCode ArgsCode { get; init; }

    public string? TestMethodName { get; init; }
}
