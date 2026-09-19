// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.AsyncEnumerables.CustomRow;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;

namespace Tests.Portamical.Converters.AsyncEnumerables.CustomRow;

[TestClass]
public class CollectionConverterAsyncEnumerablesCustomRowTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    #region ToAsyncRowEnumerable with argsCode and testMethodName

    [TestMethod]
    public async Task ToAsyncRowEnumerable_withArgsCodeAndTestMethodName_yieldsConvertedRows()
    {
        var item1 = CreateData("custom1", 1);
        var item2 = CreateData("custom2", 2);
        ITestData[] collection = [item1, item2];
        var result = new List<string>();

        await foreach (var row in collection.ToAsyncRowEnumerable(
            (testData, argsCode, testMethodName) => $"{testMethodName}|{testData.TestCaseName}|{argsCode}",
            ArgsCode.Instance,
            "MyTest"))
        {
            result.Add(row);
        }

        Assert.HasCount(2, result);
        Assert.AreEqual($"MyTest|{item1.TestCaseName}|Instance", result[0]);
        Assert.AreEqual($"MyTest|{item2.TestCaseName}|Instance", result[1]);
    }

    [TestMethod]
    public async Task ToAsyncRowEnumerable_withArgsCodeAndTestMethodName_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("v")];
        Func<ITestData, ArgsCode, string?, string> nullConverter = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
        {
            await foreach (var _ in collection.ToAsyncRowEnumerable(nullConverter, ArgsCode.Instance, null))
            {
            }
        });
    }

    #endregion

    #region ToAsyncRowEnumerable with testMethodName only

    [TestMethod]
    public async Task ToAsyncRowEnumerable_withTestMethodName_yieldsConvertedRows()
    {
        var item = CreateData("simple", 1);
        ITestData[] collection = [item];
        var result = new List<string>();

        await foreach (var row in collection.ToAsyncRowEnumerable(
            (testData, testMethodName) => $"{testMethodName}:{testData.TestCaseName}",
            "SimpleMethod"))
        {
            result.Add(row);
        }

        Assert.HasCount(1, result);
        Assert.AreEqual($"SimpleMethod:{item.TestCaseName}", result[0]);
    }

    #endregion

    #region ToDistinctAsyncRowEnumerable with argsCode and testMethodName

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_withArgsCodeAndTestMethodName_deduplicatesByTestCaseName()
    {
        var first = CreateData("custom-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("custom-dup", "result", 2);
        ITestData[] collection = [first, duplicate];
        var result = new List<string>();

        await foreach (var row in collection.ToDistinctAsyncRowEnumerable(
            (testData, argsCode, testMethodName) => testData.TestCaseName,
            ArgsCode.Instance,
            "DistinctTest"))
        {
            result.Add(row);
        }

        Assert.HasCount(1, result);
        Assert.AreEqual(first.TestCaseName, result[0]);
    }

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_withArgsCodeAndTestMethodName_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];

        await Assert.ThrowsExactlyAsync<System.ComponentModel.InvalidEnumArgumentException>(async () =>
        {
            await foreach (var _ in collection.ToDistinctAsyncRowEnumerable(
                (testData, argsCode, testMethodName) => testData.TestCaseName,
                (ArgsCode)999,
                null))
            {
            }
        });
    }

    #endregion

    #region ToDistinctAsyncRowEnumerable with testMethodName only

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_withTestMethodName_deduplicatesByTestCaseName()
    {
        var first = CreateData("simple-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("simple-dup", "result", 2);
        var unique = CreateData("simple-unique", 3);
        ITestData[] collection = [first, duplicate, unique];
        var result = new List<string>();

        await foreach (var row in collection.ToDistinctAsyncRowEnumerable(
            (testData, testMethodName) => $"{testMethodName}:{testData.TestCaseName}",
            "Method"))
        {
            result.Add(row);
        }

        Assert.HasCount(2, result);
        Assert.AreEqual($"Method:{first.TestCaseName}", result[0]);
        Assert.AreEqual($"Method:{unique.TestCaseName}", result[1]);
    }

    #endregion
}
