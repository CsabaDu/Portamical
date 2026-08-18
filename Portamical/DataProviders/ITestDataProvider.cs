// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders;

/// <summary>
/// Defines a contract for providing and managing collections of test data rows with associated test method metadata.
/// </summary>
/// <remarks>
/// <para>
/// This interface serves as the foundation for test data providers across multiple testing frameworks
/// (MSTest, NUnit, xUnit v2/v3, TUnit). Implementations manage collections of test data and provide
/// framework-specific conversion and enumeration capabilities.
/// </para>
/// <para>
/// <strong>Contravariance Support:</strong>
/// </para>
/// <para>
/// The type parameter is marked as contravariant (<c>in TTestData</c>) to enable flexible provider assignments.
/// This allows a provider that accepts base types to be used where derived types are expected:
/// </para>
/// <code>
/// // Provider accepts base type ITestData
/// ITestDataProvider&lt;ITestData&gt; baseProvider = ...;
/// 
/// // Can be assigned to variable expecting derived type
/// ITestDataProvider&lt;TestDataReturns&lt;int&gt;&gt; derivedProvider = baseProvider;
/// 
/// // This works because TestDataReturns&lt;int&gt; : ITestData
/// // The provider can accept any ITestData, including derived types
/// </code>
/// <para>
/// <strong>Design Pattern:</strong> Builder pattern where test data is added incrementally via
/// <see cref="AddRow"/> and the provider manages conversion to framework-specific formats.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Implementations are not required to be thread-safe. Providers
/// are typically constructed and populated during test discovery/initialization, then used read-only
/// during test execution.
/// </para>
/// </remarks>
/// <typeparam name="TTestData">
/// The type of test data row managed by the provider. Must implement <see cref="ITestData"/> and cannot be null.
/// Due to contravariance, the provider can accept this type or any derived type.
/// </typeparam>
public interface ITestDataProvider<in TTestData>
: IEnumerable
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Adds a new row of test data to the provider's collection.
    /// </summary>
    /// <param name="testData">
    /// The test data to add as a new row. The <c>notnull</c> constraint ensures this parameter
    /// cannot be <see langword="null"/> when nullable reference types are enabled.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method follows the builder pattern, allowing incremental construction of the test data collection.
    /// Implementations typically store the row internally and make it available through framework-specific
    /// enumeration mechanisms (e.g., <see cref="System.Collections.IEnumerable"/> for xUnit v2).
    /// </para>
    /// <para>
    /// <strong>Deduplication:</strong> Some implementations may deduplicate rows based on test case identity
    /// (via <see cref="INamedCase.TestCaseName"/>), though this is not required by the interface contract.
    /// Consult specific implementation documentation for deduplication behavior.
    /// </para>
    /// <para>
    /// <strong>Call Order:</strong> Rows are typically added in a specific order during test discovery,
    /// and implementations should preserve this order when enumerating test data.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Builder pattern usage
    /// var provider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;(testMethodName: "AddTest");
    /// 
    /// provider.AddRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5));
    /// provider.AddRow(new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12));
    /// 
    /// // Provider now contains 2 test cases
    /// </code>
    /// </example>
    void AddRow(TTestData testData);

    void AddRange(IEnumerable<TTestData> testDataCollection);
}

public interface ITestDataProvider<in TTestData, TRow>
: ITestDataConverter<TTestData, TRow>,
ITestDataProvider<TTestData>,
IEnumerable<TRow>
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
}
