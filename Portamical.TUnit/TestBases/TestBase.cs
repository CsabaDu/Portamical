// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.TUnit.Converters;

namespace Portamical.TUnit.TestBases;

public abstract class TestBase : Portamical.TestBases.TestBase
{
    protected static IAsyncEnumerable<TTestData> ConvertAsync<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToAsyncDistinct();

    protected static Task<TTestData[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctTask();
}
