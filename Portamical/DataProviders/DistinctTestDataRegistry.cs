// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders;

public abstract class DistinctTestDataRegistry
{
    private readonly HashSet<INamedCase> _namedCases = new(NamedCase.Comparer);

    protected void AddRow<TTestData, TRow>(
        TTestData testData,
        Action<TRow> add,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    => testData.AddToDistinct(
        namedCases: _namedCases,
        add: () => add(convertRow(testData)));

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
