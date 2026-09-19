// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters.RowArrays.ObjectArray;

/// <summary>
/// Provides extension methods for converting test data collections into jagged <c>object?[][]</c> arrays
/// of test method arguments, with optional deduplication based on test case identity.
/// </summary>
/// <remarks>
/// <para>
/// Each returned row is an <c>object?[]</c> produced via <see cref="ITestData.ToArgs(ArgsCode)"/> or
/// <see cref="ITestData.ToArgs(ArgsCode, PropsCode)"/>, making the output directly compatible with
/// xUnit v2 <c>[MemberData]</c>, NUnit <c>[TestCaseSource]</c>, and MSTest <c>[DynamicData]</c>.
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong> The <c>ToDistinctRowArray</c> overloads remove duplicates
/// based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>. Test data with
/// identical <c>TestCaseName</c> values are treated as duplicates, with the first occurrence retained.
/// </para>
/// </remarks>
public static class CollectionConverter
{
    #region ToRowArray

    /// <summary>
    /// Converts a collection of test data into a jagged array of argument arrays using the specified argument code.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining how arguments are extracted from each test data item.
    /// </param>
    /// <returns>
    /// A jagged array where each element is an <c>object?[]</c> containing arguments for one test data item.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// This overload uses <see cref="ITestData.ToArgs(ArgsCode)"/> for conversion. Compatible with
    /// xUnit v2 [MemberData], NUnit [TestCaseSource], and MSTest [DynamicData].
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object?[][] ToRowArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToRowArray(
        convertRow: testData => testData.ToArgs(argsCode));

    /// <summary>
    /// Converts a collection of test data into a jagged array of argument arrays using the specified
    /// argument and properties codes.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining the primary conversion strategy.
    /// </param>
    /// <param name="propsCode">
    /// The properties code determining which properties to include when flattening.
    /// </param>
    /// <returns>
    /// A jagged array where each element is an <c>object?[]</c> containing arguments extracted according
    /// to the specified codes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// This overload uses <see cref="ITestData.ToArgs(ArgsCode, PropsCode)"/> for fine-grained control
    /// over argument extraction. The combination of codes determines which data is included in each row.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object?[][] ToRowArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToRowArray(
        convertRow: testData => testData.ToArgs(argsCode, propsCode));

    #endregion

    #region ToDistinctRowArray

    /// <summary>
    /// Returns a jagged array of distinct argument arrays generated from the specified test data collection
    /// using the provided argument code.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining how arguments are extracted from each test data item.
    /// </param>
    /// <returns>
    /// A jagged array containing unique <c>object?[]</c> argument arrays produced from distinct test data items.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Uses <see cref="ITestData.ToArgs(ArgsCode)"/> for conversion. Duplicates are removed based on
    /// test case name identity using <see cref="NamedCase.Comparer"/>; the order of elements from the
    /// original collection is preserved (first occurrence wins).
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object?[][] ToDistinctRowArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(
        convertRow: testData => testData.ToArgs(argsCode));

    /// <summary>
    /// Creates a jagged array of distinct argument arrays from the specified test data collection, using the
    /// provided argument and property codes to extract values.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to convert. Cannot be null or empty.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining the primary conversion strategy.
    /// </param>
    /// <param name="propsCode">
    /// The properties code determining which properties to include when flattening.
    /// </param>
    /// <returns>
    /// A jagged array containing unique <c>object?[]</c> argument arrays extracted from distinct test data items.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Uses <see cref="ITestData.ToArgs(ArgsCode, PropsCode)"/> for fine-grained control over argument
    /// extraction. Duplicates are removed based on test case name identity using <see cref="NamedCase.Comparer"/>;
    /// the order of elements from the original collection is preserved (first occurrence wins).
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object?[][] ToDistinctRowArray<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctRowArray(
        convertRow: testData => testData.ToArgs(argsCode, propsCode));

    #endregion
}