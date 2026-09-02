// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.DataProviders.TypedRow;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using BaseProvider = global::Portamical.DataProviders.Models.TypedRow.TestDataProvider<global::Portamical.Core.TestDataTypes.ITestData, string>;

namespace Tests.Portamical.Converters.DataProviders.TypedRow;

[TestClass]
public class CollectionConverter_ToDataProviderTests
{
    private sealed class ConcreteProvider : BaseProvider
    {
        public ConcreteProvider(ArgsCode argsCode, string? testMethodName)
            : base(argsCode, testMethodName)
        {
        }

        public ConcreteProvider(ITestData testData, ArgsCode argsCode, string? testMethodName)
            : base(testData, argsCode, testMethodName)
        {
        }

        public override string ConvertRow(ITestData testData)
            => testData.TestCaseName;
    }

#pragma warning disable CA1859
    private static ITestData CreateData(string definition, int arg = 1)
        => TestDataFactory.CreateTestData<int>(definition, "result", arg);
#pragma warning restore CA1859

    [TestMethod]
    public void ToDataProvider_withInitializer_passesArgsCodeAndTestMethodNameToProvider()
    {
        var item = CreateData("init", 1);
        ITestData[] collection = [item];

        var provider = collection.ToDataProvider<ConcreteProvider, ITestData, string>(
            (first, argsCode, testMethodName) => new ConcreteProvider(first, argsCode, testMethodName),
            ArgsCode.Instance,
            "MyTest");

        Assert.AreEqual(ArgsCode.Instance, provider.ArgsCode);
        Assert.AreEqual("MyTest", provider.TestMethodName);
        CollectionAssert.AreEqual(new[] { item.TestCaseName }, provider.GetRows());
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_multipleElements_addsRemainingViaAddRow()
    {
        var item1 = CreateData("p1", 1);
        var item2 = CreateData("p2", 2);
        var item3 = CreateData("p3", 3);
        ITestData[] collection = [item1, item2, item3];

        var provider = collection.ToDataProvider<ConcreteProvider, ITestData, string>(
            (first, argsCode, testMethodName) => new ConcreteProvider(first, argsCode, testMethodName),
            ArgsCode.Instance,
            "DataProviderMethod");

        Assert.AreEqual(ArgsCode.Instance, provider.ArgsCode);
        Assert.AreEqual("DataProviderMethod", provider.TestMethodName);
        CollectionAssert.AreEqual(
            new[] { item1.TestCaseName, item2.TestCaseName, item3.TestCaseName },
            provider.GetRows());
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_nullInitFunction_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("v")];
        Func<ITestData, ArgsCode, string?, ConcreteProvider> nullInit = null!;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => collection.ToDataProvider<ConcreteProvider, ITestData, string>(
                nullInit,
                ArgsCode.Instance,
                null));
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDataProvider<ConcreteProvider, ITestData, string>(
                (first, argsCode, testMethodName) => new ConcreteProvider(first, argsCode, testMethodName),
                ArgsCode.Instance,
                null));
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();

        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToDataProvider<ConcreteProvider, ITestData, string>(
                (first, argsCode, testMethodName) => new ConcreteProvider(first, argsCode, testMethodName),
                ArgsCode.Instance,
                null));
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];

        Assert.ThrowsExactly<System.ComponentModel.InvalidEnumArgumentException>(
            () => collection.ToDataProvider<ConcreteProvider, ITestData, string>(
                (first, argsCode, testMethodName) => new ConcreteProvider(first, argsCode, testMethodName),
                (ArgsCode)999,
                null));
    }

    [TestMethod]
    public void ToDistinctDataProvider_withInitializer_deduplicatesBeforeInitializing()
    {
        var first = CreateData("dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("dup", "result", 99);
        ITestData[] collection = [first, duplicate];

        var provider = collection.ToDistinctDataProvider<ConcreteProvider, ITestData, string>(
            (item, argsCode, testMethodName) => new ConcreteProvider(item, argsCode, testMethodName),
            ArgsCode.Instance,
            "DistinctMethod");

        Assert.AreEqual(ArgsCode.Instance, provider.ArgsCode);
        Assert.AreEqual("DistinctMethod", provider.TestMethodName);
        CollectionAssert.AreEqual(new[] { first.TestCaseName }, provider.GetRows());
    }

    [TestMethod]
    public void ToDistinctDataProvider_withInitializer_multipleItemsWithDuplicates_filtersCorrectly()
    {
        var item1 = CreateData("unique1", 1);
        var item2 = CreateData("dup-name", 2);
        var item3 = CreateData("unique2", 3);
        var duplicate = TestDataFactory.CreateTestData<int>("dup-name", "result", 99);
        var item4 = CreateData("unique3", 4);
        ITestData[] collection = [item1, item2, item3, duplicate, item4];

        var provider = collection.ToDistinctDataProvider<ConcreteProvider, ITestData, string>(
            (item, argsCode, testMethodName) => new ConcreteProvider(item, argsCode, testMethodName),
            ArgsCode.Instance,
            null);

        CollectionAssert.AreEqual(
            new[] { item1.TestCaseName, item2.TestCaseName, item3.TestCaseName, item4.TestCaseName },
            provider.GetRows());
    }

    [TestMethod]
    public void ToDistinctDataProvider_withInitializer_nullInitFunction_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("v")];
        Func<ITestData, ArgsCode, string?, ConcreteProvider> nullInit = null!;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => collection.ToDistinctDataProvider<ConcreteProvider, ITestData, string>(
                nullInit,
                ArgsCode.Instance,
                null));
    }

    [TestMethod]
    public void ToDistinctDataProvider_withInitializer_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];

        Assert.ThrowsExactly<System.ComponentModel.InvalidEnumArgumentException>(
            () => collection.ToDistinctDataProvider<ConcreteProvider, ITestData, string>(
                (first, argsCode, testMethodName) => new ConcreteProvider(first, argsCode, testMethodName),
                (ArgsCode)999,
                null));
    }
}
