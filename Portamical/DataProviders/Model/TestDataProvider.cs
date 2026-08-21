// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.Model;

public sealed class TestDataProvider<TTestData>
: DistinctTestDataRegistry,
ITestDataProvider<TTestData>
where TTestData : notnull, ITestData
{
    private readonly List<TTestData> _rows = [];

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

    public TTestData[] GetRows()
    => [.. _rows];
}
