// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders;

/// <summary>
/// Defines a contract for providing and managing test data rows, along with associated command-line arguments and test
/// method metadata.
/// </summary>
/// <typeparam name="TTestData">
/// The type of test data row managed by the provider. Must implement <see cref="ITestData"/> and cannot be null.
/// <para>
/// <strong>Contravariance:</strong> Marked as contravariant (<c>in</c>) to enable using providers that accept
/// base types for variables typed with derived types. For example, a provider implementing
/// <c>ITestDataProvider&lt;ITestData&gt;</c> can be assigned to a variable of type
/// <c>ITestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;</c> because <c>TestDataReturns&lt;int&gt;</c>
/// is derived from <c>ITestData</c>.
/// </para>
/// </typeparam>
public interface ITestDataProvider<in TTestData>
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
