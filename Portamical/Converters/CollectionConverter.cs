// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Core.Processing;

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
    /// An process that adds a single test data item to the container. Called once for every item
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
    /// An process that adds a single test data item to the container. Called once for every item
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
    /// An process that adds a single test data item to the container. Called only for items whose
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
    /// An process that adds a single test data item to the container. Called only for items whose
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

    #region Private ToConvertedRows

    #region TConvertedRows : notnull

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
    /// An process that adds a single test data item to the container.
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
            nameof(testDataCollection));
        _ = NotNull(initConvertedRows, nameof(initConvertedRows));
        var convertedRows = initConvertedRows(snapshot[0]);

        return convertedRows.AddRange(
            snapshot,
            addConvertedRow,
            removeDuplicates,
            skipFirst: true);
    }

    #endregion

    #region TConvertedRows : new()

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
    /// An process that adds a single test data item to the container.
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
            nameof(testDataCollection));
        var convertedRows = new TConvertedRows();

        return convertedRows.AddRange(
            snapshot,
            addConvertedRow,
            removeDuplicates,
            skipFirst: false);
    }

    #endregion

    #endregion

    #region Helper methods

    private static TConvertedRows AddRange<TTestData, TConvertedRows>(
        this TConvertedRows convertedRows,
        TTestData[] snapshot,
        Action<TConvertedRows, TTestData> addConvertedRow,
        bool removeDuplicates,
        bool skipFirst)
    where TTestData : notnull, ITestData
    {
        _ = NotNull(addConvertedRow, nameof(addConvertedRow));

        TestDataProcessor.ProcessCollection(
            testDataCollection: snapshot,
            process: testData => addConvertedRow(convertedRows, testData),
            removeDuplicates,
            skipFirst);

        return convertedRows;
    }

    #endregion
}