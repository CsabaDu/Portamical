// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.NUnit.Converters;

namespace Portamical.NUnit.TestBases.TestCaseDataCollection;

/// <summary>
/// Abstract base class for NUnit test fixtures that provides convenient test data conversion methods
/// returning NUnit's base <see cref="TestCaseData"/> type.
/// </summary>
/// <remarks>
/// <para>
/// This base class extends <see cref="Portamical.TestBases.TestBase"/> with NUnit-specific
/// conversion methods that return <see cref="TestCaseData"/> (NUnit's base type) instead of
/// <see cref="TestCaseTestData"/> (Portamical's adapter type).
/// </para>
/// <para>
/// <strong>When to Use This vs Main TestBase:</strong>
/// </para>
/// <para>
/// <list type="table">
///   <listheader>
///     <term>Test Base Class</term>
///     <description>Return Type</description>
///     <description>Use Case</description>
///   </listheader>
///   <item>
///     <term><c>Portamical.NUnit.TestBases.TestBase</c></term>
///     <description><see cref="TestCaseTestData"/></description>
///     <description>When you need <see cref="INamedCase"/> features</description>
///   </item>
///   <item>
///     <term><c>Portamical.NUnit.TestBases.TestCaseDataCollection.TestBase</c> (this class)</term>
///     <description><see cref="TestCaseData"/></description>
///     <description>When you want raw NUnit compatibility</description>
///   </item>
/// </list>
/// </para>
/// <para>
/// <strong>Trade-offs:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>
///     ✅ <strong>Simpler Type:</strong> Returns <see cref="TestCaseData"/> (familiar NUnit type)
///   </description></item>
///   <item><description>
///     ✅ <strong>Direct Compatibility:</strong> Exact match with NUnit's expectations (no implicit upcast)
///   </description></item>
///   <item><description>
///     ❌ <strong>Lost Features:</strong> <see cref="INamedCase"/> features (TestCaseName, Equals, GetHashCode) are not accessible
///   </description></item>
///   <item><description>
///     ⚠️ <strong>Type Information:</strong> Generic parameter <c>TTestData</c> is erased (cannot downcast)
///   </description></item>
/// </list>
/// <para>
/// <strong>Design Pattern: Facade + Template Method</strong>
/// </para>
/// <para>
/// Acts as a facade over <see cref="CollectionConverter"/> extension methods, providing a cleaner API
/// for test case source methods:
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
/// <code>
/// Portamical.TestBases.TestBase (framework-agnostic base)
///   ↓ inherits
/// Portamical.NUnit.TestBases.TestCaseDataCollection.TestBase (NUnit-specific, base type variant) ← This class
///   ↓ inherits
/// YourTestClass (concrete test fixture)
/// </code>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Native Style (ArgsCode.Properties)</strong></para>
/// <code>
/// using NUnit.Framework;
/// using Portamical.NUnit.TestBases.TestCaseDataCollection;
/// 
/// [TestFixture]
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
///     // Native Style: Convert to TestCaseData with ArgsCode.Properties
///     public static IEnumerable&lt;TestCaseData&gt; AddTestCases()
///         =&gt; Convert(AddTestData, ArgsCode.Properties, nameof(TestAdd));
///     //     ^^^^^^^ Returns IReadOnlyCollection&lt;TestCaseData&gt; (raw NUnit type)
///     
///     [TestCaseSource(nameof(AddTestCases))]
///     public void TestAdd(int x, int y)
///     {
///         int result = Calculator.Add(x, y);
///         // NUnit automatically compares result with ExpectedResult
///     }
/// }
/// 
/// // NUnit Test Explorer displays:
/// // ✓ TestAdd - Add(2,3)
/// // ✓ TestAdd - Add(5,7)
/// // ✓ TestAdd - Add(-1,1)
/// </code>
/// 
/// <para><strong>Example 2: Shared Style (ArgsCode.Instance - Convenience Overload)</strong></para>
/// <code>
/// [TestFixture]
/// public class CalculatorTests : TestBase
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12)
///     ];
///     
///     // Shared Style: Convert with default ArgsCode.Instance
///     public static IEnumerable&lt;TestCaseData&gt; AddTestCases()
///         =&gt; Convert(AddTestData, nameof(TestAdd));
///     //     ^^^^^^^ No ArgsCode parameter - defaults to Instance
///     
///     [TestCaseSource(nameof(AddTestCases))]
///     public void TestAdd(TestDataReturns&lt;int&gt; testData)
///     {
///         // Framework-agnostic test method
///         var args = testData.Args;
///         var expected = testData.Expected;
///         
///         int result = Calculator.Add((int)args[0], (int)args[1]);
///         Assert.That(result, Is.EqualTo(expected));
///     }
/// }
/// </code>
/// 
/// <para><strong>Example 3: Comparison with Main TestBase</strong></para>
/// <code>
/// // Using TestCaseDataCollection.TestBase (this class):
/// public static IEnumerable&lt;TestCaseData&gt; GetTestCases1()
/// {
///     var testCases = Convert(testData, ArgsCode.Properties, "TestAdd");
///     //              ^^^^^^^ Returns IReadOnlyCollection&lt;TestCaseData&gt;
///     
///     // ❌ Cannot access INamedCase features:
///     // testCases[0].TestCaseName  ← Compile error (property doesn't exist)
///     
///     return testCases;
/// }
/// 
/// // Using Portamical.NUnit.TestBases.TestBase (main variant):
/// public static IEnumerable&lt;TestCaseData&gt; GetTestCases2()
/// {
///     var testCases = Convert(testData, ArgsCode.Properties, "TestAdd");
///     //              ^^^^^^^ Returns IReadOnlyCollection&lt;TestCaseTestData&gt;
///     
///     // ✅ Can access INamedCase features:
///     string name = testCases.First().TestCaseName;  // "Add(2,3)"
///     
///     return testCases;  // Implicit upcast to IEnumerable&lt;TestCaseData&gt;
/// }
/// </code>
/// </example>
/// <seealso cref="Portamical.TestBases.TestBase"/>
/// <seealso cref="Portamical.NUnit.TestBases.TestBase"/>
/// <seealso cref="CollectionConverter"/>
/// <seealso cref="TestDataConverter"/>
/// <seealso cref="TestCaseData"/>
/// <seealso cref="TestCaseTestData"/>
/// <seealso cref="ArgsCode"/>
public abstract class TestBase : Portamical.TestBases.TestBase
{
    /// <summary>
    /// Converts a collection of Portamical test data to a read-only collection of
    /// <see cref="TestCaseData"/> instances (NUnit's base type).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <param name="argsCode">
    /// Specifies the test data representation strategy:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire <see cref="ITestData"/> object as single argument
    ///     (Shared Style: framework-agnostic test methods)
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments
    ///     (Native Style: idiomatic NUnit test methods)
    ///   </description></item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to each test case name in the display name.
    /// If provided, the test name will be formatted as "testMethodName - TestCaseName".
    /// If <c>null</c>, only the test case name is used.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="TestCaseData"/> instances (NUnit's base type)
    /// with duplicates removed based on <see cref="INamedCase.TestCaseName"/> equality.
    /// <para>
    /// <strong>Note:</strong> The actual runtime type is <see cref="TestCaseTestData{TTestData}"/>,
    /// but it is returned as the base <see cref="TestCaseData"/> type via implicit upcast.
    /// This means <see cref="INamedCase"/> features (TestCaseName, Equals, GetHashCode) are not
    /// accessible through the returned collection.
    /// </para>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is a facade over <see cref="CollectionConverter.ToTestCaseDataCollection{TTestData}"/>,
    /// providing a cleaner API for use in test case source methods.
    /// </para>
    /// <para>
    /// <strong>Return Type Rationale:</strong>
    /// </para>
    /// <para>
    /// Returns <see cref="TestCaseData"/> (not <see cref="TestCaseTestData"/>) for:
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong>Simplicity:</strong> Familiar NUnit type, no need to understand Portamical adapters
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Direct Compatibility:</strong> Exact match with NUnit's expected type
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Trade-off:</strong> Loses <see cref="INamedCase"/> features (TestCaseName, Equals, GetHashCode)
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If you need <see cref="INamedCase"/> features, use
    /// <see cref="Portamical.NUnit.TestBases.TestBase"/> instead (main variant).
    /// </para>
    /// <para>
    /// <strong>Typical Usage:</strong>
    /// </para>
    /// <code>
    /// public static IEnumerable&lt;TestCaseData&gt; GetTestCases()
    ///     =&gt; Convert(testData, ArgsCode.Properties, nameof(TestMethod));
    /// </code>
    /// </remarks>
    /// <example>
    /// <para><strong>Native Style (ArgsCode.Properties):</strong></para>
    /// <code>
    /// private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
    /// [
    ///     new("Add(2,3)", [2, 3], 5),
    ///     new("Add(0,0)", [0, 0], 0)
    /// ];
    /// 
    /// public static IEnumerable&lt;TestCaseData&gt; AddTestCases()
    ///     =&gt; Convert(AddTestData, ArgsCode.Properties, nameof(TestAdd));
    /// 
    /// [TestCaseSource(nameof(AddTestCases))]
    /// public void TestAdd(int x, int y)
    /// {
    ///     int result = Calculator.Add(x, y);
    ///     // NUnit compares result with ExpectedResult
    /// }
    /// </code>
    /// 
    /// <para><strong>Shared Style (ArgsCode.Instance):</strong></para>
    /// <code>
    /// public static IEnumerable&lt;TestCaseData&gt; AddTestCases()
    ///     =&gt; Convert(AddTestData, ArgsCode.Instance, nameof(TestAdd));
    /// 
    /// [TestCaseSource(nameof(AddTestCases))]
    /// public void TestAdd(TestDataReturns&lt;int&gt; testData)
    /// {
    ///     var args = testData.Args;
    ///     var expected = testData.Expected;
    ///     
    ///     int result = Calculator.Add((int)args[0], (int)args[1]);
    ///     Assert.That(result, Is.EqualTo(expected));
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Convert{TTestData}(IEnumerable{TTestData}, string?)"/>
    /// <seealso cref="CollectionConverter.ToTestCaseDataCollection{TTestData}"/>
    /// <seealso cref="Portamical.NUnit.TestBases.TestBase.Convert{TTestData}(IEnumerable{TTestData}, ArgsCode, string?)"/>
    /// <seealso cref="ArgsCode"/>
    protected static IReadOnlyCollection<TestCaseData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTestCaseDataCollection(argsCode, testMethodName);

