// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters;

/// <summary>
/// Provides internal extension methods for converting collections of test data into arbitrary
/// converted-rows container types, with optional deduplication based on test case identity.
/// </summary>
/// <remarks>
/// <para>
/// This class is the generic core of the converter infrastructure in the <see cref="Portamical.Converters"/>
/// namespace hierarchy. Unlike <see cref="RowArrays.CollectionConverter"/>, which always produces arrays,
/// this class supports any <typeparamref name="TConvertedRows"/> container type (e.g., <c>List{T}</c>,
/// custom data provider collections), as long as callers supply the appropriate initialization and
/// item-adding delegates.
/// </para>
/// <para>
/// <strong>Two Initialization Strategies:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///   <strong>Custom initializer:</strong> Overloads accepting <c>initConvertedRows</c> build the container
///   from the first test data item (useful when the container's construction depends on the first item,
///   e.g., inferring a capacity or a key).
///   </item>
///   <item>
///   <strong>Parameterless constructor:</strong> Overloads constrained by <c>new()</c> create the container
///   via its default constructor and add every item, including the first.
///   </item>
/// </list>
/// <para>
/// <strong>Deduplication:</strong> The <c>ToDistinctConvertedRows</c> methods remove duplicate test data
/// based on <see cref="INamedCase.TestCaseName"/> using <see cref="NamedCase.Comparer"/>. The first
/// occurrence of each test case name is kept; later duplicates are skipped.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> All methods are stateless and thread-safe; however, the returned
/// <typeparamref name="TConvertedRows"/> instance itself is not thread-safe unless its type guarantees so.
/// </para>
/// </remarks>
internal static class CollectionConverter
{
    #region ToConvertedRows

