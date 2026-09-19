// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.Tasks.ArrayTasks.TestData;
using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;

namespace Tests.Portamical.Converters.Tasks.ArrayTasks.TestData;

[TestClass]
public class CollectionConverterTasksArrayTasksTestDataTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    #region ToArrayTask (identity)

    [TestMethod]
    public async Task ToArrayTask_identity_returnsAllItemsInOrderWithoutDeduplication()
    {
        var first = CreateData("identity-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("identity-dup", "result", 2);
        ITestData[] collection = [first, duplicate];

        var result = await collection.ToArrayTask();

        Assert.HasCount(2, result);
        Assert.AreSame(first, result[0]);
        Assert.AreSame(duplicate, result[1]);
    }

    [TestMethod]
    public async Task ToArrayTask_identity_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () => await empty.ToArrayTask());
    }

    #endregion

    #region ToDistinctArrayTask (identity)

    [TestMethod]
    public async Task ToDistinctArrayTask_identity_deduplicatesByTestCaseName()
    {
        var first = CreateData("distinct-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("distinct-dup", "result", 2);
        var unique = CreateData("distinct-unique", 3);
        ITestData[] collection = [first, duplicate, unique];

        var result = await collection.ToDistinctArrayTask();

        Assert.HasCount(2, result);
        Assert.AreSame(first, result[0]);
        Assert.AreSame(unique, result[1]);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_identity_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await nullCollection.ToDistinctArrayTask());
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_identity_largeCollection_executesAsynchronously()
    {
        var collection = Enumerable.Range(0, 150)
            .Select(i => CreateData($"large{i}", i))
            .ToArray();

        var result = await collection.ToDistinctArrayTask();

        Assert.HasCount(150, result);
    }

    #endregion
}
