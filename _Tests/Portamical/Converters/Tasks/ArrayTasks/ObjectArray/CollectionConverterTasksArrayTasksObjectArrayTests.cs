// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.Tasks.ArrayTasks.ObjectArray;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;

namespace Tests.Portamical.Converters.Tasks.ArrayTasks.ObjectArray;

[TestClass]
public class CollectionConverterTasksArrayTasksObjectArrayTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    #region ToArrayTask(argsCode)

    [TestMethod]
    public async Task ToArrayTask_withArgsCode_returnsArgumentArrays()
    {
        var item1 = CreateData("obj1", 1);
        var item2 = CreateData("obj2", 2);
        ITestData[] collection = [item1, item2];

        var result = await collection.ToArrayTask(ArgsCode.Instance);

        Assert.HasCount(2, result);
        CollectionAssert.AreEqual(item1.ToArgs(ArgsCode.Instance), result[0]);
        CollectionAssert.AreEqual(item2.ToArgs(ArgsCode.Instance), result[1]);
    }

    [TestMethod]
    public async Task ToArrayTask_withArgsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];

        await Assert.ThrowsExactlyAsync<System.ComponentModel.InvalidEnumArgumentException>(
            async () => await collection.ToArrayTask((ArgsCode)999));
    }

    #endregion

    #region ToArrayTask(argsCode, propsCode)

    [TestMethod]
    public async Task ToArrayTask_withArgsCodeAndPropsCode_returnsArgumentArrays()
    {
        var item = CreateData("props", 5);
        ITestData[] collection = [item];

        var result = await collection.ToArrayTask(ArgsCode.Properties, PropsCode.All);

        Assert.HasCount(1, result);
        CollectionAssert.AreEqual(item.ToArgs(ArgsCode.Properties, PropsCode.All), result[0]);
    }

    #endregion

    #region ToDistinctArrayTask(argsCode)

    [TestMethod]
    public async Task ToDistinctArrayTask_withArgsCode_deduplicatesByTestCaseName()
    {
        var first = CreateData("obj-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("obj-dup", "result", 2);
        ITestData[] collection = [first, duplicate];

        var result = await collection.ToDistinctArrayTask(ArgsCode.Instance);

        Assert.HasCount(1, result);
        CollectionAssert.AreEqual(first.ToArgs(ArgsCode.Instance), result[0]);
    }

    #endregion

    #region ToDistinctArrayTask(argsCode, propsCode)

    [TestMethod]
    public async Task ToDistinctArrayTask_withArgsCodeAndPropsCode_deduplicatesByTestCaseName()
    {
        var first = CreateData("props-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("props-dup", "result", 2);
        var unique = CreateData("props-unique", 3);
        ITestData[] collection = [first, duplicate, unique];

        var result = await collection.ToDistinctArrayTask(ArgsCode.Properties, PropsCode.All);

        Assert.HasCount(2, result);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_withArgsCodeAndPropsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid-props")];

        await Assert.ThrowsExactlyAsync<System.ComponentModel.InvalidEnumArgumentException>(
            async () => await collection.ToDistinctArrayTask((ArgsCode)999, PropsCode.All));
    }

    #endregion
}
