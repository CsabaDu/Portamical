// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.NUnit.Converters;
using Portamical.NUnit.TestDataTypes;

namespace Portamical.NUnit.TestBases;

public abstract class TestBase : Portamical.TestBases.TestBase
{
    protected static IReadOnlyCollection<TestCaseTestData<TTestData>> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTestCaseTestDataCollection(argsCode);
}