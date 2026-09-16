// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.CustomRow;

namespace Portamical.Converters.Tasks.ArrayTasks.CustomRow;

public static class CollectionConverter
{
    #region ToRowArray

    public static Task<TRow[]> ToArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToRowArray(convertRow, testMethodName));

    public static Task<TRow[]> ToArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToRowArray(convertRow, argsCode, testMethodName));

    #endregion

    #region ToDistinctRowArray

    public static Task<TRow[]> ToDistinctArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, string?, TRow> convertRow,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToDistinctRowArray(convertRow, testMethodName));

    public static Task<TRow[]> ToDistinctArrayTask<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToDistinctRowArray(convertRow, argsCode, testMethodName));

    #endregion
}
