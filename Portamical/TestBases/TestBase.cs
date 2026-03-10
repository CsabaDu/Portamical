// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.TestBases;

/// <summary>
/// Provides a base class for test implementations with framework-agnostic conversion utilities.
/// </summary>
/// <remarks>
/// <para>
/// This class provides:
/// <list type="bullet">
/// <item><description>Constants for <see cref="ArgsCode"/> serialization strategies (<see cref="AsInstance"/>, <see cref="AsProperties"/>)</description></item>
/// <item><description>Helper methods for converting test data using default <see cref="ArgsCode.Instance"/> strategy</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> All members are thread-safe (stateless design).
/// </para>
/// <para>
/// <strong>Example Usage (Derived Class):</strong>
/// <code>
/// public class MSTestTestBase : TestBase
/// {
///     protected static IEnumerable&lt;object?[]&gt; Convert&lt;TTestData&gt;(
///         IEnumerable&lt;TTestData&gt; testDataCollection)
///     where TTestData : notnull, ITestData
///     =&gt; ConvertAsInstance(
///         (data, argsCode, _) =&gt; data.ToArgsWithTestCaseName(argsCode, PropsCode.All),
///         testDataCollection,
///         testMethodName: null);
/// }
/// </code>
/// </para>
/// </remarks>
public abstract class TestBase
{
    /// <summary>
    /// Gets the <see cref="ArgsCode.Instance"/> constant for serializing test data as objects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this when test methods receive <see cref="ITestData"/> objects as parameters.
    /// </para>
    /// <para>
    /// <strong>Example:</strong>
    /// <code>
    /// [Theory, MemberData(nameof(Args))]
    /// public void Test(TestData&lt;int&gt; testData) { ... }
    /// </code>
    /// </para>
    /// </remarks>
    protected static ArgsCode AsInstance => ArgsCode.Instance;

    /// <summary>
    /// Gets the <see cref="ArgsCode.Properties"/> constant for flattening test data to individual properties.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this when test methods receive individual property values as parameters.
    /// </para>
    /// <para>
    /// <strong>Example:</strong>
    /// <code>
    /// [Theory, MemberData(nameof(Args))]
    /// public void Test(int arg1, int arg2) { ... }
    /// </code>
    /// </para>
    /// </remarks>
    protected static ArgsCode AsProperties => ArgsCode.Properties;

    /// <summary>
    /// Gets the <see cref="PropsCode.All"/> constant that includes <c>TestCaseName</c> in serialized arguments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Primarily used by MSTest for <c>DynamicDataDisplayName</c> feature.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> Not typically used in user code. Framework adapters use this internally.
    /// </para>
    /// <para>
    /// <strong>Technical Detail:</strong> When used, <c>TestCaseName</c> becomes the first argument in the serialized array,
    /// allowing MSTest's display name resolver to extract it via <c>data[0]</c>.
    /// </para>
    /// </remarks>
    protected static PropsCode WithTestCaseName => PropsCode.All;

    /// <summary>
    /// Converts test data using the default <see cref="ArgsCode.Instance"/> serialization strategy.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must be non-null and implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <typeparam name="T">
    /// The framework-specific return type (e.g., <c>TestDataProvider&lt;T&gt;</c> for xUnit,
    /// <c>IEnumerable&lt;object?[]&gt;</c> for MSTest).
    /// </typeparam>
    /// <param name="convert">
    /// The conversion function provided by the framework adapter.
    /// This function receives:
    /// <list type="bullet">
    /// <item><description><paramref name="testDataCollection"/> - The test data to convert</description></item>
    /// <item><description><see cref="AsInstance"/> - The default <see cref="ArgsCode"/></description></item>
    /// <item><description><paramref name="testMethodName"/> - Optional test method name</description></item>
    /// </list>
    /// </param>
    /// <param name="testDataCollection">The collection of test data to convert.</param>
    /// <param name="testMethodName">
    /// Optional. The test method name for display purposes (used by some frameworks).
    /// </param>
    /// <returns>
    /// The converted test data in framework-specific format.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Purpose:</strong> Centralizes the default <see cref="ArgsCode.Instance"/> behavior
    /// across all framework adapters. Framework adapters call this method to avoid duplicating
    /// the default value.
    /// </para>
    /// <para>
    /// <strong>For Framework Adapter Implementers Only:</strong>
    /// User code should call the derived <c>Convert()</c> method in framework-specific adapters
    /// (e.g., <c>Portamical.xUnit.TestBases.TestBase.Convert()</c>).
    /// </para>
    /// <para>
    /// <strong>Example (MSTest Adapter):</strong>
    /// <code>
    /// public class MSTestTestBase : TestBase
    ///     {
    ///         protected static IEnumerable&lt;object?[]&gt; Convert&lt;TTestData&gt;(
    ///             IEnumerable&lt;TTestData&gt; testDataCollection)
    ///     where TTestData : notnull, ITestData  // ← Add constraint
    ///     =&gt; ConvertAsInstance(
    ///         (data, argsCode, _) = &gt; data.ToArgsWithTestCaseName(argsCode, PropsCode.All),
    ///         testDataCollection,
    ///         testMethodName: null);
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="convert"/> is <c>null</c>.
    /// </exception>
    protected static T ConvertAsInstance<TTestData, T>(
        Func<IEnumerable<TTestData>, ArgsCode, string?, T> convert,
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => NotNull(convert, nameof(convert))(
        testDataCollection,
        AsInstance,
        testMethodName);

    /// <summary>
    /// Converts test data using the default <see cref="ArgsCode.Instance"/> serialization strategy.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must be non-null and implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <typeparam name="T">
    /// The framework-specific return type (e.g., <c>TestDataProvider&lt;T&gt;</c> for xUnit).
    /// </typeparam>
    /// <param name="convert">
    /// The conversion function provided by the framework adapter.
    /// This function receives:
    /// <list type="bullet">
    /// <item><description><paramref name="testDataCollection"/> - The test data to convert</description></item>
    /// <item><description><see cref="AsInstance"/> - The default <see cref="ArgsCode"/></description></item>
    /// </list>
    /// </param>
    /// <param name="testDataCollection">The collection of test data to convert.</param>
    /// <returns>
    /// The converted test data in framework-specific format.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Purpose:</strong> Overload for frameworks that do not require <c>testMethodName</c> parameter.
    /// </para>
    /// <para>
    /// See <see cref="ConvertAsInstance{TTestData, T}(Func{IEnumerable{TTestData}, ArgsCode, string?, T}, IEnumerable{TTestData}, string?)"/>
    /// for detailed documentation.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="convert"/> is <c>null</c>.
    /// </exception>
    protected static T ConvertAsInstance<TTestData, T>(
        Func<IEnumerable<TTestData>, ArgsCode, T> convert,
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => NotNull(convert, nameof(convert))(
        testDataCollection,
        AsInstance);
}