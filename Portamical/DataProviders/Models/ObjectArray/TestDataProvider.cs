// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.ObjectArray;

namespace Portamical.DataProviders.Models.ObjectArray;

/// <summary>
/// Provides an implementation of <see cref="ITestDataProvider{TTestData}"/> that converts test data
/// into <c>object?[]</c> rows using configurable argument and property conversion strategies.
/// </summary>
/// <typeparam name="TTestData">
/// The test data type that implements <see cref="ITestData"/>. Must be a non-nullable reference type.
/// </typeparam>
/// <remarks>
/// <para>
/// This class is designed for test frameworks that consume <c>object[]</c> rows, including xUnit v2,
/// NUnit, and MSTest. It uses <see cref="ArgsCode"/> and <see cref="PropsCode"/> to control how
/// test data is flattened or packaged into object arrays.
/// </para>
/// <para>
/// <strong>Constructor Accessibility:</strong> The parameterless constructor is marked <c>private</c>
/// to enforce that <see cref="ArgsCode"/> and <see cref="PropsCode"/> are always explicitly specified
/// during construction. This ensures consistent conversion behavior throughout the provider's lifetime.
/// </para>
/// <para>
/// <strong>Conversion Strategies:</strong>
/// </para>
/// <list type="bullet">
///   <item><see cref="ArgsCode.Instance"/> with any <see cref="PropsCode"/> - Returns <c>[testData]</c></item>
///   <item><see cref="ArgsCode.Properties"/> with <see cref="PropsCode"/> - Flattens properties into an array</item>
/// </list>
/// <para>
/// <strong>Framework Compatibility:</strong> The <c>object?[]</c> output is directly compatible with
/// xUnit v2 [MemberData], NUnit [TestCaseSource], and MSTest [DynamicData].
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create provider with instance conversion (pass whole object)
/// var instanceProvider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;(
///     argsCode: ArgsCode.Instance,
///     propsCode: PropsCode.All);
/// 
/// instanceProvider.AddRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5));
/// // Row: [testDataObject]
/// 
/// // Create provider with properties conversion (flatten values)
/// var propsProvider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;(
///     argsCode: ArgsCode.Properties,
///     propsCode: PropsCode.All);
/// 
/// propsProvider.AddRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5));
/// // Row: [2, 3, 5]
/// </code>
/// </example>
public class TestDataProvider<TTestData>
: DistinctDataProviderBase<TTestData, object?[]>,
ITestDataProvider<TTestData>
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Initializes a new instance with an empty collection. Private to enforce explicit
    /// <see cref="ArgsCode"/> and <see cref="PropsCode"/> configuration.
    /// </summary>
    private TestDataProvider()
    : base()
    {
    }

    /// <summary>
    /// Initializes a new instance with specified conversion strategies.
    /// </summary>
    /// <param name="argsCode">
    /// The argument code determining the primary conversion strategy.
    /// </param>
    /// <param name="propsCode">
    /// The properties code determining which properties to include when flattening.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="argsCode"/> or <paramref name="propsCode"/> is undefined or invalid.
    /// </exception>
    /// <remarks>
    /// Use this constructor when building the test data collection incrementally via
    /// <see cref="DistinctDataProviderBase{TTestData, TRow}.AddRow"/> or
    /// <see cref="DistinctDataProviderBase{TTestData, TRow}.AddRange"/>.
    /// </remarks>
    public TestDataProvider(ArgsCode argsCode, PropsCode propsCode)
    : base()
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        PropsCode = propsCode.Defined(nameof(propsCode));
    }

    /// <summary>
    /// Initializes a new instance with a single test data item and specified conversion strategies.
    /// </summary>
    /// <param name="testData">
    /// The initial test data to addRange to the provider.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining the primary conversion strategy.
    /// </param>
    /// <param name="propsCode">
    /// The properties code determining which properties to include when flattening.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="argsCode"/> or <paramref name="propsCode"/> is undefined or invalid,
    /// or if the test case name conflicts.
    /// </exception>
    /// <remarks>
    /// The test data is converted and added immediately during construction.
    /// </remarks>
    public TestDataProvider(TTestData testData, ArgsCode argsCode, PropsCode propsCode)
    : base(testData)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        PropsCode = propsCode.Defined(nameof(propsCode));
    }

    /// <summary>
    /// Initializes a new instance with a collection of test data items and specified conversion strategies.
    /// </summary>
    /// <param name="testDataCollection">
    /// The collection of test data to addRange to the provider.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining the primary conversion strategy.
    /// </param>
    /// <param name="propsCode">
    /// The properties code determining which properties to include when flattening.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="testDataCollection"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="argsCode"/> or <paramref name="propsCode"/> is undefined or invalid,
    /// if the collection is empty, or if it contains duplicate test case names.
    /// </exception>
    /// <remarks>
    /// All items are converted and added during construction.
    /// </remarks>
    public TestDataProvider(IEnumerable<TTestData> testDataCollection, ArgsCode argsCode, PropsCode propsCode)
    : base(testDataCollection)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        PropsCode = propsCode.Defined(nameof(propsCode));
    }

    /// <summary>
    /// Gets the argument code that determines the primary conversion strategy for test data rows.
    /// </summary>
    /// <value>
    /// An <see cref="ArgsCode"/> value set during construction, controlling whether to pass
    /// the entire test data object or flatten its properties.
    /// </value>
    /// <remarks>
    /// This property is immutable after construction (init-only). It works together with
    /// <see cref="PropsCode"/> to determine the final row structure in <see cref="ConvertRow"/>.
    /// </remarks>
    public ArgsCode ArgsCode { get; init; }

    /// <summary>
    /// Gets the properties code that determines which properties to include when flattening test data.
    /// </summary>
    /// <value>
    /// A <see cref="PropsCode"/> value set during construction, controlling which property groups
    /// are included in the flattened row.
    /// </value>
    /// <remarks>
    /// This property is immutable after construction (init-only). It is most relevant when
    /// <see cref="ArgsCode"/> is set to <see cref="ArgsCode.Properties"/>.
    /// </remarks>
    public PropsCode PropsCode { get; init; }

    /// <summary>
    /// Converts test data into an <c>object?[]</c> row using the configured <see cref="ArgsCode"/>
    /// and <see cref="PropsCode"/> strategies.
    /// </summary>
    /// <param name="testData">
    /// The test data to convert.
    /// </param>
    /// <returns>
    /// An <c>object?[]</c> containing either the test data object itself (if <see cref="ArgsCode.Instance"/>)
    /// or flattened property values (if <see cref="ArgsCode.Properties"/>).
    /// </returns>
    /// <remarks>
    /// This method delegates to <see cref="ITestData.ToArgs(ArgsCode, PropsCode)"/> on the test data object,
    /// which performs the actual conversion logic based on the configured strategies.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override object?[] ConvertRow(TTestData testData)
    => testData.ToArgs(ArgsCode, PropsCode);
}
