// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.TypedRow;

/// <summary>
/// Defines a test data provider that converts test data into custom typed rows with configurable
/// argument conversion strategy and test method association.
/// </summary>
/// <typeparam name="TTestData">
/// The test data type that implements <see cref="ITestData"/>. Must be a non-nullable reference type.
/// Marked as contravariant (<c>in</c>) to support variance in test data types.
/// </typeparam>
/// <typeparam name="TRow">
/// The custom row type produced by the provider. This can be any type suitable for the target test framework,
/// such as xUnit v3's <c>TheoryDataRow&lt;T1, T2, ...&gt;</c>, custom tuple types, or record types.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface extends <see cref="IDataProvider{TTestData, TRow}"/> with properties for controlling
/// conversion behavior and associating the provider with a specific test method. It offers the most
/// flexibility among the test data provider interfaces, supporting custom row types while maintaining
/// configuration through <see cref="ArgsCode"/> and <see cref="TestMethodName"/>.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// </para>
/// <list type="bullet">
///   <item>Generic <typeparamref name="TRow"/> type for framework-specific or custom row formats</item>
///   <item><see cref="ArgsCode"/> configuration for instance vs. properties conversion</item>
///   <item><see cref="TestMethodName"/> for test method association and metadata</item>
///   <item>Contravariant <typeparamref name="TTestData"/> for flexible type hierarchies</item>
/// </list>
/// <para>
/// <strong>Framework Compatibility:</strong> The generic <typeparamref name="TRow"/> makes this interface
/// suitable for xUnit v3 (strongly-typed rows), custom test frameworks, or scenarios requiring
/// domain-specific row representations.
/// </para>
/// </remarks>
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
    /// constant throughout the provider's lifetime. It's used internally by <see cref="IDataProvider{TTestData, TRow}.ConvertRow"/>
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
    /// var instanceProvider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;, object[]&gt;
    /// {
    ///     ArgsCode = ArgsCode.Instance
    /// };
    /// // Result row: [testDataObject]
    /// // Test signature: void Test(TestDataReturns&lt;int&gt; testData)
    /// 
    /// // Properties mode: Pass flattened arguments
    /// var propsProvider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;, object[]&gt;
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
