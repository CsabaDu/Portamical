// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.CustomRow;
using Portamical.xUnit_v3.DataProviders.Models;
using Portamical.xUnit_v3.TestDataTypes;
using Portamical.xUnit_v3.TestDataTypes.Model;


namespace Portamical.xUnit_v3.Converters;

/// <summary>
/// Provides extension methods for batch conversion of Portamical test data collections to xUnit v3 data formats.
/// </summary>
/// <remarks>
/// <para>
/// <strong>xUnit v3 Integration - Batch Conversion Strategies:</strong>
/// </para>
/// <para>
/// This class provides three batch conversion strategies for xUnit v3, each optimized for different use cases:
/// <list type="table">
///   <listheader>
///     <term>Method</term>
///     <description>Return Type</description>
///     <description>Use Case</description>
///     <description>Deduplication</description>
///   </listheader>
///   <item>
///     <term><see cref="ToDataProvider{TTestData}"/></term>
///     <description><see cref="TheoryTestData{TTestData}"/></description>
///     <description>Portamical collection with builder pattern</description>
///     <description>✅ Automatic (via HashSet)</description>
///   </item>
///   <item>
///     <term><see cref="ToTheoryTestDataRowCollection{TTestData}"/></term>
///     <description><see cref="IReadOnlyCollection{T}"/> of <see cref="ITheoryTestDataRow"/></description>
///     <description>Portamical rows with <see cref="INamedCase"/></description>
///     <description>✅ Automatic (via Distinct)</description>
///   </item>
///   <item>
///     <term><see cref="ToTheoryDataRowCollection{TTestData}"/></term>
///     <description><see cref="IReadOnlyCollection{T}"/> of <see cref="ITheoryDataRow"/></description>
///     <description>xUnit v3 native rows (lightweight)</description>
///     <description>✅ Automatic (via Distinct)</description>
///   </item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern: Extension Methods + Facade</strong>
/// </para>
/// <para>
/// All methods are extension methods on <see cref="IEnumerable{T}"/> that delegate to framework-agnostic
/// base methods in <c>Portamical.Converters</c>, injecting xUnit v3-specific converters:
/// <code>
/// // Framework-agnostic base (Portamical.Converters):
/// IEnumerable&lt;TTestData&gt;.ToDataProvider(initDataProvider, argsCode, testMethodName)
/// IEnumerable&lt;TTestData&gt;.ToDistinctReadOnly(convertRow, argsCode, testMethodName)
/// 
/// // xUnit v3 facade (this class):
/// testDataCollection.ToDataProvider(...)
///   ↓ delegates to
/// ToDataProvider(TestDataConverter.ToDataProvider, ...)
///   ↓ uses xUnit v3-specific converter
/// TheoryTestData&lt;TTestData&gt;
/// </code>
/// </para>
/// <para>
/// <strong>Automatic Deduplication:</strong>
/// </para>
/// <para>
/// All methods automatically remove duplicate test cases based on <see cref="INamedCase.TestCaseName"/> equality:
/// <list type="bullet">
///   <item><description>
///     <see cref="ToDataProvider{TTestData}"/> - Uses <see cref="TheoryTestData{TTestData}"/>'s internal HashSet
///   </description></item>
///   <item><description>
///     <see cref="ToTheoryTestDataRowCollection{TTestData}"/> - Uses LINQ's <c>Distinct()</c> with <see cref="INamedCase"/> comparer
///   </description></item>
///   <item><description>
///     <see cref="ToTheoryDataRowCollection{TTestData}"/> - Uses LINQ's <c>Distinct()</c> with <see cref="INamedCase"/> comparer
///     (via underlying conversion)
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Choosing the Right Method:</strong>
/// </para>
/// <para>
/// <strong>Use <see cref="ToDataProvider{TTestData}"/> when:</strong>
/// <list type="bullet">
///   <item><description>You need builder pattern (adding rows incrementally after batch conversion)</description></item>
///   <item><description>You want Portamical's collection features (ArgsCode, TestMethodName configuration)</description></item>
///   <item><description>You prefer working with a mutable collection</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Use <see cref="ToTheoryTestDataRowCollection{TTestData}"/> when:</strong>
/// <list type="bullet">
///   <item><description>You need <see cref="INamedCase"/> interface (for manual deduplication or filtering)</description></item>
///   <item><description>You want Portamical's theoryDataRow features (TestCaseName, Equals, GetHashCode)</description></item>
///   <item><description>You prefer immutable read-only collections</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Use <see cref="ToTheoryDataRowCollection{TTestData}"/> when:</strong>
/// <list type="bullet">
///   <item><description>You want lightweight xUnit v3 integration</description></item>
///   <item><description>You don't need Portamical's deduplication features (just display names)</description></item>
///   <item><description>You prefer xUnit v3's built-in types</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: ToDataProvider (Builder Pattern + Deduplication)</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit_v3.Converters;
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
///     public static TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; GetAddTestData()
///     {
///         // Batch convert to Portamical collection:
///         var data = AddTestData.ToDataProvider(
///             ArgsCode.Properties,
///             testMethodName: "TestAdd");
///         
///         // Builder pattern: add more rows after batch conversion
///         data.AddRow(new TestDataReturns&lt;int&gt;("Add(10,20)", [10, 20], 30));
///         
///         return data;  // Returns TheoryTestData&lt;T&gt;
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
/// // ✓ TestAdd - Add(2,3)
/// // ✓ TestAdd - Add(5,7)
/// // ✓ TestAdd - Add(-1,1)
/// // ✓ TestAdd - Add(10,20)
/// </code>
/// 
/// <para><strong>Example 2: ToTheoryTestDataRowCollection (Portamical Rows)</strong></para>
/// <code>
/// public class CalculatorTests
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12)
///     ];
///     
///     public static IReadOnlyCollection&lt;ITheoryTestDataRow&gt; GetAddTestData()
///     {
///         // Batch convert to Portamical theoryDataRow collection (immutable):
///         return AddTestData.ToTheoryTestDataRowCollection(
///             ArgsCode.Properties,
///             testMethodName: "TestAdd");
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
/// </code>
/// 
/// <para><strong>Example 3: ToTheoryDataRowCollection (xUnit v3 Native)</strong></para>
/// <code>
/// public static IReadOnlyCollection&lt;ITheoryDataRow&gt; GetAddTestData()
/// {
///     var testData = new[]
///     {
///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///         new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
///     };
///     
///     // Batch convert to xUnit v3 native theoryDataRow collection (lightweight):
///     return testData.ToTheoryDataRowCollection(
///         ArgsCode.Properties,
///         testMethodName: "TestAdd");
/// }
/// </code>
/// 
/// <para><strong>Example 4: Automatic Deduplication</strong></para>
/// <code>
/// var testData = new[]
/// {
///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),  // ← Duplicate TestCaseName
///     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
/// };
/// 
/// // All methods automatically remove duplicates:
/// var collection1 = testData.ToDataProvider(ArgsCode.Properties, null);
/// // collection1.Count == 2 (duplicate removed)
/// 
/// var collection2 = testData.ToTheoryTestDataRowCollection(ArgsCode.Properties, null);
/// // collection2.Count == 2 (duplicate removed)
/// 
/// var collection3 = testData.ToTheoryDataRowCollection(ArgsCode.Properties, null);
/// // collection3.Count == 2 (duplicate removed)
/// </code>
/// </example>
/// <seealso cref="TheoryTestData{TTestData}"/>
/// <seealso cref="ITheoryTestDataRow"/>
/// <seealso cref="ITheoryDataRow"/>
/// <seealso cref="TestDataConverter"/>
/// <seealso cref="ArgsCode"/>
public static class CollectionConverter
{
    public static ITheoryTestDataRow[] ToDistinctRowArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(
        convertRow: TheoryTestDataRow.From,
        argsCode, testMethodName);

