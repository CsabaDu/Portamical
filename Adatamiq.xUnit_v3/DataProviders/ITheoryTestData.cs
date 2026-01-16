// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.xUnit_v3.TestDataTypes;

namespace Adatamiq.xUnit_v3.DataProviders;

public interface ITheoryTestData<TTestData>
: ITestDataConverter<TTestData, ITheoryTestDataRow>
where TTestData : notnull, ITestData
{
    ArgsCode ArgsCode { get; }
    string? TestMethodName { get; }
}
