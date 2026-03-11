// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.NUnit.Converters;

namespace Portamical.NUnit.TestBases.TestCaseDataCollection;

public abstract class TestBase : Portamical.TestBases.TestBase
{
    protected static IReadOnlyCollection<TestCaseData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTestCaseDataCollection(argsCode, testMethodName);

    protected static IReadOnlyCollection<TestCaseData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => ConvertAsInstance(Convert, testDataCollection, testMethodName);
}