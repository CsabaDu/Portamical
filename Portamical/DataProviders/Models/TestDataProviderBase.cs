// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.Models;

public abstract class TestDataProviderBase<TTestData, TRow>
: ITestDataRegistry<TTestData>, IDataProvider<TRow>
where TTestData : notnull, ITestData
{
    protected TestDataProviderBase(TTestData testData, Func<TTestData, TRow> convertRow)
    {
        AddRow(testData, convertRow);
    }

    protected TestDataProviderBase(IEnumerable<TTestData> testDataCollection)
    {
        AddRange(testDataCollection);
    }

    private readonly HashSet<INamedCase> _namedCases = new(NamedCase.Comparer);
    private readonly List<TRow> rows = [];

    public IDictionary<string, TRow> NamedDataRows { get; } = new Dictionary<string, TRow>();

    protected void AddRow(
        TTestData testData,
        Func<TTestData, TRow> convertRow)
    => testData.ExecuteIfDistinct(_namedCases,
        action: () => rows.Add(convertRow(testData)));

    public TRow[] GetRows()
    => [.. rows];

    public IEnumerator<TRow> GetEnumerator()
    => rows.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    => GetEnumerator();

    public abstract void AddRow(TTestData testData);

    public void AddRange(IEnumerable<TTestData> testDataCollection)
    {
        _ = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));

        foreach (var testData in testDataCollection)
        {
            AddRow(testData);
        }
    }
}
