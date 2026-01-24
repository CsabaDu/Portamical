// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.Strategy;
using Portamical.TestDataTypes;

namespace Portamical.TestBases;

public abstract class TestBase(ArgsCode argsCode = ArgsCode.Instance)
{
    protected ArgsCode ArgsCode { get; init; } =
        argsCode.Defined(nameof(argsCode));

    protected IReadOnlyCollection<object?[]> ConvertToArgs<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(ArgsCode);
}