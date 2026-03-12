// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit.Converters;
using Portamical.xUnit.DataProviders;

namespace Portamical.xUnit.TestBases;

/// <summary>
/// Abstract base class for xUnit v2 test fixtures that provides convenient test data conversion methods.
/// </summary>
/// <remarks>
/// <para>
/// <strong>⚠️ xUnit v2 Legacy Support:</strong>
/// </para>
/// <para>
/// This base class extends <see cref="Portamical.TestBases.TestBase"/> with xUnit v2-specific
/// conversion methods that return <see cref="TestDataProvider{TTestData}"/> (legacy Builder pattern).
/// </para>
/// <para>
/// For new projects using xUnit v3, use <c>Portamical.xUnit_v3</c> instead, which provides:
/// <list type="bullet">
///   <item><description>Type-safe <c>TheoryData&lt;T&gt;</c> API</description></item>
///   <item><description>Custom test names via <c>ITheoryDataRow.TestDisplayName</c></description></item>
///   <item><description>Support for <see cref="ArgsCode.Properties"/> (Native Style)</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern: Facade + Template Method</strong>
/// </para>
/// <para>
/// Acts as a facade over <see cref="CollectionConverter"/> extension methods, providing a cleaner API
/// for test data source methods:
/// <list type="bullet">
///   <item><description>
///     <see cref="Convert{TTestData}(IEnumerable{TTestData}, ArgsCode)"/> - 
///     Converts with explicit <see cref="ArgsCode"/> strategy
///   </description></item>
///   <item><description>
///     <see cref="Convert{TTestData}(IEnumerable{TTestData})"/> - 
///     Converts with default <see cref="ArgsCode.Instance"/> strategy (Shared Style)
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>xUnit v2 Limitations:</strong>
/// </para>
/// <para>
/// Unlike NUnit and MSTest, xUnit v2 does not support:
/// <list type="bullet">
///   <item><description>
///     <strong>Custom Test Names:</strong> Test names are auto-generated as <c>MethodName(param1: value1, param2: value2, ...)</c>
///   </description></item>
///   <item><description>
///     <strong>Native Style (ArgsCode.Properties):</strong> Only <see cref="ArgsCode.Instance"/> (Shared Style) works correctly
///   </description></item>
///   <item><description>
///     <strong>testMethodName Parameter:</strong> Not included in methods because xUnit v2 doesn't support it
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Inheritance Chain:</strong>
/// <code>
/// Portamical.TestBases.TestBase (framework-agnostic base)
///   ↓ inherits
/// Portamical.xUnit.TestBases.TestBase (xUnit v2-specific) ← This class
///   ↓ inherits
/// YourTestClass (concrete test fixture)
/// </code>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Shared Style (ArgsCode.Instance) - Recommended</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit.TestBases;
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
///     /// Test data source for xUnit v2 [MemberData].
///     /// &lt;/summary&gt;
///     /// &lt;returns&gt;TestDataProvider (xUnit v2 legacy format).&lt;/returns&gt;
///     public static IEnumerable GetAddTestData()
///     {
///         // Shared Style: Convert with explicit ArgsCode.Instance
///         return Convert(AddTestData, ArgsCode.Instance);
///     }
///     
///     [Theory]
///     [MemberData(nameof(GetAddTestData))]
///     public void TestAdd(TestDataReturns&lt;int&gt; testData)
///     {
///         // Framework-agnostic test method (Shared Style)
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
/// <para><strong>Example 2: Convenience Overload (Default ArgsCode.Instance)</strong></para>
/// <code>
/// public class CalculatorTests : TestBase
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12)
///     ];
///     
///     public static IEnumerable GetAddTestData()
///     {
///         // Convenience overload - defaults to ArgsCode.Instance
///         return Convert(AddTestData);
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
/// <para><strong>Example 3: Why Native Style (Properties) Doesn't Work</strong></para>
/// <code>
/// // ❌ This WILL NOT WORK in xUnit v2:
/// public static IEnumerable GetTestData()
/// {
///     return Convert(testData, ArgsCode.Properties);  // ← Properties (Native Style)
/// }
/// 
/// [Theory]
/// [MemberData(nameof(GetTestData))]
/// public void TestAdd(int x, int y, int expected)
/// //                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^ Native Style signature
/// {
///     // Problem: Convert returns TestDataProvider&lt;T&gt; with ArgsCode.Instance internally
///     //          xUnit v2 receives object[] { testData }
///     //          xUnit v2 tries to map [testData] → (int x, int y, int expected)
///     //          Result: Parameter count mismatch error
///     
///     int result = Calculator.Add(x, y);
///     Assert.Equal(expected, result);
/// }
/// 
/// // ✅ Use Shared Style instead:
/// [Theory]
/// [MemberData(nameof(GetTestData))]
/// public void TestAdd(TestDataReturns&lt;int&gt; testData)
/// {
///     var args = testData.Args;
///     int result = Calculator.Add((int)args[0], (int)args[1]);
///     Assert.Equal(testData.Expected, result);
/// }
/// </code>
/// </example>
/// <seealso cref="Portamical.TestBases.TestBase"/>
/// <seealso cref="CollectionConverter"/>
/// <seealso cref="TestDataProvider{TTestData}"/>
/// <seealso cref="ArgsCode"/>
public abstract class TestBase : Portamical.TestBases.TestBase
{
    /// <summary>
    /// Converts a collection of Portamical test data to xUnit v2's <see cref="TestDataProvider{TTestData}"/>.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <param name="argsCode">
    /// Specifies the test data representation strategy. For xUnit v2, only <see cref="ArgsCode.Instance"/>
    /// (Shared Style) works correctly due to xUnit v2 limitations.
    /// </param>
    /// <returns>
    /// A <see cref="TestDataProvider{TTestData}"/> instance (xUnit v2 legacy Builder pattern)
    /// with duplicates removed based on <see cref="INamedCase.TestCaseName"/> equality.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is a facade over <see cref="CollectionConverter.ToTestDataProvider{TTestData}"/>,
    /// providing a cleaner API for use in test data source methods.
    /// </para>
    /// <para>
    /// <strong>xUnit v2 Legacy Format:</strong>
    /// </para>
    /// <para>
    /// xUnit v2 uses <see cref="TestDataProvider{TTestData}"/> (Builder pattern) instead of modern
    /// type-safe collections. This return type implements <see cref="System.Collections.IEnumerable"/>
    /// (non-generic) for compatibility with xUnit v2's <c>[MemberData]</c> attribute.
    /// </para>
    /// <para>
    /// <strong>ArgsCode Limitation:</strong>
    /// </para>
    /// <para>
    /// Although this method accepts an <paramref name="argsCode"/> parameter, only <see cref="ArgsCode.Instance"/>
    /// (Shared Style) works correctly with xUnit v2:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong><see cref="ArgsCode.Instance"/> (Shared Style):</strong> ✅ Works - Test method receives entire <see cref="ITestData"/> object
    ///   </description></item>
    ///   <item><description>
    ///     <strong><see cref="ArgsCode.Properties"/> (Native Style):</strong> ❌ Fails - xUnit v2 can't flatten properties
    ///   </description></item>
    /// </list>
    /// <para>
    /// <strong>No testMethodName Parameter:</strong>
    /// </para>
    /// <para>
    /// Unlike NUnit and MSTest, this method does not have a <c>testMethodName</c> parameter because
    /// xUnit v2 does not support custom test case names. Test names are auto-generated as
    /// <c>MethodName(param1: value1, param2: value2, ...)</c>.
    /// </para>
    /// <para>
    /// <strong>Typical Usage:</strong>
    /// </para>
    /// <code>
    /// public static IEnumerable GetTestData()
    ///     =&gt; Convert(testData, ArgsCode.Instance);
    /// </code>
    /// </remarks>
    /// <example>
    /// <para><strong>Shared Style (ArgsCode.Instance):</strong></para>
    /// <code>
    /// private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
    /// [
    ///     new("Add(2,3)", [2, 3], 5),
    ///     new("Add(0,0)", [0, 0], 0)
    /// ];
    /// 
    /// public static IEnumerable GetAddTestData()
    ///     =&gt; Convert(AddTestData, ArgsCode.Instance);
    /// 
    /// [Theory]
    /// [MemberData(nameof(GetAddTestData))]
    /// public void TestAdd(TestDataReturns&lt;int&gt; testData)
    /// {
    ///     var args = testData.Args;
    ///     var expected = testData.Expected;
    ///     
    ///     int result = Calculator.Add((int)args[0], (int)args[1]);
    ///     Assert.Equal(expected, result);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Convert{TTestData}(IEnumerable{TTestData})"/>
    /// <seealso cref="CollectionConverter.ToTestDataProvider{TTestData}"/>
    /// <seealso cref="ArgsCode"/>
    protected static TestDataProvider<TTestData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTestDataProvider(argsCode);

