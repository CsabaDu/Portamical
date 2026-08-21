// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders;

namespace Portamical.Shared.DataProviders;

public interface ITestDataProvider<in TTestData, TRow>
: ITestDataRegistry<TTestData>,
IDataProvider<TRow>
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Gets the argument code that determines how test data is converted into row format.
    /// </summary>
    /// <value>
    /// An <see cref="ArgsCode"/> value specifying the conversion strategy:
    /// <list type="bullet">
    ///   <item>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire test data object as a single argument
    ///     (default, object-oriented approach).
    ///   </item>
    ///   <item>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened property values as individual arguments
    ///     (functional style, more explicit parameter lists).
    ///   </item>
    /// </list>
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is typically set during construction via the <c>init</c> accessor and remains
    /// constant throughout the converter's lifetime. It's used internally by <see cref="ConvertRow"/>
    /// to determine how to transform test data into row format.
    /// </para>
    /// <para>
    /// <strong>Conversion Strategies:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <see cref="ArgsCode.Instance"/>: Each row contains the test data object itself.
    ///     Test methods receive a single parameter of type <typeparamref name="TTestData"/>.
    ///   </item>
    ///   <item>
    ///     <see cref="ArgsCode.Properties"/>: Each row contains flattened property values.
    ///     Test methods receive individual parameters matching the test data structure.
    ///   </item>
    /// </list>
    /// <para>
    /// <strong>Framework Compatibility:</strong> This property works across all supported frameworks
    /// (MSTest, NUnit, xUnit v2/v3, TUnit), though the specific row format depends on
    /// <typeparamref name="TRow"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Instance mode: Pass entire test data object
    /// var instanceProvider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;
    /// {
    ///     ArgsCode = ArgsCode.Instance
    /// };
    /// // Result row: [testDataObject]
    /// // Test signature: void Test(TestDataReturns&lt;int&gt; testData)
    /// 
    /// // Properties mode: Pass flattened arguments
    /// var propsProvider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;
    /// {
    ///     ArgsCode = ArgsCode.Properties
    /// };
    /// // Result row: [arg1, arg2, expected]
    /// // Test signature: void Test(int arg1, int arg2, int expected)
    /// </code>
    /// </example>
    ArgsCode ArgsCode { get; init; }

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
