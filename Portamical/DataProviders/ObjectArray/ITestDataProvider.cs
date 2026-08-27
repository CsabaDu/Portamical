// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.ObjectArray;

/// <summary>
/// Defines a test data provider that converts test data into <c>object[]</c> rows for test frameworks,
/// with configurable argument and property conversion strategies.
/// </summary>
/// <typeparam name="TTestData">
/// The test data type that implements <see cref="ITestData"/>. Must be a non-nullable reference type.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface specializes <see cref="IDataProvider{TTestData, TRow}"/> for <c>object?[]</c> rows,
/// which is the standard format for xUnit v2, NUnit, and MSTest test frameworks. It adds configuration
/// properties to control how test data is converted into argument arrays.
/// </para>
/// <para>
/// <strong>Conversion Control:</strong> The <see cref="ArgsCode"/> and <see cref="PropsCode"/> properties
/// work together to determine the final row format:
/// </para>
/// <list type="bullet">
///   <item><see cref="ArgsCode"/> - Controls whether to pass the entire test data object or flatten it</item>
///   <item><see cref="PropsCode"/> - Controls which properties to include when flattening</item>
/// </list>
/// <para>
/// <strong>Framework Compatibility:</strong> The <c>object?[]</c> row format is directly compatible with:
/// xUnit v2 [MemberData], NUnit [TestCaseSource], and MSTest [DynamicData].
/// </para>
/// </remarks>
public interface ITestDataProvider<TTestData>
: IDataProvider<TTestData, object?[]>
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Gets the argument code that determines the primary conversion strategy for test data rows.
    /// </summary>
    /// <value>
    /// An <see cref="ArgsCode"/> value specifying whether to pass the entire test data object
    /// (<see cref="ArgsCode.Instance"/>) or flatten its properties (<see cref="ArgsCode.Properties"/>).
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is typically set during construction via the <c>init</c> accessor and determines
    /// the high-level conversion strategy. When combined with <see cref="PropsCode"/>, it controls
    /// the exact structure of the resulting <c>object?[]</c> row.
    /// </para>
    /// <para>
    /// <strong>Strategies:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><see cref="ArgsCode.Instance"/> - Row contains the test data object itself: <c>[testData]</c></item>
    ///   <item><see cref="ArgsCode.Properties"/> - Row contains flattened values based on <see cref="PropsCode"/></item>
    /// </list>
    /// </remarks>
    ArgsCode ArgsCode { get; init; }

    /// <summary>
    /// Gets the properties code that determines which properties to include when flattening test data.
    /// </summary>
    /// <value>
    /// A <see cref="PropsCode"/> value specifying which property groups to include in the flattened row.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property works in conjunction with <see cref="ArgsCode"/> to control row conversion.
    /// It is most relevant when <see cref="ArgsCode"/> is set to <see cref="ArgsCode.Properties"/>,
    /// determining which test data properties are extracted into the <c>object?[]</c> row.
    /// </para>
    /// <para>
    /// <strong>Common Values:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><see cref="PropsCode.All"/> - Include all available properties</item>
    ///   <item><see cref="PropsCode.Args"/> - Include only argument properties</item>
    ///   <item><see cref="PropsCode.Expected"/> - Include only expected result properties</item>
    /// </list>
    /// <para>
    /// This property is typically set during construction via the <c>init</c> accessor.
    /// </para>
    /// </remarks>
    PropsCode PropsCode { get; init; }
}
