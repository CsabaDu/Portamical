// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.Models;

public abstract class TestDataProvider
{
    private readonly HashSet<INamedCase> _namedCases = new(NamedCase.Comparer);

    protected void AddRow<TTestData, TRow>(
        TTestData testData,
        Action<TRow> add,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        if (_namedCases.Add(testData))
        {
            var row = convertRow(testData);
            add(row);
        }
    }

    protected static void AddRange<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        Action<TTestData> addRow)
    where TTestData : notnull, ITestData
    {
        _ = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));

        foreach (var testData in testDataCollection)
        {
            addRow(testData);
        }
    }
}

public class TestDataProvider<TTestData>
: TestDataProvider,
ITestDataProvider<TTestData>,
IEnumerable<TTestData>
where TTestData : notnull, ITestData
{
    private readonly List<TTestData> _rows = [];

    public TestDataProvider()
    {
    }

    public TestDataProvider(TTestData testData)
    {
        AddRow(testData);
    }

    public TestDataProvider(IEnumerable<TTestData> testDataCollection)
    {
        AddRange(testDataCollection);
    }

    public void AddRow(TTestData testData)
    => AddRow(testData,
        add: _rows.Add,
        convertRow: td => td);

    public void AddRange(IEnumerable<TTestData> testDataCollection)
    => AddRange(testDataCollection, AddRow);

    public IEnumerator<TTestData> GetEnumerator()
    => _rows.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    => GetEnumerator();
}

public abstract class TestDataProvider<TTestData, TRow>(ArgsCode argsCode, string? testMethodName)
: TestDataProvider,
ITestDataProvider<TTestData, TRow>
where TTestData : notnull, ITestData
{
    private readonly List<TRow> _rows = [];

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

    public ArgsCode ArgsCode { get; init; } = argsCode.Defined(nameof(argsCode));
    public string? TestMethodName { get; init; } = testMethodName;

    public void AddRow(TTestData testData)
    => AddRow(testData,
        add:_rows.Add,
        convertRow: ConvertRow);

    public void AddRange(IEnumerable<TTestData> testDataCollection)
    => AddRange(testDataCollection, AddRow);

    public IEnumerator<TRow> GetEnumerator()
    => _rows.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    => GetEnumerator();

    public abstract TRow ConvertRow(TTestData testData);
}
