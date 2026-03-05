// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;
using Portamical.DataProviders;

namespace Portamical.xUnit.DataProviders;

public abstract class TheoryTestData
: TheoryData
{
    internal static TheoryTestData<TTestData> InitTheoryTestData<TTestData>(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName = null)
        where TTestData : notnull, ITestData
    => new(testData, argsCode);
}


public sealed class TheoryTestData<TTestData>
: TheoryTestData,
ITestDataProvider<TTestData>
where TTestData : notnull, ITestData
{
    internal TheoryTestData(TTestData testData, ArgsCode argsCode)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        AddRow(testData);
    }

    public ArgsCode ArgsCode { get; init; }
    public string? TestMethodName { get; init; } = null;


    public void AddRow(TTestData testData)
    {
        AddRow(testData.ToArgs(ArgsCode));
    }
}