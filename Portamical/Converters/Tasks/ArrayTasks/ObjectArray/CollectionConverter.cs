// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.ObjectArray;

namespace Portamical.Converters.Tasks.ArrayTasks.ObjectArray;

public static class CollectionConverter
{
    #region ToRowArray

    public static Task<object?[][]> ToArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToRowArray(argsCode));

    public static Task<object?[][]> ToArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToRowArray(argsCode, propsCode));

    #endregion

    #region ToDistinctRowArray

    public static Task<object?[][]> ToDistinctArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToDistinctRowArray(argsCode));

    public static Task<object?[][]> ToDistinctArrayTask<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToConvertedRowsTask(
        tdc => tdc.ToDistinctRowArray(argsCode, propsCode));

    #endregion
}