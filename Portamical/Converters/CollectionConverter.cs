// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Identity;
using Portamical.Identity.Model;
using Portamical.Strategy;
using Portamical.TestDataTypes;

namespace Portamical.Converters;

/// <summary>
/// Provides extension methods for transforming collections of strongly typed <see cref="ITestData"/> 
/// into framework-ready representations. Acts as a dual-strategy converter supporting 
/// both parameter arrays and custom row formats.
/// </summary>
public static class CollectionConverter
{
    public static IReadOnlyCollection<object?[]> Convert<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ConvertDistinct(
        testData => testData.ToArgs(argsCode, propsCode));

    public static IReadOnlyCollection<object?[]> ConvertToArgs<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ConvertDistinct(
        testData => testData.ToArgs(argsCode));

    public static IReadOnlyCollection<TRow> Convert<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> testDataConverter,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ConvertDistinct(
        testData => testDataConverter(
            testData,
            argsCode.Defined(nameof(argsCode)),
            testMethodName));

    public static TDataProvider Convert<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        Func<TRow, TDataProvider> initDataProvider,
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

    private static TRow[] ConvertDistinct<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        var source = Validator.NotNullOrEmpty(
            testDataCollection,
            nameof(testDataCollection));

        ArgumentNullException.ThrowIfNull(
            convertRow,
            nameof(convertRow));

        // Deduplicate based on 'NamedCase' identity/equality semantics
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
        var rows = new List<TRow>();

        foreach (var testData in source)
        {
            if (namedCases.Add(testData))
            {
                rows.Add(convertRow(testData));
            }
        }

        return rows.ToArray();
    }
}