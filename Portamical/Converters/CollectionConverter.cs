// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Identity;
using Portamical.Identity.Model;
using Portamical.Strategy;
using Portamical.TestDataTypes;

namespace Portamical.Converters;

/// <summary>
/// Provides extension methods for transforming collections of <see cref="ITestData"/> 
/// into framework-ready representations. Acts as a dual-strategy converter supporting 
/// both parameter arrays and custom row formats.
/// </summary>
public static class CollectionConverter
{
    /// <summary>
    /// Converts a collection of <see cref="ITestData"/> into an enumeration of 
    /// parameter arrays (<c>object?[]</c>) suitable for direct consumption by 
    /// parameterized test runners (e.g., NUnit, xUnit, MSTest).
    /// </summary>
    /// <typeparam name="TTestData">
    /// The concrete test data type, constrained to <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data objects to be transformed.
    /// </param>
    /// <param name="argsCode">
    /// Specifies the argument transformation strategy (e.g., instance vs. properties).
    /// </param>
    /// <param name="propsCode">
    /// Specifies the property transformation semantics (e.g., TrimName, returns, throws).
    /// </param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of parameter arrays, each representing a 
    /// fully shaped test case for direct invocation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="testDataCollection"/> is <c>null</c>.
    /// </exception>
    public static IReadOnlyCollection<object?[]> Convert<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode,
        PropsCode propsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ConvertDistinct(
        testData => testData.ToArgs(argsCode, propsCode));

    /// <summary>
    /// Converts a collection of <see cref="ITestData"/> into an enumeration of
    /// parameter arrays using the default <see cref="PropsCode.TrimName"/> semantics.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The concrete test data type, constrained to <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data objects to convert.
    /// </param>
    /// <param name="argsCode">
    /// Specifies the argument transformation strategy (e.g., instance vs. properties).
    /// </param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of parameter arrays representing the
    /// transformed test cases.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="testDataCollection"/> is <c>null</c>.
    /// </exception>
    public static IReadOnlyCollection<object?[]> ConvertToArgs<TTestData>(
        this IEnumerable<TTestData> testDataCollection,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    => testDataCollection.ConvertDistinct(
        testData => testData.ToArgs(argsCode));

    /// <summary>
    /// Converts a collection of <see cref="ITestData"/> into a sequence of custom 
    /// row objects using a caller-provided transformation function. This enables 
    /// integration with external systems such as reporting tools, visualizers, 
    /// or databases.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The concrete test data type, constrained to <see cref="ITestData"/>.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The target row type produced by the transformation function.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data objects to be transformed.
    /// </param>
    /// <param name="testDataConverter">
    /// A delegate that defines how each test data object is converted into a 
    /// <typeparamref name="TRow"/> instance.
    /// </param>
    /// <param name="argsCode">
    /// Specifies the argument transformation strategy (e.g., instance vs. properties).
    /// </param>
    /// <param name="testMethodName">
    /// An optional test method name used for constructing descriptive display names.
    /// </param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of custom row objects, each representing 
    /// a transformed test case suitable for external consumption.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="testDataCollection"/> or <paramref name="testDataConverter"/> is <c>null</c>.
    /// </exception>
    public static IReadOnlyCollection<TRow> Convert<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> testDataConverter,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    => testDataCollection.ConvertDistinct(
        testData => testDataConverter(
            testData,
            argsCode.Defined(nameof(argsCode)),
            testMethodName));

    public static TDataProvider Convert<TDataProvider, TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, ArgsCode, string?, TRow> convertRow,
        Func<TRow, TDataProvider> initDataProvider,
        Action<TDataProvider, TRow> addRow,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    {
        var rowCollection = testDataCollection.Convert(
            convertRow,
            argsCode,
            testMethodName);
        var rows = rowCollection.ToArray();

        if (rows.Length == 0)
        {
            throw new InvalidOperationException("No test rows provided.");
        }

        var dataProvider = initDataProvider(rows[0]);

        for (int i = 1; i < rows.Length; i++)
        {
            addRow(dataProvider, rows[i]);
        }

        return dataProvider;
    }

    /// <summary>
    /// Core transformation routine used by all public <c>CollectionConverter</c>
    /// methods to convert a sequence of <see cref="ITestData"/> items into
    /// custom row objects.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The concrete test data type, constrained to <see cref="ITestData"/>.
    /// </typeparam>
    /// <typeparam name="TRow">
    /// The target row type produced by the <paramref name="convertRow"/> delegate.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data items to be transformed.
    /// </param>
    /// <param name="convertRow">
    /// A delegate that converts a single <typeparamref name="TTestData"/> instance
    /// into a <typeparamref name="TRow"/> result.
    /// </param>
    /// <returns>
    /// A lazily streamed <see cref="IEnumerable{T}"/> of semantically distinct
    /// row objects, each representing a unique test case.
    /// </returns>
    /// <remarks>
    /// This private method provides the shared transformation pipeline for all
    /// public <c>CollectionConverter</c> overloads. It performs semantic
    /// deduplication using <see cref="INamedCase"/> identity and equality
    /// semantics, ensuring that each logical test case is emitted only once.
    /// Results are streamed using <c>yield return</c> for efficient, on‑demand
    /// processing.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="testDataCollection"/> or
    /// <paramref name="convertRow"/> is <c>null</c>.
    /// </exception>
    private static TRow[] ConvertDistinct<TTestData, TRow>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TRow> convertRow)
    where TTestData : notnull, ITestData
    {
        var source = Validator.NotNullOrEmpty(
            testDataCollection,
            nameof(testDataCollection));

        ArgumentNullException.ThrowIfNull(
            convertRow,
            nameof(convertRow));

        // Deduplicate based on 'INamedCase' identity/equality semantics
        HashSet<INamedCase> namedCases = new(NamedCase.Comparer);
        List<TRow> rows = [];

        foreach (var testData in source)
        {
            if (namedCases.Add(testData))
            {
                rows.Add(convertRow(testData));
            }
        }

        return [.. rows];
    }
}