    /// <summary>
    /// Converts a collection of Portamical test data to a read-only collection of
    /// <see cref="TestCaseData"/> instances using the default <see cref="ArgsCode.Instance"/>
    /// strategy (Shared Style).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to each test case name in the display name.
    /// If provided, the test name will be formatted as "testMethodName - TestCaseName".
    /// If <c>null</c>, only the test case name is used.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="TestCaseData"/> instances (NUnit's base type)
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
    ///     Use <see cref="Convert{TTestData}(IEnumerable{TTestData}, ArgsCode, string?)"/> 
    ///     when you need explicit control over <see cref="ArgsCode"/> (Native Style with <c>Properties</c>)
    ///   </description></item>
    /// </list>
    /// <para>
    /// <strong>Delegation Chain:</strong>
    /// <code>
    /// Convert(testData, testMethodName)
    ///   ↓ delegates to base class
    /// ConvertAsInstance(Convert, testData, testMethodName)
    ///   ↓ invokes (with ArgsCode.Instance)
    /// Convert(testData, ArgsCode.Instance, testMethodName)
    ///   ↓ calls
    /// ToTestCaseDataCollection(ArgsCode.Instance, testMethodName)
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
    /// public static IEnumerable&lt;TestCaseData&gt; AddTestCases()
    ///     =&gt; Convert(AddTestData, nameof(TestAdd));
    /// //     ^^^^^^^ No ArgsCode parameter
    /// 
    /// [TestCaseSource(nameof(AddTestCases))]
    /// public void TestAdd(TestDataReturns&lt;int&gt; testData)
    /// {
    ///     // Framework-agnostic test method (Shared Style)
    ///     var args = testData.Args;
    ///     var expected = testData.Expected;
    ///     
    ///     int result = Calculator.Add((int)args[0], (int)args[1]);
    ///     Assert.That(result, Is.EqualTo(expected));
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Convert{TTestData}(IEnumerable{TTestData}, ArgsCode, string?)"/>
    /// <seealso cref="Portamical.TestBases.TestBase.ConvertAsInstance{TTestData, TResult}"/>
    /// <seealso cref="ArgsCode.Instance"/>
    protected static IReadOnlyCollection<TestCaseData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => ConvertAsInstance(Convert, testDataCollection, testMethodName);
}