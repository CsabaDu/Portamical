// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.TestData;

/// <summary>
/// Defines a test data provider that passes test data objects unchanged as rows,
/// implementing an identity conversion strategy.
/// </summary>
/// <typeparam name="TTestData">
/// The test data type that implements <see cref="ITestData"/>. Must be a non-nullable reference type.
/// This type serves as both the input and output type for the provider.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface specializes <see cref="IDataProvider{TTestData, TRow}"/> where <c>TConvertedRows = TTestData</c>,
/// meaning the row type is identical to the input test data type. This creates an identity provider
/// where <see cref="IDataProvider{TTestData, TRow}.ConvertRow"/> simply returns the input unchanged.
/// </para>
/// <para>
/// <strong>Use Cases:</strong>
/// </para>
/// <list type="bullet">
///   <item>Strongly-typed test data for xUnit v3's <c>TheoryDataRow&lt;T&gt;</c></item>
///   <item>Direct object passing to test methods that accept the test data type</item>
///   <item>Scenarios where no conversion or flattening is needed</item>
///   <item>Building adapters or intermediate providers that defer conversion</item>
/// </list>
/// <para>
/// <strong>Framework Compatibility:</strong> Best suited for frameworks that support generic typed rows
/// or when test methods are designed to receive the test data object directly.
/// </para>
/// <para>
/// <strong>Implementation Note:</strong> Implementations typically override <see cref="IDataProvider{TTestData, TRow}.ConvertRow"/>
/// with a simple identity function: <c>return testData;</c>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Test data provider with identity conversion
/// public class MyTestDataProvider : ITestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;
/// {
///     public TestDataReturns&lt;int&gt; ConvertRow(TestDataReturns&lt;int&gt; testData)
///         => testData; // Identity conversion
/// }
/// 
/// // Test method receives the test data object directly
/// [Theory]
/// [MemberData(nameof(TestData))]
/// public void MyTest(TestDataReturns&lt;int&gt; testData)
/// {
///     var result = Calculate(testData.Args);
///     Assert.Equal(testData.Expected, result);
/// }
/// </code>
/// </example>
public interface ITestDataProvider<TTestData>
: IDataProvider<TTestData, TTestData>
where TTestData : notnull, ITestData;
