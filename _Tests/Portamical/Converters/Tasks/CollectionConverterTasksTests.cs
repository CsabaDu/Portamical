// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.ComponentModel;
using ArrayTaskCollectionConverter = global::Portamical.Converters.Tasks.ArrayTask.CollectionConverter;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;

namespace Tests.Portamical.Converters.Tasks;

[TestClass]
public class CollectionConverterTasksTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    #region ToArrayTask - identity

    [TestMethod]
    public async Task ToArrayTask_identity_singleElement_returnsArrayOfOne()
    {
        var item = CreateData("a");
        ITestData[] collection = [item];

        var result = await ArrayTaskCollectionConverter.ToArrayTask(collection);

        Assert.HasCount(1, result);
        Assert.AreSame(item, result[0]);
    }

    [TestMethod]
    public async Task ToArrayTask_identity_multipleElements_returnsAllInOrder()
    {
        var first = CreateData("a", 1);
        var second = CreateData("b", 2);
        var third = CreateData("c", 3);
        ITestData[] collection = [first, second, third];

        var result = await ArrayTaskCollectionConverter.ToArrayTask(collection);

        Assert.HasCount(3, result);
        Assert.AreSame(first, result[0]);
        Assert.AreSame(second, result[1]);
        Assert.AreSame(third, result[2]);
    }

    [TestMethod]
    public async Task ToArrayTask_identity_smallCollection_executesSynchronously()
    {
        ITestData[] collection = [CreateData("1"), CreateData("2"), CreateData("3")];

        var task = ArrayTaskCollectionConverter.ToArrayTask(collection);

        Assert.IsTrue(task.IsCompleted);

        var result = await task;

        Assert.HasCount(3, result);
    }

    [TestMethod]
    public async Task ToArrayTask_identity_largeCollection_returnsAllItems()
    {
        var collection = Enumerable.Range(0, 100)
            .Select(i => CreateData($"item{i}", i))
            .ToArray();

        var result = await ArrayTaskCollectionConverter.ToArrayTask(collection);

        Assert.HasCount(100, result);
    }

    #endregion

    #region ToArrayTask - with converter

    [TestMethod]
    public async Task ToArrayTask_converter_returnsConvertedRows()
    {
        var item1 = CreateData("x", 1);
        var item2 = CreateData("y", 2);
        ITestData[] collection = [item1, item2];

        var result = await ArrayTaskCollectionConverter.ToArrayTask(
            collection,
            testData => testData.ToArgs(ArgsCode.Properties));

        Assert.HasCount(2, result);
        CollectionAssert.AreEqual(item1.ToArgs(ArgsCode.Properties), result[0]);
        CollectionAssert.AreEqual(item2.ToArgs(ArgsCode.Properties), result[1]);
    }

    [TestMethod]
    public async Task ToArrayTask_converter_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("test")];
        Func<ITestData, object?[]> nullConverter = null!;

        var ex = await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await ArrayTaskCollectionConverter.ToArrayTask(collection, nullConverter));

        Assert.AreEqual("convertRow", ex.ParamName);
    }

    [TestMethod]
    public async Task ToArrayTask_converter_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("validate")];

        await Assert.ThrowsExactlyAsync<InvalidEnumArgumentException>(
            async () => await ArrayTaskCollectionConverter.ToArrayTask(
                collection,
                testData => testData.ToArgs((ArgsCode)999, PropsCode.All)));
    }

    #endregion

    #region ToDistinctArrayTask - identity

    [TestMethod]
    public async Task ToDistinctArrayTask_identity_singleElement_returnsArrayOfOne()
    {
        ITestData[] collection = [CreateData("a")];
        var result = await ArrayTaskCollectionConverter.ToDistinctArrayTask(collection);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_identity_multipleDistinctElements_returnsAll()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = await ArrayTaskCollectionConverter.ToDistinctArrayTask(collection);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_identity_duplicateTestCaseName_keepsFirstOccurrence()
    {
        var first = CreateData("same");
        var duplicate = TestDataFactory.CreateTestData<int>("same", "result", 99);
        ITestData[] collection = [first, duplicate];
        var result = await ArrayTaskCollectionConverter.ToDistinctArrayTask(collection);
        Assert.HasCount(1, result);
        Assert.AreSame(first, result[0]);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_identity_smallCollection_executesSynchronously()
    {
        // Collections < 10 items should use Task.FromResult
        ITestData[] collection = [CreateData("1"), CreateData("2"), CreateData("3")];
        var task = ArrayTaskCollectionConverter.ToDistinctArrayTask(collection);
        // Task should complete synchronously
        Assert.IsTrue(task.IsCompleted);
        var result = await task;
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_identity_largeCollection_executesAsynchronously()
    {
        // Collections >= 10 items should use Task.Run
        var collection = Enumerable.Range(0, 15)
            .Select(i => CreateData($"item{i}", i))
            .ToArray();
        var result = await ArrayTaskCollectionConverter.ToDistinctArrayTask(collection);
        Assert.HasCount(15, result);
    }

    #endregion

    #region ToDistinctArrayTask - with converter

    [TestMethod]
    public async Task ToDistinctArrayTask_converter_returnsConvertedRows()
    {
        var item1 = CreateData("x", 1);
        var item2 = CreateData("y", 2);
        ITestData[] collection = [item1, item2];

        var result = await ArrayTaskCollectionConverter.ToDistinctArrayTask(collection, testData => testData.ToArgs(ArgsCode.Properties));

        Assert.HasCount(2, result);
        CollectionAssert.AreEqual(item1.ToArgs(ArgsCode.Properties), result[0]);
        CollectionAssert.AreEqual(item2.ToArgs(ArgsCode.Properties), result[1]);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_converter_deduplicatesByTestCaseName()
    {
        var duplicate = TestDataFactory.CreateTestData<int>("async-dup", "result", 77);
        ITestData[] collection = [CreateData("async-dup"), duplicate];

        var result = await ArrayTaskCollectionConverter.ToDistinctArrayTask(collection, testData => testData.ToArgs(ArgsCode.Properties));

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_converter_validatesConvertRow()
    {
        ITestData[] collection = [CreateData("test")];
        Func<ITestData, object?[]> nullConverter = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await ArrayTaskCollectionConverter.ToDistinctArrayTask(collection, nullConverter));
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_converter_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("validate")];

        await Assert.ThrowsExactlyAsync<InvalidEnumArgumentException>(
            async () => await ArrayTaskCollectionConverter.ToDistinctArrayTask(collection, testData => testData.ToArgs((ArgsCode)999, PropsCode.All)));
    }

    #endregion

    #region Error handling

    [TestMethod]
    public async Task ToDistinctArrayTask_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        try
        {
            await ArrayTaskCollectionConverter.ToDistinctArrayTask(nullCollection);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected exception
        }
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();
        try
        {
            await ArrayTaskCollectionConverter.ToDistinctArrayTask(empty);
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException)
        {
            // Expected exception
        }
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("test")];
        Func<ITestData, object?[]> nullConverter = null!;
        var ex = await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await ArrayTaskCollectionConverter.ToDistinctArrayTask(collection, nullConverter));
        Assert.AreEqual("convertRow", ex.ParamName);
    }

    #endregion

    #region Performance characteristics

    [TestMethod]
    public async Task ToDistinctArrayTask_smallCollection_completesQuickly()
    {
        // Small collections should complete very quickly
        ITestData[] collection = [CreateData("perf1"), CreateData("perf2")];
var result = await ArrayTaskCollectionConverter.ToDistinctArrayTask(collection);
Assert.HasCount(2, result);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_largeCollection_handlesEfficiently()
    {
        // Large collection should still be efficient
        var collection = Enumerable.Range(0, 100)
            .Select(i => CreateData($"large{i}", i))
            .ToArray();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await ArrayTaskCollectionConverter.ToDistinctArrayTask(collection);
        stopwatch.Stop();

        Assert.HasCount(100, result);
        // Should complete reasonably quickly even with 100 items
        Assert.IsLessThan(1000, stopwatch.ElapsedMilliseconds,
            $"Large collection took {stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion
}
