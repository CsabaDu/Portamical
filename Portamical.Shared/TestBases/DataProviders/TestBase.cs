// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters.DataProviders;
using Portamical.DataProviders;

namespace Portamical.Shared.TestBases.DataProviders;

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
    protected static TDataProvider Convert<TDataProvider, TTestData>(
        IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName)
    where TDataProvider: ITestDataProvider<TTestData>, new()
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctDataProvider<TDataProvider, TTestData>(
        initDataProvider: () => new TDataProvider(),
        argsCode,
        testMethodName);

    protected static TDataProvider Convert<TDataProvider, TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TDataProvider : ITestDataProvider<TTestData>, new()
    where TTestData : notnull, ITestData
    => Convert<TDataProvider, TTestData>(testDataCollection, AsInstance);
}
