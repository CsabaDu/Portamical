// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit.DataProviders;

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
    /// <summary>
    /// Converts a collection of Portamical test data to xUnit v2's type-safe <see cref="TheoryData{T}"/> collection.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <returns>
    /// A <see cref="TheoryData{T}"/> instance containing deduplicated test data items.
    /// Each item in the collection is added to the <see cref="TheoryData{T}"/> as a single-element
    /// <c>object[]</c> array (Shared Style).
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>xUnit v2 TheoryData&lt;T&gt;:</strong>
    /// </para>
    /// <para>
    /// xUnit v2 introduced <see cref="TheoryData{T}"/> as a type-safe alternative to <c>IEnumerable&lt;object[]&gt;</c>.
    /// It's a generic collection that inherits from <c>List&lt;object[]&gt;</c> and provides compile-time type safety:
    /// </para>
    /// <code>
    /// // Traditional xUnit v2 (non-generic):
    /// public static IEnumerable&lt;object[]&gt; GetTestData()
    /// {
    ///     yield return new object[] { testData1 };
    ///     yield return new object[] { testData2 };
    /// }
    /// 
    /// // Modern xUnit v2 (generic - this method returns this):
    /// public static TheoryData&lt;TestDataReturns&lt;int&gt;&gt; GetTestData()
    /// {
    ///     return testDataCollection.ToTheoryData();  // ← Type-safe
    /// }
    /// </code>
    /// <para>
    /// <strong>Shared Style (ArgsCode.Instance):</strong>
    /// </para>
    /// <para>
    /// The returned <see cref="TheoryData{T}"/> uses Shared Style, meaning each test data item is passed
    /// as a complete <see cref="ITestData"/> object to the test method:
    /// </para>
    /// <code>
    /// // Test method signature (Shared Style):
    /// [Theory]
    /// [MemberData(nameof(GetTestData))]
    /// public void TestMethod(TestDataReturns&lt;int&gt; testData)
    /// //                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Receives entire object
    /// {
    ///     var args = testData.Args;
    ///     var expected = testData.Expected;
    ///     // ...
    /// }
    /// </code>
    /// <para>
    /// <strong>Deduplication:</strong>
    /// </para>
    /// <para>
    /// This method automatically removes duplicate test cases based on <see cref="INamedCase.TestCaseName"/> equality
    /// using <c>ToDistinctArray()</c>. This ensures that tests with identical <c>TestCaseName</c> values are only
    /// included once in the result.
    /// </para>
    /// <para>
    /// <strong>Collection Expression (C# 12):</strong>
    /// </para>
    /// <para>
    /// The implementation uses C# 12's collection expression syntax with the spread operator:
    /// </para>
    /// <code>
    /// [.. testDataCollection.ToDistinctArray()]
    /// // Equivalent to:
    /// // var theoryData = new TheoryData&lt;TTestData&gt;();
    /// // foreach (var item in testDataCollection.ToDistinctArray())
    /// //     theoryData.Add(item);
    /// // return theoryData;
    /// </code>
    /// <para>
    /// <strong>CA1825 Suppression:</strong>
    /// </para>
    /// <para>
    /// The <c>#pragma warning disable CA1825</c> suppresses a false positive from the code analyzer.
    /// CA1825 warns about zero-length array allocations, but in this case:
    /// <list type="bullet">
    ///   <item><description>
    ///     The collection expression <c>[..]</c> is optimized by the compiler
    ///   </description></item>
    ///   <item><description>
    ///     <c>ToDistinctArray()</c> already uses <c>Array.Empty&lt;T&gt;()</c> for empty collections
    ///   </description></item>
    ///   <item><description>
    ///     The warning is likely triggered by the collection expression syntax (analyzer limitation)
    ///   </description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Basic Usage:</strong></para>
    /// <code>
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
    /// };
    /// 
    /// TheoryData&lt;TestDataReturns&lt;int&gt;&gt; theoryData = testData.ToTheoryData();
    /// 
    /// // Result: TheoryData&lt;TestDataReturns&lt;int&gt;&gt; with 2 items
    /// // theoryData[0] = new object[] { testData[0] }
    /// // theoryData[1] = new object[] { testData[1] }
    /// </code>
    /// 
    /// <para><strong>With Test Method:</strong></para>
    /// <code>
    /// public static TheoryData&lt;TestDataReturns&lt;int&gt;&gt; GetAddTestData()
    /// {
    ///     var testData = new[]
    ///     {
    ///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///         new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
    ///     };
    ///     
    ///     return testData.ToTheoryData();
    /// }
    /// 
    /// [Theory]
    /// [MemberData(nameof(GetAddTestData))]
    /// public void TestAdd(TestDataReturns&lt;int&gt; testData)
    /// {
    ///     int result = Calculator.Add((int)testData.Args[0], (int)testData.Args[1]);
    ///     Assert.Equal(testData.Expected, result);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ToTestDataProvider{TTestData}"/>
    /// <seealso cref="TheoryData{T}"/>
    public static TheoryData<TTestData> ToTheoryData<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
