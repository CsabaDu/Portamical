// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters;

namespace Portamical.MSTest.Converters;

/// <summary>
/// Provides extension methods for converting test data collections into MSTest-compatible formats.
/// </summary>
/// <remarks>
/// <para>
/// These methods are specifically designed for MSTest's <see cref="DynamicDataAttribute"/>,
/// which requires <c>IEnumerable&lt;object[]&gt;</c> where each array contains test method
/// parameters including the test case name for identification.
/// </para>
/// <para>
/// <strong>Why MSTest Needs Test Case Names as Parameters:</strong>
/// </para>
/// <para>
/// Unlike NUnit's <c>TestCaseData.SetName()</c> or xUnit v3's <c>ITheoryDataRow.TestDisplayName</c>,
/// MSTest's <see cref="DynamicDataAttribute"/> does not have built-in test case naming metadata.
/// The standard pattern is to include the test case name as the <strong>first parameter</strong>
/// in your test method signature for identification in Test Explorer.
/// </para>
/// <para>
/// <strong>Framework Comparison:</strong>
/// <list type="table">
///   <listheader>
///     <term>Framework</term>
///     <description>Test Case Naming Mechanism</description>
///   </listheader>
///   <item>
///     <term>NUnit</term>
///     <description><c>TestCaseData.SetName(string)</c> - Metadata property</description>
///   </item>
///   <item>
///     <term>xUnit v3</term>
///     <description><c>ITheoryDataRow.TestDisplayName</c> - Metadata property</description>
///   </item>
///   <item>
///     <term>xUnit v2</term>
///     <description>Manual <c>[InlineData(DisplayName = "...")]</c></description>
///   </item>
///   <item>
///     <term>MSTest</term>
///     <description><strong>Test case name as first parameter</strong> (this approach)</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
public static class CollectionConverter
{
    /// <summary>
    /// Converts a collection of Portamical test data into MSTest-compatible parameter arrays
    /// with test case names prepended for identification.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>MSTest Integration Pattern (4 Steps):</strong>
    /// <list type="number">
    ///   <item><description>
    ///     Create a static method returning <c>IEnumerable&lt;object[]&gt;</c>
    ///   </description></item>
    ///   <item><description>
    ///     Call this extension method on your test data collection
    ///   </description></item>
    ///   <item><description>
    ///     Decorate your test with <c>[DynamicData(nameof(YourMethod), DynamicDataSourceType.Method)]</c>
    ///   </description></item>
    ///   <item><description>
    ///     Test method signature must include test case name as first parameter:
    ///     <c>void TestMethod(string testCaseName, ...args)</c>
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>ArgsCode Strategy:</strong>
    /// </para>
    /// <para>
    /// This method supports two test data representation strategies:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire test data object as single argument
    ///     (Shared Style: framework-agnostic, test receives <c>ITestData</c> instance)
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments
    ///     (Native Style: idiomatic MSTest, test receives individual typed parameters)
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Parameter Order:</strong>
    /// </para>
    /// <para>
    /// Each returned array contains test case name followed by arguments:
    /// <code>
    /// // With ArgsCode.Instance:
    /// [testCaseName, testDataInstance]
    /// 
    /// // With ArgsCode.Properties:
    /// [testCaseName, arg1, arg2, ..., argN]
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Deduplication:</strong>
    /// </para>
    /// <para>
    /// Uses <see cref="NamedCase.Comparer"/> to automatically remove duplicate test cases
    /// based on <see cref="INamedCase.TestCaseName"/> identity. This ensures each unique
    /// test case appears only once, even if defined multiple times in the source collection.
    /// </para>
    /// <para>
    /// <strong>Property Inclusion (PropsCode.All):</strong>
    /// </para>
    /// <para>
    /// This method uses <see cref="PropsCode.All"/>, which includes all <see cref="ITestData"/>
    /// properties (currently only <see cref="ITestData.TestCaseName"/>) in the output arrays.
    /// Properties are placed <strong>before</strong> test arguments in the resulting arrays.
    /// </para>
    /// <para>
    /// This differs from NUnit/xUnit adapters, which use <see cref="PropsCode.TrimTestCaseName"/>
    /// because those frameworks store test case names as metadata (not parameters).
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">
    /// The type of test data to convert. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <param name="argsCode">
    /// Specifies the test data representation strategy:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire <see cref="ITestData"/> object as single argument
    ///     (Shared Style: test signature is <c>void Test(string testCaseName, ITestData testData)</c>)
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments
    ///     (Native Style: test signature is <c>void Test(string testCaseName, T1 arg1, T2 arg2, ...)</c>)
    ///   </description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// A read-only collection of object arrays suitable for MSTest's <see cref="DynamicDataAttribute"/>.
    /// Each array contains the test case name as the first element, followed by test arguments.
    /// Duplicate test cases (based on <see cref="ITestData.TestCaseName"/>) are automatically removed.
    /// </returns>
    /// <example>
    /// <para><strong>Example 1: Shared Style (ArgsCode.Instance)</strong></para>
    /// <code>
    /// // 1. Define test data (framework-agnostic)
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5)  // Duplicate - will be removed
    /// };
    /// 
    /// // 2. Create data provider method (Shared Style)
    /// public static IEnumerable&lt;object[]&gt; GetSharedStyleTestData()
    ///     =&gt; testData.ToArgsWithTestCaseName(ArgsCode.Instance);
    /// 
    /// // 3. Use in test method (Shared Style)
    /// [DataTestMethod]
    /// [DynamicData(nameof(GetSharedStyleTestData), DynamicDataSourceType.Method)]
    /// public void TestSharedStyle(string testCaseName, TestDataReturns&lt;int&gt; testData)
    /// {
    ///     // Framework-agnostic test method
    ///     var args = testData.Args;
    ///     var expected = testData.Expected;
    ///     
    ///     var result = Calculator.Add((int)args[0], (int)args[1]);
    ///     Assert.AreEqual(expected, result, $"Test '{testCaseName}' failed");
    /// }
    /// 
    /// // Result format (Shared Style):
    /// // ["Add(2,3)", testDataInstance]  ← First test case
    /// // ["Add(5,7)", testDataInstance]  ← Second test case
    /// // ^testCaseName^ ^entire ITestData object^
    /// </code>
    /// 
    /// <para><strong>Example 2: Native Style (ArgsCode.Properties)</strong></para>
    /// <code>
    /// // 1. Define test data
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5)  // Duplicate - will be removed
    /// };
    /// 
    /// // 2. Create data provider method (Native Style)
    /// public static IEnumerable&lt;object[]&gt; GetNativeStyleTestData()
    ///     =&gt; testData.ToArgsWithTestCaseName(ArgsCode.Properties);
    /// 
    /// // 3. Use in test method (Native Style)
    /// [DataTestMethod]
    /// [DynamicData(nameof(GetNativeStyleTestData), DynamicDataSourceType.Method)]
    /// public void TestNativeStyle(string testCaseName, int x, int y, int expected)
    /// {
    ///     // Idiomatic MSTest with separate typed parameters
    ///     var result = Calculator.Add(x, y);
    ///     Assert.AreEqual(expected, result, $"Test '{testCaseName}' failed");
    /// }
    /// 
    /// // Result format (Native Style):
    /// // ["Add(2,3)", 2, 3, 5]   ← First test case
    /// // ["Add(5,7)", 5, 7, 12]  ← Second test case
    /// // ^testCaseName^ ^args^ ^expected^
    /// // 
    /// // The duplicate "Add(2,3)" is automatically removed
    /// </code>
    /// 
    /// <para><strong>Example 3: Test Explorer Display</strong></para>
    /// <code>
    /// // In Visual Studio Test Explorer, you'll see:
    /// 
    /// // Shared Style:
    /// ✓ TestSharedStyle ("Add(2,3)", Portamical.Core.TestDataTypes.TestDataReturns`1[System.Int32])
    /// ✓ TestSharedStyle ("Add(5,7)", Portamical.Core.TestDataTypes.TestDataReturns`1[System.Int32])
    /// 
    /// // Native Style (cleaner display):
    /// ✓ TestNativeStyle ("Add(2,3)", 2, 3, 5)
    /// ✓ TestNativeStyle ("Add(5,7)", 5, 7, 12)
    /// </code>
    /// 
    /// <para><strong>Example 4: Different Test Data Types</strong></para>
    /// <code>
    /// // TestData (no expected result):
    /// var testData1 = new TestData&lt;string&gt;("Print('hello')", ["hello"]);
    /// var args1 = new[] { testData1 }.ToArgsWithTestCaseName(ArgsCode.Properties);
    /// // Result: ["Print('hello')", "hello"]
    /// // Test signature: void Test(string testCaseName, string value)
    /// 
    /// // TestDataReturns (with expected result):
    /// var testData2 = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// var args2 = new[] { testData2 }.ToArgsWithTestCaseName(ArgsCode.Properties);
    /// // Result: ["Add(2,3)", 2, 3, 5]
    /// // Test signature: void Test(string testCaseName, int x, int y, int expected)
    /// 
    /// // TestDataThrows (with expected exception):
    /// var testData3 = new TestDataThrows&lt;DivideByZeroException&gt;("Divide(1,0)", [1, 0], new DivideByZeroException());
    /// var args3 = new[] { testData3 }.ToArgsWithTestCaseName(ArgsCode.Properties);
    /// // Result: ["Divide(1,0)", 1, 0, DivideByZeroException]
    /// // Test signature: void Test(string testCaseName, int x, int y, DivideByZeroException expectedException)
    /// </code>
    /// 
    /// <para><strong>Example 5: Handling Duplicates</strong></para>
    /// <code>
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [5, 7], 12),  // Same name, different args
    ///     new TestDataReturns&lt;int&gt;("Add(0,0)", [0, 0], 0)
    /// };
    /// 
    /// var args = testData.ToArgsWithTestCaseName(ArgsCode.Properties);
    /// 
    /// // Result (only 2 test cases - duplicate "Add(2,3)" removed):
    /// // ["Add(2,3)", 2, 3, 5]   ← First occurrence kept
    /// // ["Add(0,0)", 0, 0, 0]
    /// // 
    /// // The second "Add(2,3)" [5, 7, 12] is removed because
    /// // TestCaseName equality is based on name only, not arguments.
    /// </code>
    /// </example>
    /// <seealso cref="Portamical.Converters.CollectionConverter.ToDistinctReadOnly{TTestData}(IEnumerable{TTestData}, ArgsCode, PropsCode)"/>
    /// <seealso cref="ArgsCode"/>
    /// <seealso cref="PropsCode"/>
    /// <seealso cref="ITestData"/>
    /// <seealso cref="INamedCase.TestCaseName"/>
    public static IReadOnlyCollection<object?[]> ToArgsWithTestCaseName<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(
        argsCode,
        // Prepends all ITestData properties before args
        // Array format: [TestCaseName, arg1, arg2, ...]
        PropsCode.All);
}