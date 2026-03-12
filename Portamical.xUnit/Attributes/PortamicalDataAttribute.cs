// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Globalization;
using System.Reflection;
using Xunit.Sdk;

namespace Portamical.xUnit.Attributes;

/// <summary>
/// Abstract base class for xUnit v2 data attributes that support Portamical test data.
/// </summary>
/// <remarks>
/// <para>
/// <strong>⚠️ xUnit v2 Legacy Support:</strong>
/// </para>
/// <para>
/// This attribute provides compatibility with xUnit v2 (released 2015), which is now in maintenance mode.
/// For new projects, consider using xUnit v3 with <c>Portamical.xUnit_v3</c>, which provides:
/// <list type="bullet">
///   <item><description>Type-safe <c>TheoryData&lt;T&gt;</c> API</description></item>
///   <item><description>Custom test names via <c>ITheoryDataRow.TestDisplayName</c></description></item>
///   <item><description>Support for <see cref="ArgsCode.Properties"/> (Native Style)</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern: Adapter + Template Method</strong>
/// </para>
/// <para>
/// This class adapts xUnit v2's <see cref="MemberDataAttributeBase"/> to support Portamical's
/// <see cref="ITestData"/> interface by overriding the template method <see cref="ConvertDataItem"/>:
/// <list type="bullet">
///   <item><description>
///     Detects <see cref="ITestData"/> instances in data source
///   </description></item>
///   <item><description>
///     Converts them to <c>object[]</c> arrays using <see cref="ArgsCode.Instance"/> (Shared Style)
///   </description></item>
///   <item><description>
///     Passes through existing <c>object[]</c> arrays unchanged
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>xUnit v2 Limitations:</strong>
/// </para>
/// <para>
/// This attribute is limited to <see cref="ArgsCode.Instance"/> (Shared Style) because:
/// <list type="bullet">
///   <item><description>
///     <strong>No Custom Test Names:</strong> xUnit v2 auto-generates test names as <c>MethodName(param1, param2, ...)</c>
///   </description></item>
///   <item><description>
///     <strong>No Native Style Support:</strong> <see cref="ArgsCode.Properties"/> requires custom test case factory
///     (not available in xUnit v2)
///   </description></item>
///   <item><description>
///     <strong>No Per-Attribute Configuration:</strong> Cannot specify <see cref="ArgsCode"/> in attribute parameters
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Data Source Requirements:</strong>
/// </para>
/// <para>
/// The data source method/property must return one of:
/// <list type="bullet">
///   <item><description>
///     <c>IEnumerable&lt;object[]&gt;</c> - Standard xUnit v2 format (pass through)
///   </description></item>
///   <item><description>
///     <c>IEnumerable&lt;ITestData&gt;</c> - Portamical test data (converted to <c>object[]</c>)
///   </description></item>
///   <item><description>
///     <see cref="TestDataProvider{TTestData}"/> - Portamical data provider (implements <c>IEnumerable</c>)
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>xUnit v2 Integration:</strong>
/// </para>
/// <para>
/// This attribute uses xUnit v2's built-in <c>MemberDataDiscoverer</c> via the <c>[DataDiscoverer]</c> attribute,
/// so no custom discoverer is needed. The discoverer:
/// <list type="number">
///   <item><description>Locates the data source method/property via reflection</description></item>
///   <item><description>Invokes it to retrieve <c>IEnumerable&lt;object[]&gt;</c></description></item>
///   <item><description>Calls <see cref="ConvertDataItem"/> for each item</description></item>
///   <item><description>Creates a test case per converted item</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Shared Style (ArgsCode.Instance) - Recommended</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit.Attributes;
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
///     /// Test data source for xUnit v2 [PortamicalData].
///     /// &lt;/summary&gt;
///     /// &lt;returns&gt;Enumerable of ITestData (converted to object[] by attribute).&lt;/returns&gt;
///     public static IEnumerable&lt;ITestData&gt; GetAddTestData()
///     {
///         return AddTestData;  // Returns IEnumerable&lt;ITestData&gt;
///     }
///     
///     [Theory]
///     [PortamicalData(nameof(GetAddTestData))]
///     public void TestAdd(TestDataReturns&lt;int&gt; testData)
///     //                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Receives entire test data object (Shared Style)
///     {
///         // Framework-agnostic test method
///         var args = testData.Args;
///         var expected = testData.Expected;
///         
///         int result = Calculator.Add((int)args[0], (int)args[1]);
///         Assert.Equal(expected, result);
///     }
/// }
/// 
/// // xUnit Test Explorer displays (auto-generated):
/// // ✓ TestAdd(testData: TestDataReturns&lt;Int32&gt;)
/// // ✓ TestAdd(testData: TestDataReturns&lt;Int32&gt;)
/// // ✓ TestAdd(testData: TestDataReturns&lt;Int32&gt;)
/// </code>
/// 
/// <para><strong>Example 2: Using TestDataProvider</strong></para>
/// <code>
/// public class CalculatorTests
/// {
///     public static IEnumerable GetTestDataViaProvider()
///     {
///         var provider = TestDataProvider.From(
///             new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///             ArgsCode.Instance);
///         
///         provider.AddRow(new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12));
///         
///         return provider;  // Returns IEnumerable (non-generic)
///     }
///     
///     [Theory]
///     [PortamicalData(nameof(GetTestDataViaProvider))]
///     public void TestAdd(TestDataReturns&lt;int&gt; testData)
///     {
///         var result = Calculator.Add((int)testData.Args[0], (int)testData.Args[1]);
///         Assert.Equal(testData.Expected, result);
///     }
/// }
/// </code>
/// 
/// <para><strong>Example 3: Mixed Data Sources (object[] + ITestData)</strong></para>
/// <code>
/// public static IEnumerable&lt;object[]&gt; GetMixedTestData()
/// {
///     // Standard xUnit v2 format (pass through)
///     yield return new object[] { 2, 3, 5 };
///     
///     // Portamical test data (converted to object[] by attribute)
///     yield return new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12);
///     //           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
///     //           ConvertDataItem converts this to: new object[] { testData }
/// }
/// 
/// [Theory]
/// [PortamicalData(nameof(GetMixedTestData))]
/// public void TestAdd(object testData)
/// //                  ^^^^^^ Must use object (mixed types)
/// {
///     if (testData is TestDataReturns&lt;int&gt; portamicalData)
///     {
///         // Handle Portamical test data
///         var result = Calculator.Add((int)portamicalData.Args[0], (int)portamicalData.Args[1]);
///         Assert.Equal(portamicalData.Expected, result);
///     }
///     else if (testData is object[] rawArray)
///     {
///         // Handle raw array (not recommended - just for demonstration)
///         var result = Calculator.Add((int)rawArray[0], (int)rawArray[1]);
///         Assert.Equal((int)rawArray[2], result);
///     }
/// }
/// </code>
/// 
/// <para><strong>Example 4: Why Native Style (Properties) Doesn't Work</strong></para>
/// <code>
/// // ❌ This WILL NOT WORK in xUnit v2:
/// [Theory]
/// [PortamicalData(nameof(GetTestData))]
/// public void TestAdd(int x, int y, int expected)
/// //                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^ Native Style signature
/// {
///     // Problem: PortamicalDataAttribute converts ITestData to object[] { testData }
///     //          xUnit v2 tries to map [testData] → (int x, int y, int expected)
///     //          Result: Parameter count mismatch error
///     
///     int result = Calculator.Add(x, y);
///     Assert.Equal(expected, result);
/// }
/// 
/// // ✅ Use Shared Style instead (or wait for xUnit v3 support):
/// [Theory]
/// [PortamicalData(nameof(GetTestData))]
/// public void TestAdd(TestDataReturns&lt;int&gt; testData)
/// {
///     var args = testData.Args;
///     int result = Calculator.Add((int)args[0], (int)args[1]);
///     Assert.Equal(testData.Expected, result);
/// }
/// </code>
/// </example>
/// <seealso cref="PortamicalDataAttribute"/>
/// <seealso cref="MemberDataAttributeBase"/>
/// <seealso cref="ITestData"/>
/// <seealso cref="ArgsCode"/>
/// <seealso cref="TestDataProvider{TTestData}"/>
[DataDiscoverer("Xunit.Sdk.MemberDataDiscoverer", "xunit.core")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class PortamicalBaseDataAttribute(string memberName, object?[]? parameters)
: MemberDataAttributeBase(memberName, parameters)
{
    /// <summary>
    /// Converts a data item from the data source to an <c>object[]</c> array suitable for test method parameters.
    /// </summary>
    /// <param name="testMethod">
    /// The test method that will receive the converted data. Used for error reporting.
    /// </param>
    /// <param name="item">
    /// A single data item from the data source. Can be:
    /// <list type="bullet">
    ///   <item><description><c>null</c> - Passed through as <c>null</c></description></item>
    ///   <item><description><c>object[]</c> - Passed through unchanged (standard xUnit v2 format)</description></item>
    ///   <item><description><see cref="ITestData"/> - Converted to <c>object[]</c> using <see cref="ArgsCode.Instance"/></description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// An <c>object[]</c> array containing the test method parameters, or <c>null</c> if <paramref name="item"/> is <c>null</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Template Method Pattern:</strong>
    /// </para>
    /// <para>
    /// This method overrides <see cref="MemberDataAttributeBase.ConvertDataItem"/> to provide Portamical-specific
    /// data conversion. It is called by xUnit v2's <c>MemberDataDiscoverer</c> for each item returned by the
    /// data source.
    /// </para>
    /// <para>
    /// <strong>Conversion Logic:</strong>
    /// </para>
    /// <code>
    /// item is null          → return null (pass through)
    /// item is object[]      → return item (pass through - already correct format)
    /// item is ITestData     → return testData.ToArgs(ArgsCode.Instance)  ← Convert to [testData]
    /// item is other         → throw ArgumentException (unsupported type)
    /// </code>
    /// <para>
    /// <strong>Why ArgsCode.Instance?</strong>
    /// </para>
    /// <para>
    /// xUnit v2 limitations prevent using <see cref="ArgsCode.Properties"/> (Native Style):
    /// <list type="bullet">
    ///   <item><description>
    ///     No custom test case factory (can't map <c>ITestData</c> → flattened parameters)
    ///   </description></item>
    ///   <item><description>
    ///     No per-attribute configuration (can't specify <see cref="ArgsCode"/> in attribute)
    ///   </description></item>
    ///   <item><description>
    ///     No test name customization (can't use <c>TestCaseName</c> from <see cref="ITestData"/>)
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Shared Style (ArgsCode.Instance) Conversion:</strong>
    /// </para>
    /// <code>
    /// // Input:
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// 
    /// // Conversion:
    /// testData.ToArgs(ArgsCode.Instance)
    /// 
    /// // Output:
    /// new object[] { testData }  ← Single element: entire ITestData instance
    /// 
    /// // Test method receives:
    /// public void TestAdd(TestDataReturns&lt;int&gt; testData) { }
    /// //                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Entire object
    /// </code>
    /// <para>
    /// <strong>Error Handling:</strong>
    /// </para>
    /// <para>
    /// If the data source yields an unsupported type (not <c>null</c>, <c>object[]</c>, or <see cref="ITestData"/>),
    /// this method throws <see cref="ArgumentException"/> with a localized message indicating the member name
    /// and declaring type.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="item"/> is not <c>null</c>, <c>object[]</c>, or <see cref="ITestData"/>.
    /// </exception>
    /// <example>
    /// <para><strong>Supported Data Source Formats:</strong></para>
    /// <code>
    /// // Format 1: IEnumerable&lt;object[]&gt; (standard xUnit v2):
    /// public static IEnumerable&lt;object[]&gt; GetTestData1()
    /// {
    ///     yield return new object[] { 2, 3, 5 };  // ← Passed through
    ///     yield return new object[] { 5, 7, 12 }; // ← Passed through
    /// }
    /// 
    /// // Format 2: IEnumerable&lt;ITestData&gt; (Portamical):
    /// public static IEnumerable&lt;ITestData&gt; GetTestData2()
    /// {
    ///     yield return new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    ///     //           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ///     //           Converted to: new object[] { testData }
    ///     yield return new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12);
    /// }
    /// 
    /// // Format 3: TestDataProvider (Portamical):
    /// public static IEnumerable GetTestData3()
    /// {
    ///     var provider = TestDataProvider.From(
    ///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///         ArgsCode.Instance);
    ///     return provider;  // Returns IEnumerable&lt;object[]&gt; internally
    /// }
    /// </code>
    /// </example>
    protected override object?[]? ConvertDataItem(MethodInfo testMethod, object item)
    => item switch
    {
        null => null,
        object?[] array => array,
        ITestData testData => testData.ToArgs(ArgsCode.Instance),
        _ => throw new ArgumentException(string.Format(
                CultureInfo.CurrentCulture,
                "Property {0} on {1} yielded an item that is not an object[]",
                MemberName,
                MemberType ?? testMethod.DeclaringType)),
    };
}

/// <summary>
/// xUnit v2 data attribute that supports Portamical test data in addition to standard <c>object[]</c> arrays.
/// </summary>
/// <remarks>
/// <para>
/// <strong>⚠️ xUnit v2 Legacy Support:</strong>
/// </para>
/// <para>
/// This attribute is a drop-in replacement for xUnit v2's <c>[MemberData]</c> attribute with automatic
/// support for Portamical's <see cref="ITestData"/> interface. It is limited to <see cref="ArgsCode.Instance"/>
/// (Shared Style) due to xUnit v2 limitations.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>
///     You're using xUnit v2 (legacy) and want Portamical test data support
///   </description></item>
///   <item><description>
///     You're okay with Shared Style (<see cref="ArgsCode.Instance"/>) test methods
///   </description></item>
///   <item><description>
///     You don't need custom test names (xUnit v2 auto-generates them)
///   </description></item>
/// </list>
/// <para>
/// <strong>Migration to xUnit v3:</strong>
/// </para>
/// <para>
/// For new projects or when upgrading to xUnit v3, use <c>Portamical.xUnit_v3</c> instead, which provides:
/// <list type="bullet">
///   <item><description>Type-safe <c>TheoryData&lt;T&gt;</c> API</description></item>
///   <item><description>Custom test names via <c>ITheoryDataRow.TestDisplayName</c></description></item>
///   <item><description>Support for <see cref="ArgsCode.Properties"/> (Native Style)</description></item>
///   <item><description>Better IDE tooling and test discovery</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Usage Pattern:</strong>
/// </para>
/// <code>
/// // Replace:
/// [MemberData(nameof(GetTestData))]
/// 
/// // With:
/// [PortamicalData(nameof(GetTestData))]
/// //               ^^^^^^^^^^^^^^^^^^^^^
/// //               Same parameters as [MemberData]!
/// </code>
/// </remarks>
/// <example>
/// <para><strong>Basic Usage:</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit.Attributes;
/// 
/// public class CalculatorTests
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12)
///     ];
///     
///     public static IEnumerable&lt;ITestData&gt; GetAddTestData()
///     {
///         return AddTestData;
///     }
///     
///     [Theory]
///     [PortamicalData(nameof(GetAddTestData))]
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
/// </code>
/// </example>
/// <seealso cref="PortamicalBaseDataAttribute"/>
/// <seealso cref="MemberDataAttributeBase"/>
/// <seealso cref="ITestData"/>
/// <seealso cref="ArgsCode.Instance"/>
[DataDiscoverer("Xunit.Sdk.MemberDataDiscoverer", "xunit.core")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class PortamicalDataAttribute(string memberName, params object?[]? parameters)
    : PortamicalBaseDataAttribute(memberName, parameters)
{
}