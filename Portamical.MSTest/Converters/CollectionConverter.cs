// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.Core.TestDataTypes;

namespace Portamical.MSTest.Converters;

public static class CollectionConverter
{
    public static IReadOnlyCollection<object?[]> ToArgsWithTestCaseName<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        argsCode,
        PropsCode.All);
}