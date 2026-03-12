// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit.Converters;

namespace Portamical.xUnit.TestBases.TheoryData;

/// <summary>
/// Abstract base class for xUnit v2 test fixtures that provides type-safe <see cref="TheoryData{T}"/>
/// conversion methods.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Modern xUnit v2 API - Type-Safe Alternative to Legacy TestBase:</strong>
/// </para>
/// <para>
/// This base class provides a simpler, more type-safe alternative to
/// <see cref="Portamical.xUnit.TestBases.TestBase"/> by returning xUnit v2's generic
/// <see cref="TheoryData{T}"/> instead of the legacy <see cref="TestDataProvider{TTestData}"/> (Builder pattern).
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
///     <term><c>Portamical.xUnit.TestBases.TestBase</c> (legacy)</term>
///     <description><see cref="TestDataProvider{TTestData}"/></description>
///     <description>When you need Builder pattern (<c>AddRow</c>) or explicit <c>ArgsCode</c> control</description>
///   </item>
///   <item>
///     <term><c>Portamical.xUnit.TestBases.TheoryData.TestBase</c> (this class)</term>
///     <description><see cref="TheoryData{T}"/></description>
///     <description>When you want type-safe collections and simpler API</description>
///   </item>
/// </list>
/// <para>
/// <strong>Key Differences from Main TestBase:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>
///     <strong>Type Safety:</strong> Returns generic <see cref="TheoryData{T}"/> instead of non-generic <c>IEnumerable</c>
///   </description></item>
///   <item><description>
///     <strong>No ArgsCode Parameter:</strong> Always uses <see cref="ArgsCode.Instance"/> (Shared Style) implicitly
///   </description></item>
///   <item><description>
///     <strong>No Builder Pattern:</strong> <see cref="TheoryData{T}"/> is immutable after creation (cannot add rows dynamically)
///   </description></item>
///   <item><description>
///     <strong>Simpler API:</strong> Single method, no overloads
///   </description></item>
/// </list>
/// <para>
/// <strong>Design Pattern: Facade</strong>
/// </para>
/// <para>
/// This class is a thin facade over <see cref="CollectionConverter.ToTheoryData{TTestData}"/>,
/// providing a cleaner API for test data source methods.
/// </para>
/// <para>
/// <strong>Shared Style Only:</strong>
/// </para>
/// <para>
/// This test base always uses <see cref="ArgsCode.Instance"/> (Shared Style), meaning test methods
/// receive the entire <see cref="ITestData"/> object as a parameter. xUnit v2 does not support
/// Native Style (<see cref="ArgsCode.Properties"/>) with flattened parameters.
/// </para>
/// <para>
/// <strong>Inheritance Chain:</strong>
/// <code>
/// Portamical.TestBases.TestBase (framework-agnostic base)
///   ↓ inherits
/// Portamical.xUnit.TestBases.TheoryData.TestBase (xUnit v2 type-safe variant) ← This class
///   ↓ inherits
/// YourTestClass (concrete test fixture)
/// </code>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Basic Usage (Type-Safe TheoryData)</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit.TestBases.TheoryData;
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
///     /// Returns type-safe TheoryData&lt;T&gt; for xUnit v2.
///     /// &lt;/summary&gt;
///     public static TheoryData&lt;TestDataReturns&lt;int&gt;&gt; GetAddTestData()
///     {
///         return Convert(AddTestData);  // ← Type-safe conversion
///         //     ^^^^^^^ No ArgsCode parameter (implicit Shared Style)
///     }
///     
///     [Theory]
///     [MemberData(nameof(GetAddTestData))]
///     public void TestAdd(TestDataReturns&lt;int&gt; testData)
///     {
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
/// // ✓ TestAdd(testData: TestDataReturns&lt;Int32&gt;)
/// // ✓ TestAdd(testData: TestDataReturns&lt;Int32&gt;)
/// </code>
/// 
/// <para><strong>Example 2: Comparison with Main TestBase (Legacy)</strong></para>
/// <code>
/// // Using TheoryData TestBase (this class - modern):
/// public class CalculatorTests : Portamical.xUnit.TestBases.TheoryData.TestBase
/// {
///     public static TheoryData&lt;TestDataReturns&lt;int&gt;&gt; GetTestData()
///     {
///         return Convert(testData);  // ← Returns TheoryData&lt;T&gt; (type-safe)
///         //     ^^^^^^^ No ArgsCode parameter
///     }
///     
///     [Theory]
///     [MemberData(nameof(GetTestData))]
///     public void TestAdd(TestDataReturns&lt;int&gt; testData) { }
/// }
/// 
/// // Using Main TestBase (legacy):
/// public class CalculatorTests : Portamical.xUnit.TestBases.TestBase
/// {
///     public static IEnumerable GetTestData()
///     {
///         return Convert(testData, ArgsCode.Instance);  // ← Returns IEnumerable (non-generic)
///         //     ^^^^^^^ Explicit ArgsCode parameter
///     }
///     
///     [Theory]
///     [MemberData(nameof(GetTestData))]
///     public void TestAdd(TestDataReturns&lt;int&gt; testData) { }
/// }
/// </code>
/// 
/// <para><strong>Example 3: Why Builder Pattern Not Supported</strong></para>
/// <code>
/// // Main TestBase (legacy) - supports Builder pattern:
/// public static IEnumerable GetTestData()
/// {
///     var provider = Convert(testData, ArgsCode.Instance);
///     
///     // Can add rows dynamically (Builder pattern):
///     provider.AddRow(new TestDataReturns&lt;int&gt;("Extra", [10, 20], 30));
///     
///     return provider;
/// }
/// 
/// // TheoryData TestBase (this class) - no Builder pattern:
/// public static TheoryData&lt;TestDataReturns&lt;int&gt;&gt; GetTestData()
/// {
///     var theoryData = Convert(testData);
///     
///     // ❌ Cannot add rows dynamically:
///     // theoryData.Add(...);  ← Compile error: TheoryData is immutable after Convert
///     
///     // ✅ Must include all data in initial collection:
///     var allData = testData.Concat(new[] { new TestDataReturns&lt;int&gt;("Extra", [10, 20], 30) });
///     return Convert(allData);
/// }
/// </code>
/// </example>
/// <seealso cref="Portamical.TestBases.TestBase"/>
/// <seealso cref="Portamical.xUnit.TestBases.TestBase"/>
/// <seealso cref="CollectionConverter"/>
/// <seealso cref="TheoryData{T}"/>
/// <seealso cref="ArgsCode"/>
public abstract class TestBase : Portamical.TestBases.TestBase
{
    /// <summary>
    /// Converts a collection of Portamical test data to xUnit v2's type-safe <see cref="TheoryData{T}"/> collection.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data, which must implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of Portamical test data to convert.
    /// </param>
    /// <returns>
    /// A type-safe <see cref="TheoryData{TTestData}"/> instance containing deduplicated test data items.
    /// Each item is added as a single-element <c>object[]</c> array (Shared Style).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is a facade over <see cref="CollectionConverter.ToTheoryData{TTestData}"/>,
    /// providing a cleaner API for use in test data source methods.
    /// </para>
    /// <para>
    /// <strong>Type-Safe Generic Collection:</strong>
    /// </para>
    /// <para>
    /// xUnit v2's <see cref="TheoryData{T}"/> provides compile-time type safety compared to the legacy
    /// non-generic <c>IEnumerable&lt;object[]&gt;</c>. The compiler enforces that all test data items
    /// are of type <typeparamref name="TTestData"/>.
    /// </para>
    /// <code>
    /// // Type-safe (this method returns this):
    /// TheoryData&lt;TestDataReturns&lt;int&gt;&gt; data = Convert(testData);
    /// //                              ^^^^^^^^^^^ Compile-time type checking
    /// 
    /// // Legacy (non-generic):
    /// IEnumerable data = Convert(testData, ArgsCode.Instance);
    /// //          ^^^ No compile-time type checking
    /// </code>
    /// <para>
    /// <strong>Shared Style (ArgsCode.Instance) Implicit:</strong>
    /// </para>
    /// <para>
    /// This method always uses <see cref="ArgsCode.Instance"/> (Shared Style) implicitly, meaning each
    /// test data item is passed as a complete <see cref="ITestData"/> object to the test method.
    /// There is no <c>argsCode</c> parameter because <see cref="TheoryData{T}"/> always works with
    /// complete objects (no property flattening).
    /// </para>
    /// <code>
    /// // Test method signature (Shared Style):
    /// [Theory]
    /// [MemberData(nameof(GetTestData))]
    /// public void TestMethod(TestDataReturns&lt;int&gt; testData)
    /// //                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Receives entire object
    /// {
    ///     var args = testData.Args;
    ///     var expected = testData.Expected;
    ///     // ...
    /// }
    /// </code>
    /// <para>
    /// <strong>Automatic Deduplication:</strong>
    /// </para>
    /// <para>
    /// This method automatically removes duplicate test cases based on <see cref="INamedCase.TestCaseName"/>
    /// equality. This ensures that tests with identical <c>TestCaseName</c> values are only included once.
    /// </para>
    /// <para>
    /// <strong>No Builder Pattern:</strong>
    /// </para>
    /// <para>
    /// Unlike <see cref="TestDataProvider{TTestData}"/> (returned by main TestBase), <see cref="TheoryData{T}"/>
    /// does not support the Builder pattern. You cannot add rows dynamically after calling this method.
    /// All test data must be included in the <paramref name="testDataCollection"/> passed to this method.
    /// </para>
    /// <para>
    /// <strong>When to Use Main TestBase Instead:</strong>
    /// </para>
    /// <para>
    /// Use <see cref="Portamical.xUnit.TestBases.TestBase"/> (main variant) if you need:
    /// <list type="bullet">
    ///   <item><description>Builder pattern (<c>AddRow</c> method)</description></item>
    ///   <item><description>Explicit <see cref="ArgsCode"/> control</description></item>
    ///   <item><description>Dynamic test data generation</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Basic Usage:</strong></para>
    /// <code>
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
    /// };
    /// 
    /// TheoryData&lt;TestDataReturns&lt;int&gt;&gt; theoryData = Convert(testData);
    /// 
    /// // Result: TheoryData&lt;TestDataReturns&lt;int&gt;&gt; with 2 items
    /// // theoryData[0] = new object[] { testData[0] }
    /// // theoryData[1] = new object[] { testData[1] }
    /// </code>
    /// 
    /// <para><strong>With Test Method:</strong></para>
    /// <code>
    /// public static TheoryData&lt;TestDataReturns&lt;int&gt;&gt; GetAddTestData()
    /// {
    ///     var testData = new[]
    ///     {
    ///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///         new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
    ///     };
    ///     
    ///     return Convert(testData);
    /// }
    /// 
    /// [Theory]
    /// [MemberData(nameof(GetAddTestData))]
    /// public void TestAdd(TestDataReturns&lt;int&gt; testData)
    /// {
    ///     int result = Calculator.Add((int)testData.Args[0], (int)testData.Args[1]);
    ///     Assert.Equal(testData.Expected, result);
    /// }
    /// </code>
    /// 
    /// <para><strong>Automatic Deduplication:</strong></para>
    /// <code>
    /// var testData = new[]
    /// {
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),  // ← Duplicate TestCaseName
    ///     new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12)
    /// };
    /// 
    /// var theoryData = Convert(testData);
    /// // Result: Only 2 items (duplicate "Add(2,3)" removed)
    /// </code>
    /// </example>
    /// <seealso cref="Portamical.xUnit.TestBases.TestBase.Convert{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
    /// <seealso cref="CollectionConverter.ToTheoryData{TTestData}"/>
    /// <seealso cref="TheoryData{T}"/>
    protected static TheoryData<TTestData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryData();
}