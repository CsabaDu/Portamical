// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.ComponentModel;
using Portamical.Converters.Tasks;
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

    #region ToDistinctArrayTask - identity

    [TestMethod]
    public async Task ToDistinctArrayTask_identity_singleElement_returnsArrayOfOne()
    {
        ITestData[] collection = [CreateData("a")];
        var result = await collection.ToDistinctArrayTask();
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_identity_multipleDistinctElements_returnsAll()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = await collection.ToDistinctArrayTask();
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_identity_duplicateTestCaseName_keepsFirstOccurrence()
    {
        var first = CreateData("same");
        var duplicate = TestDataFactory.CreateTestData<int>("same", "result", 99);
        ITestData[] collection = [first, duplicate];
        var result = await collection.ToDistinctArrayTask();
        Assert.HasCount(1, result);
        Assert.AreSame(first, result[0]);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_identity_smallCollection_executesSynchronously()
    {
        // Collections < 10 items should use Task.FromResult
        ITestData[] collection = [CreateData("1"), CreateData("2"), CreateData("3")];
        var task = collection.ToDistinctArrayTask();
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
        var result = await collection.ToDistinctArrayTask();
        Assert.HasCount(15, result);
    }

    #endregion

    #region ToDistinctArrayTask - with ArgsCode

    [TestMethod]
    public async Task ToDistinctArrayTask_argsCode_singleElement_returnsArgsArray()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = await collection.ToDistinctArrayTask(ArgsCode.Properties);
        Assert.HasCount(1, result);
        CollectionAssert.AreEqual(item.ToArgs(ArgsCode.Properties), result[0]);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_argsCode_multipleElements_returnsAllArgs()
    {
        var item1 = CreateData("y", 1);
        var item2 = CreateData("z", 2);
        ITestData[] collection = [item1, item2];
        var result = await collection.ToDistinctArrayTask(ArgsCode.Instance);
        Assert.HasCount(2, result);
        CollectionAssert.AreEqual(item1.ToArgs(ArgsCode.Instance), result[0]);
        CollectionAssert.AreEqual(item2.ToArgs(ArgsCode.Instance), result[1]);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_argsCode_deduplicatesByTestCaseName()
    {
        var duplicate = TestDataFactory.CreateTestData<int>("async-dup", "result", 77);
        ITestData[] collection = [CreateData("async-dup"), duplicate];
        var result = await collection.ToDistinctArrayTask(ArgsCode.Properties);
        Assert.HasCount(1, result);
    }

    #endregion

    #region ToDistinctArrayTask - with ArgsCode and PropsCode

    [TestMethod]
    public async Task ToDistinctArrayTask_argsCodeAndPropsCode_returnsCorrectArgs()
    {
        var item = CreateData("props");
        ITestData[] collection = [item];
        var result = await collection.ToDistinctArrayTask(ArgsCode.Properties, PropsCode.All);
        Assert.HasCount(1, result);
        CollectionAssert.AreEqual(item.ToArgs(ArgsCode.Properties, PropsCode.All), result[0]);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_argsCodeAndPropsCodeTrim_returnsCorrectArgs()
    {
        var item = CreateData("trim");
        ITestData[] collection = [item];
        var result = await collection.ToDistinctArrayTask(ArgsCode.Properties, PropsCode.TrimTestCaseName);
        Assert.HasCount(1, result);
        CollectionAssert.AreEqual(item.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName), result[0]);
    }

    #endregion

    #region ToDistinctArrayTask - with converter and test method name

    [TestMethod]
    public async Task ToDistinctArrayTask_converterWithMethodName_passesMethodName()
    {
        string? capturedMethodName = null;
        ITestData[] collection = [CreateData("method")];
        await collection.ToDistinctArrayTask(
            (testData, methodName) =>
            {
                capturedMethodName = methodName;
                return testData.ToArgs(ArgsCode.Instance);
            },
            "MyTestMethod");

        Assert.AreEqual("MyTestMethod", capturedMethodName);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_converterWithMethodName_acceptsNullMethodName()
    {
        string? capturedMethodName = "notNull";
        ITestData[] collection = [CreateData("nullmethod")];
        await collection.ToDistinctArrayTask(
            (testData, methodName) =>
            {
                capturedMethodName = methodName;
                return testData.ToArgs(ArgsCode.Instance);
            },
            null);

        Assert.IsNull(capturedMethodName);
    }

    #endregion

    #region ToDistinctArrayTask - with converter, ArgsCode, and test method name

    [TestMethod]
    public async Task ToDistinctArrayTask_converterWithArgsCode_passesArgsCodeAndMethodName()
    {
        ArgsCode? capturedArgsCode = null;
        string? capturedMethodName = null;
        ITestData[] collection = [CreateData("conv")];
        await collection.ToDistinctArrayTask(
            (testData, argsCode, methodName) =>
            {
                capturedArgsCode = argsCode;
                capturedMethodName = methodName;
                return testData.ToArgs(argsCode, PropsCode.All);
            },
            ArgsCode.Properties,
            "ConvTest");

        Assert.AreEqual(ArgsCode.Properties, capturedArgsCode);
        Assert.AreEqual("ConvTest", capturedMethodName);
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_converterWithArgsCode_validatesArgsCode()
    {
        ITestData[] collection = [CreateData("validate")];
        try
        {
            await collection.ToDistinctArrayTask(
                (testData, argsCode, methodName) => testData.ToArgs(argsCode, PropsCode.All),
                (ArgsCode)999,
                null);
            Assert.Fail("Expected InvalidEnumArgumentException was not thrown");
        }
        catch (InvalidEnumArgumentException)
        {
            // Expected exception
        }
    }

    #endregion

    #region Error handling

    [TestMethod]
    public async Task ToDistinctArrayTask_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        try
        {
            await nullCollection.ToDistinctArrayTask();
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
            await empty.ToDistinctArrayTask();
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException)
        {
            // Expected exception
        }
    }

    #endregion

    #region Performance characteristics

    [TestMethod]
    public async Task ToDistinctArrayTask_smallCollection_completesQuickly()
    {
        // Small collections should complete very quickly
        ITestData[] collection = [CreateData("perf1"), CreateData("perf2")];
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await collection.ToDistinctArrayTask();
        stopwatch.Stop();
        
        // Should complete in well under 100ms (typically < 1ms)
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100,
            $"Small collection took {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task ToDistinctArrayTask_largeCollection_handlesEfficiently()
    {
        // Large collection should still be efficient
        var collection = Enumerable.Range(0, 100)
            .Select(i => CreateData($"large{i}", i))
            .ToArray();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await collection.ToDistinctArrayTask();
        stopwatch.Stop();

        Assert.HasCount(100, result);
        // Should complete reasonably quickly even with 100 items
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000,
            $"Large collection took {stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion
}
