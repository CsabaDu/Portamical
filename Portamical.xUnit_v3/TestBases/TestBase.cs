// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.Converters;
using Portamical.xUnit_v3.DataProviders.Model;

namespace Portamical.xUnit_v3.TestBases;

/// <summary>
/// Abstract base class for xUnit v3 test fixtures that provides convenient test data conversion methods.
/// </summary>
/// <remarks>
/// <para>
/// <strong>xUnit v3 Integration - Modern Test Framework:</strong>
/// </para>
/// <para>
/// This base class extends <see cref="Portamical.TestBases.TestBase"/> with xUnit v3-specific
/// conversion methods that return <see cref="TheoryTestData{TTestData}"/> (modern collection with
/// builder pattern and automatic deduplication).
/// </para>
/// <para>
/// For legacy xUnit v2 support, use <c>Portamical.xUnit</c> instead, which provides:
/// <list type="bullet">
///   <item><description><c>TestDataProvider&lt;T&gt;</c> (legacy Builder pattern)</description></item>
///   <item><description><c>TheoryData&lt;T&gt;</c> (legacy type-safe collection)</description></item>
///   <item><description>Limited Native Style support (Shared Style only)</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>xUnit v3 Advantages Over xUnit v2:</strong>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Feature</term>
///     <description>xUnit v2 (Legacy)</description>
///     <description>xUnit v3 (Modern - This Class)</description>
///   </listheader>
///   <item>
///     <term>Custom Test Names</term>
///     <description>❌ Not supported</description>
///     <description>✅ Via <c>ITheoryDataRow.TestDisplayName</c></description>
///   </item>
///   <item>
///     <term>Native Style (ArgsCode.Properties)</term>
///     <description>⚠️ Limited support</description>
///     <description>✅ Full support</description>
///   </item>
///   <item>
///     <term>Async API</term>
///     <description>❌ Synchronous only</description>
///     <description>✅ Async-first design</description>
///   </item>
///   <item>
///     <term>Skip/Timeout/Traits</term>
///     <description>⚠️ Via attributes only</description>
///     <description>✅ Per-row configuration</description>
///   </item>
///   <item>
///     <term>Deduplication</term>
///     <description>⚠️ Manual</description>
///     <description>✅ Automatic (HashSet)</description>
///   </item>
/// </list>
/// <para>
/// <strong>Design Pattern: Facade + Template Method</strong>
/// </para>
/// <para>
/// Acts as a facade over <see cref="CollectionConverter"/> extension methods, providing a cleaner API
/// for test data source methods:
/// <list type="bullet">
///   <item><description>
///     <see cref="Convert{TTestData}(IEnumerable{TTestData}, ArgsCode, string?)"/> - 
///     Converts with explicit <see cref="ArgsCode"/> strategy
///   </description></item>
///   <item><description>
///     <see cref="Convert{TTestData}(IEnumerable{TTestData}, string?)"/> - 
///     Converts with default <see cref="ArgsCode.Instance"/> strategy (Shared Style)
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Inheritance Chain:</strong>
/// </para>
/// <code>
/// Portamical.TestBases.TestBase (framework-agnostic base)
///   ↓ inherits
/// Portamical.xUnit_v3.TestBases.TestBase (xUnit v3-specific) ← This class
///   ↓ inherits
/// YourTestClass (concrete test fixture)
/// </code>
/// <para>
/// <strong>Return Type - TheoryTestData&lt;TTestData&gt;:</strong>
/// </para>
/// <para>
/// Unlike xUnit v2 (which returns <c>TestDataProvider&lt;T&gt;</c> or <c>TheoryData&lt;T&gt;</c>), this class
/// returns <see cref="TheoryTestData{TTestData}"/>, which provides:
/// <list type="bullet">
///   <item><description>
///     <strong>Builder Pattern:</strong> Incremental test data construction via <c>AddRow</c>
///   </description></item>
///   <item><description>
///     <strong>Automatic Deduplication:</strong> Duplicate test cases (same <c>TestCaseName</c>) are silently ignored
///   </description></item>
///   <item><description>
///     <strong>Configuration:</strong> <c>ArgsCode</c> and <c>TestMethodName</c> properties control conversion
///   </description></item>
///   <item><description>
///     <strong>xUnit v3 Compatibility:</strong> Implements <c>IEnumerable&lt;ITheoryDataRow&gt;</c>
///   </description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Native Style (ArgsCode.Properties) - Recommended for xUnit v3</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit_v3.TestBases;
/// using Portamical.Core.TestDataTypes;
/// 
/// public class CalculatorTests : TestBase
/// {
///     // Define test data
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12),
///         new("Add(-1,1)", [-1, 1], 0)
///     ];
///     
///     /// &lt;summary&gt;
///     /// Test data source for xUnit v3 [MemberData].
///     /// &lt;/summary&gt;
///     /// &lt;returns&gt;TheoryTestData (xUnit v3 modern format).&lt;/returns&gt;
///     public static TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; GetAddTestData()
///     {
///         // Native Style: Convert with explicit ArgsCode.Properties
///         return Convert(AddTestData, ArgsCode.Properties, testMethodName: "TestAdd");
///     }
///     
///     [Theory]
///     [MemberData(nameof(GetAddTestData))]
///     public void TestAdd(int x, int y, int expected)
///     {
///         // Native xUnit style (flattened parameters)
///         int result = Calculator.Add(x, y);
///         Assert.Equal(expected, result);
///     }
/// }
/// 
/// // xUnit v3 Test Explorer displays:
/// // ✓ TestAdd - Add(2,3)
/// // ✓ TestAdd - Add(5,7)
/// // ✓ TestAdd - Add(-1,1)
/// </code>
/// 
/// <para><strong>Example 2: Shared Style (ArgsCode.Instance) - Framework-Agnostic</strong></para>
/// <code>
/// public class CalculatorTests : TestBase
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12)
///     ];
///     
///     public static TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; GetAddTestData()
///     {
///         // Shared Style: Convert with explicit ArgsCode.Instance
///         return Convert(AddTestData, ArgsCode.Instance, testMethodName: null);
///     }
///     
///     [Theory]
///     [MemberData(nameof(GetAddTestData))]
///     public void TestAdd(TestDataReturns&lt;int&gt; testData)
///     {
///         // Framework-agnostic style (entire object)
///         var args = testData.Args;
///         var expected = testData.Expected;
///         
///         int result = Calculator.Add((int)args[0], (int)args[1]);
///         Assert.Equal(expected, result);
///     }
/// }
/// </code>
/// 
/// <para><strong>Example 3: Convenience Overload (Default ArgsCode.Instance)</strong></para>
/// <code>
/// public class CalculatorTests : TestBase
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12)
///     ];
///     
///     public static TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; GetAddTestData()
///     {
///         // Convenience overload - defaults to ArgsCode.Instance
///         return Convert(AddTestData, testMethodName: "TestAdd");
///         //     ^^^^^^^ No ArgsCode parameter
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
/// </code>
/// 
/// <para><strong>Example 4: Builder Pattern (Add Rows After Conversion)</strong></para>
/// <code>
/// public static TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; GetAddTestData()
/// {
///     // Batch convert with builder pattern
///     var data = Convert(AddTestData, ArgsCode.Properties, "TestAdd");
///     
///     // Builder pattern: add more rows after conversion
///     data.AddRow(new TestDataReturns&lt;int&gt;("Add(10,20)", [10, 20], 30));
///     data.AddRow(new TestDataReturns&lt;int&gt;("Add(100,200)", [100, 200], 300));
///     
///     return data;
/// }
/// 
/// // Result: 5 test cases (3 from AddTestData + 2 added dynamically)
/// </code>
/// 
/// <para><strong>Example 5: Automatic Deduplication</strong></para>
/// <code>
/// private static readonly TestDataReturns&lt;int&gt;[] TestDataWithDuplicates =
/// [
///     new("Add(2,3)", [2, 3], 5),
///     new("Add(2,3)", [2, 3], 5),  // ← Duplicate TestCaseName
///     new("Add(5,7)", [5, 7], 12)
/// ];
/// 
/// public static TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; GetTestData()
/// {
///     return Convert(TestDataWithDuplicates, ArgsCode.Properties, "TestAdd");
/// }
/// 
/// // Result: Only 2 test cases (duplicate "Add(2,3)" removed automatically)
/// </code>
/// </example>
/// <seealso cref="Portamical.TestBases.TestBase"/>
/// <seealso cref="TheoryTestData{TTestData}"/>
/// <seealso cref="CollectionConverter"/>
/// <seealso cref="ArgsCode"/>
public abstract class TestBase : Portamical.TestBases.TestBase
{
    /// <summary>
    /// Converts a collection of Portamical test data to xUnit v3's <see cref="TheoryTestData{TTestData}"/>
    /// with explicit argument conversion strategy.
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
    ///     (Shared Style: framework-agnostic test methods)
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments
    ///     (Native Style: idiomatic xUnit test methods) ✅ <strong>Recommended for xUnit v3</strong>
    ///   </description></item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to test case names in display names.
    /// If provided, display names will be formatted as "testMethodName - TestCaseName".
    /// If <c>null</c>, display names will be just "TestCaseName".
    /// </param>
    /// <returns>
    /// A <see cref="TheoryTestData{TTestData}"/> collection with duplicates removed based on
    /// <see cref="INamedCase.TestCaseName"/> equality. Supports builder pattern (adding rows via <c>AddRow</c>).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is a facade over <see cref="CollectionConverter.ToTheoryTestData{TTestData}"/>,
    /// providing a cleaner API for use in test data source methods.
    /// </para>
    /// <para>
    /// <strong>xUnit v3 Modern Collection:</strong>
    /// </para>
    /// <para>
    /// The returned <see cref="TheoryTestData{TTestData}"/> is xUnit v3's modern collection format, providing:
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong>Custom Test Names:</strong> Each row has <c>TestDisplayName</c> property for readable test names
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Builder Pattern:</strong> Add rows incrementally via <c>AddRow</c> method
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Automatic Deduplication:</strong> Duplicate test cases (same <c>TestCaseName</c>) are silently ignored
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Configuration:</strong> <c>ArgsCode</c> and <c>TestMethodName</c> properties control conversion
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Native Style Recommendation (xUnit v3):</strong>
    /// </para>
    /// <para>
    /// Unlike xUnit v2, xUnit v3 has full support for Native Style (<see cref="ArgsCode.Properties"/>).
    /// This is the recommended approach for xUnit v3 because:
    /// <list type="bullet">
    ///   <item><description>Idiomatic xUnit syntax (flattened parameters)</description></item>
    ///   <item><description>Better IntelliSense (parameter names visible in test signature)</description></item>
    ///   <item><description>Cleaner test method signatures</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>vs. xUnit v2:</strong>
    /// </para>
    /// <para>
    /// xUnit v2's <c>TestBase.Convert</c> returns <c>TestDataProvider&lt;T&gt;</c> or <c>TheoryData&lt;T&gt;</c>:
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong>xUnit v2:</strong> Limited Native Style support, no custom test names
    ///   </description></item>
    ///   <item><description>
    ///     <strong>xUnit v3:</strong> Full Native Style support, custom test names via <c>TestDisplayName</c>
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Typical Usage:</strong>
    /// </para>
    /// <code>
    /// public static TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; GetTestData()
    ///     =&gt; Convert(testData, ArgsCode.Properties, "TestAdd");
    /// </code>
    /// </remarks>
    /// <example>
    /// <para><strong>Native Style (ArgsCode.Properties) - Recommended:</strong></para>
    /// <code>
    /// private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
    /// [
    ///     new("Add(2,3)", [2, 3], 5),
    ///     new("Add(5,7)", [5, 7], 12)
    /// ];
    /// 
    /// public static TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; GetAddTestData()
    ///     =&gt; Convert(AddTestData, ArgsCode.Properties, "TestAdd");
    /// 
    /// [Theory]
    /// [MemberData(nameof(GetAddTestData))]
    /// public void TestAdd(int x, int y, int expected)
    /// //                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^ Native Style (flattened)
    /// {
    ///     int result = Calculator.Add(x, y);
    ///     Assert.Equal(expected, result);
    /// }
    /// 
    /// // xUnit v3 Test Explorer:
    /// // ✓ TestAdd - Add(2,3)
    /// // ✓ TestAdd - Add(5,7)
    /// </code>
    /// 
    /// <para><strong>Shared Style (ArgsCode.Instance) - Framework-Agnostic:</strong></para>
    /// <code>
    /// public static TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; GetAddTestData()
    ///     =&gt; Convert(AddTestData, ArgsCode.Instance, null);
    /// 
    /// [Theory]
    /// [MemberData(nameof(GetAddTestData))]
    /// public void TestAdd(TestDataReturns&lt;int&gt; testData)
    /// //                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Shared Style (entire object)
    /// {
    ///     var args = testData.Args;
    ///     var expected = testData.Expected;
    ///     
    ///     int result = Calculator.Add((int)args[0], (int)args[1]);
    ///     Assert.Equal(expected, result);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Convert{TTestData}(IEnumerable{TTestData}, string?)"/>
    /// <seealso cref="CollectionConverter.ToTheoryTestData{TTestData}"/>
    /// <seealso cref="TheoryTestData{TTestData}"/>
    protected static TheoryTestData<TTestData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryTestData(
        argsCode,
        testMethodName);

