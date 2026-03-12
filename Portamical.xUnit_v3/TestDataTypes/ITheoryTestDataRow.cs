// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.xUnit_v3.TestDataTypes;

/// <summary>
/// Represents a test data row for xUnit v3 theory tests that combines xUnit v3's <see cref="ITheoryDataRow"/>
/// with Portamical's <see cref="INamedCase"/> for enhanced test case naming and deduplication.
/// </summary>
/// <remarks>
/// <para>
/// <strong>xUnit v3 Integration - Modern Test Framework:</strong>
/// </para>
/// <para>
/// This interface extends xUnit v3's <see cref="ITheoryDataRow"/> (introduced in xUnit v3, released 2024)
/// with Portamical's <see cref="INamedCase"/> interface to provide:
/// <list type="bullet">
///   <item><description>
///     <strong>Custom Test Names:</strong> xUnit v3's <c>TestDisplayName</c> property for readable test names
///   </description></item>
///   <item><description>
///     <strong>Test Case Naming:</strong> Portamical's <c>TestCaseName</c> property for consistent identification
///   </description></item>
///   <item><description>
///     <strong>Deduplication:</strong> <see cref="INamedCase"/> equality/hashing based on test case name
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern: Interface Composition</strong>
/// </para>
/// <para>
/// This interface composes two existing interfaces without adding new members:
/// <code>
/// ITheoryTestDataRow
///   ↓ inherits
/// ITheoryDataRow (xUnit v3)
///   - string? TestDisplayName { get; }      ← Custom test name (NEW in xUnit v3!)
///   - object?[] GetData()                   ← Test method arguments
///   - string? Skip { get; }                 ← Skip reason
///   - int Timeout { get; }                  ← Test timeout
///   - Type[]? Traits { get; }               ← Test traits/categories
/// 
/// ITheoryTestDataRow
///   ↓ inherits
/// INamedCase (Portamical)
///   - string TestCaseName { get; }          ← Test case identifier
///   - bool Equals(INamedCase? other)        ← Equality based on TestCaseName
///   - int GetHashCode()                     ← Hashing based on TestCaseName
/// </code>
/// </para>
/// <para>
/// <strong>xUnit v3 vs xUnit v2:</strong>
/// </para>
/// <para>
/// xUnit v3 introduced <see cref="ITheoryDataRow"/> as a modern replacement for xUnit v2's <c>TheoryData&lt;T&gt;</c>:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Feature</term>
///     <description>xUnit v2 (Legacy)</description>
///     <description>xUnit v3 (Modern)</description>
///   </listheader>
///   <item>
///     <term>Custom Test Names</term>
///     <description>❌ Not supported</description>
///     <description>✅ <c>ITheoryDataRow.TestDisplayName</c></description>
///   </item>
///   <item>
///     <term>Type Safety</term>
///     <description>⚠️ <c>TheoryData&lt;T&gt;</c> (generic collection)</description>
///     <description>✅ <c>ITheoryDataRow</c> (interface-based)</description>
///   </item>
///   <item>
///     <term>Skip Support</term>
///     <description>⚠️ Via attributes only</description>
///     <description>✅ <c>ITheoryDataRow.Skip</c></description>
///   </item>
///   <item>
///     <term>Timeout Support</term>
///     <description>⚠️ Via attributes only</description>
///     <description>✅ <c>ITheoryDataRow.Timeout</c></description>
///   </item>
///   <item>
///     <term>Traits Support</term>
///     <description>⚠️ Via attributes only</description>
///     <description>✅ <c>ITheoryDataRow.Traits</c></description>
///   </item>
/// </list>
/// <para>
/// <strong>Usage Pattern:</strong>
/// </para>
/// <para>
/// Implementations of this interface (e.g., <c>TheoryTestDataRow&lt;TTestData&gt;</c>) automatically:
/// <list type="number">
///   <item><description>Set <c>TestDisplayName</c> from <c>ITestData.TestCaseName</c></description></item>
///   <item><description>Provide test method arguments via <c>GetData()</c></description></item>
///   <item><description>Enable deduplication via <c>INamedCase.Equals</c></description></item>
///   <item><description>Support xUnit v3's skip, timeout, and traits features</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Implementing ITheoryTestDataRow</strong></para>
/// <code>
/// using Xunit.v3;
/// using Portamical.xUnit_v3.TestDataTypes;
/// 
/// public class TheoryTestDataRow&lt;TTestData&gt; : ITheoryTestDataRow
/// where TTestData : notnull, ITestData
/// {
///     private readonly TTestData _testData;
///     private readonly ArgsCode _argsCode;
///     
///     public TheoryTestDataRow(TTestData testData, ArgsCode argsCode)
///     {
///         _testData = testData;
///         _argsCode = argsCode;
///     }
///     
///     // From ITheoryDataRow (xUnit v3):
///     public string? TestDisplayName =&gt; _testData.TestCaseName;
///     //                                ^^^^^^^^^^^^^^^^^^^^^^ Custom test name!
///     
///     public object?[] GetData()
///     {
///         return _testData.ToArgs(_argsCode);
///         // ArgsCode.Instance:   [testData]
///         // ArgsCode.Properties: [arg1, arg2, ..., expected]
///     }
///     
///     public string? Skip =&gt; null;         // Not skipped
///     public int Timeout =&gt; 0;             // No timeout
///     public Type[]? Traits =&gt; null;       // No traits
///     
///     // From INamedCase (Portamical):
///     public string TestCaseName =&gt; _testData.TestCaseName;
///     
///     public bool Equals(INamedCase? other)
///     =&gt; other != null &amp;&amp; TestCaseName == other.TestCaseName;
///     
///     public override int GetHashCode()
///     =&gt; TestCaseName.GetHashCode(StringComparison.Ordinal);
/// }
/// </code>
/// 
/// <para><strong>Example 2: Using with xUnit v3 [Theory]</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit_v3.TestDataTypes;
/// 
/// public class CalculatorTests
/// {
///     public static IEnumerable&lt;ITheoryDataRow&gt; AddTestData()
///     {
///         var testData = new[]
///         {
///             new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///             new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12),
///             new TestDataReturns&lt;int&gt;("Add(-1,1)", [-1, 1], 0)
///         };
///         
///         return testData.Select(td =&gt;
///             new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(td, ArgsCode.Properties));
///     }
///     
///     [Theory]
///     [MemberData(nameof(AddTestData))]
///     public void TestAdd(int x, int y, int expected)
///     //                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^ Native Style (ArgsCode.Properties)
///     {
///         int result = Calculator.Add(x, y);
///         Assert.Equal(expected, result);
///     }
/// }
/// 
/// // xUnit v3 Test Explorer displays:
/// // ✓ Add(2,3)   ← Custom test name from TestDisplayName!
/// // ✓ Add(5,7)   ← Custom test name from TestDisplayName!
/// // ✓ Add(-1,1)  ← Custom test name from TestDisplayName!
/// </code>
/// 
/// <para><strong>Example 3: Deduplication via INamedCase</strong></para>
/// <code>
/// var testData = new[]
/// {
///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),  // ← Duplicate TestCaseName
///     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
/// };
/// 
/// var rows = testData
///     .Select(td =&gt; new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(td, ArgsCode.Properties))
///     .Distinct()  // ← Uses INamedCase.Equals for deduplication
///     .ToList();
/// 
/// // Result: 2 rows (duplicate "Add(2,3)" removed)
/// </code>
/// 
/// <para><strong>Example 4: xUnit v3 Features (Skip, Timeout, Traits)</strong></para>
/// <code>
/// public class ConditionalTheoryTestDataRow&lt;TTestData&gt; : ITheoryTestDataRow
/// where TTestData : notnull, ITestData
/// {
///     private readonly TTestData _testData;
///     
///     public string? TestDisplayName =&gt; _testData.TestCaseName;
///     public string TestCaseName =&gt; _testData.TestCaseName;
///     
///     // xUnit v3 features:
///     public string? Skip =&gt; Environment.OSVersion.Platform == PlatformID.Unix
///         ? "Test skipped on Unix"
///         : null;  // ← Conditional skip
///     
///     public int Timeout =&gt; 5000;  // ← 5 second timeout
///     
///     public Type[]? Traits =&gt; new[] { typeof(SlowTestTrait) };  // ← Test trait
///     
///     public object?[] GetData() =&gt; _testData.ToArgs(ArgsCode.Properties);
///     
///     // INamedCase implementation...
/// }
/// </code>
/// </example>
/// <seealso cref="Xunit.v3.ITheoryDataRow"/>
/// <seealso cref="INamedCase"/>
/// <seealso cref="ITestData"/>
/// <seealso cref="ArgsCode"/>
public interface ITheoryTestDataRow
: ITheoryDataRow,
  INamedCase;