// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.TestData;

namespace Portamical.DataProviders.Models.TestData;

/// <summary>
/// Provides a sealed implementation of <see cref="ITestDataProvider{TTestData}"/> that uses identity conversion,
/// passing test data objects unchanged as rows.
/// </summary>
/// <typeparam name="TTestData">
/// The test data type that implements <see cref="ITestData"/>. Must be a non-nullable reference type.
/// This type serves as both the input and output (row) type.
/// </typeparam>
/// <remarks>
/// <para>
/// This is the simplest test data provider implementation, where <see cref="ConvertRow"/> returns the input
/// test data unchanged. It's ideal for scenarios where test methods are designed to receive the test data
/// object directly, or when using frameworks that support strongly-typed test data (e.g., xUnit v3).
/// </para>
/// <para>
/// <strong>Sealed Design:</strong> This class is sealed to prevent further inheritance, as the identity
/// conversion pattern leaves no room for meaningful specialization.
/// </para>
/// <para>
/// <strong>Use Cases:</strong>
/// </para>
/// <list type="bullet">
///   <item>Test methods that accept the test data object as a single parameter</item>
///   <item>xUnit v3 strongly-typed <c>TheoryDataRow&lt;T&gt;</c> scenarios</item>
///   <item>Scenarios where no flattening or conversion is needed</item>
///   <item>Building test data collections for later transformation</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Create provider and add test data
/// var provider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;();
/// provider.AddRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5));
/// provider.AddRow(new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12));
/// 
/// // Test method receives the test data object directly
/// [Theory]
/// [MemberData(nameof(TestData))]
/// public void AddTest(TestDataReturns&lt;int&gt; testData)
/// {
///     var result = Add(testData.Args);
///     Assert.Equal(testData.Expected, result);
/// }
/// </code>
/// </example>
public sealed class TestDataProvider<TTestData>
: DistinctDataProviderBase<TTestData, TTestData>,
ITestDataProvider<TTestData>
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Initializes a new instance with an empty collection of test data.
    /// </summary>
    /// <remarks>
    /// Use this constructor when building the test data collection incrementally via
    /// <see cref="DistinctDataProviderBase{TTestData, TRow}.AddRow"/> or
    /// <see cref="DistinctDataProviderBase{TTestData, TRow}.AddRange"/>.
    /// </remarks>
    public TestDataProvider()
    : base()
    {
    }

    /// <summary>
    /// Initializes a new instance with a single test data item.
    /// </summary>
    /// <param name="testData">
    /// The initial test data to add to the provider.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the test case name is invalid or conflicts with internal state.
    /// </exception>
    /// <remarks>
    /// The test data is added immediately during construction via the base class constructor.
    /// Additional items can be added later using <see cref="DistinctDataProviderBase{TTestData, TRow}.AddRow"/>.
    /// </remarks>
    public TestDataProvider(TTestData testData)
    : base(testData)
    {
    }

    /// <summary>
    /// Initializes a new instance with a collection of test data items.
    /// </summary>
    /// <param name="testDataCollection">
    /// The collection of test data to add to the provider.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="testDataCollection"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the collection is empty or contains duplicate test case names.
    /// </exception>
    /// <remarks>
    /// All items are added during construction via the base class constructor.
    /// Additional items can be added later using <see cref="DistinctDataProviderBase{TTestData, TRow}.AddRange"/>.
    /// </remarks>
    public TestDataProvider(IEnumerable<TTestData> testDataCollection)
    : base(testDataCollection)
    {
    }

    /// <summary>
    /// Converts test data to a row by returning it unchanged (identity conversion).
    /// </summary>
    /// <param name="testData">
    /// The test data to convert.
    /// </param>
    /// <returns>
    /// The same <paramref name="testData"/> instance, unchanged.
    /// </returns>
    /// <remarks>
    /// This method implements the identity function: output equals input. It's called internally
    /// by <see cref="DistinctDataProviderBase{TTestData, TRow}.AddRow"/> during row insertion.
    /// </remarks>
    public override TTestData ConvertRow(TTestData testData)
    => testData;
}
