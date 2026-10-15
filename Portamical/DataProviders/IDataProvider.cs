// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders;

public interface IDataProvider
{
    ICollection<INamedCase> GetNamedCases();
}

/// <summary>
/// Defines a provider that supplies test data rows to test frameworks through enumeration and lookup operations.
/// </summary>
/// <typeparam name="TRow">
/// The row type for the test framework (e.g., <c>object[]</c> for xUnit v2, <c>TheoryDataRow</c> for xUnit v3).
/// Marked as covariant (<c>out</c>) to support variance in row types.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface represents a read-only view of a test data collection, providing enumeration via
/// <see cref="IEnumerable{T}"/> and direct lookup by test case name. It forms the foundation for
/// test data providers that integrate with xUnit, NUnit, MSTest, and similar frameworks.
/// </para>
/// <para>
/// <strong>Type Variance:</strong> The covariant type parameter allows assignments like
/// <c>IDataProvider&lt;object&gt; = new Provider&lt;string&gt;()</c>, supporting flexible provider usage.
/// </para>
/// <para>
/// <strong>Framework Integration:</strong> Test frameworks typically enumerate providers using
/// <see cref="IEnumerable{T}"/>, while individual test lookup may use <see cref="GetRow"/> or <see cref="GetRows"/>.
/// </para>
/// </remarks>
public interface IDataProvider<out TRow>
: IEnumerable<TRow>, IDataProvider
{
    /// <summary>
    /// Retrieves the row associated with the specified test case name.
    /// </summary>
    /// <param name="testCaseName">
    /// The test case name to look up. Implementations should handle <see langword="null"/> gracefully
    /// (e.g., by treating it as <see cref="string.Empty"/>).
    /// </param>
    /// <returns>
    /// The row of type <typeparamref name="TRow"/> if testCaseNameFound; otherwise, <see langword="null"/> or
    /// <see langword="default"/> for the row type.
    /// </returns>
    /// <remarks>
    /// This method enables direct lookup of test data by name, useful for selective test execution
    /// or debugging specific test cases. The lookup mechanism (case sensitivity, comparison method)
    /// is determined by the implementation.
    /// </remarks>
    TRow? GetRow(string testCaseName);

    /// <summary>
    /// Retrieves the row associated with the specified test case.
    /// </summary>
    /// <param name="namedCase">
    /// The <see cref="INamedCase"/> identifying the test case to look up. Implementations should handle
    /// <see langword="null"/> gracefully by returning <see langword="default"/>.
    /// </param>
    /// <returns>
    /// The row of type <typeparamref name="TRow"/> if a matching test case is found; otherwise,
    /// <see langword="null"/> or <see langword="default"/> for the row type.
    /// </returns>
    /// <remarks>
    /// This overload enables lookup by identity object rather than raw test case name, useful when the
    /// caller already holds an <see cref="INamedCase"/> instance (e.g., from enumeration). The matching
    /// mechanism (equality comparer, case sensitivity) is determined by the implementation.
    /// </remarks>
    TRow? GetRow(INamedCase namedCase);

    /// <summary>
    /// Gets an array containing all rows in the provider's collection.
    /// </summary>
    /// <returns>
    /// An array of <typeparamref name="TRow"/> containing all test data rows.
    /// Returns an empty array if no rows are available.
    /// </returns>
    /// <remarks>
    /// The returned array is typically a snapshot of the current collection. The order of rows
    /// may or may not match insertion order, depending on the implementation.
    /// </remarks>
    TRow[] GetRows();
}

/// <summary>
/// Defines a provider that accepts test data items, converts them to rows, and supplies them to test frameworks.
/// Combines data ingestion (<see cref="ITestDataRegistry{TTestData}"/>) with row enumeration and conversion.
/// </summary>
/// <typeparam name="TTestData">
/// The test data type that implements <see cref="ITestData"/>. Must be a non-nullable reference type.
/// Marked as contravariant (<c>in</c>) to support variance in test data types.
/// </typeparam>
/// <typeparam name="TRow">
/// The row type for the test framework (e.g., <c>object[]</c> for xUnit v2, <c>TheoryDataRow</c> for xUnit v3).
/// Marked as covariant (<c>out</c>) to support variance in row types.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface extends <see cref="IDataProvider{TRow}"/> to include test data ingestion and conversion
/// capabilities. It represents a full-featured test data provider that can accept typed test data,
/// convert it to framework-specific row formats, and expose the results through enumeration and lookup.
/// </para>
/// <para>
/// <strong>Type Variance:</strong> The combination of contravariant input (<c>in TTestData</c>) and
/// covariant output (<c>out TConvertedRows</c>) enables flexible provider hierarchies and adapter patterns.
/// </para>
/// <para>
/// <strong>Typical Usage:</strong> Implementations populate the provider using <see cref="ITestDataRegistry{TTestData}.AddRow"/>
/// or <see cref="ITestDataRegistry{TTestData}.AddRange"/>, which internally call <see cref="ConvertRow"/> to transform
/// each test data item into the target row format.
/// </para>
/// </remarks>
public interface IDataProvider<in TTestData, out TRow>
: ITestDataConverter<TTestData, TRow>,
IDataProvider<TRow>,
ITestDataRegistry<TTestData>
where TTestData : notnull, ITestData
{
}