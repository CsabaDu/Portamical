// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;

namespace Portamical.TestBases.ObjectArrayCollection;

/// <summary>
/// Provides an abstract base class for converting test data collections into object array format
/// required by parameterized test frameworks (xUnit, NUnit, MSTest).
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="TestBases.TestBase"/> to provide specialized utilities for converting
/// Portamical test data into the <c>object[]</c> format expected by test frameworks. It ensures
/// deduplication and supports both instance-based and property-based test method signatures.
/// </para>
/// <para>
/// <strong>Framework Compatibility:</strong> The converted format works with:
/// <list type="bullet">
///   <item><strong>xUnit:</strong> <c>[Theory, MemberData]</c></item>
///   <item><strong>NUnit:</strong> <c>[Test, TestCaseSource]</c></item>
///   <item><strong>MSTest:</strong> <c>[DataTestMethod, DynamicData]</c></item>
/// </list>
/// </para>
/// <para>
/// <strong>Conversion Strategies:</strong>
/// <list type="bullet">
///   <item><see cref="ArgsCode.Instance"/> - Pass entire test data object (default, object-oriented)</item>
///   <item><see cref="ArgsCode.Properties"/> - Pass flattened properties (functional style)</item>
/// </list>
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// <list type="bullet">
///   <item>Integrating Portamical test data with xUnit/NUnit/MSTest</item>
///   <item>Converting type-safe test data to framework-required format</item>
///   <item>Ensuring test data uniqueness across multiple sources</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // xUnit with Properties mode (flattened parameters)
/// public class CalculatorTests : TestBase
/// {
///     public static IEnumerable&lt;object[]&gt; TestCases 
///         =&gt; Convert(GetTestData(), ArgsCode.Properties);
///     
///     [Theory, MemberData(nameof(TestCases))]
///     public void Add_ValidInputs_ReturnsSum(int arg1, int arg2, int expected)
///     {
///         int actual = Calculator.Add(arg1, arg2);
///         Assert.Equal(expected, actual);
///     }
/// }
/// 
/// // xUnit with Instance mode (test data object) - DEFAULT
/// public class CalculatorTests : TestBase
/// {
///     public static IEnumerable&lt;object[]&gt; TestCases 
///         =&gt; Convert(GetTestData());  // Default: ArgsCode.Instance
///     
///     [Theory, MemberData(nameof(TestCases))]
///     public void Add_ValidInputs_ReturnsSum(TestDataReturns&lt;int, int, int&gt; testData)
///     {
///         int actual = Calculator.Add(testData.Arg1, testData.Arg2);
///         Assert.Equal(testData.Expected, actual);
///     }
/// }
/// </code>
/// </example>
public abstract class TestBase : TestBases.TestBase
{
    /// <summary>
    /// Converts a collection of test data into distinct object arrays using the specified conversion strategy.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must be non-nullable and implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data that may contain duplicates.
    /// </param>
    /// <param name="argsCode">
    /// The conversion strategy: <see cref="ArgsCode.Instance"/> to pass test data objects,
    /// or <see cref="ArgsCode.Properties"/> to pass flattened properties.
    /// </param>
    /// <returns>
    /// A read-only collection where each element is an <c>object[]</c> representing one test case.
    /// Duplicates are removed based on test data equality.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Conversion Strategies:</strong>
    /// <list type="bullet">
    ///   <item><see cref="ArgsCode.Instance"/>: Each <c>object[]</c> contains the test data object itself: <c>[testData]</c></item>
    ///   <item><see cref="ArgsCode.Properties"/>: Each <c>object[]</c> contains flattened properties: <c>[arg1, arg2, ..., expected]</c></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Deduplication:</strong> Test data with identical equality (based on <see cref="ITestData"/>
    /// implementation) is automatically deduplicated, with the first occurrence retained.
    /// </para>
    /// <para>
    /// <strong>Framework Usage:</strong> The returned format is directly compatible with xUnit's <c>MemberData</c>,
    /// NUnit's <c>TestCaseSource</c>, and MSTest's <c>DynamicData</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Properties mode - test method receives individual arguments
    /// public static IEnumerable&lt;object[]&gt; TestCases_Properties 
    ///     =&gt; Convert(testData, ArgsCode.Properties);
    /// // Result: [[2, 3, 5], [10, 20, 30], ...]
    /// 
    /// [Theory, MemberData(nameof(TestCases_Properties))]
    /// public void Test(int arg1, int arg2, int expected)
    /// {
    ///     // Individual parameters
    /// }
    /// 
    /// // Instance mode - test method receives test data object
    /// public static IEnumerable&lt;object[]&gt; TestCases_Instance 
    ///     =&gt; Convert(testData, ArgsCode.Instance);
    /// // Result: [[testData1], [testData2], ...]
    /// 
    /// [Theory, MemberData(nameof(TestCases_Instance))]
    /// public void Test(TestDataReturns&lt;int, int, int&gt; testData)
    /// {
    ///     // Access via testData.Arg1, testData.Arg2, testData.Expected
    /// }
    /// </code>
    /// </example>
    protected static IReadOnlyCollection<object?[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctReadOnly(argsCode);

    /// <summary>
    /// Converts a collection of test data into distinct object arrays using instance mode (default).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must be non-nullable and implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data that may contain duplicates.
    /// </param>
    /// <returns>
    /// A read-only collection where each element is an <c>object[]</c> containing the test data instance.
    /// Duplicates are removed based on test data equality.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience overload that defaults to <see cref="ArgsCode.Instance"/> strategy,
    /// where each <c>object[]</c> contains the test data object itself: <c>[testData]</c>.
    /// </para>
    /// <para>
    /// <strong>When to Use:</strong> Prefer this overload when test methods accept the entire test data
    /// object as a parameter, providing type-safe access to all properties.
    /// </para>
    /// <para>
    /// <strong>Equivalent to:</strong> <c>Convert(testDataCollection, ArgsCode.Instance)</c>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convenience: defaults to ArgsCode.Instance
    /// public static IEnumerable&lt;object[]&gt; TestCases =&gt; Convert(GetTestData());
    /// 
    /// [Theory, MemberData(nameof(TestCases))]
    /// public void Test(TestDataReturns&lt;int, int, int&gt; testData)
    /// {
    ///     // Type-safe access to testData.Arg1, testData.Arg2, testData.Expected
    ///     int actual = Calculator.Add(testData.Arg1, testData.Arg2);
    ///     Assert.Equal(testData.Expected, actual);
    /// }
    /// </code>
    /// </example>
    protected static IReadOnlyCollection<object?[]> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => ConvertAsInstance(Convert, testDataCollection);
}
