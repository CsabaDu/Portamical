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
/// Unlike NUnit's <c>TestCaseData.SetName()</c> or xUnit's test method naming,
/// MSTest doesn't have built-in test case naming. The standard pattern is to include
/// the test case name as a parameter in your test method signature.
/// </para>
/// </remarks>
public static class CollectionConverter
{
    /// <summary>
    /// Converts a collection of test data into MSTest-compatible parameter arrays with test case names.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>MSTest Integration Pattern:</strong>
    /// <list type="number">
    ///   <item>Create a static method returning <c>IEnumerable&lt;object[]&gt;</c></item>
    ///   <item>Call this extension method on your test data collection</item>
    ///   <item>Decorate your test with <c>[DynamicData(nameof(YourMethod), DynamicDataSourceType.Method)]</c></item>
    ///   <item>Test method signature must match: <c>(string testCaseName, arg1, arg2, ..., argN)</c></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Parameter Order:</strong> Each returned array contains the test case name followed by
    /// test arguments: <c>[testCaseName, arg1, arg2, ..., argN]</c>
    /// </para>
    /// <para>
    /// <strong>Deduplication:</strong> Uses <see cref="NamedCase.Comparer"/> to automatically
    /// remove duplicate test cases based on <see cref="INamedCase.TestCaseName"/> identity.
    /// </para>
    /// <para>
    /// <strong>Property Inclusion:</strong> Uses <see cref="PropsCode.All"/>, which
    /// includes all properties from <see cref="ITestData"/> (currently only <c>TestCaseName</c>).
    /// Properties are placed <strong>before</strong> test arguments in the resulting arrays.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">
    /// The type of test data to convert. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="argsCode">
    /// Specifies which arguments to extract from each test data item:
    /// <list type="bullet">
    ///   <item><see cref="ArgsCode.In"/> - Only input arguments</item>
    ///   <item><see cref="ArgsCode.Out"/> - Only output/expected values</item>
    ///   <item><see cref="ArgsCode.InOut"/> - Both input and output arguments</item>
    /// </list>
    /// </param>
    /// <returns>
    /// A read-only collection of object arrays suitable for MSTest's <see cref="DynamicDataAttribute"/>.
    /// Each array contains the test case name followed by test arguments.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="argsCode"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <example>
    /// <para><strong>Basic Usage:</strong></para>
    /// <code>
    /// // 1. Define test data
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5)  // Duplicate - will be removed
    /// };
    /// 
    /// // 2. Create data provider method
    /// public static IEnumerable&lt;object[]&gt; GetCalculationTestData()
    ///     =&gt; testData.ToArgsWithTestCaseName(ArgsCode.InOut);
    /// 
    /// // 3. Use in test method
    /// [DataTestMethod]
    /// [DynamicData(nameof(GetCalculationTestData), DynamicDataSourceType.Method)]
    /// public void TestCalculation(string testCaseName, int x, int y, int expected)
    /// {
    ///     // testCaseName will be "Add(2,3)" or "Add(5,7)" for identification
    ///     var result = Calculate(x, y);
    ///     Assert.AreEqual(expected, result, $"Test '{testCaseName}' failed");
    /// }
    /// </code>
    /// 
    /// <para><strong>Result Format:</strong></para>
    /// <code>
    /// // Each test case becomes an array:
    /// ["Add(2,3)", 2, 3, 5]  // ← First test case
    /// ["Add(5,7)", 5, 7, 12]  // ← Second test case
    /// // ^name^ ^args^ ^exp^
    /// //
    /// // The duplicate "Add(2,3)" is automatically removed
    /// </code>
    /// 
    /// <para><strong>Test Explorer Display:</strong></para>
    /// <code>
    /// // In Visual Studio Test Explorer, you'll see:
    /// ✓ TestCalculation ("Add(2,3)", 2, 3, 5)
    /// ✓ TestCalculation ("Add(5,7)", 5, 7, 12)
    /// </code>
    /// 
    /// <para><strong>Different ArgsCode Options:</strong></para>
    /// <code>
    /// // Only inputs (for void methods or when expected value is checked differently)
    /// var inputsOnly = testData.ToArgsWithTestCaseName(ArgsCode.In);
    /// // Result: ["Add(2,3)", 2, 3]
    /// 
    /// // Only outputs (for testing factory methods)
    /// var outputsOnly = testData.ToArgsWithTestCaseName(ArgsCode.Out);
    /// // Result: ["Add(2,3)", 5]
    /// 
    /// // Both inputs and outputs (most common)
    /// var both = testData.ToArgsWithTestCaseName(ArgsCode.InOut);
    /// // Result: ["Add(2,3)", 2, 3, 5]
    /// </code>
    /// </example>
    /// <seealso cref="Portamical.Converters.CollectionConverter.ToDistinctReadOnly{TTestData}(IEnumerable{TTestData}, ArgsCode, PropsCode)"/>
    /// <seealso cref="ArgsCode"/>
    /// <seealso cref="PropsCode"/>
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