// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.Converters;

namespace Portamical.xUnit_v3.TestBases.TheoryDataRowCollection;

/// <summary>
/// Abstract base class for xUnit v3 test fixtures that provides conversion to immutable
/// <see cref="IReadOnlyCollection{T}"/> of <see cref="ITheoryDataRow"/> (xUnit v3 native rows).
/// </summary>
/// <remarks>
/// <para>
/// <strong>xUnit v3 Native Rows - Lightweight Alternative:</strong>
/// </para>
/// <para>
/// This base class provides a lightweight alternative to
/// <see cref="Portamical.xUnit_v3.TestBases.TestBase"/> by returning immutable collections of
/// xUnit v3's built-in <see cref="ITheoryDataRow"/> instead of Portamical's
/// <see cref="TheoryTestData{TTestData}"/> (Builder pattern).
/// </para>
/// <para>
/// <strong>When to Use This vs Main TestBase:</strong>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Test Base Class</term>
///     <description>Return Type</description>
///     <description>Use Case</description>
///   </listheader>
///   <item>
///     <term><c>Portamical.xUnit_v3.TestBases.TestBase</c></term>
///     <description><see cref="TheoryTestData{TTestData}"/></description>
///     <description>When you need Builder pattern (<c>AddRow</c>) or explicit configuration</description>
///   </item>
///   <item>
///     <term><c>Portamical.xUnit_v3.TestBases.TheoryTestDataRowCollection.TestBase</c></term>
///     <description><see cref="IReadOnlyCollection{T}"/> of <see cref="ITheoryTestDataRow"/></description>
///     <description>When you want Portamical rows with <see cref="INamedCase"/> (deduplication support)</description>
///   </item>
///   <item>
///     <term><c>Portamical.xUnit_v3.TestBases.TheoryDataRowCollection.TestBase</c> (this class)</term>
///     <description><see cref="IReadOnlyCollection{T}"/> of <see cref="ITheoryDataRow"/></description>
///     <description>When you want lightweight xUnit v3 native rows (no Portamical features)</description>
///   </item>
/// </list>
/// <para>
/// <strong>Key Differences from Main TestBase:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>
///     <strong>Return Type:</strong> Returns <see cref="IReadOnlyCollection{T}"/> of <see cref="ITheoryDataRow"/>
///     (xUnit v3 native) instead of <see cref="TheoryTestData{TTestData}"/> (Portamical)
///   </description></item>
///   <item><description>
///     <strong>Immutable:</strong> Collection cannot be modified after creation (no <c>AddRow</c> method)
///   </description></item>
///   <item><description>
///     <strong>Lightweight:</strong> Uses xUnit v3's built-in <see cref="Xunit.v3.TheoryDataRow"/> (simpler)
///   </description></item>
///   <item><description>
///     <strong>No INamedCase:</strong> Rows don't implement <see cref="INamedCase"/> (no manual deduplication support)
///   </description></item>
///   <item><description>
///     <strong>Automatic Deduplication:</strong> Still removes duplicates during conversion (via underlying implementation)
///   </description></item>
/// </list>
/// <para>
/// <strong>Design Pattern: Facade + Template Method</strong>
/// </para>
/// <para>
/// This class is a thin facade over <see cref="CollectionConverter.ToTheoryDataRowCollection{TTestData}"/>,
/// providing a cleaner API for test data source methods:
/// <code>
/// // Facade:
/// testDataCollection.ToTheoryDataRowCollection(argsCode, testMethodName)
///   ↓ delegates to
/// Portamical.Converters.ToDistinctReadOnly(TestDataConverter.ToTheoryDataRow, ...)
///   ↓ creates
/// IReadOnlyCollection&lt;ITheoryDataRow&gt; (xUnit v3 native)
/// </code>
/// </para>
/// <para>
/// <strong>xUnit v3 Native Rows:</strong>
/// </para>
/// <para>
/// This test base returns xUnit v3's built-in <see cref="Xunit.v3.TheoryDataRow"/> instead of Portamical's
/// <see cref="TheoryTestDataRow{TTestData}"/>:
/// <list type="bullet">
///   <item><description>
///     <strong>Lighter Weight:</strong> No Portamical features (just display name + data)
///   </description></item>
///   <item><description>
///     <strong>xUnit v3 Standard:</strong> Uses xUnit's built-in types (better integration)
///   </description></item>
///   <item><description>
///     <strong>Simpler:</strong> No <see cref="INamedCase"/> interface (fewer abstractions)
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Inheritance Chain:</strong>
/// </para>
/// <code>
/// Portamical.TestBases.TestBase (framework-agnostic base)
///   ↓ inherits
/// Portamical.TestBases.TestDataCollection.TestBase (collection-focused base)
///   ↓ inherits
/// Portamical.xUnit_v3.TestBases.TheoryDataRowCollection.TestBase (xUnit v3 native) ← This class
///   ↓ inherits
/// YourTestClass (concrete test fixture)
/// </code>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Basic Usage (Native Style)</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit_v3.TestBases.TheoryDataRowCollection;
/// using Portamical.Core.TestDataTypes;
/// 
/// public class CalculatorTests : TestBase
/// {
///     // Define test data
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12),
///         new("Add(-1,1)", [-1, 1], 0)
///     ];
///     
///     /// &lt;summary&gt;
///     /// Returns immutable collection of xUnit v3 native rows.
///     /// &lt;/summary&gt;
///     public static IReadOnlyCollection&lt;ITheoryDataRow&gt; GetAddTestData()
///     {
///         return Convert(AddTestData, ArgsCode.Properties, testMethodName: "TestAdd");
///         //     ^^^^^^^ Returns IReadOnlyCollection&lt;ITheoryDataRow&gt; (xUnit v3 native)
///     }
///     
///     [Theory]
///     [MemberData(nameof(GetAddTestData))]
///     public void TestAdd(int x, int y, int expected)
///     {
///         int result = Calculator.Add(x, y);
///         Assert.Equal(expected, result);
///     }
/// }
/// 
/// // xUnit v3 Test Explorer displays:
/// // ✓ TestAdd - Add(2,3)
/// // ✓ TestAdd - Add(5,7)
/// // ✓ TestAdd - Add(-1,1)
/// </code>
/// 
/// <para><strong>Example 2: Comparison with Main TestBase</strong></para>
/// <code>
/// // Main TestBase (Portamical collection):
/// public class CalculatorTests : Portamical.xUnit_v3.TestBases.TestBase
/// {
///     public static TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt; GetTestData()
///     {
///         var data = Convert(testData, ArgsCode.Properties, "TestAdd");
///         
///         // Builder pattern: add more rows
///         data.AddRow(new TestDataReturns&lt;int&gt;("Add(10,20)", [10, 20], 30));
///         
///         return data;  // Returns TheoryTestData&lt;T&gt; (mutable)
///     }
/// }
/// 
/// // TheoryDataRowCollection TestBase (xUnit v3 native - this class):
/// public class CalculatorTests : Portamical.xUnit_v3.TestBases.TheoryDataRowCollection.TestBase
/// {
///     public static IReadOnlyCollection&lt;ITheoryDataRow&gt; GetTestData()
///     {
///         return Convert(testData, ArgsCode.Properties, "TestAdd");
///         //     ^^^^^^^ Returns IReadOnlyCollection&lt;ITheoryDataRow&gt; (immutable, lightweight)
///         
///         // ❌ Cannot add rows after conversion (immutable)
///     }
/// }
/// </code>
/// 
/// <para><strong>Example 3: Shared Style (Framework-Agnostic)</strong></para>
/// <code>
/// public static IReadOnlyCollection&lt;ITheoryDataRow&gt; GetTestData()
/// {
///     // Convenience overload - defaults to ArgsCode.Instance
///     return Convert(AddTestData, testMethodName: "TestAdd");
///     //     ^^^^^^^ No ArgsCode parameter
/// }
/// 
/// [Theory]
/// [MemberData(nameof(GetTestData))]
/// public void TestAdd(TestDataReturns&lt;int&gt; testData)
/// {
///     // Framework-agnostic test method (Shared Style)
///     var args = testData.Args;
///     var expected = testData.Expected;
///     
///     int result = Calculator.Add((int)args[0], (int)args[1]);
///     Assert.Equal(expected, result);
/// }
/// </code>
/// </example>
/// <seealso cref="Portamical.TestBases.TestDataCollection.TestBase"/>
/// <seealso cref="Portamical.xUnit_v3.TestBases.TestBase"/>
/// <seealso cref="CollectionConverter"/>
/// <seealso cref="ITheoryDataRow"/>
/// <seealso cref="ArgsCode"/>
public abstract class TestBase : Portamical.TestBases.TestDataCollection.TestBase
{
    /// <summary>
    /// Converts a collection of Portamical test data to an immutable <see cref="IReadOnlyCollection{T}"/>
    /// of xUnit v3 native <see cref="ITheoryDataRow"/>.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must be a reference type implementing <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <param name="argsCode">
    /// Specifies how to convert test data to test method arguments:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire <see cref="ITestData"/> object as single argument
    ///     (Shared Style: framework-agnostic test methods)
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments
    ///     (Native Style: idiomatic xUnit test methods) ✅ <strong>Recommended for xUnit v3</strong>
    ///   </description></item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to test case names in display names.
    /// If provided, display names will be formatted as "testMethodName - TestCaseName".
    /// If <c>null</c>, display names will be just "TestCaseName".
    /// </param>
    /// <returns>
    /// An immutable <see cref="IReadOnlyCollection{T}"/> of <see cref="ITheoryDataRow"/> (xUnit v3's built-in type)
    /// containing converted test data rows, with duplicates removed based on <see cref="ITestData.TestCaseName"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is a facade over <see cref="CollectionConverter.ToTheoryDataRowCollection{TTestData}"/>,
    /// providing a cleaner API for use in test data source methods.
    /// </para>
    /// <para>
    /// <strong>xUnit v3 Native Rows (Lightweight):</strong>
    /// </para>
    /// <para>
    /// The returned collection contains xUnit v3's built-in <see cref="Xunit.v3.TheoryDataRow"/> instances
    /// instead of Portamical's <see cref="TheoryTestDataRow{TTestData}"/>:
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong>Lighter Weight:</strong> No Portamical abstractions (just <c>TestDisplayName</c> + data)
    ///   </description></item>
    ///   <item><description>
    ///     <strong>xUnit v3 Standard:</strong> Uses xUnit's built-in types (better integration)
    ///   </description></item>
    ///   <item><description>
    ///     <strong>No INamedCase:</strong> Rows don't implement <see cref="INamedCase"/> (no manual deduplication)
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Immutable Collection:</strong>
    /// </para>
    /// <para>
    /// The returned collection is immutable (<see cref="IReadOnlyCollection{T}"/>), meaning rows cannot be
    /// added or removed after creation. This is different from <see cref="Portamical.xUnit_v3.TestBases.TestBase.Convert{TTestData}(IEnumerable{TTestData}, ArgsCode, string?)"/>,
    /// which returns a mutable <see cref="TheoryTestData{TTestData}"/> collection with builder pattern support.
    /// </para>
    /// <para>
    /// <strong>Automatic Deduplication:</strong>
    /// </para>
    /// <para>
    /// Although the returned rows don't implement <see cref="INamedCase"/>, deduplication still occurs
    /// during conversion because the underlying <see cref="CollectionConverter.ToTheoryDataRowCollection{TTestData}"/>
    /// uses <c>ToDistinctReadOnly</c>, which removes duplicates based on <see cref="ITestData.TestCaseName"/>
    /// before creating xUnit v3 native rows.
    /// </para>
    /// <para>
    /// <strong>vs. Main TestBase:</strong>
    /// </para>
    /// <para>
    /// Use <see cref="Portamical.xUnit_v3.TestBases.TestBase"/> when you need builder pattern (adding rows after conversion).
    /// Use this test base when you prefer immutable collections and lightweight xUnit v3 integration.
    /// </para>
    /// <para>
    /// <strong>Typical Usage:</strong>
    /// </para>
    /// <code>
    /// public static IReadOnlyCollection&lt;ITheoryDataRow&gt; GetTestData()
    ///     =&gt; Convert(testData, ArgsCode.Properties, "TestAdd");
    /// </code>
    /// </remarks>
    /// <example>
    /// <para><strong>Native Style (ArgsCode.Properties):</strong></para>
    /// <code>
    /// private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
    /// [
    ///     new("Add(2,3)", [2, 3], 5),
    ///     new("Add(5,7)", [5, 7], 12)
    /// ];
    /// 
    /// public static IReadOnlyCollection&lt;ITheoryDataRow&gt; GetAddTestData()
    ///     =&gt; Convert(AddTestData, ArgsCode.Properties, "TestAdd");
    /// 
    /// [Theory]
    /// [MemberData(nameof(GetAddTestData))]
    /// public void TestAdd(int x, int y, int expected)
    /// //                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^ Native Style (flattened)
    /// {
    ///     int result = Calculator.Add(x, y);
    ///     Assert.Equal(expected, result);
    /// }
    /// 
    /// // xUnit v3 Test Explorer:
    /// // ✓ TestAdd - Add(2,3)
    /// // ✓ TestAdd - Add(5,7)
    /// </code>
    /// 
    /// <para><strong>Shared Style (ArgsCode.Instance):</strong></para>
    /// <code>
    /// public static IReadOnlyCollection&lt;ITheoryDataRow&gt; GetAddTestData()
    ///     =&gt; Convert(AddTestData, ArgsCode.Instance, null);
    /// 
    /// [Theory]
    /// [MemberData(nameof(GetAddTestData))]
    /// public void TestAdd(TestDataReturns&lt;int&gt; testData)
    /// //                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Shared Style (entire object)
    /// {
    ///     var args = testData.Args;
    ///     var expected = testData.Expected;
    ///     
    ///     int result = Calculator.Add((int)args[0], (int)args[1]);
    ///     Assert.Equal(expected, result);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Convert{TTestData}(IEnumerable{TTestData}, string?)"/>
    /// <seealso cref="CollectionConverter.ToTheoryDataRowCollection{TTestData}"/>
    /// <seealso cref="ITheoryDataRow"/>
    protected static IReadOnlyCollection<ITheoryDataRow> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryDataRowCollection(
        argsCode,
        testMethodName);