#pragma warning disable CA1825 // Avoid zero-length array allocations
        // Justification:
        // This warning is a false positive triggered by the collection expression syntax [..].
        // The underlying ToDistinctArray() method already uses Array.Empty<T>() for empty collections,
        // so no zero-length array allocation occurs. The C# 12 collection expression [.. source]
        // is optimized by the compiler and does not allocate unnecessary arrays.
        // 
        // If testDataCollection is empty:
        // - ToDistinctArray() returns Array.Empty<TTestData>()
        // - Collection expression [.. Array.Empty<TTestData>()] returns TheoryData<TTestData> { }
        // - No zero-length array is explicitly allocated
        => [.. testDataCollection.ToDistinctArray()];
#pragma warning restore CA1825 // Avoid zero-length array allocations

    /// <summary>
    /// Converts a collection of Portamical test data to xUnit v2's <see cref="TestDataProvider{TTestData}"/>
    /// with Builder pattern support.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <param name="argsCode">
    /// Specifies the test data representation strategy. For xUnit v2, only <see cref="ArgsCode.Instance"/>
    /// (Shared Style) is supported due to xUnit v2 limitations.
    /// </param>
    /// <returns>
    /// A <see cref="TestDataProvider{TTestData}"/> instance initialized with the first test data item.
    /// Additional items can be added via <see cref="TestDataProvider{TTestData}.AddRow"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Builder Pattern:</strong>
    /// </para>
    /// <para>
    /// <see cref="TestDataProvider{TTestData}"/> implements the Builder pattern, allowing incremental
    /// construction of test data. This method initializes the provider with the first item, and additional
    /// items can be added later if needed.
    /// </para>
    /// <para>
    /// <strong>Facade Over Framework-Agnostic Base:</strong>
    /// </para>
    /// <para>
    /// This method delegates to <c>Portamical.Converters.CollectionConverter.ToDataProvider</c>, which is
    /// framework-agnostic and shared across NUnit, MSTest, and xUnit adapters. This method provides
    /// xUnit-specific configuration:
    /// </para>
    /// <code>
    /// testDataCollection.ToDataProvider(
    ///     initDataProvider: TestDataConverter.InitTestDataProvider,  // ← xUnit factory method
    ///     argsCode: argsCode,
    ///     testMethodName: null);  // ← xUnit v2 doesn't support custom test names
    /// </code>
    /// <para>
    /// <strong>ArgsCode Support:</strong>
    /// </para>
    /// <para>
    /// Although this method accepts an <paramref name="argsCode"/> parameter, only <see cref="ArgsCode.Instance"/>
    /// (Shared Style) works correctly with xUnit v2. Using <see cref="ArgsCode.Properties"/> (Native Style)
    /// will result in parameter count mismatch errors because xUnit v2 lacks the necessary infrastructure
    /// to flatten <see cref="ITestData"/> properties.
    /// </para>
    /// <para>
    /// <strong>testMethodName Parameter (Unused):</strong>
    /// </para>
    /// <para>
    /// The <c>testMethodName</c> parameter is always passed as <c>null</c> because xUnit v2 does not support
    /// custom test case names. This parameter exists in the framework-agnostic base method for NUnit/MSTest
    /// compatibility but has no effect in xUnit v2.
    /// </para>
    /// <para>
    /// <strong>Deduplication:</strong>
    /// </para>
    /// <para>
    /// The framework-agnostic <c>ToDataProvider</c> method automatically removes duplicate test cases based on
    /// <see cref="INamedCase.TestCaseName"/> equality before creating the provider.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Basic Usage (Shared Style):</strong></para>
    /// <code>
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
    /// };
    /// 
    /// // Convert to TestDataProvider (Shared Style)
    /// TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt; provider =
    ///     testData.ToTestDataProvider(ArgsCode.Instance);
    /// 
    /// // provider.ArgsCode = ArgsCode.Instance
    /// // Internal: provider contains 2 rows as object[] { testData }
    /// </code>
    /// 
    /// <para><strong>With Test Method:</strong></para>
    /// <code>
    /// public static IEnumerable GetTestDataViaProvider()
    /// {
    ///     var testData = new[]
    ///     {
    ///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///         new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
    ///     };
    ///     
    ///     return testData.ToTestDataProvider(ArgsCode.Instance);
    /// }
    /// 
    /// [Theory]
    /// [MemberData(nameof(GetTestDataViaProvider))]
    /// public void TestAdd(TestDataReturns&lt;int&gt; testData)
    /// {
    ///     int result = Calculator.Add((int)testData.Args[0], (int)testData.Args[1]);
    ///     Assert.Equal(testData.Expected, result);
    /// }
    /// </code>
    /// 
    /// <para><strong>Builder Pattern (Adding Rows):</strong></para>
    /// <code>
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
    /// };
    /// 
    /// var provider = testData.ToTestDataProvider(ArgsCode.Instance);
    /// 
    /// // Add more rows dynamically (Builder pattern)
    /// provider.AddRow(new TestDataReturns&lt;int&gt;("Add(-1,1)", [-1, 1], 0));
    /// 
    /// // Final: 3 rows in provider
    /// </code>
    /// </example>
    /// <seealso cref="ToTheoryData{TTestData}"/>
    /// <seealso cref="TestDataProvider{TTestData}"/>
    /// <seealso cref="ArgsCode"/>
    /// <seealso cref="TestDataConverter.InitTestDataProvider{TTestData}"/>
    public static TestDataProvider<TTestData> ToTestDataProvider<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDataProvider(
        initDataProvider: TestDataConverter.InitTestDataProvider,
        argsCode: argsCode,
        testMethodName: null);
}