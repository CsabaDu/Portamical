// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters;

public static class CollectionConverter
{
    public static IReadOnlyCollection<TTestData> ToDistinctReadOnly<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        testData => testData);

    public static IReadOnlyCollection<object?[]> Convert<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        testData => testData.ToArgs(argsCode));

    public static IReadOnlyCollection<object?[]> Convert<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        testData => testData.ToArgs(argsCode, propsCode));

    public static TDataProvider Convert<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TRow, TDataProvider> initDataProvider,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        Action<TDataProvider, TRow> addRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    {
        var rowCollection = testDataCollection.Convert(
            convertRow,
            argsCode,
            testMethodName);
        var rows = rowCollection.ToArray();

        if (rows.Length == 0)
        {
            throw new InvalidOperationException("No test rows provided.");
        }

        var dataProvider = initDataProvider(rows[0]);

        for (int i = 1; i < rows.Length; i++)
        {
            addRow(dataProvider, rows[i]);
        }

        return dataProvider;
    }

    public static IReadOnlyCollection<TRow> Convert<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        testData => convertRow(
            testData,
            argsCode.Defined(nameof(argsCode)),
            testMethodName));

    private static TRow[] ToDistinctReadOnly<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        _ = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        _ = NotNull(convertRow, nameof(convertRow));

        // Deduplicate based on 'NamedCase' identity/equality semantics
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
        var rows = new List<TRow>();

        foreach (var testData in testDataCollection)
        {
            if (namedCases.Add(testData))
            {
                rows.Add(convertRow(testData));
            }
        }

        return [.. rows];
    }
}