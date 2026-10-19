// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.CustomRow;
using Portamical.Converters.RowArrays.ObjectArray;
using Portamical.Core.Safety;
using Portamical.MSTest.DataProviders;

namespace Portamical.MSTest.Converters;

/// <summary>
/// Provides extension methods for converting test data collections into MSTest-compatible formats.
/// </summary>
/// <remarks>
/// <para>
/// These methods are specifically designed for MSTest's <see cref="DynamicDataAttribute"/>,
/// which requires <c>IEnumerable&lt;object[]&gt;</c> where each array contains test method
/// parameters including the test case name for identification.
/// </para>
/// <para>
/// <strong>Why MSTest Needs Test Case Names as Parameters:</strong>
/// </para>
/// <para>
/// Unlike NUnit's <c>TestCaseData.SetName()</c> or xUnit v3's <c>ITheoryDataRow.TestDisplayName</c>,
/// MSTest's <see cref="DynamicDataAttribute"/> does not have built-in test case naming metadata.
/// The standard pattern is to include the test case name as the <strong>first parameter</strong>
/// in your test method signature for identification in Test Explorer.
/// </para>
/// <para>
/// <strong>Framework Comparison:</strong>
/// <list type="table">
///   <listheader>
///     <term>Framework</term>
///     <description>Test Case Naming Mechanism</description>
///   </listheader>
///   <item>
///     <term>NUnit</term>
///     <description><c>TestCaseData.SetName(string)</c> - Metadata property</description>
///   </item>
///   <item>
///     <term>xUnit v3</term>
///     <description><c>ITheoryDataRow.TestDisplayName</c> - Metadata property</description>
///   </item>
///   <item>
///     <term>xUnit v2</term>
///     <description>Manual <c>[InlineData(DisplayName = "...")]</c></description>
///   </item>
///   <item>
///     <term>MSTest</term>
///     <description><strong>Test case name as first parameter</strong> (this approach)</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
public static class CollectionConverter
{
    public static object?[][] ToDistinctRowArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        string? testMethodName,
        params string[] testCategories)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(
        convertRow: (testData, argsCode, tmn) => new object[]
        {
            argsCode switch
            {
                ArgsCode.Instance => TestDataRow.From(
                    testData,
                    convertRow: td => td,
                    testMethodName: tmn,
                    testCategories),
                ArgsCode.Properties => TestDataRow.From(
                    testData,
                    convertRow: td => td.ToArgs(ArgsCode.Properties),
                    testMethodName: tmn,
                    testCategories),
                _ => throw argsCode.GetInvalidEnumArgumentException(nameof(argsCode)),
            }
        },
        argsCode,
        testMethodName);

    public static DataProviders.Models.ObjectArray.TestDataProvider<TTestData> ToDataProvider<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        params string[] testCategories)
    where TTestData : notnull, ITestData
    => new DataProviders.Models.ObjectArray.TestDataProvider<TTestData>(
        testDataCollection,
        argsCode,
        testCategories);

    public static DataProviders.Models.TestData.TestDataProvider<TTestData> ToDataProvider<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        params string[] testCategories)
    where TTestData : notnull, ITestData
    => new DataProviders.Models.TestData.TestDataProvider<TTestData>(
        testDataCollection,
        testCategories);
}
