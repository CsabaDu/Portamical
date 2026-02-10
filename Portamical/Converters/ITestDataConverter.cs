// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters;

public interface ITestDataConverter<TTestData, TRow>
where TTestData : notnull, ITestData
{
    TRow ConvertRow(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName);
}
