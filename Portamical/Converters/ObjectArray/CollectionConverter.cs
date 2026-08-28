// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters.ObjectArray;

public static class CollectionConverter
{
    #region ToArrayRow

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
    public static object?[][] ToArrayRow<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArrayRow(
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
    public static object?[][] ToArrayRow<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToArrayRow(
        convertRow: testData => testData.ToArgs(argsCode, propsCode));

    #endregion

    #region ToDistinctArrayRow

    /// <summary>
    /// Returns a jagged array of distinct argument arrays generated from the specified test data collection
    /// using the provided argument code.
    /// </summary>
    /// <remarks>Each element in the returned array corresponds to the arguments produced by calling
    /// ToArgs on each test data item with the specified argument code. Duplicates are removed based on
    /// test case name identity using <see cref="NamedCase.Comparer"/>.</remarks>
    /// <typeparam name="TTestData">The type of the test data elements. Must implement the ITestData interface and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data items from which to generate argument arrays. Cannot be null.</param>
    /// <param name="argsCode">The argument code that determines how arguments are extracted from each test data item.</param>
    /// <returns>A jagged array containing unique argument arrays produced from distinct test data items.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object?[][] ToDistinctArrayRow<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArrayRow(
        convertRow: testData => testData.ToArgs(argsCode));

    /// <summary>
    /// Creates a jagged array of distinct argument arrays from the specified test data collection, using the
    /// provided argument and property codes to extract values.
    /// </summary>
    /// <remarks>The returned array contains only distinct argument arrays, where uniqueness is determined by
    /// test case name identity using <see cref="NamedCase.Comparer"/>. The order of elements from the
    /// original collection is preserved (first occurrence wins).</remarks>
    /// <typeparam name="TTestData">The type of the test data elements. Must implement the ITestData interface and cannot be null.</typeparam>
    /// <param name="testDataCollection">The collection of test data items from which to generate argument arrays. Cannot be null.</param>
    /// <param name="argsCode">The code specifying which arguments to extract from each test data item.</param>
    /// <param name="propsCode">The code specifying which properties to extract from each test data item.</param>
    /// <returns>A jagged array containing unique argument arrays extracted from distinct test data items.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object?[][] ToDistinctArrayRow<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArrayRow(
        convertRow: testData => testData.ToArgs(argsCode, propsCode));

    #endregion
}
