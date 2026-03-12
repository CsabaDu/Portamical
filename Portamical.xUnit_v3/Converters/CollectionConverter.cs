// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.DataProviders.Model;
using Portamical.xUnit_v3.TestDataTypes;

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
///     <term><see cref="ToTheoryTestData{TTestData}"/></term>
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
/// testDataCollection.ToTheoryTestData(...)
///   ↓ delegates to
/// ToDataProvider(TestDataConverter.ToTheoryTestData, ...)
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
///     <see cref="ToTheoryTestData{TTestData}"/> - Uses <see cref="TheoryTestData{TTestData}"/>'s internal HashSet
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
/// <strong>Use <see cref="ToTheoryTestData{TTestData}"/> when:</strong>
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
///   <item><description>You want Portamical's row features (TestCaseName, Equals, GetHashCode)</description></item>
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
/// <para><strong>Example 1: ToTheoryTestData (Builder Pattern + Deduplication)</strong></para>
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
///         var data = AddTestData.ToTheoryTestData(
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
///         // Batch convert to Portamical row collection (immutable):
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
///     // Batch convert to xUnit v3 native row collection (lightweight):
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
/// var collection1 = testData.ToTheoryTestData(ArgsCode.Properties, null);
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
    ///     initDataProvider: TestDataConverter.ToTheoryTestData,  // ← xUnit v3 factory
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
    /// var data = testDataCollection.ToTheoryTestData(ArgsCode.Properties, "TestAdd");
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
    ///     testData.ToTheoryTestData(
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
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDataProvider(
        initDataProvider: TestDataConverter.ToTheoryTestData,
        argsCode,
        testMethodName);

    /// <summary>
    /// Converts a collection of Portamical test data to an immutable read-only collection of
    /// <see cref="ITheoryTestDataRow"/> (Portamical rows with <see cref="INamedCase"/> support).
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
    ///     <see cref="ArgsCode.Instance"/> - Pass entire <see cref="ITestData"/> object as single argument
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments
    ///   </description></item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to test case names in display names.
    /// </param>
    /// <returns>
    /// An immutable <see cref="IReadOnlyCollection{T}"/> of <see cref="ITheoryTestDataRow"/> containing
    /// converted test data rows, with duplicates removed based on <see cref="INamedCase.TestCaseName"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Delegation to Framework-Agnostic Base:</strong>
    /// </para>
    /// <para>
    /// This method delegates to <c>Portamical.Converters.CollectionConverter.ToDistinctReadOnly</c>, which is
    /// framework-agnostic and shared across NUnit, MSTest, and xUnit adapters. This method provides
    /// xUnit v3-specific configuration by injecting <see cref="TestDataConverter.ToTheoryTestDataRow{TTestData}"/>
    /// as the row converter:
    /// </para>
    /// <code>
    /// testDataCollection.ToDistinctReadOnly(
    ///     convertRow: TestDataConverter.ToTheoryTestDataRow,  // ← xUnit v3 converter
    ///     argsCode,
    ///     testMethodName);
    /// </code>
    /// <para>
    /// <strong>Immutable Collection:</strong>
    /// </para>
    /// <para>
    /// The returned collection is immutable (<see cref="IReadOnlyCollection{T}"/>), meaning rows cannot be
    /// added or removed after creation. This is different from <see cref="ToTheoryTestData{TTestData}"/>,
    /// which returns a mutable collection with builder pattern support.
    /// </para>
    /// <para>
    /// <strong>Automatic Deduplication:</strong>
    /// </para>
    /// <para>
    /// The underlying <c>ToDistinctReadOnly</c> method automatically removes duplicate test cases using
    /// LINQ's <c>Distinct()</c> with <see cref="INamedCase"/> comparer (equality based on <c>TestCaseName</c>).
    /// </para>
    /// <para>
    /// <strong>Portamical Row Features:</strong>
    /// </para>
    /// <para>
    /// Each row in the collection is of type <see cref="TheoryTestDataRow{TTestData}"/>, which implements:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ITheoryTestDataRow"/> - xUnit v3 compatibility (custom display names, skip, timeout, traits)
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="INamedCase"/> - Test case naming with equality and hashing based on <c>TestCaseName</c>
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>vs. ToTheoryTestData:</strong>
    /// </para>
    /// <para>
    /// Use <see cref="ToTheoryTestData{TTestData}"/> when you need builder pattern (adding rows after batch conversion).
    /// Use <see cref="ToTheoryTestDataRowCollection{TTestData}"/> when you prefer immutable collections.
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
    /// // Batch convert to immutable Portamical row collection:
    /// IReadOnlyCollection&lt;ITheoryTestDataRow&gt; rows =
    ///     testData.ToTheoryTestDataRowCollection(
    ///         ArgsCode.Properties,
    ///         testMethodName: "TestAdd");
    /// 
    /// // rows.Count == 3
    /// // rows is immutable (cannot add/remove rows)
    /// 
    /// // Each row implements ITheoryTestDataRow + INamedCase:
    /// foreach (var row in rows)
    /// {
    ///     string? displayName = row.TestDisplayName;  // "TestAdd - Add(2,3)", etc.
    ///     string testCaseName = row.TestCaseName;     // "Add(2,3)", etc.
    ///     object?[] data = row.GetData();             // [2, 3, 5], etc.
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ToTheoryTestData{TTestData}"/>
    /// <seealso cref="ToTheoryDataRowCollection{TTestData}"/>
    /// <seealso cref="ITheoryTestDataRow"/>
    /// <seealso cref="TestDataConverter.ToTheoryTestDataRow{TTestData}"/>
    public static IReadOnlyCollection<ITheoryTestDataRow> ToTheoryTestDataRowCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        convertRow: TestDataConverter.ToTheoryTestDataRow,
        argsCode,
        testMethodName);

    /// <summary>
    /// Converts a collection of Portamical test data to an immutable read-only collection of
    /// <see cref="ITheoryDataRow"/> (xUnit v3 native rows, lightweight).
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
    ///     <see cref="ArgsCode.Instance"/> - Pass entire <see cref="ITestData"/> object as single argument
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments
    ///   </description></item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to test case names in display names.
    /// </param>
    /// <returns>
    /// An immutable <see cref="IReadOnlyCollection{T}"/> of <see cref="ITheoryDataRow"/> (xUnit v3's built-in type)
    /// containing converted test data rows, with duplicates removed.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>xUnit v3 Native Rows (Lightweight):</strong>
    /// </para>
    /// <para>
    /// This method creates xUnit v3's built-in <see cref="Xunit.v3.TheoryDataRow"/> instead of Portamical's
    /// <see cref="TheoryTestDataRow{TTestData}"/>. The key differences:
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Feature</term>
    ///     <description>Portamical (ToTheoryTestDataRowCollection)</description>
    ///     <description>xUnit Native (ToTheoryDataRowCollection)</description>
    ///   </listheader>
    ///   <item>
    ///     <term>Return Type</term>
    ///     <description><see cref="IReadOnlyCollection{T}"/> of <see cref="ITheoryTestDataRow"/></description>
    ///     <description><see cref="IReadOnlyCollection{T}"/> of <see cref="ITheoryDataRow"/></description>
    ///   </item>
    ///   <item>
    ///     <term>INamedCase</term>
    ///     <description>✅ Yes (deduplication support)</description>
    ///     <description>❌ No (deduplication via underlying conversion)</description>
    ///   </item>
    ///   <item>
    ///     <term>Weight</term>
    ///     <description>Heavier (Portamical features)</description>
    ///     <description>Lighter (xUnit built-in)</description>
    ///   </item>
    /// </list>
    /// <para>
    /// <strong>Delegation to Framework-Agnostic Base:</strong>
    /// </para>
    /// <para>
    /// This method delegates to <c>Portamical.Converters.CollectionConverter.ToDistinctReadOnly</c>,
    /// injecting <see cref="TestDataConverter.ToTheoryDataRow{TTestData}"/> as the row converter:
    /// </para>
    /// <code>
    /// testDataCollection.ToDistinctReadOnly(
    ///     convertRow: TestDataConverter.ToTheoryDataRow,  // ← xUnit v3 native converter
    ///     argsCode,
    ///     testMethodName);
    /// </code>
    /// <para>
    /// <strong>Automatic Deduplication:</strong>
    /// </para>
    /// <para>
    /// Although the returned rows don't implement <see cref="INamedCase"/>, deduplication still occurs
    /// during conversion because the underlying conversion process uses <c>ToDistinctReadOnly</c>, which
    /// removes duplicates based on <see cref="ITestData.TestCaseName"/> before creating xUnit rows.
    /// </para>
    /// <para>
    /// <strong>When to Use xUnit Native Rows:</strong>
    /// </para>
    /// <para>
    /// Use this method when:
    /// <list type="bullet">
    ///   <item><description>You don't need Portamical's <see cref="INamedCase"/> features</description></item>
    ///   <item><description>You want lightweight xUnit v3 integration</description></item>
    ///   <item><description>You prefer xUnit v3's built-in types</description></item>
    /// </list>
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
    /// // Batch convert to xUnit v3 native row collection (lightweight):
    /// IReadOnlyCollection&lt;ITheoryDataRow&gt; rows =
    ///     testData.ToTheoryDataRowCollection(
    ///         ArgsCode.Properties,
    ///         testMethodName: "TestAdd");
    /// 
    /// // rows.Count == 3
    /// // rows is immutable (cannot add/remove rows)
    /// 
    /// // Each row is xUnit's built-in TheoryDataRow:
    /// foreach (var row in rows)
    /// {
    ///     string? displayName = row.TestDisplayName;  // "TestAdd - Add(2,3)", etc.
    ///     object?[] data = row.GetData();             // [2, 3, 5], etc.
    ///     
    ///     // No INamedCase members available (not implemented)
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ToTheoryTestData{TTestData}"/>
    /// <seealso cref="ToTheoryTestDataRowCollection{TTestData}"/>
    /// <seealso cref="ITheoryDataRow"/>
    /// <seealso cref="TestDataConverter.ToTheoryDataRow{TTestData}"/>
    public static IReadOnlyCollection<ITheoryDataRow> ToTheoryDataRowCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        convertRow: TestDataConverter.ToTheoryDataRow,
        argsCode,
        testMethodName);
}