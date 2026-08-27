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
: IDataProvider<TRow>, ITestDataRegistry<TTestData>
where TTestData : notnull, ITestData
{
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
