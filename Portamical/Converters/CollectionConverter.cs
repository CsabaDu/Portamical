// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.DataProviders;

namespace Portamical.Converters;

public static class CollectionConverter
{
    public static IReadOnlyCollection<TTestData> ToDistinctReadOnly<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        testData => testData);

    public static IReadOnlyCollection<object?[]> ToDistinctReadOnly<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        testData => testData.ToArgs(argsCode));

    public static IReadOnlyCollection<object?[]> ToDistinctReadOnly<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        testData => testData.ToArgs(argsCode, propsCode));

    public static TDataProvider Convert<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        TDataProvider dataProvider)
    where TTestData : notnull, ITestData
    where TDataProvider : IDataProvider<TTestData, TRow>, ITestDataConverter<TTestData, TRow>

    {
        var testDataArray = ToDistinctReadOnly(
            testDataCollection,
            testData => testData);
        _ = NotNull(dataProvider, nameof(dataProvider));

        foreach (var testData in testDataArray)
        {
            dataProvider.AddRow(testData);
        }

        return dataProvider;
    }

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

        if (rowCollection is not TRow[] rows)
        {
            rows = [.. rowCollection];
        }

        var dataProvider = initDataProvider(rows[0]);

        if (rows.Length > 0)
        {
            for (int i = 1; i < rows.Length; i++)
            {
                addRow(dataProvider, rows[i]);
            }
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