    /// <summary>
    /// Converts a collection of Portamical test data to xUnit v3's <see cref="TheoryTestData{TTestData}"/>
    /// using the default <see cref="ArgsCode.Instance"/> strategy (Shared Style).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must be a reference type implementing <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to test case names in display names.
    /// If provided, display names will be formatted as "testMethodName - TestCaseName".
    /// If <c>null</c>, display names will be just "TestCaseName".
    /// </param>
    /// <returns>
    /// A <see cref="TheoryTestData{TTestData}"/> collection with duplicates removed based on
    /// <see cref="INamedCase.TestCaseName"/> equality. Supports builder pattern (adding rows via <c>AddRow</c>).
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Convenience Overload:</strong>
    /// </para>
    /// <para>
    /// This method is a convenience overload that delegates to the base class
    /// <see cref="Portamical.TestBases.TestBase.ConvertAsInstance{TTestData, TResult}(Func{IEnumerable{TTestData}, ArgsCode, string?, TResult}, IEnumerable{TTestData}, string?)"/>
    /// method, which automatically uses <see cref="ArgsCode.Instance"/> (Shared Style).
    /// </para>
    /// <para>
    /// <strong>When to Use:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     Use this overload when you want framework-agnostic test methods (Shared Style)
    ///   </description></item>
    ///   <item><description>
    ///     Use <see cref="Convert{TTestData}(IEnumerable{TTestData}, ArgsCode, string?)"/> 
    ///     when you want explicit control over <see cref="ArgsCode"/> (Native Style recommended for xUnit v3)
    ///   </description></item>
    /// </list>
    /// <para>
    /// <strong>Delegation Chain:</strong>
    /// </para>
    /// <code>
    /// Convert(testDataCollection, testMethodName)
    ///   ↓ delegates to base class
    /// ConvertAsInstance(Convert, testDataCollection, testMethodName)
    ///   ↓ invokes (with ArgsCode.Instance)
    /// Convert(testDataCollection, ArgsCode.Instance, testMethodName)
    ///   ↓ calls
    /// ToTheoryTestData(ArgsCode.Instance, testMethodName)
    /// </code>
    /// <para>
    /// <strong>Why This Pattern?</strong>
    /// </para>
    /// <para>
    /// The delegation to <see cref="Portamical.TestBases.TestBase.ConvertAsInstance{TTestData, TResult}"/>
    /// ensures that the logic for defaulting to <see cref="ArgsCode.Instance"/> is centralized in the
    /// framework-agnostic base class. This avoids duplication across NUnit, MSTest, and xUnit adapters.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
    /// [
    ///     new("Add(2,3)", [2, 3], 5),
    ///     new("Add(5,7)", [5, 7], 12)
    /// ];
    /// 
    /// // Convenience overload - defaults to ArgsCode.Instance
    /// public static TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; GetAddTestData()
    ///     =&gt; Convert(AddTestData, testMethodName: "TestAdd");
    /// //     ^^^^^^^ No ArgsCode parameter
    /// 
    /// [Theory]
    /// [MemberData(nameof(GetAddTestData))]
    /// public void TestAdd(TestDataReturns&lt;int&gt; testData)
    /// {
    ///     // Framework-agnostic test method (Shared Style)
    ///     var args = testData.Args;
    ///     var expected = testData.Expected;
    ///     
    ///     int result = Calculator.Add((int)args[0], (int)args[1]);
    ///     Assert.Equal(expected, result);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Convert{TTestData}(IEnumerable{TTestData}, ArgsCode, string?)"/>
    /// <seealso cref="Portamical.TestBases.TestBase.ConvertAsInstance{TTestData, TResult}"/>
    /// <seealso cref="ArgsCode.Instance"/>
    protected static TheoryTestData<TTestData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => ConvertAsInstance(Convert, testDataCollection, testMethodName);
}