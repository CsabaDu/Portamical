// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using AsyncEnumerableCollectionConverter = global::Portamical.Converters.AsyncEnumerables.CollectionConverter;
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

    #region ToAsyncRowEnumerable base method

    [TestMethod]
    public async Task ToAsyncRowEnumerable_baseMethod_yieldsAllRowsInOrderWithoutDeduplication()
    {
        var data1 = CreateData("noasync1", 1);
        var data2 = CreateData("noasync2", 2);
        ITestData[] collection = [data1, data2];
        var result = new List<string>();

        await foreach (var item in AsyncEnumerableCollectionConverter.ToAsyncRowEnumerable(collection, td => td.TestCaseName))
        {
            result.Add(item);
        }

        Assert.HasCount(2, result);
        Assert.AreEqual("noasync1 => result", result[0]);
        Assert.AreEqual("noasync2 => result", result[1]);
    }

    [TestMethod]
    public async Task ToAsyncRowEnumerable_baseMethod_doesNotDeduplicate_keepsDuplicates()
    {
        var first = CreateData("noasync-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("noasync-dup", "result", 2);
        ITestData[] collection = [first, duplicate];
        var result = new List<ITestData>();

        await foreach (var item in AsyncEnumerableCollectionConverter.ToAsyncRowEnumerable(collection, td => td))
        {
            result.Add(item);
        }

        Assert.HasCount(2, result);
        Assert.AreSame(first, result[0]);
        Assert.AreSame(duplicate, result[1]);
    }

    [TestMethod]
    public async Task ToAsyncRowEnumerable_baseMethod_convertsToCustomType()
    {
        var data1 = CreateData("convertbase1", 10);
        var data2 = CreateData("convertbase2", 20);
        ITestData[] collection = [data1, data2];
        var result = new List<int>();

        await foreach (var item in AsyncEnumerableCollectionConverter.ToAsyncRowEnumerable(collection, td => td.TestCaseName.Length))
        {
            result.Add(item);
        }

        Assert.HasCount(2, result);
    }

    [TestMethod]
    public async Task ToAsyncRowEnumerable_baseMethod_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;

        try
        {
            await foreach (var item in AsyncEnumerableCollectionConverter.ToAsyncRowEnumerable(nullCollection, td => td.TestCaseName))
            {
                Assert.Fail("Should not reach here");
            }
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
#pragma warning disable MSTEST0058 // Assertions in catch blocks
        catch (ArgumentNullException)
        {
            // Expected
        }
#pragma warning restore MSTEST0058
    }

    [TestMethod]
    public async Task ToAsyncRowEnumerable_baseMethod_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();

        try
        {
            await foreach (var item in AsyncEnumerableCollectionConverter.ToAsyncRowEnumerable(empty, td => td.TestCaseName))
            {
                Assert.Fail("Should not reach here");
            }
            Assert.Fail("Expected ArgumentException was not thrown");
        }
#pragma warning disable MSTEST0058 // Assertions in catch blocks
        catch (ArgumentException)
        {
            // Expected
        }
#pragma warning restore MSTEST0058
    }

    [TestMethod]
    public async Task ToAsyncRowEnumerable_baseMethod_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("nullconvbase")];
        Func<ITestData, string> nullConverter = null!;

        try
        {
            await foreach (var item in AsyncEnumerableCollectionConverter.ToAsyncRowEnumerable(collection, nullConverter))
            {
                Assert.Fail("Should not reach here");
            }
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
#pragma warning disable MSTEST0058 // Assertions in catch blocks
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("convertRow", ex.ParamName);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Unexpected exception type: {ex.GetType().Name}");
        }
#pragma warning restore MSTEST0058
    }

    #endregion

    #region ToDistinctAsyncRowEnumerable base method

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_baseMethod_yieldsDistinctRows()
    {
        var data1 = CreateData("async1", 1);
        var data2 = CreateData("async2", 2);
        ITestData[] collection = [data1, data2];
        var result = new List<string>();

        await foreach (var item in AsyncEnumerableCollectionConverter.ToDistinctAsyncRowEnumerable(collection, td => td.TestCaseName))
        {
            result.Add(item);
        }

        Assert.HasCount(2, result);
        Assert.AreEqual("async1 => result", result[0]);
        Assert.AreEqual("async2 => result", result[1]);
    }

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_baseMethod_deduplicatesByTestCaseName()
    {
        var first = CreateData("async-dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("async-dup", "result", 2);
        ITestData[] collection = [first, duplicate];
        var result = new List<ITestData>();

        await foreach (var item in AsyncEnumerableCollectionConverter.ToDistinctAsyncRowEnumerable(collection, td => td))
        {
            result.Add(item);
        }

        Assert.HasCount(1, result);
        Assert.AreSame(first, result[0]);
    }

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_baseMethod_convertsToCustomType()
    {
        var data1 = CreateData("convert1", 10);
        var data2 = CreateData("convert2", 20);
        ITestData[] collection = [data1, data2];
        var result = new List<int>();

        await foreach (var item in AsyncEnumerableCollectionConverter.ToDistinctAsyncRowEnumerable(collection, td => td.TestCaseName.Length))
        {
            result.Add(item);
        }

        Assert.HasCount(2, result);
    }

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_baseMethod_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("test")];
        Func<ITestData, string> nullConverter = null!;
        try
        {
            await foreach (var item in AsyncEnumerableCollectionConverter.ToDistinctAsyncRowEnumerable(collection, nullConverter))
            {
                Assert.Fail("Should not reach here");
            }
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
#pragma warning disable MSTEST0058 // Assertions in catch blocks
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("convertRow", ex.ParamName);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Unexpected exception type: {ex.GetType().Name}");
        }
