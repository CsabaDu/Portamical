// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders;

namespace Portamical.TUnit.DataProviders;

public interface ITestDataProvider<TTestData>
: IDataProvider<TTestData, TestDataRow<TTestData>>,
IDataSourceAttribute
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Gets the name of the test method associated with this provider instance.
    /// </summary>
    /// <value>
    /// The test method name, or <see langword="null"/> if no specific test method is associated.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is typically set during provider construction via the <c>init</c> accessor
    /// and remains constant throughout the provider's lifetime. It's used by testing frameworks
    /// to associate test data with specific test methods.
    /// </para>
    /// <para>
    /// <strong>Usage:</strong> Framework-specific attributes (e.g., xUnit's <c>[MemberData]</c>,
    /// MSTest's <c>[DynamicData]</c>) may use this value to match test data to the appropriate
    /// test method during discovery and execution.
    /// </para>
    /// </remarks>
    string? TestMethodName { get; init; }

    string[]? Categories { get; init; }
}