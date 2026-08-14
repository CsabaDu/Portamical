// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.AsyncEnumerables;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;

namespace Tests.Portamical.Converters.AsyncEnumerables;

[TestClass]
public class CollectionConverterAsyncEnumerablesTests
{
    #region Helper method

#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData(def, "result", arg);
#pragma warning restore CA1859

    #endregion

    #region ToDistinctAsyncEnumerable base method

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_baseMethod_yieldsDistinctRows()
    {
        var data1 = CreateData("async1", 1);
        var data2 = CreateData("async2", 2);
        ITestData[] collection = [data1, data2];
        var result = new List<string>();

        await foreach (var item in collection.ToDistinctAsyncEnumerable(td => td.TestCaseName))
        {
            result.Add(item);
        }

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("async1 => result", result[0]);
        Assert.AreEqual("async2 => result", result[1]);
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_baseMethod_deduplicatesByTestCaseName()
    {
        var first = CreateData("async-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("async-dup", "result", 2);
        ITestData[] collection = [first, duplicate];
        var result = new List<ITestData>();

        await foreach (var item in collection.ToDistinctAsyncEnumerable(td => td))
        {
            result.Add(item);
        }

        Assert.AreEqual(1, result.Count);
        Assert.AreSame(first, result[0]);
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_baseMethod_convertsToCustomType()
    {
        var data1 = CreateData("convert1", 10);
        var data2 = CreateData("convert2", 20);
        ITestData[] collection = [data1, data2];
        var result = new List<int>();

        await foreach (var item in collection.ToDistinctAsyncEnumerable(td => td.TestCaseName.Length))
        {
            result.Add(item);
        }

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_baseMethod_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("test")];
        Func<ITestData, string> nullConverter = null!;
        try
        {
            await foreach (var item in collection.ToDistinctAsyncEnumerable(nullConverter))
            {
                Assert.Fail("Should not reach here");
            }
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("convertRow", ex.ParamName);
        }
    }

    #endregion

    #region ToDistinctAsyncEnumerable identity overload

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_identity_yieldsDistinctTestData()
    {
        var data1 = CreateData("id1", 1);
        var data2 = CreateData("id2", 2);
        var data3 = CreateData("id3", 3);
        ITestData[] collection = [data1, data2, data3];
        var result = new List<ITestData>();

        await foreach (var item in collection.ToDistinctAsyncEnumerable())
        {
            result.Add(item);
        }

        Assert.AreEqual(3, result.Count);
        Assert.AreSame(data1, result[0]);
        Assert.AreSame(data2, result[1]);
        Assert.AreSame(data3, result[2]);
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_identity_deduplicatesByTestCaseName()
    {
        var first = CreateData("identity-dup", 5);
        var duplicate = TestDataFactory.CreateTestData<int>("identity-dup", "result", 10);
        ITestData[] collection = [first, duplicate];
        var result = new List<ITestData>();

        await foreach (var item in collection.ToDistinctAsyncEnumerable())
        {
            result.Add(item);
        }

        Assert.AreEqual(1, result.Count);
        Assert.AreSame(first, result[0]);
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_identity_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        try
        {
            await foreach (var item in nullCollection.ToDistinctAsyncEnumerable())
            {
                Assert.Fail("Should not reach here");
            }
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_identity_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();
        try
        {
            await foreach (var item in empty.ToDistinctAsyncEnumerable())
            {
                Assert.Fail("Should not reach here");
            }
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    #endregion

    #region ToDistinctAsyncEnumerable with testMethodName

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_withTestMethodName_passesMethodNameToConverter()
    {
        var data1 = CreateData("method1", 1);
        ITestData[] collection = [data1];
        string? capturedMethodName = null;

        await foreach (var item in collection.ToDistinctAsyncEnumerable(
            (td, methodName) =>
            {
                capturedMethodName = methodName;
                return td.TestCaseName;
            },
            "TestMethodName"))
        {
            // Iterate to trigger conversion
        }

        Assert.AreEqual("TestMethodName", capturedMethodName);
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_withTestMethodName_nullMethodName_passesNull()
    {
        var data1 = CreateData("method2", 2);
        ITestData[] collection = [data1];
        string? capturedMethodName = "not-null";

        await foreach (var item in collection.ToDistinctAsyncEnumerable(
            (td, methodName) =>
            {
                capturedMethodName = methodName;
                return td.TestCaseName;
            },
            null))
        {
            // Iterate to trigger conversion
        }

        Assert.IsNull(capturedMethodName);
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_withTestMethodName_deduplicatesCorrectly()
    {
        var first = CreateData("method-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("method-dup", "result", 2);
        ITestData[] collection = [first, duplicate];
        var result = new List<string>();

        await foreach (var item in collection.ToDistinctAsyncEnumerable(
            (td, methodName) => td.TestCaseName,
            "TestMethod"))
        {
            result.Add(item);
        }

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("method-dup => result", result[0]);
    }

    #endregion

    #region ToDistinctAsyncEnumerable with ArgsCode and testMethodName

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_withArgsCodeAndMethodName_passesAllParameters()
    {
        var data1 = CreateData("args1", 1);
        ITestData[] collection = [data1];
        ArgsCode? capturedArgsCode = null;
        string? capturedMethodName = null;

        await foreach (var item in collection.ToDistinctAsyncEnumerable(
            (td, argsCode, methodName) =>
            {
                capturedArgsCode = argsCode;
                capturedMethodName = methodName;
                return td.ToArgs(argsCode);
            },
            ArgsCode.Properties,
            "ArgsTestMethod"))
        {
            // Iterate to trigger conversion
        }

        Assert.AreEqual(ArgsCode.Properties, capturedArgsCode);
        Assert.AreEqual("ArgsTestMethod", capturedMethodName);
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_withArgsCodeAndMethodName_deduplicatesCorrectly()
    {
        var first = CreateData("args-method-dup", 5);
        var duplicate = TestDataFactory.CreateTestData<int>("args-method-dup", "result", 10);
        ITestData[] collection = [first, duplicate];
        var result = new List<object?[]>();

        await foreach (var item in collection.ToDistinctAsyncEnumerable(
            (td, argsCode, methodName) => td.ToArgs(argsCode),
            ArgsCode.Instance,
            "TestMethod"))
        {
            result.Add(item);
        }

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_withArgsCodeAndMethodName_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid-args")];
        try
        {
            await foreach (var item in collection.ToDistinctAsyncEnumerable(
                (td, argsCode, methodName) => td.ToArgs(argsCode),
                (ArgsCode)999,
                "TestMethod"))
            {
                Assert.Fail("Should not reach here");
            }
            Assert.Fail("Expected InvalidEnumArgumentException was not thrown");
        }
        catch (System.ComponentModel.InvalidEnumArgumentException)
        {
            // Expected
        }
    }

    #endregion

    #region ToDistinctAsyncEnumerable with ArgsCode only

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_withArgsCode_returnsArgumentArrays()
    {
        var data1 = CreateData("argsonly1", 10);
        var data2 = CreateData("argsonly2", 20);
        ITestData[] collection = [data1, data2];
        var result = new List<object?[]>();

        await foreach (var item in collection.ToDistinctAsyncEnumerable(ArgsCode.Instance))
        {
            result.Add(item);
        }

        Assert.AreEqual(2, result.Count);
        Assert.IsNotNull(result[0]);
        Assert.IsNotNull(result[1]);
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_withArgsCode_deduplicatesByTestCaseName()
    {
        var first = CreateData("argsonly-dup", 5);
        var duplicate = TestDataFactory.CreateTestData<int>("argsonly-dup", "result", 10);
        ITestData[] collection = [first, duplicate];
        var result = new List<object?[]>();

        await foreach (var item in collection.ToDistinctAsyncEnumerable(ArgsCode.Properties))
        {
            result.Add(item);
        }

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_withArgsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid-argsonly")];
        try
        {
            await foreach (var item in collection.ToDistinctAsyncEnumerable((ArgsCode)888))
            {
                Assert.Fail("Should not reach here");
            }
            Assert.Fail("Expected InvalidEnumArgumentException was not thrown");
        }
        catch (System.ComponentModel.InvalidEnumArgumentException)
        {
            // Expected
        }
    }

    #endregion

    #region ToDistinctAsyncEnumerable with ArgsCode and PropsCode

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_withArgsCodeAndPropsCode_returnsArgumentArrays()
    {
        var data1 = CreateData("props1", 30);
        var data2 = CreateData("props2", 40);
        ITestData[] collection = [data1, data2];
        var result = new List<object?[]>();

        await foreach (var item in collection.ToDistinctAsyncEnumerable(ArgsCode.Instance, PropsCode.All))
        {
            result.Add(item);
        }

        Assert.AreEqual(2, result.Count);
        Assert.IsNotNull(result[0]);
        Assert.IsNotNull(result[1]);
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_withArgsCodeAndPropsCode_deduplicatesByTestCaseName()
    {
        var first = CreateData("props-async-dup", 15);
        var duplicate = TestDataFactory.CreateTestData<int>("props-async-dup", "result", 25);
        ITestData[] collection = [first, duplicate];
        var result = new List<object?[]>();

        await foreach (var item in collection.ToDistinctAsyncEnumerable(ArgsCode.Properties, PropsCode.All))
        {
            result.Add(item);
        }

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_withArgsCodeAndPropsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid-props-args")];
        try
        {
            await foreach (var item in collection.ToDistinctAsyncEnumerable((ArgsCode)777, PropsCode.All))
            {
                Assert.Fail("Should not reach here");
            }
            Assert.Fail("Expected InvalidEnumArgumentException was not thrown");
        }
        catch (System.ComponentModel.InvalidEnumArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_withArgsCodeAndPropsCode_usesPropsCode()
    {
        var data1 = CreateData("props-async", 5);
        ITestData[] collection = [data1];
        var result = new List<object?[]>();

        await foreach (var item in collection.ToDistinctAsyncEnumerable(ArgsCode.Properties, PropsCode.All))
        {
            result.Add(item);
        }

        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result[0]);
    }

    #endregion

    #region Error handling

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_nullConverter_withMethodName_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("test")];
        Func<ITestData, string?, object?[]> nullConverter = null!;
        try
        {
            await foreach (var item in collection.ToDistinctAsyncEnumerable(nullConverter, null))
            {
                Assert.Fail("Should not reach here");
            }
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("convertRow", ex.ParamName);
        }
    }

    [TestMethod]
    public async Task ToDistinctAsyncEnumerable_nullConverter_withArgsCode_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("test")];
        Func<ITestData, ArgsCode, string?, object?[]> nullConverter = null!;
        try
        {
            await foreach (var item in collection.ToDistinctAsyncEnumerable(nullConverter, ArgsCode.Instance, null))
            {
                Assert.Fail("Should not reach here");
            }
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("convertRow", ex.ParamName);
        }
    }

    #endregion
}
