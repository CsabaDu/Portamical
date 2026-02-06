// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;

namespace Portamical.TestBases.ObjectArrayCollection;

public abstract class TestBase : TestBases.TestBase
{
    protected static IEnumerable<object?[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(argsCode);

    protected static IEnumerable<object?[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => Convert(testDataCollection, ArgsCode);
}