    /// <summary>
    /// Converts a collection of Portamical test data to xUnit v2's <see cref="TestDataProvider{TTestData}"/>
    /// using the default <see cref="ArgsCode.Instance"/> strategy (Shared Style).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <returns>
    /// A <see cref="TestDataProvider{TTestData}"/> instance (xUnit v2 legacy Builder pattern)
    /// with duplicates removed based on <see cref="INamedCase.TestCaseName"/> equality.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is a convenience overload that delegates to the base class
    /// <see cref="Portamical.TestBases.TestBase.ConvertAsInstance{TTestData, TResult}"/> method,
    /// which automatically uses <see cref="ArgsCode.Instance"/> (Shared Style).
    /// </para>
    /// <para>
    /// <strong>When to Use:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     Use this overload when you want framework-agnostic test methods (Shared Style)
    ///   </description></item>
    ///   <item><description>
    ///     Use <see cref="Convert{TTestData}(IEnumerable{TTestData}, ArgsCode)"/> 
    ///     when you need explicit control over <see cref="ArgsCode"/> (though only <c>Instance</c> works in xUnit v2)
    ///   </description></item>
    /// </list>
    /// <para>
    /// <strong>Delegation Chain:</strong>
    /// <code>
    /// Convert(testData)
    ///   ↓ delegates to base class
    /// ConvertAsInstance(Convert, testData)
    ///   ↓ invokes (with ArgsCode.Instance)
    /// Convert(testData, ArgsCode.Instance)
    ///   ↓ calls
    /// ToTestDataProvider(ArgsCode.Instance)
    /// </code>
    /// </para>
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
    ///     new("Add(0,0)", [0, 0], 0)
    /// ];
    /// 
    /// // Convenience overload - defaults to ArgsCode.Instance
    /// public static IEnumerable GetAddTestData()
    ///     =&gt; Convert(AddTestData);
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
    /// <seealso cref="Convert{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
    /// <seealso cref="Portamical.TestBases.TestBase.ConvertAsInstance{TTestData, TResult}"/>
    /// <seealso cref="ArgsCode.Instance"/>
    protected static TestDataProvider<TTestData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => ConvertAsInstance(Convert, testDataCollection);
}