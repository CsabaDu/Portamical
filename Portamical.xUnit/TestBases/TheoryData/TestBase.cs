// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit.Converters;

namespace Portamical.xUnit.TestBases.TheoryData;

public abstract class TestBase : Portamical.TestBases.TestBase
{
    protected static TheoryData<TTestData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryData();
}