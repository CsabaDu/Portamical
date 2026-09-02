// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.RowArrays.ObjectArray;
using Portamical.Converters.RowArrays.TestData;
using Portamical.Converters.RowArrays.TypedRow;
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

    private sealed class ObjectArrayConverter
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

    private sealed class TypedRowConverter
    {
        public ArgsCode? LastArgsCode { get; private set; }
        public string? LastMethodName { get; private set; }

        public string ConvertRow(ITestData testData, ArgsCode argsCode, string? testMethodName)
        {
            LastArgsCode = argsCode;
            LastMethodName = testMethodName;
            return $"{testMethodName}|{string.Join(",", testData.ToArgs(argsCode))}";
        }

        public string ConvertRow(ITestData testData, string? testMethodName)
        {
            LastMethodName = testMethodName;
            return $"{testMethodName}|{testData.TestCaseName}";
        }
    }

    #region ToRowArray(argsCode)

    [TestMethod]
    public void ToRowArray_withArgsCode_returnsArgumentArrays()
    {
        var data1 = CreateData("test1", 10);
        var data2 = CreateData("test2", 20);
        ITestData[] collection = [data1, data2];

        var result = collection.ToRowArray(ArgsCode.Instance);

        Assert.HasCount(2, result);
        CollectionAssert.AreEqual(data1.ToArgs(ArgsCode.Instance), result[0]);
        CollectionAssert.AreEqual(data2.ToArgs(ArgsCode.Instance), result[1]);
    }

    [TestMethod]
    public void ToRowArray_withArgsCode_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToRowArray(ArgsCode.Instance));
    }

    [TestMethod]
    public void ToRowArray_withArgsCode_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();

        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToRowArray(ArgsCode.Instance));
    }

    [TestMethod]
    public void ToRowArray_withArgsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];

        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => collection.ToRowArray((ArgsCode)999));
    }

    #endregion

    #region ToRowArray(argsCode, propsCode)

    [TestMethod]
    public void ToRowArray_withArgsCodeAndPropsCode_returnsArgumentArrays()
    {
        var data1 = CreateData("props1", 30);
        var data2 = CreateData("props2", 40);
        ITestData[] collection = [data1, data2];

        var result = collection.ToRowArray(ArgsCode.Properties, PropsCode.All);

        Assert.HasCount(2, result);
        CollectionAssert.AreEqual(data1.ToArgs(ArgsCode.Properties, PropsCode.All), result[0]);
        CollectionAssert.AreEqual(data2.ToArgs(ArgsCode.Properties, PropsCode.All), result[1]);
    }

    [TestMethod]
    public void ToRowArray_withArgsCodeAndPropsCode_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToRowArray(ArgsCode.Instance, PropsCode.All));
    }

    [TestMethod]
    public void ToRowArray_withArgsCodeAndPropsCode_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();

        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToRowArray(ArgsCode.Properties, PropsCode.All));
    }

    [TestMethod]
    public void ToRowArray_withArgsCodeAndPropsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid2")];

        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => collection.ToRowArray((ArgsCode)888, PropsCode.All));
    }

    [TestMethod]
    public void ToRowArray_withArgsCodeAndPropsCode_usesPropsCode()
    {
        var data = CreateData("props-test", 5);
        ITestData[] collection = [data];

        var result = collection.ToRowArray(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        Assert.HasCount(1, result);
        CollectionAssert.AreEqual(
            data.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName),
            result[0]);
    }

    #endregion

    #region ToRowArray(convertRow, argsCode, testMethodName)

    [TestMethod]
    public void ToRowArray_converterWithArgsCode_passesAllParametersCorrectly()
    {
        var converter = new TypedRowConverter();
        var data1 = CreateData("args-test", 5);
        var data2 = CreateData("args-test2", 10);
        ITestData[] collection = [data1, data2];

        var result = collection.ToRowArray(
            converter.ConvertRow,
            ArgsCode.Instance,
            "MyTestMethod");

        Assert.AreEqual(ArgsCode.Instance, converter.LastArgsCode);
        Assert.AreEqual("MyTestMethod", converter.LastMethodName);
        CollectionAssert.AreEqual(
            new[]
            {
                $"MyTestMethod|{string.Join(",", data1.ToArgs(ArgsCode.Instance))}",
                $"MyTestMethod|{string.Join(",", data2.ToArgs(ArgsCode.Instance))}"
            },
            result);
    }

    [TestMethod]
    public void ToRowArray_converterWithArgsCode_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToRowArray(
                (testData, argsCode, testMethodName) => testData.TestCaseName,
                ArgsCode.Instance,
                null));
    }

    [TestMethod]
    public void ToRowArray_converterWithArgsCode_emptyCollection_throwsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => Array.Empty<ITestData>().ToRowArray(
                (testData, argsCode, testMethodName) => testData.TestCaseName,
                ArgsCode.Instance,
                null));
    }

    [TestMethod]
    public void ToRowArray_converterWithArgsCode_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("null-converter")];
        Func<ITestData, ArgsCode, string?, string> nullConverter = null!;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => collection.ToRowArray(nullConverter, ArgsCode.Instance, null));
    }

    [TestMethod]
    public void ToRowArray_converterWithArgsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];

        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => collection.ToRowArray(
                (testData, argsCode, testMethodName) => testData.TestCaseName,
                (ArgsCode)999,
                null));
    }

    #endregion

    #region ToRowArray(convertRow, testMethodName)

    [TestMethod]
    public void ToRowArray_converterWithMethodName_passesTestMethodNameToConverter()
    {
        var converter = new TypedRowConverter();
        var item = CreateData("method-name", 7);
        ITestData[] collection = [item];

        var result = collection.ToRowArray(converter.ConvertRow, "MethodUnderTest");

        Assert.AreEqual("MethodUnderTest", converter.LastMethodName);
        CollectionAssert.AreEqual(
            new[] { $"MethodUnderTest|{item.TestCaseName}" },
            result);
    }

    [TestMethod]
    public void ToRowArray_converterWithMethodName_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("null-converter")];
        Func<ITestData, string?, string> nullConverter = null!;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => collection.ToRowArray(nullConverter, null));
    }

    [TestMethod]
    public void ToRowArray_converterWithMethodName_emptyCollection_throwsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => Array.Empty<ITestData>().ToRowArray(
                (testData, testMethodName) => testData.TestCaseName,
                null));
    }

    #endregion

    #region ToDistinctRowArray

    [TestMethod]
    public void ToDistinctRowArray_singleElement_returnsArrayOfOne()
    {
        ITestData[] collection = [CreateData("a")];
        var result = collection.ToDistinctRowArray();
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToDistinctRowArray_multipleDistinctElements_returnsAll()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b"), CreateData("c")];
        var result = collection.ToDistinctRowArray();
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void ToDistinctRowArray_duplicateTestCaseName_keepsFirstOccurrence()
    {
        var first = CreateData("same");
        var duplicate = TestDataFactory.CreateTestData<int>("same", "result", 99);
        ITestData[] collection = [first, duplicate];
        var result = collection.ToDistinctRowArray();
        Assert.HasCount(1, result);
        Assert.AreSame(first, result[0]);
    }

    [TestMethod]
    public void ToDistinctRowArray_deduplication_isCaseSensitive()
    {
        ITestData[] collection = [CreateData("Same"), CreateData("same")];
        var result = collection.ToDistinctRowArray();
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void ToDistinctRowArray_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDistinctRowArray());
    }

    [TestMethod]
    public void ToDistinctRowArray_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();
        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToDistinctRowArray());
    }

    #endregion

    #region ToDistinctRowArray(convertRow, argsCode, testMethodName)

    [TestMethod]
    public void ToDistinctRowArray_converter_passesArgsCodeToConverter()
    {
        var converter = new ObjectArrayConverter { ArgsCode = ArgsCode.Properties };
        ITestData[] collection = [CreateData("r")];
        collection.ToDistinctRowArray(converter.ConvertRow, null);
        Assert.AreEqual(ArgsCode.Properties, converter.LastArgsCode);
    }

    [TestMethod]
    public void ToDistinctRowArray_converter_passesTestMethodNameToConverter()
    {
        var converter = new ObjectArrayConverter { ArgsCode = ArgsCode.Instance };
        ITestData[] collection = [CreateData("s")];
        collection.ToDistinctRowArray(converter.ConvertRow, "MyTest");
        Assert.AreEqual("MyTest", converter.LastMethodName);
    }

    [TestMethod]
    public void ToDistinctRowArray_converter_deduplicatesByTestCaseName()
    {
        var converter = new ObjectArrayConverter { ArgsCode = ArgsCode.Properties };
        var duplicate = TestDataFactory.CreateTestData<int>("conv-dup", "result", 55);
        ITestData[] collection = [CreateData("conv-dup"), duplicate];
        var result = collection.ToDistinctRowArray(converter.ConvertRow, null);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToDistinctRowArray_converter_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("t")];
