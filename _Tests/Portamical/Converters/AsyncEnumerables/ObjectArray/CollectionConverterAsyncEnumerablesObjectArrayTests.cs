// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.AsyncEnumerables.ObjectArray;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;

namespace Tests.Portamical.Converters.AsyncEnumerables.ObjectArray;

[TestClass]
public class CollectionConverterAsyncEnumerablesObjectArrayTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    #region ToAsyncRowEnumerable(argsCode)

    [TestMethod]
    public async Task ToAsyncRowEnumerable_withArgsCode_yieldsArgumentArrays()
    {
        var item1 = CreateData("obj1", 1);
        var item2 = CreateData("obj2", 2);
        ITestData[] collection = [item1, item2];
        var result = new List<object?[]>();

        await foreach (var row in collection.ToAsyncRowEnumerable(ArgsCode.Instance))
        {
            result.Add(row);
        }

        Assert.HasCount(2, result);
        CollectionAssert.AreEqual(item1.ToArgs(ArgsCode.Instance), result[0]);
        CollectionAssert.AreEqual(item2.ToArgs(ArgsCode.Instance), result[1]);
    }

    [TestMethod]
    public async Task ToAsyncRowEnumerable_withArgsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];

        await Assert.ThrowsExactlyAsync<System.ComponentModel.InvalidEnumArgumentException>(async () =>
        {
            await foreach (var _ in collection.ToAsyncRowEnumerable((ArgsCode)999))
            {
            }
        });
    }

    #endregion

    #region ToAsyncRowEnumerable(argsCode, propsCode)

    [TestMethod]
    public async Task ToAsyncRowEnumerable_withArgsCodeAndPropsCode_yieldsArgumentArrays()
    {
        var item = CreateData("props", 5);
        ITestData[] collection = [item];
        var result = new List<object?[]>();

        await foreach (var row in collection.ToAsyncRowEnumerable(ArgsCode.Properties, PropsCode.All))
        {
            result.Add(row);
        }

        Assert.HasCount(1, result);
        CollectionAssert.AreEqual(item.ToArgs(ArgsCode.Properties, PropsCode.All), result[0]);
    }

    #endregion

    #region ToDistinctAsyncRowEnumerable(argsCode)

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_withArgsCode_deduplicatesByTestCaseName()
    {
        var first = CreateData("obj-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("obj-dup", "result", 2);
        ITestData[] collection = [first, duplicate];
        var result = new List<object?[]>();

        await foreach (var row in collection.ToDistinctAsyncRowEnumerable(ArgsCode.Instance))
        {
            result.Add(row);
        }

        Assert.HasCount(1, result);
        CollectionAssert.AreEqual(first.ToArgs(ArgsCode.Instance), result[0]);
    }

    #endregion

    #region ToDistinctAsyncRowEnumerable(argsCode, propsCode)

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_withArgsCodeAndPropsCode_deduplicatesByTestCaseName()
    {
        var first = CreateData("props-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("props-dup", "result", 2);
        var unique = CreateData("props-unique", 3);
        ITestData[] collection = [first, duplicate, unique];
        var result = new List<object?[]>();

        await foreach (var row in collection.ToDistinctAsyncRowEnumerable(ArgsCode.Properties, PropsCode.All))
        {
            result.Add(row);
        }

        Assert.HasCount(2, result);
    }

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_withArgsCodeAndPropsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid-props")];

        await Assert.ThrowsExactlyAsync<System.ComponentModel.InvalidEnumArgumentException>(async () =>
        {
            await foreach (var _ in collection.ToDistinctAsyncRowEnumerable((ArgsCode)999, PropsCode.All))
            {
            }
        });
    }

    #endregion
}