    /// <summary>
    /// Converts a collection of test data into a <typeparamref name="TConvertedRows"/> container, initializing
    /// the container from the first test data item and adding the remaining items without deduplication.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TConvertedRows">
    /// The type of the container that accumulates the converted rows.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="initConvertedRows">
    /// A function that creates and initializes the <typeparamref name="TConvertedRows"/> container from the
    /// first test data item in the collection.
    /// </param>
    /// <param name="addConvertedRow">
    /// An action that adds a single test data item to the container. Called once for every item
    /// after the first.
    /// </param>
    /// <returns>
    /// The <typeparamref name="TConvertedRows"/> container populated with all items from
    /// <paramref name="testDataCollection"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// This overload delegates to the private helper with <c>removeDuplicates: false</c>. If the collection
    /// contains a single item, <paramref name="addConvertedRow"/> is never invoked.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TConvertedRows ToConvertedRows<TTestData, TConvertedRows>(
    this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TConvertedRows> initConvertedRows,
        Action<TConvertedRows, TTestData> addConvertedRow)
    where TTestData : notnull, ITestData
    where TConvertedRows : notnull
    => testDataCollection.ToConvertedRows(
        initConvertedRows,
        addConvertedRow,
        removeDuplicates: false);

    /// <summary>
    /// Converts a collection of test data into a <typeparamref name="TConvertedRows"/> container, creating the
    /// container via its parameterless constructor and adding every item without deduplication.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TConvertedRows">
    /// The type of the container that accumulates the converted rows. Must have a public parameterless constructor.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="addConvertedRow">
    /// An action that adds a single test data item to the container. Called once for every item
    /// in the collection.
    /// </param>
    /// <returns>
    /// The <typeparamref name="TConvertedRows"/> container populated with all items from
    /// <paramref name="testDataCollection"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// This overload delegates to the private helper with <c>removeDuplicates: false</c>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TConvertedRows ToConvertedRows<TTestData, TConvertedRows>(
    this IEnumerable<TTestData> testDataCollection,
        Action<TConvertedRows, TTestData> addConvertedRow)
    where TTestData : notnull, ITestData
    where TConvertedRows : new()
    => testDataCollection.ToConvertedRows(
        addConvertedRow,
        removeDuplicates: false);

    #endregion

    #region ToDistinctConvertedRows

    /// <summary>
    /// Converts a collection of test data into a <typeparamref name="TConvertedRows"/> container, initializing
    /// the container from the first test data item and adding the remaining items, skipping duplicates based on
    /// test case name.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TConvertedRows">
    /// The type of the container that accumulates the converted rows.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="initConvertedRows">
    /// A function that creates and initializes the <typeparamref name="TConvertedRows"/> container from the
    /// first test data item in the collection.
    /// </param>
    /// <param name="addConvertedRow">
    /// An action that adds a single test data item to the container. Called only for items whose
    /// <see cref="INamedCase.TestCaseName"/> has not already been seen.
    /// </param>
    /// <returns>
    /// The <typeparamref name="TConvertedRows"/> container populated with the distinct items from
    /// <paramref name="testDataCollection"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Deduplication uses <see cref="NamedCase.Comparer"/> to compare <see cref="INamedCase.TestCaseName"/>
    /// values. The first test data item is always treated as already added to the container (via
    /// <paramref name="initConvertedRows"/>) and is pre-registered as seen before the remaining items
    /// are evaluated.
    /// </para>
    /// <para>
    /// This overload delegates to the private helper with <c>removeDuplicates: true</c>.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TConvertedRows ToDistinctConvertedRows<TTestData, TConvertedRows>(
    this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TConvertedRows> initConvertedRows,
        Action<TConvertedRows, TTestData> addConvertedRow)
    where TTestData : notnull, ITestData
    where TConvertedRows : notnull
    => testDataCollection.ToConvertedRows(
        initConvertedRows,
        addConvertedRow,
        removeDuplicates: true);

    /// <summary>
    /// Converts a collection of test data into a <typeparamref name="TConvertedRows"/> container, creating the
    /// container via its parameterless constructor and adding only distinct items based on test case name.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TConvertedRows">
    /// The type of the container that accumulates the converted rows. Must have a public parameterless constructor.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="addConvertedRow">
    /// An action that adds a single test data item to the container. Called only for items whose
    /// <see cref="INamedCase.TestCaseName"/> has not already been seen.
    /// </param>
    /// <returns>
    /// The <typeparamref name="TConvertedRows"/> container populated with the distinct items from
    /// <paramref name="testDataCollection"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// Deduplication uses <see cref="NamedCase.Comparer"/> to compare <see cref="INamedCase.TestCaseName"/>
    /// values, starting with an empty seen-set since no item has been added prior to iteration.
    /// This overload delegates to the private helper with <c>removeDuplicates: true</c>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TConvertedRows ToDistinctConvertedRows<TTestData, TConvertedRows>(
    this IEnumerable<TTestData> testDataCollection,
        Action<TConvertedRows, TTestData> addConvertedRow)
    where TTestData : notnull, ITestData
    where TConvertedRows : new()
    => testDataCollection.ToConvertedRows(
        addConvertedRow,
        removeDuplicates: true);

    #endregion

    #region Helper methods

    /// <summary>
    /// Validates and snapshots <paramref name="testDataCollection"/>, initializes the
    /// <typeparamref name="TConvertedRows"/> container from the first item, and adds the remaining items,
    /// optionally skipping duplicates.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TConvertedRows">
    /// The type of the container that accumulates the converted rows.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="initConvertedRows">
    /// A function that creates and initializes the container from the first test data item.
    /// </param>
    /// <param name="addConvertedRow">
    /// An action that adds a single test data item to the container.
    /// </param>
    /// <param name="removeDuplicates">
    /// If <see langword="true"/>, skips items whose <see cref="INamedCase.TestCaseName"/> duplicates
    /// an already-processed item (including the first item). If <see langword="false"/>, adds every item.
    /// </param>
    /// <returns>
    /// The <typeparamref name="TConvertedRows"/> container populated according to
    /// <paramref name="removeDuplicates"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// When the snapshot contains exactly one item, the initialized container is returned immediately
    /// without invoking <paramref name="addConvertedRow"/> or evaluating <paramref name="removeDuplicates"/>.
    /// </remarks>
    private static TConvertedRows ToConvertedRows<TTestData, TConvertedRows>(
        this IEnumerable<TTestData> testDataCollection,
        Func<TTestData, TConvertedRows> initConvertedRows,
        Action<TConvertedRows, TTestData> addConvertedRow,
        bool removeDuplicates)
    where TTestData : notnull, ITestData
    where TConvertedRows : notnull
    {
        var snapshot = NotNullOrEmpty(
            testDataCollection,
            nameof(testDataCollection),
            out var count);
        var convertedRows = initConvertedRows(snapshot[0]);

        if (count == 1)
        {
            return convertedRows;
        }

        return snapshot.ToConvertedRows(
            count,
            convertedRows,
            addConvertedRow,
            removeDuplicates,
            skipFirst: true);
    }

    /// <summary>
    /// Validates and snapshots <paramref name="testDataCollection"/>, creates the
    /// <typeparamref name="TConvertedRows"/> container via its parameterless constructor, and adds every
    /// item, optionally skipping duplicates.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the input collection. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TConvertedRows">
    /// The type of the container that accumulates the converted rows. Must have a public parameterless constructor.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection of test data to process. Cannot be null or empty.
    /// </param>
    /// <param name="addConvertedRow">
    /// An action that adds a single test data item to the container.
    /// </param>
    /// <param name="removeDuplicates">
    /// If <see langword="true"/>, skips items whose <see cref="INamedCase.TestCaseName"/> duplicates an
    /// earlier item. If <see langword="false"/>, adds every item.
    /// </param>
    /// <returns>
    /// The <typeparamref name="TConvertedRows"/> container populated according to
    /// <paramref name="removeDuplicates"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    private static TConvertedRows ToConvertedRows<TTestData, TConvertedRows>(
        this IEnumerable<TTestData> testDataCollection,
        Action<TConvertedRows, TTestData> addConvertedRow,
        bool removeDuplicates)
    where TTestData : notnull, ITestData
    where TConvertedRows : new()
    {
        var snapshot = NotNullOrEmpty(
            testDataCollection,
            nameof(testDataCollection),
            out var count);
        var convertedRows = new TConvertedRows();

        return snapshot.ToConvertedRows(
            count,
            convertedRows,
            addConvertedRow,
            removeDuplicates,
            skipFirst: false);
    }

    /// <summary>
    /// Iterates over a pre-validated snapshot array, adding each item to <paramref name="convertedRows"/> via
    /// <paramref name="addConvertedRow"/>, with optional deduplication and skip-first behavior.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the snapshot array. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <typeparam name="TConvertedRows">
    /// The type of the container that accumulates the converted rows.
    /// </typeparam>
    /// <param name="snapshot">
    /// The pre-validated snapshot array of test data to iterate through. Must not be null or empty.
    /// </param>
    /// <param name="convertedRows">
    /// The already-initialized container to which converted rows are added.
    /// </param>
    /// <param name="addConvertedRow">
    /// An action that adds a single test data item to <paramref name="convertedRows"/>.
    /// </param>
    /// <param name="removeDuplicates">
    /// If <see langword="true"/>, removes duplicate test data based on <see cref="INamedCase.TestCaseName"/>
    /// using <see cref="NamedCase.Comparer"/>. If <paramref name="skipFirst"/> is also <see langword="true"/>,
    /// <c>snapshot[0]</c> is pre-registered as seen (since it was already added by the caller) before
    /// iteration begins. If <see langword="false"/>, processes all items without deduplication.
    /// </param>
    /// <param name="skipFirst">
    /// If <see langword="true"/>, starts iteration from index 1 (skipping the first item, which the caller
    /// has already added to <paramref name="convertedRows"/>). If <see langword="false"/>, starts from index 0.
    /// </param>
    /// <returns>
    /// The <paramref name="convertedRows"/> instance, populated with the processed items.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Deduplication Strategy:</strong> When <paramref name="removeDuplicates"/> is
    /// <see langword="true"/>, a <see cref="HashSet{T}"/> keyed by <see cref="NamedCase.Comparer"/> tracks
    /// seen test case names. <see cref="HashSet{T}.Add"/> returns <see langword="true"/> only for items not
    /// already present, so <paramref name="addConvertedRow"/> is invoked exclusively for distinct items.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Uses a local function <c>addRange</c> to avoid duplicating the
    /// iteration logic between the deduplicated and non-deduplicated code paths, with the start index
    /// determined once based on <paramref name="skipFirst"/>.
    /// </para>
    /// </remarks>
    private static TConvertedRows ToConvertedRows<TTestData, TConvertedRows>(
        this TTestData[] snapshot,
        int count,
        TConvertedRows convertedRows,
        Action<TConvertedRows, TTestData> addConvertedRow,
        bool removeDuplicates,
        bool skipFirst)
    where TTestData : notnull, ITestData
    {
        if (removeDuplicates)
        {
            var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);

            if (skipFirst)
            {
                _ = namedCases.Add(snapshot[0]);
            }

            addRange(td => td.AddConvertedIfDistinct(namedCases,
                addConverted: testData => addConvertedRow(convertedRows, testData)));
        }
        else
        {
            addRange(testData => addConvertedRow(convertedRows, testData));
        }

        #region Local function

        void addRange(Action<TTestData> addConverted)
        {
            var startIndex = skipFirst ? 1 : 0;

            for (int i = startIndex; i < count; i++)
            {
                addConverted(snapshot[i]);
            }
        }

        return convertedRows;

        #endregion
    }

    /// <summary>
    /// Adds <paramref name="testData"/> to <paramref name="addConverted"/> only if it has not already been
    /// seen, based on <see cref="INamedCase.TestCaseName"/> identity.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testData">
    /// The test data item to conditionally process.
    /// </param>
    /// <param name="namedCases">
    /// The set of already-seen test cases, compared via <see cref="NamedCase.Comparer"/>. Updated in place
    /// with <paramref name="testData"/> when it is not already present.
    /// </param>
    /// <param name="addConverted">
    /// The action invoked with <paramref name="testData"/> when it is not a duplicate.
    /// </param>
    /// <remarks>
    /// Uses <see cref="HashSet{T}.Add(T)"/> as an atomic seen-check-and-register operation: if
    /// <paramref name="testData"/> is successfully added to <paramref name="namedCases"/> (i.e., it was not
    /// already present), <paramref name="addConverted"/> is invoked; otherwise the item is skipped as a duplicate.
    /// </remarks>
    internal static void AddConvertedIfDistinct<TTestData>(
        this TTestData testData,
        HashSet<INamedCase> namedCases,
        Action<TTestData> addConverted)
    where TTestData : notnull, ITestData
    {
        if (namedCases.Add(testData))
        {
            addConverted(testData);
        }
    }

    #endregion
}
