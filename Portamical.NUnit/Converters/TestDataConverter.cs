// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.NUnit.TestDataTypes;
using static Portamical.NUnit.TestDataTypes.TestCaseTestData;

namespace Portamical.NUnit.Converters;

/// <summary>
/// Provides extension methods for converting Portamical test data to NUnit test case formats.
/// </summary>
/// <remarks>
/// <para>
/// This static class offers two conversion strategies that differ in their return types:
/// <list type="bullet">
///   <item><description>
///     <see cref="ToTestCaseTestData{TTestData}"/> - Returns the rich
///     <see cref="TestCaseTestData{TTestData}"/> type with <see cref="INamedCase"/> features
///   </description></item>
///   <item><description>
///     <see cref="ToTestCaseData{TTestData}"/> - Returns the base <see cref="TestCaseData"/> type
///     for external API compatibility (implicit upcast from <see cref="TestCaseTestData{TTestData}"/>)
///   </description></item>
/// </list>
/// </para>
/// <para>
/// Both methods support two test data representation strategies via <see cref="ArgsCode"/>:
/// <list type="table">
///   <listheader>
///     <term>Strategy</term>
///     <description>Test Method Signature</description>
///     <description>Description</description>
///   </listheader>
///   <item>
///     <term><see cref="ArgsCode.Instance"/></term>
///     <description><c>void Test(ITestData testData)</c></description>
///     <description>Shared Style - Framework-agnostic</description>
///   </item>
///   <item>
///     <term><see cref="ArgsCode.Properties"/></term>
///     <description><c>void Test(T1 arg1, T2 arg2, ...)</c></description>
///     <description>Native Style - Idiomatic NUnit</description>
///   </item>
/// </list>
/// </para>
/// <para>
/// Both methods delegate to <see cref="TestCaseTestData.From{TTestData}"/> for configuration,
/// ensuring consistent behavior and adhering to the DRY (Don't Repeat Yourself) principle.
/// </para>
/// </remarks>
public static class TestDataConverter
{
    /// <summary>
    /// Converts Portamical test data to a rich <see cref="TestCaseTestData{TTestData}"/> instance.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of the test data, which must implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testData">The Portamical test data to convert.</param>
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
    /// Optional test method name to prepend to the test case name in the display name.
    /// If provided, the test name will be formatted as "testMethodName - TestCaseName".
    /// If <c>null</c>, only the test case name is used.
    /// </param>
    /// <returns>
    /// A fully configured <see cref="TestCaseTestData{TTestData}"/> instance with
    /// <see cref="INamedCase"/> features (equality based on <see cref="INamedCase.TestCaseName"/>,
    /// consistent hashing).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This extension method provides fluent syntax for calling <see cref="TestCaseTestData.From{TTestData}"/>:
    /// </para>
    /// <code>
    /// // Native Style (ArgsCode.Properties):
    /// var testCase = testData.ToTestCaseTestData(ArgsCode.Properties, "TestAdd");
    /// 
    /// // Shared Style (ArgsCode.Instance):
    /// var testCase = testData.ToTestCaseTestData(ArgsCode.Instance, "TestAdd");
    /// 
    /// // Equivalent factory method style:
    /// var testCase = TestCaseTestData.From(testData, ArgsCode.Properties, "TestAdd");
    /// </code>
    /// </remarks>
    /// <example>
    /// <para><strong>Native Style (ArgsCode.Properties):</strong></para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// var testCase = testData.ToTestCaseTestData(ArgsCode.Properties, "TestAdd");
    /// 
    /// // testCase.Arguments = [2, 3]
    /// // testCase.ExpectedResult = 5
    /// // testCase.TestName = "TestAdd - Add(2,3)"
    /// </code>
    /// 
    /// <para><strong>Shared Style (ArgsCode.Instance):</strong></para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// var testCase = testData.ToTestCaseTestData(ArgsCode.Instance, "TestAdd");
    /// 
    /// // testCase.Arguments = [testData]  ← Entire ITestData object
    /// // testCase.ExpectedResult = 5
    /// // testCase.TestName = "TestAdd - Add(2,3)"
    /// </code>
    /// </example>
    /// <seealso cref="TestCaseTestData.From{TTestData}"/>
    /// <seealso cref="ToTestCaseData{TTestData}"/>
    /// <seealso cref="ArgsCode"/>
    internal static TestCaseTestData<TTestData> ToTestCaseTestData<TTestData>(
        this TTestData testData,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => From(testData, argsCode, testMethodName);

    /// <summary>
    /// Converts Portamical test data to NUnit's base <see cref="TestCaseData"/> type.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of the test data, which must implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testData">The Portamical test data to convert.</param>
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
    /// Optional test method name to prepend to the test case name in the display name.
    /// If provided, the test name will be formatted as "testMethodName - TestCaseName".
    /// If <c>null</c>, only the test case name is used.
    /// </param>
    /// <returns>
    /// A configured <see cref="TestCaseData"/> instance (base NUnit type).
    /// <para>
    /// <strong>Note:</strong> The actual runtime type is <see cref="TestCaseTestData{TTestData}"/>,
    /// but it is returned as the base <see cref="TestCaseData"/> type via implicit upcast.
    /// This is transparent to NUnit, which uses polymorphism to access test case properties.
    /// </para>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method delegates to <see cref="TestCaseTestData.From{TTestData}"/> and returns
    /// the result as the base <see cref="TestCaseData"/> type via implicit upcast.
    /// </para>
    /// <para>
    /// <strong>Inheritance Chain:</strong>
    /// <code>
    /// TestCaseTestData&lt;TTestData&gt; : TestCaseTestData : TestCaseData
    /// </code>
    /// The implicit upcast is safe, zero-cost (no object conversion), and fully compatible with NUnit.
    /// </para>
    /// <para>
    /// <strong>When to Use This Method:</strong>
    /// <list type="bullet">
    ///   <item><description>
    ///     An external API requires <see cref="TestCaseData"/> in the method signature
    ///   </description></item>
    ///   <item><description>
    ///     You want to avoid exposing <see cref="TestCaseTestData{TTestData}"/> in public signatures
    ///   </description></item>
    ///   <item><description>
    ///     You don't need to use <see cref="INamedCase"/> features in your test code
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Alternative:</strong> Use <see cref="ToTestCaseTestData{TTestData}"/> if you want
    /// the full <see cref="TestCaseTestData{TTestData}"/> type with <see cref="INamedCase"/> features.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Native Style (ArgsCode.Properties):</strong></para>
    /// <code>
    /// public static IEnumerable&lt;TestCaseData&gt; AddTestCases()
    /// {
    ///     var testData = new[]
    ///     {
    ///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///         new TestDataReturns&lt;int&gt;("Add(0,0)", [0, 0], 0)
    ///     };
    ///     
    ///     return testData.Select(td => 
    ///         td.ToTestCaseData(ArgsCode.Properties, nameof(TestAdd)));
    /// }
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
    /// {
    ///     var testData = new[]
    ///     {
    ///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///         new TestDataReturns&lt;int&gt;("Add(0,0)", [0, 0], 0)
    ///     };
    ///     
    ///     return testData.Select(td => 
    ///         td.ToTestCaseData(ArgsCode.Instance, nameof(TestAdd)));
    /// }
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
    /// <seealso cref="ToTestCaseTestData{TTestData}"/>
    /// <seealso cref="TestCaseTestData.From{TTestData}"/>
    /// <seealso cref="ArgsCode"/>
    internal static TestCaseData ToTestCaseData<TTestData>(
        this TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => From(testData, argsCode, testMethodName);
}