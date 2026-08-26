// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.TypedRow;

public interface ITestDataProvider<in TTestData, TRow>
: IDataProvider<TTestData, TRow>
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
    /// var instanceProvider = new TestDataProviderBase&lt;TestDataReturns&lt;int&gt;&gt;
    /// {
    ///     ArgsCode = ArgsCode.Instance
    /// };
    /// // Result row: [testDataObject]
    /// // Test signature: void Test(TestDataReturns&lt;int&gt; testData)
    /// 
    /// // Properties mode: Pass flattened arguments
    /// var propsProvider = new TestDataProviderBase&lt;TestDataReturns&lt;int&gt;&gt;
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
}
