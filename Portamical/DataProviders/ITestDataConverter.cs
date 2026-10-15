// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders;

public interface ITestDataConverter<in TTestData, out TRow>
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Converts a single test data item into a row representation suitable for the target test framework.
    /// </summary>
    /// <param name="testData">
    /// The test data to convert. Cannot be <see langword="null"/> due to the <c>notnull</c>
    /// constraint on <typeparamref name="TTestData"/>. This is the source data that will be
    /// transformed into the target row format.
    /// </param>
    /// <returns>
    /// A row representation of type <typeparamref name="TRow"/>. Common return types include <c>object[]</c>
    /// for xUnit/NUnit/MSTest, or framework-specific types like xUnit v3's <c>TheoryDataRow</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the actual conversion of test data into a row format. The specific conversion
    /// logic is determined by the implementing class.
    /// </para>
    /// <para>
    /// <strong>Integration Pattern:</strong> This method is typically called internally during enumeration:
    /// </para>
    /// <code>
    /// public IEnumerator&lt;TConvertedRows&gt; GetEnumerator()
    /// {
    ///     foreach (var testData in _rows)
    ///     {
    ///         yield return ConvertRow(testData);
    ///     }
    /// }
    /// </code>
    /// <para>
    /// <strong>Stateless Design:</strong> This method should be stateless and depend only on its
    /// parameter. Multiple calls with the same input should produce equivalent results.
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
    /// public object[] ConvertRow(TestDataReturns&lt;int&gt; testData)
    /// {
    ///     return testData.ToArgs();
    /// }
    /// 
    /// var row = ConvertRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5));
    /// // Result: [2, 3, 5]
    /// </code>
    /// </example>
    TRow ConvertRow(TTestData testData);
}