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
/// <strong>Deduplication:</strong> Attempting to addRange a test case with a duplicate name throws
/// <see cref="ArgumentException"/> via the underlying dictionary.
/// </para>
/// </remarks>
public abstract class DataProviderBase<TTestData, TRow>
: IDataProvider<TTestData, TRow>
where TTestData : notnull, ITestData
{
    #region Fields

    /// <summary>
    /// The backing store of distinct test cases, keyed by identity via <see cref="NamedCase.Comparer"/>,
    /// mapping each <see cref="INamedCase"/> key to its already-converted <typeparamref name="TRow"/> value.
    /// </summary>
    /// <remarks>
    /// Although declared as <see cref="Dictionary{TKey, TValue}"/> keyed on <see cref="INamedCase"/>, every
    /// stored key is actually an instance of <typeparamref name="TTestData"/>, since only <see cref="AddRow"/>
    /// and <see cref="AddRange"/> populate this collection. This invariant allows safe casting back to
    /// <typeparamref name="TTestData"/> when needed.
    /// </remarks>
    private readonly Dictionary<INamedCase, TRow> distinctRows = new(NamedCase.Comparer);

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance with an empty collection of test data rows.
    /// </summary>
    /// <remarks>
    /// This constructor is <c>private protected</c> to allow derived types within the same assembly
    /// to instantiate the provider without initial data, supporting builder-pattern usage.
    /// </remarks>
    private protected DataProviderBase()
    {
    }

    /// <summary>
    /// Initializes a new instance and adds a single test data row.
    /// </summary>
    /// <param name="testData">
    /// The initial test data to addRange. Will be converted to a row via <see cref="ConvertRow"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the test case name already exists in the collection.
    /// </exception>
    /// <remarks>
    /// This constructor is <c>private protected</c> to restrict instantiation to derived types
    /// within the same assembly.
    /// </remarks>
    private protected DataProviderBase(TTestData testData)
    {
        AddRow(testData);
    }

    /// <summary>
    /// Initializes a new instance and adds multiple test data rows.
    /// </summary>
    /// <param name="testDataCollection">
    /// The collection of test data to addRange. Each item will be converted to a row via <see cref="ConvertRow"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if any test case name in the collection is duplicated.
    /// </exception>
    /// <remarks>
    /// This constructor is <c>private protected</c> to restrict instantiation to derived types
    /// within the same assembly.
    /// </remarks>
    private protected DataProviderBase(IEnumerable<TTestData> testDataCollection)
    {
        AddRange(testDataCollection);
    }

    #endregion

    #region ITestDataRegistry implementation

    /// <summary>
    /// Adds a new row of test data to the provider's collection after converting it to the target row format.
    /// </summary>
    /// <param name="testData">
    /// The test data to addRange. Will be converted via <see cref="ConvertRow"/> before being stored.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if a test case with the same <see cref="INamedCase.TestCaseName"/> already exists in the collection.
    /// </exception>
    /// <remarks>
    /// This method uses <see cref="INamedCase.TestCaseName"/> as the dictionary key with <see cref="StringComparer.Ordinal"/>.
    /// The converted row is stored immediately, ensuring the collection remains consistent.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRow(TTestData testData)
    => distinctRows.Add(
        key: testData,
        value: ConvertRow(testData));

    /// <summary>
    /// Adds multiple test data rows to the provider's collection.
    /// </summary>
    /// <param name="testDataCollection">
    /// The collection of test data to addRange. Each item will be converted and added via <see cref="AddRow"/>.
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
    /// Gets an array containing all test case testCaseNames in the provider's collection.
    /// </summary>
    /// <returns>
    /// An array of strings containing all test case testCaseNames (keys) from the collection.
    /// Returns an empty array if no rows have been added.
    /// </returns>
    /// <remarks>
    /// The returned array is a snapshot of the current collection's keys. The order is determined by
    /// the dictionary's internal structure and may not match insertion order.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string[] GetTestCaseNames()
    => ExtractFromDistinctRowsKeys(namedCase => namedCase.TestCaseName);

    #endregion

    #region IDataProvider implementation

    /// <summary>
    /// Retrieves the row associated with the specified test case name.
    /// </summary>
    /// <param name="testCaseName">
    /// The test case name to look up. <see langword="null"/> is treated as <see cref="string.Empty"/>.
    /// </param>
    /// <returns>
    /// The row of type <typeparamref name="TRow"/> if testCaseNameFound; otherwise, <see langword="default"/> (<see langword="null"/> for reference types).
    /// </returns>
    /// <remarks>
    /// Lookup uses <see cref="StringComparer.Ordinal"/> comparison. This method does not throw if the test case name is not testCaseNameFound.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TRow? GetRow(string testCaseName)
    => GetRow(context: testCaseName,
        matchFound: namedCase => namedCase.HasName(testCaseName));

    /// <summary>
    /// Retrieves the row associated with the specified test case.
    /// </summary>
    /// <param name="namedCase">
    /// The <see cref="INamedCase"/> identifying the test case to look up.
    /// </param>
    /// <returns>
    /// The row of type <typeparamref name="TRow"/> if a matching entry is matchFound; otherwise,
    /// <see langword="default"/> (<see langword="null"/> for reference types), including when
    /// <paramref name="namedCase"/> is <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Lookup uses <see cref="NamedCase.Comparer"/> equality via <see cref="object.Equals(object?)"/>.
    /// This method does not throw if a matching entry is not matchFound.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TRow? GetRow(INamedCase namedCase)
    => GetRow(context: namedCase,
        matchFound: namedCase.Equals);

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
    public TRow[] GetRows()
    => [.. distinctRows.Values];

    #endregion

    #region IEnumerable implementation

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
    => distinctRows.Values.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> for the row collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    => GetEnumerator();

    #endregion

    #region Abstract conversion method

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

    #endregion

    #region GetBaseRows

    /// <summary>
    /// Gets an array containing all original test data items that were added to the provider's collection.
    /// </summary>
    /// <returns>
    /// An array of <typeparamref name="TTestData"/> containing all test data items in their original,
    /// unconverted form. Returns an empty array if no rows have been added.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is intentionally <strong>not</strong> declared on <see cref="ITestDataRegistry{TTestData}"/>
    /// or any other interface implemented by this class. <typeparamref name="TTestData"/> is contravariant
    /// (<c>in</c>) on <see cref="ITestDataRegistry{TTestData}"/>, which only permits the type parameter to
    /// appear in input positions (e.g., method parameters). Returning <typeparamref name="TTestData"/> from
    /// a method is an output position, which would violate contravariance and is disallowed by the compiler.
    /// <see cref="GetBaseRows"/> is therefore exposed only as a concrete member of
    /// <see cref="DataProviderBase{TTestData, TRow}"/>, not as part of any interface contract.
    /// </para>
    /// <para>
    /// This method relies on the invariant documented on <see cref="distinctRows"/>: every key stored in
    /// the backing dictionary is actually an instance of <typeparamref name="TTestData"/>, since only
    /// <see cref="AddRow"/> and <see cref="AddRange"/> populate the collection. The cast from
    /// <see cref="INamedCase"/> back to <typeparamref name="TTestData"/> is therefore always safe.
    /// </para>
    /// <para>
    /// The returned array is a snapshot of the current collection's keys. The order is determined by
    /// the dictionary's internal structure and may not match insertion order. Subsequent modifications
    /// to the provider will not affect the returned array.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TTestData[] GetBaseRows()
    => ExtractFromDistinctRowsKeys(namedCase => (TTestData)namedCase);

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Projects each key of <see cref="distinctRows"/> into an array using the specified extraction function.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the elements produced by <paramref name="extract"/> and returned in the resulting array.
    /// </typeparam>
    /// <param name="extract">
    /// A function that converts each <see cref="INamedCase"/> key into a value of type <typeparamref name="T"/>.
    /// Called once for every key in <see cref="distinctRows"/>.
    /// </param>
    /// <returns>
    /// An array of <typeparamref name="T"/> containing the projected values for all keys in
    /// <see cref="distinctRows"/>. Returns an empty array if no rows have been added.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is the shared implementation behind <see cref="GetTestCaseNames"/> and <see cref="GetBaseRows"/>.
    /// It pre-allocates the result array based on <see cref="Dictionary{TKey, TValue}.Count"/> to avoid
    /// resizing, then fills it in a single pass over <see cref="distinctRows"/>'s keys.
    /// </para>
    /// <para>
    /// The order of elements is determined by the dictionary's internal key enumeration order and may not
    /// match insertion order. The returned array is a snapshot; subsequent modifications to the provider
    /// will not affect it.
    /// </para>
    /// </remarks>
    private T[] ExtractFromDistinctRowsKeys<T>(Func<INamedCase, T> extract)
    {
        var extractions = new T[distinctRows.Count];
        int i = 0;

        foreach (var namedCase in distinctRows.Keys)
        {
            extractions[i++] = extract(namedCase);
        }

        return extractions;
    }

    /// <summary>
    /// Locates the first stored test case that satisfies <paramref name="matchFound"/> and converts it to a row.
    /// </summary>
    /// <typeparam name="T">
    /// The type of <paramref name="context"/>, used only for the <see langword="null"/> short-circuit check.
    /// </typeparam>
    /// <param name="context">
    /// The lookup value (e.g., a test case name or <see cref="INamedCase"/>) that <paramref name="matchFound"/>
    /// was built from. If <see langword="null"/>, the lookup is skipped and <see langword="default"/> is returned.
    /// </param>
    /// <param name="matchFound">
    /// A predicate that identifies the matching <see cref="INamedCase"/> entry in the collection.
    /// </param>
    /// <returns>
    /// The converted <typeparamref name="TRow"/> for the first matching entry; otherwise, <see langword="default"/>.
    /// </returns>
    /// <remarks>
    /// This helper centralizes the shared lookup-and-convert logic used by both <see cref="GetRow(string)"/>
    /// and <see cref="GetRow(INamedCase)"/> overloads.
    /// </remarks>
    private TRow? GetRow<T>(T context, Func<INamedCase, bool> matchFound)
    {
        if (context is null) return default;

        return distinctRows.Keys.Where(matchFound)
            .Select(namedCase => distinctRows[namedCase])
            .FirstOrDefault();
    }

    #endregion
}