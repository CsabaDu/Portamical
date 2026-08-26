// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders;

public interface IDataProvider<out TRow>
: IEnumerable<TRow>
{
    TRow? GetRow(string testCaseName);

    TRow[] GetRows();

    string[] GetTestCaseNames();
}

public interface IDataProvider<in TTestData, out TRow>
: IDataProvider<TRow>
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

    /// <summary>
    /// Converts a single test data item into a row representation suitable for the target test framework.
    /// </summary>
    /// <param name="testData">
    /// The test data to convert. Cannot be <see langword="null"/> due to the <c>notnull</c>
    /// constraint on <typeparamref name="TTestData"/>. This is the source data that will be
    /// transformed into the target row format according to the <see cref="ArgsCode"/> strategy.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method for which this row is being generated, or <see langword="null"/>
    /// if not applicable. This parameter is typically passed from <see cref="ITestDataAdder{TTestData}.TestMethodName"/>
    /// and can be used for row metadata, logging, or validation purposes.
    /// </param>
    /// <returns>
    /// A row representation of type <typeparamref name="TRow"/>. The specific structure depends on
    /// <see cref="ArgsCode"/> and the target row type. Common return types include <c>object[]</c>
    /// for xUnit/NUnit/MSTest, or framework-specific types like xUnit v3's <c>TheoryDataRow</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the actual conversion of test data into a row format. Implementations
    /// typically use <see cref="ArgsCode"/> (set during construction) to determine the conversion strategy:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <see cref="ArgsCode.Instance"/>: Returns a row containing the test data object itself,
    ///     e.g., <c>new object[] { testData }</c>
    ///   </item>
    ///   <item>
    ///     <see cref="ArgsCode.Properties"/>: Returns a row with flattened property values,
    ///     e.g., <c>testData.ToArgs(ArgsCode.Properties)</c>
    ///   </item>
    /// </list>
    /// <para>
    /// <strong>Integration with ITestDataProvider:</strong> When implementing both interfaces,
    /// this method is typically called internally during enumeration:
    /// </para>
    /// <code>
    /// public IEnumerator&lt;TRow&gt; GetEnumerator()
    /// {
    ///     foreach (var testData in _rows)
    ///     {
    ///         yield return ConvertRow(testData, TestMethodName);
    ///     }
    /// }
    /// </code>
    /// <para>
    /// <strong>Stateless Design:</strong> This method should be stateless and depend only on its
    /// parameters and the <see cref="ArgsCode"/> property. Multiple calls with the same inputs
    /// should produce equivalent results.
    /// </para>
    /// <para>
    /// <strong>Framework Requirements:</strong> The returned row type must match the expectations
    /// of the target test framework. For example, xUnit v2's <c>[MemberData]</c> expects <c>object[]</c>,
    /// while xUnit v3 can use strongly-typed <c>TheoryDataRow</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example implementation for object[] conversion
    /// public object[] ConvertRow(TestDataReturns&lt;int&gt; testData, string? testMethodName)
    /// {
    ///     // Use ArgsCode property set during construction
    ///     return testData.ToArgs(ArgsCode);
    /// }
    /// 
    /// // With ArgsCode.Instance:
    /// var row = ConvertRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5), "AddTest");
    /// // Result: [testDataObject]
    /// 
    /// // With ArgsCode.Properties:
    /// var row = ConvertRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5), "AddTest");
    /// // Result: [2, 3, 5]
    /// </code>
    /// </example>
    /// <seealso cref="ArgsCode"/>
    /// <seealso cref="ITestDataAdder{TTestData}"/>
    TRow ConvertRow(TTestData testData);
}
