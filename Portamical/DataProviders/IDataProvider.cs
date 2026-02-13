// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders;

public interface IDataProvider<TTestData, TRow>
where TTestData : notnull, ITestData
{
    ArgsCode ArgsCode { get; init; }
    string? TestMethodName { get; init; }

    void AddRow(TTestData testData);
}
