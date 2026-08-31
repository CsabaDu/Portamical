// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders;

/// <summary>
/// Defines a registry for managing and enumerating test data rows, supporting incremental
/// construction through builder-pattern methods.
/// </summary>
/// <typeparam name="TTestData">
/// The test data type that implements <see cref="ITestData"/>. Must be a non-nullable reference type.
/// Marked as contravariant (<c>in</c>) to support variance in test data types.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface provides a foundation for test data providers that supply data to test frameworks
/// like xUnit, NUnit, or MSTest. Implementations typically convert and store test data rows internally,
/// making them available through <see cref="IEnumerable"/> for framework-specific enumeration mechanisms.
/// </para>
/// <para>
/// <strong>Builder Pattern:</strong> The <see cref="AddRow"/> and <see cref="AddRange"/> methods
/// support incremental construction of test data collections, allowing fluent or step-by-step population.
/// </para>
/// <para>
/// <strong>Type Variance:</strong> The contravariant type parameter allows assignments like
/// <c>ITestDataRegistry&lt;BaseTestData&gt; = new Registry&lt;DerivedTestData&gt;()</c>.
/// </para>
/// </remarks>
public interface ITestDataRegistry<in TTestData>
: IEnumerable
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Adds a new row of test data to the provider's collection.
    /// </summary>
    /// <param name="testData">
    /// The test data to addRange as a new row. The <c>notnull</c> constraint ensures this parameter
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
    /// var provider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;();
    /// 
    /// provider.AddRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5));
    /// provider.AddRow(new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12));
    /// 
    /// // Provider now contains 2 test cases
    /// </code>
    /// </example>
    void AddRow(TTestData testData);

    /// <summary>
    /// Adds multiple test data rows to the provider's collection in a single operation.
    /// </summary>
    /// <param name="testDataCollection">
    /// The collection of test data items to addRange. Each item will be processed and stored as a row.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method provides batch addition of test data, typically iterating over the collection
    /// and calling <see cref="AddRow"/> for each item. Implementations may validate the collection
    /// before processing (e.g., checking for <see langword="null"/> or empty collections).
    /// </para>
    /// <para>
    /// <strong>Exception Behavior:</strong> If an exception occurs during enumeration (e.g., duplicate
    /// test case names), the behavior depends on the implementation. Some implementations may leave
    /// previously added items from the batch in the collection.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> While this method provides convenience for bulk operations,
    /// it typically does not offer performance advantages over multiple <see cref="AddRow"/> calls
    /// in most implementations.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testDataItems = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12),
    ///     new TestDataReturns&lt;int&gt;("Add(0,0)", [0, 0], 0)
    /// };
    /// 
    /// var provider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;();
    /// provider.AddRange(testDataItems);
    /// 
    /// // Provider now contains 3 test cases
    /// </code>
    /// </example>
    void AddRange(IEnumerable<TTestData> testDataCollection);
}
