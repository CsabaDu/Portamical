// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.TestDataTypes;

namespace Portamical.MSTest.TestBases;

public abstract class TestBase_NamedObjectArrays : TestBase_MSTest
{
    protected IReadOnlyCollection<object?[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        ArgsCode,
        PropsCode.All);
}