// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.DataProviders.Model;
using Portamical.xUnit_v3.TestDataTypes.Model;

namespace Portamical.xUnit_v3.Converters;

/// <summary>
/// Provides extension methods for converting Portamical test data to xUnit v3 data formats.
/// </summary>
/// <remarks>
/// <para>
/// <strong>xUnit v3 Integration - Multiple Conversion Strategies:</strong>
/// </para>
/// <para>
/// This class provides three conversion strategies for xUnit v3:
/// <list type="table">
///   <listheader>
///     <term>Method</term>
///     <description>Return Type</description>
///     <description>Use Case</description>
///   </listheader>
///   <item>
///     <term><see cref="ToTheoryTestData{TTestData}"/></term>
///     <description><see cref="TheoryTestData{TTestData}"/></description>
///     <description>Portamical collection with builder pattern and deduplication</description>
///   </item>
///   <item>
///     <term><see cref="ToTheoryTestDataRow{TTestData}(TTestData, ArgsCode, string?)"/></term>
///     <description><see cref="TheoryTestDataRow{TTestData}"/></description>
///     <description>Portamical row with <see cref="INamedCase"/> and deduplication support</description>
///   </item>
///   <item>
///     <term><see cref="ToTheoryDataRow{TTestData}"/></term>
///     <description><see cref="Xunit.v3.TheoryDataRow"/></description>
///     <description>xUnit v3 native row (lightweight, no deduplication)</description>
///   </item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern: Extension Methods + Factory</strong>
/// </para>
/// <para>
/// All methods are extension methods on <see cref="ITestData"/> or <typeparamref name="TTestData"/>,
/// providing fluent API syntax:
/// <code>
/// // Fluent API:
/// var collection = testData.ToTheoryTestData(ArgsCode.Properties, "TestAdd");
/// var portamicalRow = testData.ToTheoryTestDataRow(ArgsCode.Properties, "TestAdd");
/// var xunitRow = testData.ToTheoryDataRow(ArgsCode.Properties, "TestAdd");
/// </code>
/// </para>
/// <para>
/// <strong>Internal Access Modifiers:</strong>
/// </para>
/// <para>
/// All methods are <c>internal</c> because they are intended to be used by Portamical's xUnit v3 extension
/// framework, not by end users directly. Public APIs (e.g., test data source methods) should use higher-level
/// abstractions like <see cref="TheoryTestData{TTestData}"/> collection or test base classes.
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Portamical Collection (Builder Pattern)</strong></para>
/// <code>
/// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
/// 
/// // Create Portamical collection:
/// TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; collection =
///     testData.ToTheoryTestData(ArgsCode.Properties, "TestAdd");
/// 
/// // collection has builder pattern support:
/// collection.AddRow(new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12));
/// 
/// // collection has automatic deduplication (HashSet)
/// </code>
/// 
/// <para><strong>Example 2: Portamical Row (with INamedCase)</strong></para>
/// <code>
/// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
/// 
/// // Create Portamical row:
/// TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt; row =
///     testData.ToTheoryTestDataRow(ArgsCode.Properties, "TestAdd");
/// 
/// // row implements INamedCase (deduplication support):
/// string testCaseName = row.TestCaseName;  // "Add(2,3)"
/// bool equals = row.Equals(otherRow);      // Compares TestCaseName
/// </code>
/// 
/// <para><strong>Example 3: xUnit v3 Native Row (Lightweight)</strong></para>
/// <code>
/// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
/// 
/// // Create xUnit v3 native row:
/// ITheoryDataRow row =
///     testData.ToTheoryDataRow(ArgsCode.Properties, "TestAdd");
/// 
/// // row is xUnit v3's built-in TheoryDataRow (no INamedCase):
/// string? displayName = row.TestDisplayName;  // "TestAdd - Add(2,3)"
/// object?[] data = row.GetData();             // [2, 3, 5]
/// </code>
/// </example>
/// <seealso cref="TheoryTestData{TTestData}"/>
/// <seealso cref="TheoryTestDataRow{TTestData}"/>
/// <seealso cref="Xunit.v3.TheoryDataRow"/>
/// <seealso cref="ITestData"/>
/// <seealso cref="ArgsCode"/>
public static class TestDataConverter
{
    /// <summary>
    /// Converts Portamical test data to a <see cref="TheoryTestData{TTestData}"/> collection with
    /// builder pattern and automatic deduplication.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must be a reference type implementing <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testData">
    /// The first test data item to add to the collection.
    /// </param>
    /// <param name="argsCode">
    /// Specifies how to convert test data to test method arguments:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire <see cref="ITestData"/> object as single argument (Shared Style)
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments (Native Style)
    ///   </description></item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to test case names in display names.
    /// </param>
    /// <returns>
    /// A <see cref="TheoryTestData{TTestData}"/> collection initialized with the first test data item.
    /// Additional items can be added via <see cref="TheoryTestData{TTestData}.AddRow"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Portamical Collection Features:</strong>
    /// </para>
    /// <para>
    /// The returned <see cref="TheoryTestData{TTestData}"/> collection provides:
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong>Builder Pattern:</strong> Add rows incrementally via <see cref="TheoryTestData{TTestData}.AddRow"/>
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Automatic Deduplication:</strong> Duplicate test cases (same <see cref="INamedCase.TestCaseName"/>) are
    ///     silently ignored
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Configuration:</strong> <see cref="TheoryTestData{TTestData}.ArgsCode"/> and
    ///     <see cref="TheoryTestData{TTestData}.TestMethodName"/> properties control conversion
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>vs. ToTheoryTestDataRow:</strong>
    /// </para>
    /// <para>
    /// Use <see cref="ToTheoryTestData{TTestData}"/> when you need:
    /// <list type="bullet">
    ///   <item><description>Builder pattern (adding rows incrementally)</description></item>
    ///   <item><description>Automatic deduplication across multiple rows</description></item>
    ///   <item><description>Collection-level configuration (ArgsCode, TestMethodName)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Use <see cref="ToTheoryTestDataRow{TTestData}(TTestData, ArgsCode, string?)"/> when you need:
    /// <list type="bullet">
    ///   <item><description>Single row conversion</description></item>
    ///   <item><description>INamedCase interface (for manual deduplication)</description></item>
    ///   <item><description>Row-level control</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// 
    /// // Create collection with builder pattern:
    /// var collection = testData.ToTheoryTestData(
    ///     ArgsCode.Properties,
    ///     "TestAdd");
    /// 
    /// // Add more rows (builder pattern):
    /// collection.AddRow(new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12));
    /// collection.AddRow(new TestDataReturns&lt;int&gt;("Add(-1,1)", [-1, 1], 0));
    /// 
    /// // collection.Count == 3
    /// // Automatic deduplication enabled
    /// </code>
    /// </example>
    /// <seealso cref="TheoryTestData{TTestData}"/>
    /// <seealso cref="ToTheoryTestDataRow{TTestData}(TTestData, ArgsCode, string?)"/>
    internal static TheoryTestData<TTestData> ToTheoryTestData<TTestData>(
        this TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => new(testData, argsCode, testMethodName);

    /// <summary>
    /// Converts Portamical test data to a <see cref="TheoryTestDataRow{TTestData}"/> with
    /// <see cref="INamedCase"/> support for deduplication.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must be a reference type implementing <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testData">
    /// The Portamical test data to convert.
    /// </param>
    /// <param name="argsCode">
    /// Specifies how to convert test data to test method arguments:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire <see cref="ITestData"/> object as single argument
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments
    ///   </description></item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to the test case name in the display name.
    /// </param>
    /// <returns>
    /// A <see cref="TheoryTestDataRow{TTestData}"/> instance that implements both <see cref="ITheoryTestDataRow"/>
    /// and <see cref="INamedCase"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Portamical Row Features:</strong>
    /// </para>
    /// <para>
    /// The returned <see cref="TheoryTestDataRow{TTestData}"/> provides:
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong>ITheoryTestDataRow:</strong> xUnit v3 compatibility (custom display names, skip, timeout, traits)
    ///   </description></item>
    ///   <item><description>
    ///     <strong>INamedCase:</strong> Test case naming with equality and hashing based on <c>TestCaseName</c>
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Type Safety:</strong> Generic type constraint ensures compile-time type checking
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>vs. ToTheoryDataRow (xUnit Native):</strong>
    /// </para>
    /// <para>
    /// Use <see cref="ToTheoryTestDataRow{TTestData}(TTestData, ArgsCode, string?)"/> (Portamical) when you need:
    /// <list type="bullet">
    ///   <item><description><see cref="INamedCase"/> interface (for manual deduplication)</description></item>
    ///   <item><description>Type-safe generic row (<c>TheoryTestDataRow&lt;TTestData&gt;</c>)</description></item>
    ///   <item><description>Richer Portamical features</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Use <see cref="ToTheoryDataRow{TTestData}"/> (xUnit Native) when you need:
    /// <list type="bullet">
    ///   <item><description>Lightweight xUnit v3 built-in row</description></item>
    ///   <item><description>No deduplication support needed</description></item>
    ///   <item><description>Simpler integration with xUnit v3</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// 
    /// // Create Portamical row (generic):
    /// TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt; row =
    ///     testData.ToTheoryTestDataRow(
    ///         ArgsCode.Properties,
    ///         "TestAdd");
    /// 
    /// // ITheoryTestDataRow members:
    /// string? displayName = row.TestDisplayName;  // "TestAdd - Add(2,3)"
    /// object?[] data = row.GetData();             // [2, 3, 5]
    /// 
    /// // INamedCase members:
    /// string testCaseName = row.TestCaseName;     // "Add(2,3)"
    /// bool equals = row.Equals(otherRow);         // Compares TestCaseName
    /// int hash = row.GetHashCode();               // Hash based on TestCaseName
    /// </code>
    /// </example>
    /// <seealso cref="TheoryTestDataRow{TTestData}"/>
    /// <seealso cref="ITheoryTestDataRow"/>
    /// <seealso cref="INamedCase"/>
    internal static TheoryTestDataRow<TTestData> ToTheoryTestDataRow<TTestData>(
        this TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => new(testData, argsCode, testMethodName);

    /// <summary>
    /// Converts Portamical test data to a non-generic <see cref="TheoryTestDataRow"/> (convenience overload).
    /// </summary>
    /// <param name="testData">
    /// The Portamical test data to convert.
    /// </param>
    /// <param name="argsCode">
    /// Specifies how to convert test data to test method arguments.
    /// </param>
    /// <returns>
    /// A <see cref="TheoryTestDataRow"/> instance (non-generic base class).
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Convenience Overload:</strong>
    /// </para>
    /// <para>
    /// This method is a convenience overload of <see cref="ToTheoryTestDataRow{TTestData}(TTestData, ArgsCode, string?)"/>
    /// that:
    /// <list type="bullet">
    ///   <item><description>Accepts non-generic <see cref="ITestData"/> interface</description></item>
    ///   <item><description>Omits <c>testMethodName</c> parameter (passes <c>null</c>)</description></item>
    ///   <item><description>Returns non-generic <see cref="TheoryTestDataRow"/> base class</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>When to Use:</strong>
    /// </para>
    /// <para>
    /// Use this overload when:
    /// <list type="bullet">
    ///   <item><description>You don't need custom test method names</description></item>
    ///   <item><description>You're working with non-generic <see cref="ITestData"/> references</description></item>
    ///   <item><description>You want simpler API syntax</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// ITestData testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// 
    /// // Non-generic convenience overload:
    /// TheoryTestDataRow row = testData.ToTheoryTestDataRow(ArgsCode.Properties);
    /// 
    /// // row.TestDisplayName = "Add(2,3)" (no method name prefix)
    /// </code>
    /// </example>
    /// <seealso cref="ToTheoryTestDataRow{TTestData}(TTestData, ArgsCode, string?)"/>
    internal static TheoryTestDataRow ToTheoryTestDataRow(
        this ITestData testData,
        ArgsCode argsCode)
    => testData.ToTheoryTestDataRow(argsCode, null);

    /// <summary>
    /// Converts Portamical test data to xUnit v3's built-in <see cref="Xunit.v3.TheoryDataRow"/>
    /// (lightweight, no deduplication support).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must be a reference type implementing <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testData">
    /// The Portamical test data to convert.
    /// </param>
    /// <param name="argsCode">
    /// Specifies how to convert test data to test method arguments:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire <see cref="ITestData"/> object as single argument
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments
    ///   </description></item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to the test case name in the display name.
    /// </param>
    /// <returns>
    /// An <see cref="ITheoryDataRow"/> instance (xUnit v3's built-in <see cref="Xunit.v3.TheoryDataRow"/>).
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>xUnit v3 Native Row:</strong>
    /// </para>
    /// <para>
    /// This method creates xUnit v3's built-in <see cref="Xunit.v3.TheoryDataRow"/> instead of Portamical's
    /// <see cref="TheoryTestDataRow{TTestData}"/>. The key differences:
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Feature</term>
    ///     <description>Portamical (ToTheoryTestDataRow)</description>
    ///     <description>xUnit Native (ToTheoryDataRow)</description>
    ///   </listheader>
    ///   <item>
    ///     <term>Return Type</term>
    ///     <description><see cref="TheoryTestDataRow{TTestData}"/></description>
    ///     <description><see cref="Xunit.v3.TheoryDataRow"/></description>
    ///   </item>
    ///   <item>
    ///     <term>INamedCase</term>
    ///     <description>✅ Yes (deduplication support)</description>
    ///     <description>❌ No</description>
    ///   </item>
    ///   <item>
    ///     <term>Type Safety</term>
    ///     <description>✅ Generic (<c>TheoryTestDataRow&lt;TTestData&gt;</c>)</description>
    ///     <description>⚠️ Non-generic (<c>TheoryDataRow</c>)</description>
    ///   </item>
    ///   <item>
    ///     <term>Weight</term>
    ///     <description>Heavier (Portamical features)</description>
    ///     <description>Lighter (xUnit built-in)</description>
    ///   </item>
    /// </list>
    /// <para>
    /// <strong>ArgsCode Handling:</strong>
    /// </para>
    /// <para>
    /// The method handles <see cref="ArgsCode"/> by creating the appropriate row data:
    /// <code>
    /// object row = argsCode == ArgsCode.Properties ?
    ///     testData.ToArgs(argsCode)  // ← Native Style: object[] array
    ///     : testData;                 // ← Shared Style: ITestData object
    /// </code>
    /// </para>
    /// <para>
    /// <strong>When to Use xUnit Native Row:</strong>
    /// </para>
    /// <para>
    /// Use this method when:
    /// <list type="bullet">
    ///   <item><description>You don't need Portamical's deduplication features</description></item>
    ///   <item><description>You want lightweight xUnit v3 integration</description></item>
    ///   <item><description>You're using xUnit v3's built-in test data infrastructure</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Native Style (ArgsCode.Properties):</strong></para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// 
    /// // Create xUnit v3 native row (Native Style):
    /// ITheoryDataRow row = testData.ToTheoryDataRow(
    ///     ArgsCode.Properties,
    ///     "TestAdd");
    /// 
    /// // row is xUnit's TheoryDataRow (not Portamical's):
    /// string? displayName = row.TestDisplayName;  // "TestAdd - Add(2,3)"
    /// object?[] data = row.GetData();             // [2, 3, 5] (flattened)
    /// 
    /// // No INamedCase members available (not implemented)
    /// </code>
    /// 
    /// <para><strong>Shared Style (ArgsCode.Instance):</strong></para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// 
    /// // Create xUnit v3 native row (Shared Style):
    /// ITheoryDataRow row = testData.ToTheoryDataRow(
    ///     ArgsCode.Instance,
    ///     "TestAdd");
    /// 
    /// // row is xUnit's TheoryDataRow:
    /// object?[] data = row.GetData();  // [testData] (entire object)
    /// </code>
    /// </example>
    /// <seealso cref="ToTheoryTestDataRow{TTestData}(TTestData, ArgsCode, string?)"/>
    /// <seealso cref="Xunit.v3.TheoryDataRow"/>
    internal static ITheoryDataRow ToTheoryDataRow<TTestData>(
        this TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    {
        object row = argsCode == ArgsCode.Properties ?
            testData.ToArgs(argsCode)
            : testData;

        // Create the xUnit TheoryDataRow with a unified display name.
        return new TheoryDataRow(row)
        {
            TestDisplayName =
                testData.GetDisplayName(testMethodName)
        };
    }
}