// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.NUnit.TestDataTypes;

namespace Portamical.NUnit.Converters;

/// <summary>
/// Provides extension methods for batch conversion of Portamical test data collections
/// to NUnit test case collections.
/// </summary>
/// <remarks>
/// <para>
/// This class offers two conversion strategies that differ in their return types:
/// <list type="bullet">
///   <item><description>
///     <see cref="ToTestCaseDataCollection{TTestData}"/> - Returns base <see cref="TestCaseData"/> collection
///   </description></item>
///   <item><description>
///     <see cref="ToTestCaseTestDataCollection{TTestData}"/> - Returns rich
///     <see cref="TestCaseTestData{TTestData}"/> collection with <see cref="INamedCase"/> features
///   </description></item>
/// </list>
/// </para>
/// <para>
/// Both methods support two test data representation strategies via <see cref="ArgsCode"/>:
/// <list type="table">
///   <listheader>
///     <term>Strategy</term>
///     <description>Description</description>
///     <description>Test Signature</description>
///   </listheader>
///   <item>
///     <term><see cref="ArgsCode.Instance"/></term>
///     <description>Pass entire test data object (Shared Style)</description>
///     <description><c>void Test(ITestData testData)</c></description>
///   </item>
///   <item>
///     <term><see cref="ArgsCode.Properties"/></term>
///     <description>Pass flattened properties as separate args (Native Style)</description>
///     <description><c>void Test(T1 arg1, T2 arg2, ...)</c></description>
///   </item>
/// </list>
/// </para>
/// </remarks>
public static class CollectionConverter
{
    /// <summary>
    /// Converts a collection of Portamical test data to a read-only collection of NUnit's base
    /// <see cref="TestCaseData"/> instances, removing duplicates based on test case name.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of the test data, which must implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <param name="argsCode">
    /// Specifies the test data representation strategy:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire test data object as single argument
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
    /// <list type="bullet">
    ///   <item><description>
    ///     If provided and non-empty: Display name = "testMethodName - TestCaseName"
    ///   </description></item>
    ///   <item><description>
    ///     If <c>null</c> or empty: Display name = "TestCaseName"
    ///   </description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="TestCaseData"/> instances with duplicates removed
    /// (based on <see cref="INamedCase.TestCaseName"/> equality).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method delegates to <see cref="Portamical.Converters"/> extension method
    /// <c>ToDistinctReadOnly</c>, which:
    /// <list type="number">
    ///   <item><description>
    ///     Converts each test data item using <see cref="TestDataConverter.ToTestCaseData{TTestData}"/>
    ///   </description></item>
    ///   <item><description>
    ///     Removes duplicate test cases (those with identical <see cref="INamedCase.TestCaseName"/>)
    ///   </description></item>
    ///   <item><description>
    ///     Returns an immutable read-only collection
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Primary Use Case:</strong> Batch conversion for <c>[TestCaseSource]</c> attributes.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Example 1: Shared Style (ArgsCode.Instance)</strong></para>
    /// <code>
    /// [TestFixture]
    /// public class CalculatorTests
    /// {
    ///     private static IEnumerable&lt;TestCaseData&gt; SharedStyleTests()
    ///     {
    ///         var testData = new[]
    ///         {
    ///             new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///             new TestDataReturns&lt;int&gt;("Add(0,0)", [0, 0], 0)
    ///         };
    ///         
    ///         // Pass entire test data object as single argument
    ///         return testData.ToTestCaseDataCollection(
    ///             ArgsCode.Instance,
    ///             nameof(TestAddShared));
    ///     }
    ///     
    ///     [TestCaseSource(nameof(SharedStyleTests))]
    ///     public void TestAddShared(TestDataReturns&lt;int&gt; testData)
    ///     {
    ///         // Framework-agnostic test method
    ///         var args = testData.Args;
    ///         var expected = testData.Expected;
    ///         
    ///         int result = Calculator.Add((int)args[0], (int)args[1]);
    ///         Assert.That(result, Is.EqualTo(expected));
    ///     }
    /// }
    /// 
    /// // NUnit Test Explorer displays:
    /// // ✓ TestAddShared - Add(2,3)
    /// // ✓ TestAddShared - Add(0,0)
    /// </code>
    /// 
    /// <para><strong>Example 2: Native Style (ArgsCode.Properties)</strong></para>
    /// <code>
    /// [TestFixture]
    /// public class CalculatorTests
    /// {
    ///     private static IEnumerable&lt;TestCaseData&gt; NativeStyleTests()
    ///     {
    ///         var testData = new[]
    ///         {
    ///             new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///             new TestDataReturns&lt;int&gt;("Add(0,0)", [0, 0], 0),
    ///             new TestDataReturns&lt;int&gt;("Add(2,3)", [5, 7], 12)  // Duplicate name (removed)
    ///         };
    ///         
    ///         // Pass flattened properties as separate arguments
    ///         return testData.ToTestCaseDataCollection(
    ///             ArgsCode.Properties,
    ///             nameof(TestAddNative));
    ///     }
    ///     
    ///     [TestCaseSource(nameof(NativeStyleTests))]
    ///     public void TestAddNative(int x, int y)
    ///     {
    ///         // Idiomatic NUnit test method
    ///         int result = Calculator.Add(x, y);
    ///         // NUnit automatically compares result with ExpectedResult
    ///     }
    /// }
    /// 
    /// // NUnit Test Explorer displays:
    /// // ✓ TestAddNative - Add(2,3)
    /// // ✓ TestAddNative - Add(0,0)
    /// // (Duplicate "Add(2,3)" removed)
    /// </code>
    /// 
    /// <para><strong>Example 3: Generic Test Method (ArgsCode.Properties)</strong></para>
    /// <code>
    /// private static IEnumerable&lt;TestCaseData&gt; GenericTests()
    /// {
    ///     var testData = new[]
    ///     {
    ///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///         new TestDataReturns&lt;string&gt;("Concat", ["Hello", "World"], "HelloWorld")
    ///     };
    ///     
    ///     return testData.ToTestCaseDataCollection(
    ///         ArgsCode.Properties,
    ///         nameof(TestGeneric));
    ///     // Sets TypeArgs for generic test method instantiation
    /// }
    /// 
    /// [TestCaseSource(nameof(GenericTests))]
    /// public void TestGeneric&lt;T&gt;(T arg1, T arg2)
    /// {
    ///     // NUnit uses TypeArgs to instantiate:
    ///     // TestGeneric&lt;int&gt;(2, 3)
    ///     // TestGeneric&lt;string&gt;("Hello", "World")
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ToTestCaseTestDataCollection{TTestData}"/>
    /// <seealso cref="TestDataConverter.ToTestCaseData{TTestData}"/>
    /// <seealso cref="ArgsCode"/>
    public static IReadOnlyCollection<TestCaseData> ToTestCaseDataCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        TestDataConverter.ToTestCaseData,
        argsCode,
        testMethodName);

    /// <summary>
    /// Converts a collection of Portamical test data to a read-only collection of
    /// <see cref="TestCaseTestData{TTestData}"/> instances (rich NUnit adapter with
    /// <see cref="INamedCase"/> features), removing duplicates based on test case name.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of the test data, which must implement <see cref="ITestData"/>.
    /// This generic parameter is preserved in the returned <see cref="TestCaseTestData{TTestData}"/> instances.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <param name="argsCode">
    /// Specifies the test data representation strategy:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire test data object as single argument
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
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="TestCaseTestData{TTestData}"/> instances with
    /// duplicates removed (based on <see cref="INamedCase.TestCaseName"/> equality).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method returns the rich <see cref="TestCaseTestData{TTestData}"/> type instead of
    /// base <see cref="TestCaseData"/>. Use this when you need <see cref="INamedCase"/> features:
    /// <list type="bullet">
    ///   <item><description>Equality based on <see cref="INamedCase.TestCaseName"/></description></item>
    ///   <item><description>Consistent hashing based on test case name</description></item>
    ///   <item><description>Access to <see cref="TestCaseTestData.TestCaseName"/> property</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Comparison:</strong>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Method</term>
    ///     <description>Return Type</description>
    ///     <description>Use Case</description>
    ///   </listheader>
    ///   <item>
    ///     <term><see cref="ToTestCaseDataCollection{TTestData}"/></term>
    ///     <description><see cref="TestCaseData"/></description>
    ///     <description>Base NUnit type, no INamedCase features</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="ToTestCaseTestDataCollection{TTestData}"/></term>
    ///     <description><see cref="TestCaseTestData{TTestData}"/></description>
    ///     <description>Rich adapter with INamedCase features</description>
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// private static IEnumerable&lt;TestCaseData&gt; GetTestCases()
    /// {
    ///     var testData = new[]
    ///     {
    ///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///         new TestDataReturns&lt;int&gt;("Add(0,0)", [0, 0], 0)
    ///     };
    ///     
    ///     // Returns TestCaseTestData&lt;TestDataReturns&lt;int&gt;&gt; collection
    ///     var testCases = testData.ToTestCaseTestDataCollection(
    ///         ArgsCode.Properties,
    ///         nameof(TestAdd));
    ///     
    ///     // INamedCase features available:
    ///     foreach (var testCase in testCases)
    ///     {
    ///         string name = testCase.TestCaseName;  // "Add(2,3)", "Add(0,0)"
    ///         bool equal = testCase.Equals(other);  // Compares by TestCaseName
    ///         int hash = testCase.GetHashCode();    // Hash based on TestCaseName
    ///     }
    ///     
    ///     return testCases;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ToTestCaseDataCollection{TTestData}"/>
    /// <seealso cref="TestDataConverter.ToTestCaseTestData{TTestData}"/>
    /// <seealso cref="ArgsCode"/>
    public static IReadOnlyCollection<TestCaseTestData<TTestData>> ToTestCaseTestDataCollection<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        TestDataConverter.ToTestCaseTestData,
        argsCode,
        testMethodName);
}