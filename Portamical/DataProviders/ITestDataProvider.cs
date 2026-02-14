// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders;

/// <summary>
/// Defines a contract for providing test data and managing test data rows for parameterized tests.
/// </summary>
/// <typeparam name="TTestData">The type of test data to be added. Must implement the ITestData interface and cannot be null.</typeparam>
/// <typeparam name="TRow">The type representing a single row of test data.</typeparam>
public interface ITestDataProvider<TTestData>
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Gets the parsed command-line arguments for the current operation.
    /// </summary>
    ArgsCode ArgsCode { get; init; }

    /// <summary>
    /// Gets the name of the test method associated with this instance, if any.
    /// </summary>
    string? TestMethodName { get; init; }

    /// <summary>
    /// Adds a new row of test data to the collection.
    /// </summary>
    /// <param name="testData">The test data to add as a new row. Cannot be null.</param>
    void AddRow(TTestData testData);
}