#pragma warning disable IDE0039
        Func<ITestData, string?, object?[]> convertRow =
            (td, name) => td.ToArgs((ArgsCode)99, PropsCode.All);
#pragma warning restore IDE0039
        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => collection.ToDistinctRowArray(convertRow, null));
    }

    [TestMethod]
    public void ToDistinctRowArray_converter_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("u")];
        Func<ITestData, string?, object?[]> nullConverter = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => collection.ToDistinctRowArray(nullConverter, null));
    }

    [TestMethod]
    public void ToDistinctRowArray_converterWithArgsCode_nullConverter_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("v")];
        Func<ITestData, ArgsCode, string?, object?[]> nullConverter = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => collection.ToDistinctRowArray(nullConverter, ArgsCode.Instance, null));
    }

    [TestMethod]
    public void ToDistinctRowArray_converterWithArgsCode_passesAllParametersCorrectly()
    {
        var data1 = CreateData("args-test", 5);
        var data2 = CreateData("args-test2", 10);
        ITestData[] collection = [data1, data2];
        ArgsCode? capturedArgsCode = null;
        string? capturedMethodName = null;
        int callCount = 0;

        var result = collection.ToDistinctRowArray(
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
    public void ToDistinctRowArray_converter_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDistinctRowArray(
                (testData, testMethodName) => testData.ToArgs(ArgsCode.Instance, PropsCode.All),
                null));
    }

    [TestMethod]
    public void ToDistinctRowArray_converter_emptyCollection_throwsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => Array.Empty<ITestData>().ToDistinctRowArray(
                (testData, testMethodName) => testData.ToArgs(ArgsCode.Instance, PropsCode.All),
                null));
    }

    #endregion

    #region ToDistinctRowArray(argsCode)

    [TestMethod]
    public void ToDistinctRowArray_withArgsCode_returnsArgumentArrays()
    {
        var data1 = CreateData("test1", 10);
        var data2 = CreateData("test2", 20);
        ITestData[] collection = [data1, data2];
        var result = collection.ToDistinctRowArray(ArgsCode.Instance);
        Assert.HasCount(2, result);
        Assert.IsNotNull(result[0]);
        Assert.IsNotNull(result[1]);
    }

    [TestMethod]
    public void ToDistinctRowArray_withArgsCode_deduplicatesByTestCaseName()
    {
        var first = CreateData("args-dup", 10);
        var duplicate = TestDataFactory.CreateTestData<int>("args-dup", "result", 20);
        ITestData[] collection = [first, duplicate];
        var result = collection.ToDistinctRowArray(ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToDistinctRowArray_withArgsCode_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDistinctRowArray(ArgsCode.Instance));
    }

    [TestMethod]
    public void ToDistinctRowArray_withArgsCode_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();
        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToDistinctRowArray(ArgsCode.Instance));
    }

    [TestMethod]
    public void ToDistinctRowArray_withArgsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];
        Assert.ThrowsExactly<System.ComponentModel.InvalidEnumArgumentException>(
            () => collection.ToDistinctRowArray((ArgsCode)999));
    }

    #endregion

    #region ToDistinctRowArray(argsCode, propsCode)

    [TestMethod]
    public void ToDistinctRowArray_withArgsCodeAndPropsCode_returnsArgumentArrays()
    {
        var data1 = CreateData("test3", 30);
        var data2 = CreateData("test4", 40);
        ITestData[] collection = [data1, data2];
        var result = collection.ToDistinctRowArray(ArgsCode.Instance, PropsCode.All);
        Assert.HasCount(2, result);
        Assert.IsNotNull(result[0]);
        Assert.IsNotNull(result[1]);
    }

    [TestMethod]
    public void ToDistinctRowArray_withArgsCodeAndPropsCode_deduplicatesByTestCaseName()
    {
        var first = CreateData("props-dup", 15);
        var duplicate = TestDataFactory.CreateTestData<int>("props-dup", "result", 25);
        ITestData[] collection = [first, duplicate];
        var result = collection.ToDistinctRowArray(ArgsCode.Properties, PropsCode.All);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToDistinctRowArray_withArgsCodeAndPropsCode_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDistinctRowArray(ArgsCode.Instance, PropsCode.All));
    }

    [TestMethod]
    public void ToDistinctRowArray_withArgsCodeAndPropsCode_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();
        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToDistinctRowArray(ArgsCode.Properties, PropsCode.All));
    }

    [TestMethod]
    public void ToDistinctRowArray_withArgsCodeAndPropsCode_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid2")];
        Assert.ThrowsExactly<System.ComponentModel.InvalidEnumArgumentException>(
            () => collection.ToDistinctRowArray((ArgsCode)888, PropsCode.All));
    }

    [TestMethod]
    public void ToDistinctRowArray_withArgsCodeAndPropsCode_usesPropsCode()
    {
        var data1 = CreateData("props-test", 5);
        ITestData[] collection = [data1];
        var result = collection.ToDistinctRowArray(ArgsCode.Properties, PropsCode.All);
        Assert.HasCount(1, result);
        Assert.IsNotNull(result[0]);
    }

    #endregion

    #region converter delegate variance

    [TestMethod]
    public void ConvertRow_delegate_contravariance_allowsBaseConverterAsSpecificType()
    {
        Func<ITestData, string?, object?[]> general = new ObjectArrayConverter().ConvertRow;
        Func<TestData<int>, string?, object?[]> specific = general;
        Assert.IsNotNull(specific);
    }

    [TestMethod]
    public void ConvertRow_delegate_covariance_allowsTypedConverterAsBaseReturnType()
    {
        Func<ITestData, string?, object?[]> typed = new ObjectArrayConverter().ConvertRow;
        Func<ITestData, string?, object> covariant = typed;
        Assert.IsNotNull(covariant);
    }

    [TestMethod]
    public void ConvertRow_returnsExpectedResult()
    {
        var converter = new ObjectArrayConverter { ArgsCode = ArgsCode.Properties };
        var item = CreateData("conv");
        var result = converter.ConvertRow(item, "method");
        var expected = item.ToArgs(ArgsCode.Properties, PropsCode.All);
        CollectionAssert.AreEqual(expected, result);
    }

    #endregion
}
