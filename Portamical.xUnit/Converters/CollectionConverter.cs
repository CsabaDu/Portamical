// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.ObjectArray;
using Portamical.Converters.RowArrays.TestData;

namespace Portamical.xUnit.Converters;

/// <summary>
/// Provides extension methods for batch conversion of Portamical test data collections
/// to xUnit v2 data formats.
/// </summary>
/// <remarks>
/// <para>
/// <strong>⚠️ xUnit v2 Legacy Support:</strong>
/// </para>
/// <para>
/// This class provides two conversion strategies for xUnit v2 (released 2015), which is now in
/// maintenance mode. For new projects, consider using xUnit v3 with <c>Portamical.xUnit_v3</c>.
/// </para>
/// <para>
/// <strong>Conversion Strategies:</strong>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Method</term>
///     <description>Return Type</description>
///     <description>Use Case</description>
///     <description>xUnit Version</description>
///   </listheader>
///   <item>
///     <term><see cref="ToTheoryData{TTestData}"/></term>
///     <description><see cref="TheoryData{T}"/></description>
///     <description>Type-safe generic collection (Shared Style)</description>
///     <description>xUnit v2+</description>
///   </item>
///   <item>
///     <term><see cref="ToTestDataProvider{TTestData}"/></term>
///     <description><see cref="TestDataProvider{TTestData}"/></description>
///     <description>Builder pattern with deduplication</description>
///     <description>xUnit v2 (legacy)</description>
///   </item>
/// </list>
/// <para>
/// <strong>Design Pattern: Extension Method + Facade</strong>
/// </para>
/// <para>
/// Both methods act as facades over Portamical's framework-agnostic collection converters:
/// <list type="bullet">
///   <item><description>
///     <see cref="ToTheoryData{TTestData}"/> wraps <c>Portamical.Converters.ToDistinctArray</c>
///   </description></item>
///   <item><description>
///     <see cref="ToTestDataProvider{TTestData}"/> wraps <c>Portamical.Converters.ToDataProvider</c>
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Deduplication:</strong>
/// </para>
/// <para>
/// Both methods automatically remove duplicate test cases based on <see cref="INamedCase.TestCaseName"/> equality.
/// This prevents running the same test multiple times if test data contains duplicates.
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: ToTheoryData (Type-Safe Generic Collection)</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit.Converters;
/// using Portamical.Core.TestDataTypes;
/// 
/// public class CalculatorTests
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12),
///         new("Add(-1,1)", [-1, 1], 0)
///     ];
///     
///     /// &lt;summary&gt;
///     /// Returns type-safe TheoryData&lt;T&gt; for xUnit v2.
///     /// &lt;/summary&gt;
///     public static TheoryData&lt;TestDataReturns&lt;int&gt;&gt; GetAddTestData()
///     {
///         return AddTestData.ToTheoryData();  // ← Type-safe conversion
///     }
///     
///     [Theory]
///     [MemberData(nameof(GetAddTestData))]
///     public void TestAdd(TestDataReturns&lt;int&gt; testData)
///     {
///         var result = Calculator.Add((int)testData.Args[0], (int)testData.Args[1]);
///         Assert.Equal(testData.Expected, result);
///     }
/// }
/// 
/// // xUnit Test Explorer displays:
/// // ✓ TestAdd(testData: TestDataReturns&lt;Int32&gt;)
/// // ✓ TestAdd(testData: TestDataReturns&lt;Int32&gt;)
/// // ✓ TestAdd(testData: TestDataReturns&lt;Int32&gt;)
/// </code>
/// 
/// <para><strong>Example 2: ToTestDataProvider (Builder Pattern)</strong></para>
/// <code>
/// public class CalculatorTests
/// {
///     public static IEnumerable GetTestDataViaProvider()
///     {
///         var testData = new[]
///         {
///             new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///             new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
///         };
///         
///         // Convert to TestDataProvider (Builder pattern)
///         return testData.ToTestDataProvider(ArgsCode.Instance);
///     }
///     
///     [Theory]
///     [MemberData(nameof(GetTestDataViaProvider))]
///     public void TestAdd(TestDataReturns&lt;int&gt; testData)
///     {
///         var result = Calculator.Add((int)testData.Args[0], (int)testData.Args[1]);
///         Assert.Equal(testData.Expected, result);
///     }
/// }
/// </code>
/// 
/// <para><strong>Example 3: Automatic Deduplication</strong></para>
/// <code>
/// public static TheoryData&lt;TestDataReturns&lt;int&gt;&gt; GetTestDataWithDuplicates()
/// {
///     var testData = new[]
///     {
///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),  // ← Duplicate TestCaseName
///         new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
///     };
///     
///     return testData.ToTheoryData();  // ← Deduplication applied
/// }
/// 
/// [Theory]
/// [MemberData(nameof(GetTestDataWithDuplicates))]
/// public void TestAdd(TestDataReturns&lt;int&gt; testData) { }
/// 
/// // Result: Only 2 test cases run (duplicate "Add(2,3)" removed)
/// </code>
/// </example>
/// <seealso cref="TestDataProvider{TTestData}"/>
/// <seealso cref="TheoryData{T}"/>
/// <seealso cref="ArgsCode"/>
/// <seealso cref="ITestData"/>
public static class CollectionConverter
{
    public static TheoryData<TTestData> ToDistinctTheoryData<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => [.. testDataCollection.ToDistinctRowArray()];

    public static TheoryData<object?[]> ToDistinctTheoryData<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => [.. testDataCollection.ToDistinctRowArray(argsCode)];

