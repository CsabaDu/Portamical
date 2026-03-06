// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;

namespace Portamical.TestBases.ObjectArrayCollection;

/// <summary>
/// Provides an abstract base class for test implementations that require standardized conversion of test data
/// collections into a distinct, read-only format suitable for parameterized testing.
/// </summary>
/// <remarks>Inherit from this class to enable consistent handling and transformation of test data across
/// different test scenarios. The conversion methods facilitate the preparation of test data for use with test
/// frameworks that expect collections of object arrays.</remarks>
public abstract class TestBase : TestBases.TestBase
{
    protected static IEnumerable<object?[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(argsCode);

    protected static IEnumerable<object?[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => Convert(testDataCollection, ArgsCode);
}
