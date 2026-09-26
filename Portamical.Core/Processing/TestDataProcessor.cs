// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Identity;
using Portamical.Core.Identity.Model;
using Portamical.Core.TestDataTypes;
using static Portamical.Core.Safety.Validator;

namespace Portamical.Core.Processing;

/// <summary>
/// Provides processing helpers for strongly typed <see cref="ITestData"/> sequences.
/// </summary>
/// <remarks>
/// <para>
/// This type is intentionally sequence-oriented rather than limited to concrete collection types. Callers can
/// pass any <see cref="IEnumerable{T}"/> of <see cref="ITestData"/> and let the processor validate, snapshot,
/// and iterate the items in a consistent way.
/// </para>
/// <para>
/// The helpers in this type are used when the caller needs to process each test data item directly, while still
/// optionally skipping the first item and/or removing duplicates by <see cref="INamedCase.TestCaseName"/>.
/// </para>
/// <para>
/// Instances of this type track already-seen <see cref="INamedCase"/> identities via <see cref="namedCases"/>,
/// which enables the identity-based deduplication used by <see cref="ProcessIfDistinct{TTestData}(TTestData, Action{TTestData})"/>
/// and the static <see cref="ProcessCollection{TTestData}(IEnumerable{TTestData}, Action{TTestData}, bool, bool)"/> helper.
/// </para>
/// </remarks>
public sealed class TestDataProcessor
{
    /// <summary>
    /// Gets the set of <see cref="INamedCase"/> identities that have already been processed by this instance.
    /// </summary>
    /// <remarks>
    /// The set is compared using <see cref="NamedCase.Comparer"/>, based on <see cref="INamedCase.TestCaseName"/>.
    /// Callers may pre-populate this set (e.g., to mark an item as already seen without invoking an process for it)
    /// before calling <see cref="ProcessIfDistinct{TTestData}(TTestData, Action{TTestData})"/>.
    /// </remarks>
    private readonly HashSet<INamedCase> namedCases = new(NamedCase.Comparer);

    /// <summary>
    /// Invokes <paramref name="process"/> for <paramref name="testData"/> only if its
    /// <see cref="INamedCase.TestCaseName"/> has not already been registered in <see cref="namedCases"/>.
    /// </summary>
    /// <typeparam name="TTestData">The type of test data to process. Must implement <see cref="ITestData"/> and be non-null.</typeparam>
    /// <param name="testData">The test data item to conditionally process.</param>
    /// <param name="process">
    /// The process to invoke when <paramref name="testData"/> has not already been seen. If <see langword="null"/>,
    /// no process is invoked, but <paramref name="testData"/> is still registered in <see cref="namedCases"/>.
    /// </param>
    /// <remarks>
    /// Uses <see cref="HashSet{T}.Add(T)"/> as an atomic seen-check-and-register operation: if
    /// <paramref name="testData"/> is successfully added to <see cref="namedCases"/> (i.e., it was not
    /// already present), <paramref name="process"/> is invoked; otherwise the item is skipped as a duplicate.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessIfDistinct<TTestData>(
        TTestData testData,
        Action<TTestData> process)
    where TTestData : notnull, ITestData
    {
        if (namedCases.Add(testData))
        {
            process?.Invoke(testData);
        }
    }

    /// <summary>
    /// Validates a sequence of strongly typed test data, snapshots it, and invokes an process for each item.
    /// </summary>
    /// <typeparam name="TTestData">The type of test data to process. Must implement <see cref="ITestData"/> and be non-null.</typeparam>
    /// <param name="testDataCollection">The source sequence of test data to process. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="process">
    /// The process to invoke for each processed item. If <see langword="null"/>, no process is invoked for any
    /// item, but the snapshot is still validated, deduplication (when requested) still registers seen items,
    /// and no exception is thrown.
    /// </param>
    /// <param name="removeDuplicates">
    /// When <see langword="true"/>, the processor emits each unique <see cref="INamedCase.TestCaseName"/> only once.
    /// Deduplication is applied after the source sequence has been validated and snapshotted, and only when the snapshot
    /// contains more than one item.
    /// </param>
    /// <param name="skipFirst">
    /// When <see langword="true"/>, the first item in the snapshot is excluded from the process callback.
    /// This is useful when the caller has already handled the first item separately, such as during container setup.
    /// If <paramref name="removeDuplicates"/> is also <see langword="true"/>, the first item is still registered as
    /// seen so later duplicates of that item are skipped.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="testDataCollection"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="testDataCollection"/> is empty.</exception>
    /// <remarks>
    /// <para>
    /// The method uses <see cref="NamedCase.Comparer"/> together with <see cref="HashSet{T}.Add(T)"/> to perform
    /// identity-based deduplication on <see cref="INamedCase.TestCaseName"/> while preserving the original ordering
    /// of the snapshot.
    /// </para>
    /// <para>
    /// When deduplication is required, a new <see cref="TestDataProcessor"/> instance is created internally and
    /// <see cref="ProcessIfDistinct{TTestData}(TTestData, Action{TTestData})"/> is invoked for each candidate item.
    /// </para>
    /// <para>
    /// When deduplication is not required (or the snapshot has a single item) and <paramref name="process"/> is
    /// <see langword="null"/>, the snapshot is not iterated at all, since there is no work to perform.
    /// </para>
    /// </remarks>
    public static void ProcessCollection<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        Action<TTestData> process,
        bool removeDuplicates,
        bool skipFirst)
    where TTestData : notnull, ITestData
    {
        var snapshot = NotNullOrEmpty(
            testDataCollection,
            nameof(testDataCollection),
            out var count);

        if (removeDuplicates && count > 1)
        {
            var testDataProcessor = new TestDataProcessor();

            if (skipFirst)
            {
                _ = testDataProcessor.namedCases.Add(snapshot[0]);
            }

            processSnapshot(processTestData: testData =>
                testDataProcessor.ProcessIfDistinct(testData, process)
            );
        }
        else if (process is not null)
        {
            processSnapshot(processTestData: process);
        }

        #region Local methods

        void processSnapshot(Action<TTestData> processTestData)
        {
            var index = skipFirst ? 1 : 0;

            for (int i = index; i < count; i++)
            {
                processTestData(snapshot[i]);
            }
        }

        #endregion
    }
}