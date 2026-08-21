// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders;
using static Portamical.Core.Safety.EnumValidator;

namespace Portamical.Shared.DataProviders.Model;

public abstract class TestDataProvider<TTestData, TRow>
: DistinctTestDataRegistry,
ITestDataProvider<TTestData, TRow>
where TTestData : notnull, ITestData
{
    private readonly List<TRow> _rows = [];

    private TestDataProvider(ArgsCode argsCode, string? testMethodName)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        TestMethodName = testMethodName;
    }

    protected TestDataProvider(TTestData testData, ArgsCode argsCode, string? testMethodName)
    : this(argsCode, testMethodName)
    {
        AddRow(testData);
    }

    protected TestDataProvider(IEnumerable<TTestData> testDataCollection, ArgsCode argsCode, string? testMethodName)
    : this(argsCode, testMethodName)
    {
        AddRange(testDataCollection);
    }

    public ArgsCode ArgsCode { get; init; }

    public string? TestMethodName { get; init; }

    public void AddRow(TTestData testData)
    => AddRow(testData,
        add: _rows.Add,
        convertRow: ConvertRow);

    public void AddRange(IEnumerable<TTestData> testDataCollection)
    => AddRange(testDataCollection, AddRow);

    public IEnumerator<TRow> GetEnumerator()
    => _rows.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    => GetEnumerator();

    public TRow[] GetRows()
    => [.. _rows];

    public abstract TRow ConvertRow(TTestData testData);
}

public class TestDataProvider<TTestData>
: TestDataProvider<TTestData, object?[]>
where TTestData : notnull, ITestData
{
    public TestDataProvider(TTestData testData, ArgsCode argsCode)
    : base(testData, argsCode, testMethodName: null)
    {
    }

    public TestDataProvider(IEnumerable<TTestData> testDataCollection, ArgsCode argsCode)
    : base(testDataCollection, argsCode, testMethodName: null)
    {
    }

    public override object?[] ConvertRow(TTestData testData)
    => testData.ToArgs(ArgsCode);
}
