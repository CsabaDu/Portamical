// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters;

/// <summary>
/// Provides internal utility methods for collection conversion operations, including iteration,
/// snapshotting, and deduplication logic used by converter classes.
/// </summary>
/// <remarks>
/// <para>
/// This class contains low-level helper methods that are shared across multiple converter implementations
/// in the <see cref="Portamical.Converters"/> namespace. All methods are marked <c>internal</c> and are
/// not part of the public API.
/// </para>
/// <para>
/// <strong>Key Utilities:</strong>
/// </para>
/// <list type="bullet">
///   <item><see cref="AddConvertedRows{TTestData}"/> - Efficient iteration with optional deduplication and skip-first capability</item>
///   <item><see cref="SnapshotWithCount{TTestData}"/> - Collection validation and snapshotting with count</item>
/// </list>
/// </remarks>
internal static class Utilities
{
    /// <summary>
    /// Internal helper method that iterates through a snapshot array and applies a conversion action to each item,
    /// with support for deduplication and optionally skipping the first item.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data in the snapshot array. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="snapshot">
    /// The pre-validated snapshot array of test data to iterate through. Must not be null or empty.
    /// </param>
    /// <param name="addConvertedRow">
    /// An action that processes and adds each test data item (typically after conversion).
    /// Called once for each item that passes deduplication checks (if enabled).
    /// </param>
    /// <param name="beDistinct">
    /// If <see langword="true"/>, removes duplicate test data based on <see cref="INamedCase.TestCaseName"/>
    /// using <see cref="NamedCase.Comparer"/>. The first item in the snapshot is always added to the deduplication
    /// set before iteration begins. If <see langword="false"/>, processes all items without deduplication.
    /// </param>
    /// <param name="skipFirst">
    /// If <see langword="true"/>, starts iteration from index 1 (skipping the first item). Useful when the
    /// first item has already been processed for initialization. If <see langword="false"/>, starts from index 0.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method provides a flexible iteration strategy used by multiple converter methods. It encapsulates
    /// the common pattern of iterating through a snapshot with optional deduplication and first-item handling.
    /// </para>
    /// <para>
    /// <strong>Deduplication Strategy:</strong> When <paramref name="beDistinct"/> is <see langword="true"/>:
    /// </para>
    /// <list type="bullet">
    ///   <item>Creates a <see cref="HashSet{T}"/> with <see cref="NamedCase.Comparer"/> for O(1) lookups</item>
    ///   <item>Pre-populates the set with the first item (<c>snapshot[0]</c>) to mark it as seen</item>
    ///   <item>For each item, uses <see cref="HashSet{T}.Add"/> which returns <see langword="true"/> if the item is unique (not already in the set)</item>
    ///   <item>Executes <paramref name="addConvertedRow"/> only when <c>Add</c> returns <see langword="true"/>, ensuring only distinct items are processed</item>
    /// </list>
    /// <para>
    /// <strong>Performance:</strong> Uses a local function <c>addRange</c> to minimize allocation and
    /// provide efficient iteration with configurable start index based on <paramref name="skipFirst"/>.
    /// The direct <see cref="HashSet{T}.Add"/> check provides O(1) deduplication without additional method calls.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Process all items without deduplication
    /// AddConvertedRows(snapshot, 
    ///     addConvertedRow: td => collection.Add(Convert(td)),
    ///     beDistinct: false,
    ///     skipFirst: false);
    /// 
    /// // Process remaining items (skip first) with deduplication
    /// AddConvertedRows(snapshot,
    ///     addConvertedRow: td => collection.Add(Convert(td)),
    ///     beDistinct: true,
    ///     skipFirst: true);
    /// </code>
    /// </example>
    internal static void AddConvertedRows<TTestData>(
        TTestData[] snapshot,
        Action<TTestData> addConvertedRow,
        bool beDistinct,
        bool skipFirst)
    where TTestData : notnull, ITestData
    {
        if (beDistinct)
        {
            var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
            _ = namedCases.Add(snapshot[0]);

            addRange((testData) =>
            {
                if (namedCases.Add(testData))
                {
                    addConvertedRow(testData);
                }
            });
        }
        else
        {
            addRange(addConvertedRow);
        }

        void addRange(Action<TTestData> addConverted)
        {
            var startIndex = skipFirst ? 1 : 0;

            for (int i = startIndex; i < snapshot.Length; i++)
            {
                var testData = snapshot[i];
                addConverted(testData);
            }
        }
    }

    /// <summary>
    /// Internal helper that validates and snapshots a collection, returning both the snapshot array and its count.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must implement <see cref="ITestData"/> and be non-null.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The collection to snapshot. Cannot be null or empty.
    /// </param>
    /// <returns>
    /// A tuple containing the snapshot array and its length.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="testDataCollection"/> is empty.
    /// </exception>
    /// <remarks>
    /// This method is used internally to avoid recalculating array length in performance-critical paths.
    /// </remarks>
    internal static (TTestData[] snapshot, int count) SnapshotWithCount<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    {
        var snapshot = NotNullOrEmpty(testDataCollection, nameof(testDataCollection));
        var count = snapshot.Length;

        return (snapshot, count);
    }
}
