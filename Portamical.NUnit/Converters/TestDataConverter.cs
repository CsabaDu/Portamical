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
/// This static class offers two conversion strategies:
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
    /// Specifies which components of the test data to include as test method arguments.
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
    /// // Fluent style (extension method):
    /// var testCase = testData.ToTestCaseTestData(ArgsCode.InOut, "TestAdd");
    /// 
    /// // Equivalent factory method style:
    /// var testCase = TestCaseTestData.From(testData, ArgsCode.InOut, "TestAdd");
    /// </code>
    /// </remarks>
    /// <seealso cref="TestCaseTestData.From{TTestData}"/>
    /// <seealso cref="ToTestCaseData{TTestData}"/>
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
    /// Specifies which components of the test data to include as test method arguments.
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
    ///         td.ToTestCaseData(ArgsCode.InOut, nameof(TestAdd)));
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ToTestCaseTestData{TTestData}"/>
    /// <seealso cref="TestCaseTestData.From{TTestData}"/>
    internal static TestCaseData ToTestCaseData<TTestData>(
        this TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => From(testData, argsCode, testMethodName);
}