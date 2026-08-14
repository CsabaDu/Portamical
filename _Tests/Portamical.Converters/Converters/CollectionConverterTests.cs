// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.Converters.DataProviders;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.Core.TestDataTypes.Models.General;
using System.ComponentModel;

namespace Tests.Portamical.Converters.Converters;

[TestClass]
public class CollectionConverterTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    private sealed class ObjectArrayConverter : ITestDataConverter<ITestData, object?[]>
    {
        public ArgsCode ArgsCode { get; init; }
        public ArgsCode? LastArgsCode { get; private set; }
        public string? LastMethodName { get; private set; }

        public object?[] ConvertRow(ITestData testData, string? testMethodName)
        {
            LastArgsCode = ArgsCode;
            LastMethodName = testMethodName;
            return testData.ToArgs(ArgsCode, PropsCode.All);
        }
    }

    #region ToDistinctArray

    [TestMethod]
    public void ToDistinctArray_singleElement_returnsArrayOfOne()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToDistinctArray();
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToDistinctArray_multipleDistinctElements_returnsAll()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = collection.ToDistinctArray();
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void ToDistinctArray_duplicateTestCaseName_keepsFirstOccurrence()
    {
        var first = CreateData("same");
        var duplicate = TestDataFactory.CreateTestData<int>("same", "result", 99);
        ITestData[] collection = [first, duplicate];
        var result = collection.ToDistinctArray();
        Assert.HasCount(1, result);
        Assert.AreSame(first, result[0]);
    }

    [TestMethod]
    public void ToDistinctArray_deduplication_isCaseSensitive()
    {
        ITestData[] collection = [CreateData("Same"), CreateData("same")];
        var result = collection.ToDistinctArray();
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void ToDistinctArray_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDistinctArray());
    }

    [TestMethod]
    public void ToDistinctArray_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();
        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToDistinctArray());
    }

    #endregion

    #region ToDistinctArray(convertRow, argsCode, testMethodName)

    [TestMethod]
    public void ToDistinctArray_converter_passesArgsCodeToConverter()
    {
        var converter = new ObjectArrayConverter { ArgsCode = ArgsCode.Properties };
        ITestData[] collection = [CreateData("r")];
        collection.ToDistinctArray(converter.ConvertRow, null);
        Assert.AreEqual(ArgsCode.Properties, converter.LastArgsCode);
    }

    [TestMethod]
    public void ToDistinctArray_converter_passesTestMethodNameToConverter()
    {
        var converter = new ObjectArrayConverter { ArgsCode = ArgsCode.Instance };
        ITestData[] collection = [CreateData("s")];
        collection.ToDistinctArray(converter.ConvertRow, "MyTest");
        Assert.AreEqual("MyTest", converter.LastMethodName);
    }

    [TestMethod]
    public void ToDistinctArray_converter_deduplicatesByTestCaseName()
    {
        var converter = new ObjectArrayConverter { ArgsCode = ArgsCode.Properties };
        var duplicate = TestDataFactory.CreateTestData<int>("conv-dup", "result", 55);
        ITestData[] collection = [CreateData("conv-dup"), duplicate];
        var result = collection.ToDistinctArray(converter.ConvertRow, null);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToDistinctArray_converter_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("t")];
#pragma warning disable IDE0039
        Func<ITestData, string?, object?[]> convertRow =
            (td, name) => td.ToArgs((ArgsCode)99, PropsCode.All);