    ///// <summary>
    ///// Converts a collection of Portamical test data to xUnit v2's <see cref="TestDataProvider{TTestData}"/>
    ///// with Builder pattern support.
    ///// </summary>
    ///// <typeparam name="TTestData">
    ///// The type of test data, which must implement <see cref="ITestData"/>.
    ///// </typeparam>
    ///// <param name="testDataCollection">
    ///// The collection of Portamical test data to convert.
    ///// </param>
    ///// <param name="argsCode">
    ///// Specifies the test data representation strategy. For xUnit v2, only <see cref="ArgsCode.Instance"/>
    ///// (Shared Style) is supported due to xUnit v2 limitations.
    ///// </param>
    ///// <returns>
    ///// A <see cref="TestDataProvider{TTestData}"/> instance initialized with the first test data item.
    ///// Additional items can be added via <see cref="TestDataProvider{TTestData}.AddRow"/>.
    ///// </returns>
    ///// <remarks>
    ///// <para>
    ///// <strong>Builder Pattern:</strong>
    ///// </para>
    ///// <para>
    ///// <see cref="TestDataProvider{TTestData}"/> implements the Builder pattern, allowing incremental
    ///// construction of test data. This method initializes the provider with the first item, and additional
    ///// items can be added later if needed.
    ///// </para>
    ///// <para>
    ///// <strong>Facade Over Framework-Agnostic Base:</strong>
    ///// </para>
    ///// <para>
    ///// This method delegates to <c>Portamical.Converters.CollectionConverter.ToDataProvider</c>, which is
    ///// framework-agnostic and shared across NUnit, MSTest, and xUnit adapters. This method provides
    ///// xUnit-specific configuration:
    ///// </para>
    ///// <code>
    ///// testDataCollection.ToDataProvider(
    /////     initDataProvider: TestDataConverter.InitTestDataProvider,  // ← xUnit factory method
    /////     argsCode: argsCode,
    /////     testMethodName: null);  // ← xUnit v2 doesn't support custom test names
    ///// </code>
    ///// <para>
    ///// <strong>ArgsCode Support:</strong>
    ///// </para>
    ///// <para>
    ///// Although this method accepts an <paramref name="argsCode"/> parameter, only <see cref="ArgsCode.Instance"/>
    ///// (Shared Style) works correctly with xUnit v2. Using <see cref="ArgsCode.Properties"/> (Native Style)
    ///// will result in parameter count mismatch errors because xUnit v2 lacks the necessary infrastructure
    ///// to flatten <see cref="ITestData"/> properties.
    ///// </para>
    ///// <para>
    ///// <strong>testMethodName Parameter (Unused):</strong>
    ///// </para>
    ///// <para>
    ///// The <c>testMethodName</c> parameter is always passed as <c>null</c> because xUnit v2 does not support
    ///// custom test case names. This parameter exists in the framework-agnostic base method for NUnit/MSTest
    ///// compatibility but has no effect in xUnit v2.
    ///// </para>
    ///// <para>
    ///// <strong>Deduplication:</strong>
    ///// </para>
    ///// <para>
    ///// The framework-agnostic <c>ToDataProvider</c> method automatically removes duplicate test cases based on
    ///// <see cref="INamedCase.TestCaseName"/> equality before creating the provider.
    ///// </para>
    ///// </remarks>
    ///// <example>
    ///// <para><strong>Basic Usage (Shared Style):</strong></para>
    ///// <code>
    ///// var testData = new[]
    ///// {
    /////     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    /////     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
    ///// };
    ///// 
    ///// // Convert to TestDataProvider (Shared Style)
    ///// TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt; provider =
    /////     testData.ToTestDataProvider(ArgsCode.Instance);
    ///// 
    ///// // provider.ArgsCode = ArgsCode.Instance
    ///// // Internal: provider contains 2 rows as object[] { testData }
    ///// </code>
    ///// 
    ///// <para><strong>With Test Method:</strong></para>
    ///// <code>
    ///// public static IEnumerable GetTestDataViaProvider()
    ///// {
    /////     var testData = new[]
    /////     {
    /////         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    /////         new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
    /////     };
    /////     
    /////     return testData.ToTestDataProvider(ArgsCode.Instance);
    ///// }
    ///// 
    ///// [Theory]
    ///// [MemberData(nameof(GetTestDataViaProvider))]
    ///// public void TestAdd(TestDataReturns&lt;int&gt; testData)
    ///// {
    /////     int result = Calculator.Add((int)testData.Args[0], (int)testData.Args[1]);
    /////     Assert.Equal(testData.Expected, result);
    ///// }
    ///// </code>
    ///// 
    ///// <para><strong>Builder Pattern (Adding Rows):</strong></para>
    ///// <code>
    ///// var testData = new[]
    ///// {
    /////     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    /////     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
    ///// };
    ///// 
    ///// var provider = testData.ToTestDataProvider(ArgsCode.Instance);
    ///// 
    ///// // Add more rows dynamically (Builder pattern)
    ///// provider.AddRow(new TestDataReturns&lt;int&gt;("Add(-1,1)", [-1, 1], 0));
    ///// 
    ///// // Final: 3 rows in provider
    ///// </code>
    ///// </example>
    ///// <seealso cref="ToTheoryData{TTestData}"/>
    ///// <seealso cref="TestDataProvider{TTestData}"/>
    ///// <seealso cref="ArgsCode"/>
    ///// <seealso cref="TestDataConverter.InitTestDataProvider{TTestData}"/>
    //public static TestDataProvider<TTestData> ToTestDataProvider<TTestData>(
    //    this IEnumerable<TTestData> testDataCollection,
    //    ArgsCode argsCode)
    //where TTestData : notnull, ITestData
    //=> testDataCollection.ToDataProvider<TestDataProvider<TTestData>, TTestData, object?[]>(
    //    initDataProvider: TestDataConverter.InitTestDataProvider,
    //    argsCode: argsCode);
}