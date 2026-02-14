// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters;

/// <summary>
/// Defines a method that converts test data into a row representation suitable for use in test frameworks or
/// data-driven testing scenarios.
/// </summary>
/// <remarks>Implementations of this interface are typically used to transform structured test data into a format
/// that can be consumed by test runners or data-driven test methods. The specific structure of the row depends on the
/// requirements of the test framework or the test method signature.</remarks>
/// <typeparam name="TTestData">The type of the test data to convert. Must implement ITestData and cannot be null.</typeparam>
/// <typeparam name="TRow">The type representing a single row of converted test data.</typeparam>
public interface ITestDataConverter<TTestData, TRow>
where TTestData : notnull, ITestData
{
    TRow ConvertRow(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName);
}
