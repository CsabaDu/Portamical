// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.xUnit.Converters;

namespace Portamical.xUnit.TestBases;

public abstract class TestBase_TheoryData(ArgsCode argsCode = ArgsCode.Instance)
: TestBase_xUnit(argsCode)
{
    public TheoryData ConvertToTheoryData<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryData(ArgsCode);
}