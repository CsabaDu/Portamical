// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.xUnit.Converters;

namespace Portamical.xUnit.TestBases;

public abstract class TestBase : Portamical.TestBases.TestBase
{
    protected static TheoryData<TTestData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryData();
}