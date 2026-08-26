// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.Models;

public abstract class DistinctDataProvider<TTestData, TRow>
: IDataProvider<TTestData, TRow>
where TTestData : notnull, ITestData
{
    private readonly Dictionary<string, TRow> _distinctRows = new(StringComparer.Ordinal);

    protected DistinctDataProvider()
    {
    }
        
    protected DistinctDataProvider(TTestData testData)
    {
        AddRow(testData);
    }

    protected DistinctDataProvider(IEnumerable<TTestData> testDataCollection)
    {
        AddRange(testDataCollection);
    }

    public void AddRow(TTestData testData)
    => _distinctRows.Add(testData.TestCaseName, ConvertRow(testData));

    public void AddRange(IEnumerable<TTestData> testDataCollection)
    {
        var snapshot = NotNullOrEmpty(
            testDataCollection, nameof(testDataCollection));

        foreach (var testData in snapshot)
        {
            AddRow(testData);
        }
    }

    public TRow? GetRow(string testCaseName)
    => _distinctRows.TryGetValue(testCaseName ?? string.Empty, out var row) ?
        row
        : default;

    public TRow[] GetRows()
    => [.. _distinctRows.Values];

    public string[] GetTestCaseNames()
    => [.. _distinctRows.Keys];

    public IEnumerator<TRow> GetEnumerator()
    => _distinctRows.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    => GetEnumerator();

    public abstract TRow ConvertRow(TTestData testData);
}
