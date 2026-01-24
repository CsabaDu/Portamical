// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.Core.TestDataTypes;

namespace Portamical.MSTest.TestBases;

public abstract class TestBase(ArgsCode argsCode = ArgsCode.Instance)
: Portamical.TestBases.TestBase(argsCode)
{
    protected IReadOnlyCollection<object?[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        ArgsCode,
        PropsCode.All);
}