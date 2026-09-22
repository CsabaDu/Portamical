// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using TaskCollectionConverter = global::Portamical.Converters.Tasks.CollectionConverter;
using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;

namespace Tests.Portamical.Converters.Tasks;

[TestClass]
public class CollectionConverterTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    #region ToConvertedRowsTask

    [TestMethod]
    public async Task ToConvertedRowsTask_smallCollection_returnsConvertedResult()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b")];

        var result = await TaskCollectionConverter.ToConvertedRowsTask(
            collection,
            rows => rows.Count());

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public async Task ToConvertedRowsTask_smallCollection_executesSynchronously()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b")];

        var task = TaskCollectionConverter.ToConvertedRowsTask(
            collection,
            rows => rows.Count());

        Assert.IsTrue(task.IsCompleted);

        var result = await task;

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public async Task ToConvertedRowsTask_largeCollection_returnsConvertedResult()
    {
        var collection = Enumerable.Range(0, 100)
            .Select(i => CreateData($"item{i}", i))
            .ToArray();

        var result = await TaskCollectionConverter.ToConvertedRowsTask(
            collection,
            rows => rows.Count());

        Assert.AreEqual(100, result);
    }

    [TestMethod]
    public async Task ToConvertedRowsTask_doesNotDeduplicate_keepsDuplicates()
    {
        var first = CreateData("dup");
        var duplicate = TestDataFactory.CreateTestData<int>("dup", "result", 2);
        ITestData[] collection = [first, duplicate];

        var result = await TaskCollectionConverter.ToConvertedRowsTask(
            collection,
            rows => rows.Count());

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public async Task ToConvertedRowsTask_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await TaskCollectionConverter.ToConvertedRowsTask(
                nullCollection,
                rows => rows.Count()));
    }

    [TestMethod]
    public async Task ToConvertedRowsTask_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () => await TaskCollectionConverter.ToConvertedRowsTask(
                empty,
                rows => rows.Count()));
    }

    [TestMethod]
    public async Task ToConvertedRowsTask_nullConvertRows_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("a")];
        Func<IEnumerable<ITestData>, int> nullConvertRows = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await TaskCollectionConverter.ToConvertedRowsTask(
                collection,
                nullConvertRows));
    }

    #endregion

    #region ToDistinctConvertedRowsTask

    [TestMethod]
    public async Task ToDistinctConvertedRowsTask_smallCollection_deduplicatesByTestCaseName()
    {
        var first = CreateData("dup");
        var duplicate = TestDataFactory.CreateTestData<int>("dup", "result", 2);
        var other = CreateData("other");
        ITestData[] collection = [first, duplicate, other];

        var result = await TaskCollectionConverter.ToDistinctConvertedRowsTask(
            collection,
            rows => rows.Count());

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public async Task ToDistinctConvertedRowsTask_smallCollection_executesSynchronously()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b")];

        var task = TaskCollectionConverter.ToDistinctConvertedRowsTask(
            collection,
            rows => rows.Count());

        Assert.IsTrue(task.IsCompleted);

        var result = await task;

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public async Task ToDistinctConvertedRowsTask_largeCollection_returnsDistinctCount()
    {
        var collection = Enumerable.Range(0, 100)
            .Select(i => CreateData($"item{i}", i))
            .ToArray();

        var result = await TaskCollectionConverter.ToDistinctConvertedRowsTask(
            collection,
            rows => rows.Count());

        Assert.AreEqual(100, result);
    }

    [TestMethod]
    public async Task ToDistinctConvertedRowsTask_keepsFirstOccurrenceOfDuplicate()
    {
        var first = CreateData("same", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("same", "result", 99);
        ITestData[] collection = [first, duplicate];

        var result = await TaskCollectionConverter.ToDistinctConvertedRowsTask(
            collection,
            rows => rows.Single());

        Assert.AreSame(first, result);
    }

    [TestMethod]
    public async Task ToDistinctConvertedRowsTask_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await TaskCollectionConverter.ToDistinctConvertedRowsTask(
                nullCollection,
                rows => rows.Count()));
    }

    [TestMethod]
    public async Task ToDistinctConvertedRowsTask_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () => await TaskCollectionConverter.ToDistinctConvertedRowsTask(
                empty,
                rows => rows.Count()));
    }

    [TestMethod]
    public async Task ToDistinctConvertedRowsTask_nullConvertRows_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("a")];
        Func<IEnumerable<ITestData>, int> nullConvertRows = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await TaskCollectionConverter.ToDistinctConvertedRowsTask(
                collection,
                nullConvertRows));
    }

    [TestMethod]
    public async Task ToDistinctConvertedRowsTask_thresholdEvaluatedAfterDeduplication_executesSynchronously()
    {
        // 150 items, but all sharing the same TestCaseName so only 1 survives deduplication.
        // The threshold check (100) must be evaluated against the distinct count, not the raw input size.
        var collection = Enumerable.Range(0, 150)
            .Select(i => TestDataFactory.CreateTestData<int>("duplicate", "result", i))
            .ToArray();

        var task = TaskCollectionConverter.ToDistinctConvertedRowsTask(
            collection,
            rows => rows.Count());

        Assert.IsTrue(task.IsCompleted);

        var result = await task;

        Assert.AreEqual(1, result);
    }

    #endregion
}
