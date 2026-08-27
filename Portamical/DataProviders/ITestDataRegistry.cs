// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders;

public interface ITestDataRegistry<in TTestData>
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
    /// <strong>Deduplication:</strong> Some implementations may deduplicate _rows based on test case identity
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
    /// var provider = new DistinctTestDataRegistry&lt;TestDataReturns&lt;int&gt;&gt;(testMethodName: "AddTest");
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
