// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.ObjectArray;

namespace Portamical.Converters.AsyncEnumerables.ObjectArray;

public static class CollectionConverter
{
    #region ToRowArray

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<object?[]> ToAsyncRowEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToRowArray(argsCode).ToAsyncRowEnumerable();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<object?[]> ToAsyncRowEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToRowArray(argsCode, propsCode).ToAsyncRowEnumerable();

    #endregion

    #region ToDistinctRowArray

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<object?[]> ToDistinctAsyncRowEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(argsCode).ToAsyncRowEnumerable();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<object?[]> ToDistinctAsyncRowEnumerable<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(argsCode, propsCode).ToAsyncRowEnumerable();

    #endregion
}
