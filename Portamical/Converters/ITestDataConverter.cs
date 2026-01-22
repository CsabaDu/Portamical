// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Strategy;
using Portamical.TestDataTypes;

namespace Portamical.Converters;

public interface ITestDataConverter<TTestData, TRow>
where TTestData : notnull, ITestData
{
    TRow Convert(TTestData testData, ArgsCode argsCode, string? testMethodName);
}
