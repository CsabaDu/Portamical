// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.xUnit.Converters;

/// <summary>
/// Provides extension methods for transforming collections of <see cref="ITestData"/> 
/// into framework-ready representations. Acts as a dual-strategy converter supporting 
/// both parameter arrays and custom row formats.
/// </summary>
public static class CollectionConverter
{
    public static TheoryData<TTestData> ToTheoryData<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => [.. testDataCollection.ToDistinctReadOnly()];
}