// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.ComponentModel;
using System.Collections;
using Portamical.Converters;
using Portamical.Converters.DataProviders;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.Core.TestDataTypes.Models.General;

namespace Tests.Portamical.Converters;

[TestClass]
public class CollectionConverterTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    private sealed class TestProvider : ITestDataProvider<ITestData>
    {
        public ArgsCode ArgsCode { get; init; }
        public string? TestMethodName { get; init; }
        public List<ITestData> Rows { get; } = [];
        public void AddRow(ITestData testData) => Rows.Add(testData);
        public IEnumerator GetEnumerator() => Rows.GetEnumerator();
    }

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
        Assert.ThrowsExactly<NullReferenceException>(
            () => collection.ToDistinctArray(nullConverter, null));
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
