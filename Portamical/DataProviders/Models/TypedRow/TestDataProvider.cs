// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.TypedRow;
using static Portamical.Core.Safety.EnumValidator;

namespace Portamical.DataProviders.Models.TypedRow;

public abstract class TestDataProvider<TTestData, TRow>
: TestDataProviderBase<TTestData, TRow>,
DataProviders.TypedRow.ITestDataProvider<TTestData, TRow>
where TTestData : notnull, ITestData
{
    protected TestDataProvider(TTestData testData, Func<TTestData, TRow> convertRow, ArgsCode argsCode, string? testMethodName)
    : base(testData, convertRow: convertRow)
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

    public override void AddRow(TTestData testData)
    => AddRow(testData,
        convertRow: ConvertRow);

    public abstract TRow ConvertRow(TTestData testData);
}