#pragma warning restore IDE0039
        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => collection.ToDistinctArray(convertRow, null));
    }

    [TestMethod]
    public void ToDistinctArray_converter_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("u")];
        Func<ITestData, string?, object?[]> nullConverter = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => collection.ToDistinctArray(nullConverter, null));
    }

    [TestMethod]
    public void ToDistinctArray_converterWithArgsCode_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("v")];
        Func<ITestData, ArgsCode, string?, object?[]> nullConverter = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => collection.ToDistinctArray(nullConverter, ArgsCode.Instance, null));
    }

    [TestMethod]
    public void ToDistinctArray_converterWithArgsCode_passesAllParametersCorrectly()
    {
        var data1 = CreateData("args-test", 5);
        var data2 = CreateData("args-test2", 10);
        ITestData[] collection = [data1, data2];
        ArgsCode? capturedArgsCode = null;
        string? capturedMethodName = null;
        int callCount = 0;

        var result = collection.ToDistinctArray(
            (td, argsCode, methodName) =>
            {
                capturedArgsCode = argsCode;
                capturedMethodName = methodName;
                callCount++;
                return td.ToArgs(argsCode);
            },
            ArgsCode.Properties,
            "TestMethodName");

        Assert.HasCount(2, result);
        Assert.AreEqual(ArgsCode.Properties, capturedArgsCode);
        Assert.AreEqual("TestMethodName", capturedMethodName);
        Assert.AreEqual(2, callCount);
    }

    [TestMethod]
    public void ToDistinctArray_converter_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDistinctArray(
                (testData, testMethodName) => testData.ToArgs(ArgsCode.Instance, PropsCode.All),
                null));
    }

    [TestMethod]
    public void ToDistinctArray_converter_emptyCollection_throwsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => Array.Empty<ITestData>().ToDistinctArray(
                (testData, testMethodName) => testData.ToArgs(ArgsCode.Instance, PropsCode.All),
                null));
    }

    #endregion

    #region ToDistinctArray(argsCode)

    [TestMethod]
    public void ToDistinctArray_withArgsCode_returnsArgumentArrays()
    {
        var data1 = CreateData("test1", 10);
        var data2 = CreateData("test2", 20);
        ITestData[] collection = [data1, data2];
        var result = collection.ToDistinctArray(ArgsCode.Instance);
        Assert.HasCount(2, result);
        Assert.IsNotNull(result[0]);
        Assert.IsNotNull(result[1]);
    }

    [TestMethod]
    public void ToDistinctArray_withArgsCode_deduplicatesByTestCaseName()
    {
        var first = CreateData("args-dup", 10);
        var duplicate = TestDataFactory.CreateTestData<int>("args-dup", "result", 20);
        ITestData[] collection = [first, duplicate];
        var result = collection.ToDistinctArray(ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToDistinctArray_withArgsCode_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDistinctArray(ArgsCode.Instance));
    }

    [TestMethod]
    public void ToDistinctArray_withArgsCode_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();
        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToDistinctArray(ArgsCode.Instance));
    }

    [TestMethod]
    public void ToDistinctArray_withArgsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];
        Assert.ThrowsExactly<System.ComponentModel.InvalidEnumArgumentException>(
            () => collection.ToDistinctArray((ArgsCode)999));
    }

    #endregion

    #region ToDistinctArray(argsCode, propsCode)

    [TestMethod]
    public void ToDistinctArray_withArgsCodeAndPropsCode_returnsArgumentArrays()
    {
        var data1 = CreateData("test3", 30);
        var data2 = CreateData("test4", 40);
        ITestData[] collection = [data1, data2];
        var result = collection.ToDistinctArray(ArgsCode.Instance, PropsCode.All);
        Assert.HasCount(2, result);
        Assert.IsNotNull(result[0]);
        Assert.IsNotNull(result[1]);
    }

    [TestMethod]
    public void ToDistinctArray_withArgsCodeAndPropsCode_deduplicatesByTestCaseName()
    {
        var first = CreateData("props-dup", 15);
        var duplicate = TestDataFactory.CreateTestData<int>("props-dup", "result", 25);
        ITestData[] collection = [first, duplicate];
        var result = collection.ToDistinctArray(ArgsCode.Properties, PropsCode.All);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToDistinctArray_withArgsCodeAndPropsCode_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDistinctArray(ArgsCode.Instance, PropsCode.All));
    }

    [TestMethod]
    public void ToDistinctArray_withArgsCodeAndPropsCode_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();
        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToDistinctArray(ArgsCode.Properties, PropsCode.All));
    }

    [TestMethod]
    public void ToDistinctArray_withArgsCodeAndPropsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid2")];
        Assert.ThrowsExactly<System.ComponentModel.InvalidEnumArgumentException>(
            () => collection.ToDistinctArray((ArgsCode)888, PropsCode.All));
    }

    [TestMethod]
    public void ToDistinctArray_withArgsCodeAndPropsCode_usesPropsCode()
    {
        var data1 = CreateData("props-test", 5);
        ITestData[] collection = [data1];
        var result = collection.ToDistinctArray(ArgsCode.Properties, PropsCode.All);
        Assert.HasCount(1, result);
        Assert.IsNotNull(result[0]);
    }

    #endregion

    #region ITestDataConverter variance

    [TestMethod]
    public void ITestDataConverter_contravariance_allowsBaseConverterAsSpecificType()
    {
        ITestDataConverter<ITestData, object?[]> general = new ObjectArrayConverter();
        ITestDataConverter<TestData<int>, object?[]> specific = general;
        Assert.IsNotNull(specific);
    }

    [TestMethod]
    public void ITestDataConverter_covariance_allowsTypedConverterAsBaseReturnType()
    {
        ITestDataConverter<ITestData, object?[]> typed = new ObjectArrayConverter();
        ITestDataConverter<ITestData, object> covariant = typed;
        Assert.IsNotNull(covariant);
    }

    [TestMethod]
    public void ITestDataConverter_convertRow_returnsExpectedResult()
    {
        var converter = new ObjectArrayConverter { ArgsCode = ArgsCode.Properties };
        var item = CreateData("conv");
        var result = converter.ConvertRow(item, "method");
        var expected = item.ToArgs(ArgsCode.Properties, PropsCode.All);
        CollectionAssert.AreEqual(expected, result);
    }

    #endregion
}
