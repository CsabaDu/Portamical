// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.ComponentModel;
using Portamical.Converters;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.Core.TestDataTypes.Models.General;
using Portamical.DataProviders;

namespace Tests.Portamical.Converters;

[TestClass]
public class CollectionConverterTests
{
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);

    private sealed class TestProvider : ITestDataProvider<ITestData>
    {
        public ArgsCode ArgsCode { get; init; }
        public string? TestMethodName { get; init; }
        public List<ITestData> Rows { get; } = [];
        public void AddRow(ITestData testData) => Rows.Add(testData);
    }

    private sealed class ObjectArrayConverter : ITestDataConverter<ITestData, object?[]>
    {
        public ArgsCode? LastArgsCode { get; private set; }
        public string? LastMethodName { get; private set; }

        public object?[] ConvertRow(ITestData testData, ArgsCode argsCode, string? testMethodName)
        {
            LastArgsCode = argsCode;
            LastMethodName = testMethodName;
            return testData.ToArgs(argsCode, PropsCode.All);
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

    #region ToDistinctReadOnly(argsCode)

    [TestMethod]
    public void ToDistinctReadOnly_argsCode_returnsReadOnlyCollection()
    {
        ITestData[] collection = [CreateData("a"), CreateData("b")];
        var result = collection.ToDistinctReadOnly(ArgsCode.Instance);
        Assert.IsInstanceOfType<IReadOnlyCollection<object?[]>>(result);
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void ToDistinctReadOnly_argsCode_rowMatchesToArgsOutput()
    {
        var item = CreateData("x");
        ITestData[] collection = [item];
        var result = collection.ToDistinctReadOnly(ArgsCode.Properties);
        var expected = item.ToArgs(ArgsCode.Properties);
        CollectionAssert.AreEqual(expected, result.Single());
    }

    [TestMethod]
    public void ToDistinctReadOnly_argsCode_deduplicatesByTestCaseName()
    {
        var duplicate = TestDataFactory.CreateTestData<int>("dup", "result", 99);
        ITestData[] collection = [CreateData("dup"), duplicate];
        var result = collection.ToDistinctReadOnly(ArgsCode.Instance);
        Assert.HasCount(1, result);
    }

    #endregion

    #region ToDistinctReadOnly(argsCode, propsCode)

    [TestMethod]
    public void ToDistinctReadOnly_withPropsCodeAll_rowMatchesToArgsOutput()
    {
        var item = CreateData("p");
        ITestData[] collection = [item];
        var result = collection.ToDistinctReadOnly(ArgsCode.Properties, PropsCode.All);
        var expected = item.ToArgs(ArgsCode.Properties, PropsCode.All);
        CollectionAssert.AreEqual(expected, result.Single());
    }

    [TestMethod]
    public void ToDistinctReadOnly_withPropsCodeTrimTestCaseName_rowMatchesToArgsOutput()
    {
        var item = CreateData("q");
        ITestData[] collection = [item];
        var result = collection.ToDistinctReadOnly(ArgsCode.Properties, PropsCode.TrimTestCaseName);
        var expected = item.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);
        CollectionAssert.AreEqual(expected, result.Single());
    }

    [TestMethod]
    public void ToDistinctReadOnly_withPropsCode_deduplicatesByTestCaseName()
    {
        var duplicate = TestDataFactory.CreateTestData<int>("props-dup", "result", 42);
        ITestData[] collection = [CreateData("props-dup"), duplicate];
        var result = collection.ToDistinctReadOnly(ArgsCode.Properties, PropsCode.All);
        Assert.HasCount(1, result);
    }

    #endregion

    #region ToDistinctReadOnly(convertRow, argsCode, testMethodName)

    [TestMethod]
    public void ToDistinctReadOnly_converter_passesArgsCodeToConverter()
    {
        var converter = new ObjectArrayConverter();
        ITestData[] collection = [CreateData("r")];
        collection.ToDistinctReadOnly(converter.ConvertRow, ArgsCode.Properties, null);
        Assert.AreEqual(ArgsCode.Properties, converter.LastArgsCode);
    }

    [TestMethod]
    public void ToDistinctReadOnly_converter_passesTestMethodNameToConverter()
    {
        var converter = new ObjectArrayConverter();
        ITestData[] collection = [CreateData("s")];
        collection.ToDistinctReadOnly(converter.ConvertRow, ArgsCode.Instance, "MyTest");
        Assert.AreEqual("MyTest", converter.LastMethodName);
    }

    [TestMethod]
    public void ToDistinctReadOnly_converter_deduplicatesByTestCaseName()
    {
        var converter = new ObjectArrayConverter();
        var duplicate = TestDataFactory.CreateTestData<int>("conv-dup", "result", 55);
        ITestData[] collection = [CreateData("conv-dup"), duplicate];
        var result = collection.ToDistinctReadOnly(converter.ConvertRow, ArgsCode.Properties, null);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ToDistinctReadOnly_converter_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("t")];
        Func<ITestData, ArgsCode, string?, object?[]> convertRow =
            (td, ac, name) => td.ToArgs(ac, PropsCode.All);
        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => collection.ToDistinctReadOnly(convertRow, (ArgsCode)99, null));
    }

    #endregion

    #region ToDataProvider

    [TestMethod]
    public void ToDataProvider_singleElement_initializesProviderWithFirstElement()
    {
        var item = CreateData("init");
        ITestData[] collection = [item];
        var provider = collection.ToDataProvider<TestProvider, ITestData>(
            (first, ac, name) =>
            {
                var p = new TestProvider { ArgsCode = ac, TestMethodName = name };
                p.Rows.Add(first);
                return p;
            },
            ArgsCode.Properties, "TestMethod");

        Assert.IsNotNull(provider);
        Assert.AreEqual(ArgsCode.Properties, provider.ArgsCode);
        Assert.AreEqual("TestMethod", provider.TestMethodName);
        Assert.HasCount(1, provider.Rows);
        Assert.AreSame(item, provider.Rows[0]);
    }

    [TestMethod]
    public void ToDataProvider_multipleElements_addsRemainingRowsViaAddRow()
    {
        var item1 = CreateData("p1");
        var item2 = CreateData("p2");
        var item3 = CreateData("p3");
        ITestData[] collection = [item1, item2, item3];
        var provider = collection.ToDataProvider<TestProvider, ITestData>(
            (first, ac, name) =>
            {
                var p = new TestProvider { ArgsCode = ac, TestMethodName = name };
                p.Rows.Add(first);
                return p;
            },
            ArgsCode.Instance, null);

        Assert.HasCount(3, provider.Rows);
        Assert.AreSame(item1, provider.Rows[0]);
        Assert.AreSame(item2, provider.Rows[1]);
        Assert.AreSame(item3, provider.Rows[2]);
    }

    [TestMethod]
    public void ToDataProvider_deduplicatesBeforeInitializing()
    {
        var duplicate = TestDataFactory.CreateTestData<int>("prov-dup", "result", 77);
        ITestData[] collection = [CreateData("prov-dup"), duplicate];
        var provider = collection.ToDataProvider<TestProvider, ITestData>(
            (first, ac, name) =>
            {
                var p = new TestProvider { ArgsCode = ac, TestMethodName = name };
                p.Rows.Add(first);
                return p;
            },
            ArgsCode.Instance, null);

        Assert.HasCount(1, provider.Rows);
    }

    [TestMethod]
    public void ToDataProvider_nullInitFunction_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("v")];
        Func<ITestData, ArgsCode, string?, TestProvider> nullInit = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => collection.ToDataProvider<TestProvider, ITestData>(
                nullInit, ArgsCode.Instance, null));
    }

    [TestMethod]
    public void ToDataProvider_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDataProvider<TestProvider, ITestData>(
                (first, ac, name) => new TestProvider { ArgsCode = ac },
                ArgsCode.Instance, null));
    }

    [TestMethod]
    public void ToDataProvider_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();
        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToDataProvider<TestProvider, ITestData>(
                (first, ac, name) => new TestProvider { ArgsCode = ac },
                ArgsCode.Instance, null));
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
        var converter = new ObjectArrayConverter();
        var item = CreateData("conv");
        var result = converter.ConvertRow(item, ArgsCode.Properties, "method");
        var expected = item.ToArgs(ArgsCode.Properties, PropsCode.All);
        CollectionAssert.AreEqual(expected, result);
    }

    #endregion
}
