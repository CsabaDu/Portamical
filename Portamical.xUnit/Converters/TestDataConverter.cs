// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit.DataProviders;

namespace Portamical.xUnit.Converters;

/// <summary>
/// Provides factory methods for creating xUnit v2 data providers from Portamical test data.
/// </summary>
/// <remarks>
/// <para>
/// <strong>⚠️ xUnit v2 Legacy Support:</strong>
/// </para>
/// <para>
/// This class provides factory methods for creating <see cref="TestDataProvider{TTestData}"/> instances
/// with signature compatibility for the <c>Portamical.Converters.CollectionConverter.ToDataProvider</c>
/// extension method.
/// </para>
/// <para>
/// <strong>Design Pattern: Factory Method + Adapter</strong>
/// </para>
/// <para>
/// The factory method <see cref="InitTestDataProvider{TTestData}"/> adapts the framework-agnostic
/// <c>ToDataProvider</c> signature (which includes <c>testMethodName</c> for NUnit/MSTest) to xUnit v2's
/// requirements (which doesn't support custom test names):
/// </para>
/// <code>
/// // Framework-agnostic signature (3 parameters):
/// Func&lt;TTestData, ArgsCode, string?, TDataProvider&gt; initDataProvider
/// 
/// // xUnit v2 TestDataProvider constructor (2 parameters):
/// new TestDataProvider&lt;TTestData&gt;(testData, argsCode)
/// //                                           ^^^^^^^^ No testMethodName
/// 
/// // InitTestDataProvider bridges the gap:
/// InitTestDataProvider(testData, argsCode, testMethodName)
/// =&gt; new TestDataProvider&lt;TTestData&gt;(testData, argsCode)
/// //                                           ^^^^^^^^ Ignores testMethodName
/// </code>
/// <para>
/// <strong>xUnit v2 Limitation - Unused testMethodName Parameter:</strong>
/// </para>
/// <para>
/// The <c>testMethodName</c> parameter is included for signature compatibility with the framework-agnostic
/// <c>ToDataProvider</c> method but is intentionally unused because:
/// <list type="bullet">
///   <item><description>
///     <strong>xUnit v2 No Custom Test Names:</strong> xUnit v2 auto-generates test names as
///     <c>MethodName(param1, param2, ...)</c> with no API for customization
///   </description></item>
///   <item><description>
///     <strong>Cross-Framework Consistency:</strong> NUnit and MSTest use <c>testMethodName</c> for custom test names,
///     so the framework-agnostic signature includes it
///   </description></item>
///   <item><description>
///     <strong>Future Compatibility:</strong> xUnit v3 supports custom test names via <c>ITheoryDataRow.TestDisplayName</c>,
///     so this parameter is reserved for potential xUnit v3 support
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Usage Pattern:</strong>
/// </para>
/// <para>
/// This factory method is typically used as a method group parameter to <c>CollectionConverter.ToDataProvider</c>:
/// </para>
/// <code>
/// var testDataCollection = new[] { testData1, testData2, testData3 };
/// 
/// var provider = testDataCollection.ToDataProvider(
///     TestDataConverter.InitTestDataProvider,  // ← Method group
///     ArgsCode.Instance,
///     testMethodName: null);
/// </code>
/// </remarks>
/// <seealso cref="TestDataProvider{TTestData}"/>
/// <seealso cref="ArgsCode"/>
/// <seealso cref="ITestData"/>
public static class TestDataConverter
{
    /// <summary>
    /// Creates a new <see cref="TestDataProvider{TTestData}"/> instance from the specified test data
    /// and argument conversion strategy.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testData">
    /// The first test data item to add to the provider. Additional rows can be added later
    /// via <see cref="TestDataProvider{TTestData}.AddRow"/>.
    /// </param>
    /// <param name="argsCode">
    /// Specifies the test data representation strategy. For xUnit v2, only <see cref="ArgsCode.Instance"/>
    /// (Shared Style) is supported due to xUnit v2 limitations.
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name. <strong>This parameter is unused in xUnit v2</strong> because xUnit v2
    /// does not support custom test names. It exists for signature compatibility with the framework-agnostic
    /// <c>Portamical.Converters.CollectionConverter.ToDataProvider</c> method.
    /// </param>
    /// <returns>
    /// A new <see cref="TestDataProvider{TTestData}"/> instance initialized with the specified test data
    /// and argument conversion strategy.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Factory Method Pattern:</strong>
    /// </para>
    /// <para>
    /// This method serves as a factory method for <see cref="TestDataProvider{TTestData}"/> that matches
    /// the delegate signature required by <c>CollectionConverter.ToDataProvider</c>:
    /// </para>
    /// <code>
    /// Func&lt;TTestData, ArgsCode, string?, TestDataProvider&lt;TTestData&gt;&gt; initDataProvider
    /// </code>
    /// <para>
    /// <strong>Unused Parameter - testMethodName:</strong>
    /// </para>
    /// <para>
    /// The <paramref name="testMethodName"/> parameter is intentionally unused (suppressed with
    /// <c>#pragma warning disable IDE0060</c>) because:
    /// </para>
    /// <list type="number">
    ///   <item><description>
    ///     <strong>xUnit v2 Limitation:</strong> xUnit v2 does not provide any API for custom test case names.
    ///     Test names are automatically generated as <c>MethodName(param1: value1, param2: value2, ...)</c>.
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Signature Compatibility:</strong> The framework-agnostic <c>CollectionConverter.ToDataProvider</c>
    ///     method requires a 3-parameter delegate to support NUnit and MSTest (which do support custom test names).
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Cross-Framework Consistency:</strong> All framework adapters (NUnit, MSTest, xUnit) use the same
    ///     <c>ToDataProvider</c> method, so the signature must be consistent.
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Future Compatibility:</strong> xUnit v3 introduced <c>ITheoryDataRow.TestDisplayName</c> for
    ///     custom test names. This parameter is reserved for potential xUnit v3 support in <c>Portamical.xUnit_v3</c>.
    ///   </description></item>
    /// </list>
    /// <para>
    /// <strong>Internal Access Modifier:</strong>
    /// </para>
    /// <para>
    /// This method is <c>internal</c> because it's only intended to be used as a method group parameter to
    /// <c>CollectionConverter.ToDataProvider</c>. External code should use the <c>ToDataProvider</c> extension
    /// method directly, not this factory method.
    /// </para>
    /// <para>
    /// <strong>Comparison with Other Frameworks:</strong>
    /// </para>
    /// <code>
    /// // NUnit (uses testMethodName):
    /// InitTestCaseTestData(testData, argsCode, testMethodName)
    /// =&gt; TestCaseTestData.From(testData, argsCode, testMethodName)
    /// //                                            ^^^^^^^^^^^^^^^ Used for display name
    /// 
    /// // MSTest (uses testMethodName):
    /// InitArgsWithTestCaseName(testData, argsCode, testMethodName)
    /// =&gt; testData.ToArgsWithTestCaseName(argsCode, testMethodName)
    /// //                                            ^^^^^^^^^^^^^^^ Used for display name
    /// 
    /// // xUnit v2 (ignores testMethodName):
    /// InitTestDataProvider(testData, argsCode, testMethodName)
    /// =&gt; new TestDataProvider&lt;TTestData&gt;(testData, argsCode)
    /// //                                           ^^^^^^^^ testMethodName not passed
    /// </code>
    /// </remarks>
    /// <example>
    /// <para><strong>Usage as Method Group (Typical):</strong></para>
    /// <code>
    /// var testDataCollection = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12),
    ///     new TestDataReturns&lt;int&gt;("Add(-1,1)", [-1, 1], 0)
    /// };
    /// 
    /// // Method group syntax (clean):
    /// var provider = testDataCollection.ToDataProvider(
    ///     TestDataConverter.InitTestDataProvider,  // ← Method group
    ///     ArgsCode.Instance,
    ///     testMethodName: null);  // ← Unused in xUnit v2
    /// </code>
    /// 
    /// <para><strong>Direct Call (Rare - Not Recommended):</strong></para>
    /// <code>
    /// // Direct call (verbose, not recommended):
    /// var provider = TestDataConverter.InitTestDataProvider(
    ///     testData,
    ///     ArgsCode.Instance,
    ///     testMethodName: null);  // ← Unused in xUnit v2
    /// 
    /// // Result: TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt; with 1 row
    /// provider.AddRow(testData2);  // Add more rows via Builder pattern
    /// </code>
    /// 
    /// <para><strong>Lambda Alternative (Verbose - Not Used):</strong></para>
    /// <code>
    /// // Why method group is better than lambda:
    /// var provider = testDataCollection.ToDataProvider(
    ///     (td, ac, tmn) =&gt; new TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;(td, ac),
    ///     //              ^^^ Lambda ignores tmn - verbose
    ///     ArgsCode.Instance,
    ///     testMethodName: null);
    /// 
    /// // vs. method group (current):
    /// var provider = testDataCollection.ToDataProvider(
    ///     TestDataConverter.InitTestDataProvider,  // ← Clean, concise
    ///     ArgsCode.Instance,
    ///     testMethodName: null);
    /// </code>
    /// </example>
    /// <seealso cref="TestDataProvider{TTestData}"/>
    /// <seealso cref="ArgsCode"/>
    internal static TestDataProvider<TTestData> InitTestDataProvider<TTestData>(
        TTestData testData,
        ArgsCode argsCode,
#pragma warning disable IDE0060 // Remove unused parameter
        // Justification:
        // The 'testMethodName' parameter is required for signature compatibility with the
        // framework-agnostic 'CollectionConverter.ToDataProvider' method, which uses the delegate:
        //     Func<TTestData, ArgsCode, string?, TDataProvider> initDataProvider
        //
        // This parameter is intentionally unused in xUnit v2 because:
        // 1. xUnit v2 does not support custom test case names (auto-generates as "Method(param1, param2, ...)")
        // 2. NUnit and MSTest use this parameter for custom test names, so the signature must be consistent
        // 3. xUnit v3 supports custom test names via ITheoryDataRow.TestDisplayName (reserved for future use)
        //
        // See class-level documentation for detailed explanation.
        string? testMethodName = null)
#pragma warning restore IDE0060 // Remove unused parameter
    where TTestData : notnull, ITestData
    => new(testData, argsCode);
}