    public static TestDataProvider<TTestData> ToDataProvider<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => new(testDataCollection, argsCode, testMethodName);

    /// <summary>
    /// Converts a collection of Portamical test data to a <see cref="TheoryTestData{TTestData}"/> collection
    /// with builder pattern and automatic deduplication.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must be a reference type implementing <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <param name="argsCode">
    /// Specifies how to convert test data to test method arguments:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire <see cref="ITestData"/> object as single argument (Shared Style)
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments (Native Style)
    ///   </description></item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to test case names in display names.
    /// If provided, display names will be formatted as "testMethodName - TestCaseName".
    /// </param>
    /// <returns>
    /// A <see cref="TheoryTestData{TTestData}"/> collection containing all test data items from
    /// <paramref name="testDataCollection"/>, with duplicates removed based on <see cref="INamedCase.TestCaseName"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Delegation to Framework-Agnostic Base:</strong>
    /// </para>
    /// <para>
    /// This method delegates to <c>Portamical.Converters.CollectionConverter.ToDataProvider</c>, which is
    /// framework-agnostic and shared across NUnit, MSTest, and xUnit adapters. This method provides
    /// xUnit v3-specific configuration by injecting <see cref="TestDataConverter.ToTheoryTestData{TTestData}"/>
    /// as the data provider factory:
    /// </para>
    /// <code>
    /// testDataCollection.ToDataProvider(
    ///     initDataProvider: TestDataConverter.ToDataProvider,  // ← xUnit v3 factory
    ///     argsCode,
    ///     testMethodName);
    /// </code>
    /// <para>
    /// <strong>Builder Pattern Support:</strong>
    /// </para>
    /// <para>
    /// The returned <see cref="TheoryTestData{TTestData}"/> collection supports the builder pattern,
    /// allowing additional rows to be added after batch conversion:
    /// </para>
    /// <code>
    /// var data = testDataCollection.ToDataProvider(ArgsCode.Properties, "TestAdd");
    /// 
    /// // Add more rows using builder pattern:
    /// data.AddRow(new TestDataReturns&lt;int&gt;("Add(10,20)", [10, 20], 30));
    /// </code>
    /// <para>
    /// <strong>Automatic Deduplication:</strong>
    /// </para>
    /// <para>
    /// The returned collection automatically removes duplicate test cases based on <see cref="INamedCase.TestCaseName"/>
    /// using an internal <see cref="HashSet{T}"/>. See <see cref="TheoryTestData{TTestData}.Add"/> for details.
    /// </para>
    /// <para>
    /// <strong>Configuration:</strong>
    /// </para>
    /// <para>
    /// The returned collection is initialized with:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="TheoryTestData{TTestData}.ArgsCode"/> = <paramref name="argsCode"/>
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="TheoryTestData{TTestData}.TestMethodName"/> = <paramref name="testMethodName"/>
    ///   </description></item>
    /// </list>
    /// All rows added via <see cref="TheoryTestData{TTestData}.AddRow"/> will use this configuration.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12),
    ///     new TestDataReturns&lt;int&gt;("Add(-1,1)", [-1, 1], 0)
    /// };
    /// 
    /// // Batch convert to TheoryTestData collection:
    /// TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; collection =
    ///     testData.ToDataProvider(
    ///         ArgsCode.Properties,
    ///         testMethodName: "TestAdd");
    /// 
    /// // collection.Count == 3
    /// // collection.ArgsCode == ArgsCode.Properties
    /// // collection.TestMethodName == "TestAdd"
    /// 
    /// // Builder pattern: add more rows
    /// collection.AddRow(new TestDataReturns&lt;int&gt;("Add(10,20)", [10, 20], 30));
    /// 
    /// // collection.Count == 4
    /// </code>
    /// </example>
    /// <seealso cref="TheoryTestData{TTestData}"/>
    /// <seealso cref="ToTheoryTestDataRowCollection{TTestData}"/>
    /// <seealso cref="TestDataConverter.ToTheoryTestData{TTestData}"/>
    public static TheoryTestData<TTestData> ToTheoryTestData<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => new(testDataCollection, argsCode, testMethodName);

    public static TTheoryData ToDistinctTheoryData<TTestData, TTheoryData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    where TTheoryData : TheoryData, new()
    => testDataCollection.ToDistinctConvertedRows<TTestData, TTheoryData>(
        addConvertedRow: (theoryData, testData) => theoryData.Add(
            [TheoryTestDataRow.From(testData, argsCode, testMethodName)]));
}