#pragma warning restore MSTEST0058
    }

    #endregion

    #region ToDistinctAsyncRowEnumerable object-array conversion

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_withConverter_returnsArgumentArrays()
    {
        var data1 = CreateData("argsonly1", 10);
        var data2 = CreateData("argsonly2", 20);
        ITestData[] collection = [data1, data2];
        var result = new List<object?[]>();

        await foreach (var item in AsyncEnumerableCollectionConverter.ToDistinctAsyncRowEnumerable(collection, testData => testData.ToArgs(ArgsCode.Instance)))
        {
            result.Add(item);
        }

        Assert.HasCount(2, result);
        Assert.IsNotNull(result[0]);
        Assert.IsNotNull(result[1]);
    }

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_withConverter_deduplicatesByTestCaseName()
    {
        var first = CreateData("argsonly-dup", 5);
        var duplicate = TestDataFactory.CreateTestData<int>("argsonly-dup", "result", 10);
        ITestData[] collection = [first, duplicate];
        var result = new List<object?[]>();

        await foreach (var item in AsyncEnumerableCollectionConverter.ToDistinctAsyncRowEnumerable(collection, testData => testData.ToArgs(ArgsCode.Properties)))
        {
            result.Add(item);
        }

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_withConverter_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid-argsonly")];
        try
        {
            await foreach (var item in AsyncEnumerableCollectionConverter.ToDistinctAsyncRowEnumerable(collection, testData => testData.ToArgs((ArgsCode)888)))
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
    public async Task ToDistinctAsyncRowEnumerable_withConverterAndPropsCode_returnsArgumentArrays()
    {
        var data1 = CreateData("props1", 30);
        var data2 = CreateData("props2", 40);
        ITestData[] collection = [data1, data2];
        var result = new List<object?[]>();

        await foreach (var item in AsyncEnumerableCollectionConverter.ToDistinctAsyncRowEnumerable(collection, testData => testData.ToArgs(ArgsCode.Instance, PropsCode.All)))
        {
            result.Add(item);
        }

        Assert.HasCount(2, result);
        Assert.IsNotNull(result[0]);
        Assert.IsNotNull(result[1]);
    }

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_withConverterAndPropsCode_deduplicatesByTestCaseName()
    {
        var first = CreateData("props-async-dup", 15);
        var duplicate = TestDataFactory.CreateTestData<int>("props-async-dup", "result", 25);
        ITestData[] collection = [first, duplicate];
        var result = new List<object?[]>();

        await foreach (var item in AsyncEnumerableCollectionConverter.ToDistinctAsyncRowEnumerable(collection, testData => testData.ToArgs(ArgsCode.Properties, PropsCode.All)))
        {
            result.Add(item);
        }

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_withConverterAndPropsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid-props-args")];
        try
        {
            await foreach (var item in AsyncEnumerableCollectionConverter.ToDistinctAsyncRowEnumerable(collection, testData => testData.ToArgs((ArgsCode)777, PropsCode.All)))
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
    public async Task ToDistinctAsyncRowEnumerable_withConverterAndPropsCode_usesPropsCode()
    {
        var data1 = CreateData("props-async", 5);
        ITestData[] collection = [data1];
        var result = new List<object?[]>();

        await foreach (var item in AsyncEnumerableCollectionConverter.ToDistinctAsyncRowEnumerable(collection, testData => testData.ToArgs(ArgsCode.Properties, PropsCode.All)))
        {
            result.Add(item);
        }

        Assert.HasCount(1, result);
        Assert.IsNotNull(result[0]);
    }

    #endregion

    #region Error handling

    [TestMethod]
    public async Task ToDistinctAsyncRowEnumerable_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("test")];
        Func<ITestData, object?[]> nullConverter = null!;
        try
        {
            await foreach (var item in AsyncEnumerableCollectionConverter.ToDistinctAsyncRowEnumerable(collection, nullConverter))
            {
                Assert.Fail("Should not reach here");
            }
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
#pragma warning disable MSTEST0058 // Assertions in catch blocks
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("convertRow", ex.ParamName);
        }
#pragma warning restore MSTEST0058
    }

    #endregion
}
