// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;
using Portamical.DataProviders;
using System.Collections;

namespace Portamical.xUnit.DataProviders;

/// <summary>
/// Provides a data provider for xUnit v2's <c>[MemberData]</c> attribute that converts
/// Portamical test data to the legacy xUnit v2 format.
/// </summary>
/// <typeparam name="TTestData">
/// The type of test data, which must implement <see cref="ITestData"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// <strong>⚠️ xUnit v2 Legacy Support:</strong>
/// </para>
/// <para>
/// This class provides compatibility with xUnit v2 (released 2015), which is now in maintenance mode.
/// For new projects, consider using xUnit v3 with <c>Portamical.xUnit_v3</c>, which provides:
/// <list type="bullet">
///   <item><description>Type-safe <c>TheoryData&lt;T&gt;</c> API</description></item>
///   <item><description>Custom test names via <c>ITheoryDataRow.TestDisplayName</c></description></item>
///   <item><description>Better IDE tooling support</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>xUnit v2 Requirements:</strong>
/// </para>
/// <para>
/// xUnit v2's <c>[MemberData]</c> attribute requires:
/// <list type="bullet">
///   <item><description>
///     <strong>Non-generic <see cref="IEnumerable"/>:</strong> Cannot use <c>IEnumerable&lt;object[]&gt;</c>
///   </description></item>
///   <item><description>
///     <strong><c>object[]</c> arrays:</strong> Each test case is an array of untyped objects
///   </description></item>
///   <item><description>
///     <strong>No custom test names:</strong> Test names are auto-generated as <c>MethodName(param1, param2, ...)</c>
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern: Builder + Iterator</strong>
/// </para>
/// <para>
/// This class combines two patterns:
/// <list type="bullet">
///   <item><description>
///     <strong>Builder:</strong> Use <see cref="AddRow"/> to incrementally build test data collection
///   </description></item>
///   <item><description>
///     <strong>Iterator:</strong> Implements non-generic <see cref="IEnumerable"/> for xUnit v2 consumption
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>ArgsCode Strategy Support:</strong>
/// </para>
/// <para>
/// Supports two test data representation strategies:
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
///     <description>Native Style - Idiomatic xUnit</description>
///   </item>
/// </list>
/// </para>
/// <para>
/// <strong>Factory Method Pattern:</strong>
/// </para>
/// <para>
/// This class has an <c>internal</c> constructor. Instances should be created via the static factory method
/// <c>TestDataProvider.From&lt;TTestData&gt;(testData, argsCode)</c> (assumed to exist in a separate static class).
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Native Style (ArgsCode.Properties) - Recommended</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit.DataProviders;
/// using Portamical.Core.TestDataTypes;
/// 
/// public class CalculatorTests
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12),
///         new("Add(-1,1)", [-1, 1], 0)
///     ];
///     
///     /// &lt;summary&gt;
///     /// Test data provider for xUnit v2 [MemberData].
///     /// &lt;/summary&gt;
///     /// &lt;returns&gt;Non-generic IEnumerable (xUnit v2 requirement).&lt;/returns&gt;
///     public static IEnumerable GetAddTestData()
///     {
///         // Create provider with first test data (Native Style)
///         var provider = TestDataProvider.From(
///             AddTestData[0],
///             ArgsCode.Properties);
///         
///         // Add remaining rows using Builder pattern
///         provider.AddRow(AddTestData[1]);
///         provider.AddRow(AddTestData[2]);
///         
///         return provider;  // Returns IEnumerable (non-generic)
///     }
///     
///     [Theory]
///     [MemberData(nameof(GetAddTestData))]
///     public void TestAdd(int x, int y, int expected)
///     {
///         // Idiomatic xUnit test method with separate typed parameters
///         int result = Calculator.Add(x, y);
///         Assert.Equal(expected, result);
///     }
/// }
/// 
/// // xUnit Test Explorer displays (auto-generated names):
/// // ✓ TestAdd(x: 2, y: 3, expected: 5)
/// // ✓ TestAdd(x: 5, y: 7, expected: 12)
/// // ✓ TestAdd(x: -1, y: 1, expected: 0)
/// </code>
/// 
/// <para><strong>Example 2: Shared Style (ArgsCode.Instance) - Framework-Agnostic</strong></para>
/// <code>
/// public class CalculatorTests
/// {
///     public static IEnumerable GetSharedStyleTestData()
///     {
///         var testData = new[]
///         {
///             new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///             new TestDataReturns&lt;int&gt;("Add(0,0)", [0, 0], 0)
///         };
///         
///         // Create provider with Shared Style (pass entire test data object)
///         var provider = TestDataProvider.From(
///             testData[0],
///             ArgsCode.Instance);
///         
///         provider.AddRow(testData[1]);
///         
///         return provider;
///     }
///     
///     [Theory]
///     [MemberData(nameof(GetSharedStyleTestData))]
///     public void TestAdd(TestDataReturns&lt;int&gt; testData)
///     {
///         // Framework-agnostic test method (same test data works with NUnit, MSTest, xUnit)
///         var args = testData.Args;
///         var expected = testData.Expected;
///         
///         int result = Calculator.Add((int)args[0], (int)args[1]);
///         Assert.Equal(expected, result);
///     }
/// }
/// 
/// // xUnit Test Explorer displays:
/// // ✓ TestAdd(testData: TestDataReturns&lt;Int32&gt;)
/// //   (Note: Less descriptive than Native Style due to xUnit v2 limitations)
/// </code>
/// 
/// <para><strong>Example 3: Dynamic Test Generation (Builder Pattern)</strong></para>
/// <code>
/// public static IEnumerable GetDynamicTestData()
/// {
///     // Start with one test case
///     var provider = TestDataProvider.From(
///         new TestDataReturns&lt;int&gt;("Add(0,0)", [0, 0], 0),
///         ArgsCode.Properties);
///     
///     // Add rows dynamically based on runtime conditions
///     for (int i = 1; i &lt;= 5; i++)
///     {
///         provider.AddRow(
///             new TestDataReturns&lt;int&gt;(
///                 $"Add({i},{i})",
///                 [i, i],
///                 i + i));
///     }
///     
///     return provider;  // Returns 6 test cases (0+0, 1+1, 2+2, 3+3, 4+4, 5+5)
/// }
/// 
/// [Theory]
/// [MemberData(nameof(GetDynamicTestData))]
/// public void TestAddDynamic(int x, int y, int expected)
/// {
///     Assert.Equal(expected, Calculator.Add(x, y));
/// }
/// </code>
/// 
/// <para><strong>Example 4: Comparison with xUnit v3 (Future Migration Path)</strong></para>
/// <code>
/// // xUnit v2 (current - legacy):
/// public static IEnumerable GetTestDataV2()  // ← Non-generic IEnumerable
/// {
///     var provider = TestDataProvider.From(testData, ArgsCode.Properties);
///     return provider;
/// }
/// 
/// // xUnit v3 (future - modern):
/// public static TheoryData&lt;int, int, int&gt; GetTestDataV3()  // ← Type-safe TheoryData
/// {
///     return new TheoryData&lt;int, int, int&gt;
///     {
///         { 2, 3, 5 },
///         { 5, 7, 12 }
///     };
/// }
/// 
/// // Migration strategy: Use Portamical.xUnit_v3 extension when ready
/// </code>
/// </example>
/// <seealso cref="ITestDataProvider{TTestData}"/>
/// <seealso cref="ArgsCode"/>
/// <seealso cref="ITestData"/>
/// <seealso cref="IEnumerable"/>
public sealed class TestDataProvider<TTestData>
: ITestDataProvider<TTestData>,
IEnumerable
where TTestData : notnull, ITestData
{
    private readonly List<object?[]> _dataList = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataProvider{TTestData}"/> class
    /// with the specified test data and argument conversion strategy.
    /// </summary>
    /// <param name="testData">
    /// The first test data item to add to the provider. Additional rows can be added later
    /// via <see cref="AddRow"/>.
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
    ///     (Native Style: idiomatic xUnit test methods)
    ///   </description></item>
    /// </list>
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Factory Method Pattern:</strong>
    /// </para>
    /// <para>
    /// This constructor is <c>internal</c> to encourage usage of the static factory method
    /// <c>TestDataProvider.From&lt;TTestData&gt;(testData, argsCode)</c> (assumed to exist in a companion
    /// static class), which provides cleaner syntax and better API discoverability.
    /// </para>
    /// <para>
    /// <strong>Initialization:</strong>
    /// </para>
    /// <para>
    /// The first test data item is added immediately during construction via <see cref="AddRow"/>.
    /// This ensures the provider always contains at least one test case, preventing empty
    /// test data collections.
    /// </para>
    /// <para>
    /// <strong>Validation:</strong>
    /// </para>
    /// <para>
    /// The <paramref name="argsCode"/> parameter is validated using the <c>Defined</c> extension method
    /// (from <c>Portamical.Core.Safety</c>), which ensures it contains a valid enum value.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="argsCode"/> is not a defined <see cref="ArgsCode"/> enum value.
    /// </exception>
    /// <example>
    /// <code>
    /// // Via factory method (recommended):
    /// var provider = TestDataProvider.From(testData, ArgsCode.Properties);
    /// 
    /// // Direct construction (internal - not recommended):
    /// var provider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;(testData, ArgsCode.Properties);
    /// </code>
    /// </example>
    internal TestDataProvider(TTestData testData, ArgsCode argsCode)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));

        AddRow(testData);
    }

    /// <summary>
    /// Gets the test data representation strategy used by this provider.
    /// </summary>
    /// <value>
    /// An <see cref="ArgsCode"/> value indicating how test data is converted to test method arguments:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Each test case receives the entire <see cref="ITestData"/> object
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Each test case receives flattened property values
    ///   </description></item>
    /// </list>
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is immutable after construction (init-only). All test data rows added via
    /// <see cref="AddRow"/> will be converted using this strategy.
    /// </para>
    /// <para>
    /// <strong>Example Conversion:</strong>
    /// </para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// 
    /// // ArgsCode.Instance:
    /// // Converts to: [testData]  ← Single element: the ITestData instance
    /// 
    /// // ArgsCode.Properties:
    /// // Converts to: [2, 3, 5]  ← Flattened: arg1, arg2, expected
    /// </code>
    /// </remarks>
    public ArgsCode ArgsCode { get; init; }

    /// <summary>
    /// Gets or sets the test method name. Reserved for future use; unused in xUnit v2.
    /// </summary>
    /// <value>
    /// Always <c>null</c> in xUnit v2. This property exists for API consistency with NUnit/MSTest adapters
    /// and is reserved for potential xUnit v3 compatibility.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>⚠️ xUnit v2 Limitation - No Custom Test Names:</strong>
    /// </para>
    /// <para>
    /// xUnit v2 does not provide any API for custom test case names. Test names are automatically
    /// generated by xUnit's test framework in the format:
    /// </para>
    /// <code>
    /// MethodName(param1: value1, param2: value2, ...)
    /// 
    /// // Example:
    /// TestAdd(x: 2, y: 3, expected: 5)
    /// </code>
    /// <para>
    /// <strong>Why This Property Exists:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong>API Consistency:</strong> NUnit and MSTest adapters use <c>testMethodName</c> parameters
    ///     for custom test names. This property maintains a consistent API across frameworks.
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Future Compatibility:</strong> xUnit v3 introduced <c>ITheoryDataRow.TestDisplayName</c>
    ///     for custom test names. This property is reserved for potential migration to xUnit v3 support
    ///     in <c>Portamical.xUnit_v3</c>.
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Interface Implementation:</strong> May be required by <see cref="ITestDataProvider{TTestData}"/>
    ///     interface for cross-framework compatibility.
    ///   </description></item>
    /// </list>
    /// <para>
    /// <strong>Comparison with Other Frameworks:</strong>
    /// </para>
    /// <code>
    /// // NUnit (supports custom names):
    /// var testCase = new TestCaseData(2, 3).SetName("Add(2,3)");
    /// // Test Explorer shows: TestAdd - Add(2,3)
    /// 
    /// // MSTest (supports custom names via parameter):
    /// public void TestAdd(string testCaseName, int x, int y) { }
    /// // Test Explorer shows: TestAdd - Add(2,3)
    /// 
    /// // xUnit v2 (no custom names):
    /// public void TestAdd(int x, int y) { }
    /// // Test Explorer shows: TestAdd(x: 2, y: 3)  ← Auto-generated, cannot customize
    /// 
    /// // xUnit v3 (supports custom names):
    /// var row = new TheoryDataRow&lt;int, int&gt;(2, 3) 
    ///     { TestDisplayName = "Add(2,3)" };
    /// // Test Explorer shows: Add(2,3)
    /// </code>
    /// <para>
    /// <strong>Impact:</strong> None. This property has no effect on xUnit v2 test execution or naming.
    /// </para>
    /// </remarks>
    public string? TestMethodName { get; init; } = null;

    /// <summary>
    /// Adds a test data row to the provider.
    /// </summary>
    /// <param name="testData">
    /// The test data to add. It will be immediately converted to an <c>object[]</c> array
    /// using the provider's <see cref="ArgsCode"/> strategy and stored in the internal collection.
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Builder Pattern:</strong>
    /// </para>
    /// <para>
    /// This method implements the Builder pattern, allowing incremental construction of test data:
    /// </para>
    /// <code>
    /// var provider = TestDataProvider.From(testData1, ArgsCode.Properties);
    /// provider.AddRow(testData2);  // ← Builder method
    /// provider.AddRow(testData3);  // ← Builder method
    /// // Final: 3 test data rows
    /// </code>
    /// <para>
    /// <strong>Conversion Process:</strong>
    /// </para>
    /// <para>
    /// The test data is converted using <see cref="ITestData.ToArgs(ArgsCode)"/>, which:
    /// <list type="bullet">
    ///   <item><description>
    ///     For <see cref="ArgsCode.Instance"/>: Returns <c>[testData]</c> (single element)
    ///   </description></item>
    ///   <item><description>
    ///     For <see cref="ArgsCode.Properties"/>: Returns <c>[arg1, arg2, ..., expected]</c> (flattened)
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The converted array is immediately added to the internal <c>_dataList</c>, so this operation
    /// is eager (not lazy).
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Dynamic Test Generation:</strong></para>
    /// <code>
    /// public static IEnumerable GetDynamicTests()
    /// {
    ///     var provider = TestDataProvider.From(
    ///         new TestDataReturns&lt;int&gt;("Base", [0, 0], 0),
    ///         ArgsCode.Properties);
    ///     
    ///     // Add rows conditionally
    ///     if (shouldTestPositive)
    ///         provider.AddRow(new TestDataReturns&lt;int&gt;("Positive", [2, 3], 5));
    ///     
    ///     if (shouldTestNegative)
    ///         provider.AddRow(new TestDataReturns&lt;int&gt;("Negative", [-1, 1], 0));
    ///     
    ///     return provider;
    /// }
    /// </code>
    /// </example>
    public void AddRow(TTestData testData)
    {
        _dataList.Add(testData.ToArgs(ArgsCode));
    }

    /// <summary>
    /// Returns a non-generic enumerator that iterates through the test data rows.
    /// </summary>
    /// <returns>
    /// A non-generic <see cref="IEnumerator"/> for the collection of <c>object[]</c> test data rows.
    /// Each enumerated element is an <c>object[]</c> containing the test method arguments.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>⚠️ xUnit v2 Requirement - Non-Generic IEnumerator:</strong>
    /// </para>
    /// <para>
    /// This method implements the non-generic <see cref="IEnumerable.GetEnumerator"/> (not the modern
    /// generic <c>IEnumerable&lt;object[]&gt;.GetEnumerator()</c>) to satisfy xUnit v2's <c>[MemberData]</c>
    /// requirements.
    /// </para>
    /// <para>
    /// xUnit v2's <c>[MemberData]</c> attribute expects:
    /// <code>
    /// public static IEnumerable GetTestData()
    /// //             ^^^^^^^^^^^ Non-generic (legacy requirement)
    /// {
    ///     // Must return IEnumerable (not IEnumerable&lt;object[]&gt;)
    ///     return testDataProvider;
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Iteration Result:</strong>
    /// </para>
    /// <para>
    /// Each enumerated item is an <c>object[]</c> array containing test method arguments:
    /// </para>
    /// <code>
    /// // ArgsCode.Properties example:
    /// foreach (var row in provider)
    /// {
    ///     // row is object[] = [2, 3, 5]
    ///     //                    ^^^^^^^ arg1, arg2, expected
    /// }
    /// 
    /// // ArgsCode.Instance example:
    /// foreach (var row in provider)
    /// {
    ///     // row is object[] = [testDataInstance]
    ///     //                    ^^^^^^^^^^^^^^^^^ Entire ITestData object
    /// }
    /// </code>
    /// <para>
    /// <strong>xUnit v3 Note:</strong>
    /// </para>
    /// <para>
    /// xUnit v3 uses the modern <c>IEnumerable&lt;ITheoryDataRow&gt;</c> instead of non-generic <see cref="IEnumerable"/>.
    /// For xUnit v3 support, use <c>Portamical.xUnit_v3</c> extension (under development).
    /// </para>
    /// </remarks>
    public IEnumerator GetEnumerator()
    {
        return _dataList.GetEnumerator();
    }
}