// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;
using Portamical.DataProviders;
using System.Collections;

namespace Portamical.xUnit.DataProviders;

public abstract class TestDataProvider : IEnumerable
{
    internal static TestDataProvider<TTestData> InitTestDataProvider<TTestData>(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => new(testData, argsCode);

    public abstract IEnumerator GetEnumerator();
}

public sealed class TestDataProvider<TTestData>
: TestDataProvider,
ITestDataProvider<TTestData>
where TTestData : notnull, ITestData
{
    private readonly List<object?[]> _dataList = [];

    internal TestDataProvider(TTestData testData, ArgsCode argsCode)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));

        AddRow(testData);
    }

    public ArgsCode ArgsCode { get; init; }
    public string? TestMethodName { get; init; } = null;

    public void AddRow(TTestData testData)
    {
        _dataList.Add(testData.ToArgs(ArgsCode));
    }

    public override IEnumerator GetEnumerator()
    {
        return _dataList.GetEnumerator();
    }
}