    /// <summary>
    /// Converts a collection of Portamical test data to an immutable <see cref="IReadOnlyCollection{T}"/>
    /// of xUnit v3 native <see cref="ITheoryDataRow"/> using the default <see cref="ArgsCode.Instance"/>
    /// strategy (Shared Style).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must be a reference type implementing <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to test case names in display names.
    /// If provided, display names will be formatted as "testMethodName - TestCaseName".
    /// If <c>null</c>, display names will be just "TestCaseName".
    /// </param>
    /// <returns>
    /// An immutable <see cref="IReadOnlyCollection{T}"/> of <see cref="ITheoryDataRow"/> (xUnit v3's built-in type)
    /// containing converted test data rows, with duplicates removed based on <see cref="ITestData.TestCaseName"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Convenience Overload:</strong>
    /// </para>
    /// <para>
    /// This method is a convenience overload that delegates to the base class
    /// <see cref="Portamical.TestBases.TestDataCollection.TestBase.ConvertAsInstance{TTestData, TResult}(Func{IEnumerable{TTestData}, ArgsCode, string?, TResult}, IEnumerable{TTestData}, string?)"/>
    /// method, which automatically uses <see cref="ArgsCode.Instance"/> (Shared Style).
    /// </para>
    /// <para>
    /// <strong>When to Use:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     Use this overload when you want framework-agnostic test methods (Shared Style)
    ///   </description></item>
    ///   <item><description>
    ///     Use <see cref="Convert{TTestData}(IEnumerable{TTestData}, ArgsCode, string?)"/> 
    ///     when you want explicit control over <see cref="ArgsCode"/> (Native Style recommended for xUnit v3)
    ///   </description></item>
    /// </list>
    /// <para>
    /// <strong>Delegation Chain:</strong>
    /// </para>
    /// <code>
    /// Convert(testDataCollection, testMethodName)
    ///   ↓ delegates to base class
    /// ConvertAsInstance(Convert, testDataCollection, testMethodName)
    ///   ↓ invokes (with ArgsCode.Instance)
    /// Convert(testDataCollection, ArgsCode.Instance, testMethodName)
    ///   ↓ calls
    /// ToTheoryDataRowCollection(ArgsCode.Instance, testMethodName)
    /// </code>
    /// <para>
    /// <strong>Why This Pattern?</strong>
    /// </para>
    /// <para>
    /// The delegation to <see cref="Portamical.TestBases.TestDataCollection.TestBase.ConvertAsInstance{TTestData, TResult}"/>
    /// ensures that the logic for defaulting to <see cref="ArgsCode.Instance"/> is centralized in the
    /// framework-agnostic base class. This avoids duplication across NUnit, MSTest, and xUnit adapters.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
    /// [
    ///     new("Add(2,3)", [2, 3], 5),
    ///     new("Add(5,7)", [5, 7], 12)
    /// ];
    /// 
    /// // Convenience overload - defaults to ArgsCode.Instance
    /// public static IReadOnlyCollection&lt;ITheoryDataRow&gt; GetAddTestData()
    ///     =&gt; Convert(AddTestData, testMethodName: "TestAdd");
    /// //     ^^^^^^^ No ArgsCode parameter
    /// 
    /// [Theory]
    /// [MemberData(nameof(GetAddTestData))]
    /// public void TestAdd(TestDataReturns&lt;int&gt; testData)
    /// {
    ///     // Framework-agnostic test method (Shared Style)
    ///     var args = testData.Args;
    ///     var expected = testData.Expected;
    ///     
    ///     int result = Calculator.Add((int)args[0], (int)args[1]);
    ///     Assert.Equal(expected, result);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Convert{TTestData}(IEnumerable{TTestData}, ArgsCode, string?)"/>
    /// <seealso cref="Portamical.TestBases.TestDataCollection.TestBase.ConvertAsInstance{TTestData, TResult}"/>
    /// <seealso cref="ArgsCode.Instance"/>
    protected static IReadOnlyCollection<ITheoryDataRow> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => ConvertAsInstance(Convert, testDataCollection, testMethodName);
}