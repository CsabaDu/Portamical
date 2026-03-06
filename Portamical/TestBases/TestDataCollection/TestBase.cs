// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;

namespace Portamical.TestBases.TestDataCollection;

/// <summary>
/// Provides an abstract base class for test implementations that require standardized handling of test data
/// collections.
/// </summary>
/// <remarks>Inherit from this class to enable consistent conversion of test data collections into distinct
/// arrays, ensuring that each test scenario operates on unique data elements. This can help prevent issues caused by
/// duplicate test data and supports more reliable and isolated testing.</remarks>
public abstract class TestBase : TestBases.TestBase
{
    protected static IEnumerable<TTestData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray();
}
