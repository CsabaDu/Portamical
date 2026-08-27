// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders;

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
: IEnumerable<TRow>
{
    /// <summary>
    /// Retrieves the row associated with the specified test case name.
    /// </summary>
    /// <param name="testCaseName">
    /// The test case name to look up. Implementations should handle <see langword="null"/> gracefully
    /// (e.g., by treating it as <see cref="string.Empty"/>).
    /// </param>
    /// <returns>
    /// The row of type <typeparamref name="TRow"/> if found; otherwise, <see langword="null"/> or
    /// <see langword="default"/> for the row type.
    /// </returns>
    /// <remarks>
    /// This method enables direct lookup of test data by name, useful for selective test execution
    /// or debugging specific test cases. The lookup mechanism (case sensitivity, comparison method)
    /// is determined by the implementation.
    /// </remarks>
    TRow? GetRow(string testCaseName);

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

    /// <summary>
    /// Gets an array containing all test case names in the provider's collection.
    /// </summary>
    /// <returns>
    /// An array of strings representing the test case names (typically from <see cref="INamedCase.TestCaseName"/>).
    /// Returns an empty array if no rows are available.
    /// </returns>
    /// <remarks>
    /// This method is useful for discovering available test cases, generating reports, or
    /// implementing test filtering logic. The order of names may or may not match insertion order.
    /// </remarks>
    string[] GetTestCaseNames();
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
/// covariant output (<c>out TRow</c>) enables flexible provider hierarchies and adapter patterns.
/// </para>
/// <para>
/// <strong>Typical Usage:</strong> Implementations populate the provider using <see cref="ITestDataRegistry{TTestData}.AddRow"/>
/// or <see cref="ITestDataRegistry{TTestData}.AddRange"/>, which internally call <see cref="ConvertRow"/> to transform
/// each test data item into the target row format.
/// </para>
/// </remarks>
public interface IDataProvider<in TTestData, out TRow>
: IDataProvider<TRow>, ITestDataRegistry<TTestData>
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Converts a single test data item into a row representation suitable for the target test framework.
    /// </summary>
    /// <param name="testData">
    /// The test data to convert. Cannot be <see langword="null"/> due to the <c>notnull</c>
    /// constraint on <typeparamref name="TTestData"/>. This is the source data that will be
    /// transformed into the target row format.
    /// </param>
    /// <returns>
    /// A row representation of type <typeparamref name="TRow"/>. Common return types include <c>object[]</c>
    /// for xUnit/NUnit/MSTest, or framework-specific types like xUnit v3's <c>TheoryDataRow</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the actual conversion of test data into a row format. The specific conversion
    /// logic is determined by the implementing class.
    /// </para>
    /// <para>
    /// <strong>Integration Pattern:</strong> This method is typically called internally during enumeration:
    /// </para>
    /// <code>
    /// public IEnumerator&lt;TRow&gt; GetEnumerator()
    /// {
    ///     foreach (var testData in _rows)
    ///     {
    ///         yield return ConvertRow(testData);
    ///     }
    /// }
    /// </code>
    /// <para>
    /// <strong>Stateless Design:</strong> This method should be stateless and depend only on its
    /// parameter. Multiple calls with the same input should produce equivalent results.
    /// </para>
    /// <para>
    /// <strong>Framework Requirements:</strong> The returned row type must match the expectations
    /// of the target test framework. For example, xUnit v2's <c>[MemberData]</c> expects <c>object[]</c>,
    /// while xUnit v3 can use strongly-typed <c>TheoryDataRow</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example implementation for object[] conversion
    /// public object[] ConvertRow(TestDataReturns&lt;int&gt; testData)
    /// {
    ///     return testData.ToArgs();
    /// }
    /// 
    /// var row = ConvertRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5));
    /// // Result: [2, 3, 5]
    /// </code>
    /// </example>
    TRow ConvertRow(TTestData testData);
}
