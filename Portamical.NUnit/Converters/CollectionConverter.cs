// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.CustomRow;
using Portamical.NUnit.DataProviders.Model;
using Portamical.NUnit.TestDataTypes.Model;

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
    public static TestCaseTestData<TTestData>[] ToDistinctRowArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(
        convertRow: TestCaseTestData.From,
        argsCode, testMethodName);

    public static TestDataProvider<TTestData> ToDataProvider<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => new(testDataCollection, argsCode, testMethodName);
}