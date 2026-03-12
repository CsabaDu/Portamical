// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.MSTest.Converters;

namespace Portamical.MSTest.TestBases;

/// <summary>
/// Abstract base class for MSTest test classes, providing MSTest-specific test data conversion helpers.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="Portamical.TestBases.TestBase"/> with MSTest-specific functionality,
/// primarily focused on converting test data collections into <see cref="DynamicDataAttribute"/>-compatible
/// formats with test case names.
/// </para>
/// <para>
/// <strong>Purpose:</strong> Provides convenient <c>Convert</c> methods that test classes can use to
/// transform test data collections into MSTest parameter arrays. This eliminates boilerplate code in
/// individual test classes.
/// </para>
/// <para>
/// <strong>Inheritance Chain:</strong>
/// <code>
/// Your Test Class
///   ↓ inherits
/// Portamical.MSTest.TestBases.TestBase (this class)
///   ↓ inherits
/// Portamical.TestBases.TestBase (shared base)
/// </code>
/// </para>
/// <para>
/// <strong>Why Inherit from This Class?</strong>
/// <list type="bullet">
///   <item>
///     <strong>Simplified Conversion:</strong> Call <c>Convert</c> methods without fully-qualifying
///     extension method namespaces.
///   </item>
///   <item>
///     <strong>Default Behavior:</strong> Parameterless <c>Convert</c> overload provides smart defaults
///     based on test data structure.
///   </item>
///   <item>
///     <strong>Shared Utilities:</strong> Access common test utilities from <c>Portamical.TestBases.TestBase</c>.
///   </item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Basic Test Class Inheritance:</strong></para>
/// <code>
/// using Portamical.MSTest.TestBases;
/// 
/// [TestClass]
/// public class CalculatorTests : TestBase
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] TestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12)
///     ];
///     
///     // Use inherited Convert method
///     public static IEnumerable&lt;object[]&gt; GetTestData()
///         =&gt; Convert(TestData, ArgsCode.InOut);
///     
///     [DataTestMethod]
///     [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
///     public void TestCalculation(string testCaseName, int x, int y, int expected)
///     {
///         var result = Calculator.Add(x, y);
///         Assert.AreEqual(expected, result, $"Test '{testCaseName}' failed");
///     }
/// }
/// </code>
/// 
/// <para><strong>Using Default ArgsCode:</strong></para>
/// <code>
/// [TestClass]
/// public class CalculatorTests : TestBase
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] TestData = [...];
///     
///     // Parameterless Convert uses default ArgsCode
///     public static IEnumerable&lt;object[]&gt; GetTestData()
///         =&gt; Convert(TestData);  // Uses default ArgsCode
/// }
/// </code>
/// </example>
/// <seealso cref="Portamical.TestBases.TestBase"/>
/// <seealso cref="Convert{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
/// <seealso cref="Convert{TTestData}(IEnumerable{TTestData})"/>
/// <seealso cref="CollectionConverter.ToArgsWithTestCaseName{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
public abstract class TestBase : Portamical.TestBases.TestBase
{
    /// <summary>
    /// Converts a collection of test data into MSTest-compatible parameter arrays with explicit argument selection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method transforms test data into the format required by MSTest's <see cref="DynamicDataAttribute"/>.
    /// Each resulting array contains the test case name followed by test arguments.
    /// </para>
    /// <para>
    /// <strong>Array Format:</strong> <c>[testCaseName, arg1, arg2, ..., argN]</c>
    /// </para>
    /// <para>
    /// <strong>Deduplication:</strong> Automatically removes duplicate test cases based on
    /// <see cref="INamedCase.TestCaseName"/> identity using <see cref="NamedCase.Comparer"/>.
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
    ///   <item><see cref="ArgsCode.InOut"/> - Both input and output arguments (most common)</item>
    /// </list>
    /// </param>
    /// <returns>
    /// A read-only collection of object arrays where each array contains:
    /// <c>[testCaseName, arg1, arg2, ..., argN]</c>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> or <paramref name="argsCode"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <example>
    /// <code>
    /// [TestClass]
    /// public class CalculatorTests : TestBase
    /// {
    ///     private static readonly TestDataReturns&lt;int&gt;[] TestData =
    ///     [
    ///         new("Add(2,3)", [2, 3], 5),
    ///         new("Add(5,7)", [5, 7], 12)
    ///     ];
    ///     
    ///     // Explicit ArgsCode
    ///     public static IEnumerable&lt;object[]&gt; GetTestData()
    ///         =&gt; Convert(TestData, ArgsCode.InOut);
    ///     //     ^^^^^^^ Inherited from TestBase
    ///     
    ///     [DataTestMethod]
    ///     [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
    ///     public void TestAdd(string testCaseName, int x, int y, int expected)
    ///     {
    ///         var result = Calculator.Add(x, y);
    ///         Assert.AreEqual(expected, result);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Convert{TTestData}(IEnumerable{TTestData})"/>
    /// <seealso cref="CollectionConverter.ToArgsWithTestCaseName{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
    /// <seealso cref="ArgsCode"/>
    protected static IReadOnlyCollection<object?[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArgsWithTestCaseName(argsCode);

    /// <summary>
    /// Converts a collection of test data into MSTest-compatible parameter arrays using default argument selection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This overload delegates to <see cref="ConvertAsInstance{TTestData, TResult}(Func{IEnumerable{TTestData}, ArgsCode, TResult}, IEnumerable{TTestData})"/>
    /// from the base class, which determines the appropriate <see cref="ArgsCode"/> based on the test data structure
    /// or test class configuration.
    /// </para>
    /// <para>
    /// <strong>Default Behavior:</strong> The <see cref="ArgsCode"/> is typically determined by:
    /// <list type="bullet">
    ///   <item>Test data type structure (e.g., <c>TestDataReturns</c> implies <c>InOut</c>)</item>
    ///   <item>Test class configuration (if base class provides <c>DefaultArgsCode</c> property)</item>
    ///   <item>Framework conventions (fallback to <c>ArgsCode.InOut</c>)</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>When to Use:</strong> Prefer this overload when your test data structure clearly indicates
    /// the argument pattern (e.g., <c>TestDataReturns</c>, <c>TestDataVoid</c>) and you want less verbose code.
    /// Use the explicit <see cref="Convert{TTestData}(IEnumerable{TTestData}, ArgsCode)"/> overload when you need
    /// precise control over which arguments are included.
    /// </para>
    /// </remarks>
    /// <typeparam name="TTestData">
    /// The type of test data to convert. Must implement <see cref="ITestData"/> and cannot be null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <returns>
    /// A read-only collection of object arrays where each array contains:
    /// <c>[testCaseName, arg1, arg2, ..., argN]</c>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <example>
    /// <code>
    /// [TestClass]
    /// public class CalculatorTests : TestBase
    /// {
    ///     private static readonly TestDataReturns&lt;int&gt;[] TestData =
    ///     [
    ///         new("Add(2,3)", [2, 3], 5),
    ///         new("Add(5,7)", [5, 7], 12)
    ///     ];
    ///     
    ///     // Parameterless - uses default ArgsCode
    ///     public static IEnumerable&lt;object[]&gt; GetTestData()
    ///         =&gt; Convert(TestData);  // ArgsCode determined automatically
    ///     
    ///     [DataTestMethod]
    ///     [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
    ///     public void TestAdd(string testCaseName, int x, int y, int expected)
    ///     {
    ///         var result = Calculator.Add(x, y);
    ///         Assert.AreEqual(expected, result);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Convert{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
    /// <seealso cref="Portamical.TestBases.TestBase.ConvertAsInstance{TTestData, TResult}(Func{IEnumerable{TTestData}, ArgsCode, TResult}, IEnumerable{TTestData})"/>
    protected static IReadOnlyCollection<object?[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => ConvertAsInstance(Convert, testDataCollection);
}