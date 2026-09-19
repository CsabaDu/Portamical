// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.AsyncEnumerables.TestData;
using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;

namespace Tests.Portamical.Converters.AsyncEnumerables.TestData;

[TestClass]
public class CollectionConverterAsyncEnumerablesTestDataTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    #region ToAsyncRowEnumerable (identity)

    [TestMethod]
    public async Task ToAsyncRowEnumerable_identity_yieldsAllItemsInOrderWithoutDeduplication()
    {
        var first = CreateData("identity-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("identity-dup", "result", 2);
        ITestData[] collection = [first, duplicate];
        var result = new List<ITestData>();

        await foreach (var item in collection.ToAsyncRowEnumerable())
        {
            result.Add(item);
        }

        Assert.HasCount(2, result);
        Assert.AreSame(first, result[0]);
        Assert.AreSame(duplicate, result[1]);
    }

    [TestMethod]
    public async Task ToAsyncRowEnumerable_identity_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
        {
            await foreach (var _ in empty.ToAsyncRowEnumerable())
            {
            }
        });
    }

    #endregion

    #region ToDistinctAsyncRowEnumerable (identity)

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_identity_deduplicatesByTestCaseName()
    {
        var first = CreateData("distinct-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("distinct-dup", "result", 2);
        var unique = CreateData("distinct-unique", 3);
        ITestData[] collection = [first, duplicate, unique];
        var result = new List<ITestData>();

        await foreach (var item in collection.ToDistinctAsyncRowEnumerable())
        {
            result.Add(item);
        }

        Assert.HasCount(2, result);
        Assert.AreSame(first, result[0]);
        Assert.AreSame(unique, result[1]);
    }

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_identity_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
        {
            await foreach (var _ in nullCollection.ToDistinctAsyncRowEnumerable())
            {
            }
        });
    }

    #endregion
}
