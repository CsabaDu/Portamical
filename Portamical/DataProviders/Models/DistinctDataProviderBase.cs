// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.Models;

/// <summary>
/// Provides an abstract base implementation of <see cref="IDataProvider{TTestData, TRow}"/> that ensures
/// each test case name maps to exactly one row, using ordinal string comparison for deduplication.
/// </summary>
/// <typeparam name="TTestData">
/// The test data type that implements <see cref="ITestData"/>. Must be a non-nullable reference type.
/// </typeparam>
/// <typeparam name="TRow">
/// The target row type for the test framework (e.g., <c>object[]</c>, <c>TheoryDataRow</c>).
/// </typeparam>
/// <remarks>
/// <para>
/// This class is the foundation for test data providers in the Portamical library. It maintains a dictionary
/// of distinct test cases keyed by <see cref="INamedCase.TestCaseName"/> using <see cref="StringComparer.Ordinal"/>.
/// </para>
/// <para>
/// <strong>Constructor Accessibility:</strong> All constructors are marked <c>private protected</c> to restrict
/// instantiation to derived classes within the same assembly, supporting controlled inheritance patterns.
/// </para>
/// <para>
/// <strong>Deduplication:</strong> Attempting to add a test case with a duplicate name throws
/// <see cref="ArgumentException"/> via the underlying dictionary.
/// </para>
/// </remarks>
public abstract class DistinctDataProviderBase<TTestData, TRow>
: IDataProvider<TTestData, TRow>
where TTestData : notnull, ITestData
{
    private readonly Dictionary<string, TRow> _distinctRows = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance with an empty collection of test data rows.
    /// </summary>
    /// <remarks>
    /// This constructor is <c>private protected</c> to allow derived types within the same assembly
    /// to instantiate the provider without initial data, supporting builder-pattern usage.
    /// </remarks>
    private protected DistinctDataProviderBase()
    {
    }

    /// <summary>
    /// Initializes a new instance and adds a single test data row.
    /// </summary>
    /// <param name="testData">
    /// The initial test data to add. Will be converted to a row via <see cref="ConvertRow"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the test case name already exists in the collection.
    /// </exception>
    /// <remarks>
    /// This constructor is <c>private protected</c> to restrict instantiation to derived types
    /// within the same assembly.
    /// </remarks>
    private protected DistinctDataProviderBase(TTestData testData)
    {
        AddRow(testData);
    }

    /// <summary>
    /// Initializes a new instance and adds multiple test data rows.
    /// </summary>
    /// <param name="testDataCollection">
    /// The collection of test data to add. Each item will be converted to a row via <see cref="ConvertRow"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if any test case name in the collection is duplicated.
    /// </exception>
    /// <remarks>
    /// This constructor is <c>private protected</c> to restrict instantiation to derived types
    /// within the same assembly.
    /// </remarks>
    private protected DistinctDataProviderBase(IEnumerable<TTestData> testDataCollection)
    {
        AddRange(testDataCollection);
    }

    /// <summary>
    /// Adds a new row of test data to the provider's collection after converting it to the target row format.
    /// </summary>
    /// <param name="testData">
    /// The test data to add. Will be converted via <see cref="ConvertRow"/> before being stored.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if a test case with the same <see cref="INamedCase.TestCaseName"/> already exists in the collection.
    /// </exception>
    /// <remarks>
    /// This method uses <see cref="INamedCase.TestCaseName"/> as the dictionary key with <see cref="StringComparer.Ordinal"/>.
    /// The converted row is stored immediately, ensuring the collection remains consistent.
    /// </remarks>
    public void AddRow(TTestData testData)
    => _distinctRows.Add(
        key: testData.TestCaseName,
        value: ConvertRow(testData));

    /// <summary>
    /// Adds multiple test data rows to the provider's collection.
    /// </summary>
    /// <param name="testDataCollection">
    /// The collection of test data to add. Each item will be converted and added via <see cref="AddRow"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="testDataCollection"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the collection is empty, or if any test case name already exists in the collection.
    /// </exception>
    /// <remarks>
    /// The collection is validated and snapshotted before enumeration to ensure stability during iteration.
    /// If any duplicate test case name is encountered during enumeration, an exception is thrown and
    /// previously added items from this batch remain in the collection.
    /// </remarks>
    public void AddRange(IEnumerable<TTestData> testDataCollection)
    {
        var snapshot = NotNullOrEmpty(
            testDataCollection, nameof(testDataCollection));

        foreach (var testData in snapshot)
        {
            AddRow(testData);
        }
    }

    /// <summary>
    /// Retrieves the row associated with the specified test case name.
    /// </summary>
    /// <param name="testCaseName">
    /// The test case name to look up. <see langword="null"/> is treated as <see cref="string.Empty"/>.
    /// </param>
    /// <returns>
    /// The row of type <typeparamref name="TRow"/> if found; otherwise, <see langword="default"/> (<see langword="null"/> for reference types).
    /// </returns>
    /// <remarks>
    /// Lookup uses <see cref="StringComparer.Ordinal"/> comparison. This method does not throw if the test case name is not found.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TRow? GetRow(string testCaseName)
    => _distinctRows.TryGetValue(testCaseName ?? string.Empty, out var row) ?
        row
        : default;

    /// <summary>
    /// Gets an array containing all rows in the provider's collection.
    /// </summary>
    /// <returns>
    /// An array of <typeparamref name="TRow"/> containing all converted test data rows.
    /// Returns an empty array if no rows have been added.
    /// </returns>
    /// <remarks>
    /// The returned array is a snapshot of the current collection. Subsequent modifications to the provider
    /// will not affect the returned array.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TRow[] GetRows()
    => [.. _distinctRows.Values];

    /// <summary>
    /// Gets an array containing all test case names in the provider's collection.
    /// </summary>
    /// <returns>
    /// An array of strings containing all test case names (keys) from the collection.
    /// Returns an empty array if no rows have been added.
    /// </returns>
    /// <remarks>
    /// The returned array is a snapshot of the current collection's keys. The order is determined by
    /// the dictionary's internal structure and may not match insertion order.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string[] GetTestCaseNames()
    => [.. _distinctRows.Keys];

    /// <summary>
    /// Returns an enumerator that iterates through the collection of rows.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{TRow}"/> for the row collection.
    /// </returns>
    /// <remarks>
    /// This method supports LINQ operations and foreach iteration over the provider's rows.
    /// The enumeration order is determined by the dictionary's internal structure.
    /// </remarks>
    public IEnumerator<TRow> GetEnumerator()
    => _distinctRows.Values.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> for the row collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    => GetEnumerator();

    /// <summary>
    /// When overridden in a derived class, converts a test data item into a row representation
    /// suitable for the target test framework.
    /// </summary>
    /// <param name="testData">
    /// The test data to convert.
    /// </param>
    /// <returns>
    /// A row of type <typeparamref name="TRow"/> representing the converted test data.
    /// </returns>
    /// <remarks>
    /// This method is called by <see cref="AddRow"/> during row insertion. Implementations should be
    /// stateless and produce consistent results for the same input.
    /// </remarks>
    public abstract TRow ConvertRow(TTestData testData);
}
