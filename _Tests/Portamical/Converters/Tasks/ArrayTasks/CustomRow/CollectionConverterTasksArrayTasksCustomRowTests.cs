// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.Tasks.ArrayTasks.CustomRow;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;

namespace Tests.Portamical.Converters.Tasks.ArrayTasks.CustomRow;

[TestClass]
public class CollectionConverterTasksArrayTasksCustomRowTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    #region ToArrayTask with testMethodName

    [TestMethod]
    public async Task ToArrayTask_withTestMethodName_returnsConvertedRows()
    {
        var item = CreateData("custom", 1);
        ITestData[] collection = [item];

        var result = await collection.ToArrayTask(
            (testData, testMethodName) => $"{testMethodName}:{testData.TestCaseName}",
            "Method");

        Assert.HasCount(1, result);
        Assert.AreEqual($"Method:{item.TestCaseName}", result[0]);
    }

    [TestMethod]
    public async Task ToArrayTask_withTestMethodName_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("v")];
        Func<ITestData, string?, string> nullConverter = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await collection.ToArrayTask(nullConverter, null));
    }

    #endregion

    #region ToArrayTask with argsCode and testMethodName

    [TestMethod]
    public async Task ToArrayTask_withArgsCodeAndTestMethodName_returnsConvertedRows()
    {
        var item1 = CreateData("a1", 1);
        var item2 = CreateData("a2", 2);
        ITestData[] collection = [item1, item2];

        var result = await collection.ToArrayTask(
            (testData, argsCode, testMethodName) => $"{testMethodName}|{testData.TestCaseName}|{argsCode}",
            ArgsCode.Instance,
            "MyTest");

        Assert.HasCount(2, result);
        Assert.AreEqual($"MyTest|{item1.TestCaseName}|Instance", result[0]);
        Assert.AreEqual($"MyTest|{item2.TestCaseName}|Instance", result[1]);
    }

    [TestMethod]
    public async Task ToArrayTask_withArgsCodeAndTestMethodName_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];

        await Assert.ThrowsExactlyAsync<System.ComponentModel.InvalidEnumArgumentException>(
            async () => await collection.ToArrayTask(
                (testData, argsCode, testMethodName) => testData.TestCaseName,
                (ArgsCode)999,
                null));
    }

    #endregion

    #region ToDistinctArrayTask with testMethodName

    [TestMethod]
    public async Task ToDistinctArrayTask_withTestMethodName_deduplicatesByTestCaseName()
    {
        var first = CreateData("dist-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("dist-dup", "result", 2);
        ITestData[] collection = [first, duplicate];

        var result = await collection.ToDistinctArrayTask(
            (testData, testMethodName) => testData.TestCaseName,
            "Method");

        Assert.HasCount(1, result);
        Assert.AreEqual(first.TestCaseName, result[0]);
    }

    #endregion

    #region ToDistinctArrayTask with argsCode and testMethodName

    [TestMethod]
    public async Task ToDistinctArrayTask_withArgsCodeAndTestMethodName_deduplicatesByTestCaseName()
    {
        var first = CreateData("dist-args-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("dist-args-dup", "result", 2);
        var unique = CreateData("dist-args-unique", 3);
        ITestData[] collection = [first, duplicate, unique];

        var result = await collection.ToDistinctArrayTask(
            (testData, argsCode, testMethodName) => testData.TestCaseName,
            ArgsCode.Instance,
            "DistinctMethod");

        Assert.HasCount(2, result);
        Assert.AreEqual(first.TestCaseName, result[0]);
        Assert.AreEqual(unique.TestCaseName, result[1]);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_withArgsCodeAndTestMethodName_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("v")];
        Func<ITestData, ArgsCode, string?, string> nullConverter = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await collection.ToDistinctArrayTask(nullConverter, ArgsCode.Instance, null));
    }

    #endregion
}
