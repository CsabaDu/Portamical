// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders;
using Portamical.xUnit_v3.TestDataTypes;

namespace Portamical.xUnit_v3.DataProviders;

/// <summary>
/// Represents a combined data provider and converter for xUnit v3 theory tests that supports
/// both builder-style test data construction and batch conversion to xUnit v3's <see cref="ITheoryTestDataRow"/> format.
/// </summary>
/// <typeparam name="TTestData">
/// The type of test data, which must be a reference type implementing <see cref="ITestData"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// <strong>xUnit v3 Integration - Modern Test Framework:</strong>
/// </para>
/// <para>
/// This interface combines Portamical's data provider and converter patterns to provide a unified
/// API for working with xUnit v3 theory tests. It extends:
/// <list type="bullet">
///   <item><description>
///     <see cref="ITestDataProvider{TTestData}"/> - Provides builder pattern for incremental test data construction
///     and configuration (ArgsCode, TestMethodName)
///   </description></item>
///   <item><description>
///     <see cref="ITestDataConverter{TTestData, TResult}"/> - Provides conversion methods to transform
///     Portamical test data into xUnit v3's <see cref="ITheoryTestDataRow"/> format
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern: Interface Composition</strong>
/// </para>
/// <para>
/// This interface composes two existing interfaces without adding new members:
/// <code>
/// ITheoryTestData&lt;TTestData&gt;
///   ↓ inherits
/// ITestDataProvider&lt;TTestData&gt; (Portamical)
///   - ArgsCode ArgsCode { get; init; }
///   - string? TestMethodName { get; init; }
///   - void AddRow(TTestData testData)
/// 
/// ITheoryTestData&lt;TTestData&gt;
///   ↓ inherits
/// ITestDataConverter&lt;TTestData, ITheoryTestDataRow&gt; (Portamical)
///   - ITheoryTestDataRow ToData(TTestData, ArgsCode, string?)
///   - IReadOnlyCollection&lt;ITheoryTestDataRow&gt; ToDataCollection(IEnumerable&lt;TTestData&gt;, ArgsCode, string?)
/// </code>
/// </para>
/// <para>
/// <strong>xUnit v3 vs xUnit v2:</strong>
/// </para>
/// <para>
/// This interface is specific to xUnit v3 and represents a modern approach compared to xUnit v2's legacy API:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Feature</term>
///     <description>xUnit v2 (Legacy)</description>
///     <description>xUnit v3 (Modern - This Interface)</description>
///   </listheader>
///   <item>
///     <term>Data Provider Type</term>
///     <description>Class-based (<c>TestDataProvider&lt;T&gt;</c>)</description>
///     <description>Interface-based (<see cref="ITheoryTestData{TTestData}"/>)</description>
///   </item>
///   <item>
///     <term>Return Type</term>
///     <description>Non-generic <c>IEnumerable</c></description>
///     <description>Type-safe <see cref="ITheoryTestDataRow"/></description>
///   </item>
///   <item>
///     <term>Custom Test Names</term>
///     <description>❌ Not supported</description>
///     <description>✅ Via <see cref="ITheoryTestDataRow.TestDisplayName"/></description>
///   </item>
///   <item>
///     <term>Converter Methods</term>
///     <description>❌ No</description>
///     <description>✅ <c>ToData</c>, <c>ToDataCollection</c></description>
///   </item>
///   <item>
///     <term>Native Style Support</term>
///     <description>❌ Limited (Shared Style only)</description>
///     <description>✅ Full (both Shared and Native styles)</description>
///   </item>
/// </list>
/// <para>
/// <strong>Usage Pattern:</strong>
/// </para>
/// <para>
/// Implementations of this interface (e.g., <c>TheoryTestData&lt;TTestData&gt;</c>) provide:
/// <list type="number">
///   <item><description>
///     <strong>Builder Pattern:</strong> <see cref="ITestDataProvider{TTestData}.AddRow"/> for incremental construction
///   </description></item>
///   <item><description>
///     <strong>Configuration:</strong> <see cref="ITestDataProvider{TTestData}.ArgsCode"/> and
///     <see cref="ITestDataProvider{TTestData}.TestMethodName"/> properties
///   </description></item>
///   <item><description>
///     <strong>Single Conversion:</strong> <c>ToData</c> method to convert one <typeparamref name="TTestData"/> to
///     <see cref="ITheoryTestDataRow"/>
///   </description></item>
///   <item><description>
///     <strong>Batch Conversion:</strong> <c>ToDataCollection</c> method to convert multiple test data items
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Type Safety:</strong>
/// </para>
/// <para>
/// The generic constraint <c>where TTestData : notnull, ITestData</c> ensures:
/// <list type="bullet">
///   <item><description><c>notnull</c> - Prevents nullable reference types</description></item>
///   <item><description><c>ITestData</c> - Ensures test data implements Portamical's test data interface</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Builder Pattern Usage</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit_v3.DataProviders;
/// 
/// public class CalculatorTests
/// {
///     public static IEnumerable&lt;ITheoryDataRow&gt; GetAddTestData()
///     {
///         // Create provider with builder pattern
///         ITheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; provider =
///             TheoryTestData.Create(
///                 new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///                 ArgsCode.Properties);
///         
///         // Add rows incrementally (builder pattern)
///         provider.AddRow(new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12));
///         provider.AddRow(new TestDataReturns&lt;int&gt;("Add(-1,1)", [-1, 1], 0));
///         
///         // Convert to xUnit v3 format
///         return provider.ToDataCollection(
///             provider.GetAllTestData(),
///             provider.ArgsCode,
///             provider.TestMethodName);
///     }
///     
///     [Theory]
///     [MemberData(nameof(GetAddTestData))]
///     public void TestAdd(int x, int y, int expected)
///     {
///         int result = Calculator.Add(x, y);
///         Assert.Equal(expected, result);
///     }
/// }
/// 
/// // xUnit v3 Test Explorer displays:
/// // ✓ Add(2,3)   ← Custom test name
/// // ✓ Add(5,7)   ← Custom test name
/// // ✓ Add(-1,1)  ← Custom test name
/// </code>
/// 
/// <para><strong>Example 2: Batch Conversion (No Builder Pattern)</strong></para>
/// <code>
/// public static IEnumerable&lt;ITheoryDataRow&gt; GetAddTestData()
/// {
///     var testDataCollection = new[]
///     {
///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///         new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12),
///         new TestDataReturns&lt;int&gt;("Add(-1,1)", [-1, 1], 0)
///     };
///     
///     // Use converter methods directly (no builder pattern)
///     ITheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; converter =
///         TheoryTestData.CreateConverter();
///     
///     return converter.ToDataCollection(
///         testDataCollection,
///         ArgsCode.Properties,
///         testMethodName: "TestAdd");
/// }
/// 
/// // Result: IReadOnlyCollection&lt;ITheoryTestDataRow&gt; with 3 items
/// </code>
/// 
/// <para><strong>Example 3: Single Test Data Conversion</strong></para>
/// <code>
/// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
/// 
/// ITheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; converter =
///     TheoryTestData.CreateConverter();
/// 
/// // Convert single test data
/// ITheoryTestDataRow row = converter.ToData(
///     testData,
///     ArgsCode.Properties,
///     testMethodName: "TestAdd");
/// 
/// // row.TestDisplayName = "TestAdd - Add(2,3)"
/// // row.GetData() = [2, 3, 5]
/// </code>
/// 
/// <para><strong>Example 4: Configuration Properties</strong></para>
/// <code>
/// ITheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; provider =
///     TheoryTestData.Create(
///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///         ArgsCode.Properties)
///     {
///         TestMethodName = "TestAdd"  // ← Configure display name prefix
///     };
/// 
/// provider.AddRow(new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12));
/// 
/// // All rows will use:
/// // - provider.ArgsCode (ArgsCode.Properties)
/// // - provider.TestMethodName ("TestAdd")
/// </code>
/// 
/// <para><strong>Example 5: Comparison with xUnit v2</strong></para>
/// <code>
/// // xUnit v2 (Legacy):
/// public static IEnumerable GetTestData_xUnitV2()
/// {
///     var provider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;(
///         testData,
///         ArgsCode.Instance);  // ← Only Shared Style works well
///     
///     provider.AddRow(testData2);
///     
///     return provider;  // ← Returns non-generic IEnumerable
///     // Test Explorer shows: TestMethod(testData: TestDataReturns&lt;Int32&gt;) ← Generic
/// }
/// 
/// // xUnit v3 (Modern - this interface):
/// public static IEnumerable&lt;ITheoryDataRow&gt; GetTestData_xUnitV3()
/// {
///     ITheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; provider =
///         TheoryTestData.Create(
///             testData,
///             ArgsCode.Properties);  // ← Native Style supported!
///     
///     provider.AddRow(testData2);
///     
///     return provider.ToDataCollection(...);  // ← Returns IReadOnlyCollection&lt;ITheoryTestDataRow&gt;
///     // Test Explorer shows: Add(2,3) ← Custom test name!
/// }
/// </code>
/// </example>
/// <seealso cref="ITestDataProvider{TTestData}"/>
/// <seealso cref="ITestDataConverter{TTestData, TResult}"/>
/// <seealso cref="ITheoryTestDataRow"/>
/// <seealso cref="ITestData"/>
/// <seealso cref="ArgsCode"/>
public interface ITheoryTestData<TTestData>
: ITestDataProvider<TTestData>,
ITestDataConverter<TTestData, ITheoryTestDataRow>
where TTestData : notnull, ITestData;