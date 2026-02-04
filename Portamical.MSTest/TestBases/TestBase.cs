// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Core.TestDataTypes;
using Portamical.MSTest.Converters;

namespace Portamical.MSTest.TestBases;

public abstract class TestBase : Portamical.TestBases.TestData.TestBase
{
    protected static IEnumerable<object?[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArgsWithTestCaseName(argsCode);
}
